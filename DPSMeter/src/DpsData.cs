using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DPSMeter;

public sealed class DpsData
{
    private readonly Dictionary<SkillTrigger, float> _currentPersonalSkills = new Dictionary<SkillTrigger, float>();
    private readonly Dictionary<SkillTrigger, float> _cumulativePersonalSkills = new Dictionary<SkillTrigger, float>();
    private readonly Dictionary<Gem, float> _currentPersonalEssences = new Dictionary<Gem, float>();
    private readonly Dictionary<Gem, float> _cumulativePersonalEssences = new Dictionary<Gem, float>();
    private readonly Dictionary<string, float> _currentOtherPersonal = new Dictionary<string, float>();
    private readonly Dictionary<SkillTrigger, Dictionary<ElementalType, float>> _currentPersonalSkillElements = new Dictionary<SkillTrigger, Dictionary<ElementalType, float>>();
    private readonly Dictionary<SkillTrigger, Dictionary<ElementalType, float>> _cumulativePersonalSkillElements = new Dictionary<SkillTrigger, Dictionary<ElementalType, float>>();
    private readonly Dictionary<Gem, Dictionary<ElementalType, float>> _currentPersonalEssenceElements = new Dictionary<Gem, Dictionary<ElementalType, float>>();
    private readonly Dictionary<Gem, Dictionary<ElementalType, float>> _cumulativePersonalEssenceElements = new Dictionary<Gem, Dictionary<ElementalType, float>>();
    private readonly Dictionary<string, float> _cumulativeOtherPersonal = new Dictionary<string, float>();
    private readonly Dictionary<string, float> _currentParty = new Dictionary<string, float>();
    private readonly Dictionary<string, float> _cumulativeParty = new Dictionary<string, float>();

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

    public IReadOnlyList<KeyValuePair<SkillTrigger, float>> CurrentPersonalSkills =>
        _currentPersonalSkills.OrderByDescending(pair => pair.Value).ToList();

    public IReadOnlyList<KeyValuePair<SkillTrigger, float>> CumulativePersonalSkills =>
        _cumulativePersonalSkills.OrderByDescending(pair => pair.Value).ToList();

    public IReadOnlyList<KeyValuePair<Gem, float>> CurrentPersonalEssences =>
        _currentPersonalEssences.OrderByDescending(pair => pair.Value).ToList();

    public IReadOnlyList<KeyValuePair<Gem, float>> CumulativePersonalEssences =>
        _cumulativePersonalEssences.OrderByDescending(pair => pair.Value).ToList();

    public IReadOnlyList<KeyValuePair<string, float>> CurrentPersonalOther =>
        _currentOtherPersonal.OrderByDescending(pair => pair.Value).ToList();

    public IReadOnlyList<KeyValuePair<string, float>> CumulativePersonalOther =>
        _cumulativeOtherPersonal.OrderByDescending(pair => pair.Value).ToList();

    public IReadOnlyList<KeyValuePair<string, float>> CurrentParty =>
        _currentParty.OrderByDescending(pair => pair.Value).ToList();

    public IReadOnlyList<KeyValuePair<string, float>> CumulativeParty =>
        _cumulativeParty.OrderByDescending(pair => pair.Value).ToList();

    public void AddDamage(
        float producedDamage,
        float appliedDamage,
        bool isLocalPlayer,
        SkillTrigger skill,
        string sourceName,
        Gem essence,
        ElementalType? elemental,
        string playerName)
    {
        if (producedDamage <= 0f)
        {
            return;
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

        if (skill != null)
        {
            Add(_currentPersonalSkills, skill, producedDamage);
            Add(_cumulativePersonalSkills, skill, producedDamage);
            AddElement(_currentPersonalSkillElements, skill, elemental, producedDamage);
            AddElement(_cumulativePersonalSkillElements, skill, elemental, producedDamage);
        }
        else
        {
            Add(_currentOtherPersonal, sourceName, producedDamage);
            Add(_cumulativeOtherPersonal, sourceName, producedDamage);
        }

        if (essence != null)
        {
            Add(_currentPersonalEssences, essence, producedDamage);
            Add(_cumulativePersonalEssences, essence, producedDamage);
            AddElement(_currentPersonalEssenceElements, essence, elemental, producedDamage);
            AddElement(_cumulativePersonalEssenceElements, essence, elemental, producedDamage);
        }
    }

    public void ResetCurrentInstance()
    {
        CurrentInstancePersonalDamage = 0f;
        CurrentInstancePersonalAppliedDamage = 0f;
        CurrentInstancePersonalOverkill = 0f;
        CurrentInstancePartyDamage = 0f;
        CurrentInstancePartyAppliedDamage = 0f;
        CurrentInstancePartyOverkill = 0f;
        StartedAt = 0f;
        LastHitAt = 0f;
        CurrentHitCount = 0;

        _currentPersonalSkills.Clear();
        _currentPersonalEssences.Clear();
        _currentPersonalSkillElements.Clear();
        _currentPersonalEssenceElements.Clear();
        _currentOtherPersonal.Clear();
        _currentParty.Clear();
    }

    public void Reset()
    {
        ResetCurrentInstance();

        CumulativePersonalDamage = 0f;
        CumulativePersonalAppliedDamage = 0f;
        CumulativePersonalOverkill = 0f;
        CumulativePartyDamage = 0f;
        CumulativePartyAppliedDamage = 0f;
        CumulativePartyOverkill = 0f;

        _cumulativePersonalSkills.Clear();
        _cumulativePersonalEssences.Clear();
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

    public ElementalType? GetCurrentSkillElement(SkillTrigger skill) => GetDominantElement(_currentPersonalSkillElements, skill);

    public ElementalType? GetCurrentEssenceElement(Gem gem) => GetDominantElement(_currentPersonalEssenceElements, gem);

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

    private static void Add(Dictionary<Gem, float> map, Gem key, float amount)
    {
        if (key == null)
            return;

        float current;
        map.TryGetValue(key, out current);
        map[key] = current + amount;
    }

    private static void Add(Dictionary<SkillTrigger, float> map, SkillTrigger key, float amount)
    {
        float current;
        map.TryGetValue(key, out current);
        map[key] = current + amount;
    }
}