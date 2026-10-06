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

        Actor sourceActor = FindBarrierSourceActor(statusEffect, 0);
        if (sourceActor != null)
        {
            SkillTrigger skill = sourceActor.firstTrigger as SkillTrigger;
            sourceIdentity = GetSkillSlotIdentity(sourceActor, skill);

            if (skill != null)
            {
                sourceName = skill.GetFormattedSkillTitle();
                icon = FindSkillIcon(skill);
            }

            if (string.IsNullOrEmpty(sourceName))
            {
                Gem gem = sourceActor as Gem;
                if (gem != null)
                {
                    sourceName = gem.GetOriginalName();
                    icon = FindSpriteMember(gem);
                }
            }

            if (string.IsNullOrEmpty(sourceName))
            {
                sourceName = sourceActor.name;
            }
        }

        if (string.IsNullOrEmpty(sourceName) && statusEffect is UnityEngine.Object unityObject)
        {
            sourceName = unityObject.name;
            icon = FindSpriteMember(statusEffect);
        }

        if (string.IsNullOrEmpty(sourceName) && statusEffect != null)
        {
            sourceName = statusEffect.GetType().Name;
            icon = FindSpriteMember(statusEffect);
        }

        if (string.IsNullOrEmpty(sourceIdentity))
        {
            sourceIdentity = sourceName;
        }
    }

    private static Actor FindBarrierSourceActor(object value, int depth)
    {
        if (value == null || depth > 3)
        {
            return null;
        }

        Actor directActor = value as Actor;
        if (directActor != null)
        {
            return directActor;
        }

        Gem directGem = value as Gem;
        if (directGem != null)
        {
            return directGem;
        }

        Type type = value.GetType();
        FieldInfo[] fields = type.GetFields(
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        for (int i = 0; i < fields.Length; i++)
        {
            FieldInfo field = fields[i];
            if (field == null || !IsBarrierSourceMemberName(field.Name))
            {
                continue;
            }

            object fieldValue;
            try
            {
                fieldValue = field.GetValue(value);
            }
            catch (Exception)
            {
                continue;
            }

            Actor found = FindBarrierSourceActor(fieldValue, depth + 1);
            if (found != null)
            {
                return found;
            }
        }

        PropertyInfo[] properties = type.GetProperties(
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        for (int i = 0; i < properties.Length; i++)
        {
            PropertyInfo property = properties[i];
            if (property == null ||
                property.GetIndexParameters().Length != 0 ||
                property.GetMethod == null ||
                !IsBarrierSourceMemberName(property.Name))
            {
                continue;
            }

            object propertyValue;
            try
            {
                propertyValue = property.GetValue(value, null);
            }
            catch (Exception)
            {
                continue;
            }

            Actor found = FindBarrierSourceActor(propertyValue, depth + 1);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }

    private static bool IsBarrierSourceMemberName(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return false;
        }

        return name.IndexOf("source", StringComparison.OrdinalIgnoreCase) >= 0 ||
               name.IndexOf("owner", StringComparison.OrdinalIgnoreCase) >= 0 ||
               name.IndexOf("caster", StringComparison.OrdinalIgnoreCase) >= 0 ||
               name.IndexOf("actor", StringComparison.OrdinalIgnoreCase) >= 0 ||
               name.IndexOf("skill", StringComparison.OrdinalIgnoreCase) >= 0 ||
               name.IndexOf("gem", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private static string GetSkillSlotIdentity(Actor source)
    {
        if (source == null)
        {
            return null;
        }

        SkillTrigger skill = source.firstTrigger as SkillTrigger;
        if (skill == null)
        {
            Gem gem = source as Gem;
            if (gem != null)
            {
                skill = gem.skill;
            }
        }

        return GetSkillSlotIdentity(source, skill);
    }

    private static string GetSkillSlotIdentity(Actor source, SkillTrigger skill)
    {
        if (source == null || skill == null)
        {
            return null;
        }

        Hero hero = source.firstEntity as Hero;
        if (hero == null || hero.Skill == null)
        {
            return null;
        }

        HeroSkillLocation location;
        if (!hero.Skill.TryGetSkillLocation(skill, out location))
        {
            return null;
        }

        return location.ToString();
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

    private static Sprite FindHealingIcon(Actor source)
    {
        if (source == null)
        {
            return null;
        }

        Gem gem = source as Gem;
        if (gem != null)
        {
            Sprite icon = FindSpriteMember(gem);
            if (icon != null)
            {
                return icon;
            }
        }

        SkillTrigger skill = source.firstTrigger as SkillTrigger;
        if (skill != null)
        {
            Sprite icon = FindSkillIcon(skill);
            if (icon != null)
            {
                return icon;
            }
        }

        return FindSpriteInActorChain(source);
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
        string skillIdentity = null;
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
                skillIdentity = GetSkillSlotIdentity(info.actor, skill);
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
            _data.RegisterSkillIcon(skillIdentity, icon);
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
                scalingType = GetCachedSkillScaling(skillIdentity, skill, info.actor);
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
            skillIdentity,
            skillName,
            sourceName,
            essenceContributions,
            elementalType,
            playerName,
            isDirectEssenceDamage,
            scalingType);
    }

    private void LogBarrierEventCandidates()
    {
        if (_barrierDiagnosticLogged || _clientEvents == null)
        {
            return;
        }

        _barrierDiagnosticLogged = true;
        Type managerType = _clientEvents.GetType();

        Debug.Log("[DPS Meter][BARRIER TRACE] ClientEventManager=" + managerType.FullName);

        EventInfo[] events = managerType.GetEvents(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        for (int i = 0; i < events.Length; i++)
        {
            EventInfo eventInfo = events[i];
            if (eventInfo == null || !IsBarrierDiagnosticName(eventInfo.Name))
            {
                continue;
            }

            Debug.Log("[DPS Meter][BARRIER TRACE] Event candidate: " + eventInfo.Name +
                " handler=" + (eventInfo.EventHandlerType != null ? eventInfo.EventHandlerType.FullName : "unknown"));
        }

        FieldInfo[] fields = managerType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        for (int i = 0; i < fields.Length; i++)
        {
            FieldInfo field = fields[i];
            if (field == null || !IsBarrierDiagnosticName(field.Name))
            {
                continue;
            }

            Debug.Log("[DPS Meter][BARRIER TRACE] Field candidate: " + field.Name +
                " type=" + field.FieldType.FullName);
        }

        PropertyInfo[] properties = managerType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        for (int i = 0; i < properties.Length; i++)
        {
            PropertyInfo property = properties[i];
            if (property == null || !IsBarrierDiagnosticName(property.Name))
            {
                continue;
            }

            Debug.Log("[DPS Meter][BARRIER TRACE] Property candidate: " + property.Name +
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
                if (type == null || !IsBarrierDiagnosticType(type))
                {
                    continue;
                }

                Debug.Log("[DPS Meter][BARRIER TRACE] Runtime type candidate: " + type.FullName);

                FieldInfo[] typeFields = type.GetFields(
                    BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public |
                    BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

                for (int k = 0; k < typeFields.Length; k++)
                {
                    FieldInfo field = typeFields[k];
                    if (field == null || !IsBarrierDiagnosticName(field.Name))
                    {
                        continue;
                    }

                    Debug.Log("[DPS Meter][BARRIER TRACE] Type member: " + type.FullName + "." +
                        field.Name + " type=" + field.FieldType.FullName);
                }

                PropertyInfo[] typeProperties = type.GetProperties(
                    BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public |
                    BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

                for (int k = 0; k < typeProperties.Length; k++)
                {
                    PropertyInfo property = typeProperties[k];
                    if (property == null || !IsBarrierDiagnosticName(property.Name))
                    {
                        continue;
                    }

                    Debug.Log("[DPS Meter][BARRIER TRACE] Type member: " + type.FullName + "." +
                        property.Name + " type=" + property.PropertyType.FullName);
                }
            }
        }
    }

    private static bool IsBarrierDiagnosticType(Type type)
    {
        string name = type.FullName ?? type.Name;
        return name.IndexOf("barrier", StringComparison.OrdinalIgnoreCase) >= 0 ||
               name.IndexOf("shield", StringComparison.OrdinalIgnoreCase) >= 0 ||
               name.IndexOf("ward", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private static bool IsBarrierDiagnosticName(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return false;
        }

        return name.IndexOf("barrier", StringComparison.OrdinalIgnoreCase) >= 0 ||
               name.IndexOf("shield", StringComparison.OrdinalIgnoreCase) >= 0 ||
               name.IndexOf("ward", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private DpsData.DamageScalingType GetCachedSkillScaling(string skillIdentity, SkillTrigger skill, Actor actor)
    {
        DpsData.DamageScalingType cached;
        if (_skillScalingCache.TryGetValue(skillIdentity, out cached))
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
            _skillScalingCache[skillIdentity] = scaling;
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