using System;
using System.Collections.Generic;
using System.Reflection;

using UnityEngine;

namespace DPSMeter;

public sealed class DPSMeter : ModBehaviour
{
    public static DPSMeter Instance { get; private set; }

    private ClientEventManager _clientEvents;
    private ZoneManager _zoneManager;
    private DpsData _data;
    private DpsOverlay _overlay;
    private bool _subscribed;
    private Hero _currentHero;
    private readonly Dictionary<string, DpsData.DamageScalingType> _skillScalingCache = new Dictionary<string, DpsData.DamageScalingType>();
    private readonly Dictionary<Gem, DpsData.DamageScalingType> _essenceScalingCache = new Dictionary<Gem, DpsData.DamageScalingType>();
    private static int _barrierTraceCount;
    private System.Func<EventInfoTravelToNodeInterrupt, bool> _travelInterruptHandler;
    private void Awake()
    {
        Instance = this;
        _data = new DpsData();
        _overlay = gameObject.AddComponent<DpsOverlay>();
        _overlay.Initialize(_data);
        _travelInterruptHandler = OnTravelToNodeInterrupt;

        CallOnNetworkedManager<ClientEventManager>(AttachToClientEvents, DetachFromClientEvents);
        CallOnNetworkedManager<ZoneManager>(AttachToZoneManager, DetachFromZoneManager);
    }

    private void AttachToClientEvents()
    {
        ClientEventManager currentManager = ClientEventManager.instance;

        if (currentManager == null)
        {
            return;
        }

        if (_subscribed && _clientEvents == currentManager)
        {
            return;
        }

        if (_subscribed)
        {
            DetachFromClientEvents();
        }

        _clientEvents = currentManager;

        _clientEvents.OnTakeDamage += OnTakeDamage;
        _clientEvents.OnTakeHeal += OnTakeHeal;
        _clientEvents.OnTakeShield += OnTakeShield;
        _clientEvents.OnLocalHeroAbilityChanged += OnLocalHeroAbilityChanged;
        _subscribed = true;
        Debug.Log("[DPS Meter] Damage event listener attached.");
    }

    private void DetachFromClientEvents()
    {
        if (_clientEvents != null && _subscribed)
        {
            _clientEvents.OnTakeDamage -= OnTakeDamage;
            _clientEvents.OnTakeHeal -= OnTakeHeal;
            _clientEvents.OnTakeShield -= OnTakeShield;
            _clientEvents.OnLocalHeroAbilityChanged -= OnLocalHeroAbilityChanged;
        }

        _clientEvents = null;
        _subscribed = false;
    }

    private void OnLocalHeroAbilityChanged(Hero hero, HeroSkillLocation location)
    {
        if (hero == null || hero == _currentHero)
        {
            return;
        }

        if (_currentHero != null)
        {
            _data.ResetCurrentInstance();
        }

        _currentHero = hero;
        _skillScalingCache.Clear();
        _essenceScalingCache.Clear();
        Debug.Log("[DPS Meter] Reset current damage window for hero ability change.");
    }

    private void OnZoneLoadStarted(EventInfoLoadZone info)
    {
        _data.ResetCurrentInstance();
        _currentHero = null;

        // A new run can recreate the networked event manager. Re-check the
        // active manager so damage events continue reaching the meter.
        AttachToClientEvents();
        AttachToZoneManager();
    }

    private bool OnTravelToNodeInterrupt(EventInfoTravelToNodeInterrupt info)
    {
        if (_data != null)
        {
            _data.ResetCurrentInstance();
            Debug.Log("[DPS Meter] Reset current damage window for node travel " + info.from + " -> " + info.to + ".");
        }

        return false;
    }

    private void OnTakeHeal(EventInfoHeal info)
    {
        DewPlayer local = DewPlayer.local;

        if (local == null || local.hero == null || info.actor == null || info.target == null)
        {
            return;
        }

        if (info.target != local.hero)
        {
            return;
        }

        float healing = Mathf.Max(0f, info.amount) + Mathf.Max(0f, info.discardedAmount);

        if (healing <= 0f)
        {
            return;
        }

        string sourceName = GetHealingSourceName(info.actor);
        string sourceIdentity = GetSkillSlotIdentity(info.actor);
        Sprite healingIcon = FindHealingIcon(info.actor);
        _data.AddHealing(healing, sourceIdentity, sourceName, healingIcon);

    }

    private void OnTakeShield(EventInfoShield info)
    {
        DewPlayer local = DewPlayer.local;

        if (local == null || local.hero == null || info.target == null)
        {
            return;
        }

        if (info.target != local.hero)
        {
            return;
        }

        float barrier = Mathf.Max(0f, info.finalAmount);
        TraceBarrierStatusEffect(info.statusEffect);
        if (barrier <= 0f)
        {
            return;
        }

        string sourceIdentity;
        string sourceName;
        Sprite icon;
        ResolveBarrierSource(info.statusEffect, out sourceIdentity, out sourceName, out icon);

        _data.AddBarrier(barrier, sourceIdentity, sourceName, icon);
    }

