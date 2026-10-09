using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using UnityEngine;

namespace DPSMeter;

[DefaultExecutionOrder(-10000)]
public sealed class DpsOverlay : MonoBehaviour
{
    private sealed class UiInputBlocker
    {
        public GameObject GameObject;
        public RectTransform RectTransform;
    }

    private GameObject _uiInputCanvasObject;
    private UiInputBlocker _mainWindowInputBlocker;
    private UiInputBlocker _historyInputBlocker;
    private UiInputBlocker _settingsInputBlocker;
    private UiInputBlocker _contextMenuInputBlocker;
    private bool _uiInputBlockerInitializationAttempted;
    private bool _uiInputBlockerFailureLogged;

    private enum DisplayMode
    {
        CurrentDps,
        DamageTotal,
        PartyDps,
        PartyTotal
    }

    private static readonly Color DefaultBarColor = new Color(0.875f, 0.871f, 0.867f, 1f);
    private static readonly Color WindowFillColor = new Color(0f, 0f, 0f, 0.55f);
    private static readonly Color FireBarColor = new Color(0.996f, 0.404f, 0.224f, 1f);
    private static readonly Color IceBarColor = new Color(0.855f, 0.937f, 0.996f, 1f);
    private static readonly Color LightBarColor = new Color(0.996f, 0.929f, 0.667f, 1f);
    private static readonly Color DarkBarColor = new Color(0.737f, 0.565f, 0.988f, 1f);
    private static readonly Color AdScalingBarColor = new Color(0.996f, 0.631f, 0.416f, 1f);
    private static readonly Color ApScalingBarColor = new Color(0.365f, 0.906f, 0.996f, 1f);
    private static readonly Color HpScalingBarColor = new Color(0.549f, 0.996f, 0.345f, 1f);
    private static readonly Color HealingBarColor = new Color(0.357f, 0.686f, 0.271f, 1f);
    private static readonly Color SourceNameColor = Color.white;
    private static readonly Color BarTextStrokeColor = Color.black;
    private static readonly Color DefaultBarOutlineColor = new Color(0.212f, 0.208f, 0.200f, 1f);
    private static readonly Color FireBarOutlineColor = new Color(0.506f, 0.051f, 0.008f, 1f);
    private static readonly Color IceBarOutlineColor = new Color(0.145f, 0.392f, 0.682f, 1f);
    private static readonly Color LightBarOutlineColor = new Color(0.553f, 0.408f, 0.141f, 1f);
    private static readonly Color DarkBarOutlineColor = new Color(0.173f, 0.020f, 0.459f, 1f);
    private static readonly Color AdScalingBarOutlineColor = new Color(0.227f, 0.118f, 0.016f, 1f);
    private static readonly Color ApScalingBarOutlineColor = new Color(0.086f, 0.231f, 0.498f, 1f);
    private static readonly Color HpScalingBarOutlineColor = new Color(0.180f, 0.235f, 0.137f, 1f);
    private static readonly Color HealingBarOutlineColor = new Color(0.180f, 0.235f, 0.137f, 1f);


    private DpsData _data;
    private readonly Dictionary<string, Sprite> _historyIconCache = new Dictionary<string, Sprite>();
    private Vector2 _scroll;
    private DisplayMode _mode;

    private Rect _windowRect = new Rect(20f, 20f, 260f, 197f);
    private bool _dragging;
    private bool _resizing;
    private Vector2 _dragOffset;
    private Vector2 _resizeStartMouse;
    private Vector2 _resizeStartSize;
    private bool _resizeMoved;
    private float _resizeStartHealingHeight;
    private bool _headerMoved;
    private bool _showHealing = true;
    private bool _showBarrier = true;
    private float _collapsedWindowHeight;
    private bool _manualResize;
    private bool _contextMenuOpen;
    private bool _settingsOpen;
    private bool _settingsDragging;
    private Vector2 _settingsDragOffset;
    private Texture2D _contextMenuHighlightTexture;
    private Rect _contextMenuRect;
    private Rect _settingsRect = new Rect(0f, 0f, 230f, 150f);

    private GUIStyle _header;
    private GUIStyle _headerRight;
    private GUIStyle _row;
    private GUIStyle _small;
    private GUIStyle _barBackground;
    private GUIStyle _barFill;
    private GUIStyle _rowRight;
    private Texture2D _whiteTexture;
    private Sprite _basicAttackIcon;
    private Vector2 _historyScroll;
    private bool _showRunHistory;
    private const float HistoryPanelWidth = 210f;
    private const float HistoryPanelHeight = 180f;

    public bool Visible { get; set; } = true;

    public void Initialize(DpsData data)
    {
        _data = data;
    }

    public void NotifyLiveDataReceived()
    {
        if (_selectedRunRecord != null)
            _selectedRunRecord = null;
    }

    private void Update()
    {
        UpdateUiInputBlockers();
    }

    private void OnGUI()
    {
        if (!Visible || _data == null)
        {
            return;
        }

        EnsureStyles();

        if (_collapsedWindowHeight <= 0f)
        {
            _collapsedWindowHeight = _windowRect.height;
        }

        HandleWindowInput();
        UpdateWindowHeight();

        GUI.color = WindowFillColor;
        GUI.DrawTexture(_windowRect, _whiteTexture);
        GUI.color = Color.white;

        Rect headerRect = new Rect(
            _windowRect.x + 6f,
            _windowRect.y + 4f,
            _windowRect.width - 12f,
            24f);

        DrawHeader(headerRect);

        Rect contentRect = new Rect(
            _windowRect.x + 6f,
            headerRect.yMax + 2f,
            _windowRect.width - 12f,
            Mathf.Max(20f, _collapsedWindowHeight - 50f));

        GUILayout.BeginArea(contentRect);
        _scroll = GUILayout.BeginScrollView(
            _scroll,
            GUILayout.Width(contentRect.width),
            GUILayout.Height(contentRect.height));

        if (_selectedRunRecord != null)
        {
            DrawSelectedRunLog();
        }
        else
        {
            switch (_mode)
            {
                case DisplayMode.CurrentDps:
                    DrawPersonal(
                        _data.CurrentPersonalSkillRows,
                        _data.CurrentPersonalOther,
                        _data.CurrentPersonalEssences,
                        _data.CurrentInstancePersonalDamage,
                        false);
                    break;

                case DisplayMode.DamageTotal:
                    DrawPersonal(
                        _data.CumulativePersonalSkillRows,
                        _data.CumulativePersonalOther,
                        _data.CumulativePersonalEssences,
                        _data.CumulativePersonalDamage,
                        true);
                    break;

                case DisplayMode.PartyDps:
                    DrawParty(_data.CurrentParty, _data.CurrentInstancePartyDamage, _data.CurrentPartyDps, "DPS");
                    break;

                case DisplayMode.PartyTotal:
                    DrawParty(_data.CumulativeParty, _data.CumulativePartyDamage, _data.CumulativePartyDps, "DPS");
                    break;
            }
        }

        GUILayout.EndScrollView();
        GUILayout.EndArea();

        if (_selectedRunRecord != null ||
            _mode == DisplayMode.CurrentDps || _mode == DisplayMode.DamageTotal ||
            _mode == DisplayMode.PartyDps || _mode == DisplayMode.PartyTotal)
        {
            Rect breakdownRect = new Rect(
                _windowRect.x + 6f,
                _windowRect.y + _collapsedWindowHeight - 20f,
                _windowRect.width - 12f,
                Mathf.Max(20f, _windowRect.height - _collapsedWindowHeight + 20f));

            GUILayout.BeginArea(breakdownRect);

            if (_selectedRunRecord != null)
            {
                if (_showHealing) DrawSelectedRunHealing(_selectedRunRecord);
                if (_showBarrier) DrawSelectedRunBarrier(_selectedRunRecord);
            }
            else
            {
                if (_showHealing)
                {
                    if (_mode == DisplayMode.PartyDps || _mode == DisplayMode.PartyTotal)
                    {
                        DrawParty(
                            _mode == DisplayMode.PartyDps ? _data.CurrentPartyHealing : _data.CumulativePartyHealingRows,
                            _mode == DisplayMode.PartyDps ? _data.CurrentInstancePartyHealing : _data.CumulativePartyHealing,
                            _mode == DisplayMode.PartyDps ? _data.CurrentPartyHps : _data.TotalPartyHps,
                            "HPS",
                            _mode == DisplayMode.PartyDps
                                ? "HPS: " + FormatNumber(_data.CurrentPartyHps)
                                : "HEAL: " + FormatNumber(_data.CumulativePartyHealing));
                    }
                    else
                    {
                        DrawHealingBreakdown(
                            _mode == DisplayMode.CurrentDps ? _data.CurrentPersonalHealingRows : _data.CumulativeHealingRows,
                            _mode == DisplayMode.CurrentDps ? _data.CurrentInstancePersonalHealing : _data.CumulativePersonalHealing);
                    }
                }

                if (_showBarrier)
                {
                    if (_mode == DisplayMode.PartyDps || _mode == DisplayMode.PartyTotal)
                    {
                        DrawParty(
                            _mode == DisplayMode.PartyDps ? _data.CurrentPartyBarrier : _data.CumulativePartyBarrierRows,
                            _mode == DisplayMode.PartyDps ? _data.CurrentInstancePartyBarrier : _data.CumulativePartyBarrier,
                            _mode == DisplayMode.PartyDps ? _data.CurrentPartyBps : _data.TotalPartyBps,
                            "BPS",
                            _mode == DisplayMode.PartyDps
                                ? "BPS: " + FormatNumber(_data.CurrentPartyBps)
                                : "BARRIER: " + FormatNumber(_data.CumulativePartyBarrier));
                    }
                    else
                    {
                        DrawBarrierBreakdown(
                            _mode == DisplayMode.CurrentDps ? _data.CurrentPersonalBarrierRows : _data.CumulativeBarrierRows,
                            _mode == DisplayMode.CurrentDps ? _data.CurrentInstancePersonalBarrier : _data.CumulativePersonalBarrier);
                    }
                }
            }

            GUILayout.EndArea();
        }

        DrawResizeGrip();

        if (_contextMenuOpen)
        {
            DrawContextMenu();
        }

        if (_settingsOpen)
        {
            DrawSettingsWindow();
        }

        if (_showRunHistory)
        {
            DrawRunHistoryPanel();
        }

        UpdateUiInputBlockers();
    }

