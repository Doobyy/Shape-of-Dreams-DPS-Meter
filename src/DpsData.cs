using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DPSMeter;

public sealed class DpsData
{
    public sealed class BreakdownRow
    {
        public string Identity;
        public string Name;
        public float Amount;
    }

    public sealed class HealingRecord
    {
        public int Index;
        public float Time;
        public float Amount;
        public string Identity;
        public string SourceName;
        public string ActorChain;
    }

    private readonly List<HealingRecord> _healingRecords = new List<HealingRecord>();
    private const int MaxHealingRecords = 10000;
    private int _nextHealingRecordIndex = 1;

    public enum DamageScalingType
    {
        None,
        Ad,
        Ap,
        Hp
    }
    private readonly Dictionary<string, float> _currentPersonalSkills = new Dictionary<string, float>();
    private readonly Dictionary<string, Sprite> _skillIcons = new Dictionary<string, Sprite>();
    private readonly Dictionary<string, string> _skillDisplayNames = new Dictionary<string, string>();
    private readonly Dictionary<string, Sprite> _otherIcons = new Dictionary<string, Sprite>();
    private readonly Dictionary<string, float> _cumulativePersonalSkills = new Dictionary<string, float>();
    private readonly Dictionary<string, float> _currentPersonalEssences = new Dictionary<string, float>();
    private readonly Dictionary<string, Dictionary<DamageScalingType, float>> _currentPersonalEssenceScaling = new Dictionary<string, Dictionary<DamageScalingType, float>>();
    private readonly Dictionary<string, Sprite> _currentPersonalEssenceIcons = new Dictionary<string, Sprite>();
    private readonly Dictionary<string, float> _cumulativePersonalEssences = new Dictionary<string, float>();
    private readonly Dictionary<string, string> _essenceDisplayNames = new Dictionary<string, string>();
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
    private readonly Dictionary<string, Sprite> _currentPersonalHealingIcons = new Dictionary<string, Sprite>();
    private readonly Dictionary<string, float> _cumulativePersonalHealing = new Dictionary<string, float>();
    private readonly Dictionary<string, Sprite> _cumulativePersonalHealingIcons = new Dictionary<string, Sprite>();
    private readonly Dictionary<string, string> _healingDisplayNames = new Dictionary<string, string>();
    private readonly Dictionary<string, float> _currentPersonalBarrier = new Dictionary<string, float>();
    private readonly Dictionary<string, Sprite> _currentPersonalBarrierIcons = new Dictionary<string, Sprite>();
    private readonly Dictionary<string, float> _cumulativePersonalBarrier = new Dictionary<string, float>();
    private readonly Dictionary<string, Sprite> _cumulativePersonalBarrierIcons = new Dictionary<string, Sprite>();
    private readonly Dictionary<string, string> _barrierDisplayNames = new Dictionary<string, string>();
    private readonly Dictionary<string, float> _currentParty = new Dictionary<string, float>();
    private readonly Dictionary<string, float> _cumulativeParty = new Dictionary<string, float>();
    private readonly Dictionary<string, float> _currentPartyHealing = new Dictionary<string, float>();
    private readonly Dictionary<string, float> _cumulativePartyHealing = new Dictionary<string, float>();
    private readonly Dictionary<string, float> _currentPartyBarrier = new Dictionary<string, float>();
    private readonly Dictionary<string, float> _cumulativePartyBarrier = new Dictionary<string, float>();
    private bool _pendingInstanceReset;

    public float CurrentInstancePersonalHealing { get; private set; }
    public float CumulativePersonalHealing { get; private set; }
    public float CurrentInstancePersonalBarrier { get; private set; }
    public float CumulativePersonalBarrier { get; private set; }

    public float BarrierStartedAt { get; private set; }
    public float LastBarrierAt { get; private set; }
    public int CurrentBarrierCount { get; private set; }
    public float CumulativeBarrierStartedAt { get; private set; }
    public float LastCumulativeBarrierAt { get; private set; }

    public float CurrentBarrierDuration =>
        CurrentBarrierCount == 0 ? 0f : Mathf.Max(0.001f, LastBarrierAt - BarrierStartedAt);

    public float CurrentPersonalBps =>
        CurrentBarrierCount == 0 ? 0f : CurrentInstancePersonalBarrier / CurrentBarrierDuration;

    public float TotalPersonalBps =>
        CumulativePersonalBarrier <= 0f || CumulativeBarrierStartedAt <= 0f
            ? 0f
            : CumulativePersonalBarrier / Mathf.Max(0.001f, LastCumulativeBarrierAt - CumulativeBarrierStartedAt);

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

