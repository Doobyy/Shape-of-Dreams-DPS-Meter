using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DPSMeter;

public sealed class DpsData
{
    private readonly Dictionary<SkillTrigger, float> _currentPersonal = new Dictionary<SkillTrigger, float>();
    private readonly Dictionary<SkillTrigger, float> _cumulativePersonal = new Dictionary<SkillTrigger, float>();
    private readonly Dictionary<string, float> _currentOtherPersonal = new Dictionary<string, float>();
    private readonly Dictionary<string, float> _cumulativeOtherPersonal = new Dictionary<string, float>();

    private readonly Dictionary<string, float> _currentParty = new Dictionary<string, float>();
    private readonly Dictionary<string, float> _cumulativeParty = new Dictionary<string, float>();

    public float CurrentTotalDamage { get; private set; }
    public float CumulativeTotalDamage { get; private set; }
    public float StartedAt { get; private set; }
    public float LastHitAt { get; private set; }
    public int CurrentHitCount { get; private set; }

    public float CurrentDuration =>
        CurrentHitCount == 0 ? 0f : Mathf.Max(0.001f, LastHitAt - StartedAt);

    public float CurrentDps =>
        CurrentHitCount == 0 ? 0f : CurrentTotalDamage / CurrentDuration;

    public float CumulativeDps =>
        CumulativeTotalDamage <= 0f || StartedAt <= 0f
            ? 0f
            : CumulativeTotalDamage / Mathf.Max(0.001f, Time.time - StartedAt);

    public IReadOnlyList<KeyValuePair<SkillTrigger, float>> CurrentPersonalSkills =>
        _currentPersonal.OrderByDescending(pair => pair.Value).ToList();

    public IReadOnlyList<KeyValuePair<SkillTrigger, float>> CumulativePersonalSkills =>
        _cumulativePersonal.OrderByDescending(pair => pair.Value).ToList();

    public IReadOnlyList<KeyValuePair<string, float>> CurrentPersonalOther =>
        _currentOtherPersonal.OrderByDescending(pair => pair.Value).ToList();

    public IReadOnlyList<KeyValuePair<string, float>> CumulativePersonalOther =>
        _cumulativeOtherPersonal.OrderByDescending(pair => pair.Value).ToList();

    public IReadOnlyList<KeyValuePair<string, float>> CurrentParty =>
        _currentParty.OrderByDescending(pair => pair.Value).ToList();

    public IReadOnlyList<KeyValuePair<string, float>> CumulativeParty =>
        _cumulativeParty.OrderByDescending(pair => pair.Value).ToList();

    public void AddDamage(
        float amount,
        bool isLocalPlayer,
        SkillTrigger skill,
        string sourceName,
        string playerName)
    {
        if (amount <= 0f)
        {
            return;
        }

        float now = Time.time;

        if (CurrentHitCount == 0)
        {
            StartedAt = now;
        }

        LastHitAt = now;
        CurrentHitCount++;
        CurrentTotalDamage += amount;
        CumulativeTotalDamage += amount;

        Add(_currentParty, playerName, amount);
        Add(_cumulativeParty, playerName, amount);

        if (!isLocalPlayer)
        {
            return;
        }

        if (skill != null)
        {
            Add(_currentPersonal, skill, amount);
            Add(_cumulativePersonal, skill, amount);
        }
        else
        {
            Add(_currentOtherPersonal, sourceName, amount);
            Add(_cumulativeOtherPersonal, sourceName, amount);
        }
    }

    public void ResetCurrentInstance()
    {
        CurrentTotalDamage = 0f;
        StartedAt = 0f;
        LastHitAt = 0f;
        CurrentHitCount = 0;
        _currentPersonal.Clear();
        _currentOtherPersonal.Clear();
        _currentParty.Clear();
    }

    public void Reset()
    {
        ResetCurrentInstance();
        CumulativeTotalDamage = 0f;
        _cumulativePersonal.Clear();
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

    private static void Add(Dictionary<SkillTrigger, float> map, SkillTrigger key, float amount)
    {
        float current;
        map.TryGetValue(key, out current);
        map[key] = current + amount;
    }
}
