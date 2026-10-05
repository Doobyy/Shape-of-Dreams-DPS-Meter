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

    private DpsData _data;
    private Vector2 _scroll;
    private DisplayMode _mode;
    private GUIStyle _header;
    private GUIStyle _headerValue;
    private GUIStyle _row;
    private GUIStyle _value;
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

        float width = Mathf.Min(520f, Screen.width * 0.42f);
        float height = Mathf.Min(620f, Screen.height - 40f);

        GUILayout.BeginArea(new Rect(20f, 20f, width, height), GUI.skin.box);

        DrawHeader();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Reset", GUILayout.Width(80f)))
        {
            _data.Reset();
        }

        if (GUILayout.Button("Hide", GUILayout.Width(80f)))
        {
            Visible = false;
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(6f);
        _scroll = GUILayout.BeginScrollView(_scroll);

        switch (_mode)
        {
            case DisplayMode.CurrentDps:
                DrawPersonal(_data.CurrentPersonalSkills, _data.CurrentPersonalOther, _data.CurrentTotalDamage);
                break;

            case DisplayMode.DamageTotal:
                DrawPersonal(_data.CumulativePersonalSkills, _data.CumulativePersonalOther, _data.CumulativeTotalDamage);
                break;

            case DisplayMode.PartyDps:
                DrawParty(_data.CurrentParty, _data.CurrentTotalDamage);
                break;

            case DisplayMode.PartyTotal:
                DrawParty(_data.CumulativeParty, _data.CumulativeTotalDamage);
                break;
        }

        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }

    private void DrawHeader()
    {
        string title;
        string metric;

        switch (_mode)
        {
            case DisplayMode.CurrentDps:
                title = "CURRENT DPS";
                metric = FormatNumber(_data.CurrentDps) + " DPS";
                break;

            case DisplayMode.DamageTotal:
                title = "DAMAGE TOTAL";
                metric = FormatNumber(_data.CumulativeTotalDamage) + " DAMAGE";
                break;

            case DisplayMode.PartyDps:
                title = "PARTY DPS";
                metric = FormatNumber(_data.CurrentDps) + " DPS";
                break;

            default:
                title = "PARTY TOTAL";
                metric = FormatNumber(_data.CumulativeTotalDamage) + " DAMAGE";
                break;
        }

        Rect headerRect = GUILayoutUtility.GetRect(0f, 34f, GUILayout.ExpandWidth(true));

        if (GUI.Button(headerRect, GUIContent.none, GUIStyle.none))
        {
            _mode = (DisplayMode)(((int)_mode + 1) % 4);
        }

        GUI.Label(
            new Rect(headerRect.x + 6f, headerRect.y, headerRect.width * 0.55f, headerRect.height),
            title,
            _header);

        GUI.Label(
            new Rect(headerRect.x + headerRect.width * 0.55f, headerRect.y, headerRect.width * 0.42f, headerRect.height),
            metric,
            _headerValue);
    }

    private void DrawPersonal(
        IReadOnlyList<KeyValuePair<SkillTrigger, float>> skills,
        IReadOnlyList<KeyValuePair<string, float>> other,
        float total)
    {
        if (skills.Count == 0 && other.Count == 0)
        {
            GUILayout.Label("No damage recorded yet.", _small);
            return;
        }

        for (int i = 0; i < skills.Count; i++)
        {
            KeyValuePair<SkillTrigger, float> entry = skills[i];
            DrawDamageRow(GetSkillLabel(entry.Key), entry.Value, total);
        }

        for (int i = 0; i < other.Count; i++)
        {
            KeyValuePair<string, float> entry = other[i];
            DrawDamageRow(entry.Key, entry.Value, total);
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
            DrawDamageRow(row.Key, row.Value, total);
        }
    }

    private void DrawDamageRow(string name, float amount, float total)
    {
        float percent = total > 0f ? amount / total * 100f : 0f;

        GUILayout.BeginHorizontal(GUILayout.Height(26f));

        GUILayout.Label(name, _row, GUILayout.Width(190f));

        Rect barRect = GUILayoutUtility.GetRect(100f, 18f, GUILayout.ExpandWidth(true));
        GUI.Box(barRect, GUIContent.none, _barBackground);

        float ratio = total > 0f ? Mathf.Clamp01(amount / total) : 0f;
        GUI.Box(
            new Rect(barRect.x, barRect.y, barRect.width * ratio, barRect.height),
            GUIContent.none,
            _barFill);

        GUILayout.Label(FormatNumber(amount), _value, GUILayout.Width(82f));
        GUILayout.Label(percent.ToString("0.0") + "%", _value, GUILayout.Width(48f));

        GUILayout.EndHorizontal();
    }

    private string GetSkillLabel(SkillTrigger skill)
    {
        if (skill == null)
        {
            return "Basic / Other";
        }

        string title = skill.GetFormattedSkillTitle();

        HeroSkillLocation location;
        if (HeroSkill.TryGetSkillLocation(skill, out location))
        {
            return title + " [" + location + "]";
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

    private void EnsureStyles()
    {
        if (_header != null)
        {
            return;
        }

        _header = new GUIStyle(GUI.skin.label)
        {
            fontSize = 17,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft,
            wordWrap = false
        };

        _headerValue = new GUIStyle(_header)
        {
            alignment = TextAnchor.MiddleRight
        };

        _row = new GUIStyle(GUI.skin.label)
        {
            fontSize = 13,
            wordWrap = false
        };

        _value = new GUIStyle(_row)
        {
            alignment = TextAnchor.MiddleRight
        };

        _small = new GUIStyle(GUI.skin.label)
        {
            fontSize = 11
        };

        _barBackground = new GUIStyle(GUI.skin.box);
        _barFill = new GUIStyle(GUI.skin.box);
    }
}
