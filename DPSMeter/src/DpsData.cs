using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DPSMeter;

public sealed class DpsData
{
    public enum DamageScalingType
    {
        None,
        Ad,
        Ap,
        Hp
    }
    private readonly Dictionary<string, float> _currentPersonalSkills = new Dictionary<string, float>();
    private readonly Dictionary<string, Sprite> _skillIcons = new Dictionary<string, Sprite>();
    private readonly Dictionary<string, float> _cumulativePersonalSkills = new Dictionary<string, float>();
    private readonly Dictionary<string, float> _currentPersonalEssences = new Dictionary<string, float>();
    private readonly Dictionary<string, Dictionary<DamageScalingType, float>> _currentPersonalEssenceScaling = new Dictionary<string, Dictionary<DamageScalingType, float>>();
    private readonly Dictionary<string, Sprite> _currentPersonalEssenceIcons = new Dictionary<string, Sprite>();
    private readonly Dictionary<string, float> _cumulativePersonalEssences = new Dictionary<string, float>();
    private readonly Dictionary<string, Dictionary<DamageScalingType, float>> _cumulativePersonalEssenceScaling = new Dictionary<string, Dictionary<DamageScalingType, float>>();
    private readonly Dictionary<string, Sprite> _cumulativePersonalEssenceIcons = new Dictionary<string, Sprite>();
    private readonly Dictionary<string, float> _currentOtherPersonal = new Dictionary<string, float>();
    private readonly Dictionary<string, Dictionary<DamageScalingType, float>> _currentPersonalSkillScaling = new Dictionary<string, Dictionary<DamageScalingType, float>>();
    private readonly Dictionary<string, Dictionary<DamageScalingType, float>> _currentPersonalOtherScaling = new Dictionary<string, Dictionary<DamageScalingType, float>>();
    private readonly Dictionary<string, Dictionary<ElementalType, float>> _currentPersonalSkillElements = new Dictionary<string, Dictionary<ElementalType, float>>();
    private readonly Dictionary<string, Dictionary<ElementalType, float>> _cumulativePersonalSkillElements = new Dictionary<string, Dictionary<ElementalType, float>>();
    private readonly Dictionary<string, Dictionary<ElementalType, float>> _currentPersonalEssenceElements = new Dictionary<string, Dictionary<ElementalType, float>>();
    private readonly Dictionary<string, Dictionary<ElementalType, float>> _cumulativePersonalEssenceElements = new Dictionary<string, Dictionary<ElementalType, float>>();
    private readonly Dictionary<string, float> _cumulativeOtherPersonal = new Dictionary<string, float>();
    private readonly Dictionary<string, float> _currentPersonalHealing = new Dictionary<string, float>();
    private readonly Dictionary<string, float> _cumulativePersonalHealing = new Dictionary<string, float>();
    private readonly Dictionary<string, float> _currentParty = new Dictionary<string, float>();
    private readonly Dictionary<string, float> _cumulativeParty = new Dictionary<string, float>();
    private bool _pendingInstanceReset;

    public float CurrentInstancePersonalHealing { get; private set; }
    public float CumulativePersonalHealing { get; private set; }

    public float HealingStartedAt { get; private set; }
    public float LastHealAt { get; private set; }
    public int CurrentHealCount { get; private set; }
    public float CumulativeHealingStartedAt { get; private set; }
    public float LastCumulativeHealAt { get; private set; }

    public float CurrentHealingDuration =>
        CurrentHealCount == 0 ? 0f : Mathf.Max(0.001f, LastHealAt - HealingStartedAt);

    public float CurrentPersonalHps =>
        CurrentHealCount == 0 ? 0f : CurrentInstancePersonalHealing / CurrentHealingDuration;

    public float TotalPersonalHps =>
        CumulativePersonalHealing <= 0f || CumulativeHealingStartedAt <= 0f
            ? 0f
            : CumulativePersonalHealing / Mathf.Max(0.001f, LastCumulativeHealAt - CumulativeHealingStartedAt);

    public float CurrentInstancePersonalDamage { get; private set; }
    public float CumulativePersonalDamage { get; private set; }
    public float CurrentInstancePersonalAppliedDamage { get; private set; }
    public float CumulativePersonalAppliedDamage { get; private set; }
    public float CurrentInstancePersonalOverkill { get; private set; }
    public float CumulativePersonalOverkill { get; private set; }

    public float CurrentInstancePartyDamage { get; private set; }
    public float CumulativePartyDamage { get; private set; }
    public float CurrentInstancePartyAppliedDamage { get; private set; }
    public float CumulativePartyAppliedDamage { get; private set; }
    public float CurrentInstancePartyOverkill { get; private set; }
    public float CumulativePartyOverkill { get; private set; }

