using System.Collections.Generic;
using UnityEngine;

namespace DPSMeter;

public sealed class DpsOverlay : MonoBehaviour
{
    private enum DisplayMode
    {
        CurrentDps,
        DamageTotal,
        PartyDps,
        PartyTotal
    }

    private static readonly Color[] BarColors =
    {
        new Color(0.20f, 0.55f, 0.95f, 0.90f),
        new Color(0.45f, 0.75f, 0.25f, 0.90f),
        new Color(0.95f, 0.55f, 0.20f, 0.90f),
        new Color(0.70f, 0.35f, 0.90f, 0.90f),
        new Color(0.90f, 0.30f, 0.35f, 0.90f),
        new Color(0.20f, 0.75f, 0.70f, 0.90f)
    };

    private DpsData _data;
    private Vector2 _scroll;
    private DisplayMode _mode;

    private Rect _windowRect = new Rect(20f, 20f, 420f, 300f);
    private bool _dragging;
    private bool _resizing;
    private Vector2 _dragOffset;
    private Vector2 _resizeStartMouse;
    private Vector2 _resizeStartSize;
    private bool _headerMoved;

    private GUIStyle _header;
    private GUIStyle _row;
    private GUIStyle _small;
    private GUIStyle _barBackground;
    private GUIStyle _barFill;

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
        HandleWindowInput();

        GUI.Box(_windowRect, GUIContent.none, GUI.skin.window);

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
            Mathf.Max(20f, _windowRect.height - 36f));

        GUILayout.BeginArea(contentRect);
        _scroll = GUILayout.BeginScrollView(_scroll);

        switch (_mode)
        {
            case DisplayMode.CurrentDps:
                DrawPersonal(_data.CurrentPersonalSkills, _data.CurrentPersonalOther, _data.CurrentInstancePersonalDamage);
                break;

            case DisplayMode.DamageTotal:
                DrawPersonal(_data.CumulativePersonalSkills, _data.CumulativePersonalOther, _data.CumulativePersonalDamage);
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

        if (e.type == EventType.MouseDown && e.button == 0)
        {
            if (resizeRect.Contains(e.mousePosition))
            {
                _resizing = true;
                _resizeStartMouse = e.mousePosition;
                _resizeStartSize = _windowRect.size;
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
                _windowRect.width = Mathf.Clamp(
                    _resizeStartSize.x + delta.x,
                    260f,
                    Mathf.Max(260f, Screen.width - _windowRect.x - 10f));

                _windowRect.height = Mathf.Clamp(
                    _resizeStartSize.y + delta.y,
                    90f,
                    Mathf.Max(90f, Screen.height - _windowRect.y - 10f));

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

    private void DrawHeader(Rect headerRect)
    {
        string title;
        string metric;

        switch (_mode)
        {
            case DisplayMode.CurrentDps:
                title = "CURRENT DPS";
                metric = FormatNumber(_data.CurrentPersonalDps) + " DPS";
                break;

            case DisplayMode.DamageTotal:
                title = "DAMAGE TOTAL";
                metric = FormatNumber(_data.CumulativeTotalDamage) + " DAMAGE";
                break;

            case DisplayMode.PartyDps:
                title = "PARTY DPS";
                metric = FormatNumber(_data.CurrentPartyDps) + " DPS";
                break;

            default:
                title = "PARTY TOTAL";
                metric = FormatNumber(_data.CumulativeTotalDamage) + " DAMAGE";
                break;
        }

        GUI.Label(
            new Rect(headerRect.x, headerRect.y, headerRect.width * 0.55f, headerRect.height),
            title,
            _header);

        GUI.Label(
            new Rect(
                headerRect.x + headerRect.width * 0.55f,
                headerRect.y,
                headerRect.width * 0.45f,
                headerRect.height),
            metric,
            _header);
    }

    private void DrawPersonal(
        IReadOnlyList<KeyValuePair<Actor, float>> sources,
        IReadOnlyList<KeyValuePair<string, float>> other,
        float total)
    {
        if (sources.Count == 0 && other.Count == 0)
        {
            GUILayout.Label("No damage recorded yet.", _small);
            return;
        }

        for (int i = 0; i < sources.Count; i++)
        {
            KeyValuePair<Actor, float> entry = sources[i];
            DrawDamageRow(GetSourceLabel(entry.Key), entry.Value, total, i);
        }

        for (int i = 0; i < other.Count; i++)
        {
            KeyValuePair<string, float> entry = other[i];
            DrawDamageRow(entry.Key, entry.Value, total, sources.Count + i);
        }
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
            DrawDamageRow(row.Key, row.Value, total, i);
        }
    }

    private void DrawDamageRow(string name, float amount, float total, int index)
    {
        float ratio = total > 0f ? Mathf.Clamp01(amount / total) : 0f;
        float percent = ratio * 100f;

        Rect rowRect = GUILayoutUtility.GetRect(0f, 22f, GUILayout.ExpandWidth(true));

        GUI.color = new Color(0.10f, 0.10f, 0.10f, 0.75f);
        GUI.Box(rowRect, GUIContent.none, _barBackground);

        GUI.color = BarColors[index % BarColors.Length];
        GUI.Box(
            new Rect(
                rowRect.x,
                rowRect.y,
                rowRect.width * ratio,
                rowRect.height),
            GUIContent.none,
            _barFill);

        GUI.color = Color.white;

        GUI.Label(
            new Rect(rowRect.x + 7f, rowRect.y, rowRect.width - 14f, rowRect.height),
            name,
            _row);

        GUI.Label(
            new Rect(rowRect.x + 7f, rowRect.y, rowRect.width - 14f, rowRect.height),
            FormatNumber(amount) + "  " + percent.ToString("0.0") + "%",
            _rowRight);

        GUI.color = Color.white;
    }

    private string GetSourceLabel(Actor source)
    {
        if (source is SkillTrigger skill)
        {
            return skill.GetFormattedSkillTitle();
        }

        if (source is Gem gem)
        {
            string typeName = gem.GetType().Name;

            if (typeName.StartsWith("Gem_"))
            {
                string name = typeName.Substring(4);
                int separator = name.IndexOf('_');

                if (separator >= 0)
                {
                    name = name.Substring(separator + 1);
                }

                return "Essence of " + SplitPascalCase(name);
            }

            return typeName;
        }

        return source != null ? source.GetType().Name : "Unknown Source";
    }

    private static string SplitPascalCase(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return "Unknown";
        }

        System.Text.StringBuilder result = new System.Text.StringBuilder(value.Length + 8);

        for (int i = 0; i < value.Length; i++)
        {
            char c = value[i];

            if (i > 0 && char.IsUpper(c) && !char.IsUpper(value[i - 1]))
            {
                result.Append(' ');
            }

            result.Append(c);
        }

        return result.ToString();
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

    private GUIStyle _rowRight;

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

        _row = new GUIStyle(GUI.skin.label)
        {
            fontSize = 12,
            alignment = TextAnchor.MiddleLeft,
            wordWrap = false,
            clipping = TextClipping.Clip
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
    }
}