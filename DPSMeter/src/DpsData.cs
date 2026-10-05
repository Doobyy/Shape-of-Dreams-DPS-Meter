using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DPSMeter;

public sealed class DpsData
{
    private readonly Dictionary<string, float> _currentPersonalSkills = new Dictionary<string, float>();
    private readonly Dictionary<string, float> _cumulativePersonalSkills = new Dictionary<string, float>();
    private readonly Dictionary<string, float> _currentPersonalEssences = new Dictionary<string, float>();
    private readonly Dictionary<string, float> _cumulativePersonalEssences = new Dictionary<string, float>();
    private readonly Dictionary<string, float> _currentOtherPersonal = new Dictionary<string, float>();
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

    public float CurrentPersonalOverkill =>
        CurrentInstancePersonalOverkill;

    public float CurrentPartyAppliedDps =>
        CurrentHitCount == 0 ? 0f : CurrentInstancePartyAppliedDamage / CurrentDuration;

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

    public void AddDamage(
        float producedDamage,
        float appliedDamage,
        bool isLocalPlayer,
        string skillName,
        string essenceName,
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

        if (!string.IsNullOrEmpty(skillName))
        {
            Add(_currentPersonalSkills, skillName, producedDamage);
            Add(_cumulativePersonalSkills, skillName, producedDamage);
        }
        else
        {
            Add(_currentOtherPersonal, "Basic / Other", producedDamage);
            Add(_cumulativeOtherPersonal, "Basic / Other", producedDamage);
        }

        if (!string.IsNullOrEmpty(essenceName))
        {
            Add(_currentPersonalEssences, essenceName, producedDamage);
            Add(_cumulativePersonalEssences, essenceName, producedDamage);
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
}