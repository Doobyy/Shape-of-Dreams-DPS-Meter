using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DPSMeter;

public sealed class DpsData
{
    private readonly Dictionary<Actor, float> _currentPersonal = new Dictionary<Actor, float>();
    private readonly Dictionary<Actor, float> _cumulativePersonal = new Dictionary<Actor, float>();
    private readonly Dictionary<string, float> _currentOtherPersonal = new Dictionary<string, float>();
    private readonly Dictionary<string, float> _cumulativeOtherPersonal = new Dictionary<string, float>();
    private readonly Dictionary<string, float> _currentParty = new Dictionary<string, float>();
    private readonly Dictionary<string, float> _cumulativeParty = new Dictionary<string, float>();

    public float CurrentInstancePersonalDamage { get; private set; }
    public float CumulativePersonalDamage { get; private set; }
    public float CurrentInstancePartyDamage { get; private set; }
    public float CumulativePartyDamage { get; private set; }
    public float StartedAt { get; private set; }
    public float LastHitAt { get; private set; }
    public int CurrentHitCount { get; private set; }

    public float CurrentDuration =>
        CurrentHitCount == 0 ? 0f : Mathf.Max(0.001f, LastHitAt - StartedAt);

    public float CurrentPersonalDps =>
        CurrentHitCount == 0 ? 0f : CurrentInstancePersonalDamage / CurrentDuration;

    public float CurrentPartyDps =>
        CurrentHitCount == 0 ? 0f : CurrentInstancePartyDamage / CurrentDuration;

    public IReadOnlyList<KeyValuePair<Actor, float>> CurrentPersonalSkills =>
        _currentPersonal.OrderByDescending(pair => pair.Value).ToList();

    public IReadOnlyList<KeyValuePair<Actor, float>> CumulativePersonalSkills =>
        _cumulativePersonal.OrderByDescending(pair => pair.Value).ToList();

    public IReadOnlyList<KeyValuePair<string, float>> CurrentPersonalOther =>
        _currentOtherPersonal.OrderByDescending(pair => pair.Value).ToList();

    public IReadOnlyList<KeyValuePair<string, float>> CumulativePersonalOther =>
        _cumulativeOtherPersonal.OrderByDescending(pair => pair.Value).ToList();

    public IReadOnlyList<KeyValuePair<string, float>> CurrentParty =>
        _currentParty.OrderByDescending(pair => pair.Value).ToList();

    public IReadOnlyList<KeyValuePair<string, float>> CumulativeParty =>
        _cumulativeParty.OrderByDescending(pair => pair.Value).ToList();

    public void AddDamage(float amount, bool isLocalPlayer, Actor source, string sourceName, string playerName)
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
        CurrentInstancePartyDamage += amount;
        CumulativePartyDamage += amount;

        Add(_currentParty, playerName, amount);
        Add(_cumulativeParty, playerName, amount);

        if (!isLocalPlayer)
        {
            return;
        }

        CurrentInstancePersonalDamage += amount;
        CumulativePersonalDamage += amount;

        if (source != null)
        {
            Add(_currentPersonal, source, amount);
            Add(_cumulativePersonal, source, amount);
        }
        else
        {
            Add(_currentOtherPersonal, sourceName, amount);
            Add(_cumulativeOtherPersonal, sourceName, amount);
        }
    }

    public void ResetCurrentInstance()
    {
        CurrentInstancePersonalDamage = 0f;
        CurrentInstancePartyDamage = 0f;
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
        CumulativePersonalDamage = 0f;
        CumulativePartyDamage = 0f;
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

    private static void Add(Dictionary<Actor, float> map, Actor key, float amount)
    {
        float current;
        map.TryGetValue(key, out current);
        map[key] = current + amount;
    }
}