    private static void ResolveBarrierSource(
        object statusEffect,
        out string sourceIdentity,
        out string sourceName,
        out Sprite icon)
    {
        sourceIdentity = null;
        sourceName = null;
        icon = null;

        Actor statusActor = statusEffect as Actor;
        if (statusActor == null)
        {
            sourceName = statusEffect == null ? null : statusEffect.GetType().Name;
            icon = FindSpriteMember(statusEffect);
            sourceIdentity = sourceName;
            return;
        }

        // Barrier events expose the actual Essence directly on the shield
        // status effect (for example Se_GenericShield_Stacking.gem). Prefer
        // that immediate Gem instead of attributing the barrier to the skill
        // that happened to trigger the Essence.
        Gem directGem = FindDirectGemMember(statusActor);
        if (directGem != null)
        {
            sourceIdentity = directGem.GetOriginalName();
            if (string.IsNullOrEmpty(sourceIdentity))
            {
                sourceIdentity = string.IsNullOrEmpty(directGem.name)
                    ? directGem.GetType().Name
                    : directGem.name;
            }

            sourceName = sourceIdentity;
            icon = FindSpriteMember(directGem);
            return;
        }

        // Innate Memory barriers expose the originating SkillTrigger through
        // the status effect's parentActor chain. Stop at the first skill
        // rather than recursively searching unrelated object fields.
        Actor current = statusActor.parentActor;
        int depth = 0;
        while (current != null && depth < 8)
        {
            directGem = FindDirectGemMember(current);
            if (directGem != null)
            {
                sourceIdentity = directGem.GetOriginalName();
                if (string.IsNullOrEmpty(sourceIdentity))
                {
                    sourceIdentity = string.IsNullOrEmpty(directGem.name)
                        ? directGem.GetType().Name
                        : directGem.name;
                }

                sourceName = sourceIdentity;
                icon = FindSpriteMember(directGem);
                return;
            }

            SkillTrigger skill = current.firstTrigger as SkillTrigger;
            if (skill != null)
            {
                sourceIdentity = GetSkillSlotIdentity(current, skill);
                sourceName = skill.GetFormattedSkillTitle();
                icon = FindSkillIcon(skill);

                if (string.IsNullOrEmpty(sourceIdentity))
                {
                    sourceIdentity = sourceName;
                }

                return;
            }

            current = current.parentActor;
            depth++;
        }

        sourceName = statusActor.name;
        icon = FindSpriteMember(statusActor);
        sourceIdentity = sourceName;
    }

    private static Gem FindDirectGemMember(object value)
    {
        if (value == null)
        {
            return null;
        }

        Type type = value.GetType();
        PropertyInfo gemProperty = type.GetProperty(
            "gem",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        if (gemProperty != null && gemProperty.GetIndexParameters().Length == 0 && gemProperty.GetMethod != null)
        {
            try
            {
                Gem gem = gemProperty.GetValue(value, null) as Gem;
                if (gem != null)
                {
                    return gem;
                }
            }
            catch (Exception)
            {
            }
        }

        FieldInfo gemField = type.GetField(
            "_gem",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        if (gemField != null)
        {
            try
            {
                Gem gem = gemField.GetValue(value) as Gem;
                if (gem != null)
                {
                    return gem;
                }
            }
            catch (Exception)
            {
            }
        }

        return null;
    }

    private DewPlayer FindPlayer(Hero hero)
    {
        if (hero == null || DewPlayer.gamePlayers == null)
        {
            return null;
        }

        for (int i = 0; i < DewPlayer.gamePlayers.Count; i++)
        {
            DewPlayer player = DewPlayer.gamePlayers[i];

            if (player != null && player.hero == hero)
            {
                return player;
            }
        }

        return null;
    }

    [ModBehaviour.ConsoleCommand("Toggle the DPS meter overlay.", "dps_meter")]
    private void ToggleMeter()
    {
        if (_overlay != null)
        {
            _overlay.Visible = !_overlay.Visible;
        }
    }

    [ModBehaviour.ConsoleCommand("Reset the DPS meter.", "dps_reset")]
    private void ResetMeter()
    {
        if (_data != null)
        {
            _data.Reset();
        }
    }

    private void OnDestroy()
    {
        DetachFromClientEvents();
        DetachFromZoneManager();

        if (_overlay != null)
        {
            Destroy(_overlay);
            _overlay = null;
        }

        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void AttachToZoneManager()
    {
        ZoneManager currentManager = ZoneManager.instance;

        if (currentManager == null)
        {
            return;
        }

        if (_zoneManager == currentManager)
        {
            return;
        }

        if (_zoneManager != null)
        {
            DetachFromZoneManager();
        }

        _zoneManager = currentManager;
        _zoneManager.ClientEvent_OnZoneLoadStarted += OnZoneLoadStarted;
        _zoneManager.AddTravelToNodeInterrupt(_travelInterruptHandler);
        Debug.Log("[DPS Meter] Zone/node reset listeners attached.");
    }

    private void DetachFromZoneManager()
    {
        if (_zoneManager != null)
        {
            _zoneManager.ClientEvent_OnZoneLoadStarted -= OnZoneLoadStarted;
            _zoneManager.RemoveTravelToNodeInterrupt(_travelInterruptHandler);
        }

        _zoneManager = null;
    }
}