    public float CurrentInstancePartyHealing { get; private set; }
    public float CumulativePartyHealing { get; private set; }
    public float CurrentInstancePartyBarrier { get; private set; }
    public float CumulativePartyBarrier { get; private set; }
    public float PartyHealingStartedAt { get; private set; }
    public float LastPartyHealingAt { get; private set; }
    public float PartyBarrierStartedAt { get; private set; }
    public float LastPartyBarrierAt { get; private set; }

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

    public float CurrentPartyHps => CurrentHealCount == 0 ? 0f : CurrentInstancePartyHealing / Mathf.Max(0.001f, LastPartyHealingAt - HealingStartedAt);
    public float TotalPartyHps => CumulativePartyHealing <= 0f || PartyHealingStartedAt <= 0f ? 0f : CumulativePartyHealing / Mathf.Max(0.001f, LastPartyHealingAt - PartyHealingStartedAt);
    public float CurrentPartyBps => CurrentBarrierCount == 0 ? 0f : CurrentInstancePartyBarrier / Mathf.Max(0.001f, LastPartyBarrierAt - BarrierStartedAt);
    public float TotalPartyBps => CumulativePartyBarrier <= 0f || PartyBarrierStartedAt <= 0f ? 0f : CumulativePartyBarrier / Mathf.Max(0.001f, LastPartyBarrierAt - PartyBarrierStartedAt);
    public float CumulativePartyDps => CumulativePartyDamage <= 0f || StartedAt <= 0f ? 0f : CumulativePartyDamage / Mathf.Max(0.001f, LastHitAt - StartedAt);

    public float CurrentPartyAppliedDps =>
        CurrentHitCount == 0 ? 0f : CurrentInstancePartyAppliedDamage / CurrentDuration;

    public IReadOnlyList<BreakdownRow> CurrentPersonalBarrierRows =>
        _currentPersonalBarrier
            .OrderByDescending(pair => pair.Value)
            .Select(pair => new BreakdownRow { Identity = pair.Key, Name = GetBarrierDisplayName(pair.Key), Amount = pair.Value })
            .ToList();

    public IReadOnlyList<BreakdownRow> CumulativeBarrierRows =>
        _cumulativePersonalBarrier
            .OrderByDescending(pair => pair.Value)
            .Select(pair => new BreakdownRow { Identity = pair.Key, Name = GetBarrierDisplayName(pair.Key), Amount = pair.Value })
            .ToList();

    public IReadOnlyList<KeyValuePair<string, float>> CurrentPersonalHealing =>
        _currentPersonalHealing
            .OrderByDescending(pair => pair.Value)
            .Select(pair => new KeyValuePair<string, float>(GetHealingDisplayName(pair.Key), pair.Value))
            .ToList();

    public IReadOnlyList<BreakdownRow> CurrentPersonalHealingRows =>
        _currentPersonalHealing
            .OrderByDescending(pair => pair.Value)
            .Select(pair => new BreakdownRow { Identity = pair.Key, Name = GetHealingDisplayName(pair.Key), Amount = pair.Value })
            .ToList();

    public IReadOnlyList<KeyValuePair<string, float>> CumulativeHealingSources =>
        _cumulativePersonalHealing
            .OrderByDescending(pair => pair.Value)
            .Select(pair => new KeyValuePair<string, float>(GetHealingDisplayName(pair.Key), pair.Value))
            .ToList();

    public IReadOnlyList<BreakdownRow> CumulativeHealingRows =>
        _cumulativePersonalHealing
            .OrderByDescending(pair => pair.Value)
            .Select(pair => new BreakdownRow { Identity = pair.Key, Name = GetHealingDisplayName(pair.Key), Amount = pair.Value })
            .ToList();

    public IReadOnlyList<KeyValuePair<string, float>> CurrentPersonalSkills =>
        _currentPersonalSkills
            .OrderByDescending(pair => pair.Value)
            .Select(pair => new KeyValuePair<string, float>(GetSkillDisplayName(pair.Key), pair.Value))
            .ToList();

    public IReadOnlyList<BreakdownRow> CurrentPersonalSkillRows =>
        _currentPersonalSkills
            .OrderByDescending(pair => pair.Value)
            .Select(pair => new BreakdownRow { Identity = pair.Key, Name = GetSkillDisplayName(pair.Key), Amount = pair.Value })
            .ToList();

