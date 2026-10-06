using System.Collections.Generic;
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
    private System.Func<EventInfoTravelToNodeInterrupt, bool> _travelInterruptHandler;
    private readonly Dictionary<Gem, EssenceProcessorHooks> _essenceProcessorHooks = new Dictionary<Gem, EssenceProcessorHooks>();
    private readonly Dictionary<Gem, Stack<float>> _essenceProcessorStarts = new Dictionary<Gem, Stack<float>>();
    private readonly Dictionary<Gem, System.Action<EventInfoDamage>> _essenceDamageHandlers = new Dictionary<Gem, System.Action<EventInfoDamage>>();
    private readonly List<EssenceContribution> _pendingEssenceContributions = new List<EssenceContribution>();
    private float _nextEssenceProcessorRefreshTime;

    private sealed class EssenceProcessorHooks
    {
        public DataProcessor<DamageData, Actor, Entity> Before;
        public DataProcessor<DamageData, Actor, Entity> After;
    }

    private sealed class EssenceContribution
    {
        public Actor Source;
        public Entity Victim;
        public Gem Essence;
        public float Amount;
        public int Frame;
    }

    private void Awake()
    {
        Instance = this;
        _data = new DpsData();
        _overlay = gameObject.AddComponent<DpsOverlay>();
        _overlay.Initialize(_data);
        _travelInterruptHandler = OnTravelToNodeInterrupt;
        RefreshEssenceProcessors();

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

    private bool OnTravelToNodeInterrupt(EventInfoTravelToNodeInterrupt info)
    {
        if (_data != null)
        {
            _data.ResetCurrentInstance();
            Debug.Log("[DPS Meter] Reset current damage window for node travel " + info.from + " -> " + info.to + ".");
        }

        return false;
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
        // An Essence can create its own AbilityInstance/child actor. In that
        // case the damage event's actor chain can contain the Gem even when
        // the first AbilityInstance is not the Essence's instance.
        Gem directGem = info.actor.FindFirstOfType<Gem>();
        Dictionary<Gem, float> essenceContributions = ConsumeEssenceContributions(
            info.actor,
            info.victim,
            producedDamage);

        if (IsEssenceGem(directGem))
        {
            float directAmount;
            if (!essenceContributions.TryGetValue(directGem, out directAmount))
            {
                essenceContributions[directGem] = producedDamage;
            }
        }

        // Damage events carry the reaction chain that produced the hit. This
        // is different from the actor parent/ancestor tree: an Essence can
        // contribute to a reaction without being an ancestor of the actor
        // reported by OnTakeDamage.
        //
        // If exactly one equipped Essence is recorded in the reaction chain
        // and we have not already attributed this hit through the processor
        // hooks, treat the actual produced damage as that Essence's damage.
        // This is especially important for Essence effects that spawn their
        // own projectile/damage while the game's ledger still reports the
        // Memory as the actor.
        if (essenceContributions.Count == 0)
        {
            Gem reactionGem = FindSingleReactionEssence(info.actor, info.chain, sourceHero);
            if (reactionGem != null)
            {
                essenceContributions[reactionGem] = producedDamage;
            }
        }

        string skillName = skill != null
            ? skill.GetFormattedSkillTitle()
            : null;

        if (isLocalPlayer && skill != null && !string.IsNullOrEmpty(skillName))
        {
            Sprite icon = FindSkillIcon(skill);
            _data.RegisterSkillIcon(skillName, icon);
        }

        ElementalType? elementalType = info.damage.elemental;

        string playerName = isLocalPlayer ? "You" : sourcePlayer.playerName;

        _data.AddDamage(
            producedDamage,
            appliedDamage,
            isLocalPlayer,
            skillName,
            "Basic / Other",
            essenceContributions,
            elementalType,
            playerName);
    }

    private static Sprite FindSkillIcon(SkillTrigger skill)
    {
        if (skill == null)
            return null;

        if (skill.currentConfig != null && skill.currentConfig.triggerIcon != null)
            return skill.currentConfig.triggerIcon;

        if (skill.configs != null)
        {
            for (int i = 0; i < skill.configs.Length; i++)
            {
                if (skill.configs[i] != null && skill.configs[i].triggerIcon != null)
                    return skill.configs[i].triggerIcon;
            }
        }

        return null;
    }

    private void Update()
    {
        if (Time.time >= _nextEssenceProcessorRefreshTime)
        {
            _nextEssenceProcessorRefreshTime = Time.time + 0.5f;
            RefreshEssenceProcessors();
        }
    }

    private void RefreshEssenceProcessors()
    {
        if (DewPlayer.gamePlayers == null)
        {
            return;
        }

        for (int i = 0; i < DewPlayer.gamePlayers.Count; i++)
        {
            DewPlayer player = DewPlayer.gamePlayers[i];
            if (player == null || player.hero == null)
            {
                continue;
            }

            Hero hero = player.hero;

            if (hero.Skill != null && hero.Skill.gems != null)
            {
                foreach (Gem gem in hero.Skill.gems.Values)
                {
                    AttachEssenceProcessor(gem);
                }
            }

            Gem[] heroGems = hero.GetComponentsInChildren<Gem>(true);
            for (int j = 0; j < heroGems.Length; j++)
            {
                AttachEssenceProcessor(heroGems[j]);
            }
        }
    }

    private void AttachEssenceProcessor(Gem gem)
    {
        if (gem == null || _essenceProcessorHooks.ContainsKey(gem))
        {
            return;
        }

        DataProcessor<DamageData, Actor, Entity> before =
            (ref DamageData data, Actor from, Entity to) => OnEssenceProcessorBefore(gem, ref data);

        DataProcessor<DamageData, Actor, Entity> after =
            (ref DamageData data, Actor from, Entity to) => OnEssenceProcessorAfter(gem, ref data, from, to);

        gem.dealtDamageProcessor.Add(before, int.MinValue);
        gem.dealtDamageProcessor.Add(after, int.MaxValue);

        System.Action<EventInfoDamage> damageHandler = info => OnEssenceDealDamage(gem, info);

        gem.ActorEvent_OnDealDamage += damageHandler;

        _essenceProcessorHooks[gem] = new EssenceProcessorHooks
        {
            Before = before,
            After = after
        };

        _essenceDamageHandlers[gem] = damageHandler;
        _essenceProcessorStarts[gem] = new Stack<float>();
    }

    private Gem FindSingleReactionEssence(Actor damageActor, ReactionChain chain, Hero sourceHero)
    {
        if (sourceHero == null || sourceHero.Skill == null || sourceHero.Skill.gems == null)
        {
            return null;
        }

        Gem found = null;

        foreach (Gem gem in sourceHero.Skill.gems.Values)
        {
            if (!IsEssenceGem(gem))
            {
                continue;
            }

            if (!chain.DidReact(gem))
            {
                continue;
            }

            if (found != null && found != gem)
            {
                // More than one Essence participated in this reaction. We do
                // not guess how the final damage should be divided between
                // them. A later patch can use the processor/origin data for
                // that case.
                return null;
            }

            found = gem;
        }

        return found;
    }

    private static bool IsEssenceGem(Gem gem)
    {
        // Gem itself is the game's Essence type. Do not use location.index:
        // an equipped Essence can legitimately occupy index 0.
        return gem != null;
    }

    private void OnEssenceDealDamage(Gem gem, EventInfoDamage info)
    {
        if (gem == null || info.actor == null || info.victim == null || info.damage.amount <= 0f)
        {
            return;
        }

        _pendingEssenceContributions.Add(new EssenceContribution
        {
            Source = info.actor,
            Victim = info.victim,
            Essence = gem,
            Amount = Mathf.Max(0f, info.damage.amount + info.damage.discardedAmount),
            Frame = Time.frameCount
        });
    }

    private void OnEssenceProcessorBefore(Gem gem, ref DamageData data)
    {
        Stack<float> starts;
        if (!_essenceProcessorStarts.TryGetValue(gem, out starts))
        {
            starts = new Stack<float>();
            _essenceProcessorStarts[gem] = starts;
        }

        starts.Push(data.currentAmount);
    }

    private void OnEssenceProcessorAfter(Gem gem, ref DamageData data, Actor from, Entity to)
    {
        Stack<float> starts;
        if (!_essenceProcessorStarts.TryGetValue(gem, out starts) || starts.Count == 0)
        {
            return;
        }

        float before = starts.Pop();
        float contribution = data.currentAmount - before;

        if (contribution <= 0.0001f || from == null || to == null)
        {
            return;
        }

        _pendingEssenceContributions.Add(new EssenceContribution
        {
            Source = from,
            Victim = to,
            Essence = gem,
            Amount = contribution,
            Frame = Time.frameCount
        });
    }

    private Dictionary<Gem, float> ConsumeEssenceContributions(
        Actor source,
        Entity victim,
        float producedDamage)
    {
        Dictionary<Gem, float> result = new Dictionary<Gem, float>();

        for (int i = _pendingEssenceContributions.Count - 1; i >= 0; i--)
        {
            EssenceContribution pending = _pendingEssenceContributions[i];

            if (pending.Frame != Time.frameCount)
            {
                if (pending.Frame < Time.frameCount - 1)
                {
                    _pendingEssenceContributions.RemoveAt(i);
                }

                continue;
            }

            if (!AreActorsRelated(pending.Source, source) || pending.Victim != victim)
            {
                continue;
            }

            // A processor contribution is only the amount this Essence added
            // to the damage. It will normally be smaller than the final hit,
            // so matching the contribution against producedDamage would reject
            // the exact modifier contribution we are trying to track.
            //
            // Actor ancestry is the correlation key here: the API guarantees
            // that an actor's damage is processed by that actor and every
            // ancestor's dealtDamageProcessor.
            if (pending.Amount <= 0.0001f)
            {
                continue;
            }

            _pendingEssenceContributions.RemoveAt(i);

            if (pending.Essence == null)
            {
                continue;
            }

            float current;
            result.TryGetValue(pending.Essence, out current);
            result[pending.Essence] = current + pending.Amount;
        }

        return result;
    }

    private static bool AreActorsRelated(Actor a, Actor b)
    {
        if (a == null || b == null)
        {
            return false;
        }

        if (a == b)
        {
            return true;
        }

        return a.IsDescendantOf(b) || b.IsDescendantOf(a);
    }

    private void DetachEssenceProcessors()
    {
        foreach (KeyValuePair<Gem, EssenceProcessorHooks> pair in _essenceProcessorHooks)
        {
            Gem gem = pair.Key;
            EssenceProcessorHooks hooks = pair.Value;

            if (gem != null && hooks != null)
            {
                gem.dealtDamageProcessor.Remove(hooks.Before);
                gem.dealtDamageProcessor.Remove(hooks.After);

                System.Action<EventInfoDamage> damageHandler;
                if (_essenceDamageHandlers.TryGetValue(gem, out damageHandler))
                {
                    gem.ActorEvent_OnDealDamage -= damageHandler;
                }
            }
        }

        _essenceProcessorHooks.Clear();
        _essenceDamageHandlers.Clear();
        _essenceProcessorStarts.Clear();
        _pendingEssenceContributions.Clear();
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
        DetachEssenceProcessors();
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