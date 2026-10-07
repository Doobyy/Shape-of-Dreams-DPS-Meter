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
        CurrentBps,
        BarrierTotal,
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
    private const string DevelopmentVersion = "v5.38";

    private DpsData _data;
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

    private GUIStyle _header;
    private GUIStyle _headerRight;
    private GUIStyle _row;
    private GUIStyle _small;
    private GUIStyle _barBackground;
    private GUIStyle _barFill;
    private GUIStyle _rowRight;
    private Texture2D _whiteTexture;
    private Sprite _basicAttackIcon;
    private Texture2D _healingExpandIcon;
    private Texture2D _barrierExpandIcon;

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

            case DisplayMode.CurrentBps:
                DrawBarrierBreakdown(_data.CurrentPersonalBarrierRows, _data.CurrentInstancePersonalBarrier);
                break;

            case DisplayMode.BarrierTotal:
                DrawBarrierBreakdown(_data.CumulativeBarrierRows, _data.CumulativePersonalBarrier);
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
}