    public IReadOnlyList<KeyValuePair<string, float>> CumulativePersonalSkills =>
        _cumulativePersonalSkills
            .OrderByDescending(pair => pair.Value)
            .Select(pair => new KeyValuePair<string, float>(GetSkillDisplayName(pair.Key), pair.Value))
            .ToList();

    public IReadOnlyList<BreakdownRow> CumulativePersonalSkillRows =>
        _cumulativePersonalSkills
            .OrderByDescending(pair => pair.Value)
            .Select(pair => new BreakdownRow { Identity = pair.Key, Name = GetSkillDisplayName(pair.Key), Amount = pair.Value })
            .ToList();

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

    public IReadOnlyList<KeyValuePair<string, float>> CurrentPartyHealing => _currentPartyHealing.OrderByDescending(pair => pair.Value).ToList();
    public IReadOnlyList<KeyValuePair<string, float>> CumulativePartyHealing => _cumulativePartyHealing.OrderByDescending(pair => pair.Value).ToList();
    public IReadOnlyList<KeyValuePair<string, float>> CurrentPartyBarrier => _currentPartyBarrier.OrderByDescending(pair => pair.Value).ToList();
    public IReadOnlyList<KeyValuePair<string, float>> CumulativePartyBarrier => _cumulativePartyBarrier.OrderByDescending(pair => pair.Value).ToList();

    public void AddBarrier(float barrier, string sourceIdentity, string sourceName, Sprite icon)
    {
        if (barrier <= 0f)
        {
            return;
        }

        if (_pendingInstanceReset)
        {
            ClearCurrentInstance();
            _pendingInstanceReset = false;
        }

        float now = Time.time;

        if (CurrentBarrierCount == 0)
        {
            BarrierStartedAt = now;
        }

        if (CumulativeBarrierStartedAt <= 0f)
        {
            CumulativeBarrierStartedAt = now;
        }

        LastBarrierAt = now;
        LastCumulativeBarrierAt = now;
        CurrentBarrierCount++;

        CurrentInstancePersonalBarrier += barrier;
        CumulativePersonalBarrier += barrier;

        string identity = string.IsNullOrEmpty(sourceIdentity) ? sourceName : sourceIdentity;
        if (string.IsNullOrEmpty(identity))
        {
            identity = "Unknown Barrier";
        }

        Add(_currentPersonalBarrier, identity, barrier);
        Add(_cumulativePersonalBarrier, identity, barrier);
        _barrierDisplayNames[identity] = string.IsNullOrEmpty(sourceName) ? identity : sourceName;

        if (icon != null)
        {
            _currentPersonalBarrierIcons[identity] = icon;
            _cumulativePersonalBarrierIcons[identity] = icon;
        }
    }
    public void AddPartyHealing(float healing, string playerName)
    {
        if (healing <= 0f || string.IsNullOrEmpty(playerName)) return;
        float now = Time.time;
        if (PartyHealingStartedAt <= 0f) PartyHealingStartedAt = now;
        LastPartyHealingAt = now;
        CurrentInstancePartyHealing += healing;
        CumulativePartyHealing += healing;
        Add(_currentPartyHealing, playerName, healing);
        Add(_cumulativePartyHealing, playerName, healing);
    }

    public void AddPartyBarrier(float barrier, string playerName)
    {
        if (barrier <= 0f || string.IsNullOrEmpty(playerName)) return;
        float now = Time.time;
        if (PartyBarrierStartedAt <= 0f) PartyBarrierStartedAt = now;
        LastPartyBarrierAt = now;
        CurrentInstancePartyBarrier += barrier;
        CumulativePartyBarrier += barrier;
        Add(_currentPartyBarrier, playerName, barrier);
        Add(_cumulativePartyBarrier, playerName, barrier);
    }

    public void AddHealing(float healing, string sourceIdentity, string sourceName, Sprite icon, string actorChain)
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

        if (icon != null && !string.IsNullOrEmpty(sourceName))
        {
            _currentPersonalHealingIcons[sourceName] = icon;
            _cumulativePersonalHealingIcons[sourceName] = icon;
        }

        if (_healingRecords.Count >= MaxHealingRecords)
        {
            _healingRecords.RemoveAt(0);
        }

