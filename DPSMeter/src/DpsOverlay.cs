using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace DPSMeter;

public sealed class DpsOverlay : MonoBehaviour
{
    private enum DisplayMode
    {
        CurrentDps,
        DamageTotal,
        CurrentHps,
        TotalHps,
        PartyDps,
        PartyTotal
    }

    private static readonly Color DefaultBarColor = new Color(0.30f, 0.30f, 0.30f, 0.68f);
    private static readonly Color WindowFillColor = new Color(0f, 0f, 0f, 0.55f);
    private static readonly Color FireBarColor = new Color(0.62f, 0.18f, 0.18f, 0.68f);
    private static readonly Color IceBarColor = new Color(0.18f, 0.38f, 0.68f, 0.68f);
    private static readonly Color LightBarColor = new Color(0.68f, 0.60f, 0.16f, 0.68f);
    private static readonly Color DarkBarColor = new Color(0.40f, 0.18f, 0.52f, 0.68f);
    private static readonly Color AdScalingBarColor = new Color(0.55f, 0.36f, 0.18f, 0.68f);
    private static readonly Color ApScalingBarColor = new Color(0.18f, 0.50f, 0.55f, 0.68f);
    private static readonly Color HpScalingBarColor = new Color(0.36f, 0.55f, 0.22f, 0.68f);
    private static readonly Color HealingBarColor = new Color(0.22f, 0.62f, 0.30f, 0.68f);
    private static readonly Color SourceNameColor = new Color(0.97f, 0.97f, 0.97f, 1f);
    private const string DevelopmentVersion = "v4.56";

    private DpsData _data;
    private Vector2 _scroll;
    private DisplayMode _mode;

    private Rect _windowRect = new Rect(20f, 20f, 260f, 272f);
    private bool _dragging;
    private bool _resizing;
    private Vector2 _dragOffset;
    private Vector2 _resizeStartMouse;
    private Vector2 _resizeStartSize;
    private bool _resizeMoved;
    private float _resizeStartHealingHeight;
    private bool _headerMoved;
    private bool _showHealing;
    private bool _showBarrier;
    private float _collapsedWindowHeight;
    private bool _manualResize;

    private GUIStyle _header;
    private GUIStyle _headerRight;
    private GUIStyle _row;
    private GUIStyle _small;
    private GUIStyle _barBackground;
    private GUIStyle _barFill;
    private GUIStyle _rowRight;
    private Texture2D _whiteTexture;
    private Sprite _basicAttackIcon;

    public bool Visible { get; set; } = true;

    public void Initialize(DpsData data)
    {
        _data = data;
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
        DrawDevelopmentReloadButton();

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

            case DisplayMode.CurrentHps:
                DrawHealingSources(_data.CurrentPersonalHealingRows, _data.CurrentInstancePersonalHealing);
                break;

            case DisplayMode.TotalHps:
                DrawHealingSources(_data.CumulativeHealingRows, _data.CumulativePersonalHealing);
                break;

            case DisplayMode.PartyDps:
                DrawParty(_data.CurrentParty, _data.CurrentInstancePartyDamage);
                break;

            case DisplayMode.PartyTotal:
                DrawParty(_data.CumulativeParty, _data.CumulativePartyDamage);
                break;
        }

        GUILayout.EndScrollView();
        GUILayout.EndArea();

        if (_mode == DisplayMode.CurrentDps || _mode == DisplayMode.DamageTotal)
        {
            Rect breakdownRect = new Rect(
                _windowRect.x + 6f,
                _windowRect.y + _collapsedWindowHeight - 20f,
                _windowRect.width - 12f,
                Mathf.Max(20f, _windowRect.height - _collapsedWindowHeight + 20f));

            GUILayout.BeginArea(breakdownRect);

            DrawBreakdownToggles();

            if (_showHealing)
            {
                DrawHealingBreakdown(
                    _mode == DisplayMode.CurrentDps
                        ? _data.CurrentPersonalHealingRows
                        : _data.CumulativeHealingRows,
                    _mode == DisplayMode.CurrentDps
                        ? _data.CurrentInstancePersonalHealing
                        : _data.CumulativePersonalHealing);
            }

            if (_showBarrier)
            {
                DrawBarrierBreakdown(
                    _mode == DisplayMode.CurrentDps
                        ? _data.CurrentPersonalBarrierRows
                        : _data.CumulativeBarrierRows,
                    _mode == DisplayMode.CurrentDps
                        ? _data.CurrentInstancePersonalBarrier
                        : _data.CumulativePersonalBarrier);
            }

            GUILayout.EndArea();
        }

