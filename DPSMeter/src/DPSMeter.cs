using System;
using System.Collections.Generic;
using System.IO;
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
    private readonly HashSet<string> _healingSourceTraceTypes = new HashSet<string>(StringComparer.Ordinal);
    private static readonly object _debugLogLock = new object();
    private static readonly string _debugLogPath = Path.Combine(Application.persistentDataPath, "DPSMeter-debug.log");

    private static void WriteDebugLog(string message)
    {
        try
        {
            lock (_debugLogLock)
            {
                File.AppendAllText(
                    _debugLogPath,
                    "[v4.96]" + message + Environment.NewLine);
            }
        }
        catch (Exception ex)
        {
            Debug.Log("[DPS Meter] Failed to write debug log: " + ex.GetType().Name);
        }
    }
    private static void ClearDebugLog()
    {
        try
        {
            lock (_debugLogLock)
            {
                File.WriteAllText(_debugLogPath, string.Empty);
            }
        }
        catch (Exception ex)
        {
            Debug.Log("[DPS Meter] Failed to clear debug log: " + ex.GetType().Name);
        }
    }

    private System.Func<EventInfoTravelToNodeInterrupt, bool> _travelInterruptHandler;
    private void Awake()
    {
        ClearDebugLog();
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

        // Healing Essences can report their heal through an AbilityInstance
        // whose first trigger belongs to the host Memory. Resolve the Gem from
        // the actor chain first so an Essence heal is attributed to the Essence
        // rather than the Memory that contains it.
        Gem healingGem = FindDirectGemMember(info.actor);
        if (healingGem == null)
        {
            Actor current = info.actor.parentActor;
            int depth = 0;
            while (current != null && depth < 8 && healingGem == null)
            {
                healingGem = FindDirectGemMember(current);
                current = current.parentActor;
                depth++;
            }
        }

        string sourceName = GetHealingSourceName(info.actor);
        string localizedActorName = TryGetLocalizedHealingActorName(info.actor);
        if (!string.IsNullOrEmpty(localizedActorName))
        {
            sourceName = localizedActorName;
        }

        string sourceIdentity = GetSkillSlotIdentity(info.actor);

        if (healingGem != null)
        {
            sourceIdentity = GetEssenceIdentity(healingGem);
            sourceName = GetLocalizedEssenceName(healingGem) ?? sourceName;
        }

        Sprite healingIcon = FindHealingIcon(healingGem ?? info.actor);
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

        // Prefer the Gem directly attached to the barrier status effect.
        // This identifies the actual barrier creator rather than the skill
        // that happened to trigger an Essence.
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

            sourceName = GetLocalizedEssenceName(directGem) ?? sourceIdentity;
            icon = FindSpriteMember(directGem);
            return;
        }

        // Innate Memory barriers expose their originating skill through the
        // status effect's parentActor chain.
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

                sourceName = GetLocalizedEssenceName(directGem) ?? sourceIdentity;
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

        // Character-specific passives can use a different actor structure from
        // the generic Essence/status-effect path. Walk the actor chain for both
        // a localized Star/passive name and its icon before falling back to the
        // raw status-effect name.
        Actor fallbackActor = statusActor;
        int fallbackDepth = 0;
        while (fallbackActor != null && fallbackDepth < 8)
        {
            string starName = TryGetStarDisplayName(fallbackActor);
            if (!string.IsNullOrEmpty(starName))
            {
                sourceName = starName;
                sourceIdentity = fallbackActor.name;
                icon = FindSpriteInActorChain(statusActor);
                return;
            }

            fallbackActor = fallbackActor.parentActor;
            fallbackDepth++;
        }

        sourceName = statusActor.name;
        icon = FindSpriteInActorChain(statusActor);
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

        if (gemProperty != null &&
            gemProperty.GetIndexParameters().Length == 0 &&
            gemProperty.GetMethod != null)
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

        PropertyInfo networkGemProperty = type.GetProperty(
            "Network_gem",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        if (networkGemProperty != null &&
            networkGemProperty.GetIndexParameters().Length == 0 &&
            networkGemProperty.GetMethod != null)
        {
            try
            {
                Gem gem = networkGemProperty.GetValue(value, null) as Gem;
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


    private static string TryGetStarDisplayName(Actor source)
    {
        if (source == null)
        {
            return null;
        }

        string key = source.name;
        if (string.IsNullOrEmpty(key))
        {
            return null;
        }

        int suffix = key.IndexOf('(');
        if (suffix > 0)
        {
            key = key.Substring(0, suffix).Trim();
        }

        if (!key.StartsWith("Se_Star_", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        try
        {
            string displayName = DewLocalization.GetStarName(key);
            return string.IsNullOrEmpty(displayName) ? null : displayName;
        }
        catch (Exception)
        {
            return null;
        }
    }


    private static string GetHealingSourceName(Actor source)
    {
        string starName = TryGetStarDisplayName(source);
        if (!string.IsNullOrEmpty(starName))
        {
            return starName;
        }

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

    private string TryGetLocalizedHealingActorName(Actor source)
    {
        if (source == null)
        {
            return null;
        }

        string actorType = source.GetType().Name;
        bool trace = _healingSourceTraceTypes.Add(actorType);

        // Match the game's own Actor/StatusEffect display-name resolution:
        // first try the localized "<TypeName>_Name" UI key for the active language.
        string uiKey = actorType + "_Name";
        try
        {
            string localizedName;
            bool resolved = DewLocalization.TryGetUIValue(uiKey, out localizedName);
            if (resolved && !string.IsNullOrEmpty(localizedName))
            {
                return localizedName;
            }
        }
        catch (Exception)
        {
        }

        // The confirmed next reverse-engineering target for this actor is its
        // parent actor. Keep this diagnostic focused on that object only.
        if (trace)
        {
            TraceHealingParentMetadata(source);
        }

        // Keep the existing skill-localization fallback for actors that really
        // are represented by a skill localization entry.
        try
        {
            string skillKey = DewLocalization.GetSkillKey(source.GetType());
            string displayName = null;

            if (!string.IsNullOrEmpty(skillKey))
            {
                displayName = DewLocalization.GetSkillName(skillKey, 0);
            }

            SkillTrigger skillTrigger = source.firstTrigger as SkillTrigger;
            if (skillTrigger != null)
            {
                try
                {
                    string skillNameFromTrigger = DewLocalization.GetSkillName(skillTrigger, 0);
                    if (!string.IsNullOrEmpty(skillNameFromTrigger) &&
                        !skillNameFromTrigger.StartsWith("skills.!", StringComparison.Ordinal))
                    {
                        return skillNameFromTrigger;
                    }
                }
                catch (Exception)
                {
                }
            }

            if (!string.IsNullOrEmpty(displayName) &&
                !displayName.StartsWith("skills.!", StringComparison.Ordinal))
            {
                return displayName;
            }
        }
        catch (Exception)
        {
        }

        return null;
    }

    private static void TraceHealingParentMetadata(Actor source)
    {
        if (source == null || source.parentActor == null)
        {
            return;
        }

        Actor parent = source.parentActor;
        Type type = parent.GetType();

        try
        {
            WriteDebugLog(
                "[DPS Meter][HEAL LOCALIZATION TRACE] parentMetadata type=" +
                (type.FullName ?? type.Name) +
                " name=" + (parent.name ?? "<null>"));

            FieldInfo[] fields = type.GetFields(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            int logged = 0;
            for (int i = 0; i < fields.Length && logged < 32; i++)
            {
                FieldInfo field = fields[i];
                if (field.IsStatic)
                {
                    continue;
                }

                string fieldName = field.Name;
                string lowerName = fieldName.ToLowerInvariant();
                if (lowerName.IndexOf("name", StringComparison.Ordinal) < 0 &&
                    lowerName.IndexOf("local", StringComparison.Ordinal) < 0 &&
                    lowerName.IndexOf("key", StringComparison.Ordinal) < 0 &&
                    lowerName.IndexOf("skill", StringComparison.Ordinal) < 0 &&
                    lowerName.IndexOf("room", StringComparison.Ordinal) < 0 &&
                    lowerName.IndexOf("display", StringComparison.Ordinal) < 0 &&
                    lowerName.IndexOf("title", StringComparison.Ordinal) < 0 &&
                    lowerName.IndexOf("description", StringComparison.Ordinal) < 0)
                {
                    continue;
                }

                object value;
                try
                {
                    value = field.GetValue(parent);
                }
                catch (Exception ex)
                {
                    WriteDebugLog(
                        "[DPS Meter][HEAL LOCALIZATION TRACE] parentField=" + fieldName +
                        " readException=" + ex.GetType().Name);
                    logged++;
                    continue;
                }

                string rendered = value == null
                    ? "<null>"
                    : value is string || field.FieldType.IsPrimitive || field.FieldType.IsEnum || field.FieldType == typeof(decimal)
                        ? value.ToString()
                        : (value.GetType().FullName ?? value.GetType().Name);

                WriteDebugLog(
                    "[DPS Meter][HEAL LOCALIZATION TRACE] parentField=" + fieldName +
                    " type=" + (field.FieldType.FullName ?? field.FieldType.Name) +
                    " value=" + rendered);
                logged++;
            }

            if (logged == 0)
            {
                WriteDebugLog(
                    "[DPS Meter][HEAL LOCALIZATION TRACE] parentMetadata noRelevantFields");
            }
        }
        catch (Exception ex)
        {
            WriteDebugLog(
                "[DPS Meter][HEAL LOCALIZATION TRACE] parentMetadata traceException=" +
                ex.GetType().Name);
        }
    }


    private static string GetEssenceIdentity(Gem gem)
    {
        if (gem == null)
        {
            return null;
        }

        string key = gem.GetOriginalName();
        if (string.IsNullOrEmpty(key))
        {
            key = gem.name;
        }

        return string.IsNullOrEmpty(key) ? gem.GetType().Name : key;
    }

    private static string GetLocalizedEssenceName(Gem gem)
    {
        if (gem == null)
        {
            return null;
        }

        try
        {
            string name = DewLocalization.GetGemName(gem);
            return string.IsNullOrEmpty(name) ? null : name;
        }
        catch (Exception)
        {
            return null;
        }
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

        // Scaling and elemental type are separate pieces of information.
        // The final damage event may expose an elemental value even when the
        // source still has a useful AD/AP/HP scaler. Always resolve the source
        // scaling instead of suppressing it whenever elemental damage is set.
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

        // Character abilities such as Bismuth's Valiant Heart can carry their
        // scaling on the skill's configured AbilityInstance rather than on a
        // Gem. Check that configured instance before falling back to the live
        // damage actor.
        if (scaling == DpsData.DamageScalingType.None &&
            skill != null &&
            skill.currentConfig != null)
        {
            scaling = FindConfiguredAbilityScaling(skill.currentConfig.spawnedInstance, 0);
        }

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

        // First use the Essence's configured data. This keeps socketed
        // Essences independent from the host skill's scaler.
        DpsData.DamageScalingType scaling = FindConfiguredGemScaling(gem);

        // Some Essences expose their actual scaler only on the runtime
        // AbilityInstance that owns the Essence. Resolve that runtime source
        // by matching the same Essence identity, rather than walking into the
        // host skill's unrelated scaler (for example Valiant Heart = AD).
        if (scaling == DpsData.DamageScalingType.None)
        {
            scaling = FindRuntimeEssenceScaling(actor, gem, 0);
        }

        if (scaling != DpsData.DamageScalingType.None)
        {
            _essenceScalingCache[gem] = scaling;
        }

        return scaling;
    }


    private static DpsData.DamageScalingType FindRuntimeEssenceScaling(
        Actor actor,
        Gem gem,
        int depth)
    {
        if (actor == null || gem == null || depth > 8)
        {
            return DpsData.DamageScalingType.None;
        }

        AbilityInstance instance = actor as AbilityInstance;
        if (instance != null && AreSameGemIdentity(instance.gem, gem))
        {
            DamageInstance damageInstance = instance as DamageInstance;
            if (damageInstance != null)
            {
                DpsData.DamageScalingType scaling = GetScalingType(damageInstance.dmgFactor);
                if (scaling != DpsData.DamageScalingType.None)
                {
                    return scaling;
                }
            }

            DpsData.DamageScalingType fieldScaling = FindRuntimeDamageScaling(instance, "damage");
            if (fieldScaling != DpsData.DamageScalingType.None)
            {
                return fieldScaling;
            }

            fieldScaling = FindRuntimeDamageScaling(instance, "normalDamage");
            if (fieldScaling != DpsData.DamageScalingType.None)
            {
                return fieldScaling;
            }

            fieldScaling = FindRuntimeDamageScaling(instance, null);
            if (fieldScaling != DpsData.DamageScalingType.None)
            {
                return fieldScaling;
            }

            DpsData.DamageScalingType childScaling = FindConfiguredAbilityScaling(instance, 0);
            if (childScaling != DpsData.DamageScalingType.None)
            {
                return childScaling;
            }
        }

        return FindRuntimeEssenceScaling(actor.parentActor, gem, depth + 1);
    }


    private static bool AreSameGemIdentity(Gem first, Gem second)
    {
        if (first == null || second == null)
        {
            return false;
        }

        if (first == second)
        {
            return true;
        }

        string firstIdentity = first.GetOriginalName();
        string secondIdentity = second.GetOriginalName();

        return !string.IsNullOrEmpty(firstIdentity) &&
            !string.IsNullOrEmpty(secondIdentity) &&
            string.Equals(firstIdentity, secondIdentity, StringComparison.OrdinalIgnoreCase);
    }


    private static DpsData.DamageScalingType FindConfiguredGemAbilityScaling(
        AbilityInstance instance,
        Gem gem,
        int depth)
    {
        if (instance == null || gem == null || depth > 8)
        {
            return DpsData.DamageScalingType.None;
        }

        if (instance.gem == gem)
        {
            DamageInstance damageInstance = instance as DamageInstance;
            if (damageInstance != null)
            {
                DpsData.DamageScalingType scaling = GetScalingType(damageInstance.dmgFactor);
                if (scaling != DpsData.DamageScalingType.None)
                {
                    return scaling;
                }
            }

            FieldInfo dmgFactorField = instance.GetType().GetField(
                "dmgFactor",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (dmgFactorField != null && dmgFactorField.FieldType == typeof(ScalingValue))
            {
                try
                {
                    ScalingValue configuredScaling = (ScalingValue)dmgFactorField.GetValue(instance);
                    DpsData.DamageScalingType scaling = GetScalingType(configuredScaling);
                    if (scaling != DpsData.DamageScalingType.None)
                    {
                        return scaling;
                    }
                }
                catch (Exception)
                {
                }
            }

            DpsData.DamageScalingType nestedScaling = FindConfiguredAbilityScaling(instance, 0);
            if (nestedScaling != DpsData.DamageScalingType.None)
            {
                return nestedScaling;
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

            DpsData.DamageScalingType scaling = FindConfiguredGemAbilityScaling(child, gem, depth + 1);
            if (scaling != DpsData.DamageScalingType.None)
            {
                return scaling;
            }
        }

        return DpsData.DamageScalingType.None;
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

        FieldInfo dmgFactorField = instance.GetType().GetField(
            "dmgFactor",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        if (dmgFactorField != null && dmgFactorField.FieldType == typeof(ScalingValue))
        {
            try
            {
                ScalingValue configuredScaling = (ScalingValue)dmgFactorField.GetValue(instance);
                DpsData.DamageScalingType scaling = GetScalingType(configuredScaling);
                if (scaling != DpsData.DamageScalingType.None)
                {
                    return scaling;
                }
            }
            catch (Exception)
            {
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
            if (gem.skill == null || gem.skill.currentConfig == null)
            {
                return DpsData.DamageScalingType.None;
            }

            AbilityInstance configured = gem.skill.currentConfig.spawnedInstance;
            if (configured == null)
            {
                return DpsData.DamageScalingType.None;
            }

            // Mystic Dagger stores its configured scaling directly on the
            // configured Essence ability rather than a DamageInstance child.
            if (configured.GetType().Name == "Ai_E_MysticDagger")
            {
                FieldInfo damageField = configured.GetType().GetField(
                    "damage",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                if (damageField != null && damageField.FieldType == typeof(ScalingValue))
                {
                    ScalingValue scaling = (ScalingValue)damageField.GetValue(configured);
                    DpsData.DamageScalingType result = GetScalingType(scaling);
                    if (result != DpsData.DamageScalingType.None)
                    {
                        return result;
                    }
                }
            }

            // If the configured root is actually owned by this Gem, its
            // direct scaler is authoritative.
            if (configured.gem == gem)
            {
                DpsData.DamageScalingType directScaling = FindConfiguredAbilityScaling(configured, 0);
                if (directScaling != DpsData.DamageScalingType.None)
                {
                    return directScaling;
                }
            }

            // Some Essences expose their real damage scaler on a child
            // DamageInstance/AbilityInstance. Search the configured tree,
            // but deliberately skip the host skill's root so a character
            // skill such as Valiant Heart cannot donate its 2.4ad scaler to
            // an AP Essence socketed inside it.
            List<Actor> children = configured.children;
            if (children != null)
            {
                for (int i = 0; i < children.Count; i++)
                {
                    AbilityInstance child = children[i] as AbilityInstance;
                    if (child == null)
                    {
                        continue;
                    }

                    DpsData.DamageScalingType childScaling = FindConfiguredAbilityScaling(child, 0);
                    if (childScaling != DpsData.DamageScalingType.None)
                    {
                        return childScaling;
                    }
                }
            }

            return DpsData.DamageScalingType.None;
        }
        catch (Exception)
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