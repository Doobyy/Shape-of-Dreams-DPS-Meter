using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class EffectAutoDestroy : MonoBehaviour
{
	private struct Entry
	{
		public GameObject gameObject;

		public Transform transform;

		public ParticleSystem rootParticleSystem;

		public IEffectComponent[] fxComponents;

		public float startTime;

		public float nextCheckTime;

		public bool pooled;

		public NetworkIdentity parent;

		public uint parentNetId;

		public ParticleSystem[] loopingParticleSystems;

		public IEffectComponent[] loopingFxComponents;
	}

	private sealed class EffectAutoDestroyRunner : MonoBehaviour
	{
		private void Update()
		{
			Tick();
		}
	}

	private const float AutoDestroyGracePeriod = 0.1f;

	private const float Timeout = 30f;

	private const float CheckInterval = 0.5f;

	private static readonly List<Entry> _entries = new List<Entry>(256);

	private static EffectAutoDestroyRunner _driver;

	private static ZoneManager _subscribedZoneManager;

	private static readonly List<ParticleSystem> s_psScratch = new List<ParticleSystem>(64);

	private static readonly List<ParticleSystem> s_loopingPsScratch = new List<ParticleSystem>(16);

	private void Awake()
	{
		Register(gameObject);
		UnityEngine.Object.Destroy(this);
	}

	public static void Register(GameObject gameObject, bool pooled = false, NetworkIdentity parent = null)
	{
		if (gameObject == null)
		{
			return;
		}
		ParticleSystem component2;
		IEffectComponent[] fxComponents;
		ParticleSystem[] array;
		IEffectComponent[] array2;
		if (pooled && gameObject.TryGetComponent<EffectAutoDestroyCache>(out var component))
		{
			component2 = component.rootParticleSystem;
			fxComponents = component.fxComponents;
			array = component.loopingParticleSystems;
			array2 = component.loopingFxComponents;
		}
		else
		{
			gameObject.TryGetComponent<ParticleSystem>(out component2);
			fxComponents = (pooled ? gameObject.GetComponentsInChildren<IEffectComponent>(includeInactive: true) : gameObject.GetComponentsInChildren<IEffectComponent>());
			if ((UnityEngine.Object)(object)component2 == null && gameObject.TryGetComponent<FxPlayPlan>(out var component3) && component3.rootParticleSystems.Length != 0)
			{
				component2 = component3.rootParticleSystems[0];
			}
			if (pooled || (UnityEngine.Object)(object)parent != null)
			{
				array = CollectLoopingParticleSystems(gameObject);
				array2 = CollectLoopingFxComponents(fxComponents);
			}
			else
			{
				array = Array.Empty<ParticleSystem>();
				array2 = Array.Empty<IEffectComponent>();
			}
			if (pooled)
			{
				component = gameObject.AddComponent<EffectAutoDestroyCache>();
				component.rootParticleSystem = component2;
				component.fxComponents = fxComponents;
				component.resetParticleSystems = DewEffect.BakeResetParticleSystems(gameObject);
				component.loopingParticleSystems = array;
				component.loopingFxComponents = array2;
				component.attachables = gameObject.GetComponentsInChildren<IAttachableToEntity>(includeInactive: true);
			}
		}
		AddEntry(gameObject, component2, fxComponents, pooled, parent, array, array2);
	}

	public static void Register(GameObject gameObject, IEffectComponent[] fxComponents)
	{
		if (!(gameObject == null))
		{
			gameObject.TryGetComponent<ParticleSystem>(out var component);
			if ((UnityEngine.Object)(object)component == null && gameObject.TryGetComponent<FxPlayPlan>(out var component2) && component2.rootParticleSystems.Length != 0)
			{
				component = component2.rootParticleSystems[0];
			}
			AddEntry(gameObject, component, fxComponents, pooled: false);
		}
	}

	private static void AddEntry(GameObject gameObject, ParticleSystem ps, IEffectComponent[] fxComponents, bool pooled, NetworkIdentity parent = null, ParticleSystem[] loopingPs = null, IEffectComponent[] loopingFx = null)
	{
		EnsureDriver();
		EnsureZoneSubscription();
		bool flag = (UnityEngine.Object)(object)parent != null && ((loopingPs != null && loopingPs.Length != 0) || (loopingFx != null && loopingFx.Length != 0));
		_entries.Add(new Entry
		{
			gameObject = gameObject,
			transform = gameObject.transform,
			rootParticleSystem = ps,
			fxComponents = fxComponents,
			startTime = Time.time,
			nextCheckTime = 0f,
			pooled = pooled,
			parent = (flag ? parent : null),
			parentNetId = (flag ? parent.netId : 0u),
			loopingParticleSystems = loopingPs,
			loopingFxComponents = loopingFx
		});
	}

	private static ParticleSystem[] CollectLoopingParticleSystems(GameObject gameObject)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		gameObject.GetComponentsInChildren(includeInactive: true, s_psScratch);
		s_loopingPsScratch.Clear();
		for (int i = 0; i < s_psScratch.Count; i++)
		{
			ParticleSystem val = s_psScratch[i];
			MainModule main = val.main;
			if (main.loop)
			{
				EmissionModule emission = val.emission;
				if (emission.enabled)
				{
					s_loopingPsScratch.Add(val);
				}
			}
		}
		s_psScratch.Clear();
		if (s_loopingPsScratch.Count != 0)
		{
			return s_loopingPsScratch.ToArray();
		}
		return Array.Empty<ParticleSystem>();
	}

	private static IEffectComponent[] CollectLoopingFxComponents(IEffectComponent[] fxComponents)
	{
		int num = 0;
		int num2 = 0;
		while (fxComponents != null && num2 < fxComponents.Length)
		{
			if (fxComponents[num2].isLooping)
			{
				num++;
			}
			num2++;
		}
		if (num == 0)
		{
			return Array.Empty<IEffectComponent>();
		}
		IEffectComponent[] array = new IEffectComponent[num];
		int num3 = 0;
		for (int i = 0; i < fxComponents.Length; i++)
		{
			if (fxComponents[i].isLooping)
			{
				array[num3++] = fxComponents[i];
			}
		}
		return array;
	}

	public static void DestroyAll()
	{
		for (int num = _entries.Count - 1; num >= 0; num--)
		{
			GameObject gameObject = _entries[num].gameObject;
			if (gameObject != null)
			{
				UnityEngine.Object.Destroy(gameObject);
			}
		}
		_entries.Clear();
	}

	internal static void Tick()
	{
		if (_entries.Count == 0)
		{
			return;
		}
		EnsureZoneSubscription();
		float time = Time.time;
		bool flag = (UnityEngine.Object)(object)NetworkedManagerBase<ZoneManager>.softInstance != null && NetworkedManagerBase<ZoneManager>.softInstance.isInRoomTransition;
		float num = ((ManagerBase<GraphicsManager>.instance != null) ? ManagerBase<GraphicsManager>.instance.perfPressureStrength : 0f);
		for (int num2 = _entries.Count - 1; num2 >= 0; num2--)
		{
			Entry e = _entries[num2];
			if (e.gameObject == null)
			{
				SwapRemoveAt(num2);
			}
			else if (!e.gameObject.activeInHierarchy && e.gameObject.scene.isLoaded)
			{
				DestroyEntry(num2);
			}
			else if (num > 0f && UnityEngine.Random.value < num)
			{
				DestroyEntry(num2);
			}
			else if (!(time < e.nextCheckTime))
			{
				e.nextCheckTime = time + 0.5f;
				if (e.parentNetId != 0 && ((UnityEngine.Object)(object)e.parent == null || e.parent.netId != e.parentNetId || !((Component)(object)e.parent).gameObject.activeInHierarchy))
				{
					IEffectComponent[] loopingFxComponents = e.loopingFxComponents;
					int num3 = 0;
					while (loopingFxComponents != null && num3 < loopingFxComponents.Length)
					{
						if (!(loopingFxComponents[num3] is UnityEngine.Object obj) || (bool)obj)
						{
							loopingFxComponents[num3].Stop();
						}
						num3++;
					}
					ParticleSystem[] loopingParticleSystems = e.loopingParticleSystems;
					int num4 = 0;
					while (loopingParticleSystems != null && num4 < loopingParticleSystems.Length)
					{
						if ((UnityEngine.Object)(object)loopingParticleSystems[num4] != null)
						{
							loopingParticleSystems[num4].Stop(false, (ParticleSystemStopBehavior)1);
						}
						num4++;
					}
					e.parentNetId = 0u;
				}
				_entries[num2] = e;
				if (time - e.startTime > 30f)
				{
					LogTimeout(in e);
					DestroyEntry(num2);
				}
				else if (!(time - e.startTime < 0.1f))
				{
					if (flag)
					{
						DestroyEntry(num2);
					}
					else if ((!((UnityEngine.Object)(object)e.rootParticleSystem != null) || !e.rootParticleSystem.IsAlive()) && !AnyPlaying(e.fxComponents))
					{
						DestroyEntry(num2);
					}
				}
			}
		}
	}

	public static bool AnyPlaying(IEffectComponent[] fxComponents)
	{
		if (fxComponents == null)
		{
			return false;
		}
		foreach (IEffectComponent effectComponent in fxComponents)
		{
			if ((!(effectComponent is UnityEngine.Object obj) || (bool)obj) && effectComponent.isPlaying)
			{
				return true;
			}
		}
		return false;
	}

	private static void DestroyEntry(int index)
	{
		Entry entry = _entries[index];
		GameObject gameObject = entry.gameObject;
		SwapRemoveAt(index);
		if (gameObject == null)
		{
			return;
		}
		if (entry.pooled)
		{
			IEffectComponent[] fxComponents = entry.fxComponents;
			for (int i = 0; i < fxComponents.Length; i++)
			{
				if (!(fxComponents[i] is UnityEngine.Object obj) || (bool)obj)
				{
					fxComponents[i].Stop();
				}
			}
			if (gameObject.TryGetComponent<EffectAutoDestroyCache>(out var component))
			{
				DewEffect.StopResetParticleSystems(component.resetParticleSystems);
				IAttachableToEntity[] attachables = component.attachables;
				for (int j = 0; j < attachables.Length; j++)
				{
					if (!(attachables[j] is UnityEngine.Object obj2) || (bool)obj2)
					{
						attachables[j].OnAttachToEntity(null);
					}
				}
			}
			gameObject.SetActive(value: false);
		}
		else if (ManagerBase<SpawnManager>.softInstance == null)
		{
			UnityEngine.Object.Destroy(gameObject);
		}
		else
		{
			SpawnManager.Destroy(gameObject);
		}
	}

	private static void SwapRemoveAt(int index)
	{
		int num = _entries.Count - 1;
		if (index != num)
		{
			_entries[index] = _entries[num];
		}
		_entries.RemoveAt(num);
	}

	private static void LogTimeout(in Entry e)
	{
		string text = "";
		for (int i = 0; i < e.fxComponents.Length; i++)
		{
			IEffectComponent effectComponent = e.fxComponents[i];
			if ((!(effectComponent is UnityEngine.Object obj) || (bool)obj) && effectComponent.isPlaying)
			{
				text = ((!(effectComponent is Component component)) ? (text + "Unknown (???) ") : (text + component.name + " (" + component.GetType().Name + ") "));
			}
		}
		if ((UnityEngine.Object)(object)e.rootParticleSystem != null && e.rootParticleSystem.IsAlive())
		{
			text = text + ((UnityEngine.Object)(object)e.rootParticleSystem).name + " (ParticleSystem)";
		}
		Debug.LogWarning($"Effect '{e.transform.GetScenePath()}' was timed out ({30f} seconds) waiting for: {text}");
	}

	private static void OnRoomChange(EventInfoLoadRoom _)
	{
		for (int num = _entries.Count - 1; num >= 0; num--)
		{
			DestroyEntry(num);
		}
	}

	private static void EnsureDriver()
	{
		if (!(_driver != null))
		{
			GameObject gameObject = new GameObject("[EffectAutoDestroyDriver]")
			{
				hideFlags = HideFlags.HideAndDontSave
			};
			UnityEngine.Object.DontDestroyOnLoad(gameObject);
			_driver = gameObject.AddComponent<EffectAutoDestroyRunner>();
		}
	}

	private static void EnsureZoneSubscription()
	{
		ZoneManager softInstance = NetworkedManagerBase<ZoneManager>.softInstance;
		if (!((UnityEngine.Object)(object)softInstance == (UnityEngine.Object)(object)_subscribedZoneManager))
		{
			if ((UnityEngine.Object)(object)_subscribedZoneManager != null)
			{
				_subscribedZoneManager.ClientEvent_OnRoomLoaded -= new Action<EventInfoLoadRoom>(OnRoomChange);
			}
			if ((UnityEngine.Object)(object)softInstance != null)
			{
				softInstance.ClientEvent_OnRoomLoaded += new Action<EventInfoLoadRoom>(OnRoomChange);
			}
			_subscribedZoneManager = softInstance;
		}
	}
}