    private DpsData.RunRecord _selectedRunRecord;

    private void DrawRunHistoryPanel()
    {
        if (DPSMeter.Instance != null)
            DPSMeter.Instance.RefreshActiveRunSummaryForDisplay();

        Rect panel = GetRunHistoryPanelRect();
        GUI.Box(panel, GUIContent.none);
        const float padding = 6f;
        const float rowHeight = 34f;
        Rect collapseRect = new Rect(panel.xMax - 22f, panel.y + 3f, 18f, 18f);
        if (GUI.Button(collapseRect, "‹"))
            _showRunHistory = false;

        Rect listRect = new Rect(panel.x + padding, panel.y + 5f,
            panel.width - padding * 2f, panel.height - 10f);
        float contentHeight = rowHeight * (1 + _data.CompletedRuns.Count);
        Rect viewRect = new Rect(0f, 0f, listRect.width - 14f,
            Mathf.Max(listRect.height, contentHeight));
        _historyScroll = GUI.BeginScrollView(listRect, _historyScroll, viewRect);

        float y = 0f;
        DpsData.RunRecord active = _data.ActiveRun;
        string activeCharacter = active == null || string.IsNullOrEmpty(active.CharacterName)
            ? "Unknown" : active.CharacterName;
        Rect activeRect = new Rect(0f, y, viewRect.width, rowHeight);
        DrawRunHistoryRow(activeRect, active == null ? null : active,
            active == null ? "Current " + activeCharacter : 
                "Current " + activeCharacter + " " + FormatDuration(active.DurationSeconds),
            active == null ? "Awaiting next run" :
                "Visited: " + active.WorldsVisited + " Worlds - " + active.MapsVisited + " Maps",
            true);
        y += rowHeight;

        for (int i = 0; i < _data.CompletedRuns.Count; i++)
        {
            DpsData.RunRecord run = _data.CompletedRuns[i];
            if (run == null) continue;
            string character = string.IsNullOrEmpty(run.CharacterName) ? "Unknown" : run.CharacterName;
            Rect rowRect = new Rect(0f, y, viewRect.width, rowHeight);
            DrawRunHistoryRow(rowRect, run,
                character + " " + FormatNumber(run.TotalDamage) + " - " + FormatDuration(run.DurationSeconds),
                "Visited: " + run.WorldsVisited + " Worlds - " + run.MapsVisited + " Maps", false);
            y += rowHeight;
        }

        GUI.EndScrollView();
    }

    private void DrawRunHistoryRow(Rect rect, DpsData.RunRecord run, string line1, string line2, bool isCurrent)
    {
        bool selected = isCurrent ? _selectedRunRecord == null : ReferenceEquals(_selectedRunRecord, run);
        bool hovered = rect.Contains(Event.current.mousePosition);

        GUI.color = new Color(0.08f, 0.08f, 0.08f, 0.48f);
        GUI.DrawTexture(rect, _whiteTexture);
        GUI.color = Color.white;

        if (selected || hovered)
        {
            GUI.color = new Color(1f, 1f, 1f, hovered ? 0.24f : 0.13f);
            GUI.DrawTexture(rect, _whiteTexture);
            GUI.color = Color.white;
        }

        if (GUI.Button(rect, GUIContent.none, GUIStyle.none))
            _selectedRunRecord = isCurrent ? null : run;

        GUI.Label(new Rect(rect.x, rect.y, rect.width, 17f), line1, _row);
        GUI.Label(new Rect(rect.x, rect.y + 16f, rect.width, 17f), line2, _row);
    }

    private string GetSelectedRunViewName()
    {
        switch (_mode)
        {
            case DisplayMode.CurrentDps: return "CURRENT";
            case DisplayMode.DamageTotal: return "TOTAL";
            case DisplayMode.PartyDps: return "PARTY";
            case DisplayMode.PartyTotal: return "PARTY_TOTAL";
            default: return "CURRENT";
        }
    }

    private DpsData.RunViewMetrics GetSelectedRunMetrics(DpsData.RunRecord run, string view)
    {
        if (run == null || run.ViewMetrics == null) return null;
        for (int i = 0; i < run.ViewMetrics.Count; i++)
            if (run.ViewMetrics[i] != null && string.Equals(run.ViewMetrics[i].View, view, StringComparison.Ordinal))
                return run.ViewMetrics[i];
        return null;
    }

    private List<DpsData.RunBreakdownRow> GetSelectedRunViewRows(
        DpsData.RunRecord run, string view, string category)
    {
        List<DpsData.RunBreakdownRow> rows = new List<DpsData.RunBreakdownRow>();
        if (run == null || run.ViewRows == null) return rows;
        for (int i = 0; i < run.ViewRows.Count; i++)
        {
            DpsData.RunBreakdownRow row = run.ViewRows[i];
            if (row != null &&
                string.Equals(row.View, view, StringComparison.Ordinal) &&
                string.Equals(row.Category, category, StringComparison.Ordinal))
                rows.Add(row);
        }
        rows.Sort((a, b) => b.Amount.CompareTo(a.Amount));
        return rows;
    }

    private static List<KeyValuePair<string, float>> ToPartyRows(List<DpsData.RunBreakdownRow> rows)
    {
        List<KeyValuePair<string, float>> result = new List<KeyValuePair<string, float>>();
        for (int i = 0; i < rows.Count; i++)
            result.Add(new KeyValuePair<string, float>(rows[i].Name, rows[i].Amount));
        return result;
    }

    private void DrawSelectedRunLog()
    {
        DpsData.RunRecord run = _selectedRunRecord;
        if (run == null) return;
        if (run.ViewRows == null || run.ViewRows.Count == 0)
        {
            DrawLegacySelectedRunLog();
            return;
        }

        string view = GetSelectedRunViewName();
        DpsData.RunViewMetrics metrics = GetSelectedRunMetrics(run, view);
        if (metrics == null) return;

        List<DpsData.RunBreakdownRow> rows = GetSelectedRunViewRows(run, view, "DAMAGE");
        if (_mode == DisplayMode.PartyDps || _mode == DisplayMode.PartyTotal)
        {
            DrawParty(ToPartyRows(rows), metrics.Damage, metrics.DamageRate, "DPS");
            return;
        }

        float maxAmount = GetSelectedRunMax(rows);
        if (rows.Count == 0)
        {
            GUILayout.Label("No damage recorded yet.", _small);
            return;
        }

        for (int i = 0; i < rows.Count; i++)
        {
            DpsData.RunBreakdownRow row = rows[i];
            ElementalType? elemental = null;
            ElementalType parsedElement;
            if (!string.IsNullOrEmpty(row.Elemental) && Enum.TryParse<ElementalType>(row.Elemental, out parsedElement))
                elemental = parsedElement;

            DpsData.DamageScalingType scaling = DpsData.DamageScalingType.None;
            DpsData.DamageScalingType parsedScaling;
            if (!string.IsNullOrEmpty(row.Scaling) && Enum.TryParse<DpsData.DamageScalingType>(row.Scaling, out parsedScaling))
                scaling = parsedScaling;

            DrawDamageRow(StripRichTextTags(row.Name), row.Amount, metrics.Damage,
                maxAmount, i, elemental, scaling, ResolveRunHistoryIcon(row));
        }
    }

    private void DrawLegacySelectedRunLog()
    {
        DpsData.RunRecord run = _selectedRunRecord;
        if (run == null) return;

        GUILayout.Label(
            (string.IsNullOrEmpty(run.CharacterName) ? "Unknown Character" : run.CharacterName) +
            "  |  " + (string.IsNullOrEmpty(run.Outcome) ? "Completed Run" : run.Outcome),
            _header);
        GUILayout.Label(
            FormatDuration(run.DurationSeconds) + "  |  Visited: " +
            run.WorldsVisited + " Worlds " + run.MapsVisited + " Maps  |  " +
            (run.CompletedAt ?? string.Empty),
            _small);
        GUILayout.Space(3f);
        GUILayout.Label("DAMAGE: " + FormatNumber(run.TotalDamage), _headerRight);

        List<DpsData.RunBreakdownRow> rows = GetSelectedRunRows(run, "DAMAGE");
        float maxAmount = GetSelectedRunMax(rows);
        if (rows.Count == 0)
        {
            GUILayout.Label("No damage breakdown was saved for this run.", _small);
            return;
        }

        for (int i = 0; i < rows.Count; i++)
        {
            DpsData.RunBreakdownRow row = rows[i];
            ElementalType? elemental = null;
            ElementalType parsedElement;
            if (!string.IsNullOrEmpty(row.Elemental) && Enum.TryParse<ElementalType>(row.Elemental, out parsedElement))
                elemental = parsedElement;

            DpsData.DamageScalingType scaling = DpsData.DamageScalingType.None;
            DpsData.DamageScalingType parsedScaling;
            if (!string.IsNullOrEmpty(row.Scaling) && Enum.TryParse<DpsData.DamageScalingType>(row.Scaling, out parsedScaling))
                scaling = parsedScaling;

            DrawDamageRow(StripRichTextTags(row.Name), row.Amount, run.TotalDamage,
                maxAmount, i, elemental, scaling, ResolveRunHistoryIcon(row));
        }
    }

