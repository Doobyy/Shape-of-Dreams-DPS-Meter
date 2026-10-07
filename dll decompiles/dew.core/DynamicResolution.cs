using System;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public sealed class DynamicResolution : MonoBehaviour
{
	public static Func<int> targetFpsProvider;

	public static Func<bool> suspendProvider;

	public static float currentScale = 1f;

	private const int TargetFps = 60;

	private const float Safety = 0.85f;

	private const float UpBand = 0.8f;

	private const float DownBand = 0.9f;

	private const float Damping = 0.5f;

	private const float MinScale = 0.5f;

	private const float MaxScale = 1f;

	private const float MaxStepDownPerSec = 1.5f;

	private const float MaxStepUpPerSec = 0.4f;

	private const float ApplyEpsilon = 0.01f;

	private const float SnapStep = 0.05f;

	private const uint SampleCount = 4u;

	private const int WarmupFrames = 30;

	private Camera _camera;

	private float _scale = 1f;

	private float _applied = 1f;

	private int _frame;

	private readonly FrameTiming[] _timings = new FrameTiming[8];

	private static bool s_fixedOverride;

	private static float s_fixedScale = 1f;

	private static bool s_ignoreLoad;

	private static bool s_simActive;

	private static int s_simPhase;

	private static float s_simScale;

	private static float s_simLow;

	private static float s_simHigh;

	private static float s_simRatePerSec;

	private static int s_simCyclesLeft;

	private void Awake()
	{
		_camera = GetComponent<Camera>();
	}

	private void OnEnable()
	{
		if (_camera != null)
		{
			_camera.allowDynamicResolution = true;
		}
		_scale = ScalableBufferManager.widthScaleFactor;
		_applied = _scale;
		_frame = 0;
		s_fixedOverride = false;
		s_simActive = false;
		s_simPhase = 0;
	}

	private void OnDisable()
	{
		ScalableBufferManager.ResizeBuffers(1f, 1f);
		currentScale = 1f;
		if (_camera != null)
		{
			_camera.allowDynamicResolution = false;
		}
	}

	private void LateUpdate()
	{
		if (_camera == null)
		{
			return;
		}
		if (s_ignoreLoad)
		{
			_scale = 1f;
			ApplyScale(_scale);
			_frame = 0;
			return;
		}
		if (s_simActive)
		{
			StepScaleSweep();
			return;
		}
		if (s_fixedOverride)
		{
			_scale = s_fixedScale;
			ApplyScale(_scale);
			return;
		}
		if (!DewSave.platformSettings.graphics.enableDynamicResolution)
		{
			_scale = 1f;
			ApplyScale(_scale);
			_frame = 0;
			return;
		}
		bool num = suspendProvider != null && suspendProvider();
		bool flag = ManagerBase<TransitionManager>.softInstance.state == TransitionManager.StateType.Loading;
		if (num | flag)
		{
			_scale = 1f;
			ApplyScale(_scale);
			_frame = 0;
		}
		else
		{
			if (_frame++ < 30)
			{
				return;
			}
			FrameTimingManager.CaptureFrameTimings();
			uint latestTimings = FrameTimingManager.GetLatestTimings(4u, _timings);
			if (latestTimings == 0)
			{
				return;
			}
			double num2 = 0.0;
			int num3 = 0;
			for (int i = 0; i < latestTimings; i++)
			{
				double gpuFrameTime = _timings[i].gpuFrameTime;
				if (gpuFrameTime > 0.0)
				{
					num2 += gpuFrameTime;
					num3++;
				}
			}
			if (num3 == 0)
			{
				return;
			}
			float num4 = (float)(num2 / (double)num3);
			if (!(num4 <= 0.01f))
			{
				int num5 = ((targetFpsProvider != null) ? targetFpsProvider() : 60);
				if (num5 < 1)
				{
					num5 = 60;
				}
				float num6 = 1000f / (float)num5;
				float num7 = num6 * 0.85f;
				float value = _scale;
				if (num4 > num6 * 0.9f || num4 < num6 * 0.8f)
				{
					value = _scale * (1f + 0.5f * (Mathf.Sqrt(num7 / num4) - 1f));
				}
				value = Mathf.Clamp(value, 0.5f, 1f);
				float maxDelta = ((value < _scale) ? 1.5f : 0.4f) * Time.unscaledDeltaTime;
				_scale = Mathf.MoveTowards(_scale, value, maxDelta);
				ApplyScale(_scale);
			}
		}
	}

	private void ApplyScale(float s)
	{
		float value = Mathf.Round(s / 0.05f) * 0.05f;
		value = Mathf.Clamp(value, 0.1f, 1f);
		if (!(Mathf.Abs(value - _applied) < 0.01f))
		{
			_applied = value;
			currentScale = value;
			ScalableBufferManager.ResizeBuffers(value, value);
		}
	}

	public static void SetFixedScale(float scale)
	{
		s_fixedScale = Mathf.Clamp(scale, 0.3f, 1f);
		s_fixedOverride = true;
	}

	public static void ClearFixedScale()
	{
		s_fixedOverride = false;
	}

	public static void SetIgnoreLoad(bool ignore)
	{
		s_ignoreLoad = ignore;
	}

	public static void StartScaleSweep(float low, float high, float ratePerSec, int cycles)
	{
		s_simLow = Mathf.Clamp(low, 0.1f, 1f);
		s_simHigh = Mathf.Clamp(high, s_simLow + 0.05f, 1f);
		s_simRatePerSec = Mathf.Max(0.05f, ratePerSec);
		s_simCyclesLeft = Mathf.Max(1, cycles);
		s_simScale = s_simHigh;
		s_simPhase = 1;
		s_simActive = true;
		s_fixedOverride = false;
		Debug.Log($"[DynamicResolution] sweep START: {s_simHigh:0.00}<->{s_simLow:0.00} x{s_simCyclesLeft} @ {s_simRatePerSec:0.00}/s");
	}

	public static void StopSimulation()
	{
		s_simActive = false;
		s_simPhase = 0;
	}

	private void StepScaleSweep()
	{
		float num = s_simRatePerSec * Time.unscaledDeltaTime;
		if (s_simPhase == 1)
		{
			s_simScale -= num;
			if (s_simScale <= s_simLow)
			{
				s_simScale = s_simLow;
				s_simPhase = 2;
				Debug.Log($"[DynamicResolution] sweep LOW {s_simLow:0.00}, scale={ScalableBufferManager.widthScaleFactor:0.00}, cyclesLeft={s_simCyclesLeft} -> up");
			}
		}
		else
		{
			s_simScale += num;
			if (s_simScale >= s_simHigh)
			{
				s_simScale = s_simHigh;
				s_simCyclesLeft--;
				if (s_simCyclesLeft <= 0)
				{
					ApplyScaleDirect(s_simHigh);
					s_simActive = false;
					s_simPhase = 0;
					Debug.Log("[DynamicResolution] sweep DONE");
					return;
				}
				s_simPhase = 1;
			}
		}
		ApplyScaleDirect(s_simScale);
	}

	private void ApplyScaleDirect(float s)
	{
		s = Mathf.Clamp(s, 0.1f, 1f);
		if (!(Mathf.Abs(s - _applied) < 0.005f))
		{
			_applied = s;
			_scale = s;
			currentScale = s;
			ScalableBufferManager.ResizeBuffers(s, s);
		}
	}
}
