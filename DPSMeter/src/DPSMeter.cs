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
        private readonly Dictionary<Gem, System.Action<EventInfoAbilityInstance>> _essenceAbilityHandlers = new Dictionary<Gem, System.Action<EventInfoAbilityInstance>>();
    private readonly Dictionary<AbilityInstance, Gem> _essenceAbilityInstances = new Dictionary<AbilityInstance, Gem>();
    private readonly List<EssenceContribution> _pendingEssenceContributions = new List<EssenceContribution>();
    private readonly Dictionary<Gem, System.Action<string>> _essenceSyncHandlers = new Dictionary<Gem, System.Action<string>>();
    private const string EssenceContributionSyncKey = "dps_meter_contribution";
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
        public float ReceivedTime;
        public uint VictimNetId;
        public uint TriggerNetId;
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
        Gem directGem = FindDamageSourceEssence(info.actor);
        AbilityInstance abilityInstance = info.actor.FindFirstOfType<AbilityInstance>();
        Gem abilityGem = null;
        if (abilityInstance != null)
        {
            _essenceAbilityInstances.TryGetValue(abilityInstance, out abilityGem);
        }
        // v3.0 diagnostic: specifically inspect Memory damage events for
        // Essences that modify the Memory's own damage amount. Projectile/on-hit
        // Essence damage is already solved and is excluded here.
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

        // Essence-generated damage is already represented by its Essence row.
        // Do not also attribute that same hit to the parent Memory/Skill.
        bool isDirectEssenceDamage = IsEssenceGem(directGem);

        string skillName = !isDirectEssenceDamage && skill != null
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
            playerName,
            isDirectEssenceDamage);
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

        System.Action<EventInfoAbilityInstance> abilityHandler =
            info => OnEssenceAbilityInstanceCreated(gem, info);

        gem.ActorEvent_OnAbilityInstanceCreated += abilityHandler;

        System.Action<string> syncHandler = key => OnEssenceContributionSynced(gem, key);
        gem.ClientEvent_OnPersistentSyncedDataChanged += syncHandler;

        _essenceProcessorHooks[gem] = new EssenceProcessorHooks
        {
            Before = before,
            After = after
        };

        _essenceAbilityHandlers[gem] = abilityHandler;
        _essenceSyncHandlers[gem] = syncHandler;
        _essenceProcessorStarts[gem] = new Stack<float>();
    }

    private void OnEssenceAbilityInstanceCreated(Gem gem, EventInfoAbilityInstance info)
    {
        if (gem == null || info.instance == null)
        {
            return;
        }

        _essenceAbilityInstances[info.instance] = gem;

    }

    private static bool IsEssenceGem(Gem gem)
    {
        // Gem itself is the game's Essence type. Do not use location.index:
        // an equipped Essence can legitimately occupy index 0.
        return gem != null;
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

        // DamageData processing is server-only. Send the exact delta produced by
        // this Essence to the client through the actor's synced persistent data.
        if (gem.owner != null)
        {
            uint victimNetId = to.persistentNetId;
            uint triggerNetId = from.firstTrigger != null
                ? from.firstTrigger.persistentNetId
                : 0u;

            gem.persistentSyncedData[EssenceContributionSyncKey] =
                contribution.ToString("R", System.Globalization.CultureInfo.InvariantCulture)
                + "|" + victimNetId
                + "|" + triggerNetId
                + "|" + Time.frameCount;
        }
    }

    private void OnEssenceContributionSynced(Gem gem, string key)
    {
        if (gem == null || key != EssenceContributionSyncKey)
        {
            return;
        }

        string payload;
        if (!gem.persistentSyncedData.TryGetValue(EssenceContributionSyncKey, out payload)
            || string.IsNullOrEmpty(payload))
        {
            return;
        }

        string[] parts = payload.Split('|');
        if (parts.Length < 4)
        {
            return;
        }

        float amount;
        uint victimNetId;
        uint triggerNetId;
        int serverFrame;

        if (!float.TryParse(parts[0], System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out amount)
            || !uint.TryParse(parts[1], out victimNetId)
            || !uint.TryParse(parts[2], out triggerNetId)
            || !int.TryParse(parts[3], out serverFrame)
            || amount <= 0.0001f)
        {
            return;
        }

        _pendingEssenceContributions.Add(new EssenceContribution
        {
            Source = null,
            Victim = null,
            Essence = gem,
            Amount = amount,
            Frame = serverFrame,
            ReceivedTime = Time.time,
            VictimNetId = victimNetId,
            TriggerNetId = triggerNetId
        });
    }

    private Dictionary<Gem, float> ConsumeEssenceContributions(
        Actor source,
        Entity victim,
        float producedDamage)
    {
        Dictionary<Gem, float> result = new Dictionary<Gem, float>();
        uint victimNetId = victim != null ? victim.persistentNetId : 0u;
        uint triggerNetId = source != null && source.firstTrigger != null
            ? source.firstTrigger.persistentNetId
            : 0u;

        for (int i = _pendingEssenceContributions.Count - 1; i >= 0; i--)
        {
            EssenceContribution pending = _pendingEssenceContributions[i];

            if (pending.Essence == null || Time.time - pending.ReceivedTime > 0.75f)
            {
                _pendingEssenceContributions.RemoveAt(i);
                continue;
            }

            if (pending.VictimNetId != victimNetId)
            {
                continue;
            }

            if (pending.TriggerNetId != 0u
                && triggerNetId != 0u
                && pending.TriggerNetId != triggerNetId)
            {
                continue;
            }

            _pendingEssenceContributions.RemoveAt(i);

            float current;
            result.TryGetValue(pending.Essence, out current);
            result[pending.Essence] = current + pending.Amount;
        }

        return result;
    }

    private static Gem FindDamageSourceEssence(Actor actor)
    {
        if (actor == null)
        {
            return null;
        }

        Actor current = actor;
        int depth = 0;

        while (current != null && depth < 8)
        {
            AbilityInstance instance = current as AbilityInstance;
            if (instance != null && IsEssenceGem(instance.gem))
            {
                return instance.gem;
            }

            current = current.parentActor;
            depth++;
        }

        return null;
    }


    private static string DescribeEssenceLocator(Gem gem, Actor damageActor)
    {
        if (gem == null || damageActor == null)
        {
            return "not-found";
        }

        Actor current = damageActor;
        int depth = 0;

        while (current != null && depth < 12)
        {
            AbilityInstance instance = current as AbilityInstance;
            if (instance != null && instance.gem == gem)
            {
                return "actor-depth=" + depth + ":" + instance.GetActorReadableName();
            }

            current = current.parentActor;
            depth++;
        }

        return "reaction-only";
    }

    private static string DescribeActorChain(Actor actor)
    {
        if (actor == null)
        {
            return "null";
        }

        List<string> names = new List<string>();
        Actor current = actor;
        int depth = 0;
        while (current != null && depth < 8)
        {
            names.Add(current.GetActorReadableName());
            current = current.parentActor;
            depth++;
        }

        return string.Join(" <- ", names.ToArray());
    }

    private static string DescribeActorChainDetailed(Actor actor)
    {
        if (actor == null)
        {
            return "null";
        }

        List<string> entries = new List<string>();
        Actor current = actor;
        int depth = 0;

        while (current != null && depth < 8)
        {
            string entry = current.GetActorReadableName()
                + " {type=" + current.GetType().FullName;

            AbilityInstance instance = current as AbilityInstance;
            if (instance != null)
            {
                entry += ", abilityGem="
                    + (instance.gem != null ? instance.gem.GetActorReadableName() : "null");
            }

            DamageInstance damageInstance = current as DamageInstance;
            if (damageInstance != null)
            {
                entry += ", damageOrigin=" + damageInstance.origin.ToString();
            }

            entry += "}";
            entries.Add(entry);

            current = current.parentActor;
            depth++;
        }

        return string.Join(" <- ", entries.ToArray());
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

                System.Action<EventInfoAbilityInstance> abilityHandler;
                if (_essenceAbilityHandlers.TryGetValue(gem, out abilityHandler))
                {
                    gem.ActorEvent_OnAbilityInstanceCreated -= abilityHandler;
                }

                System.Action<string> syncHandler;
                if (_essenceSyncHandlers.TryGetValue(gem, out syncHandler))
                {
                    gem.ClientEvent_OnPersistentSyncedDataChanged -= syncHandler;
                }
            }
        }

        _essenceProcessorHooks.Clear();
        _essenceAbilityHandlers.Clear();
        _essenceSyncHandlers.Clear();
        _essenceAbilityInstances.Clear();
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