    private Sprite ResolveRunHistoryIcon(DpsData.RunBreakdownRow row)
    {
        if (row == null) return null;
        string identity = string.IsNullOrEmpty(row.Identity) ? row.Name : row.Identity;

        string cacheKey = row.IconName + "|" + (row.IconTextureName ?? string.Empty);
        if (!string.IsNullOrEmpty(row.IconName))
        {
            Sprite cached;
            if (_historyIconCache.TryGetValue(cacheKey, out cached)) return cached;

            Sprite[] sprites = Resources.FindObjectsOfTypeAll<Sprite>();
            for (int i = 0; sprites != null && i < sprites.Length; i++)
            {
                Sprite candidate = sprites[i];
                if (candidate == null || !string.Equals(candidate.name, row.IconName, StringComparison.Ordinal))
                    continue;
                if (!string.IsNullOrEmpty(row.IconTextureName) &&
                    (candidate.texture == null || !string.Equals(candidate.texture.name, row.IconTextureName, StringComparison.Ordinal)))
                    continue;
                _historyIconCache[cacheKey] = candidate;
                return candidate;
            }
        }

        // The saved sprite may not be loaded yet. Retry live icon registries as a fallback.
        if (_data != null)
        {
            Sprite liveIcon = null;
            if (row.SourceType == "SKILL") liveIcon = _data.GetSkillIcon(identity);
            else if (row.SourceType == "OTHER")
                liveIcon = string.Equals(row.Name, "Basic Attack", StringComparison.Ordinal)
                    ? GetBasicAttackIcon()
                    : _data.GetCurrentOtherIcon(identity);
            else if (row.SourceType == "ESSENCE") liveIcon = _data.GetCumulativeEssenceIcon(identity);
            else if (row.SourceType == "HEALING") liveIcon = _data.GetCumulativeHealingIcon(identity);
            else if (row.SourceType == "BARRIER") liveIcon = _data.GetCumulativeBarrierIcon(identity);
            if (liveIcon != null) return liveIcon;
        }

        return null;
    }

    private static List<DpsData.RunBreakdownRow> GetSelectedRunRows(
        DpsData.RunRecord run, string category)
    {
        List<DpsData.RunBreakdownRow> rows = new List<DpsData.RunBreakdownRow>();
        if (run == null || run.BreakdownRows == null) return rows;

        for (int i = 0; i < run.BreakdownRows.Count; i++)
        {
            DpsData.RunBreakdownRow row = run.BreakdownRows[i];
            if (row != null && string.Equals(row.Category, category, StringComparison.Ordinal))
                rows.Add(row);
        }

        rows.Sort((a, b) => b.Amount.CompareTo(a.Amount));
        return rows;
    }

    private static float GetSelectedRunTotal(List<DpsData.RunBreakdownRow> rows)
    {
        float total = 0f;
        for (int i = 0; i < rows.Count; i++) total += rows[i].Amount;
        return total;
    }

    private static float GetSelectedRunMax(List<DpsData.RunBreakdownRow> rows)
    {
        float max = 0f;
        for (int i = 0; i < rows.Count; i++)
            if (rows[i].Amount > max) max = rows[i].Amount;
        return max;
    }

    private void DrawSelectedRunHealing(DpsData.RunRecord run)
    {
        if (run.ViewRows == null || run.ViewRows.Count == 0)
        {
            DrawLegacySelectedRunHealing(run);
            return;
        }
        string view = GetSelectedRunViewName();
        DpsData.RunViewMetrics metrics = GetSelectedRunMetrics(run, view);
        if (metrics == null) return;
        List<DpsData.RunBreakdownRow> rows = GetSelectedRunViewRows(run, view, "HEALING");
        if (_mode == DisplayMode.PartyDps || _mode == DisplayMode.PartyTotal)
        {
            DrawParty(ToPartyRows(rows), metrics.Healing, metrics.HealingRate, "HPS",
                _mode == DisplayMode.PartyDps
                    ? "HPS: " + FormatNumber(metrics.HealingRate)
                    : "HEAL: " + FormatNumber(metrics.Healing));
            return;
        }
        GUILayout.Label(_mode == DisplayMode.CurrentDps
            ? "HPS: " + FormatNumber(metrics.HealingRate)
            : "HEAL: " + FormatNumber(metrics.Healing), _headerRight);
        float max = GetSelectedRunMax(rows);
        if (rows.Count == 0)
        {
            GUILayout.Label("No healing recorded yet.", _small);
            return;
        }
        for (int i = 0; i < rows.Count; i++)
            DrawHealingRow(rows[i].Name, rows[i].Amount, metrics.Healing, max, ResolveRunHistoryIcon(rows[i]));
    }

    private void DrawSelectedRunBarrier(DpsData.RunRecord run)
    {
        if (run.ViewRows == null || run.ViewRows.Count == 0)
        {
            DrawLegacySelectedRunBarrier(run);
            return;
        }
        string view = GetSelectedRunViewName();
        DpsData.RunViewMetrics metrics = GetSelectedRunMetrics(run, view);
        if (metrics == null) return;
        List<DpsData.RunBreakdownRow> rows = GetSelectedRunViewRows(run, view, "BARRIER");
        if (_mode == DisplayMode.PartyDps || _mode == DisplayMode.PartyTotal)
        {
            DrawParty(ToPartyRows(rows), metrics.Barrier, metrics.BarrierRate, "BPS",
                _mode == DisplayMode.PartyDps
                    ? "BPS: " + FormatNumber(metrics.BarrierRate)
                    : "BARRIER: " + FormatNumber(metrics.Barrier));
            return;
        }
        GUILayout.Label(_mode == DisplayMode.CurrentDps
            ? "BPS: " + FormatNumber(metrics.BarrierRate)
            : "BARRIER: " + FormatNumber(metrics.Barrier), _headerRight);
        float max = GetSelectedRunMax(rows);
        if (rows.Count == 0)
        {
            GUILayout.Label("No barrier generated yet.", _small);
            return;
        }
        for (int i = 0; i < rows.Count; i++)
            DrawBarrierRow(rows[i].Name, rows[i].Amount, metrics.Barrier, max, ResolveRunHistoryIcon(rows[i]));
    }

    private void DrawLegacySelectedRunHealing(DpsData.RunRecord run)
    {
        List<DpsData.RunBreakdownRow> rows = GetSelectedRunRows(run, "HEALING");
        float total = run.TotalHealing > 0f ? run.TotalHealing : GetSelectedRunTotal(rows);
        GUILayout.Label("HEAL: " + FormatNumber(total), _headerRight);
        float max = GetSelectedRunMax(rows);
        if (rows.Count == 0)
        {
            GUILayout.Label("No healing recorded for this run.", _small);
            return;
        }
        for (int i = 0; i < rows.Count; i++)
            DrawHealingRow(rows[i].Name, rows[i].Amount, total, max, ResolveRunHistoryIcon(rows[i]));
    }

    private void DrawLegacySelectedRunBarrier(DpsData.RunRecord run)
    {
        List<DpsData.RunBreakdownRow> rows = GetSelectedRunRows(run, "BARRIER");
        float total = run.TotalBarrier > 0f ? run.TotalBarrier : GetSelectedRunTotal(rows);
        GUILayout.Label("BARRIER: " + FormatNumber(total), _headerRight);
        float max = GetSelectedRunMax(rows);
        if (rows.Count == 0)
        {
            GUILayout.Label("No barrier generated for this run.", _small);
            return;
        }
        for (int i = 0; i < rows.Count; i++)
            DrawBarrierRow(rows[i].Name, rows[i].Amount, total, max, ResolveRunHistoryIcon(rows[i]));
    }

    private static Type FindLoadedType(string fullName)
    {
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        for (int i = 0; i < assemblies.Length; i++)
        {
            Type type = assemblies[i].GetType(fullName, false);
            if (type != null)
            {
                return type;
            }
        }

        return null;
    }

    private UiInputBlocker CreateUiInputBlocker(Transform parent, Type imageType)
    {
        GameObject blockerObject = new GameObject(
            "DPS Meter Input Blocker",
            typeof(RectTransform));
        blockerObject.transform.SetParent(parent, false);

        Component image = blockerObject.AddComponent(imageType);
        PropertyInfo raycastTarget = imageType.GetProperty("raycastTarget");
        PropertyInfo color = imageType.GetProperty("color");
        if (raycastTarget == null || color == null)
        {
            throw new MissingMemberException("Unity UI Image raycast properties were not found.");
        }

        raycastTarget.SetValue(image, true, null);
        color.SetValue(image, new Color(0f, 0f, 0f, 0f), null);

        RectTransform rectTransform = blockerObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0f, 1f);
        rectTransform.anchorMax = new Vector2(0f, 1f);
        rectTransform.pivot = new Vector2(0f, 1f);

