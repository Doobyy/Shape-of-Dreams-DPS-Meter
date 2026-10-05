using System.Collections.Generic;
using UnityEngine;

namespace DPSMeter;

public sealed class DpsOverlay : MonoBehaviour
{
    private DpsData _data;
    private Vector2 _scroll;
    private GUIStyle _header;
    private GUIStyle _row;
    private GUIStyle _small;

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

        float width = Mathf.Min(430f, Screen.width * 0.38f);
        float height = Mathf.Min(620f, Screen.height - 40f);

        GUILayout.BeginArea(new Rect(20f, 20f, width, height), GUI.skin.box);

        GUILayout.Label("DPS METER", _header);
        GUILayout.Label(string.Format("Total: {0:N0}", _data.TotalDamage), _row);
        GUILayout.Label(string.Format("DPS: {0:N0}", _data.Dps), _header);
        GUILayout.Label(string.Format("Time: {0:0.0}s   Hits: {1}", _data.Duration, _data.HitCount), _small);

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

        GUILayout.Space(8f);
        _scroll = GUILayout.BeginScrollView(_scroll);

        DrawSection("Skills", _data.Skills, _data.TotalDamage);
        GUILayout.Space(10f);
        DrawSection("Essences", _data.Essences, _data.TotalDamage);

        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }

    private void DrawSection(string title, IReadOnlyList<KeyValuePair<string, float>> rows, float total)
    {
        GUILayout.Label(title, _header);

        if (rows.Count == 0)
        {
            GUILayout.Label("No damage recorded yet.", _small);
            return;
        }

        for (int i = 0; i < rows.Count; i++)
        {
            KeyValuePair<string, float> row = rows[i];
            float percent = total > 0f ? row.Value / total * 100f : 0f;
            GUILayout.Label(
                string.Format("{0} — {1:N0} ({2:0.0}%)", row.Key, row.Value, percent),
                _row);
        }
    }

    private void EnsureStyles()
    {
        if (_header != null)
        {
            return;
        }

        _header = new GUIStyle(GUI.skin.label)
        {
            fontSize = 18,
            fontStyle = FontStyle.Bold,
            wordWrap = true
        };

        _row = new GUIStyle(GUI.skin.label)
        {
            fontSize = 14,
            wordWrap = true
        };

        _small = new GUIStyle(GUI.skin.label)
        {
            fontSize = 11,
            wordWrap = true
        };
    }
}
