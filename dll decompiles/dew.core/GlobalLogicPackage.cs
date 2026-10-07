using System;
using System.Collections.Generic;
using MagicaCloth2;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

public class GlobalLogicPackage : ManagerBase<GlobalLogicPackage>
{
	private static List<Action> _callbacks = new List<Action>();

	[NonSerialized]
	public bool showFps;

	private GUIStyle _fpsStyle;

	private float _averageFps = 1f;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void OnInit()
	{
		Profiler.maxUsedMemory = int.MaxValue;
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void AddGlobalLogicPackage()
	{
		if (!(UnityEngine.Object.FindObjectOfType<GlobalLogicPackage>() != null))
		{
			UnityEngine.Object.Instantiate(Resources.Load<GameObject>("GlobalLogicPackage"));
		}
	}

	public static void CallOnReady(Action callback)
	{
		if (ManagerBase<GlobalLogicPackage>.instance != null)
		{
			try
			{
				callback?.Invoke();
				return;
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
				return;
			}
		}
		_callbacks.Add(callback);
	}

	protected override void Awake()
	{
		base.Awake();
		MagicaManager.SetInitializationLocation((InitializationLocation)1);
		Debug.Log($"== System Report START ==\nVersion: {Application.version}\nCPU: {SystemInfo.processorType}\nCPU Cores: {SystemInfo.processorCount}\nCPU Frequency: {SystemInfo.processorFrequency}MHz\n\nRAM: {SystemInfo.systemMemorySize}MB\n\nGraphics Device Name: {SystemInfo.graphicsDeviceName}\nGraphics Device Type: {SystemInfo.graphicsDeviceType}\nGraphics Memory: {SystemInfo.graphicsMemorySize}MB\nGraphics Device Version: {SystemInfo.graphicsDeviceVersion}\nGraphics Shader Level: {SystemInfo.graphicsShaderLevel}\n\nOperating System: {SystemInfo.operatingSystem}\nDevice Model: {SystemInfo.deviceModel}\n\nSupports Compute Shaders: {SystemInfo.supportsComputeShaders}\nSupports Instancing: {SystemInfo.supportsInstancing}\n== System Report END ==\n");
		EntityVisual.PreloadCachedEffectPrefabs();
	}

	private void Start()
	{
		if (ManagerBase<GlobalLogicPackage>.instance != null && ManagerBase<GlobalLogicPackage>.instance != this)
		{
			UnityEngine.Object.Destroy(gameObject);
			return;
		}
		DebugManager.instance.enableRuntimeUI = false;
		UnityEngine.Object.DontDestroyOnLoad(gameObject);
		foreach (Action callback in _callbacks)
		{
			try
			{
				callback?.Invoke();
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
		_callbacks.Clear();
	}

	private void OnGUI()
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Expected Obj, but got Unknown
		//IL_007a: Expected Obj, but got Unknown
		if (showFps)
		{
			float num = 0.97f;
			if (!float.IsNormal(_averageFps))
			{
				_averageFps = 0f;
			}
			_averageFps = num * _averageFps + (1f - num) * 1f / Time.unscaledDeltaTime;
			if (_fpsStyle == null)
			{
				_fpsStyle = new GUIStyle
				{
					fontSize = 28,
					normal = new GUIStyleState
					{
						textColor = Color.green
					}
				};
			}
			GUILayout.Label($"{_averageFps:0} ({1f / Time.unscaledDeltaTime:0})", _fpsStyle, Array.Empty<GUILayoutOption>());
		}
	}

	private void OnApplicationQuit()
	{
		Debug.Log("Bye bye!");
	}
}
