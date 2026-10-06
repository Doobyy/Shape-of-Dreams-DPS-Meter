using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace DPSMeter;

public sealed class DPSMeter : ModBehaviour
{
    public static DPSMeter Instance { get; private set; }

    private static readonly List<DamageProcessingContext> _damageContexts = new List<DamageProcessingContext>();

    private sealed class DamageProcessingContext
    {
        public Actor actor;
        public Entity victim;
        public readonly List<Gem> essences = new List<Gem>();
    }

    internal static List<Gem> GetActiveModifierEssences(Actor actor, Entity victim)
    {
        for (int i = _damageContexts.Count - 1; i >= 0; i--)
        {
            DamageProcessingContext context = _damageContexts[i];
            if (context.actor == actor && context.victim == victim)
            {
                return new List<Gem>(context.essences);
            }
        }

        return null;
    }

    [HarmonyPatch(typeof(Entity), nameof(Entity.ProcessReceivedDamage))]
    private static class ProcessReceivedDamagePatch
    {
        [HarmonyPrefix]
        private static void Prefix(Entity __instance, Actor actor)
        {
            _damageContexts.Add(new DamageProcessingContext
            {
                actor = actor,
                victim = __instance
            });
        }

        [HarmonyPostfix]
        private static void Postfix()
        {
            if (_damageContexts.Count > 0)
            {
                _damageContexts.RemoveAt(_damageContexts.Count - 1);
            }
        }
    }

    [HarmonyPatch(typeof(DamageData), nameof(DamageData.SetAmountModifiedBy), typeof(Actor))]
    private static class SetAmountModifiedByPatch
    {
        [HarmonyPostfix]
        private static void Postfix(Actor actor)
        {
            Gem gem = actor as Gem;
            if (gem == null || _damageContexts.Count == 0)
            {
                return;
            }

            DamageProcessingContext context = _damageContexts[_damageContexts.Count - 1];
            if (!context.essences.Contains(gem))
            {
                context.essences.Add(gem);
            }
        }
    }

    private ClientEventManager _clientEvents;
    private ZoneManager _zoneManager;
    private DpsData _data;
    private DpsOverlay _overlay;
    private bool _subscribed;
    private Hero _currentHero;

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
        _clientEvents = ClientEventManager.instance;

        if (_clientEvents == null || _subscribed)
        {
            return;
        }

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

        if (directGem == null)
        {
            directGem = info.actor.FindFirstOfType<Gem>();
        }

        if (directGem != null)
        {
            essences.Add(directGem);
        }

        List<Gem> modifierEssences = GetActiveModifierEssences(info.actor, info.victim);
        if (modifierEssences != null)
        {
            for (int i = 0; i < modifierEssences.Count; i++)
            {
                Gem modifierGem = modifierEssences[i];
                if (modifierGem != null && !essences.Contains(modifierGem))
                {
                    essences.Add(modifierGem);
                }
            }
        }

        string skillName = skill != null
            ? skill.GetFormattedSkillTitle()
            : "Basic / Other";

        ElementalType? elementalType = info.damage.elemental;

        string playerName = isLocalPlayer ? "You" : sourcePlayer.playerName;

        _data.AddDamage(
            producedDamage,
            appliedDamage,
            isLocalPlayer,
            skill,
            skillName,
            essences,
            elementalType,
            playerName);
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
        _zoneManager = ZoneManager.instance;

        if (_zoneManager == null)
        {
            return;
        }

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