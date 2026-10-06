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
    private readonly HashSet<AbilityInstance> _tracedAbilityInstances = new HashSet<AbilityInstance>();
    private readonly HashSet<Gem> _tracedGems = new HashSet<Gem>();
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
        _skillScalingCache.Clear();
        _essenceScalingCache.Clear();
        _tracedAbilityInstances.Clear();
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

        TraceAbilitySource(info.actor);
        TraceGemSource(FindDamageSourceEssence(info.actor));

        SkillTrigger skill = info.actor.firstTrigger as SkillTrigger;
        // An Essence can create its own AbilityInstance/child actor. In that
        // case the damage event's actor chain can contain the Gem even when
        // the first AbilityInstance is not the Essence's instance.
        Gem directGem = FindDamageSourceEssence(info.actor);
        Dictionary<Gem, float> essenceContributions = new Dictionary<Gem, float>();
        bool isDirectEssenceDamage = directGem != null;

        if (isDirectEssenceDamage)
        {
            essenceContributions[directGem] = producedDamage;
        }

        // Essence-generated damage is already represented by its Essence row.
        // Do not also attribute that same hit to the parent Memory/Skill.

        string skillName = null;
        string sourceName = "Other";
        bool isBasicAttack = !isDirectEssenceDamage && skill == null;

        if (isLocalPlayer && isBasicAttack && _overlay != null)
        {
            Sprite basicAttackIcon = FindSpriteInActorChain(info.actor);
            if (basicAttackIcon != null)
            {
                _overlay.SetBasicAttackIcon(basicAttackIcon);
                Debug.Log("[DPS Meter][v4.11 TRACE] Basic Attack icon captured from actor chain sprite=" + basicAttackIcon.name +
                    " texture=" + (basicAttackIcon.texture != null ? basicAttackIcon.texture.name : "none"));
            }
        }

        if (!isDirectEssenceDamage)
        {
            if (skill != null)
            {
                string formattedSkillName = skill.GetFormattedSkillTitle();

                if (!string.IsNullOrEmpty(formattedSkillName))
                {
                    skillName = formattedSkillName;
                }
            }

            if (string.IsNullOrEmpty(skillName) && isBasicAttack)
            {
                sourceName = "Basic Attack";
            }
        }

        if (isLocalPlayer && skill != null && !string.IsNullOrEmpty(skillName))
        {
            Sprite icon = FindSkillIcon(skill);
            _data.RegisterSkillIcon(skillName, icon);
        }

        TraceMysticDaggerDamage(info.actor, info.damage);

        ElementalType? elementalType = info.damage.elemental;
        DpsData.DamageScalingType scalingType = DpsData.DamageScalingType.None;

        Debug.Log("[DPS Meter][v4.5 TRACE] damage skill=" +
            (skillName ?? sourceName) +
            " directEssence=" + isDirectEssenceDamage +
            " directGem=" + (directGem != null ? directGem.GetActorReadableName() : "none") +
            " elemental=" + (elementalType.HasValue ? elementalType.Value.ToString() : "none"));

        // The final damage event tells us the actual elemental result. Only
        // fall back to source scaling when no elemental result was produced.
        // Basic attacks are treated as AD-scaled when the game does not expose
        // a more specific runtime scaling source.
        if (!elementalType.HasValue)
        {
            if (isDirectEssenceDamage)
            {
                scalingType = GetCachedEssenceScaling(directGem, info.actor);
            }
            else if (!string.IsNullOrEmpty(skillName))
            {
                scalingType = GetCachedSkillScaling(skillName, skill, info.actor);
            }
            else if (isBasicAttack)
            {
                scalingType = DpsData.DamageScalingType.Ad;
            }
            else
            {
                scalingType = FindDamageScalingType(info.actor);
            }
        }

        Debug.Log("[DPS Meter][v4.5 TRACE] resolved scaling=" + scalingType +
            " skill=" + (skillName ?? sourceName));

        string playerName = isLocalPlayer ? "You" : sourcePlayer.playerName;

        _data.AddDamage(
            producedDamage,
            appliedDamage,
            isLocalPlayer,
            skillName,
            sourceName,
            essenceContributions,
            elementalType,
            playerName,
            isDirectEssenceDamage,
            scalingType);
    }

    private DpsData.DamageScalingType GetCachedSkillScaling(string skillName, SkillTrigger skill, Actor actor)
    {
        DpsData.DamageScalingType cached;
        if (_skillScalingCache.TryGetValue(skillName, out cached))
        {
            return cached;
        }

        // Some Memories, such as Mystic Dagger, are configured by an
        // Essence Gem whose spawned AbilityInstance is not a DamageInstance.
        // Use that configured Gem source before falling back to runtime damage
        // ancestry.
        Gem sourceGem = FindDamageSourceEssence(actor);

        if (sourceGem == null && skill != null)
        {
            sourceGem = FindGemOnSkillTrigger(skill, actor);
        }

        DpsData.DamageScalingType scaling = FindConfiguredGemScaling(sourceGem);

        if (scaling == DpsData.DamageScalingType.None)
        {
            scaling = FindDamageScalingType(actor);
        }

        if (scaling != DpsData.DamageScalingType.None)
        {
            _skillScalingCache[skillName] = scaling;
        }

        return scaling;
    }

    private static Gem FindGemOnSkillTrigger(SkillTrigger skill, Actor actor)
    {
        if (skill == null || actor == null)
        {
            return null;
        }

        Hero hero = actor.firstEntity as Hero;
        if (hero == null || hero.Skill == null)
        {
            return null;
        }

        HeroSkillLocation location;
        if (!hero.Skill.TryGetSkillLocation(skill, out location))
        {
            Debug.Log("[DPS Meter][v4.8 TRACE] SkillTrigger location lookup failed skill=" + skill.GetFormattedSkillTitle());
            return null;
        }

        Debug.Log("[DPS Meter][v4.4 TRACE] SkillTrigger location=" + location + " skill=" + skill.GetFormattedSkillTitle());

        IEnumerable<Gem> gems = hero.Skill.GetGemsInSkill(location);
        if (gems == null)
        {
            return null;
        }

        foreach (Gem gem in gems)
        {
            if (gem == null)
            {
                continue;
            }

            DpsData.DamageScalingType scaling = FindConfiguredGemScaling(gem);
            if (scaling != DpsData.DamageScalingType.None)
            {
                return gem;
            }
        }

        return null;
    }

    private DpsData.DamageScalingType GetCachedEssenceScaling(Gem gem, Actor actor)
    {
        if (gem == null)
        {
            return DpsData.DamageScalingType.None;
        }

        DpsData.DamageScalingType cached;
        if (_essenceScalingCache.TryGetValue(gem, out cached))
        {
            return cached;
        }

        DpsData.DamageScalingType scaling = FindConfiguredGemScaling(gem);
        if (scaling == DpsData.DamageScalingType.None)
        {
            scaling = FindDamageScalingType(actor);
        }

        if (scaling != DpsData.DamageScalingType.None)
        {
            _essenceScalingCache[gem] = scaling;
        }

        return scaling;
    }

    private void TraceMysticDaggerDamage(Actor actor, FinalDamageData damage)
    {
        Actor current = actor;
        int depth = 0;

        while (current != null && depth < 12)
        {
            AbilityInstance instance = current as AbilityInstance;
            if (instance != null)
            {
                DamageInstance damageInstance = instance as DamageInstance;
                if (damageInstance != null)
                {
                    ScalingValue scaling = damageInstance.dmgFactor;
                    Debug.Log($"[DPS Meter][v3.9 TRACE] Damage ancestry depth={depth} type={instance.GetType().Name} gem={(instance.gem != null ? instance.gem.GetActorReadableName() : "none")} scaling=ad={scaling.adFactor}, ap={scaling.apFactor}, addedHp={scaling.addedHpFactor}, base={scaling.baseValue} elemental={damageInstance.elemental}");
                }
            }

            current = current.parentActor;
            depth++;
        }
    }

    private void TraceGemSource(Gem gem)
    {
        if (gem == null || !_tracedGems.Add(gem))
        {
            return;
        }

        SkillTrigger skill = gem.skill;
        if (skill == null)
        {
            Debug.Log("[DPS Meter][v3.9 TRACE] Gem has no SkillTrigger: " + gem.GetActorReadableName());
            return;
        }

        TriggerConfig config = skill.currentConfig;
        if (config == null)
        {
            Debug.Log("[DPS Meter][v3.9 TRACE] Gem skill has no current config: " + gem.GetActorReadableName());
            return;
        }

        AbilityInstance configured = config.spawnedInstance;
        string configuredName = configured != null ? configured.GetType().Name : "none";

        Debug.Log("[DPS Meter][v3.9 TRACE] Gem source=" + gem.GetActorReadableName()
            + " level=" + gem.effectiveLevel
            + " skill=" + skill.GetFormattedSkillTitle()
            + " configIndex=" + skill.currentConfigIndex
            + " spawnedInstance=" + configuredName);

        TraceConfiguredAbilityInstance(configured, 0);
    }

    private void TraceConfiguredAbilityInstance(AbilityInstance instance, int depth)
    {
        if (instance == null || depth > 4)
        {
            return;
        }

        DamageInstance damageInstance = instance as DamageInstance;
        if (damageInstance != null)
        {
            ScalingValue scaling = damageInstance.dmgFactor;
            Debug.Log("[DPS Meter][v3.9 TRACE] Configured ability depth=" + depth
                + " type=" + instance.GetType().Name
                + " scaling=ad=" + scaling.adFactor
                + ", ap=" + scaling.apFactor
                + ", addedHp=" + scaling.addedHpFactor
                + ", base=" + scaling.baseValue
                + " elemental=" + damageInstance.elemental);
        }
        else
        {
            Debug.Log("[DPS Meter][v3.9 TRACE] Configured ability depth=" + depth
                + " type=" + instance.GetType().Name
                + " no DamageInstance");
        }

        List<Actor> children = instance.children;
        if (children == null)
        {
            return;
        }

        for (int i = 0; i < children.Count; i++)
        {
            AbilityInstance child = children[i] as AbilityInstance;
            if (child != null)
            {
                TraceConfiguredAbilityInstance(child, depth + 1);
            }
        }
    }

    private static DpsData.DamageScalingType FindConfiguredAbilityScaling(AbilityInstance instance, int depth)
    {
        if (instance == null || depth > 6)
        {
            return DpsData.DamageScalingType.None;
        }

        DamageInstance damageInstance = instance as DamageInstance;
        if (damageInstance != null)
        {
            DpsData.DamageScalingType scaling = GetScalingType(damageInstance.dmgFactor);
            if (scaling != DpsData.DamageScalingType.None)
            {
                return scaling;
            }
        }

        List<Actor> children = instance.children;
        if (children == null)
        {
            return DpsData.DamageScalingType.None;
        }

        for (int i = 0; i < children.Count; i++)
        {
            AbilityInstance child = children[i] as AbilityInstance;
            if (child == null)
            {
                continue;
            }

            DpsData.DamageScalingType scaling = FindConfiguredAbilityScaling(child, depth + 1);
            if (scaling != DpsData.DamageScalingType.None)
            {
                return scaling;
            }
        }

        return DpsData.DamageScalingType.None;
    }

    private static DpsData.DamageScalingType GetScalingType(ScalingValue scaling)
    {
        float ad = Mathf.Max(0f, scaling.adFactor);
        float ap = Mathf.Max(0f, scaling.apFactor);
        float hp = Mathf.Max(0f, scaling.addedHpFactor);

        if (ad <= 0f && ap <= 0f && hp <= 0f)
        {
            return DpsData.DamageScalingType.None;
        }

        if (ap > ad && ap >= hp)
        {
            return DpsData.DamageScalingType.Ap;
        }

        if (hp > ad && hp > ap)
        {
            return DpsData.DamageScalingType.Hp;
        }

        return DpsData.DamageScalingType.Ad;
    }

    private static DpsData.DamageScalingType FindConfiguredGemScaling(Gem gem)
    {
        if (gem == null)
        {
            return DpsData.DamageScalingType.None;
        }

        try
        {
            UnityEngine.Debug.Log("[DPS Meter][v4.8 TRACE] FindConfiguredGemScaling gem=" + gem + " type=" + gem.GetType().Name + " originalName=" + gem.GetOriginalName());

            if (gem.skill == null)
            {
                UnityEngine.Debug.Log("[DPS Meter][v4.4 TRACE] gem.skill=null");
                return DpsData.DamageScalingType.None;
            }

            if (gem.skill.currentConfig == null)
            {
                UnityEngine.Debug.Log("[DPS Meter][v4.4 TRACE] gem.skill.currentConfig=null");
                return DpsData.DamageScalingType.None;
            }

            AbilityInstance configured = gem.skill.currentConfig.spawnedInstance;

            if (configured == null)
            {
                UnityEngine.Debug.Log("[DPS Meter][v4.4 TRACE] spawnedInstance=null");
                return DpsData.DamageScalingType.None;
            }

            UnityEngine.Debug.Log("[DPS Meter][v4.4 TRACE] configured type=" + configured.GetType().Name);

            if (configured.GetType().Name == "Ai_E_MysticDagger")
            {
                System.Reflection.FieldInfo damageField = configured.GetType().GetField(
                    "damage",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);

                if (damageField == null)
                {
                    UnityEngine.Debug.Log("[DPS Meter][v4.4 TRACE] Mystic Dagger damage field=null");
                    return DpsData.DamageScalingType.None;
                }

                object value = damageField.GetValue(configured);

                if (!(value is ScalingValue))
                {
                    UnityEngine.Debug.Log("[DPS Meter][v4.4 TRACE] Mystic Dagger damage field type=" + damageField.FieldType);
                    return DpsData.DamageScalingType.None;
                }

                ScalingValue scaling = (ScalingValue)value;

                UnityEngine.Debug.Log("[DPS Meter][v4.4 TRACE] Mystic Dagger damage scaling=ad=" +
                    scaling.adFactor + ", ap=" + scaling.apFactor + ", addedHp=" + scaling.addedHpFactor +
                    ", base=" + scaling.baseValue);

                return GetScalingType(scaling);
            }

            DamageInstance damageInstance = configured as DamageInstance;

            if (damageInstance != null)
            {
                ScalingValue damageScaling = damageInstance.dmgFactor;

                UnityEngine.Debug.Log("[DPS Meter][v4.4 TRACE] configured DamageInstance scaling=ad=" +
                    damageScaling.adFactor + ", ap=" + damageScaling.apFactor +
                    ", addedHp=" + damageScaling.addedHpFactor + ", base=" + damageScaling.baseValue);

                DpsData.DamageScalingType directScaling = GetScalingType(damageScaling);
                if (directScaling != DpsData.DamageScalingType.None)
                {
                    return directScaling;
                }
            }

            // Some Essences expose their scaling on a child DamageInstance
            // rather than on the configured root AbilityInstance. Search the
            // configured ability tree before giving up and falling back to the
            // runtime damage actor.
            DpsData.DamageScalingType childScaling = FindConfiguredAbilityScaling(configured, 0);
            if (childScaling != DpsData.DamageScalingType.None)
            {
                return childScaling;
            }

            UnityEngine.Debug.Log("[DPS Meter][v4.4 TRACE] configured has no usable DamageInstance scaling");
            return DpsData.DamageScalingType.None;
        }
        catch (System.Exception ex)
        {
            UnityEngine.Debug.Log("[DPS Meter][v4.4 TRACE] FindConfiguredGemScaling exception=" + ex.GetType().Name + ": " + ex.Message);
            return DpsData.DamageScalingType.None;
        }
    }

    private static void TraceIdentityScaling(Gem gem, AbilityInstance configured)
    {
        if (gem == null || configured == null)
        {
            return;
        }

        string gemName = gem.GetActorReadableName();
        string originalName = gem.GetOriginalName();
        string skillName = gem.skill != null ? gem.skill.GetFormattedSkillTitle() : "none";

        Debug.Log("[DPS Meter][v4.8 IDENTITY TRACE] gem=" + gemName
            + " originalName=" + originalName
            + " gemType=" + gem.GetType().Name
            + " skill=" + skillName
            + " configIndex=" + (gem.skill != null ? gem.skill.currentConfigIndex.ToString() : "none")
            + " configuredType=" + configured.GetType().Name);

        TraceScalingFields("Gem", gem);
        TraceScalingFields("SkillTrigger", gem.skill);
        TraceScalingFields("TriggerConfig", gem.skill != null ? gem.skill.currentConfig : null);
        TraceScalingFields("Configured", configured);
    }

    private static void TraceScalingFields(string label, object target)
    {
        if (target == null)
        {
            return;
        }

        Type type = target.GetType();
        FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        for (int i = 0; i < fields.Length; i++)
        {
            FieldInfo field = fields[i];
            if (field.FieldType != typeof(ScalingValue))
            {
                continue;
            }

            try
            {
                ScalingValue scaling = (ScalingValue)field.GetValue(target);
                Debug.Log("[DPS Meter][v4.8 IDENTITY TRACE] " + label
                    + "." + field.Name
                    + " scaling=ad=" + scaling.adFactor
                    + ", ap=" + scaling.apFactor
                    + ", addedHp=" + scaling.addedHpFactor
                    + ", base=" + scaling.baseValue
                    + ", armor=" + scaling.armorFactor
                    + ", crit=" + scaling.critPercentageFactor);
            }
            catch (Exception ex)
            {
                Debug.Log("[DPS Meter][v4.8 IDENTITY TRACE] " + label
                    + "." + field.Name
                    + " read failed=" + ex.GetType().Name + ": " + ex.Message);
            }
        }

        PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        for (int i = 0; i < properties.Length; i++)
        {
            PropertyInfo property = properties[i];
            if (property.PropertyType != typeof(ScalingValue)
                || property.GetIndexParameters().Length != 0
                || !property.CanRead)
            {
                continue;
            }

            try
            {
                ScalingValue scaling = (ScalingValue)property.GetValue(target, null);
                Debug.Log("[DPS Meter][v4.8 IDENTITY TRACE] " + label
                    + "." + property.Name + " [property]"
                    + " scaling=ad=" + scaling.adFactor
                    + ", ap=" + scaling.apFactor
                    + ", addedHp=" + scaling.addedHpFactor
                    + ", base=" + scaling.baseValue
                    + ", armor=" + scaling.armorFactor
                    + ", crit=" + scaling.critPercentageFactor);
            }
            catch (Exception ex)
            {
                Debug.Log("[DPS Meter][v4.8 IDENTITY TRACE] " + label
                    + "." + property.Name + " [property] read failed="
                    + ex.GetType().Name + ": " + ex.Message);
            }
        }
    }

    private void TraceAbilityChildren(AbilityInstance source)
    {
        List<Actor> children = source.children;
        if (children == null)
            return;

        foreach (Actor child in children)
        {
            AbilityInstance instance = child as AbilityInstance;
            if (instance == null)
                continue;

            DamageInstance damageInstance = instance as DamageInstance;
            Gem gem = instance.gem;
            string gemName = gem != null ? gem.GetActorReadableName() : "none";

            if (damageInstance != null)
            {
                ScalingValue scaling = damageInstance.dmgFactor;
                Debug.Log($"[DPS Meter][v3.8 TRACE] MysticDagger child type={instance.GetType().Name} gem={gemName} scaling=ad={scaling.adFactor}, ap={scaling.apFactor}, addedHp={scaling.addedHpFactor}, base={scaling.baseValue} elemental={damageInstance.elemental}");
            }
            else
            {
                Debug.Log($"[DPS Meter][v3.8 TRACE] MysticDagger child type={instance.GetType().Name} gem={gemName} no DamageInstance");
            }
        }
    }

    private void TraceAbilitySource(Actor actor)
    {
        Actor current = actor;
        int depth = 0;

        while (current != null && depth < 8)
        {
            AbilityInstance instance = current as AbilityInstance;
            if (instance != null && _tracedAbilityInstances.Add(instance))
            {
                Gem gem = instance.gem;
                DamageInstance damageInstance = instance as DamageInstance;
                ScalingValue scaling = damageInstance != null ? damageInstance.dmgFactor : default(ScalingValue);

                string gemName = gem != null ? gem.GetActorReadableName() : "none";
                string typeName = instance.GetType().Name;
                string scalingText = damageInstance != null
                    ? $"ad={scaling.adFactor}, ap={scaling.apFactor}, addedHp={scaling.addedHpFactor}, base={scaling.baseValue}"
                    : "no DamageInstance/dmgFactor";

                Debug.Log($"[DPS Meter][v3.8 TRACE] AbilityInstance type={typeName} gem={gemName} scaling={scalingText}");

                if (typeName == "Ai_E_MysticDagger")
                    TraceAbilityChildren(instance);
            }

            current = current.parentActor;
            depth++;
        }
    }

    private static DpsData.DamageScalingType FindDamageScalingType(Actor actor)
    {
        DamageInstance damageInstance = FindDamageInstance(actor);

        if (damageInstance == null)
        {
            return DpsData.DamageScalingType.None;
        }

        ScalingValue scaling = damageInstance.dmgFactor;

        float ad = Mathf.Max(0f, scaling.adFactor);
        float ap = Mathf.Max(0f, scaling.apFactor);
        float hp = Mathf.Max(0f, scaling.addedHpFactor);

        if (ad <= 0f && ap <= 0f && hp <= 0f)
        {
            return DpsData.DamageScalingType.None;
        }

        if (ap > ad && ap >= hp)
        {
            return DpsData.DamageScalingType.Ap;
        }

        if (hp > ad && hp > ap)
        {
            return DpsData.DamageScalingType.Hp;
        }

        return DpsData.DamageScalingType.Ad;
    }

    private static DamageInstance FindDamageInstance(Actor actor)
    {
        if (actor == null)
        {
            return null;
        }

        Actor current = actor;
        int depth = 0;

        while (current != null && depth < 8)
        {
            DamageInstance damageInstance = current as DamageInstance;
            if (damageInstance != null)
            {
                return damageInstance;
            }

            current = current.parentActor;
            depth++;
        }

        return null;
    }

    private static Sprite FindSpriteMember(object target)
    {
        if (target == null)
            return null;

        Type type = target.GetType();

        FieldInfo iconField = type.GetField("icon", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (iconField != null && typeof(Sprite).IsAssignableFrom(iconField.FieldType))
            return iconField.GetValue(target) as Sprite;

        PropertyInfo iconProperty = type.GetProperty("icon", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (iconProperty != null && typeof(Sprite).IsAssignableFrom(iconProperty.PropertyType))
            return iconProperty.GetValue(target, null) as Sprite;

        FieldInfo spriteField = type.GetField("sprite", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (spriteField != null && typeof(Sprite).IsAssignableFrom(spriteField.FieldType))
            return spriteField.GetValue(target) as Sprite;

        PropertyInfo spriteProperty = type.GetProperty("sprite", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (spriteProperty != null && typeof(Sprite).IsAssignableFrom(spriteProperty.PropertyType))
            return spriteProperty.GetValue(target, null) as Sprite;

        return null;
    }

    private static Sprite FindSpriteInActorChain(Actor actor)
    {
        Actor current = actor;
        int depth = 0;

        while (current != null && depth < 8)
        {
            Sprite icon = FindSpriteMember(current);
            if (icon != null)
            {
                return icon;
            }

            AbilityInstance instance = current as AbilityInstance;
            if (instance != null && instance.gem != null)
            {
                icon = FindSpriteMember(instance.gem);
                if (icon != null)
                {
                    return icon;
                }

                if (instance.gem.skill != null)
                {
                    icon = FindSpriteMember(instance.gem.skill);
                    if (icon != null)
                    {
                        return icon;
                    }

                    if (instance.gem.skill.currentConfig != null)
                    {
                        icon = FindSpriteMember(instance.gem.skill.currentConfig);
                        if (icon != null)
                        {
                            return icon;
                        }
                    }
                }
            }

            current = current.parentActor;
            depth++;
        }

        return null;
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
            if (instance != null && instance.gem != null)
            {
                return instance.gem;
            }

            current = current.parentActor;
            depth++;
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