    public float StartedAt { get; private set; }
    public float LastHitAt { get; private set; }
    public int CurrentHitCount { get; private set; }

    public float CurrentDuration =>
        CurrentHitCount == 0 ? 0f : Mathf.Max(0.001f, LastHitAt - StartedAt);

    public float CurrentPersonalDps =>
        CurrentHitCount == 0 ? 0f : CurrentInstancePersonalDamage / CurrentDuration;

    public float CurrentPartyDps =>
        CurrentHitCount == 0 ? 0f : CurrentInstancePartyDamage / CurrentDuration;

    public float CurrentPersonalAppliedDps =>
        CurrentHitCount == 0 ? 0f : CurrentInstancePersonalAppliedDamage / CurrentDuration;

    public float CurrentPersonalOverkill => CurrentInstancePersonalOverkill;

    public float CurrentPartyAppliedDps =>
        CurrentHitCount == 0 ? 0f : CurrentInstancePartyAppliedDamage / CurrentDuration;

    public IReadOnlyList<KeyValuePair<string, float>> CurrentPersonalHealing =>
        _currentPersonalHealing.OrderByDescending(pair => pair.Value).ToList();

    public IReadOnlyList<KeyValuePair<string, float>> CumulativeHealingSources =>
        _cumulativePersonalHealing.OrderByDescending(pair => pair.Value).ToList();

    public IReadOnlyList<KeyValuePair<string, float>> CurrentPersonalSkills =>
        _currentPersonalSkills.OrderByDescending(pair => pair.Value).ToList();

    public IReadOnlyList<KeyValuePair<string, float>> CumulativePersonalSkills =>
        _cumulativePersonalSkills.OrderByDescending(pair => pair.Value).ToList();

    public IReadOnlyList<KeyValuePair<string, float>> CurrentPersonalEssences =>
        _currentPersonalEssences.OrderByDescending(pair => pair.Value).ToList();

    public IReadOnlyList<KeyValuePair<string, float>> CumulativePersonalEssences =>
        _cumulativePersonalEssences.OrderByDescending(pair => pair.Value).ToList();

    public IReadOnlyList<KeyValuePair<string, float>> CurrentPersonalOther =>
        _currentOtherPersonal.OrderByDescending(pair => pair.Value).ToList();

    public IReadOnlyList<KeyValuePair<string, float>> CumulativePersonalOther =>
        _cumulativeOtherPersonal.OrderByDescending(pair => pair.Value).ToList();

    public IReadOnlyList<KeyValuePair<string, float>> CurrentParty =>
        _currentParty.OrderByDescending(pair => pair.Value).ToList();

    public IReadOnlyList<KeyValuePair<string, float>> CumulativeParty =>
        _cumulativeParty.OrderByDescending(pair => pair.Value).ToList();

    public void AddHealing(float healing, string sourceName)
    {
        if (healing <= 0f)
        {
            return;
        }

        if (_pendingInstanceReset)
        {
            ClearCurrentInstance();
            _pendingInstanceReset = false;
        }

        float now = Time.time;

        if (CurrentHealCount == 0)
        {
            HealingStartedAt = now;
        }

        if (CumulativeHealingStartedAt <= 0f)
        {
            CumulativeHealingStartedAt = now;
        }

        LastHealAt = now;
        LastCumulativeHealAt = now;
        CurrentHealCount++;

        CurrentInstancePersonalHealing += healing;
        CumulativePersonalHealing += healing;

        Add(_currentPersonalHealing, sourceName, healing);
        Add(_cumulativePersonalHealing, sourceName, healing);
    }

