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
    private System.Func<EventInfoTravelToNodeInterrupt, bool> _travelInterruptHandler;
    private bool _healingDiagnosticLogged;
    private int _healingEventDiagnosticCount;
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
        _clientEvents.OnLocalHeroAbilityChanged += OnLocalHeroAbilityChanged;
        _subscribed = true;
        LogHealingEventCandidates();
        Debug.Log("[DPS Meter] Damage event listener attached.");
    }

    private void DetachFromClientEvents()
    {
        if (_clientEvents != null && _subscribed)
        {
            _clientEvents.OnTakeDamage -= OnTakeDamage;
            _clientEvents.OnTakeHeal -= OnTakeHeal;
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
        _healingEventDiagnosticCount = 0;
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
        _data.AddHealing(healing, sourceName);

        const int maxEvents = 20;
        if (_healingEventDiagnosticCount < maxEvents)
        {
            _healingEventDiagnosticCount++;
            TraceHealingEvent(info, _healingEventDiagnosticCount);
        }
    }

    private static string GetHealingSourceName(Actor source)
    {
        if (source == null)
        {
            return "Unknown Healing";
        }

        Gem gem = source as Gem;
        if (gem != null)
        {
            string gemKey = gem.GetOriginalName();
            if (!string.IsNullOrEmpty(gemKey))
            {
                return gemKey;
            }

            return string.IsNullOrEmpty(gem.name) ? gem.GetType().Name : gem.name;
        }

        SkillTrigger skill = source.firstTrigger as SkillTrigger;
        if (skill != null)
        {
            string skillName = skill.GetFormattedSkillTitle();
            if (!string.IsNullOrEmpty(skillName))
            {
                return skillName;
            }
        }

        string actorName = source.name;
        if (!string.IsNullOrEmpty(actorName))
        {
            actorName = actorName.Replace("(Adjusted)", string.Empty)
                .Replace("(Clone)", string.Empty)
                .Trim();

            if (!string.IsNullOrEmpty(actorName))
            {
                return actorName;
            }
        }

        return source.GetType().Name;
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

        ElementalType? elementalType = info.damage.elemental;
        DpsData.DamageScalingType scalingType = DpsData.DamageScalingType.None;

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

                if (isLocalPlayer && string.IsNullOrEmpty(skillName) && !string.IsNullOrEmpty(sourceName))
                {
                    _data.RegisterOtherIcon(sourceName, FindActorIcon(info.actor));
                }
            }
        }

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

    private void LogHealingEventCandidates()
    {
        if (_healingDiagnosticLogged || _clientEvents == null)
        {
            return;
        }

        _healingDiagnosticLogged = true;
        Type managerType = _clientEvents.GetType();

        Debug.Log("[DPS Meter][HEAL TRACE] ClientEventManager=" + managerType.FullName);

        EventInfo[] events = managerType.GetEvents(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        for (int i = 0; i < events.Length; i++)
        {
            EventInfo eventInfo = events[i];
            if (eventInfo == null || eventInfo.Name.IndexOf("heal", StringComparison.OrdinalIgnoreCase) < 0)
            {
                continue;
            }

            Debug.Log("[DPS Meter][HEAL TRACE] Event candidate: " + eventInfo.Name +
                " handler=" + (eventInfo.EventHandlerType != null ? eventInfo.EventHandlerType.FullName : "unknown"));
        }

        FieldInfo[] fields = managerType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        for (int i = 0; i < fields.Length; i++)
        {
            FieldInfo field = fields[i];
            if (field == null || field.Name.IndexOf("heal", StringComparison.OrdinalIgnoreCase) < 0)
            {
                continue;
            }

            Debug.Log("[DPS Meter][HEAL TRACE] Field candidate: " + field.Name +
                " type=" + field.FieldType.FullName);
        }

        PropertyInfo[] properties = managerType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        for (int i = 0; i < properties.Length; i++)
        {
            PropertyInfo property = properties[i];
            if (property == null || property.Name.IndexOf("heal", StringComparison.OrdinalIgnoreCase) < 0)
            {
                continue;
            }

            Debug.Log("[DPS Meter][HEAL TRACE] Property candidate: " + property.Name +
                " type=" + property.PropertyType.FullName);
        }

        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        for (int i = 0; i < assemblies.Length; i++)
        {
            Type[] types;
            try
            {
                types = assemblies[i].GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                types = ex.Types;
            }
            catch (Exception)
            {
                continue;
            }

            for (int j = 0; j < types.Length; j++)
            {
                Type type = types[j];
                if (type == null)
                {
                    continue;
                }

                string name = type.Name;
                if (name.IndexOf("EventInfo", StringComparison.OrdinalIgnoreCase) < 0 ||
                    name.IndexOf("heal", StringComparison.OrdinalIgnoreCase) < 0)
                {
                    continue;
                }

                Debug.Log("[DPS Meter][HEAL TRACE] Runtime type candidate: " + type.FullName);
            }
        }
    }

    private static void TraceHealingEvent(EventInfoHeal info, int eventNumber)
    {
        Type eventType = info.GetType();
        Debug.Log("[DPS Meter][HEAL TRACE] ===== HEAL EVENT #" + eventNumber + " =====");
        Debug.Log("[DPS Meter][HEAL TRACE] EventInfoHeal runtime type=" + eventType.FullName);

        FieldInfo[] fields = eventType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        for (int i = 0; i < fields.Length; i++)
        {
            FieldInfo field = fields[i];
            if (field == null)
            {
                continue;
            }

            object value = null;
            try
            {
                value = field.GetValue(info);
            }
            catch (Exception)
            {
            }

            Debug.Log("[DPS Meter][HEAL TRACE] EventInfoHeal." + field.Name +
                " type=" + field.FieldType.FullName +
                " value=" + DescribeDiagnosticValue(value));
        }

        PropertyInfo[] properties = eventType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        for (int i = 0; i < properties.Length; i++)
        {
            PropertyInfo property = properties[i];
            if (property == null || !property.CanRead)
            {
                continue;
            }

            object value = null;
            try
            {
                value = property.GetValue(info, null);
            }
            catch (Exception)
            {
            }

            Debug.Log("[DPS Meter][HEAL TRACE] EventInfoHeal." + property.Name +
                " type=" + property.PropertyType.FullName +
                " value=" + DescribeDiagnosticValue(value));
        }

        Actor source = null;
        Actor target = null;
        FieldInfo sourceField = eventType.GetField("actor", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (sourceField != null)
        {
            source = sourceField.GetValue(info) as Actor;
        }

        FieldInfo targetField = eventType.GetField("target", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (targetField != null)
        {
            target = targetField.GetValue(info) as Actor;
        }

        if (target == null)
        {
            targetField = eventType.GetField("victim", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (targetField != null)
            {
                target = targetField.GetValue(info) as Actor;
            }
        }

        if (source != null)
        {
            Actor current = source;
            int depth = 0;
            while (current != null && depth < 8)
            {
                Debug.Log("[DPS Meter][HEAL TRACE] Heal source actor[" + depth + "] type=" +
                    current.GetType().FullName + " name=" + current.name);
                TraceHealingActorDetails(current, depth);
                current = current.parentActor;
                depth++;
            }
        }

        if (target != null)
        {
            Debug.Log("[DPS Meter][HEAL TRACE] Heal target actor type=" +
                target.GetType().FullName + " name=" + target.name);
        }
    }

    private static void TraceHealingActorDetails(Actor actor, int depth)
    {
        if (actor == null)
        {
            return;
        }

        Debug.Log("[DPS Meter][HEAL TRACE] Heal actor[" + depth + "] type=" +
            actor.GetType().FullName + " name=" + actor.name);

        TraceHealingLinkMembers(actor, "actor", depth);

        if (actor.GetType().Name == "Se_Star_L_HealOnAttack")
        {
            TraceHealOnAttackDetails(actor);
        }

        AbilityInstance ability = actor as AbilityInstance;
        if (ability != null && ability.gem != null)
        {
            Gem gem = ability.gem;
            Debug.Log("[DPS Meter][HEAL TRACE] Heal actor[" + depth + "] GEM type=" +
                gem.GetType().FullName + " name=" + gem.name);
        }

        AbilityTrigger firstTrigger = actor.firstTrigger;
        if (firstTrigger != null)
        {
            Debug.Log("[DPS Meter][HEAL TRACE] Heal actor[" + depth + "] firstTrigger type=" +
                firstTrigger.GetType().FullName + " name=" + firstTrigger.name);
            TraceHealingLinkMembers(firstTrigger, "firstTrigger", depth);
        }
    }

    private static void TraceHealOnAttackDetails(Actor actor)
    {
        Type type = actor.GetType();

        Debug.Log("[DPS Meter][HEAL TRACE] ===== HEAL ON ATTACK DETAILS =====");
        Debug.Log("[DPS Meter][HEAL TRACE] HealOnAttack runtime type=" + type.FullName);
        Debug.Log("[DPS Meter][HEAL TRACE] HealOnAttack name=" + actor.name);

        Type currentType = type;
        int typeDepth = 0;

        while (currentType != null && typeDepth < 8)
        {
            FieldInfo[] fields = currentType.GetFields(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.DeclaredOnly);

            for (int i = 0; i < fields.Length; i++)
            {
                FieldInfo field = fields[i];
                if (field == null)
                {
                    continue;
                }

                string name = field.Name;
                if (name.IndexOf("star", StringComparison.OrdinalIgnoreCase) < 0 &&
                    name.IndexOf("heal", StringComparison.OrdinalIgnoreCase) < 0 &&
                    name.IndexOf("effect", StringComparison.OrdinalIgnoreCase) < 0 &&
                    name.IndexOf("data", StringComparison.OrdinalIgnoreCase) < 0 &&
                    name.IndexOf("variant", StringComparison.OrdinalIgnoreCase) < 0 &&
                    name.IndexOf("display", StringComparison.OrdinalIgnoreCase) < 0 &&
                    name.IndexOf("source", StringComparison.OrdinalIgnoreCase) < 0 &&
                    name.IndexOf("gem", StringComparison.OrdinalIgnoreCase) < 0 &&
                    name.IndexOf("skill", StringComparison.OrdinalIgnoreCase) < 0)
                {
                    continue;
                }

                object value = null;
                try
                {
                    value = field.GetValue(actor);
                }
                catch (Exception)
                {
                }

                Debug.Log("[DPS Meter][HEAL TRACE] HealOnAttack." + field.Name +
                    " declaredOn=" + currentType.FullName +
                    " type=" + field.FieldType.FullName +
                    " value=" + DescribeDiagnosticValue(value));
            }

            PropertyInfo[] properties = currentType.GetProperties(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.DeclaredOnly);

            for (int i = 0; i < properties.Length; i++)
            {
                PropertyInfo property = properties[i];
                if (property == null || property.GetIndexParameters().Length != 0 ||
                    !property.CanRead)
                {
                    continue;
                }

                string name = property.Name;
                if (name.IndexOf("star", StringComparison.OrdinalIgnoreCase) < 0 &&
                    name.IndexOf("heal", StringComparison.OrdinalIgnoreCase) < 0 &&
                    name.IndexOf("effect", StringComparison.OrdinalIgnoreCase) < 0 &&
                    name.IndexOf("data", StringComparison.OrdinalIgnoreCase) < 0 &&
                    name.IndexOf("variant", StringComparison.OrdinalIgnoreCase) < 0 &&
                    name.IndexOf("display", StringComparison.OrdinalIgnoreCase) < 0 &&
                    name.IndexOf("source", StringComparison.OrdinalIgnoreCase) < 0 &&
                    name.IndexOf("gem", StringComparison.OrdinalIgnoreCase) < 0 &&
                    name.IndexOf("skill", StringComparison.OrdinalIgnoreCase) < 0)
                {
                    continue;
                }

                object value = null;
                try
                {
                    value = property.GetValue(actor, null);
                }
                catch (Exception)
                {
                }

                Debug.Log("[DPS Meter][HEAL TRACE] HealOnAttack." + property.Name +
                    " declaredOn=" + currentType.FullName +
                    " propertyType=" + property.PropertyType.FullName +
                    " value=" + DescribeDiagnosticValue(value));
            }

            currentType = currentType.BaseType;
            typeDepth++;
        }
    }

    private static void TraceHealingLinkMembers(object value, string label, int depth)
    {
        if (value == null)
        {
            return;
        }

        Type type = value.GetType();
        int typeDepth = 0;

        while (type != null && typeDepth < 8)
        {
            FieldInfo[] fields = type.GetFields(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.DeclaredOnly);

            for (int i = 0; i < fields.Length; i++)
            {
                FieldInfo field = fields[i];
                if (field == null || !IsHealingLinkName(field.Name))
                {
                    continue;
                }

                object fieldValue = null;
                try
                {
                    fieldValue = field.GetValue(value);
                }
                catch (Exception)
                {
                }

                Debug.Log("[DPS Meter][HEAL TRACE] Heal " + label + "[" + depth + "]." +
                    field.Name + " declaredOn=" + type.FullName +
                    " type=" + field.FieldType.FullName +
                    " value=" + DescribeDiagnosticValue(fieldValue));
            }

            PropertyInfo[] properties = type.GetProperties(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.DeclaredOnly);

            for (int i = 0; i < properties.Length; i++)
            {
                PropertyInfo property = properties[i];
                if (property == null || property.GetIndexParameters().Length != 0 ||
                    !IsHealingLinkName(property.Name) || !property.CanRead)
                {
                    continue;
                }

                object propertyValue = null;
                try
                {
                    propertyValue = property.GetValue(value, null);
                }
                catch (Exception)
                {
                }

                Debug.Log("[DPS Meter][HEAL TRACE] Heal " + label + "[" + depth + "]." +
                    property.Name + " declaredOn=" + type.FullName +
                    " propertyType=" + property.PropertyType.FullName +
                    " value=" + DescribeDiagnosticValue(propertyValue));
            }

            type = type.BaseType;
            typeDepth++;
        }
    }

    private static bool IsHealingLinkName(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return false;
        }

        return name.IndexOf("gem", StringComparison.OrdinalIgnoreCase) >= 0 ||
               name.IndexOf("skill", StringComparison.OrdinalIgnoreCase) >= 0 ||
               name.IndexOf("ability", StringComparison.OrdinalIgnoreCase) >= 0 ||
               name.IndexOf("trigger", StringComparison.OrdinalIgnoreCase) >= 0 ||
               name.IndexOf("source", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private static string DescribeDiagnosticValue(object value)
    {
        if (value == null)
        {
            return "null";
        }

        Actor actor = value as Actor;
        if (actor != null)
        {
            return actor.GetType().FullName + " name=" + actor.name;
        }

        return value.ToString();
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
            return null;
        }

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
                        if (gem.skill == null)
            {
                return DpsData.DamageScalingType.None;
            }

            if (gem.skill.currentConfig == null)
            {
                return DpsData.DamageScalingType.None;
            }

            AbilityInstance configured = gem.skill.currentConfig.spawnedInstance;

            if (configured == null)
            {
                return DpsData.DamageScalingType.None;
            }

            if (configured.GetType().Name == "Ai_E_MysticDagger")
            {
                System.Reflection.FieldInfo damageField = configured.GetType().GetField(
                    "damage",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);

                if (damageField == null)
                {
                    return DpsData.DamageScalingType.None;
                }

                object value = damageField.GetValue(configured);

                if (!(value is ScalingValue))
                {
                    return DpsData.DamageScalingType.None;
                }

                ScalingValue scaling = (ScalingValue)value;

                return GetScalingType(scaling);
            }

            DamageInstance damageInstance = configured as DamageInstance;

            if (damageInstance != null)
            {
                ScalingValue damageScaling = damageInstance.dmgFactor;

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

            return DpsData.DamageScalingType.None;
        }
        catch (System.Exception)
        {
            return DpsData.DamageScalingType.None;
        }
    }









    private static DpsData.DamageScalingType FindDamageScalingType(Actor actor)
    {
        DamageInstance damageInstance = FindDamageInstance(actor);

        if (damageInstance != null)
        {
            DpsData.DamageScalingType scaling = GetScalingType(damageInstance.dmgFactor);
            if (scaling != DpsData.DamageScalingType.None)
            {
                return scaling;
            }
        }

        // Some runtime projectile/skill actors are not DamageInstance subclasses.
        // Their actual damage scaling is exposed as a ScalingValue field on the
        // actor itself (for example, a field named "damage"). Prefer that exact
        // runtime damage field before broader damage fields such as normalDamage
        // so a parent projectile's metadata does not win over the hit actor.
        DpsData.DamageScalingType runtimeScaling = FindRuntimeDamageScaling(actor, "damage");
        if (runtimeScaling != DpsData.DamageScalingType.None)
        {
            return runtimeScaling;
        }

        runtimeScaling = FindRuntimeDamageScaling(actor, "normalDamage");
        if (runtimeScaling != DpsData.DamageScalingType.None)
        {
            return runtimeScaling;
        }

        return FindRuntimeDamageScaling(actor, null);
    }

    private static DpsData.DamageScalingType FindRuntimeDamageScaling(Actor actor, string preferredFieldName)
    {
        Actor current = actor;
        int depth = 0;

        while (current != null && depth < 8)
        {
            FieldInfo[] fields = current.GetType().GetFields(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            for (int i = 0; i < fields.Length; i++)
            {
                FieldInfo field = fields[i];

                if (field.FieldType != typeof(ScalingValue))
                {
                    continue;
                }

                if (preferredFieldName != null &&
                    !string.Equals(field.Name, preferredFieldName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (preferredFieldName == null &&
                    field.Name.IndexOf("damage", StringComparison.OrdinalIgnoreCase) < 0)
                {
                    continue;
                }

                try
                {
                    ScalingValue scaling = (ScalingValue)field.GetValue(current);
                    DpsData.DamageScalingType result = GetScalingType(scaling);

                    if (result != DpsData.DamageScalingType.None)
                    {
                        return result;
                    }
                }
                catch (Exception)
                {
                }
            }

            current = current.parentActor;
            depth++;
        }

        return DpsData.DamageScalingType.None;
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

        return FindSpriteMember(skill.currentConfig) ?? FindSpriteMember(skill);
    }

    private static Sprite FindActorIcon(Actor actor)
    {
        Actor current = actor;
        int depth = 0;

        while (current != null && depth < 8)
        {
            Sprite icon = FindSpriteMember(current);
            if (icon != null)
                return icon;

            current = current.parentActor;
            depth++;
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