        return new UiInputBlocker
        {
            GameObject = blockerObject,
            RectTransform = rectTransform
        };
    }

    private void EnsureUiInputBlockers()
    {
        if (_uiInputBlockerInitializationAttempted)
        {
            return;
        }

        _uiInputBlockerInitializationAttempted = true;

        try
        {
            Type canvasType = FindLoadedType("UnityEngine.Canvas");
            Type raycasterType = FindLoadedType("UnityEngine.UI.GraphicRaycaster");
            Type imageType = FindLoadedType("UnityEngine.UI.Image");
            if (canvasType == null || raycasterType == null || imageType == null)
            {
                throw new TypeLoadException("Unity Canvas, GraphicRaycaster, or Image type was not found.");
            }

            _uiInputCanvasObject = new GameObject(
                "DPS Meter Input Blockers",
                typeof(RectTransform));
            Component canvas = _uiInputCanvasObject.AddComponent(canvasType);
            PropertyInfo renderMode = canvasType.GetProperty("renderMode");
            PropertyInfo sortingOrder = canvasType.GetProperty("sortingOrder");
            if (renderMode == null || sortingOrder == null)
            {
                throw new MissingMemberException("Unity Canvas render properties were not found.");
            }

            renderMode.SetValue(canvas, Enum.Parse(renderMode.PropertyType, "ScreenSpaceOverlay"), null);
            sortingOrder.SetValue(canvas, 32767, null);
            _uiInputCanvasObject.AddComponent(raycasterType);

            _mainWindowInputBlocker = CreateUiInputBlocker(_uiInputCanvasObject.transform, imageType);
            _historyInputBlocker = CreateUiInputBlocker(_uiInputCanvasObject.transform, imageType);
            _settingsInputBlocker = CreateUiInputBlocker(_uiInputCanvasObject.transform, imageType);
            _contextMenuInputBlocker = CreateUiInputBlocker(_uiInputCanvasObject.transform, imageType);

            if (DPSMeter.Instance != null)
            {
                DPSMeter.Instance.WriteOverlayInputDiagnostic("initialized");
            }
        }
        catch (Exception ex)
        {
            if (_uiInputCanvasObject != null)
            {
                Destroy(_uiInputCanvasObject);
                _uiInputCanvasObject = null;
            }

            _mainWindowInputBlocker = null;
            _historyInputBlocker = null;
            _settingsInputBlocker = null;
            _contextMenuInputBlocker = null;

            if (!_uiInputBlockerFailureLogged)
            {
                _uiInputBlockerFailureLogged = true;
                if (DPSMeter.Instance != null)
                {
                    DPSMeter.Instance.WriteOverlayInputDiagnostic(
                        "initialization failed: " + ex.GetType().Name);
                }
            }
        }
    }

    private static void SetUiInputBlocker(UiInputBlocker blocker, bool visible, Rect screenRect)
    {
        if (blocker == null || blocker.GameObject == null)
        {
            return;
        }

        if (blocker.GameObject.activeSelf != visible)
        {
            blocker.GameObject.SetActive(visible);
        }

        if (!visible)
        {
            return;
        }

        blocker.RectTransform.anchoredPosition = new Vector2(screenRect.x, -screenRect.y);
        blocker.RectTransform.sizeDelta = new Vector2(
            Mathf.Max(0f, screenRect.width),
            Mathf.Max(0f, screenRect.height));
    }

    private void UpdateUiInputBlockers()
    {
        EnsureUiInputBlockers();
        if (_uiInputCanvasObject == null)
        {
            return;
        }

        bool visible = Visible && _data != null;
        SetUiInputBlocker(_mainWindowInputBlocker, visible,
            visible ? _windowRect : new Rect());
        SetUiInputBlocker(_historyInputBlocker, visible && _showRunHistory,
            visible && _showRunHistory ? GetRunHistoryPanelRect() : new Rect());
        SetUiInputBlocker(_settingsInputBlocker, visible && _settingsOpen,
            visible && _settingsOpen ? _settingsRect : new Rect());
        SetUiInputBlocker(_contextMenuInputBlocker, visible && _contextMenuOpen,
            visible && _contextMenuOpen ? _contextMenuRect : new Rect());
    }

    private Rect GetRunHistoryPanelRect()
    {
        float panelX = Mathf.Clamp(_windowRect.xMax + 4f, 4f,
            Mathf.Max(4f, Screen.width - HistoryPanelWidth - 4f));
        float panelY = Mathf.Clamp(_windowRect.y, 4f,
            Mathf.Max(4f, Screen.height - HistoryPanelHeight - 4f));
        return new Rect(panelX, panelY, HistoryPanelWidth, HistoryPanelHeight);
    }

    private bool IsPointerOverUi(Vector2 position, bool includeContextMenu)
    {
        if (_windowRect.Contains(position))
        {
            return true;
        }

        if (_showRunHistory && GetRunHistoryPanelRect().Contains(position))
        {
            return true;
        }

        if (_settingsOpen && _settingsRect.Contains(position))
        {
            return true;
        }

        return includeContextMenu && _contextMenuOpen &&
            _contextMenuRect.Contains(position);
    }

    private static string FormatDuration(float seconds)
    {
        int totalSeconds = Mathf.Max(0, Mathf.FloorToInt(seconds));
        int hours = totalSeconds / 3600;
        int minutes = (totalSeconds / 60) % 60;
        int remainingSeconds = totalSeconds % 60;
        return hours > 0
            ? hours + ":" + minutes.ToString("00") + ":" + remainingSeconds.ToString("00")
            : minutes + ":" + remainingSeconds.ToString("00");
    }

    private void HandleWindowInput()
    {
        Event e = Event.current;

        Rect headerRect = new Rect(
            _windowRect.x + 6f,
            _windowRect.y + 4f,
            _windowRect.width - 12f,
            24f);

        Rect resizeRect = new Rect(
            _windowRect.xMax - 16f,
            _windowRect.yMax - 16f,
            16f,
            16f);

        bool rightClick =
            e.type == EventType.MouseDown && e.button == 1;

        if (rightClick)
        {
            if (_contextMenuOpen)
            {
                if (_contextMenuRect.Contains(e.mousePosition))
                {
                    return;
                }

                _contextMenuOpen = false;
                if (IsPointerOverUi(e.mousePosition, false))
                {
                    e.Use();
                    return;
                }
            }

            if (_settingsOpen && !_settingsRect.Contains(e.mousePosition))
            {
                _settingsOpen = false;
                e.Use();
                return;
            }

            if (_contextMenuOpen && _contextMenuRect.Contains(e.mousePosition))
            {
                return;
            }

            if (_windowRect.Contains(e.mousePosition))
            {
                const float menuWidth = 100f;
                const float menuHeight = 120f;
                const float menuGap = 4f;

                _contextMenuOpen = false;

                float x = Mathf.Clamp(
                    e.mousePosition.x + menuGap,
                    4f,
                    Mathf.Max(4f, Screen.width - menuWidth - 4f));
                float y = Mathf.Clamp(
                    e.mousePosition.y + menuGap,
                    4f,
                    Mathf.Max(4f, Screen.height - menuHeight - 4f));

                _contextMenuRect = new Rect(x, y, menuWidth, menuHeight);
                _contextMenuOpen = true;
                e.Use();
                return;
            }

            _contextMenuOpen = false;
        }
        if (_settingsOpen)
        {
            Rect healingToggleRect = new Rect(
                _settingsRect.x + 12f,
                _settingsRect.y + 32f,
                _settingsRect.width - 24f,
                26f);

            Rect barrierToggleRect = new Rect(
                _settingsRect.x + 12f,
                _settingsRect.y + 58f,
                _settingsRect.width - 24f,
                26f);

            Rect closeRect = new Rect(
                _settingsRect.xMax - 72f,
                _settingsRect.yMax - 30f,
                60f,
                22f);

            if (e.type == EventType.MouseDown && e.button == 0)
            {
                if (_settingsRect.Contains(e.mousePosition)
                    && !healingToggleRect.Contains(e.mousePosition)
                    && !barrierToggleRect.Contains(e.mousePosition)
                    && !closeRect.Contains(e.mousePosition))
                {
                    _settingsDragging = true;
                    _settingsDragOffset = e.mousePosition - _settingsRect.position;
                    e.Use();
                    return;
                }

                return;
            }

            if (e.type == EventType.MouseDrag && e.button == 0 && _settingsDragging)
            {
                Vector2 nextPosition = e.mousePosition - _settingsDragOffset;

                _settingsRect.x = Mathf.Clamp(
                    nextPosition.x,
                    4f,
                    Mathf.Max(4f, Screen.width - _settingsRect.width - 4f));

                _settingsRect.y = Mathf.Clamp(
                    nextPosition.y,
                    4f,
                    Mathf.Max(4f, Screen.height - _settingsRect.height - 4f));

                e.Use();
                return;
            }

            if (e.type == EventType.MouseUp && e.button == 0 && _settingsDragging)
            {
                _settingsDragging = false;
                e.Use();
            }

            return;
        }

        if (_contextMenuOpen && e.type == EventType.MouseDown)
        {
            if (_contextMenuRect.Contains(e.mousePosition))
            {
                return;
            }

            _contextMenuOpen = false;

            // Clicking another part of the meter dismisses the menu without
            // activating the control underneath it. Outside clicks go to the game.
            if (IsPointerOverUi(e.mousePosition, false))
            {
                e.Use();
                return;
            }
        }

        if (e.type == EventType.MouseDown && e.button == 0)
        {
            if (resizeRect.Contains(e.mousePosition))
            {
                _resizing = true;
                _resizeStartMouse = e.mousePosition;

                float resizeBreakdownHeight = GetExpandedBreakdownHeight();

                _resizeStartSize = new Vector2(
                    _windowRect.width,
                    _collapsedWindowHeight);
                _resizeStartHealingHeight = resizeBreakdownHeight;
                _resizeMoved = false;

                e.Use();
                return;
            }

            if (headerRect.Contains(e.mousePosition))
            {
                _dragging = true;
                _headerMoved = false;
                _dragOffset = e.mousePosition - _windowRect.position;
                e.Use();
                return;
            }
        }

        if (e.type == EventType.MouseDrag && e.button == 0)
        {
            if (_resizing)
            {
                Vector2 delta = e.mousePosition - _resizeStartMouse;
                _resizeMoved = true;
                _windowRect.width = Mathf.Clamp(
                    _resizeStartSize.x + delta.x,
                    260f,
                    Mathf.Max(260f, Screen.width - _windowRect.x - 10f));

                _collapsedWindowHeight = Mathf.Clamp(
                    _resizeStartSize.y + delta.y,
                    90f,
                    Mathf.Max(90f, Screen.height - _windowRect.y - 10f));

                _windowRect.height = _collapsedWindowHeight + _resizeStartHealingHeight;

                e.Use();
                return;
            }

            if (_dragging)
            {
                Vector2 nextPosition = e.mousePosition - _dragOffset;

                if (Vector2.Distance(nextPosition, _windowRect.position) > 2f)
                {
                    _headerMoved = true;
                }

                _windowRect.x = Mathf.Clamp(
                    nextPosition.x,
                    0f,
                    Mathf.Max(0f, Screen.width - _windowRect.width));

                _windowRect.y = Mathf.Clamp(
                    nextPosition.y,
                    0f,
                    Mathf.Max(0f, Screen.height - _windowRect.height));

                e.Use();
                return;
            }
        }

        if (e.type == EventType.MouseUp && e.button == 0)
        {
            if (_resizing)
            {
                _resizing = false;

                if (_resizeMoved)
                {
                    _manualResize = true;
                    Vector2 delta = e.mousePosition - _resizeStartMouse;
                    _collapsedWindowHeight = Mathf.Clamp(
                        _resizeStartSize.y + delta.y,
                        90f,
                        Mathf.Max(90f, Screen.height - _windowRect.y - 10f));

                    _windowRect.width = Mathf.Clamp(
                        _resizeStartSize.x + delta.x,
                        260f,
                        Mathf.Max(260f, Screen.width - _windowRect.x - 10f));
                }

                _windowRect.height = _collapsedWindowHeight + _resizeStartHealingHeight;

                e.Use();
                return;
            }

            if (_dragging)
            {
                if (!_headerMoved)
                {
                    _mode = (DisplayMode)(((int)_mode + 1) % 4);
                }

                _dragging = false;
                e.Use();
            }
        }
    }

    private void UpdateWindowHeight()
    {
        if (!_manualResize && (_mode == DisplayMode.CurrentDps || _mode == DisplayMode.DamageTotal))
        {
            int rowCount = GetCurrentDpsRowCount();
            float desiredHeight = 50f + (rowCount * 22f) + (rowCount > 0 ? 2f : 0f);
            float maxCollapsedHeight = Mathf.Max(90f, Screen.height - _windowRect.y - 10f);

            _collapsedWindowHeight = Mathf.Min(
                Mathf.Max(197f, desiredHeight),
                maxCollapsedHeight);
        }

        float breakdownHeight = GetExpandedBreakdownHeight();
        float maxHeight = Mathf.Max(
            _collapsedWindowHeight,
            Screen.height - _windowRect.y - 10f);

        _windowRect.height = Mathf.Min(
            _collapsedWindowHeight + breakdownHeight,
            maxHeight);
    }

    private int GetCurrentDpsRowCount()
    {
        IReadOnlyList<DpsData.BreakdownRow> skills =
            _mode == DisplayMode.CurrentDps
                ? _data.CurrentPersonalSkillRows
                : _data.CumulativePersonalSkillRows;
        IReadOnlyList<KeyValuePair<string, float>> other =
            _mode == DisplayMode.CurrentDps
                ? _data.CurrentPersonalOther
                : _data.CumulativePersonalOther;
        IReadOnlyList<KeyValuePair<string, float>> essences =
            _mode == DisplayMode.CurrentDps
                ? _data.CurrentPersonalEssences
                : _data.CumulativePersonalEssences;

        int count = 0;
        if (skills != null)
            count += skills.Count;
        if (other != null)
            count += other.Count;
        if (essences != null)
        {
            for (int i = 0; i < essences.Count; i++)
            {
                if (!string.IsNullOrEmpty(essences[i].Key) && essences[i].Value > 0f)
                    count++;
            }
        }

        return count > 0 ? count : 1;
    }

    private float GetExpandedBreakdownHeight()
    {
        if (_mode != DisplayMode.CurrentDps && _mode != DisplayMode.DamageTotal &&
            _mode != DisplayMode.PartyDps && _mode != DisplayMode.PartyTotal)
        {
            return 0f;
        }

        bool partyMode = _mode == DisplayMode.PartyDps || _mode == DisplayMode.PartyTotal;
        float height = 0f;

        if (!_showHealing && !_showBarrier)
        {
            return 0f;
        }

        height = 16f;

        if (_showHealing)
        {
            int rowCount;
            if (partyMode)
            {
                IReadOnlyList<KeyValuePair<string, float>> rows =
                    _mode == DisplayMode.PartyDps ? _data.CurrentPartyHealing : _data.CumulativePartyHealingRows;
                rowCount = rows != null ? rows.Count : 0;
            }
            else
            {
                IReadOnlyList<DpsData.BreakdownRow> rows =
                    _mode == DisplayMode.CurrentDps ? _data.CurrentPersonalHealingRows : _data.CumulativeHealingRows;
                rowCount = rows != null ? rows.Count : 0;
            }
            height += 22f + (Mathf.Max(1, rowCount) * 22f);
        }

        if (_showBarrier)
        {
            int rowCount;
            if (partyMode)
            {
                IReadOnlyList<KeyValuePair<string, float>> rows =
                    _mode == DisplayMode.PartyDps ? _data.CurrentPartyBarrier : _data.CumulativePartyBarrierRows;
                rowCount = rows != null ? rows.Count : 0;
            }
            else
            {
                IReadOnlyList<DpsData.BreakdownRow> rows =
                    _mode == DisplayMode.CurrentDps ? _data.CurrentPersonalBarrierRows : _data.CumulativeBarrierRows;
                rowCount = rows != null ? rows.Count : 0;
            }
            height += 22f + (Mathf.Max(1, rowCount) * 22f);
        }

        if (_showHealing != _showBarrier)
        {
            height += 1.36f;
        }

        // Keep a small visual gap below the final barrier row.
        height += 2f;

        return height;
    }

    private void DrawHeader(Rect headerRect)
    {
        string title;
        string metric;

        switch (_mode)
        {
            case DisplayMode.CurrentDps:
                title = "CURRENT";
                metric = "DPS: " + FormatNumber(_data.CurrentPersonalDps);
                break;

            case DisplayMode.DamageTotal:
                title = "TOTAL";
                metric = "DMG: " + FormatNumber(_data.CumulativePersonalDamage);
                break;

            case DisplayMode.PartyDps:
                title = "PARTY DPS";
                metric = "DPS: " + FormatNumber(_data.CurrentPartyDps);
                break;

            case DisplayMode.PartyTotal:
                title = "PARTY TOTAL";
                metric = "DMG: " + FormatNumber(_data.CumulativePartyDamage);
                break;

            default:
                title = "TOTAL";
                metric = "DMG: " + FormatNumber(_data.CumulativePersonalDamage);
                break;
        }

        if (_selectedRunRecord != null)
        {
            DpsData.RunViewMetrics selectedMetrics = GetSelectedRunMetrics(_selectedRunRecord, GetSelectedRunViewName());
            switch (_mode)
            {
                case DisplayMode.CurrentDps:
                    title = "CURRENT";
                    metric = "DPS: " + FormatNumber(selectedMetrics == null ? 0f : selectedMetrics.DamageRate);
                    break;
                case DisplayMode.DamageTotal:
                    title = "TOTAL";
                    metric = "DMG: " + FormatNumber(selectedMetrics == null ? 0f : selectedMetrics.Damage);
                    break;
                case DisplayMode.PartyDps:
                    title = "PARTY DPS";
                    metric = "DPS: " + FormatNumber(selectedMetrics == null ? 0f : selectedMetrics.DamageRate);
                    break;
                case DisplayMode.PartyTotal:
                    title = "PARTY TOTAL";
                    metric = "DMG: " + FormatNumber(selectedMetrics == null ? 0f : selectedMetrics.Damage);
                    break;
            }
        }

        float metricWidth = headerRect.width * 0.45f;
        float titleWidth = headerRect.width - metricWidth;

        GUI.Label(
            new Rect(headerRect.x, headerRect.y, titleWidth, headerRect.height),
            title,
            _header);

        GUI.Label(
            new Rect(
                headerRect.x + titleWidth,
                headerRect.y,
                metricWidth,
                headerRect.height),
            metric,
            _headerRight);

        if (_mode == DisplayMode.CurrentDps)
        {
            GUIStyle versionStyle = new GUIStyle(_small)
            {
                fontSize = 8,
                alignment = TextAnchor.LowerRight,
                normal = { textColor = new Color(0.68f, 0.68f, 0.70f, 0.78f) }
            };

            GUI.Label(
                new Rect(
                    _windowRect.x + 6f,
                    _windowRect.yMax - 15f,
                    _windowRect.width - 25f,
                    11f),
                DPSMeter.DevelopmentVersion,
                versionStyle);
        }
    }

    private void DrawContextMenu()
    {
        GUI.Box(_contextMenuRect, GUIContent.none);

        const float padding = 4f;
        const float rowHeight = 28f;
        const float rowWidth = 92f;

        Rect settingsRect = new Rect(
            _contextMenuRect.x + padding,
            _contextMenuRect.y + padding,
            rowWidth,
            rowHeight);

        Rect reloadRect = new Rect(
            settingsRect.x,
            settingsRect.yMax,
            rowWidth,
            rowHeight);

        Rect exportRect = new Rect(
            reloadRect.x,
            reloadRect.yMax,
            rowWidth,
            rowHeight);

        Rect historyRect = new Rect(
            exportRect.x,
            exportRect.yMax,
            rowWidth,
            rowHeight);

        Event e = Event.current;

        DrawContextMenuRow(settingsRect, "Settings");
        DrawContextMenuRow(reloadRect, "Reload");
        DrawContextMenuRow(exportRect, "Export");
        DrawContextMenuRow(historyRect, "Run History");

        if (e.type == EventType.MouseDown && e.button == 0)
        {
            if (settingsRect.Contains(e.mousePosition))
            {
                _contextMenuOpen = false;
                OpenSettingsWindow();
                e.Use();
            }
            else if (reloadRect.Contains(e.mousePosition))
            {
                _contextMenuOpen = false;
                DewMod.ReloadFromActiveMods();
                e.Use();
            }
            else if (exportRect.Contains(e.mousePosition))
            {
                _contextMenuOpen = false;
                ExportHealingLog();
                e.Use();
            }
            else if (historyRect.Contains(e.mousePosition))
            {
                _contextMenuOpen = false;
                _showRunHistory = !_showRunHistory;
                e.Use();
            }
        }
    }

    private void DrawContextMenuRow(Rect rect, string label)
    {
        if (rect.Contains(Event.current.mousePosition))
        {
            if (_contextMenuHighlightTexture == null)
            {
                _contextMenuHighlightTexture = new Texture2D(1, 1);
                _contextMenuHighlightTexture.SetPixel(0, 0, new Color(1f, 1f, 1f, 0.10f));
                _contextMenuHighlightTexture.Apply();
            }

            GUI.DrawTexture(rect, _contextMenuHighlightTexture);
        }

        GUI.Label(rect, label);
    }

    private void OpenSettingsWindow()
    {
        const float gap = 8f;

        float x = _windowRect.xMax + gap;
        if (x + _settingsRect.width > Screen.width - 4f)
        {
            x = _windowRect.x - _settingsRect.width - gap;
        }

        x = Mathf.Clamp(
            x,
            4f,
            Mathf.Max(4f, Screen.width - _settingsRect.width - 4f));

        float y = Mathf.Clamp(
            _windowRect.y,
            4f,
            Mathf.Max(4f, Screen.height - _settingsRect.height - 4f));

        _settingsRect = new Rect(
            x,
            y,
            _settingsRect.width,
            _settingsRect.height);

        _settingsOpen = true;
        _settingsDragging = false;
    }

    private void DrawSettingsWindow()
    {
        GUI.Box(_settingsRect, "Settings");

        const float left = 12f;
        const float top = 32f;
        const float rowHeight = 26f;

        _showHealing = GUI.Toggle(
            new Rect(_settingsRect.x + left, _settingsRect.y + top, _settingsRect.width - 24f, rowHeight),
            _showHealing,
            "Expand Healing Breakdown");

        _showBarrier = GUI.Toggle(
            new Rect(_settingsRect.x + left, _settingsRect.y + top + rowHeight, _settingsRect.width - 24f, rowHeight),
            _showBarrier,
            "Expand Barrier Breakdown");

        if (GUI.Button(
            new Rect(
                _settingsRect.xMax - 72f,
                _settingsRect.yMax - 30f,
                60f,
                22f),
            "Close"))
        {
            _settingsOpen = false;
        }
    }

    private void ExportHealingLog()
    {
        try
        {
            string path = Path.Combine(
                Application.persistentDataPath,
                "DPSMeter-healing-export.log");

            File.WriteAllText(path, _data.ExportHealingLog());
        }
        catch (Exception)
        {
        }
    }

    private sealed class DamageRow
    {
        public string Name;
        public float Amount;
        public ElementalType? Elemental;
        public DpsData.DamageScalingType Scaling;
        public Sprite Icon;
    }

    private void DrawPersonal(
        IReadOnlyList<DpsData.BreakdownRow> sources,
        IReadOnlyList<KeyValuePair<string, float>> other,
        IReadOnlyList<KeyValuePair<string, float>> essences,
        float total,
        bool cumulativeEssences)
    {
        List<DamageRow> rows = new List<DamageRow>(sources.Count + other.Count + essences.Count);

        for (int i = 0; i < sources.Count; i++)
        {
            DpsData.BreakdownRow row = sources[i];
            rows.Add(new DamageRow
            {
                Name = StripRichTextTags(row.Name),
                Amount = row.Amount,
                Elemental = _data.GetCurrentSkillElement(row.Identity),
                Scaling = _data.GetCurrentSkillScaling(row.Identity),
                Icon = _data.GetSkillIcon(row.Identity)
            });
        }

        for (int i = 0; i < other.Count; i++)
        {
            KeyValuePair<string, float> row = other[i];
            rows.Add(new DamageRow
            {
                Name = StripRichTextTags(row.Key),
                Amount = row.Value,
                Scaling = _data.GetCurrentOtherScaling(row.Key),
                Icon = row.Key == "Basic Attack" ? GetBasicAttackIcon() : _data.GetCurrentOtherIcon(row.Key)
            });
        }

        for (int i = 0; i < essences.Count; i++)
        {
            KeyValuePair<string, float> row = essences[i];
            if (string.IsNullOrEmpty(row.Key) || row.Value <= 0f)
                continue;

            rows.Add(new DamageRow
            {
                Name = StripRichTextTags(_data.GetEssenceDisplayName(row.Key)),
                Amount = row.Value,
                Elemental = cumulativeEssences
                    ? _data.GetCumulativeEssenceElement(row.Key)
                    : _data.GetCurrentEssenceElement(row.Key),
                Scaling = cumulativeEssences
                    ? _data.GetCumulativeEssenceScaling(row.Key)
                    : _data.GetCurrentEssenceScaling(row.Key),
                Icon = cumulativeEssences
                    ? _data.GetCumulativeEssenceIcon(row.Key)
                    : _data.GetCurrentEssenceIcon(row.Key)
            });
        }

        rows.Sort((a, b) => b.Amount.CompareTo(a.Amount));

        if (rows.Count == 0)
        {
            GUILayout.Label("No damage recorded yet.", _small);
            return;
        }

        float maxAmount = rows[0].Amount;
        for (int i = 0; i < rows.Count; i++)
        {
            DamageRow row = rows[i];
            DrawDamageRow(row.Name, row.Amount, total, maxAmount, i, row.Elemental, row.Scaling, row.Icon);
        }
    }

    private void DrawBarrierBreakdown(IReadOnlyList<DpsData.BreakdownRow> rows, float total)
    {
        float bps = _mode == DisplayMode.CurrentDps
            ? _data.CurrentPersonalBps
            : _data.CumulativePersonalBarrier;

        string barrierLabel = _mode == DisplayMode.CurrentDps
            ? "BPS: " + FormatNumber(bps)
            : "BARRIER: " + FormatNumber(_data.CumulativePersonalBarrier);

        GUILayout.Label(
            barrierLabel,
            _headerRight);

        DrawBarrierSources(rows, total);
    }

    private void DrawBarrierSources(IReadOnlyList<DpsData.BreakdownRow> rows, float total)
    {
        if (rows == null || rows.Count == 0)
        {
            GUILayout.Label("No barrier generated yet.", _small);
            return;
        }

        float maxAmount = rows[0].Amount;
        bool cumulative = _mode == DisplayMode.DamageTotal;

        for (int i = 0; i < rows.Count; i++)
        {
            DpsData.BreakdownRow row = rows[i];
            Sprite icon = cumulative
                ? _data.GetCumulativeBarrierIcon(row.Identity)
                : _data.GetCurrentBarrierIcon(row.Identity);

            DrawBarrierRow(row.Name, row.Amount, total, maxAmount, icon);
        }
    }

    private void DrawBarrierRow(string name, float amount, float total, float maxAmount, Sprite icon)
    {
        float ratio = maxAmount > 0f ? Mathf.Clamp01(amount / maxAmount) : 0f;
        float percent = total > 0f ? Mathf.Clamp01(amount / total) * 100f : 0f;

        Rect rowRect = GUILayoutUtility.GetRect(0f, 22f, GUILayout.ExpandWidth(true));
        rowRect.x = Mathf.Round(rowRect.x);
        rowRect.y = Mathf.Round(rowRect.y);
        rowRect.width = Mathf.Round(rowRect.width);
        rowRect.height = Mathf.Round(rowRect.height);

        float iconSize = rowRect.height;
        float barX = rowRect.x;

        if (icon != null)
        {
            Rect iconRect = new Rect(rowRect.x, rowRect.y, iconSize, iconSize);
            DrawSprite(icon, iconRect);
            barX = iconRect.xMax;
        }

        Rect barRect = new Rect(
            barX,
            rowRect.y,
            Mathf.Max(0f, rowRect.xMax - barX),
            rowRect.height);

        GUI.color = new Color(0.10f, 0.10f, 0.10f, 0.75f);
        GUI.DrawTexture(barRect, _whiteTexture);

        GUI.color = new Color(0.824f, 0.831f, 0.827f, 1f);
        Rect fillRect = new Rect(
            barRect.x,
            barRect.y,
            barRect.width * ratio,
            barRect.height);
        GUI.DrawTexture(fillRect, _whiteTexture);

        const float barrierOutline = 2f;
        GUI.color = new Color(0.655f, 0.659f, 0.663f, 1f);
        GUI.DrawTexture(new Rect(fillRect.x, fillRect.y, fillRect.width, barrierOutline), _whiteTexture);
        GUI.DrawTexture(new Rect(fillRect.x, fillRect.yMax - barrierOutline, fillRect.width, barrierOutline), _whiteTexture);
        GUI.DrawTexture(new Rect(fillRect.x, fillRect.y, barrierOutline, fillRect.height), _whiteTexture);
        GUI.DrawTexture(new Rect(fillRect.xMax - barrierOutline, fillRect.y, barrierOutline, fillRect.height), _whiteTexture);

        string valueText = FormatNumber(amount) + "  " + percent.ToString("0.0") + "%";
        float valueWidth = _rowRight.CalcSize(new GUIContent(valueText)).x;
        float valueRight = rowRect.xMax - 7f;
        float valueLeft = Mathf.Max(barRect.x + 7f, valueRight - valueWidth);
        float nameWidth = Mathf.Max(0f, valueLeft - (barRect.x + 7f) - 7f - _row.CalcSize(new GUIContent(" ")).x);

        GUI.color = SourceNameColor;
        DrawBarTextWithStroke(
            new Rect(barRect.x + 7f, rowRect.y, nameWidth, rowRect.height),
            TruncateTextToWidth(StripRichTextTags(name), nameWidth, _row),
            _row);
        DrawBarTextWithStroke(
            new Rect(valueLeft, rowRect.y, Mathf.Max(0f, valueRight - valueLeft), rowRect.height),
            valueText,
            _rowRight);

        GUI.color = Color.white;
    }

    private void DrawHealingBreakdown(IReadOnlyList<DpsData.BreakdownRow> rows, float total)
    {
        float hps = _mode == DisplayMode.CurrentDps
            ? _data.CurrentPersonalHps
            : _data.TotalPersonalHps;

        string healingLabel = _mode == DisplayMode.CurrentDps
            ? "HPS: " + FormatNumber(hps)
            : "HEAL: " + FormatNumber(_data.CumulativePersonalHealing);

        GUILayout.Label(
            healingLabel,
            _headerRight);

        DrawHealingSources(rows, total);
    }

    private void DrawHealingSources(IReadOnlyList<DpsData.BreakdownRow> rows, float total)
    {
        if (rows == null || rows.Count == 0)
        {
            GUILayout.Label("No healing recorded yet.", _small);
            return;
        }

        float maxAmount = rows[0].Amount;
        for (int i = 0; i < rows.Count; i++)
        {
            DpsData.BreakdownRow row = rows[i];
            Sprite icon = _mode == DisplayMode.DamageTotal
                ? _data.GetCumulativeHealingIcon(row.Identity)
                : _data.GetCurrentHealingIcon(row.Identity);
            DrawHealingRow(row.Name, row.Amount, total, maxAmount, icon);
        }
    }

    private void DrawHealingRow(string name, float amount, float total, float maxAmount, Sprite icon)
    {
        float ratio = maxAmount > 0f ? Mathf.Clamp01(amount / maxAmount) : 0f;
        float percent = total > 0f ? Mathf.Clamp01(amount / total) * 100f : 0f;

        Rect rowRect = GUILayoutUtility.GetRect(0f, 22f, GUILayout.ExpandWidth(true));
        rowRect.x = Mathf.Round(rowRect.x);
        rowRect.y = Mathf.Round(rowRect.y);
        rowRect.width = Mathf.Round(rowRect.width);
        rowRect.height = Mathf.Round(rowRect.height);

        float iconSize = rowRect.height;
        Rect iconRect = new Rect(rowRect.x, rowRect.y, iconSize, iconSize);
        float barX = iconRect.xMax;

        if (icon != null)
        {
            DrawSprite(icon, iconRect);
        }
        else
        {
            // Keep healing rows aligned when the game does not expose an icon.
            GUI.color = new Color(0.10f, 0.10f, 0.10f, 0.75f);
            GUI.DrawTexture(iconRect, _whiteTexture);
            GUI.color = Color.white;
        }

        Rect barRect = new Rect(
            barX,
            rowRect.y,
            Mathf.Max(0f, rowRect.xMax - barX),
            rowRect.height);

        GUI.color = new Color(0.10f, 0.10f, 0.10f, 0.75f);
        GUI.DrawTexture(barRect, _whiteTexture);

        DrawBar(barRect, ratio, HealingBarColor, null, DpsData.DamageScalingType.Hp);

        string valueText = FormatNumber(amount) + "  " + percent.ToString("0.0") + "%";
        float valueWidth = _rowRight.CalcSize(new GUIContent(valueText)).x;
        float valueRight = rowRect.xMax - 7f;
        float valueLeft = Mathf.Max(barRect.x + 7f, valueRight - valueWidth);
        float nameWidth = Mathf.Max(0f, valueLeft - (barRect.x + 7f) - 7f - _row.CalcSize(new GUIContent(" ")).x);

        GUI.color = SourceNameColor;
        DrawBarTextWithStroke(
            new Rect(barRect.x + 7f, rowRect.y, nameWidth, rowRect.height),
            TruncateTextToWidth(StripRichTextTags(name), nameWidth, _row),
            _row);
        DrawBarTextWithStroke(
            new Rect(valueLeft, rowRect.y, Mathf.Max(0f, valueRight - valueLeft), rowRect.height),
            valueText,
            _rowRight);

        GUI.color = Color.white;
    }

    private void DrawParty(
        IReadOnlyList<KeyValuePair<string, float>> rows,
        float total,
        float rate,
        string rateLabel,
        string sectionLabel = null)
    {
        if (!string.IsNullOrEmpty(sectionLabel))
        {
            GUILayout.Label(sectionLabel, _headerRight);
        }

        if (rows == null || rows.Count == 0)
        {
            GUILayout.Label("No party data recorded yet.", _small);
            return;
        }

        float duration = rate > 0f && total > 0f ? total / rate : 0f;
        float maxAmount = rows[0].Value;

        for (int i = 0; i < rows.Count; i++)
        {
            KeyValuePair<string, float> row = rows[i];
            float playerRate = duration > 0f ? row.Value / duration : 0f;
            float percent = total > 0f ? Mathf.Clamp01(row.Value / total) * 100f : 0f;
            float ratio = maxAmount > 0f ? Mathf.Clamp01(row.Value / maxAmount) : 0f;

            Rect rowRect = GUILayoutUtility.GetRect(0f, 22f, GUILayout.ExpandWidth(true));
            DrawBar(rowRect, ratio, rateLabel == "DPS" ? DefaultBarColor : HealingBarColor, null, DpsData.DamageScalingType.None);

            string valueText = FormatNumber(playerRate) + " " + rateLabel + " (" +
                FormatNumber(row.Value) + ", " + percent.ToString("0.0") + "%)";
            float valueWidth = _rowRight.CalcSize(new GUIContent(valueText)).x;
            float valueRight = rowRect.xMax - 7f;
            float valueLeft = Mathf.Max(rowRect.x + 7f, valueRight - valueWidth);
            float nameWidth = Mathf.Max(0f, valueLeft - rowRect.x - 14f);

            GUI.color = SourceNameColor;
            DrawBarTextWithStroke(
                new Rect(rowRect.x + 7f, rowRect.y, nameWidth, rowRect.height),
                TruncateTextToWidth(StripRichTextTags(row.Key), nameWidth, _row),
                _row);
            DrawBarTextWithStroke(
                new Rect(valueLeft, rowRect.y, Mathf.Max(0f, valueRight - valueLeft), rowRect.height),
                valueText,
                _rowRight);
            GUI.color = Color.white;
        }
    }

    private void DrawDamageRow(string name, float amount, float total, float maxAmount, int index, ElementalType? elemental, DpsData.DamageScalingType scaling, Sprite icon)
    {
        float ratio = maxAmount > 0f ? Mathf.Clamp01(amount / maxAmount) : 0f;
        float percent = total > 0f ? Mathf.Clamp01(amount / total) * 100f : 0f;

        Rect rowRect = GUILayoutUtility.GetRect(0f, 22f, GUILayout.ExpandWidth(true));

        rowRect.x = Mathf.Round(rowRect.x);
        rowRect.y = Mathf.Round(rowRect.y);
        rowRect.width = Mathf.Round(rowRect.width);
        rowRect.height = Mathf.Round(rowRect.height);

        float iconSize = rowRect.height;
        float barX = rowRect.x;

        if (icon != null)
        {
            Rect iconRect = new Rect(rowRect.x, rowRect.y, iconSize, iconSize);

            DrawSprite(icon, iconRect);

            barX = iconRect.xMax;
        }

        Rect barRect = new Rect(
            barX,
            rowRect.y,
            Mathf.Max(0f, rowRect.xMax - barX),
            rowRect.height);

        GUI.color = new Color(0.10f, 0.10f, 0.10f, 0.75f);
        GUI.DrawTexture(barRect, _whiteTexture);

        DrawBar(barRect, ratio, GetBarColor(elemental, scaling), elemental, scaling);

        GUI.color = SourceNameColor;

        float textX = barRect.x + 7f;
        Rect nameRect = new Rect(
            textX,
            rowRect.y,
            Mathf.Max(0f, barRect.width - 14f),
            rowRect.height);

        Rect valueRect = new Rect(
            rowRect.x + 7f,
            rowRect.y,
            rowRect.width - 14f,
            rowRect.height);

        DrawBarTextWithStroke(nameRect, name, _row);
        DrawBarTextWithStroke(valueRect, FormatNumber(amount) + "  " + percent.ToString("0.0") + "%", _rowRight);

        GUI.color = Color.white;
    }

    private static string TruncateTextToWidth(string text, float width, GUIStyle style)
    {
        if (string.IsNullOrEmpty(text) || width <= 0f || style == null)
        {
            return string.Empty;
        }

        if (style.CalcSize(new GUIContent(text)).x <= width)
        {
            return text;
        }

        const string ellipsis = "...";
        if (style.CalcSize(new GUIContent(ellipsis)).x > width)
        {
            return string.Empty;
        }

        int low = 0;
        int high = text.Length;
        while (low < high)
        {
            int mid = (low + high + 1) / 2;
            string candidate = text.Substring(0, mid).TrimEnd() + ellipsis;
            if (style.CalcSize(new GUIContent(candidate)).x <= width)
            {
                low = mid;
            }
            else
            {
                high = mid - 1;
            }
        }

        return text.Substring(0, low).TrimEnd() + ellipsis;
    }

    private static string StripRichTextTags(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        int start;
        while ((start = text.IndexOf("<color", StringComparison.OrdinalIgnoreCase)) >= 0)
        {
            int end = text.IndexOf('>', start);
            if (end < 0)
                break;

            text = text.Remove(start, end - start + 1);
        }

        return text.Replace("</color>", string.Empty);
    }

    private void DrawBar(Rect barRect, float ratio, Color fillColor, ElementalType? elemental, DpsData.DamageScalingType scaling)
    {
        float fillWidth = barRect.width * ratio;
        if (fillWidth <= 0f)
        {
            return;
        }

        Rect fillRect = new Rect(barRect.x, barRect.y, fillWidth, barRect.height);
        GUI.color = fillColor;
        GUI.DrawTexture(fillRect, _whiteTexture);

        Color outlineColor = GetBarOutlineColor(elemental, scaling);
        const float outline = 2f;
        GUI.color = outlineColor;
        GUI.DrawTexture(new Rect(fillRect.x, fillRect.y, fillRect.width, outline), _whiteTexture);
        GUI.DrawTexture(new Rect(fillRect.x, fillRect.yMax - outline, fillRect.width, outline), _whiteTexture);
        GUI.DrawTexture(new Rect(fillRect.x, fillRect.y, outline, fillRect.height), _whiteTexture);
        GUI.DrawTexture(new Rect(fillRect.xMax - outline, fillRect.y, outline, fillRect.height), _whiteTexture);
    }

    private static void DrawBarTextWithStroke(Rect textRect, string text, GUIStyle style)
    {
        if (style == null || string.IsNullOrEmpty(text) || textRect.width <= 0f)
        {
            return;
        }

        GUIStyle shadowStyle = CreateBarTextStyle(style, BarTextStrokeColor);
        GUIStyle textStyle = CreateBarTextStyle(style, Color.white);
        const float offset = 1f;
        GUI.color = Color.white;

        GUI.Label(
            new Rect(textRect.x + offset, textRect.y + offset, textRect.width, textRect.height),
            text,
            shadowStyle);
        GUI.Label(textRect, text, textStyle);
    }

    private static GUIStyle CreateBarTextStyle(GUIStyle source, Color color)
    {
        GUIStyle style = new GUIStyle(source);
        style.normal.textColor = color;
        return style;
    }

    private static Color GetBarOutlineColor(ElementalType? elemental, DpsData.DamageScalingType scaling)
    {
        if (!elemental.HasValue)
        {
            switch (scaling)
            {
                case DpsData.DamageScalingType.Ad: return AdScalingBarOutlineColor;
                case DpsData.DamageScalingType.Ap: return ApScalingBarOutlineColor;
                case DpsData.DamageScalingType.Hp: return HpScalingBarOutlineColor;
                default: return DefaultBarOutlineColor;
            }
        }

        switch (elemental.Value.ToString())
        {
            case "Fire": return FireBarOutlineColor;
            case "Cold": return IceBarOutlineColor;
            case "Light": return LightBarOutlineColor;
            case "Dark": return DarkBarOutlineColor;
            default:
                switch (scaling)
                {
                    case DpsData.DamageScalingType.Ad: return AdScalingBarOutlineColor;
                    case DpsData.DamageScalingType.Ap: return ApScalingBarOutlineColor;
                    case DpsData.DamageScalingType.Hp: return HpScalingBarOutlineColor;
                    default: return DefaultBarOutlineColor;
                }
        }
    }
    private static Color GetBarColor(ElementalType? elemental, DpsData.DamageScalingType scaling)
    {
        if (!elemental.HasValue)
        {
            switch (scaling)
            {
                case DpsData.DamageScalingType.Ad: return AdScalingBarColor;
                case DpsData.DamageScalingType.Ap: return ApScalingBarColor;
                case DpsData.DamageScalingType.Hp: return HpScalingBarColor;
                default: return DefaultBarColor;
            }
        }

        switch (elemental.Value.ToString())
        {
            case "Fire": return FireBarColor;
            case "Cold": return IceBarColor;
            case "Light": return LightBarColor;
            case "Dark": return DarkBarColor;
            default:
                switch (scaling)
                {
                    case DpsData.DamageScalingType.Ad: return AdScalingBarColor;
                    case DpsData.DamageScalingType.Ap: return ApScalingBarColor;
                    case DpsData.DamageScalingType.Hp: return HpScalingBarColor;
                    default: return DefaultBarColor;
                }
        }
    }

    public void SetBasicAttackIcon(Sprite icon)
    {
        if (icon != null)
        {
            _basicAttackIcon = icon;
        }
    }

    public Sprite GetBasicAttackIconForHistory()
    {
        return GetBasicAttackIcon();
    }

    private Sprite GetBasicAttackIcon()
    {
        if (_basicAttackIcon != null)
        {
            return _basicAttackIcon;
        }

        Sprite[] loadedSprites = Resources.FindObjectsOfTypeAll<Sprite>();
        for (int i = 0; i < loadedSprites.Length; i++)
        {
            Sprite sprite = loadedSprites[i];
            if (sprite == null)
                continue;

            if (IsBasicAttackAssetName(sprite.name)
                || (sprite.texture != null && IsBasicAttackAssetName(sprite.texture.name)))
            {
                _basicAttackIcon = sprite;
                return _basicAttackIcon;
            }
        }

        Texture2D[] loadedTextures = Resources.FindObjectsOfTypeAll<Texture2D>();
        for (int i = 0; i < loadedTextures.Length; i++)
        {
            Texture2D texture = loadedTextures[i];
            if (texture == null || !IsBasicAttackAssetName(texture.name))
                continue;

            _basicAttackIcon = Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                100f);

            return _basicAttackIcon;
        }

        return null;
    }

    private static bool IsBasicAttackAssetName(string name)
    {
        if (string.IsNullOrEmpty(name))
            return false;

        return string.Equals(name, "2", StringComparison.OrdinalIgnoreCase)
            || string.Equals(name, "2.png", StringComparison.OrdinalIgnoreCase);
    }

    private static void DrawSprite(Sprite sprite, Rect rect)
    {
        if (sprite == null || sprite.texture == null)
            return;

        Rect r = sprite.textureRect;
        Texture texture = sprite.texture;
        Rect uv = new Rect(
            r.x / texture.width,
            r.y / texture.height,
            r.width / texture.width,
            r.height / texture.height);
        GUI.DrawTextureWithTexCoords(rect, texture, uv, true);
    }

    private static Sprite GetIcon(SkillTrigger skill)
    {
        if (skill == null || skill.currentConfig == null)
            return null;

        return skill.currentConfig.triggerIcon;
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

    private string GetSkillLabel(SkillTrigger skill)
    {
        if (skill == null)
        {
            return "Basic / Other";
        }

        string title = skill.GetFormattedSkillTitle();
        int end = title.LastIndexOf(']');
        if (end == title.Length - 1)
        {
            int start = title.LastIndexOf('[');
            if (start >= 0)
                title = title.Substring(0, start).TrimEnd();
        }

        return title;
    }

    private static string FormatNumber(float value)
    {
        if (value >= 1000000000f)
        {
            return (value / 1000000000f).ToString("0.##") + "B";
        }

        if (value >= 1000000f)
        {
            return (value / 1000000f).ToString("0.##") + "M";
        }

        if (value >= 1000f)
        {
            return (value / 1000f).ToString("0.##") + "K";
        }

        return value.ToString("0");
    }

    private void DrawResizeGrip()
    {
        Rect grip = new Rect(
            _windowRect.xMax - 14f,
            _windowRect.yMax - 14f,
            14f,
            14f);

        GUI.color = new Color(0.60f, 0.60f, 0.62f, 0.45f);
        for (int i = 0; i < 3; i++)
        {
            float inset = 2f + (i * 3f);
            GUI.DrawTexture(new Rect(_windowRect.xMax - 2f - inset, _windowRect.yMax - 1f, inset, 1f), _whiteTexture);
            GUI.DrawTexture(new Rect(_windowRect.xMax - 1f, _windowRect.yMax - 2f - inset, 1f, inset), _whiteTexture);
        }
        GUI.color = Color.white;
    }

    private void EnsureStyles()
    {
        if (_header != null)
        {
            return;
        }

        _header = new GUIStyle(GUI.skin.label)
        {
            fontSize = 13,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft,
            wordWrap = false
        };

        _headerRight = new GUIStyle(_header)
        {
            alignment = TextAnchor.MiddleRight
        };

        _row = new GUIStyle(GUI.skin.label)
        {
            fontSize = 13,
            fontStyle = FontStyle.Normal,
            alignment = TextAnchor.MiddleLeft,
            wordWrap = false,
            richText = false,
            clipping = TextClipping.Clip,
            padding = new RectOffset(0, 0, 0, 0),
            contentOffset = Vector2.zero
        };

        _rowRight = new GUIStyle(_row)
        {
            alignment = TextAnchor.MiddleRight
        };

        _small = new GUIStyle(_row)
        {
            fontSize = 11
        };

        _barBackground = new GUIStyle(GUI.skin.box);
        _barFill = new GUIStyle(GUI.skin.box);
        _whiteTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        _whiteTexture.SetPixel(0, 0, Color.white);
        _whiteTexture.Apply();
    }
}