    public void AddDamage(
        float producedDamage,
        float appliedDamage,
        bool isLocalPlayer,
        string skillName,
        string sourceName,
        IReadOnlyDictionary<Gem, float> essenceContributions,
        ElementalType? elemental,
        string playerName,
        bool isDirectEssenceDamage,
        DamageScalingType scalingType)
    {
        if (producedDamage <= 0f)
        {
            return;
        }

        if (_pendingInstanceReset)
        {
            ClearCurrentInstance();
            _pendingInstanceReset = false;
        }

        float overkill = Mathf.Max(0f, producedDamage - appliedDamage);
        float now = Time.time;

        if (CurrentHitCount == 0)
        {
            StartedAt = now;
        }

        LastHitAt = now;
        CurrentHitCount++;

        CurrentInstancePartyDamage += producedDamage;
        CumulativePartyDamage += producedDamage;
        CurrentInstancePartyAppliedDamage += appliedDamage;
        CumulativePartyAppliedDamage += appliedDamage;
        CurrentInstancePartyOverkill += overkill;
        CumulativePartyOverkill += overkill;

        Add(_currentParty, playerName, producedDamage);
        Add(_cumulativeParty, playerName, producedDamage);

        if (!isLocalPlayer)
        {
            return;
        }

        CurrentInstancePersonalDamage += producedDamage;
        CumulativePersonalDamage += producedDamage;
        CurrentInstancePersonalAppliedDamage += appliedDamage;
        CumulativePersonalAppliedDamage += appliedDamage;
        CurrentInstancePersonalOverkill += overkill;
        CumulativePersonalOverkill += overkill;

        if (!string.IsNullOrEmpty(skillName))
        {
            Add(_currentPersonalSkills, skillName, producedDamage);
            Add(_cumulativePersonalSkills, skillName, producedDamage);
            AddElement(_currentPersonalSkillElements, skillName, elemental, producedDamage);
            AddScaling(_currentPersonalSkillScaling, skillName, scalingType, producedDamage);
            AddElement(_cumulativePersonalSkillElements, skillName, elemental, producedDamage);
        }
        else if (!isDirectEssenceDamage)
        {
            Add(_currentOtherPersonal, sourceName, producedDamage);
            AddScaling(_currentPersonalOtherScaling, sourceName, scalingType, producedDamage);
            Add(_cumulativeOtherPersonal, sourceName, producedDamage);
        }

        if (essenceContributions != null)
        {
            foreach (KeyValuePair<Gem, float> pair in essenceContributions)
            {
                Gem essence = pair.Key;
                float contribution = pair.Value;

                if (essence == null || contribution <= 0f)
                    continue;

                string essenceKey = GetEssenceKey(essence);
                if (string.IsNullOrEmpty(essenceKey))
                    continue;

                Add(_currentPersonalEssences, essenceKey, contribution);
                Add(_cumulativePersonalEssences, essenceKey, contribution);
                AddElement(_currentPersonalEssenceElements, essenceKey, elemental, contribution);
                AddScaling(_currentPersonalEssenceScaling, essenceKey, scalingType, contribution);
                AddScaling(_cumulativePersonalEssenceScaling, essenceKey, scalingType, contribution);
                AddElement(_cumulativePersonalEssenceElements, essenceKey, elemental, contribution);

                if (essence.icon != null)
                {
                    _currentPersonalEssenceIcons[essenceKey] = essence.icon;
                    _cumulativePersonalEssenceIcons[essenceKey] = essence.icon;
                }
            }
        }
    }

    public void ResetCurrentInstance()
    {
        _pendingInstanceReset = true;
    }

    private void ClearCurrentInstance()
    {
        CurrentInstancePersonalHealing = 0f;
        HealingStartedAt = 0f;
        LastHealAt = 0f;
        CurrentHealCount = 0;

        CurrentInstancePersonalDamage = 0f;
        CurrentInstancePersonalAppliedDamage = 0f;
        CurrentInstancePersonalOverkill = 0f;
        CurrentInstancePartyDamage = 0f;
        CurrentInstancePartyAppliedDamage = 0f;
        CurrentInstancePartyOverkill = 0f;
        StartedAt = 0f;
        LastHitAt = 0f;
        CurrentHitCount = 0;

        _currentPersonalHealing.Clear();
        _currentPersonalSkills.Clear();
        _currentPersonalEssences.Clear();
        _currentPersonalSkillElements.Clear();
        _currentPersonalSkillScaling.Clear();
        _currentPersonalOtherScaling.Clear();
        _currentPersonalEssenceScaling.Clear();
        _currentPersonalEssenceIcons.Clear();
        _currentPersonalEssenceElements.Clear();
        _currentOtherPersonal.Clear();
        _currentParty.Clear();
    }

    public void Reset()
    {
        ClearCurrentInstance();
        _pendingInstanceReset = false;

        CumulativePersonalHealing = 0f;
        CumulativeHealingStartedAt = 0f;
        LastCumulativeHealAt = 0f;
        CumulativePersonalDamage = 0f;
        CumulativePersonalAppliedDamage = 0f;
        CumulativePersonalOverkill = 0f;
        CumulativePartyDamage = 0f;
        CumulativePartyAppliedDamage = 0f;
        CumulativePartyOverkill = 0f;

        _cumulativePersonalHealing.Clear();
        _cumulativePersonalSkills.Clear();
        _skillIcons.Clear();
        _cumulativePersonalEssences.Clear();
        _cumulativePersonalEssenceScaling.Clear();
        _cumulativePersonalEssenceIcons.Clear();
        _cumulativePersonalSkillElements.Clear();
        _cumulativePersonalEssenceElements.Clear();
        _cumulativeOtherPersonal.Clear();
        _cumulativeParty.Clear();
    }

    private static void Add(Dictionary<string, float> map, string key, float amount)
    {
        if (string.IsNullOrEmpty(key))
        {
            key = "Unknown";
        }

        float current;
        map.TryGetValue(key, out current);
        map[key] = current + amount;
    }

