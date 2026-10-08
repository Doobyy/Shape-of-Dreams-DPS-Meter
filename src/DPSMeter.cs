using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.IO;
using System.Text;

using UnityEngine;

namespace DPSMeter;

public sealed class DPSMeter : ModBehaviour
{
    public const string DevelopmentVersion = "v7.100";
    public static DPSMeter Instance { get; private set; }

    private ClientEventManager _clientEvents;
    private ZoneManager _zoneManager;
    private DpsData _data;
    private DpsOverlay _overlay;
    private bool _subscribed;
    private Hero _currentHero;
    private System.Func<EventInfoTravelToNodeInterrupt, bool> _travelInterruptHandler;
    private readonly Dictionary<string, DpsData.DamageScalingType> _skillScalingCache = new Dictionary<string, DpsData.DamageScalingType>();
    private readonly Dictionary<Gem, DpsData.DamageScalingType> _essenceScalingCache = new Dictionary<Gem, DpsData.DamageScalingType>();
    private readonly HashSet<string> _essenceScalingDiagnosticSeen = new HashSet<string>();
    private static readonly HashSet<string> _memoryScalingDiagnosticSeen = new HashSet<string>();
    private static readonly HashSet<string> _chompScalingDiagnosticSeen = new HashSet<string>();private static readonly object _debugLogLock = new object();
    private static readonly string _debugLogPath = Path.Combine(Application.persistentDataPath, "DPSMeter-debug.log");

