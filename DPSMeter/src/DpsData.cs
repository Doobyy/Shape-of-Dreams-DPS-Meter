using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DPSMeter;

public sealed class DpsData
{
    private readonly Dictionary<string, float> _skillDamage = new Dictionary<string, float>();
    private readonly Dictionary<string, float> _essenceDamage = new Dictionary<string, float>();

    public float TotalDamage { get; private set; }
    public float StartedAt { get; private set; }
    public float LastHitAt { get; private set; }
    public int HitCount { get; private set; }

    public float Duration => HitCount == 0 ? 0f : Mathf.Max(0.001f, LastHitAt - StartedAt);
    public float Dps => HitCount == 0 ? 0f : TotalDamage / Duration;

    public IReadOnlyList<KeyValuePair<string, float>> Skills =>
        _skillDamage.OrderByDescending(pair => pair.Value).ToList();

    public IReadOnlyList<KeyValuePair<string, float>> Essences =>
        _essenceDamage.OrderByDescending(pair => pair.Value).ToList();

    public void AddDamage(float amount, string skill, string essence)
    {
        float now = Time.time;

        if (HitCount == 0)
        {
            StartedAt = now;
        }

        LastHitAt = now;
        HitCount++;
        TotalDamage += amount;

        Add(_skillDamage, skill, amount);
        Add(_essenceDamage, essence, amount);
    }

    public void Reset()
    {
        TotalDamage = 0f;
        StartedAt = 0f;
        LastHitAt = 0f;
        HitCount = 0;
        _skillDamage.Clear();
        _essenceDamage.Clear();
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