    public ElementalType? GetCurrentSkillElement(string skillName) => GetDominantElement(_currentPersonalSkillElements, skillName);

    public DamageScalingType GetCurrentSkillScaling(string skillName) => GetDominantScaling(_currentPersonalSkillScaling, skillName);

    public DamageScalingType GetCurrentOtherScaling(string sourceName) => GetDominantScaling(_currentPersonalOtherScaling, sourceName);

    public DamageScalingType GetCurrentEssenceScaling(string essenceKey) => GetDominantScaling(_currentPersonalEssenceScaling, essenceKey);

    public DamageScalingType GetCumulativeEssenceScaling(string essenceKey) => GetDominantScaling(_cumulativePersonalEssenceScaling, essenceKey);

    public Sprite GetCurrentEssenceIcon(string essenceKey)
    {
        Sprite icon;
        return !string.IsNullOrEmpty(essenceKey) && _currentPersonalEssenceIcons.TryGetValue(essenceKey, out icon) ? icon : null;
    }

    public Sprite GetCumulativeEssenceIcon(string essenceKey)
    {
        Sprite icon;
        return !string.IsNullOrEmpty(essenceKey) && _cumulativePersonalEssenceIcons.TryGetValue(essenceKey, out icon) ? icon : null;
    }

    public Sprite GetSkillIcon(string skillName)
    {
        Sprite icon;
        return !string.IsNullOrEmpty(skillName) && _skillIcons.TryGetValue(skillName, out icon) ? icon : null;
    }

    public void RegisterSkillIcon(string skillName, Sprite icon)
    {
        if (string.IsNullOrEmpty(skillName) || icon == null)
            return;

        _skillIcons[skillName] = icon;
    }

    public ElementalType? GetCurrentEssenceElement(string essenceKey) => GetDominantElement(_currentPersonalEssenceElements, essenceKey);

    public ElementalType? GetCumulativeEssenceElement(string essenceKey) => GetDominantElement(_cumulativePersonalEssenceElements, essenceKey);

    private static ElementalType? GetDominantElement<T>(Dictionary<T, Dictionary<ElementalType, float>> map, T key) where T : class
    {
        Dictionary<ElementalType, float> elements;
        if (key == null || !map.TryGetValue(key, out elements) || elements.Count == 0)
            return null;

        ElementalType dominant = default(ElementalType);
        float amount = 0f;
        bool found = false;
        foreach (KeyValuePair<ElementalType, float> pair in elements)
        {
            if (!found || pair.Value > amount)
            {
                dominant = pair.Key;
                amount = pair.Value;
                found = true;
            }
        }

        return found ? (ElementalType?)dominant : null;
    }

    private static DamageScalingType GetDominantScaling<T>(Dictionary<T, Dictionary<DamageScalingType, float>> map, T key) where T : class
    {
        Dictionary<DamageScalingType, float> scaling;
        if (key == null || !map.TryGetValue(key, out scaling) || scaling.Count == 0)
            return DamageScalingType.None;

        DamageScalingType dominant = DamageScalingType.None;
        float amount = 0f;

        foreach (KeyValuePair<DamageScalingType, float> pair in scaling)
        {
            if (pair.Key != DamageScalingType.None && pair.Value > amount)
            {
                dominant = pair.Key;
                amount = pair.Value;
            }
        }

        return dominant;
    }

    private static void AddScaling<T>(Dictionary<T, Dictionary<DamageScalingType, float>> map, T key, DamageScalingType scalingType, float amount) where T : class
    {
        if (key == null || scalingType == DamageScalingType.None || amount <= 0f)
            return;

        Dictionary<DamageScalingType, float> scaling;
        if (!map.TryGetValue(key, out scaling))
        {
            scaling = new Dictionary<DamageScalingType, float>();
            map[key] = scaling;
        }

        float current;
        scaling.TryGetValue(scalingType, out current);
        scaling[scalingType] = current + amount;
    }

    private static void AddElement<T>(Dictionary<T, Dictionary<ElementalType, float>> map, T key, ElementalType? elemental, float amount) where T : class
    {
        if (key == null || !elemental.HasValue)
            return;

        Dictionary<ElementalType, float> elements;
        if (!map.TryGetValue(key, out elements))
        {
            elements = new Dictionary<ElementalType, float>();
            map[key] = elements;
        }

        float current;
        elements.TryGetValue(elemental.Value, out current);
        elements[elemental.Value] = current + amount;
    }

    private static string GetEssenceKey(Gem gem)
    {
        if (gem == null)
            return null;

        string key = gem.GetOriginalName();
        if (string.IsNullOrEmpty(key))
            key = gem.GetActorReadableName();

        return string.IsNullOrEmpty(key) ? null : key;
    }

}