        _healingRecords.Add(new HealingRecord
        {
            Index = _nextHealingRecordIndex++,
            Time = now,
            Amount = healing,
            Identity = sourceIdentity,
            SourceName = sourceName,
            ActorChain = actorChain
        });
    }

    public string ExportHealingLog()
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("Shape of Dreams DPS Meter - Healing Diagnostic Export");
        builder.AppendLine("Version: " + DPSMeter.DevelopmentVersion);
        builder.AppendLine("Records retained: " + _healingRecords.Count);
        builder.AppendLine();

        for (int i = 0; i < _healingRecords.Count; i++)
        {
            HealingRecord record = _healingRecords[i];
            builder.AppendLine("=== Healing Event " + record.Index + " ===");
            builder.AppendLine("Time: " + record.Time.ToString("0.000"));
            builder.AppendLine("Amount: " + record.Amount.ToString("0.######"));
            builder.AppendLine("Identity: " + (record.Identity ?? "<null>"));
            builder.AppendLine("SourceName: " + (record.SourceName ?? "<null>"));
            builder.AppendLine("Actor chain:");
            builder.AppendLine(string.IsNullOrEmpty(record.ActorChain) ? "  <none>" : record.ActorChain);
            builder.AppendLine();
        }

        return builder.ToString();
    }

    public void AddDamage(
        float producedDamage,
        float appliedDamage,
        bool isLocalPlayer,
        string skillIdentity,
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

        if (!string.IsNullOrEmpty(skillIdentity))
        {
            Add(_currentPersonalSkills, skillIdentity, producedDamage);
            Add(_cumulativePersonalSkills, skillIdentity, producedDamage);
            AddElement(_currentPersonalSkillElements, skillIdentity, elemental, producedDamage);
            AddScaling(_currentPersonalSkillScaling, skillIdentity, scalingType, producedDamage);
            AddElement(_cumulativePersonalSkillElements, skillIdentity, elemental, producedDamage);
            _skillDisplayNames[skillIdentity] = string.IsNullOrEmpty(skillName) ? skillIdentity : skillName;
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
                _essenceDisplayNames[essenceKey] = GetLocalizedEssenceName(essence);
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
        CurrentInstancePersonalBarrier = 0f;
        HealingStartedAt = 0f;
        BarrierStartedAt = 0f;
        LastHealAt = 0f;
        CurrentHealCount = 0;
        LastBarrierAt = 0f;
        CurrentBarrierCount = 0;

        CurrentInstancePersonalDamage = 0f;
        CurrentInstancePersonalAppliedDamage = 0f;
        CurrentInstancePersonalOverkill = 0f;
        CurrentInstancePartyHealing = 0f;
        CurrentInstancePartyBarrier = 0f;
        PartyHealingStartedAt = 0f;
        LastPartyHealingAt = 0f;
        PartyBarrierStartedAt = 0f;
        LastPartyBarrierAt = 0f;
        CurrentInstancePartyDamage = 0f;
        CurrentInstancePartyAppliedDamage = 0f;
        CurrentInstancePartyOverkill = 0f;
        StartedAt = 0f;
        LastHitAt = 0f;
        CurrentHitCount = 0;

        _currentPersonalHealing.Clear();
        _currentPersonalHealingIcons.Clear();
        _currentPersonalBarrier.Clear();
        _currentPersonalBarrierIcons.Clear();
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
        _currentPartyHealing.Clear();
        _currentPartyBarrier.Clear();
    }

    public void Reset()
    {
        ClearCurrentInstance();
        _pendingInstanceReset = false;

        CumulativePersonalHealing = 0f;
        CumulativePersonalBarrier = 0f;
        CumulativeHealingStartedAt = 0f;
        CumulativeBarrierStartedAt = 0f;
        LastCumulativeHealAt = 0f;
        LastCumulativeBarrierAt = 0f;
        CumulativePersonalDamage = 0f;
        CumulativePersonalAppliedDamage = 0f;
        CumulativePersonalOverkill = 0f;
        CumulativePartyHealing = 0f;
        CumulativePartyBarrier = 0f;
        PartyHealingStartedAt = 0f;
        LastPartyHealingAt = 0f;
        PartyBarrierStartedAt = 0f;
        LastPartyBarrierAt = 0f;
        CumulativePartyDamage = 0f;
        CumulativePartyAppliedDamage = 0f;
        CumulativePartyOverkill = 0f;

        _cumulativePersonalHealing.Clear();
        _cumulativePersonalHealingIcons.Clear();
        _healingDisplayNames.Clear();
        _cumulativePersonalBarrier.Clear();
        _cumulativePersonalBarrierIcons.Clear();
        _barrierDisplayNames.Clear();
        _cumulativePersonalSkills.Clear();
        _skillIcons.Clear();
        _skillDisplayNames.Clear();
        _otherIcons.Clear();
        _cumulativePersonalEssences.Clear();
        _essenceDisplayNames.Clear();
        _cumulativePersonalEssenceScaling.Clear();
        _cumulativePersonalEssenceIcons.Clear();
        _cumulativePersonalSkillElements.Clear();
        _cumulativePersonalEssenceElements.Clear();
        _cumulativeOtherPersonal.Clear();
        _cumulativeParty.Clear();
        _cumulativePartyHealing.Clear();
        _cumulativePartyBarrier.Clear();
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

    public ElementalType? GetCurrentSkillElement(string skillIdentity) => GetDominantElement(_currentPersonalSkillElements, skillIdentity);

    public DamageScalingType GetCurrentSkillScaling(string skillIdentity) => GetDominantScaling(_currentPersonalSkillScaling, skillIdentity);

    public DamageScalingType GetCurrentOtherScaling(string sourceName) => GetDominantScaling(_currentPersonalOtherScaling, sourceName);

    public DamageScalingType GetCurrentEssenceScaling(string essenceKey) => GetDominantScaling(_currentPersonalEssenceScaling, essenceKey);

    public DamageScalingType GetCumulativeEssenceScaling(string essenceKey) => GetDominantScaling(_cumulativePersonalEssenceScaling, essenceKey);

    public string GetEssenceDisplayName(string essenceKey)
    {
        string name;
        return !string.IsNullOrEmpty(essenceKey) && _essenceDisplayNames.TryGetValue(essenceKey, out name) && !string.IsNullOrEmpty(name)
            ? name
            : essenceKey;
    }

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

    public Sprite GetCurrentBarrierIcon(string sourceIdentity)
    {
        Sprite icon;
        return !string.IsNullOrEmpty(sourceIdentity) && _currentPersonalBarrierIcons.TryGetValue(sourceIdentity, out icon) ? icon : null;
    }

    public Sprite GetCumulativeBarrierIcon(string sourceIdentity)
    {
        Sprite icon;
        return !string.IsNullOrEmpty(sourceIdentity) && _cumulativePersonalBarrierIcons.TryGetValue(sourceIdentity, out icon) ? icon : null;
    }

    public Sprite GetCurrentHealingIcon(string sourceName)
    {
        Sprite icon;
        return !string.IsNullOrEmpty(sourceName) && _currentPersonalHealingIcons.TryGetValue(sourceName, out icon) ? icon : null;
    }

    public Sprite GetCumulativeHealingIcon(string sourceName)
    {
        Sprite icon;
        return !string.IsNullOrEmpty(sourceName) && _cumulativePersonalHealingIcons.TryGetValue(sourceName, out icon) ? icon : null;
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

    public Sprite GetCurrentOtherIcon(string sourceName)
    {
        Sprite icon;
        return !string.IsNullOrEmpty(sourceName) && _otherIcons.TryGetValue(sourceName, out icon) ? icon : null;
    }

    public void RegisterOtherIcon(string sourceName, Sprite icon)
    {
        if (string.IsNullOrEmpty(sourceName) || icon == null)
            return;

        _otherIcons[sourceName] = icon;
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

    private string GetSkillDisplayName(string skillIdentity)
    {
        string name;
        return !string.IsNullOrEmpty(skillIdentity) && _skillDisplayNames.TryGetValue(skillIdentity, out name) && !string.IsNullOrEmpty(name)
            ? name
            : skillIdentity;
    }

    private string GetBarrierDisplayName(string sourceIdentity)
    {
        string name;
        return !string.IsNullOrEmpty(sourceIdentity) && _barrierDisplayNames.TryGetValue(sourceIdentity, out name) && !string.IsNullOrEmpty(name)
            ? name
            : sourceIdentity;
    }

    private string GetHealingDisplayName(string sourceIdentity)
    {
        string name;
        return !string.IsNullOrEmpty(sourceIdentity) && _healingDisplayNames.TryGetValue(sourceIdentity, out name) && !string.IsNullOrEmpty(name)
            ? name
            : sourceIdentity;
    }

    private static string GetLocalizedEssenceName(Gem gem)
    {
        if (gem == null)
            return null;

        try
        {
            string name = DewLocalization.GetGemName(gem);
            return string.IsNullOrEmpty(name) ? null : name;
        }
        catch (System.Exception)
        {
            return null;
        }
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