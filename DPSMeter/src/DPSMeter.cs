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
    private int _lastTravelTargetNode = int.MinValue;

    private void Awake()
    {
        Instance = this;
        _data = new DpsData();
        _overlay = gameObject.AddComponent<DpsOverlay>();
        _overlay.Initialize(_data);

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
        _clientEvents.OnLocalHeroAbilityChanged += OnLocalHeroAbilityChanged;
        _subscribed = true;
        Debug.Log("[DPS Meter] Damage event listener attached.");
    }

    private void DetachFromClientEvents()
    {
        if (_clientEvents != null && _subscribed)
        {
            _clientEvents.OnTakeDamage -= OnTakeDamage;
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

    private void Update()
    {
        Rift_RoomExit exit = Rift_RoomExit.softInstance;
        if (exit == null)
        {
            return;
        }

        int targetNode = exit.nextNodeIndex;
        if (targetNode == _lastTravelTargetNode)
        {
            return;
        }

        _lastTravelTargetNode = targetNode;
        if (_data.CurrentHitCount > 0)
        {
            _data.ResetCurrentInstance();
            Debug.Log("[DPS Meter] Reset current damage window for node transition target " + targetNode + ".");
        }
    }

    private void OnTakeDamage(EventInfoDamage info)
    {
        if (info.actor == null || info.victim == null)
        {
            return;
        }

        float appliedDamage = Mathf.Max(0f, info.damage.amount);
        float producedDamage = appliedDamage + Mathf.Max(0f, info.damage.discardedAmount);

        if (producedDamage <= 0f)
        {
            return;
        }

        DewPlayer local = DewPlayer.local;

        if (local == null || local.hero == null)
        {
            return;
        }

        Hero sourceHero = info.actor.firstEntity as Hero;

        if (sourceHero == null)
        {
            return;
        }

        DewPlayer sourcePlayer = FindPlayer(sourceHero);

        if (sourcePlayer == null || !sourcePlayer.isHumanPlayer)
        {
            return;
        }

        bool isLocalPlayer = sourcePlayer == local;

        SkillTrigger skill = info.actor.firstTrigger as SkillTrigger;
        AbilityInstance ability = info.actor.FindFirstOfType<AbilityInstance>();

        List<Gem> essences = new List<Gem>();
        Gem directGem = ability != null ? ability.gem : null;

        if (directGem != null)
        {
            essences.Add(directGem);
        }

        if (sourceHero.Skill != null && sourceHero.Skill.gems != null)
        {
            foreach (Gem candidate in sourceHero.Skill.gems.Values)
            {
                if (candidate != null && !essences.Contains(candidate) && IsDamageModifiedBy(info.damage, candidate))
                {
                    essences.Add(candidate);
                }
            }
        }

        Gem[] heroGems = sourceHero.GetComponentsInChildren<Gem>(true);
        for (int i = 0; i < heroGems.Length; i++)
        {
            Gem candidate = heroGems[i];
            if (candidate != null && !essences.Contains(candidate) && IsDamageModifiedBy(info.damage, candidate))
            {
                essences.Add(candidate);
            }
        }

        string skillName = skill != null
            ? skill.GetFormattedSkillTitle()
            : null;

        ElementalType? elementalType = info.damage.elemental;

        string playerName = isLocalPlayer ? "You" : sourcePlayer.playerName;

        _data.AddDamage(
            producedDamage,
            appliedDamage,
            isLocalPlayer,
            skillName,
            "Basic / Other",
            essences,
            elementalType,
            playerName);
    }

    private static bool IsDamageModifiedBy(FinalDamageData finalDamage, Gem gem)
    {
        if (gem == null)
        {
            return false;
        }

        FieldInfo[] fields = finalDamage.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        for (int i = 0; i < fields.Length; i++)
        {
            object value = fields[i].GetValue(finalDamage);
            if (value is DamageData damageData && damageData.IsAmountModifiedBy(gem))
            {
                return true;
            }
        }

        PropertyInfo[] properties = finalDamage.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        for (int i = 0; i < properties.Length; i++)
        {
            if (properties[i].GetIndexParameters().Length != 0)
            {
                continue;
            }

            if (!typeof(DamageData).IsAssignableFrom(properties[i].PropertyType))
            {
                continue;
            }

            object value = properties[i].GetValue(finalDamage, null);
            if (value is DamageData damageData && damageData.IsAmountModifiedBy(gem))
            {
                return true;
            }
        }

        return false;
    }

    private static string GetEssenceLabel(Gem gem)
    {
        return gem != null ? gem.GetActorReadableName() : null;
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
        Debug.Log("[DPS Meter] Zone reset listener attached.");
    }

    private void DetachFromZoneManager()
    {
        if (_zoneManager != null)
        {
            _zoneManager.ClientEvent_OnZoneLoadStarted -= OnZoneLoadStarted;
        }

        _zoneManager = null;
    }
}