        DrawResizeGrip();
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

        Rect reloadRect = GetReloadButtonRect();

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

            if (reloadRect.Contains(e.mousePosition))
            {
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
                    _mode = (DisplayMode)(((int)_mode + 1) % 6);
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
                Mathf.Max(272f, desiredHeight),
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
        if (_mode != DisplayMode.CurrentDps && _mode != DisplayMode.DamageTotal)
        {
            return 0f;
        }

        float height = 0f;

        if (!_showHealing && !_showBarrier)
        {
            return 0f;
        }

        height = 16f;

        if (_showHealing)
        {
            IReadOnlyList<DpsData.BreakdownRow> rows =
                _mode == DisplayMode.CurrentDps
                    ? _data.CurrentPersonalHealingRows
                    : _data.CumulativeHealingRows;

            int rowCount = rows != null ? rows.Count : 0;
            height += 22f + (Mathf.Max(1, rowCount) * 22f);
        }

        if (_showBarrier)
        {
            IReadOnlyList<DpsData.BreakdownRow> rows =
                _mode == DisplayMode.CurrentDps
                    ? _data.CurrentPersonalBarrierRows
                    : _data.CumulativeBarrierRows;

            int rowCount = rows != null ? rows.Count : 0;
            height += 22f + (Mathf.Max(1, rowCount) * 22f);
        }

        if (_showHealing != _showBarrier)
        {
            height += 1.36f;
        }

        return height;
    }

    private void DrawHeader(Rect headerRect)
    {
        string title;
        string metric;

        switch (_mode)
        {
            case DisplayMode.CurrentDps:
                title = "CURRENT DPS  " + DevelopmentVersion;
                metric = FormatNumber(_data.CurrentPersonalDps) + " DPS";
                break;

            case DisplayMode.DamageTotal:
                title = "DAMAGE TOTAL";
                metric = FormatNumber(_data.CumulativePersonalDamage) + " DAMAGE";
                break;

            case DisplayMode.CurrentHps:
                title = "CURRENT HPS";
                metric = FormatNumber(_data.CurrentPersonalHps) + " HPS";
                break;

            case DisplayMode.TotalHps:
                title = "TOTAL HPS";
                metric = FormatNumber(_data.TotalPersonalHps) + " HPS";
                break;

            case DisplayMode.PartyDps:
                title = "PARTY DPS";
                metric = FormatNumber(_data.CurrentPartyDps) + " DPS";
                break;

            default:
                title = "PARTY TOTAL";
                metric = FormatNumber(_data.CumulativePartyDamage) + " DAMAGE";
                break;
        }

        float metricWidth = headerRect.width * 0.35f;
        float titleWidth = headerRect.width - metricWidth - 6f;

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
    }

    private Rect GetReloadButtonRect()
    {
        const float reloadWidth = 74f;
        const float reloadHeight = 24f;
        const float gap = 6f;

        float x = _windowRect.xMax + gap;
        if (x + reloadWidth > Screen.width - 4f)
        {
            x = _windowRect.x - reloadWidth - gap;
        }

        x = Mathf.Clamp(x, 4f, Mathf.Max(4f, Screen.width - reloadWidth - 4f));

        float y = _windowRect.y + 4f;
        y = Mathf.Clamp(y, 4f, Mathf.Max(4f, Screen.height - reloadHeight - 4f));

        return new Rect(x, y, reloadWidth, reloadHeight);
    }

    private void DrawDevelopmentReloadButton()
    {
        Rect reloadRect = GetReloadButtonRect();

        if (GUI.Button(reloadRect, "RELOAD"))
        {
            DewMod.ReloadFromActiveMods();
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

        for (int i = 0; i < rows.Count; i++)
        {
            DamageRow row = rows[i];
            DrawDamageRow(row.Name, row.Amount, total, rows[0].Amount, i, row.Elemental, row.Scaling, row.Icon);
        }
    }

    private void DrawBreakdownToggles()
    {
        GUIStyle toggleStyle = new GUIStyle(_small)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 13,
            fontStyle = FontStyle.Bold,
            normal = { textColor = new Color(0.80f, 0.80f, 0.80f, 0.95f) },
            hover = { textColor = Color.white }
        };

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        if (GUILayout.Button(_showHealing ? "+" : "+", toggleStyle, GUILayout.Width(20f), GUILayout.Height(16f)))
        {
            _showHealing = !_showHealing;
        }

        if (GUILayout.Button(_showBarrier ? "🛡" : "🛡", toggleStyle, GUILayout.Width(20f), GUILayout.Height(16f)))
        {
            _showBarrier = !_showBarrier;
        }

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }

    private void DrawBarrierBreakdown(IReadOnlyList<DpsData.BreakdownRow> rows, float total)
    {
        float bps = _mode == DisplayMode.CurrentDps
            ? _data.CurrentPersonalBps
            : _data.TotalPersonalBps;

        GUILayout.Label(
            "----------  " + FormatNumber(bps) + " BPS  ----------",
            _small);

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

        GUI.color = new Color(0.28f, 0.50f, 0.68f, 0.68f);
        GUI.DrawTexture(
            new Rect(barRect.x, barRect.y, barRect.width * ratio, barRect.height),
            _whiteTexture);

        GUI.color = SourceNameColor;
        GUI.Label(
            new Rect(barRect.x + 7f, rowRect.y, Mathf.Max(0f, barRect.width - 14f), rowRect.height),
            StripRichTextTags(name),
            _row);
        GUI.Label(
            new Rect(rowRect.x + 7f, rowRect.y, rowRect.width - 14f, rowRect.height),
            FormatNumber(amount) + "  " + percent.ToString("0.0") + "%",
            _rowRight);

        GUI.color = Color.white;
    }

    private void DrawHealingBreakdown(IReadOnlyList<DpsData.BreakdownRow> rows, float total)
    {
        float hps = _mode == DisplayMode.CurrentDps
            ? _data.CurrentPersonalHps
            : _data.TotalPersonalHps;

        GUILayout.Label(
            "----------  " + FormatNumber(hps) + " HPS  ----------",
            _small);

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
            Sprite icon = _mode == DisplayMode.TotalHps || _mode == DisplayMode.DamageTotal
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

        GUI.color = HealingBarColor;
        GUI.DrawTexture(
            new Rect(barRect.x, barRect.y, barRect.width * ratio, barRect.height),
            _whiteTexture);

        GUI.color = SourceNameColor;
        GUI.Label(
            new Rect(barRect.x + 7f, rowRect.y, Mathf.Max(0f, barRect.width - 14f), rowRect.height),
            StripRichTextTags(name),
            _row);
        GUI.Label(
            new Rect(rowRect.x + 7f, rowRect.y, rowRect.width - 14f, rowRect.height),
            FormatNumber(amount) + "  " + percent.ToString("0.0") + "%",
            _rowRight);

        GUI.color = Color.white;
    }

    private void DrawParty(IReadOnlyList<KeyValuePair<string, float>> rows, float total)
    {
        if (rows.Count == 0)
        {
            GUILayout.Label("No party damage recorded yet.", _small);
            return;
        }

        for (int i = 0; i < rows.Count; i++)
        {
            KeyValuePair<string, float> row = rows[i];
            DrawDamageRow(row.Key, row.Value, total, rows[0].Value, i, null, DpsData.DamageScalingType.None, null);
        }
    }

    private void DrawDamageRow(string name, float amount, float total, float maxAmount, int index, ElementalType? elemental, DpsData.DamageScalingType scaling, Sprite icon)
    {
        float ratio = maxAmount > 0f ? Mathf.Clamp01(amount / maxAmount) : 0f;
        float percent = total > 0f ? Mathf.Clamp01(amount / total) * 100f : 0f;

        Rect rowRect = GUILayoutUtility.GetRect(0f, 22f, GUILayout.ExpandWidth(true));

        // Keep IMGUI text and bar edges on whole pixels. Fractional positions can
        // make small fonts look noticeably soft even when the font itself is sharp.
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

        GUI.color = GetBarColor(elemental, scaling);
        GUI.DrawTexture(
            new Rect(
                barRect.x,
                barRect.y,
                barRect.width * ratio,
                barRect.height),
            _whiteTexture);

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

        GUI.Label(nameRect, name, _row);
        GUI.Label(valueRect, FormatNumber(amount) + "  " + percent.ToString("0.0"), _rowRight);

        GUI.color = Color.white;
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
            default: return DefaultBarColor;
        }
    }

    public void SetBasicAttackIcon(Sprite icon)
    {
        if (icon != null)
        {
            _basicAttackIcon = icon;
        }
    }

    private Sprite GetBasicAttackIcon()
    {
        if (_basicAttackIcon != null)
        {
            return _basicAttackIcon;
        }

        // Rawdata/!Sprites/2.png is not a Resources path, so Resources.Load cannot
        // locate it. First reuse a Sprite/Texture that the game has already loaded.
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

        // The game's skill icon is stored on the active TriggerConfig.
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

        GUI.color = new Color(1f, 1f, 1f, 0.35f);
        GUI.Box(grip, "...");
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