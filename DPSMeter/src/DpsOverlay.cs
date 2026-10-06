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
    private static readonly Color WindowFillColor = new Color(0f, 0f, 0f, 0.55f);
    private static readonly Color FireBarColor = new Color(0.62f, 0.18f, 0.18f, 0.68f);
    private static readonly Color IceBarColor = new Color(0.18f, 0.38f, 0.68f, 0.68f);
    private static readonly Color LightBarColor = new Color(0.68f, 0.60f, 0.16f, 0.68f);
    private static readonly Color DarkBarColor = new Color(0.40f, 0.18f, 0.52f, 0.68f);
    private static readonly Color AdScalingBarColor = new Color(0.55f, 0.36f, 0.18f, 0.68f);
    private static readonly Color ApScalingBarColor = new Color(0.18f, 0.50f, 0.55f, 0.68f);
    private static readonly Color HpScalingBarColor = new Color(0.36f, 0.55f, 0.22f, 0.68f);
    private static readonly Color SourceNameColor = new Color(0.97f, 0.97f, 0.97f, 1f);
    private const string DevelopmentVersion = "v4.12";

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
    private Sprite _basicAttackIcon;
    private bool _basicAttackIconScanLogged;

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
                    _data.CurrentInstancePersonalDamage,
                    false);
                break;

            case DisplayMode.DamageTotal:
                DrawPersonal(
                    _data.CumulativePersonalSkills,
                    _data.CumulativePersonalOther,
                    _data.CumulativePersonalEssences,
                    _data.CumulativePersonalDamage,
                    true);
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
        IReadOnlyList<KeyValuePair<string, float>> sources,
        IReadOnlyList<KeyValuePair<string, float>> other,
        IReadOnlyList<KeyValuePair<string, float>> essences,
        float total,
        bool cumulativeEssences)
    {
        List<DamageRow> rows = new List<DamageRow>(sources.Count + other.Count + essences.Count);

        for (int i = 0; i < sources.Count; i++)
        {
            KeyValuePair<string, float> row = sources[i];
            rows.Add(new DamageRow
            {
                Name = StripRichTextTags(row.Key),
                Amount = row.Value,
                Elemental = _data.GetCurrentSkillElement(row.Key),
                Scaling = _data.GetCurrentSkillScaling(row.Key),
                Icon = _data.GetSkillIcon(row.Key)
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
                Icon = row.Key == "Basic Attack" ? GetBasicAttackIcon() : null
            });
        }

        for (int i = 0; i < essences.Count; i++)
        {
            KeyValuePair<string, float> row = essences[i];
            if (string.IsNullOrEmpty(row.Key) || row.Value <= 0f)
                continue;

            rows.Add(new DamageRow
            {
                Name = StripRichTextTags(GetEssenceDisplayName(row.Key)),
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
                Debug.Log("[DPS Meter][v4.10 TRACE] Basic Attack icon candidate sprite=" +
                    sprite.name + " texture=" + (sprite.texture != null ? sprite.texture.name : "none"));
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

            Debug.Log("[DPS Meter][v4.10 TRACE] Basic Attack icon candidate texture=" +
                texture.name + " size=" + texture.width + "x" + texture.height);
            return _basicAttackIcon;
        }

        // Do this once per overlay lifetime. The important part of this trace is
        // that it tells us what Unity object names actually exist at runtime;
        // we should not guess another file-loading API without that evidence.
        if (!_basicAttackIconScanLogged)
        {
            _basicAttackIconScanLogged = true;
            Debug.Log("[DPS Meter][v4.10 TRACE] Basic Attack source asset not found by loaded Sprite/Texture name. Candidate loaded assets:");

            for (int i = 0; i < loadedSprites.Length; i++)
            {
                Sprite sprite = loadedSprites[i];
                if (sprite == null)
                    continue;

                string name = sprite.name ?? string.Empty;
                string textureName = sprite.texture != null ? sprite.texture.name : string.Empty;
                if (ContainsBasicAttackTraceTerm(name) || ContainsBasicAttackTraceTerm(textureName))
                {
                    Debug.Log("[DPS Meter][v4.10 TRACE] Sprite candidate name=" + name +
                        " texture=" + textureName +
                        " size=" + (sprite.texture != null ? sprite.texture.width + "x" + sprite.texture.height : "none"));
                }
            }

            for (int i = 0; i < loadedTextures.Length; i++)
            {
                Texture2D texture = loadedTextures[i];
                if (texture == null || !ContainsBasicAttackTraceTerm(texture.name))
                    continue;

                Debug.Log("[DPS Meter][v4.10 TRACE] Texture candidate name=" + texture.name +
                    " size=" + texture.width + "x" + texture.height);
            }
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

    private static bool ContainsBasicAttackTraceTerm(string name)
    {
        if (string.IsNullOrEmpty(name))
            return false;

        return name.IndexOf("sword", StringComparison.OrdinalIgnoreCase) >= 0
            || name.IndexOf("attack", StringComparison.OrdinalIgnoreCase) >= 0
            || name.IndexOf("basic", StringComparison.OrdinalIgnoreCase) >= 0
            || string.Equals(name, "2", StringComparison.OrdinalIgnoreCase)
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

    private static string GetEssenceDisplayName(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return "Essence";
        }

        if (name.StartsWith("Gem_"))
        {
            name = name.Substring(4);

            int separator = name.IndexOf('_');
            if (separator > 0 && separator <= 2)
            {
                name = name.Substring(separator + 1);
            }
        }

        name = name.Replace('_', ' ').Trim();

        return string.IsNullOrEmpty(name) ? "Essence" : "Essence of " + name;
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