    private static void WriteDebugLog(string message)
    {
        try
        {
            lock (_debugLogLock)
            {
                File.AppendAllText(_debugLogPath, message + Environment.NewLine);
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
        string sourceIdentity = GetSkillSlotIdentity(info.actor);


        // Passive Stars already have a dedicated resolver. Keep that path
        // authoritative so a generated actor cannot be renamed by an
        // unrelated localized parent.
        string starName = TryGetStarDisplayName(info.actor);
        if (!string.IsNullOrEmpty(starName))
        {
            sourceName = starName;
        }
        else
        {
            // Room Mods are a separate category: walk parents only far enough
            // to find a confirmed RoomMod_* actor, then resolve that category's
            // own UI name. Do not apply this parent walk to every healing actor.
            string roomModName = TryGetLocalizedRoomModName(info.actor);
            if (!string.IsNullOrEmpty(roomModName))
            {
                sourceName = roomModName;
            }
            else
            {
                // GenericHealOverTime is only the effect mechanism. A Shrine of
                // Guidance is identified by the actual Shrine_Guidance actor in
                // its chain, and its player-facing name comes from native UI
                // localization rather than a hard-coded string.
                string shrineIdentity;
                string shrineName = TryGetLocalizedShrineName(info.actor, out shrineIdentity);
                if (!string.IsNullOrEmpty(shrineName))
                {
                    sourceName = shrineName;
                    sourceIdentity = shrineIdentity;
                }
                else
                {
                    string localizedActorName = TryGetLocalizedHealingActorName(info.actor);
                    if (!string.IsNullOrEmpty(localizedActorName))
                    {
                        sourceName = localizedActorName;
                    }
                }
            }
        }

        if (healingGem != null)
        {
            sourceIdentity = GetEssenceIdentity(healingGem);
            sourceName = GetLocalizedEssenceName(healingGem) ?? sourceName;
        }

        Sprite healingIcon = FindHealingIcon(healingGem ?? info.actor);

        string healingActorChain = BuildHealingExportActorChain(info.actor);
        _data.AddHealing(healing, sourceIdentity, sourceName, healingIcon, healingActorChain);

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


    private static SkillTrigger FindSkillTriggerInActorChain(Actor source, out Actor skillSourceActor)
    {
        skillSourceActor = source;

        Actor current = source;
        int depth = 0;

        while (current != null && depth < 8)
        {
            SkillTrigger skill = current.firstTrigger as SkillTrigger;
            if (skill != null)
            {
                skillSourceActor = current;
                return skill;
            }

            current = current.parentActor;
            depth++;
        }

        return null;
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
        catch (Exception)        {
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

    private static string TryGetLocalizedRoomModName(Actor source)
    {
        if (source == null)
        {
            return null;
        }

        Actor current = source;
        int depth = 0;

        while (current != null && depth < 8)
        {
            string typeName = current.GetType().Name;
            if (typeName.StartsWith("RoomMod_", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    string localizedName;
                    if (DewLocalization.TryGetUIValue(typeName + "_Name", out localizedName) &&
                        !string.IsNullOrEmpty(localizedName))
                    {
                        return localizedName;
                    }
                }
                catch (Exception)
                {
                }

                return null;
            }

            current = current.parentActor;
            depth++;
        }

        return null;
    }

    private static string TryGetLocalizedShrineName(Actor source, out string sourceIdentity)
    {
        sourceIdentity = null;

        if (source == null)
        {
            return null;
        }

        Actor current = source;
        int depth = 0;
        while (current != null && depth < 8)
        {
            string typeName = current.GetType().Name;
            string actorName = current.name;
            int suffix = string.IsNullOrEmpty(actorName) ? -1 : actorName.IndexOf('(');
            if (suffix > 0)
            {
                actorName = actorName.Substring(0, suffix).Trim();
            }

            bool isShrineGuidance =
                string.Equals(typeName, "Shrine_Guidance", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(actorName, "Shrine_Guidance", StringComparison.OrdinalIgnoreCase);

            if (isShrineGuidance)
            {
                const string shrineKey = "Shrine_Guidance";
                sourceIdentity = shrineKey;

                try
                {
                    string localizedName;
                    if (DewLocalization.TryGetUIValue(shrineKey + "_Name", out localizedName) &&
                        !string.IsNullOrEmpty(localizedName))
                    {
                        return localizedName;
                    }
                }
                catch (Exception)
                {
                }

                return null;
            }

            current = current.parentActor;
            depth++;
        }

        return null;
    }


    private static string TryGetLocalizedHealingActorName(Actor source)
    {
        if (source == null)
        {
            return null;
        }

        // Preserve the pre-Lingering-Aura behavior for actors that are actually
        // represented by their own UI/skill localization. Parent localization
        // belongs to the dedicated RoomMod resolver above.
        try
        {
            string localizedName;
            if (DewLocalization.TryGetUIValue(source.GetType().Name + "_Name", out localizedName) &&
                !string.IsNullOrEmpty(localizedName))
            {
                return localizedName;
            }
        }
        catch (Exception)
        {
        }

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
private static bool IsPrismaticReadableNameILReference(string operandText)
    {
        return !string.IsNullOrEmpty(operandText) &&
            operandText.IndexOf("GetActorReadableName", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private static bool IsPrismaticRelevantILReference(string operandText)
    {
        if (string.IsNullOrEmpty(operandText))
        {
            return false;
        }

        string[] keywords =
        {
            "SourceType",
            "DamageData",
            "Actor",
            "Entity",
            "Ability",
            "Skill",
            "CastInfo",
            "Gem",
            "Trigger",
            "Name",
            "Title",
            "Display",
            "Localization",
            "Localized",
            "Identity",
            "Source",
            "AbilityIndex"
        };

        for (int i = 0; i < keywords.Length; i++)
        {
            if (operandText.IndexOf(keywords[i], StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }
        }

        return false;
    }


    private static int _healthOrbTraceCount;
    private static int _bismuthHealTraceCount;
    private static int _bismuthBookDamageTraceCount;

    private static void TraceBismuthBookDamage(EventInfoDamage info)
    {
        if (info.actor == null || _bismuthBookDamageTraceCount >= 8)
        {
            return;
        }

        _bismuthBookDamageTraceCount++;
        FinalDamageData damage = info.damage;

        WriteDebugLog("[" + DevelopmentVersion + "]  Bismuth Book damage trace " + _bismuthBookDamageTraceCount);
        WriteDebugLog("[" + DevelopmentVersion + "]  book   amount=" + damage.amount
            + " discarded=" + damage.discardedAmount
            + " source=" + damage.type
            + " elemental=" + damage.elemental
            + " attributes=" + damage.attributes
            + " attackEffectType=" + damage.attackEffectType
            + " attackEffectStrength=" + damage.attackEffectStrength);
        WriteDebugLog("[" + DevelopmentVersion + "]  book   resolvedScaling=" +
            FindDamageScalingType(info.actor));

        WriteDebugLog("[" + DevelopmentVersion + "]  book   eventActor type=" + info.actor.GetType().Name
            + " name=" + info.actor.name);

        TraceSourceChain(info.actor, "[" + DevelopmentVersion + "]  book");
    }


    private static string BuildHealingExportActorChain(Actor source)
    {
        if (source == null)
        {
            return null;
        }

        StringBuilder builder = new StringBuilder();
        Actor current = source;
        int depth = 0;

        while (current != null && depth < 8)
        {
            Type type = current.GetType();

            builder.Append("  actor[")
                .Append(depth)
                .Append("] type=")
                .Append(type.Name)
                .Append(" name=")
                .Append(current.name ?? "<null>");

            try
            {
                MethodInfo originalNameMethod = type.GetMethod(
                    "GetOriginalName",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                if (originalNameMethod != null &&
                    originalNameMethod.GetParameters().Length == 0 &&
                    originalNameMethod.ReturnType == typeof(string))
                {
                    string originalName = originalNameMethod.Invoke(current, null) as string;
                    if (!string.IsNullOrEmpty(originalName))
                    {
                        builder.Append(" original=").Append(originalName);
                    }
                }
            }
            catch (Exception)
            {
            }

            try
            {
                string localized;
                if (DewLocalization.TryGetUIValue(type.Name + "_Name", out localized) &&
                    !string.IsNullOrEmpty(localized))
                {
                    builder.Append(" uiName=").Append(localized);
                }
            }
            catch (Exception)
            {
            }

            SkillTrigger skill = current.firstTrigger as SkillTrigger;
            if (skill != null)
            {
                try
                {
                    string skillName = DewLocalization.GetSkillName(skill, 0);
                    if (!string.IsNullOrEmpty(skillName))
                    {
                        builder.Append(" skillName=").Append(skillName);
                    }
                }
                catch (Exception)
                {
                }
            }

            builder.AppendLine();
            current = current.parentActor;
            depth++;
        }

        return builder.ToString().TrimEnd();
    }


    private static void TraceHealthOrbSource(Actor source)
    {
        if (source == null || _healthOrbTraceCount >= 3)
        {
            return;
        }

        Actor current = source;
        bool foundGeneric = false;
        bool foundRegenOrb = false;

        while (current != null && !foundRegenOrb)
        {
            string typeName = current.GetType().Name;
            if (string.Equals(typeName, "Se_GenericHealOverTime", StringComparison.OrdinalIgnoreCase))
            {
                foundGeneric = true;
            }

            if (string.Equals(typeName, "Ai_RegenOrb_Projectile", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(typeName, "Pickup_RegenOrb", StringComparison.OrdinalIgnoreCase))
            {
                foundRegenOrb = true;
            }

            current = current.parentActor;
        }

        if (!foundGeneric || !foundRegenOrb)
        {
            return;
        }

        _healthOrbTraceCount++;
        WriteDebugLog("[" + DevelopmentVersion + "]  Health Orb trace " + _healthOrbTraceCount);
        TraceSourceChain(source, "[" + DevelopmentVersion + "]  orb");
        TraceHealingRuntimeData(source, "[" + DevelopmentVersion + "]  orb");
        TraceRegenOrbPickup(source);
    }

    private static void TraceHealingRuntimeData(Actor source, string label)
    {
        Actor current = source;
        int depth = 0;

        while (current != null && depth < 4)
        {
            Type type = current.GetType();
            FieldInfo[] fields = type.GetFields(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            int logged = 0;
            for (int i = 0; i < fields.Length && logged < 24; i++)
            {
                FieldInfo field = fields[i];

                if (field.FieldType != typeof(ScalingValue) &&
                    field.FieldType != typeof(float) &&
                    field.FieldType != typeof(int) &&
                    field.FieldType != typeof(string))
                {
                    continue;
                }

                try
                {
                    object value = field.GetValue(current);
                    if (value == null)
                    {
                        continue;
                    }

                    if (field.FieldType == typeof(string) && string.IsNullOrEmpty((string)value))
                    {
                        continue;
                    }

                    WriteDebugLog(label + "   runtime actor[" + depth + "] field=" +
                        field.Name + " type=" + field.FieldType.Name + " value=[" + value + "]");
                    logged++;
                }
                catch (Exception)
                {
                }
            }

            current = current.parentActor;
            depth++;
        }
    }


    private static void TraceRegenOrbPickup(Actor source)
    {
        Actor current = source;

        while (current != null)
        {
            if (string.Equals(current.GetType().Name, "Pickup_RegenOrb", StringComparison.OrdinalIgnoreCase))
            {
                WriteDebugLog("[" + DevelopmentVersion + "]  regen pickup begin");
                TraceReadableIdentity(current, "[" + DevelopmentVersion + "]  regen");
                TraceMainEffect(current, "[" + DevelopmentVersion + "]  regen");
                WriteDebugLog("[" + DevelopmentVersion + "]  regen pickup end");
                return;
            }

            current = current.parentActor;
        }
    }

    private static void TraceMainEffect(Actor pickup, string label)
    {
        if (pickup == null)
        {
            return;
        }

        Type type = pickup.GetType();
        FieldInfo field = null;

        while (type != null && field == null)
        {
            field = type.GetField(
                "mainEffect",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            type = type.BaseType;
        }

        if (field == null || !typeof(UnityEngine.Object).IsAssignableFrom(field.FieldType))
        {
            return;
        }

        UnityEngine.Object effect = null;

        try
        {
            effect = field.GetValue(pickup) as UnityEngine.Object;
        }
        catch (Exception)
        {
        }

        GameObject gameObject = effect as GameObject;
        if (gameObject == null)
        {
            return;
        }

        WriteDebugLog(label + "   mainEffect GameObject=[" + gameObject.name + "]");

        Component[] components = gameObject.GetComponentsInChildren<Component>(true);
        int logged = 0;

        for (int i = 0; i < components.Length && logged < 32; i++)
        {
            Component component = components[i];
            if (component == null)
            {
                continue;
            }

            Type componentType = component.GetType();
            WriteDebugLog(label + "   mainEffect component type=" +
                componentType.Name + " name=[" + component.name + "]");

            TraceComponentStringMembers(component, label + "   mainEffect");
            TraceComponentLocalization(componentType, label + "   mainEffect");
            logged++;
        }
    }


    private static void TraceComponentLocalization(Type componentType, string label)
    {
        if (componentType == null)
        {
            return;
        }

        string[] keys = new string[]
        {
            componentType.Name + "_Name",
            componentType.Name
        };

        for (int i = 0; i < keys.Length; i++)
        {
            try
            {
                string localized;
                if (DewLocalization.TryGetUIValue(keys[i], out localized) &&
                    !string.IsNullOrEmpty(localized))
                {
                    WriteDebugLog(label + "   uiKey=" + keys[i] + " value=[" + localized + "]");
                }
            }
            catch (Exception)
            {
            }
        }
    }

    private static void TraceComponentStringMembers(Component component, string label)
    {
        Type currentType = component.GetType();
        int hierarchyDepth = 0;
        int logged = 0;

        while (currentType != null && hierarchyDepth < 4 && logged < 16)
        {
            FieldInfo[] fields = currentType.GetFields(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.DeclaredOnly);

            for (int i = 0; i < fields.Length && logged < 16; i++)
            {
                FieldInfo field = fields[i];

                if (field.FieldType != typeof(string))
                {
                    continue;
                }

                try
                {
                    object value = field.GetValue(component);
                    if (value is string text && !string.IsNullOrEmpty(text))
                    {
                        WriteDebugLog(label + "   field=" + field.Name + " value=[" + text + "]");
                        logged++;
                    }
                }
                catch (Exception)
                {
                }
            }

            currentType = currentType.BaseType;
            hierarchyDepth++;
        }
    }

    private static void TraceBismuthHealingSource(Actor source)
    {
        if (source == null || _bismuthHealTraceCount >= 3)
        {
            return;
        }

        Actor current = source;
        bool foundBismuth = false;

        while (current != null)
        {
            if (string.Equals(current.GetType().Name, "Hero_Bismuth", StringComparison.OrdinalIgnoreCase))
            {
                foundBismuth = true;
                break;
            }

            current = current.parentActor;
        }

        if (!foundBismuth)
        {
            return;
        }

        _bismuthHealTraceCount++;
        WriteDebugLog("[" + DevelopmentVersion + "]  Bismuth heal trace " + _bismuthHealTraceCount);
        TraceSourceChain(source, "[" + DevelopmentVersion + "]  bismuth");
    }

    private static void TraceSourceChain(Actor source, string label)
    {
        Actor current = source;
        int depth = 0;

        while (current != null && depth < 8)
        {
            Type type = current.GetType();
            WriteDebugLog(label + " actor[" + depth + "] type=" + type.Name + " name=" + current.name);

            TraceReadableIdentity(current, label);
            TraceStringMembers(current, label);

            try
            {
                string localized;
                if (DewLocalization.TryGetUIValue(type.Name + "_Name", out localized) &&
                    !string.IsNullOrEmpty(localized))
                {
                    WriteDebugLog(label + "   uiKey=" + type.Name + "_Name value=" + localized);
                }
            }
            catch (Exception)
            {
            }

            SkillTrigger skill = current.firstTrigger as SkillTrigger;
            if (skill != null)
            {
                try
                {
                    string skillName = DewLocalization.GetSkillName(skill, 0);
                    if (!string.IsNullOrEmpty(skillName))
                    {
                        WriteDebugLog(label + "   skillName=" + skillName);
                    }
                }
                catch (Exception)
                {
                }
            }

            current = current.parentActor;
            depth++;
        }
    }

    private static void TraceReadableIdentity(Actor actor, string label)
    {
        string[] methodNames = new string[]
        {
            "GetActorReadableName",
            "GetOriginalName"
        };

        for (int i = 0; i < methodNames.Length; i++)
        {
            try
            {
                MethodInfo method = actor.GetType().GetMethod(
                    methodNames[i],
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                if (method == null || method.GetParameters().Length != 0 ||
                    method.ReturnType != typeof(string))
                {
                    continue;
                }

                string value = method.Invoke(actor, null) as string;
                if (!string.IsNullOrEmpty(value))
                {
                    WriteDebugLog(label + "   " + methodNames[i] + "=[" + value + "]");
                }
            }
            catch (Exception)
            {
            }
        }
    }

    private static void TraceStringMembers(Actor actor, string label)
    {
        Type type = actor.GetType();
        Type currentType = type;
        int hierarchyDepth = 0;
        int logged = 0;

        while (currentType != null && hierarchyDepth < 3 && logged < 16)
        {
            FieldInfo[] fields = currentType.GetFields(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.DeclaredOnly);

            for (int i = 0; i < fields.Length && logged < 16; i++)
            {
                FieldInfo field = fields[i];
                if (field.FieldType != typeof(string))
                {
                    continue;
                }

                string fieldName = field.Name;
                if (fieldName.IndexOf("name", StringComparison.OrdinalIgnoreCase) < 0 &&
                    fieldName.IndexOf("key", StringComparison.OrdinalIgnoreCase) < 0 &&
                    fieldName.IndexOf("local", StringComparison.OrdinalIgnoreCase) < 0 &&
                    fieldName.IndexOf("display", StringComparison.OrdinalIgnoreCase) < 0 &&
                    fieldName.IndexOf("text", StringComparison.OrdinalIgnoreCase) < 0 &&
                    fieldName.IndexOf("skill", StringComparison.OrdinalIgnoreCase) < 0 &&
                    fieldName.IndexOf("attack", StringComparison.OrdinalIgnoreCase) < 0)
                {
                    continue;
                }

                try
                {
                    object value = field.GetValue(actor);
                    if (value is string text && !string.IsNullOrEmpty(text))
                    {
                        WriteDebugLog(label + "   field=" + fieldName + " value=[" + text + "]");
                        logged++;
                    }
                }
                catch (Exception)
                {
                }
            }

            currentType = currentType.BaseType;
            hierarchyDepth++;
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

        if (sourceHero.GetType().Name == "Hero_Bismuth" && local.hero == sourceHero)
        {
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
        TraceTargetEssenceScaling(info.actor, directGem);
        TraceTargetMemoryScaling(info.actor, skill);
        TraceTargetChompScaling(info.actor, skill);
        
        
        Dictionary<Gem, float> essenceContributions = new Dictionary<Gem, float>();
        bool isDirectEssenceDamage = directGem != null;
        Actor skillSourceActor = info.actor;

        if (!isDirectEssenceDamage && skill == null)
        {
            skill = FindSkillTriggerInActorChain(info.actor, out skillSourceActor);
        }

        if (isDirectEssenceDamage)
        {
            essenceContributions[directGem] = producedDamage;
        }

        // Essence-generated damage is already represented by its Essence row.
        // Do not also attribute that same hit to the parent Memory/Skill.

        string skillName = null;
        string skillIdentity = null;
        string sourceName = ResolveLocalizedDamageSourceName(info.actor);
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
                skillIdentity = GetSkillSlotIdentity(skillSourceActor, skill);
                string formattedSkillName = skill.GetFormattedSkillTitle();

                if (!string.IsNullOrEmpty(formattedSkillName))
                {
                    skillName = formattedSkillName;
                }
            }

        }

        if (isLocalPlayer && skill != null && !string.IsNullOrEmpty(skillName))
        {
            Sprite icon = FindSkillIcon(skill);
            _data.RegisterSkillIcon(skillIdentity, icon);
        }

        ElementalType? elementalType = info.damage.elemental;

        // Prismatic Vision is a special case: its generated attack actor
        // produces two damage events, one physical/basic-attack portion and
        // one magic/AP portion. The game gives both events the same internal
        // actor, so keep them under the same displayed skill name while using
        // separate internal identities and explicit AD/AP scaling for the UI.
        bool isPrismaticVision = info.actor.GetType().Name == "Ai_D_PrismaticEyes_Attack";
        bool isPrismaticPhysical = isPrismaticVision &&
            string.Equals(info.damage.type.ToString(), "Physical", StringComparison.OrdinalIgnoreCase);
        bool isPrismaticMagic = isPrismaticVision &&
            string.Equals(info.damage.type.ToString(), "Magic", StringComparison.OrdinalIgnoreCase);

        if (isPrismaticPhysical || isPrismaticMagic)
        {
            skillName = "Prismatic Vision";
            skillIdentity = isPrismaticPhysical
                ? "Prismatic Vision|AD"
                : "Prismatic Vision|AP";

            Sprite prismaticIcon = skill != null
                ? FindSkillIcon(skill)
                : FindActorIcon(info.actor);

            if (prismaticIcon != null)
            {
                _data.RegisterSkillIcon("Prismatic Vision|AD", prismaticIcon);
                _data.RegisterSkillIcon("Prismatic Vision|AP", prismaticIcon);
            }
        }
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
            // A damage event without a direct SkillTrigger is not necessarily
            // a basic attack. Resolve its actual runtime scaler instead of
            // assuming AD.
            scalingType = FindDamageScalingType(info.actor);
        }
        else
        {
            scalingType = FindDamageScalingType(info.actor);
        }

        // The Prismatic split is intentionally authoritative for this one
        // generated actor. Do not let the normal skill-scaling resolver
        // collapse the AD/AP portions back into one scaling color.
        if (isPrismaticPhysical)
        {
            scalingType = DpsData.DamageScalingType.Ad;
        }
        else if (isPrismaticMagic)
        {
            scalingType = DpsData.DamageScalingType.Ap;
        }

        // Source-only damage such as Fire has no SkillTrigger, so it must not
        // be hidden behind the non-basic-attack branch above.
        if (isLocalPlayer && skill == null && !string.IsNullOrEmpty(sourceName))
        {
            Sprite otherIcon = string.Equals(sourceName, "Fire", StringComparison.OrdinalIgnoreCase)
                ? FindElmFireIcon(info.actor)
                : FindActorIcon(info.actor);

            _data.RegisterOtherIcon(sourceName, otherIcon);
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
            if (IsTargetBackstepSkill(skill))
            {
                WriteDebugLog("[" + DevelopmentVersion + "] BACKSTEP resolver cache=HIT identity=" +
                    (skillIdentity ?? "<null>") + " value=" + cached);
            }

            return cached;
        }

        bool traceBackstep = IsTargetBackstepSkill(skill);
        if (traceBackstep)
        {
            WriteDebugLog("[" + DevelopmentVersion + "] BACKSTEP resolver cache=MISS identity=" +
                (skillIdentity ?? "<null>"));
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

        if (traceBackstep)
        {
            WriteDebugLog("[" + DevelopmentVersion + "] BACKSTEP resolver sourceGem=" +
                DescribeGem(sourceGem));
        }

        DpsData.DamageScalingType scaling = FindConfiguredGemScaling(sourceGem);

        if (traceBackstep)
        {
            WriteDebugLog("[" + DevelopmentVersion + "] BACKSTEP resolver configuredGem=" + scaling);
        }

        // Character abilities such as Bismuth's Valiant Heart can carry their
        // scaling on the skill's configured AbilityInstance rather than on a
        // Gem. Check that configured instance before falling back to the live
        // damage actor.
        if (scaling == DpsData.DamageScalingType.None &&
            skill != null &&
            skill.currentConfig != null)
        {
            scaling = FindConfiguredAbilityScaling(skill.currentConfig.spawnedInstance, 0);

            if (traceBackstep)
            {
                WriteDebugLog("[" + DevelopmentVersion + "] BACKSTEP resolver configuredAbility=" + scaling);
            }
        }

        if (scaling == DpsData.DamageScalingType.None)
        {
            scaling = FindDamageScalingType(actor);

            if (traceBackstep)
            {
                WriteDebugLog("[" + DevelopmentVersion + "] BACKSTEP resolver runtimeFallback=" + scaling);
            }
        }

        if (scaling != DpsData.DamageScalingType.None)
        {
            _skillScalingCache[skillIdentity] = scaling;

            if (traceBackstep)
            {
                WriteDebugLog("[" + DevelopmentVersion + "] BACKSTEP resolver FINAL=" +
                    scaling + " cached=YES");
            }
        }
        else if (traceBackstep)
        {
            WriteDebugLog("[" + DevelopmentVersion + "] BACKSTEP resolver FINAL=None cached=NO");
        }

        return scaling;
    }

    private static bool IsTargetBackstepSkill(SkillTrigger skill)
    {
        if (skill == null)
        {
            return false;
        }

        try
        {
            if (ContainsTargetMemoryName(skill.GetFormattedSkillTitle()))
            {
                return true;
            }
        }
        catch (Exception)
        {
        }

        try
        {
            return ContainsTargetMemoryName(DewLocalization.GetSkillName(skill, 0));
        }
        catch (Exception)
        {
            return false;
        }
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

    private void TraceTargetEssenceScaling(Actor actor, Gem directGem)
    {
        Gem targetGem = null;
        Actor current = actor;
        int depth = 0;

        while (current != null && depth < 8)
        {
            AbilityInstance instance = current as AbilityInstance;
            if (instance != null && IsTargetEssenceGem(instance.gem))
            {
                targetGem = instance.gem;
                break;
            }

            if (current is Gem currentGem && IsTargetEssenceGem(currentGem))
            {
                targetGem = currentGem;
                break;
            }

            current = current.parentActor;
            depth++;
        }

        if (targetGem == null)
        {
            return;
        }

        string identity = targetGem.GetOriginalName();
        if (string.IsNullOrEmpty(identity))
        {
            identity = targetGem.name;
        }

        if (string.IsNullOrEmpty(identity) || !_essenceScalingDiagnosticSeen.Add(identity))
        {
            return;
        }

        WriteDebugLog("[v5.62] TARGET ESSENCE trace gem=" + identity +
            " localized=" + (GetLocalizedEssenceName(targetGem) ?? "<null>") +
            " directGem=" + DescribeGem(directGem));

        current = actor;
        depth = 0;
        while (current != null && depth < 8)
        {
            AbilityInstance instance = current as AbilityInstance;
            string instanceGem = instance == null ? "<none>" : DescribeGem(instance.gem);
            WriteDebugLog("[v5.62] runtime depth=" + depth +
                " type=" + current.GetType().FullName +
                " name=" + (current.name ?? "<null>") +
                " instanceGem=" + instanceGem);

            LogScalingFields(current, "[v5.62] runtime depth=" + depth);
            current = current.parentActor;
            depth++;
        }

        try
        {
            if (targetGem.skill == null || targetGem.skill.currentConfig == null)
            {
                WriteDebugLog("[v5.62] configured unavailable skillOrConfig=<null>");
                return;
            }

            AbilityInstance configured = targetGem.skill.currentConfig.spawnedInstance;
            if (configured == null)
            {
                WriteDebugLog("[v5.62] configured unavailable spawnedInstance=<null>");
                return;
            }

            WriteDebugLog("[v5.62] configured root type=" + configured.GetType().FullName +
                " name=" + (configured.name ?? "<null>") +
                " gem=" + DescribeGem(configured.gem));
            TraceConfiguredEssenceTree(configured, targetGem, 0);
        }
        catch (Exception ex)
        {
            WriteDebugLog("[v5.62] configured trace exception=" + ex.GetType().Name);
        }
    }

    private static void TraceTargetMemoryScaling(Actor actor, SkillTrigger directSkill)
    {
        if (actor == null)
        {
            return;
        }

        SkillTrigger skill = directSkill;
        Actor skillSourceActor = actor;
        if (skill == null)
        {
            skill = FindSkillTriggerInActorChain(actor, out skillSourceActor);
        }

        if (skill == null)
        {
            return;
        }

        string skillName = null;
        try
        {
            skillName = skill.GetFormattedSkillTitle();
        }
        catch (Exception)
        {
        }

        if (!ContainsTargetMemoryName(skillName))
        {
            try
            {
                string localized = DewLocalization.GetSkillName(skill, 0);
                if (ContainsTargetMemoryName(localized))
                {
                    skillName = localized;
                }
            }
            catch (Exception)
            {
            }
        }

        if (!ContainsTargetMemoryName(skillName))
        {
            return;
        }

        string identity = GetSkillSlotIdentity(skillSourceActor, skill);
        string traceKey = "memory:" + (identity ?? skillName);
        if (!_memoryScalingDiagnosticSeen.Add(traceKey))
        {
            return;
        }

        WriteDebugLog("[" + DevelopmentVersion + "] TARGET MEMORY trace skill=" +
            (skillName ?? "<null>") + " identity=" + (identity ?? "<null>"));

        Actor current = actor;
        int depth = 0;
        while (current != null && depth < 8)
        {
            WriteDebugLog("[" + DevelopmentVersion + "] memory runtime depth=" + depth +
                " type=" + current.GetType().FullName +
                " name=" + (current.name ?? "<null>"));
            LogScalingFields(current, "[" + DevelopmentVersion + "] memory runtime depth=" + depth);
            current = current.parentActor;
            depth++;
        }

        try
        {
            var config = skill.currentConfig;
            if (config == null)
            {
                WriteDebugLog("[" + DevelopmentVersion + "] memory configured unavailable currentConfig=<null>");
                return;
            }

            AbilityInstance configured = config.spawnedInstance;
            WriteDebugLog("[" + DevelopmentVersion + "] memory configured root type=" +
                (configured == null ? "<null>" : configured.GetType().FullName) +
                " name=" + (configured == null ? "<null>" : (configured.name ?? "<null>")));

            if (configured != null)
            {
                TraceConfiguredAbilityTree(configured, 0);
            }
        }
        catch (Exception ex)
        {
            WriteDebugLog("[" + DevelopmentVersion + "] memory configured trace exception=" + ex.GetType().Name);
        }
    }

    private static void TraceConfiguredAbilityTree(AbilityInstance instance, int depth)
    {
        if (instance == null || depth > 8)
        {
            return;
        }

        WriteDebugLog("[" + DevelopmentVersion + "] memory configured depth=" + depth +
            " type=" + instance.GetType().FullName +
            " name=" + (instance.name ?? "<null>") +
            " gem=" + DescribeGem(instance.gem));
        LogScalingFields(instance, "[" + DevelopmentVersion + "] memory configured depth=" + depth);

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
                TraceConfiguredAbilityTree(child, depth + 1);
            }
        }
    }

    private static bool ContainsTargetChompName(string value)
    {
        return !string.IsNullOrEmpty(value) &&
            value.IndexOf("Chomp", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private static bool IsTargetChompSkill(SkillTrigger skill)
    {
        if (skill == null)
        {
            return false;
        }

        try
        {
            if (ContainsTargetChompName(skill.GetFormattedSkillTitle()))
            {
                return true;
            }
        }
        catch (Exception)
        {
        }

        try
        {
            return ContainsTargetChompName(DewLocalization.GetSkillName(skill, 0));
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static bool IsTargetChompActor(Actor actor)
    {
        if (actor == null)
        {
            return false;
        }

        Actor current = actor;
        int depth = 0;

        while (current != null && depth < 8)
        {
            if (current.GetType().Name.IndexOf("Chomp", StringComparison.OrdinalIgnoreCase) >= 0 ||
                (!string.IsNullOrEmpty(current.name) &&
                 current.name.IndexOf("Chomp", StringComparison.OrdinalIgnoreCase) >= 0))
            {
                return true;
            }

            current = current.parentActor;
            depth++;
        }

        return false;
    }


    private static Actor GetPrismaticPendingAttack(Actor effect)
    {
        if (effect == null)
        {
            return null;
        }

        FieldInfo pendingField = effect.GetType().GetField(
            "_pendingAttack",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        if (pendingField == null)
        {
            return null;
        }

        try
        {
            return pendingField.GetValue(effect) as Actor;
        }
        catch (Exception)
        {
            return null;
        }
    }


    private static Sprite FindElmFireIcon(Actor actor)
    {
        Actor current = actor;
        int depth = 0;

        while (current != null && depth < 8)
        {
            if (string.Equals(current.GetType().Name, "Se_Elm_Fire", StringComparison.Ordinal))
            {
                return FindSpriteMember(current);
            }

            current = current.parentActor;
            depth++;
        }

        return null;
    }


    private static int GetPrismaticIlOperandSize(OperandType operandType)
    {
        switch (operandType)
        {
            case OperandType.InlineNone:
                return 0;
            case OperandType.ShortInlineI:
            case OperandType.ShortInlineR:
            case OperandType.ShortInlineBrTarget:
            case OperandType.ShortInlineVar:
                return 1;
            case OperandType.InlineVar:
                return 2;
            case OperandType.InlineI:
            case OperandType.InlineBrTarget:
            case OperandType.InlineField:
            case OperandType.InlineMethod:
            case OperandType.InlineSig:
            case OperandType.InlineString:
            case OperandType.InlineTok:
            case OperandType.InlineType:
                return 4;
            case OperandType.InlineI8:
            case OperandType.InlineR:
                return 8;
            case OperandType.InlineSwitch:
                return 4;
        }

        return -1;
    }

    private static readonly OpCode[] OneByteOpCodes = BuildOneByteOpCodes();
    private static readonly OpCode[] TwoByteOpCodes = BuildTwoByteOpCodes();

    private static OpCode[] BuildOneByteOpCodes()
    {
        OpCode[] result = new OpCode[256];
        FieldInfo[] fields = typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static);

        for (int i = 0; i < fields.Length; i++)
        {
            if (fields[i].GetValue(null) is OpCode opcode && opcode.Size == 1)
                result[opcode.Value & 0xFF] = opcode;
        }

        return result;
    }

    private static OpCode[] BuildTwoByteOpCodes()
    {
        OpCode[] result = new OpCode[256];
        FieldInfo[] fields = typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static);

        for (int i = 0; i < fields.Length; i++)
        {
            if (fields[i].GetValue(null) is OpCode opcode && opcode.Size == 2)
                result[opcode.Value & 0xFF] = opcode;
        }

        return result;
    }

    private static void TryPrismaticLocalizationLookup(object source, Type type)
    {
        if (source == null || type == null)
        {
            return;
        }

        try
        {
            string[] candidates = new string[]
            {
                type.Name,
                source is Actor ? ((Actor)source).GetOriginalName() : null,
                "63"
            };

            MethodInfo getSkillKeyString = typeof(DewLocalization).GetMethod(
                "GetSkillKey", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                null, new Type[] { typeof(string) }, null);
            MethodInfo getSkillNameString = typeof(DewLocalization).GetMethod(
                "GetSkillName", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                null, new Type[] { typeof(string), typeof(int) }, null);
            MethodInfo getSkillMemoryString = typeof(DewLocalization).GetMethod(
                "GetSkillMemory", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                null, new Type[] { typeof(string) }, null);

            MethodInfo getSkillNameKeyString = typeof(DewLocalization).GetMethod(
                "GetSkillNameKey", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                null, new Type[] { typeof(SkillTrigger), typeof(int) }, null);

            for (int i = 0; i < candidates.Length; i++)
            {
                string candidate = candidates[i];
                if (string.IsNullOrEmpty(candidate))
                {
                    continue;
                }

                string key = null;
                if (getSkillKeyString != null)
                {
                    try { key = getSkillKeyString.Invoke(null, new object[] { candidate }) as string; }
                    catch (Exception) { }
                }

                string name = null;
                if (getSkillNameString != null)
                {
                    try { name = getSkillNameString.Invoke(null, new object[] { candidate, 0 }) as string; }
                    catch (Exception) { }
                }

                string memory = null;
                if (getSkillMemoryString != null)
                {
                    try { memory = getSkillMemoryString.Invoke(null, new object[] { candidate }) as string; }
                    catch (Exception) { }
                }

                WriteDebugLog("[" + DevelopmentVersion + "] PRISMATIC NAME lookup candidate=[" + candidate +
                    "] skillKey=[" + (key ?? "<null>") + "] skillName=[" + (name ?? "<null>") +
                    "] skillMemory=[" + (memory ?? "<null>") + "]");

                if (!string.IsNullOrEmpty(key) && !string.Equals(key, candidate, StringComparison.Ordinal))
                {
                    string keyedName = null;
                    try { keyedName = getSkillNameString.Invoke(null, new object[] { key, 0 }) as string; }
                    catch (Exception) { }

                    string keyedMemory = null;
                    if (getSkillMemoryString != null)
                    {
                        try { keyedMemory = getSkillMemoryString.Invoke(null, new object[] { key }) as string; }
                        catch (Exception) { }
                    }

                    WriteDebugLog("[" + DevelopmentVersion + "] PRISMATIC NAME lookup key=[" + key +
                        "] skillName=[" + (keyedName ?? "<null>") + "] skillMemory=[" +
                        (keyedMemory ?? "<null>") + "]");
                }
            }

            MethodInfo getSkillKeyType = typeof(DewLocalization).GetMethod(
                "GetSkillKey", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                null, new Type[] { typeof(Type) }, null);
            if (getSkillKeyType != null)
            {
                string key = null;
                try { key = getSkillKeyType.Invoke(null, new object[] { type }) as string; }
                catch (Exception) { }
                WriteDebugLog("[" + DevelopmentVersion + "] PRISMATIC NAME lookup type=[" + type.FullName +
                    "] skillKey=[" + (key ?? "<null>") + "]");
            }
        }
        catch (Exception ex)
        {
            WriteDebugLog("[" + DevelopmentVersion + "] PRISMATIC NAME lookup error=" + ex.GetType().Name);
        }
    }

    private static void TraceTargetChompScaling(Actor actor, SkillTrigger directSkill)
    {
        if (actor == null)
        {
            return;
        }

        SkillTrigger skill = directSkill;
        Actor skillSourceActor = actor;
        if (skill == null)
        {
            skill = FindSkillTriggerInActorChain(actor, out skillSourceActor);
        }

        if (skill == null || !IsTargetChompSkill(skill))
        {
            return;
        }

        string skillName = null;
        try
        {
            skillName = skill.GetFormattedSkillTitle();
        }
        catch (Exception)
        {
        }

        if (!ContainsTargetChompName(skillName))
        {
            try
            {
                string localized = DewLocalization.GetSkillName(skill, 0);
                if (ContainsTargetChompName(localized))
                {
                    skillName = localized;
                }
            }
            catch (Exception)
            {
            }
        }

        string identity = GetSkillSlotIdentity(skillSourceActor, skill);
        string traceKey = "memory:chomp:" + (identity ?? skillName);
        if (!_chompScalingDiagnosticSeen.Add(traceKey))
        {
            return;
        }

        WriteDebugLog("[" + DevelopmentVersion + "] TARGET MEMORY CHOMP trace skill=" +
            (skillName ?? "<null>") + " identity=" + (identity ?? "<null>"));

        Actor current = actor;
        int depth = 0;
        while (current != null && depth < 8)
        {
            WriteDebugLog("[" + DevelopmentVersion + "] CHOMP runtime depth=" + depth +
                " type=" + current.GetType().FullName +
                " name=" + (current.name ?? "<null>"));
            LogScalingFields(current, "[" + DevelopmentVersion + "] CHOMP runtime depth=" + depth);
            current = current.parentActor;
            depth++;
        }

        try
        {
            var config = skill.currentConfig;
            if (config == null)
            {
                WriteDebugLog("[" + DevelopmentVersion + "] CHOMP configured unavailable currentConfig=<null>");
                return;
            }

            AbilityInstance configured = config.spawnedInstance;
            WriteDebugLog("[" + DevelopmentVersion + "] CHOMP configured root type=" +
                (configured == null ? "<null>" : configured.GetType().FullName) +
                " name=" + (configured == null ? "<null>" : (configured.name ?? "<null>")));

            if (configured != null)
            {
                TraceConfiguredChompAbilityTree(configured, 0);
            }
        }
        catch (Exception ex)
        {
            WriteDebugLog("[" + DevelopmentVersion + "] CHOMP configured trace exception=" + ex.GetType().Name);
        }
    }

    private static void TraceConfiguredChompAbilityTree(AbilityInstance instance, int depth)
    {
        if (instance == null || depth > 8)
        {
            return;
        }

        WriteDebugLog("[" + DevelopmentVersion + "] CHOMP configured depth=" + depth +
            " type=" + instance.GetType().FullName +
            " name=" + (instance.name ?? "<null>") +
            " gem=" + DescribeGem(instance.gem));
        LogScalingFields(instance, "[" + DevelopmentVersion + "] CHOMP configured depth=" + depth);

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
                TraceConfiguredChompAbilityTree(child, depth + 1);
            }
        }
    }

    private static bool ContainsTargetMemoryName(string value)
    {
        return !string.IsNullOrEmpty(value) &&
            value.IndexOf("Backstep", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private static bool IsTargetEssenceGem(Gem gem)
    {
        if (gem == null)
        {
            return false;
        }

        string original = gem.GetOriginalName();
        string name = gem.name;
        string localized = GetLocalizedEssenceName(gem);

        return ContainsTargetEssenceName(original) ||
            ContainsTargetEssenceName(name) ||
            ContainsTargetEssenceName(localized);
    }

    private static bool ContainsTargetEssenceName(string value)
    {
        return !string.IsNullOrEmpty(value) &&
            (value.IndexOf("Backstep", StringComparison.OrdinalIgnoreCase) >= 0 ||
             value.IndexOf("Sharpness", StringComparison.OrdinalIgnoreCase) >= 0);
    }

    private static string DescribeGem(Gem gem)
    {
        if (gem == null)
        {
            return "<null>";
        }

        return "original=" + (gem.GetOriginalName() ?? "<null>") +
            " name=" + (gem.name ?? "<null>") +
            " localized=" + (GetLocalizedEssenceName(gem) ?? "<null>");
    }

    private static void LogScalingFields(Actor actor, string label)
    {
        if (actor == null)
        {
            return;
        }

        FieldInfo[] fields = actor.GetType().GetFields(
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        for (int i = 0; i < fields.Length; i++)
        {
            FieldInfo field = fields[i];
            if (field.FieldType != typeof(ScalingValue))
            {
                continue;
            }

            try
            {
                ScalingValue value = (ScalingValue)field.GetValue(actor);
                WriteDebugLog(label + " scalingField=" + field.Name +
                    " ad=" + value.adFactor.ToString("0.######") +
                    " ap=" + value.apFactor.ToString("0.######") +
                    " hp=" + value.addedHpFactor.ToString("0.######") +
                    " resolved=" + GetScalingType(value));
            }
            catch (Exception ex)
            {
                WriteDebugLog(label + " scalingField=" + field.Name +
                    " readException=" + ex.GetType().Name);
            }
        }
    }

    private static void TraceConfiguredEssenceTree(
        AbilityInstance instance,
        Gem targetGem,
        int depth)
    {
        if (instance == null || depth > 8)
        {
            return;
        }

        WriteDebugLog("[v5.62] configured depth=" + depth +
            " type=" + instance.GetType().FullName +
            " name=" + (instance.name ?? "<null>") +
            " gem=" + DescribeGem(instance.gem) +
            " gemMatches=" + (instance.gem == targetGem));

        LogScalingFields(instance, "[v5.62] configured depth=" + depth);

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
                TraceConfiguredEssenceTree(child, targetGem, depth + 1);
            }
        }
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


    private static string ResolveLocalizedDamageSourceName(Actor source)
    {
        if (source == null)
        {
            return null;
        }

        Actor current = source;
        int depth = 0;

        while (current != null && depth < 8)
        {
            string key = current.name;
            if (!string.IsNullOrEmpty(key))
            {
                int suffix = key.IndexOf('(');
                if (suffix > 0)
                {
                    key = key.Substring(0, suffix).Trim();
                }

                if (!string.IsNullOrEmpty(key))
                {
                    try
                    {
                        string localizedName;
                        if (DewLocalization.TryGetUIValue(key + "_Name", out localizedName) &&
                            !string.IsNullOrEmpty(localizedName))
                        {
                            return localizedName;
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }

            string typeName = current.GetType().Name;
            if (!string.IsNullOrEmpty(typeName))
            {
                try
                {
                    string localizedName;
                    if (DewLocalization.TryGetUIValue(typeName + "_Name", out localizedName) &&
                        !string.IsNullOrEmpty(localizedName))
                    {
                        return localizedName;
                    }
                }
                catch (Exception)
                {
                }
            }

            current = current.parentActor;
            depth++;
        }

        return null;
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

        List<Actor> children = instance.children;        if (children == null)
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

            // Resolve the configured AbilityInstance that is actually owned by
            // this Essence. This is the critical distinction from the host
            // skill's root scaler: a socketed AP Essence must not inherit an
            // unrelated AD scaler from its host Memory.
            DpsData.DamageScalingType essenceScaling =
                FindConfiguredGemAbilityScaling(configured, gem, 0);

            if (essenceScaling != DpsData.DamageScalingType.None)
            {
                return essenceScaling;
            }

            // Never fall back to the host skill's scaler here. If the Essence
            // has no identifiable configured scaler, leave it unresolved
            // rather than inventing one.
            return DpsData.DamageScalingType.None;
        }
        catch (Exception)
        {
            return DpsData.DamageScalingType.None;
        }
    }

    private static DpsData.DamageScalingType FindRuntimeDamageScaling(
        Actor actor,
        string preferredFieldName)
    {
        return FindRuntimeDamageScaling(actor, preferredFieldName, false);
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

    private static DpsData.DamageScalingType FindDamageScalingType(Actor actor)
    {
        bool traceBackstep = IsTargetBackstepActor(actor);

        if (traceBackstep)
        {
            WriteDebugLog("[" + DevelopmentVersion + "] BACKSTEP scaling actor=" +
                DescribeActor(actor));
        }

        DamageInstance damageInstance = FindDamageInstance(actor);

        if (traceBackstep)
        {
            WriteDebugLog("[" + DevelopmentVersion + "] BACKSTEP scaling damageInstance=" +
                DescribeActor(damageInstance));
        }

        if (damageInstance != null)
        {
            DpsData.DamageScalingType scaling = GetScalingType(damageInstance.dmgFactor);

            if (traceBackstep)
            {
                WriteDebugLog("[" + DevelopmentVersion + "] BACKSTEP scaling damageInstance.dmgFactor=" +
                    DescribeScaling(damageInstance.dmgFactor) + " result=" + scaling);
            }

            if (scaling != DpsData.DamageScalingType.None)
            {
                return scaling;
            }
        }

        DpsData.DamageScalingType runtimeScaling = FindRuntimeDamageScaling(actor, "damage", traceBackstep);

        if (runtimeScaling != DpsData.DamageScalingType.None)
        {
            if (traceBackstep)
            {
                WriteDebugLog("[" + DevelopmentVersion + "] BACKSTEP scaling stage=damage result=" +
                    runtimeScaling);
            }

            return runtimeScaling;
        }

        runtimeScaling = FindRuntimeDamageScaling(actor, "normalDamage", traceBackstep);

        if (runtimeScaling != DpsData.DamageScalingType.None)
        {
            if (traceBackstep)
            {
                WriteDebugLog("[" + DevelopmentVersion + "] BACKSTEP scaling stage=normalDamage result=" +
                    runtimeScaling);
            }

            return runtimeScaling;
        }

        runtimeScaling = FindRuntimeDamageScaling(actor, null, traceBackstep);

        if (traceBackstep)
        {
            WriteDebugLog("[" + DevelopmentVersion + "] BACKSTEP scaling stage=generic result=" +
                runtimeScaling);
        }

        return runtimeScaling;
    }

    private static DpsData.DamageScalingType FindRuntimeDamageScaling(
        Actor actor,
        string preferredFieldName,
        bool traceBackstep)
    {
        Actor current = actor;
        int depth = 0;

        while (current != null && depth < 8)
        {
            if (traceBackstep)
            {
                WriteDebugLog("[" + DevelopmentVersion + "] BACKSTEP runtime depth=" + depth +
                    " actor=" + DescribeActor(current));
            }

            FieldInfo[] fields = current.GetType().GetFields(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            for (int i = 0; i < fields.Length; i++)
            {
                FieldInfo field = fields[i];

                if (traceBackstep)
                {
                    WriteDebugLog("[" + DevelopmentVersion + "] BACKSTEP runtime depth=" + depth +
                        " field=" + field.Name + " type=" + field.FieldType.FullName);
                }

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
                    field.Name.IndexOf("damage", StringComparison.OrdinalIgnoreCase) < 0 &&
                    !string.Equals(field.Name, "dmgFactor", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                try
                {
                    ScalingValue scaling = (ScalingValue)field.GetValue(current);
                    DpsData.DamageScalingType result = GetScalingType(scaling);

                    if (traceBackstep)
                    {
                        WriteDebugLog("[" + DevelopmentVersion + "] BACKSTEP runtime depth=" + depth +
                            " scalingField=" + field.Name + " " + DescribeScaling(scaling) +
                            " result=" + result);
                    }

                    if (result != DpsData.DamageScalingType.None)
                    {
                        return result;
                    }
                }
                catch (Exception ex)
                {
                    if (traceBackstep)
                    {
                        WriteDebugLog("[" + DevelopmentVersion + "] BACKSTEP runtime depth=" + depth +
                            " field=" + field.Name + " readError=" + ex.GetType().Name);
                    }
                }
            }

            current = current.parentActor;
            depth++;
        }

        return DpsData.DamageScalingType.None;
    }

    private static bool IsTargetBackstepActor(Actor actor)
    {
        if (actor == null)
        {
            return false;
        }

        try
        {
            Actor current = actor;
            int depth = 0;

            while (current != null && depth < 8)
            {
                if (current.GetType().Name.IndexOf("BackStep", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (!string.IsNullOrEmpty(current.name) &&
                     current.name.IndexOf("BackStep", StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    return true;
                }

                current = current.parentActor;
                depth++;
            }
        }
        catch (Exception)
        {
        }

        return false;
    }

    private static string DescribeEntity(Entity entity)
    {
        if (entity == null)
        {
            return "<null>";
        }

        return "type=" + entity.GetType().Name + " name=" + (entity.name ?? "<null>");
    }

    private static string DescribeActor(Actor actor)
    {
        if (actor == null)
        {
            return "<null>";
        }

        return "type=" + actor.GetType().Name + " name=" + (actor.name ?? "<null>");
    }

    private static string DescribeScaling(ScalingValue scaling)
    {
        return "ad=" + scaling.adFactor + " ap=" + scaling.apFactor +
            " hp=" + scaling.addedHpFactor;
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