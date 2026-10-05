using System;
using UnityEngine;

public class GraphicsManager : ManagerBase<GraphicsManager>
{
	[NonSerialized]
	public Quality3Levels? effectQualityOverride;

	[NonSerialized]
	public float? perfPressureStrengthOverride;

	public float sampleInterval = 1f;

	private float _accumulatedDeltaTime;

	private int _frameCount;

	private const float kLowFpsMemorySeconds = 5f;

	private float _lastLowFpsTime = float.NegativeInfinity;

	public Quality3Levels currentEffectQuality { get; private set; }

	public float perfPressureStrength { get; private set; }

	public static bool WasLowFpsInLast5Seconds()
	{
		GraphicsManager graphicsManager = ManagerBase<GraphicsManager>.softInstance;
		if (graphicsManager != null)
		{
			return Time.unscaledTime - graphicsManager._lastLowFpsTime < 5f;
		}
		return false;
	}

	public override void LogicUpdate(float dt)
	{
		base.LogicUpdate(dt);
		_accumulatedDeltaTime += Time.unscaledDeltaTime;
		_frameCount++;
		if (!(_accumulatedDeltaTime < sampleInterval))
		{
			float num = _accumulatedDeltaTime / (float)_frameCount;
			float currFps = ((num > 0f) ? (1f / num) : 0f);
			_accumulatedDeltaTime = 0f;
			_frameCount = 0;
			UpdateEffectQuality(currFps);
			UpdatePerfPressureStrength(currFps);
			if (perfPressureStrength > 0f)
			{
				_lastLowFpsTime = Time.unscaledTime;
			}
		}
	}

	private float GetDesiredFrameRate()
	{
		return Mathf.Min(Mathf.Min((int)Screen.currentResolution.refreshRateRatio.value, (DewSave.platformSettings.graphics.gameFrameLimit == -1) ? int.MaxValue : DewSave.platformSettings.graphics.gameFrameLimit), 90);
	}

	private void UpdateEffectQuality(float currFps)
	{
		if (effectQualityOverride.HasValue)
		{
			currentEffectQuality = effectQualityOverride.Value;
		}
		else
		{
			currentEffectQuality = DewSave.platformSettings.graphics.particleEffectQuality;
		}
	}

	private void UpdatePerfPressureStrength(float currFps)
	{
		if (perfPressureStrengthOverride.HasValue)
		{
			perfPressureStrength = perfPressureStrengthOverride.Value;
			return;
		}
		if (!DewSave.platformSettings.graphics.activityAdaptivePerformance)
		{
			perfPressureStrength = 0f;
			return;
		}
		float desiredFrameRate = GetDesiredFrameRate();
		float a = desiredFrameRate * 0.3f;
		float b = desiredFrameRate * 0.15f;
		perfPressureStrength = Mathf.InverseLerp(a, b, currFps);
	}
}
