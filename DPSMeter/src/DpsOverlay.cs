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
        PartyDps,
        PartyTotal
    }

    private static readonly Color DefaultBarColor = new Color(0.30f, 0.30f, 0.30f, 0.68f);
    private static readonly Color FireBarColor = new Color(0.62f, 0.18f, 0.18f, 0.68f);
    private static readonly Color IceBarColor = new Color(0.18f, 0.38f, 0.68f, 0.68f);
    private static readonly Color LightBarColor = new Color(0.68f, 0.60f, 0.16f, 0.68f);
    private static readonly Color DarkBarColor = new Color(0.40f, 0.18f, 0.52f, 0.68f);
    private const string DevelopmentVersion = "v1.3";

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
    private GUIStyle _rowRight;
    private Texture2D _whiteTexture;

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
                DrawPersonal(
                    _data.CurrentPersonalSkills,
                    _data.CurrentPersonalOther,
                    _data.CurrentPersonalEssences,
                    _data.CurrentInstancePersonalDamage);
                break;

            case DisplayMode.DamageTotal:
                DrawPersonal(
                    _data.CumulativePersonalSkills,
                    _data.CumulativePersonalOther,
                    _data.CumulativePersonalEssences,
                    _data.CumulativePersonalDamage);
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

        float reloadWidth = 74f;
        float metricWidth = headerRect.width * 0.35f;
        float titleWidth = headerRect.width - metricWidth - reloadWidth - 6f;
        Rect reloadRect = new Rect(
            headerRect.x + titleWidth + metricWidth + 6f,
            headerRect.y + 1f,
            reloadWidth,
            headerRect.height - 2f);

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
                title = "CURRENT DPS  " + DevelopmentVersion;
                metric = FormatNumber(_data.CurrentPersonalDps) + " DPS";
                break;

            case DisplayMode.DamageTotal:
                title = "DAMAGE TOTAL";
                metric = FormatNumber(_data.CumulativePersonalDamage) + " DAMAGE";
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

        float reloadWidth = 74f;
        float metricWidth = headerRect.width * 0.35f;
        float titleWidth = headerRect.width - metricWidth - reloadWidth - 6f;

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
            _header);

        if (GUI.Button(
            new Rect(
                headerRect.x + titleWidth + metricWidth + 6f,
                headerRect.y + 1f,
                reloadWidth,
                headerRect.height - 2f),
            "RELOAD"))
        {
            Debug.Log("[DPS Meter] Reloading active mods...");
            DewMod.ReloadFromActiveMods();
        }
    }

    private void DrawPersonal(
        IReadOnlyList<KeyValuePair<string, float>> sources,
        IReadOnlyList<KeyValuePair<string, float>> other,
        IReadOnlyList<KeyValuePair<Gem, float>> essences,
        float total)
    {
        DrawSkillRows(sources, total);
        DrawRows(other, total, sources.Count);

        if (essences.Count > 0)
        {
            GUILayout.Space(4f);
            GUILayout.Label("ESSENCES", _small);
            DrawRows(essences, total, sources.Count + other.Count + 1);
        }
    }

    private void DrawSkillRows(
        IReadOnlyList<KeyValuePair<string, float>> rows,
        float total,
        int indexOffset = 0)
    {
        if (rows.Count == 0)
        {
            GUILayout.Label("No damage recorded yet.", _small);
            return;
        }

        for (int i = 0; i < rows.Count; i++)
        {
            KeyValuePair<string, float> row = rows[i];
            DrawDamageRow(row.Key, row.Value, total, indexOffset + i, _data.GetCurrentSkillElement(row.Key), _data.GetSkillIcon(row.Key));
        }
    }

    private void DrawRows(
        IReadOnlyList<KeyValuePair<string, float>> rows,
        float total,
        int indexOffset = 0)
    {
        if (rows.Count == 0)
        {
            GUILayout.Label("No damage recorded yet.", _small);
            return;
        }

        for (int i = 0; i < rows.Count; i++)
        {
            KeyValuePair<string, float> row = rows[i];
            DrawDamageRow(row.Key, row.Value, total, indexOffset + i, null, null);
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
            DrawDamageRow(row.Key, row.Value, total, i, null, null);
        }
    }

    private void DrawRows(
        IReadOnlyList<KeyValuePair<Gem, float>> rows,
        float total,
        int indexOffset = 0)
    {
        if (rows.Count == 0)
        {
            GUILayout.Label("No damage recorded yet.", _small);
            return;
        }

        for (int i = 0; i < rows.Count; i++)
        {
            KeyValuePair<Gem, float> row = rows[i];
            string name = row.Key != null ? row.Key.GetActorReadableName() : "Unknown Essence";
            DrawDamageRow(name, row.Value, total, indexOffset + i, _data.GetCurrentEssenceElement(row.Key), row.Key != null ? row.Key.icon : null);
        }
    }

    private void DrawDamageRow(string name, float amount, float total, int index, ElementalType? elemental, Sprite icon)
    {
        float ratio = total > 0f ? Mathf.Clamp01(amount / total) : 0f;
        float percent = ratio * 100f;

        Rect rowRect = GUILayoutUtility.GetRect(0f, 22f, GUILayout.ExpandWidth(true));

        GUI.color = new Color(0.10f, 0.10f, 0.10f, 0.75f);
        GUI.DrawTexture(rowRect, _whiteTexture);

        GUI.color = GetBarColor(elemental);
        GUI.DrawTexture(
            new Rect(
                rowRect.x,
                rowRect.y,
                rowRect.width * ratio,
                rowRect.height),
            _whiteTexture);

        GUI.color = Color.white;

        float textX = rowRect.x + 7f;
        if (icon != null)
        {
            Rect iconRect = new Rect(rowRect.x + 2f, rowRect.y + 2f, 18f, 18f);
            DrawSprite(icon, iconRect);
            textX = rowRect.x + 24f;
        }

        GUI.Label(
            new Rect(textX, rowRect.y, rowRect.width - (textX - rowRect.x) - 7f, rowRect.height),
            name,
            _row);

        GUI.Label(
            new Rect(rowRect.x + 7f, rowRect.y, rowRect.width - 14f, rowRect.height),
            FormatNumber(amount) + "  " + percent.ToString("0.0") + "%",
            _rowRight);

        GUI.color = Color.white;
    }

    private static Color GetBarColor(ElementalType? elemental)
    {
        if (!elemental.HasValue)
            return DefaultBarColor;

        switch (elemental.Value.ToString())
        {
            case "Fire": return FireBarColor;
            case "Cold": return IceBarColor;
            case "Light": return LightBarColor;
            case "Dark": return DarkBarColor;
            default: return DefaultBarColor;
        }
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
        _whiteTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        _whiteTexture.SetPixel(0, 0, Color.white);
        _whiteTexture.Apply();
    }
}