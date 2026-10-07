using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Cinemachine;
using DewInternal;
using Mirror;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VFX;

public static class DewEffect
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public readonly struct RumblePlayScope : IDisposable
	{
		public void Dispose()
		{
			_rumblePlayScopeDepth = Mathf.Max(0, _rumblePlayScopeDepth - 1);
		}
	}

	public struct PlayEffectMessage : NetworkMessage
	{
		public bool isNew;

		public NetworkIdentity parent;

		public int pathId;
	}

	public struct PlayPositionedEffectMessage : NetworkMessage
	{
		public bool isNew;

		public bool isPooled;

		public NetworkIdentity parent;

		public int pathId;

		public Vector3 position;

		public Quaternion rotation;
	}

	public struct PlayAttachedEffectMessage : NetworkMessage
	{
		public bool isNew;

		public bool isPooled;

		public NetworkIdentity parent;

		public int pathId;

		public bool isPositioned;

		public Vector3 position;

		public Quaternion rotation;

		public Entity entity;
	}

	public struct PlayCastEffectMessage : NetworkMessage
	{
		public NetworkIdentity parent;

		public int pathId;

		public CastInfo info;

		public CastMethodType method;

		public float duration;
	}

	public struct StopEffectMessage : NetworkMessage
	{
		public NetworkIdentity parent;

		public int pathId;
	}

	public struct ApplySpeedMultiplierToEffectMessage : NetworkMessage
	{
		public float multiplier;

		public NetworkIdentity parent;

		public int pathId;
	}

	private class FxPool
	{
		public readonly List<GameObject> instances = new List<GameObject>();

		public int plays;

		public int roomsSinceUsed;

		public GameObject source;

		public bool hasSource;
	}

	private class FxParamComponents
	{
		public DewAudioSource[] audios;

		public FxCameraShake[] shakes;

		public FxInterpolatedEffectBase[] interps;

		public FxSyncChildScale[] scaledChildren;
	}

	public struct FxPoolStat
	{
		public string label;

		public string caller;

		public bool networked;

		public int instantiated;

		public int plays;
	}

	public class FxColorSnapshot
	{
		public readonly List<ParticleSystem> ps = new List<ParticleSystem>();

		public readonly List<MinMaxGradient> psStart = new List<MinMaxGradient>();

		public readonly List<MinMaxGradient> psColLife = new List<MinMaxGradient>();

		public readonly List<MinMaxGradient> psColSpeed = new List<MinMaxGradient>();

		public readonly List<bool> psHasCustom1 = new List<bool>();

		public readonly List<MinMaxGradient> psCustom1 = new List<MinMaxGradient>();

		public readonly List<bool> psHasCustom2 = new List<bool>();

		public readonly List<MinMaxGradient> psCustom2 = new List<MinMaxGradient>();

		public readonly List<Light> lights = new List<Light>();

		public readonly List<Color> lightColors = new List<Color>();

		public readonly List<FxEntityColor> entityColors = new List<FxEntityColor>();

		public readonly List<Color> ecBase = new List<Color>();

		public readonly List<Color> ecEmission = new List<Color>();

		public readonly List<FxMeshTrail> trails = new List<FxMeshTrail>();

		public readonly List<Gradient> trailGrads = new List<Gradient>();

		public readonly List<FxEntityShell> shells = new List<FxEntityShell>();

		public readonly List<Color> shellColors = new List<Color>();
	}

	public static bool disablePlay;

	public static bool disablePlayNew;

	private static int _rumblePlayScopeDepth;

	private static Dictionary<int, int> _effectsInThisFrame = new Dictionary<int, int>();

	private static int _effectsInThisFrameCount = 0;

	private static readonly Dictionary<GameObject, FxPool> _pool = new Dictionary<GameObject, FxPool>();

	private static readonly Dictionary<(uint assetId, int pathId, bool toned, int skinVar), FxPool> _netPool = new Dictionary<(uint, int, bool, int), FxPool>();

	private static Transform _poolRoot;

	private const int kMaxActiveOneShotsPerPool = 10;

	private static readonly Dictionary<GameObject, ArcTelegraphController[]> _templateArcTelegraphs = new Dictionary<GameObject, ArcTelegraphController[]>();

	private static readonly Dictionary<GameObject, BoxTelegraphController[]> _templateBoxTelegraphs = new Dictionary<GameObject, BoxTelegraphController[]>();

	private static readonly Dictionary<GameObject, FxParamComponents> _fxParamComponents = new Dictionary<GameObject, FxParamComponents>();

	private const int kPoolDropAfterRooms = 3;

	private static readonly Dictionary<uint, string> _callerNameCache = new Dictionary<uint, string>();

	private static readonly Dictionary<(uint assetId, int pathId), string> _effectLabelCache = new Dictionary<(uint, int), string>();

	private static readonly Dictionary<ParticleSystem, float> _psBaseSimSpeed = new Dictionary<ParticleSystem, float>();

	private static readonly Dictionary<GameObject, float> _lastAppliedSpeedMul = new Dictionary<GameObject, float>();

	private static readonly Dictionary<GameObject, float> _cloneBirthSpeedMul = new Dictionary<GameObject, float>();

	private static readonly StringBuilder _getPathSb = new StringBuilder(64);

	private static readonly Dictionary<int, string> _pathCache = new Dictionary<int, string>();

	private static readonly Dictionary<int, int> _pathIdCache = new Dictionary<int, int>();

	private const uint kFnvOffset = 2166136261u;

	private const uint kFnvPrime = 16777619u;

	private const int kRootPathId = 0;

	private const int kInvalidPathId = int.MinValue;

	private static readonly Dictionary<ulong, bool> _sourceSkinnable = new Dictionary<ulong, bool>();

	private static readonly Dictionary<ulong, Dictionary<int, int[]>> _sourcePathCache = new Dictionary<ulong, Dictionary<int, int[]>>();

	private static readonly List<int> _indexPathBuffer = new List<int>();

	private static List<Light> _psLights = new List<Light>();

	private static readonly Dictionary<Gradient, (GradientColorKey[] orig, GradientColorKey[] work)> _gradientKeyCache = new Dictionary<Gradient, (GradientColorKey[], GradientColorKey[])>();

	public static bool isRumbleEnabledForCurrentPlay => _rumblePlayScopeDepth > 0;

	public static RumblePlayScope EnableRumbleForPlay()
	{
		_rumblePlayScopeDepth++;
		return default;
	}

	public static void PlayNewNetworked(NetworkIdentity parent, GameObject effect, Vector3 position, Quaternion? rotation)
	{
		if (AssertServerActive() && !(effect == null))
		{
			if (!rotation.HasValue)
			{
				rotation = ManagerBase<CameraManager>.instance.entityCamAngleRotation;
			}
			NetworkServer.SendToAll<PlayPositionedEffectMessage>(new PlayPositionedEffectMessage
			{
				isNew = true,
				parent = parent,
				pathId = GetPathId(((Component)(object)parent).transform, effect.transform),
				position = position,
				rotation = rotation.Value
			}, 0, false);
		}
	}

	public static void PlayNewNetworked(NetworkIdentity parent, GameObject effect, Entity entity)
	{
		if (AssertServerActive() && !(effect == null) && (!((UnityEngine.Object)(object)entity != null) || ((NetworkBehaviour)entity).netId != 0))
		{
			NetworkServer.SendToAll<PlayAttachedEffectMessage>(new PlayAttachedEffectMessage
			{
				isNew = true,
				parent = parent,
				pathId = GetPathId(((Component)(object)parent).transform, effect.transform),
				entity = entity
			}, 0, false);
		}
	}

	public static void PlayNewNetworked(NetworkIdentity parent, GameObject effect, Entity entity, Vector3 position, Quaternion? rotation)
	{
		if (AssertServerActive() && !(effect == null) && (!((UnityEngine.Object)(object)entity != null) || ((NetworkBehaviour)entity).netId != 0))
		{
			if (!rotation.HasValue)
			{
				rotation = ManagerBase<CameraManager>.instance.entityCamAngleRotation;
			}
			NetworkServer.SendToAll<PlayAttachedEffectMessage>(new PlayAttachedEffectMessage
			{
				isNew = true,
				parent = parent,
				pathId = GetPathId(((Component)(object)parent).transform, effect.transform),
				entity = entity,
				isPositioned = true,
				position = position,
				rotation = rotation.Value
			}, 0, false);
		}
	}

	public static void PlayNewNetworked(NetworkIdentity parent, GameObject effect)
	{
		if (AssertServerActive() && !(effect == null))
		{
			NetworkServer.SendToAll<PlayEffectMessage>(new PlayEffectMessage
			{
				isNew = true,
				parent = parent,
				pathId = GetPathId(((Component)(object)parent).transform, effect.transform)
			}, 0, false);
		}
	}

	public static void PlayNetworked(NetworkIdentity parent, GameObject effect)
	{
		if (AssertServerActive() && !(effect == null))
		{
			NetworkServer.SendToAll<PlayEffectMessage>(new PlayEffectMessage
			{
				isNew = false,
				parent = parent,
				pathId = GetPathId(((Component)(object)parent).transform, effect.transform)
			}, 0, false);
		}
	}

	public static void PlayNetworked(NetworkIdentity parent, GameObject effect, Entity entity)
	{
		if (AssertServerActive() && !(effect == null) && (!((UnityEngine.Object)(object)entity != null) || ((NetworkBehaviour)entity).netId != 0))
		{
			NetworkServer.SendToAll<PlayAttachedEffectMessage>(new PlayAttachedEffectMessage
			{
				isNew = false,
				parent = parent,
				pathId = GetPathId(((Component)(object)parent).transform, effect.transform),
				entity = entity
			}, 0, false);
		}
	}

	public static void PlayNetworked(NetworkIdentity parent, GameObject effect, Vector3 position, Quaternion? rotation)
	{
		if (AssertServerActive() && !(effect == null))
		{
			if (!rotation.HasValue)
			{
				rotation = ManagerBase<CameraManager>.instance.entityCamAngleRotation;
			}
			NetworkServer.SendToAll<PlayPositionedEffectMessage>(new PlayPositionedEffectMessage
			{
				isNew = false,
				parent = parent,
				pathId = GetPathId(((Component)(object)parent).transform, effect.transform),
				position = position,
				rotation = rotation.Value
			}, 0, false);
		}
	}

	public static void PlayNetworked(NetworkIdentity parent, GameObject effect, Entity entity, Vector3 position, Quaternion? rotation)
	{
		if (AssertServerActive() && !(effect == null) && (!((UnityEngine.Object)(object)entity != null) || ((NetworkBehaviour)entity).netId != 0))
		{
			if (!rotation.HasValue)
			{
				rotation = ManagerBase<CameraManager>.instance.entityCamAngleRotation;
			}
			NetworkServer.SendToAll<PlayAttachedEffectMessage>(new PlayAttachedEffectMessage
			{
				isNew = false,
				parent = parent,
				pathId = GetPathId(((Component)(object)parent).transform, effect.transform),
				entity = entity,
				isPositioned = true,
				position = position,
				rotation = rotation.Value
			}, 0, false);
		}
	}

	public static void PlayCastEffectNetworked(NetworkIdentity parent, GameObject effect, CastInfo info, CastMethodType method, float duration = -1f)
	{
		if (AssertServerActive() && !(effect == null))
		{
			NetworkServer.SendToAll<PlayCastEffectMessage>(new PlayCastEffectMessage
			{
				parent = parent,
				pathId = GetPathId(((Component)(object)parent).transform, effect.transform),
				info = info,
				method = method,
				duration = duration
			}, 0, false);
		}
	}

	public static void StopNetworked(NetworkIdentity parent, GameObject effect)
	{
		if (AssertServerActive() && !(effect == null))
		{
			NetworkServer.SendToAll<StopEffectMessage>(new StopEffectMessage
			{
				parent = parent,
				pathId = GetPathId(((Component)(object)parent).transform, effect.transform)
			}, 0, false);
		}
	}

	public static void ApplySpeedMultiplierNetworked(NetworkIdentity parent, GameObject effect, float multiplier)
	{
		if (AssertServerActive() && !(effect == null))
		{
			NetworkServer.SendToAll<ApplySpeedMultiplierToEffectMessage>(new ApplySpeedMultiplierToEffectMessage
			{
				parent = parent,
				pathId = GetPathId(((Component)(object)parent).transform, effect.transform),
				multiplier = multiplier
			}, 0, false);
		}
	}

	internal static void HandleEffectMessage(PlayPositionedEffectMessage msg)
	{
		if ((UnityEngine.Object)(object)msg.parent == null)
		{
			if (!((UnityEngine.Object)(object)DewPlayer.local != null) || DewPlayer.local.state != PlayerState.InLobby || !((UnityEngine.Object)(object)NetworkedManagerBase<GameSettingsManager>.instance != null) || NetworkedManagerBase<GameSettingsManager>.instance.state != GameState.InGame)
			{
				Debug.Log($"Effect '{msg.pathId}' parent not found");
			}
			return;
		}
		GameObject gameObject = ResolvePathById(msg.parent, msg.pathId);
		if (gameObject == null)
		{
			Debug.Log($"Effect '{((UnityEngine.Object)(object)msg.parent).name}/{msg.pathId}' not found");
		}
		else if (msg.isPooled || msg.isNew)
		{
			PlayPooledNetworkedLocal((assetId: msg.parent.assetId, pathId: msg.pathId, toned: IsTonedDownForOtherPlayer(msg.parent), skinVar: GetSkinVisualVariantId(msg.parent)), gameObject, null, positioned: true, msg.position, msg.rotation, msg.parent);
		}
		else
		{
			Play(gameObject, msg.position, msg.rotation, msg.parent);
		}
	}

	internal static void HandleEffectMessage(PlayAttachedEffectMessage msg)
	{
		if ((UnityEngine.Object)(object)msg.parent == null)
		{
			if (!((UnityEngine.Object)(object)DewPlayer.local != null) || DewPlayer.local.state != PlayerState.InLobby || !((UnityEngine.Object)(object)NetworkedManagerBase<GameSettingsManager>.instance != null) || NetworkedManagerBase<GameSettingsManager>.instance.state != GameState.InGame)
			{
				Debug.Log($"Effect '{msg.pathId}' parent not found");
			}
			return;
		}
		GameObject gameObject = ResolvePathById(msg.parent, msg.pathId);
		if (gameObject == null)
		{
			Debug.Log($"Effect '{((UnityEngine.Object)(object)msg.parent).name}/{msg.pathId}' not found");
		}
		else if (msg.isPooled || msg.isNew)
		{
			PlayPooledNetworkedLocal((assetId: msg.parent.assetId, pathId: msg.pathId, toned: IsTonedDownForOtherPlayer(msg.parent), skinVar: GetSkinVisualVariantId(msg.parent)), gameObject, msg.entity, msg.isPositioned, msg.position, msg.rotation, msg.parent);
		}
		else if (msg.isPositioned)
		{
			Play(gameObject, msg.entity, msg.position, msg.rotation, msg.parent);
		}
		else
		{
			Play(gameObject, msg.entity, msg.parent);
		}
	}

	internal static void HandleEffectMessage(PlayEffectMessage msg)
	{
		if ((UnityEngine.Object)(object)msg.parent == null)
		{
			if (!((UnityEngine.Object)(object)DewPlayer.local != null) || DewPlayer.local.state != PlayerState.InLobby || !((UnityEngine.Object)(object)NetworkedManagerBase<GameSettingsManager>.instance != null) || NetworkedManagerBase<GameSettingsManager>.instance.state != GameState.InGame)
			{
				Debug.Log($"Effect '{msg.pathId}' parent not found");
			}
			return;
		}
		GameObject gameObject = ResolvePathById(msg.parent, msg.pathId);
		if (gameObject == null)
		{
			Debug.Log($"Effect '{((UnityEngine.Object)(object)msg.parent).name}/{msg.pathId}' not found");
		}
		else if (msg.isNew)
		{
			Transform transform = gameObject.transform;
			PlayPooledNetworkedLocal((assetId: msg.parent.assetId, pathId: msg.pathId, toned: IsTonedDownForOtherPlayer(msg.parent), skinVar: GetSkinVisualVariantId(msg.parent)), gameObject, null, positioned: true, transform.position, transform.rotation, msg.parent);
		}
		else
		{
			Play(gameObject, msg.parent);
		}
	}

	internal static void HandleEffectMessage(PlayCastEffectMessage msg)
	{
		if ((UnityEngine.Object)(object)msg.parent == null)
		{
			if (!((UnityEngine.Object)(object)DewPlayer.local != null) || DewPlayer.local.state != PlayerState.InLobby || !((UnityEngine.Object)(object)NetworkedManagerBase<GameSettingsManager>.instance != null) || NetworkedManagerBase<GameSettingsManager>.instance.state != GameState.InGame)
			{
				Debug.Log($"Effect '{msg.pathId}' parent not found");
			}
			return;
		}
		GameObject gameObject = ResolvePathById(msg.parent, msg.pathId);
		if (gameObject == null)
		{
			Debug.Log($"Effect '{((UnityEngine.Object)(object)msg.parent).name}/{msg.pathId}' not found");
		}
		else
		{
			PlayCastEffect(gameObject, msg.info, msg.method, msg.duration, msg.parent);
		}
	}

	internal static void HandleEffectMessage(StopEffectMessage msg)
	{
		if ((UnityEngine.Object)(object)msg.parent == null)
		{
			if (!((UnityEngine.Object)(object)DewPlayer.local != null) || DewPlayer.local.state != PlayerState.InLobby || !((UnityEngine.Object)(object)NetworkedManagerBase<GameSettingsManager>.instance != null) || NetworkedManagerBase<GameSettingsManager>.instance.state != GameState.InGame)
			{
				Debug.Log($"Effect '{msg.pathId}' parent not found");
			}
			return;
		}
		GameObject gameObject = ResolvePathById(msg.parent, msg.pathId);
		if (gameObject == null)
		{
			Debug.Log($"Effect '{((UnityEngine.Object)(object)msg.parent).name}/{msg.pathId}' not found");
		}
		else
		{
			Stop(gameObject);
		}
	}

	internal static void HandleEffectMessage(ApplySpeedMultiplierToEffectMessage msg)
	{
		if ((UnityEngine.Object)(object)msg.parent == null)
		{
			if (!((UnityEngine.Object)(object)DewPlayer.local != null) || DewPlayer.local.state != PlayerState.InLobby || !((UnityEngine.Object)(object)NetworkedManagerBase<GameSettingsManager>.instance != null) || NetworkedManagerBase<GameSettingsManager>.instance.state != GameState.InGame)
			{
				Debug.Log($"Effect '{msg.pathId}' parent not found");
			}
			return;
		}
		GameObject gameObject = ResolvePathById(msg.parent, msg.pathId);
		if (gameObject == null)
		{
			Debug.Log($"Effect '{((UnityEngine.Object)(object)msg.parent).name}/{msg.pathId}' not found");
		}
		else
		{
			ApplySpeedMultiplier(gameObject, msg.multiplier);
		}
	}

	public static GameObject PlayNew(GameObject effect, NetworkIdentity parent = null)
	{
		if (disablePlayNew)
		{
			return null;
		}
		if (effect == null)
		{
			return null;
		}
		if (!CheckLimit(effect, parent))
		{
			return null;
		}
		return PlayIntoPool(GetPoolForPlayNew(effect, parent), effect, null, positioned: true, effect.transform.position, effect.transform.rotation, parent);
	}

	public static GameObject PlayNew(GameObject effect, Vector3 position, Quaternion? rotation, NetworkIdentity parent = null)
	{
		if (disablePlayNew)
		{
			return null;
		}
		if (effect == null)
		{
			return null;
		}
		if (!rotation.HasValue)
		{
			rotation = ManagerBase<CameraManager>.instance.entityCamAngleRotation;
		}
		if (!CheckLimit(effect, position, rotation.Value, parent))
		{
			return null;
		}
		return PlayIntoPool(GetPoolForPlayNew(effect, parent), effect, null, positioned: true, position, rotation.Value, parent);
	}

	public static GameObject PlayNew(GameObject effect, Entity attach, NetworkIdentity parent = null)
	{
		if (disablePlayNew)
		{
			return null;
		}
		if (effect == null)
		{
			return null;
		}
		if ((UnityEngine.Object)(object)attach == null)
		{
			return PlayNew(effect);
		}
		if (!CheckLimit(effect, attach, parent))
		{
			return null;
		}
		return PlayIntoPool(GetPoolForPlayNew(effect, parent), effect, attach, positioned: false, default, default, parent);
	}

	public static GameObject PlayNew(GameObject effect, Entity attach, Vector3 position, Quaternion? rotation, NetworkIdentity parent = null)
	{
		if (disablePlayNew)
		{
			return null;
		}
		if (effect == null)
		{
			return null;
		}
		if (!rotation.HasValue)
		{
			rotation = ManagerBase<CameraManager>.instance.entityCamAngleRotation;
		}
		if ((UnityEngine.Object)(object)attach == null)
		{
			return PlayNew(effect, position, rotation);
		}
		if (!CheckLimit(effect, attach, parent))
		{
			return null;
		}
		return PlayIntoPool(GetPoolForPlayNew(effect, parent), effect, attach, positioned: true, position, rotation.Value, parent);
	}

	public static void PlayDetached(GameObject inPrefabChild, NetworkIdentity parent = null)
	{
		if (!disablePlayNew && !(inPrefabChild == null) && CheckLimit(inPrefabChild, parent))
		{
			inPrefabChild.transform.SetParent(null, worldPositionStays: true);
			inPrefabChild.SetActive(value: true);
			Play(inPrefabChild, parent);
			EffectAutoDestroy.Register(inPrefabChild);
		}
	}

	public static void PlayDetached(GameObject inPrefabChild, Vector3 position, Quaternion? rotation, NetworkIdentity parent = null)
	{
		if (!disablePlayNew && !(inPrefabChild == null))
		{
			if (!rotation.HasValue)
			{
				rotation = ManagerBase<CameraManager>.instance.entityCamAngleRotation;
			}
			if (CheckLimit(inPrefabChild, position, rotation.Value, parent))
			{
				inPrefabChild.transform.SetParent(null, worldPositionStays: true);
				inPrefabChild.transform.SetPositionAndRotation(position, rotation.Value);
				inPrefabChild.SetActive(value: true);
				Play(inPrefabChild, parent);
				EffectAutoDestroy.Register(inPrefabChild);
			}
		}
	}

	public static void PlayDetached(GameObject inPrefabChild, Entity attach, NetworkIdentity parent = null)
	{
		if (!disablePlayNew && !(inPrefabChild == null))
		{
			if ((UnityEngine.Object)(object)attach == null)
			{
				PlayDetached(inPrefabChild, parent);
			}
			else if (CheckLimit(inPrefabChild, attach, parent))
			{
				inPrefabChild.transform.SetParent(null, worldPositionStays: true);
				inPrefabChild.SetActive(value: true);
				Play(inPrefabChild, attach, parent);
				EffectAutoDestroy.Register(inPrefabChild);
			}
		}
	}

	public static void PlayDetached(GameObject inPrefabChild, Entity attach, Vector3 position, Quaternion? rotation, NetworkIdentity parent = null)
	{
		if (!disablePlayNew && !(inPrefabChild == null))
		{
			if (!rotation.HasValue)
			{
				rotation = ManagerBase<CameraManager>.instance.entityCamAngleRotation;
			}
			if ((UnityEngine.Object)(object)attach == null)
			{
				PlayDetached(inPrefabChild, position, rotation, parent);
			}
			else if (CheckLimit(inPrefabChild, attach, parent))
			{
				inPrefabChild.transform.SetParent(null, worldPositionStays: true);
				inPrefabChild.transform.SetPositionAndRotation(position, rotation.Value);
				inPrefabChild.SetActive(value: true);
				Play(inPrefabChild, attach, parent);
				EffectAutoDestroy.Register(inPrefabChild);
			}
		}
	}

	private static FxPool GetLocalPool(GameObject source)
	{
		if (!_pool.TryGetValue(source, out var value))
		{
			value = new FxPool
			{
				source = source,
				hasSource = true
			};
			_pool[source] = value;
		}
		return value;
	}

	private static FxPool GetNetPool((uint assetId, int pathId, bool toned, int skinVar) key)
	{
		if (!_netPool.TryGetValue(key, out var value))
		{
			value = new FxPool();
			ResolveNetPoolSource(value, key.assetId);
			_netPool[key] = value;
			return value;
		}
		if (value.hasSource && value.source == null)
		{
			DestroyPoolInstances(value);
			ResolveNetPoolSource(value, key.assetId);
		}
		return value;
	}

	private static void ResolveNetPoolSource(FxPool pool, uint assetId)
	{
		DewResourceDatabase database = DewResources.database;
		pool.source = (((UnityEngine.Object)(object)database != null && database.netObjectAssetIdToGuid.ContainsKey(assetId)) ? DewResources.GetNetworkedPrefab(assetId) : null);
		pool.hasSource = pool.source != null;
	}

	private static FxPool GetPoolForPlayNew(GameObject effect, NetworkIdentity parent)
	{
		if ((UnityEngine.Object)(object)parent != null && parent.assetId != 0 && effect.transform.IsChildOf(((Component)(object)parent).transform))
		{
			int pathId = GetPathId(((Component)(object)parent).transform, effect.transform);
			if (pathId != int.MinValue)
			{
				return GetNetPool((assetId: parent.assetId, pathId: pathId, toned: IsTonedDownForOtherPlayer(parent), skinVar: GetSkinVisualVariantId(parent)));
			}
		}
		return GetLocalPool(effect);
	}

	public static void PlayPooled(GameObject source, Vector3 position, Quaternion rotation, NetworkIdentity parent = null)
	{
		if (!(source == null))
		{
			PlayIntoPool(GetLocalPool(source), source, null, positioned: true, position, rotation, parent);
		}
	}

	public static void PlayPooledLocal(NetworkIdentity parent, GameObject effect, Entity attach, Vector3 position, Quaternion rotation)
	{
		if (!disablePlayNew && !(effect == null) && CheckLimit(effect, attach, parent))
		{
			PlayPooledNetworkedLocal((assetId: parent.assetId, pathId: GetPathId(((Component)(object)parent).transform, effect.transform), toned: IsTonedDownForOtherPlayer(parent), skinVar: GetSkinVisualVariantId(parent)), effect, attach, positioned: true, position, rotation, parent);
		}
	}

	internal static void PlayPooledNetworkedLocal((uint assetId, int pathId, bool toned, int skinVar) key, GameObject template, Entity attach, bool positioned, Vector3 position, Quaternion rotation, NetworkIdentity parent)
	{
		if (!(template == null))
		{
			PlayIntoPool(GetNetPool(key), template, attach, positioned, position, rotation, parent);
		}
	}

	private static GameObject PlayIntoPool(FxPool pool, GameObject template, Entity attach, bool positioned, Vector3 position, Quaternion rotation, NetworkIdentity parent)
	{
		pool.plays++;
		pool.roomsSinceUsed = 0;
		GameObject gameObject = null;
		int num = 0;
		for (int i = 0; i < pool.instances.Count; i++)
		{
			GameObject gameObject2 = pool.instances[i];
			if (!(gameObject2 == null))
			{
				if (gameObject2.activeSelf)
				{
					num++;
				}
				else if (gameObject == null)
				{
					gameObject = gameObject2;
				}
			}
		}
		if (num >= 10 && GraphicsManager.WasLowFpsInLast5Seconds() && GetFxEntity(parent) is Hero)
		{
			return null;
		}
		bool flag = gameObject != null;
		if (gameObject == null)
		{
			EnsureSourceBaked(template);
			gameObject = UnityEngine.Object.Instantiate(template, GetPoolRoot());
			pool.instances.Add(gameObject);
			if (_lastAppliedSpeedMul.TryGetValue(template, out var value) && value != 1f)
			{
				_cloneBirthSpeedMul[gameObject] = value;
			}
		}
		gameObject.SetActive(value: true);
		SyncPooledCloneSpeed(template, gameObject);
		if (flag)
		{
			SyncPooledCloneTelegraphGeometry(template, gameObject);
			SyncPooledCloneEffectParams(template, gameObject);
		}
		if ((UnityEngine.Object)(object)attach != null)
		{
			if (positioned)
			{
				Play(gameObject, attach, position, rotation, parent);
			}
			else
			{
				gameObject.transform.SetPositionAndRotation(template.transform.position, template.transform.rotation);
				Play(gameObject, attach, parent);
			}
		}
		else
		{
			Play(gameObject, position, rotation, parent);
		}
		EffectAutoDestroy.Register(gameObject, pooled: true, parent);
		return gameObject;
	}

	private static void SyncPooledCloneSpeed(GameObject template, GameObject inst)
	{
		bool flag = _lastAppliedSpeedMul.TryGetValue(template, out var value);
		bool flag2 = _lastAppliedSpeedMul.TryGetValue(inst, out var value2);
		if (flag || flag2)
		{
			if (!flag)
			{
				value = 1f;
			}
			if (!flag2)
			{
				value2 = 1f;
			}
			if (!_cloneBirthSpeedMul.TryGetValue(inst, out var value3))
			{
				value3 = 1f;
			}
			float num = value / Mathf.Max(value3, 0.0001f);
			if (!Mathf.Approximately(num, value2))
			{
				ApplySpeedMultiplier(inst, num);
			}
		}
	}

	private static void SyncPooledCloneTelegraphGeometry(GameObject template, GameObject inst)
	{
		if (!_templateArcTelegraphs.TryGetValue(template, out var value))
		{
			value = (_templateArcTelegraphs[template] = template.GetComponentsInChildren<ArcTelegraphController>(includeInactive: true));
		}
		if (value.Length != 0)
		{
			ArcTelegraphController[] componentsInChildren2 = inst.GetComponentsInChildren<ArcTelegraphController>(includeInactive: true);
			if (componentsInChildren2.Length == value.Length)
			{
				for (int i = 0; i < value.Length; i++)
				{
					componentsInChildren2[i].innerRadius = value[i].innerRadius;
					componentsInChildren2[i].outerRadius = value[i].outerRadius;
					componentsInChildren2[i].arcAngle = value[i].arcAngle;
				}
			}
		}
		if (!_templateBoxTelegraphs.TryGetValue(template, out var value2))
		{
			value2 = (_templateBoxTelegraphs[template] = template.GetComponentsInChildren<BoxTelegraphController>(includeInactive: true));
		}
		if (value2.Length == 0)
		{
			return;
		}
		BoxTelegraphController[] componentsInChildren4 = inst.GetComponentsInChildren<BoxTelegraphController>(includeInactive: true);
		if (componentsInChildren4.Length == value2.Length)
		{
			for (int j = 0; j < value2.Length; j++)
			{
				componentsInChildren4[j].width = value2[j].width;
				componentsInChildren4[j].height = value2[j].height;
			}
		}
	}

	private static FxParamComponents GetFxParamComponents(GameObject go)
	{
		if (!_fxParamComponents.TryGetValue(go, out var value))
		{
			Dictionary<GameObject, FxParamComponents> fxParamComponents = _fxParamComponents;
			FxParamComponents fxParamComponents2 = new FxParamComponents
			{
				audios = go.GetComponentsInChildren<DewAudioSource>(includeInactive: true),
				shakes = go.GetComponentsInChildren<FxCameraShake>(includeInactive: true),
				interps = go.GetComponentsInChildren<FxInterpolatedEffectBase>(includeInactive: true),
				scaledChildren = go.GetComponentsInChildren<FxSyncChildScale>(includeInactive: true)
			};
			value = fxParamComponents2;
			fxParamComponents[go] = fxParamComponents2;
		}
		return value;
	}

	private static void SyncPooledCloneEffectParams(GameObject template, GameObject inst)
	{
		inst.transform.localScale = template.transform.localScale;
		FxParamComponents fxParamComponents = GetFxParamComponents(template);
		if (fxParamComponents.audios.Length == 0 && fxParamComponents.shakes.Length == 0 && fxParamComponents.interps.Length == 0 && fxParamComponents.scaledChildren.Length == 0)
		{
			return;
		}
		FxParamComponents fxParamComponents2 = GetFxParamComponents(inst);
		if (fxParamComponents.scaledChildren.Length == fxParamComponents2.scaledChildren.Length)
		{
			for (int i = 0; i < fxParamComponents.scaledChildren.Length; i++)
			{
				if (!(fxParamComponents.scaledChildren[i] == null) && !(fxParamComponents2.scaledChildren[i] == null))
				{
					fxParamComponents2.scaledChildren[i].transform.localScale = fxParamComponents.scaledChildren[i].transform.localScale;
				}
			}
		}
		if (fxParamComponents.audios.Length == fxParamComponents2.audios.Length)
		{
			for (int j = 0; j < fxParamComponents.audios.Length; j++)
			{
				if (!(fxParamComponents.audios[j] == null) && !(fxParamComponents2.audios[j] == null))
				{
					fxParamComponents2.audios[j].pitchMultiplier = fxParamComponents.audios[j].pitchMultiplier;
					fxParamComponents2.audios[j].volumeMultiplier = fxParamComponents.audios[j].volumeMultiplier;
				}
			}
		}
		if (fxParamComponents.shakes.Length == fxParamComponents2.shakes.Length)
		{
			for (int k = 0; k < fxParamComponents.shakes.Length; k++)
			{
				if (!(fxParamComponents.shakes[k] == null) && !(fxParamComponents2.shakes[k] == null))
				{
					fxParamComponents2.shakes[k].amplitude = fxParamComponents.shakes[k].amplitude;
				}
			}
		}
		if (fxParamComponents.interps.Length != fxParamComponents2.interps.Length)
		{
			return;
		}
		for (int l = 0; l < fxParamComponents.interps.Length; l++)
		{
			if (!(fxParamComponents.interps[l] == null) && !(fxParamComponents2.interps[l] == null))
			{
				fxParamComponents2.interps[l].sustainTime = fxParamComponents.interps[l].sustainTime;
			}
		}
	}

	public static void ClearPool()
	{
		foreach (FxPool value in _pool.Values)
		{
			for (int i = 0; i < value.instances.Count; i++)
			{
				if (value.instances[i] != null)
				{
					UnityEngine.Object.Destroy(value.instances[i]);
				}
			}
		}
		_pool.Clear();
		foreach (FxPool value2 in _netPool.Values)
		{
			for (int j = 0; j < value2.instances.Count; j++)
			{
				if (value2.instances[j] != null)
				{
					UnityEngine.Object.Destroy(value2.instances[j]);
				}
			}
		}
		_netPool.Clear();
		_pathCache.Clear();
		_pathIdCache.Clear();
		_gradientKeyCache.Clear();
		_sourceSkinnable.Clear();
		_sourcePathCache.Clear();
		_fxParamComponents.Clear();
		SweepDeadSpeedCaches();
	}

	public static void AgeAndDropInactivePools()
	{
		AgeAndDrop(_pool);
		AgeAndDrop(_netPool);
		_pathCache.Clear();
		_pathIdCache.Clear();
		_gradientKeyCache.Clear();
		SweepDeadSpeedCaches();
	}

	private static void SweepDeadSpeedCaches()
	{
		SweepDeadKeys<ParticleSystem, float>(_psBaseSimSpeed);
		SweepDeadKeys(_lastAppliedSpeedMul);
		SweepDeadKeys(_cloneBirthSpeedMul);
		SweepDeadKeys(_templateArcTelegraphs);
		SweepDeadKeys(_templateBoxTelegraphs);
		SweepDeadKeys(_fxParamComponents);
	}

	private static void SweepDeadKeys<TKey, TValue>(Dictionary<TKey, TValue> dict) where TKey : UnityEngine.Object
	{
		List<TKey> list = null;
		foreach (KeyValuePair<TKey, TValue> item in dict)
		{
			if (item.Key == null)
			{
				(list ?? (list = new List<TKey>())).Add(item.Key);
			}
		}
		if (list != null)
		{
			for (int i = 0; i < list.Count; i++)
			{
				dict.Remove(list[i]);
			}
		}
	}

	private static void AgeAndDrop<TKey>(Dictionary<TKey, FxPool> pools)
	{
		List<TKey> list = null;
		foreach (KeyValuePair<TKey, FxPool> pool in pools)
		{
			FxPool value = pool.Value;
			value.roomsSinceUsed++;
			if (value.roomsSinceUsed < 3 && (!value.hasSource || !(value.source == null)))
			{
				continue;
			}
			for (int i = 0; i < value.instances.Count; i++)
			{
				if (value.instances[i] != null)
				{
					UnityEngine.Object.Destroy(value.instances[i]);
				}
			}
			(list ?? (list = new List<TKey>())).Add(pool.Key);
		}
		if (list != null)
		{
			for (int j = 0; j < list.Count; j++)
			{
				pools.Remove(list[j]);
			}
		}
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
	private static void RegisterPoolFlushOnVariantsCleared()
	{
		DewResources.onVariantsCleared += new Action(FlushInactivePools);
		DewResources.onAssetVariantsDestroyed += new Action<string>(PurgePoolsOfAsset);
	}

	private static void PurgePoolsOfAsset(string guid)
	{
		DewResourceDatabase database = DewResources.database;
		if (!((UnityEngine.Object)(object)database != null) || string.IsNullOrEmpty(guid) || !database.guidToNetAssetId.TryGetValue(guid, out var value))
		{
			return;
		}
		List<(uint, int, bool, int)> list = null;
		foreach (KeyValuePair<(uint, int, bool, int), FxPool> item in _netPool)
		{
			if (item.Key.Item1 == value)
			{
				DestroyPoolInstances(item.Value);
				(list ?? (list = new List<(uint, int, bool, int)>())).Add(item.Key);
			}
		}
		if (list != null)
		{
			for (int i = 0; i < list.Count; i++)
			{
				_netPool.Remove(list[i]);
			}
		}
	}

	private static void DestroyPoolInstances(FxPool pool)
	{
		for (int i = 0; i < pool.instances.Count; i++)
		{
			if (pool.instances[i] != null)
			{
				UnityEngine.Object.Destroy(pool.instances[i]);
			}
		}
		pool.instances.Clear();
	}

	public static void FlushInactivePools()
	{
		FlushInactive(_pool);
		FlushInactive(_netPool);
	}

	private static void FlushInactive<TKey>(Dictionary<TKey, FxPool> pools)
	{
		List<TKey> list = null;
		foreach (KeyValuePair<TKey, FxPool> pool in pools)
		{
			FxPool value = pool.Value;
			for (int num = value.instances.Count - 1; num >= 0; num--)
			{
				GameObject gameObject = value.instances[num];
				if (gameObject == null || !gameObject.activeSelf)
				{
					if (gameObject != null)
					{
						UnityEngine.Object.Destroy(gameObject);
					}
					value.instances.RemoveAt(num);
				}
			}
			if (value.instances.Count == 0)
			{
				(list ?? (list = new List<TKey>())).Add(pool.Key);
			}
		}
		if (list != null)
		{
			for (int i = 0; i < list.Count; i++)
			{
				pools.Remove(list[i]);
			}
		}
	}

	private static string ResolveEffectLabel(uint assetId, int pathId, FxPool pool)
	{
		(uint, int) key = (assetId, pathId);
		if (_effectLabelCache.TryGetValue(key, out var value))
		{
			return value;
		}
		GameObject source = pool.source;
		if (source == null)
		{
			return pathId.ToString();
		}
		Transform transform = source.transform;
		Transform[] componentsInChildren = source.GetComponentsInChildren<Transform>(includeInactive: true);
		foreach (Transform transform2 in componentsInChildren)
		{
			if (!(transform2 == transform) && GetPathId(transform, transform2) == pathId)
			{
				value = GetPath(transform, transform2);
				break;
			}
		}
		if (value == null)
		{
			value = pathId.ToString();
		}
		_effectLabelCache[key] = value;
		return value;
	}

	public static void GetPoolStats(List<FxPoolStat> outList)
	{
		outList.Clear();
		foreach (KeyValuePair<GameObject, FxPool> item in _pool)
		{
			outList.Add(new FxPoolStat
			{
				label = ((item.Key != null) ? item.Key.name : "<destroyed>"),
				caller = "",
				networked = false,
				instantiated = item.Value.instances.Count,
				plays = item.Value.plays
			});
		}
		foreach (KeyValuePair<(uint, int, bool, int), FxPool> item2 in _netPool)
		{
			outList.Add(new FxPoolStat
			{
				label = ResolveEffectLabel(item2.Key.Item1, item2.Key.Item2, item2.Value) + (item2.Key.Item3 ? " (other players)" : "") + ((item2.Key.Item4 != 0) ? $" (skin {item2.Key.Item4})" : ""),
				caller = ResolveCaller(item2.Key.Item1),
				networked = true,
				instantiated = item2.Value.instances.Count,
				plays = item2.Value.plays
			});
		}
	}

	private static string ResolveCaller(uint assetId)
	{
		if (_callerNameCache.TryGetValue(assetId, out var value))
		{
			return value;
		}
		DewResourceDatabase database = DewResources.database;
		if ((UnityEngine.Object)(object)database != null && database.netObjectAssetIdToGuid.TryGetValue(assetId, out var value2) && database.guidToType.TryGetValue(value2, out var value3))
		{
			value = value3.Name;
			_callerNameCache[assetId] = value;
			return value;
		}
		return "#" + assetId;
	}

	private static Transform GetPoolRoot()
	{
		if (_poolRoot == null)
		{
			GameObject gameObject = new GameObject("Pooled Fx");
			UnityEngine.Object.DontDestroyOnLoad(gameObject);
			_poolRoot = gameObject.transform;
		}
		return _poolRoot;
	}

	public static void EnsureSourceBaked(GameObject effect)
	{
		if (!(effect == null) && !effect.TryGetComponent<FxPlayPlan>(out var _))
		{
			Quality3Levels quality = ((ManagerBase<GraphicsManager>.instance != null) ? ManagerBase<GraphicsManager>.instance.currentEffectQuality : Quality3Levels.High);
			BakeQualityScaling(effect, quality, buildPlan: true);
		}
	}

	public static void PrewarmBakeInPlaceEffects(GameObject root)
	{
		if (root == null || ManagerBase<GraphicsManager>.instance == null)
		{
			return;
		}
		AbilityTrigger[] componentsInChildren = root.GetComponentsInChildren<AbilityTrigger>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			TriggerConfig[] configs = componentsInChildren[i].configs;
			if (configs != null)
			{
				for (int j = 0; j < configs.Length; j++)
				{
					if (configs[j] != null)
					{
						EnsureSourceBaked(configs[j].effectOnCast);
					}
				}
			}
			if (componentsInChildren[i] is SkillTrigger skillTrigger)
			{
				EnsureSourceBaked(skillTrigger.startEffect);
				EnsureSourceBaked(skillTrigger.endEffect);
			}
		}
		AbilityInstance[] componentsInChildren2 = root.GetComponentsInChildren<AbilityInstance>(includeInactive: true);
		foreach (AbilityInstance abilityInstance in componentsInChildren2)
		{
			EnsureSourceBaked(abilityInstance.startEffect);
			EnsureSourceBaked(abilityInstance.startEffectNoStop);
			EnsureSourceBaked(abilityInstance.endEffect);
			if (abilityInstance is Projectile projectile)
			{
				EnsureSourceBaked(projectile.effectOnFly);
			}
			else if (abilityInstance is DamageInstance damageInstance)
			{
				EnsureSourceBaked(damageInstance.mainEffect);
			}
		}
	}

	public static void BakeQualityScaling(GameObject root, Quality3Levels quality, bool buildPlan = false, bool applyScaling = true, List<GameObject> collectDisabled = null)
	{
		if (root == null || (buildPlan && root.TryGetComponent<FxPlayPlan>(out var _)))
		{
			return;
		}
		List<FxPlayPlan.Node> list = null;
		List<ParticleSystem> list2 = null;
		List<VisualEffect> list3 = null;
		List<CinemachineImpulseSource> list4 = null;
		List<Component> list5 = null;
		if (buildPlan)
		{
			list = new List<FxPlayPlan.Node>();
			list2 = new List<ParticleSystem>();
			list3 = new List<VisualEffect>();
			list4 = new List<CinemachineImpulseSource>();
			list5 = new List<Component>();
		}
		BakeNode(root.transform, quality, apcStateFromParent: true, underTopPs: false, applyScaling, list, list2, list3, list4, list5, collectDisabled);
		if (!buildPlan)
		{
			return;
		}
		FxPlayPlan fxPlayPlan = root.AddComponent<FxPlayPlan>();
		fxPlayPlan.hideFlags = HideFlags.DontSaveInEditor;
		fxPlayPlan.nodes = list.ToArray();
		fxPlayPlan.rootParticleSystems = list2.ToArray();
		fxPlayPlan.visualEffects = list3.ToArray();
		fxPlayPlan.impulses = list4.ToArray();
		fxPlayPlan.effectComponents = list5.ToArray();
		ParticleSystem[] componentsInChildren = root.GetComponentsInChildren<ParticleSystem>(includeInactive: true);
		List<ParticleSystem> list6 = new List<ParticleSystem>();
		List<ParticleSystem> list7 = new List<ParticleSystem>();
		foreach (ParticleSystem val in componentsInChildren)
		{
			if (((Component)(object)val).TryGetComponent(out FxParticleSystem component2))
			{
				if (component2.clearParticlesOnStop == FxParticleSystem.ClearParticlesBehavior.ClearSelf)
				{
					list6.Add(val);
				}
				else if (component2.clearParticlesOnStop == FxParticleSystem.ClearParticlesBehavior.ClearWithChildren)
				{
					list7.Add(val);
				}
			}
		}
		fxPlayPlan.clearSelfOnStopParticleSystems = list6.ToArray();
		fxPlayPlan.clearWithChildrenOnStopParticleSystems = list7.ToArray();
		FxSelectiveQuality[] componentsInChildren2 = root.GetComponentsInChildren<FxSelectiveQuality>(includeInactive: true);
		for (int j = 0; j < componentsInChildren2.Length; j++)
		{
			UnityEngine.Object.DestroyImmediate(componentsInChildren2[j]);
		}
	}

	private static void BakeNode(Transform node, Quality3Levels quality, bool apcStateFromParent, bool underTopPs, bool applyScaling, List<FxPlayPlan.Node> nodes, List<ParticleSystem> rootPs, List<VisualEffect> vfx, List<CinemachineImpulseSource> impulses, List<Component> effectComps, List<GameObject> collectDisabled)
	{
		FxSelectiveVisibility vis = null;
		if (node.TryGetComponent<FxSelectiveVisibility>(out var component))
		{
			if (component.wasDisabled)
			{
				node.gameObject.SetActive(value: false);
				return;
			}
			vis = component;
		}
		bool flag = apcStateFromParent;
		bool apcStateFromParent2 = apcStateFromParent;
		if (node.TryGetComponent<FxSelectiveQuality>(out var component2))
		{
			if (component2.wasDisabled)
			{
				node.gameObject.SetActive(value: false);
				return;
			}
			if (component2.disableGameObject && quality < component2.allowedIfQualityIs)
			{
				node.gameObject.SetActive(value: false);
				collectDisabled?.Add(node.gameObject);
				return;
			}
			node.gameObject.SetActive(value: true);
			flag = component2.adjustParticleCount;
			if (component2.includeChildrenWhenAdjustingParticleCount)
			{
				apcStateFromParent2 = flag;
			}
		}
		bool flag2 = node.TryGetComponent<ParticleSystem>(out var component3);
		if (applyScaling & flag2)
		{
			FxSelectiveQuality.ApplyQualityScaling(quality, node.gameObject, flag);
		}
		bool underTopPs2 = underTopPs | flag2;
		if (nodes == null)
		{
			for (int i = 0; i < node.childCount; i++)
			{
				BakeNode(node.GetChild(i), quality, apcStateFromParent2, underTopPs2, applyScaling, nodes, rootPs, vfx, impulses, effectComps, collectDisabled);
			}
			return;
		}
		if (flag2 && !underTopPs)
		{
			rootPs.Add(component3);
		}
		if (node.TryGetComponent<VisualEffect>(out var component4))
		{
			vfx.Add(component4);
		}
		if (node.TryGetComponent<CinemachineImpulseSource>(out var component5))
		{
			impulses.Add(component5);
		}
		IEffectComponent[] components = node.GetComponents<IEffectComponent>();
		for (int j = 0; j < components.Length; j++)
		{
			if (!(components[j] is FxGameObject) && components[j] is Component item)
			{
				effectComps.Add(item);
			}
		}
		node.TryGetComponent<FxGameObject>(out var component6);
		MonoBehaviour[] array = null;
		IEffectSetupComponent[] components2 = node.GetComponents<IEffectSetupComponent>();
		if (components2.Length != 0)
		{
			array = new MonoBehaviour[components2.Length];
			for (int k = 0; k < components2.Length; k++)
			{
				array[k] = components2[k] as MonoBehaviour;
			}
		}
		int count = nodes.Count;
		nodes.Add(new FxPlayPlan.Node
		{
			go = node.gameObject,
			vis = vis,
			fxGameObject = component6,
			setups = array,
			subtreeEnd = 0
		});
		for (int l = 0; l < node.childCount; l++)
		{
			BakeNode(node.GetChild(l), quality, apcStateFromParent2, underTopPs2, applyScaling, nodes, rootPs, vfx, impulses, effectComps, collectDisabled);
		}
		FxPlayPlan.Node value = nodes[count];
		value.subtreeEnd = nodes.Count;
		nodes[count] = value;
	}

	private static void ExecutePlan(FxPlayPlan plan, NetworkIdentity parent)
	{
		Actor fxActor = GetFxActor(parent);
		FxPlayPlan.Node[] nodes = plan.nodes;
		int num = 0;
		while (num < nodes.Length)
		{
			FxPlayPlan.Node node = nodes[num];
			if (node.go == null)
			{
				num = node.subtreeEnd;
				continue;
			}
			if (node.vis != null)
			{
				if ((UnityEngine.Object)(object)fxActor != null)
				{
					node.vis.parent = fxActor;
				}
				if (!node.vis.IsVisibleLocally())
				{
					node.go.SetActive(value: false);
					num = node.subtreeEnd;
					continue;
				}
				node.go.SetActive(value: true);
			}
			if (node.fxGameObject != null)
			{
				node.fxGameObject.Play();
			}
			if (!node.go.activeInHierarchy)
			{
				num = node.subtreeEnd;
				continue;
			}
			if (node.setups != null)
			{
				for (int i = 0; i < node.setups.Length; i++)
				{
					try
					{
						(node.setups[i] as IEffectSetupComponent)?.OnEffectSetup();
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
					}
				}
			}
			num++;
		}
		ParticleSystem[] rootParticleSystems = plan.rootParticleSystems;
		for (int j = 0; j < rootParticleSystems.Length; j++)
		{
			if ((UnityEngine.Object)(object)rootParticleSystems[j] != null && ((Component)(object)rootParticleSystems[j]).gameObject.activeInHierarchy)
			{
				rootParticleSystems[j].Play();
			}
		}
		VisualEffect[] visualEffects = plan.visualEffects;
		for (int k = 0; k < visualEffects.Length; k++)
		{
			if ((UnityEngine.Object)(object)visualEffects[k] != null && ((Behaviour)(object)visualEffects[k]).isActiveAndEnabled)
			{
				visualEffects[k].Play();
			}
		}
		CinemachineImpulseSource[] impulses = plan.impulses;
		for (int l = 0; l < impulses.Length; l++)
		{
			if ((UnityEngine.Object)(object)impulses[l] != null && ((Behaviour)(object)impulses[l]).isActiveAndEnabled)
			{
				impulses[l].GenerateImpulse();
			}
		}
		Component[] effectComponents = plan.effectComponents;
		foreach (Component component in effectComponents)
		{
			if (!(component == null) && (!(component is MonoBehaviour monoBehaviour) || (monoBehaviour.enabled && monoBehaviour.gameObject.activeInHierarchy)))
			{
				ApplyOwnerContextAndPlay((IEffectComponent)component, parent);
			}
		}
	}

	private static Entity GetFxEntity(NetworkIdentity parent)
	{
		if ((UnityEngine.Object)(object)parent == null)
		{
			return null;
		}
		if (!parent.fxDidCacheEntity)
		{
			parent.fxDidCacheEntity = true;
			Actor component = ((Component)(object)parent).GetComponent<Actor>();
			if ((UnityEngine.Object)(object)component != null)
			{
				if (component is Entity fxEntity)
				{
					parent.fxEntity = (Component)(object)fxEntity;
				}
				else
				{
					parent.fxEntity = (Component)(object)component.firstEntity;
				}
			}
		}
		return parent.fxEntity as Entity;
	}

	private static Actor GetFxActor(NetworkIdentity parent)
	{
		if ((UnityEngine.Object)(object)parent == null)
		{
			return null;
		}
		if (!parent.fxDidCacheActor)
		{
			parent.fxDidCacheActor = true;
			parent.fxActor = (Component)(object)((Component)(object)parent).GetComponent<Actor>();
		}
		return parent.fxActor as Actor;
	}

	private static int GetSkinVisualVariantId(NetworkIdentity parent)
	{
		Entity fxEntity = GetFxEntity(parent);
		if (!((UnityEngine.Object)(object)fxEntity != null))
		{
			return 0;
		}
		return fxEntity._skinVisualVariantId;
	}

	private static bool IsTonedDownForOtherPlayer(NetworkIdentity parent)
	{
		Entity fxEntity = GetFxEntity(parent);
		if ((UnityEngine.Object)(object)fxEntity == null)
		{
			return false;
		}
		DewPlayer owner = fxEntity.owner;
		if ((UnityEngine.Object)(object)owner == null || !owner.isHumanPlayer)
		{
			return false;
		}
		CameraManager instance = ManagerBase<CameraManager>.instance;
		Entity entity = ((instance != null) ? instance.focusedEntity : null);
		DewPlayer dewPlayer = (((UnityEngine.Object)(object)entity != null) ? entity.owner : DewPlayer.local);
		return (UnityEngine.Object)(object)owner != (UnityEngine.Object)(object)dewPlayer;
	}

	private static void ApplyOwnerContextAndPlay(IEffectComponent comp, NetworkIdentity parent)
	{
		if (comp is IEffectWithOwnerContext effectWithOwnerContext)
		{
			Entity fxEntity = GetFxEntity(parent);
			EffectOwnerContext effectOwnerContext;
			if ((UnityEngine.Object)(object)fxEntity == null)
			{
				effectOwnerContext = EffectOwnerContext.None;
			}
			else if (fxEntity.IsAnyBoss())
			{
				effectOwnerContext = EffectOwnerContext.Boss;
			}
			else
			{
				Entity focusedEntity = ManagerBase<CameraManager>.instance.focusedEntity;
				DewPlayer dewPlayer = (((UnityEngine.Object)(object)focusedEntity != null) ? focusedEntity.owner : DewPlayer.local);
				DewPlayer owner = fxEntity.owner;
				effectOwnerContext = (((UnityEngine.Object)(object)owner == (UnityEngine.Object)(object)dewPlayer) ? EffectOwnerContext.Self : ((!((UnityEngine.Object)(object)owner != null) || !owner.isHumanPlayer) ? EffectOwnerContext.Others : EffectOwnerContext.OtherPlayers));
			}
			if (effectOwnerContext == EffectOwnerContext.Self && FxSelectiveVisibility.forceFail)
			{
				effectOwnerContext = EffectOwnerContext.OtherPlayers;
			}
			effectWithOwnerContext.SetOwnerContext(effectOwnerContext);
		}
		comp.Play();
	}

	public static void Play(GameObject effect, NetworkIdentity parent = null)
	{
		Play_Imp(effect, parent, isPlayingNew: false);
	}

	private static void Play_Imp(GameObject effect, NetworkIdentity parent, bool isPlayingNew)
	{
		if (effect == null || disablePlay)
		{
			return;
		}
		if (effect.TryGetComponent<FxPlayPlan>(out var component))
		{
			ExecutePlan(component, parent);
			return;
		}
		List<Transform> list = DewPool.GetList(out ListReturnHandle<Transform> handle);
		List<bool> list2 = DewPool.GetList(out ListReturnHandle<bool> handle2);
		list.Add(effect.transform);
		list2.Add(item: true);
		Quality3Levels quality3Levels = ((ManagerBase<GraphicsManager>.instance != null) ? ManagerBase<GraphicsManager>.instance.currentEffectQuality : Quality3Levels.High);
		while (list.Count > 0)
		{
			int index = list.Count - 1;
			Transform transform = list[index];
			bool flag = list2[index];
			list.RemoveAt(index);
			list2.RemoveAt(index);
			if (transform.TryGetComponent<FxSelectiveVisibility>(out var component2))
			{
				if (component2.wasDisabled)
				{
					transform.gameObject.SetActive(value: false);
					continue;
				}
				if (!component2.IsVisibleLocally())
				{
					transform.gameObject.SetActive(value: false);
					continue;
				}
				transform.gameObject.SetActive(value: true);
			}
			bool flag2 = flag;
			bool item = flag;
			if (transform.TryGetComponent<FxSelectiveQuality>(out var component3))
			{
				if (component3.wasDisabled)
				{
					transform.gameObject.SetActive(value: false);
					continue;
				}
				if (component3.disableGameObject && quality3Levels < component3.allowedIfQualityIs)
				{
					transform.gameObject.SetActive(value: false);
					continue;
				}
				transform.gameObject.SetActive(value: true);
				flag2 = component3.adjustParticleCount;
				if (component3.includeChildrenWhenAdjustingParticleCount)
				{
					item = flag2;
				}
			}
			if (transform.TryGetComponent<ParticleSystem>(out var _))
			{
				if (!isPlayingNew)
				{
					if (!transform.TryGetComponent<FxParticleSystemCacheData>(out var component5))
					{
						transform.gameObject.AddComponent<FxParticleSystemCacheData>();
					}
					else
					{
						component5.SetDefault();
					}
				}
				FxSelectiveQuality.ApplyQualityScaling(quality3Levels, transform.gameObject, flag2);
			}
			if (transform.TryGetComponent<FxGameObject>(out var component6))
			{
				component6.Play();
			}
			if (!transform.gameObject.activeInHierarchy)
			{
				continue;
			}
			ListReturnHandle<IEffectSetupComponent> handle3;
			foreach (IEffectSetupComponent item2 in ((Component)transform).GetComponentsNonAlloc(out handle3))
			{
				try
				{
					item2.OnEffectSetup();
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
				}
			}
			handle3.Return();
			for (int i = 0; i < transform.childCount; i++)
			{
				list.Add(transform.GetChild(i));
				list2.Add(item);
			}
		}
		handle.Return();
		handle2.Return();
		PlayAllParticleSystems(effect.transform);
		PlayAllVisualEffects(effect.transform);
		List<CinemachineImpulseSource> componentsInChildrenNonAlloc = effect.GetComponentsInChildrenNonAlloc(true, out ListReturnHandle<CinemachineImpulseSource> handle4);
		for (int j = 0; j < componentsInChildrenNonAlloc.Count; j++)
		{
			if (((Behaviour)(object)componentsInChildrenNonAlloc[j]).isActiveAndEnabled)
			{
				componentsInChildrenNonAlloc[j].GenerateImpulse();
			}
		}
		handle4.Return();
		List<IEffectComponent> componentsInChildrenNonAlloc2 = effect.GetComponentsInChildrenNonAlloc(true, out ListReturnHandle<IEffectComponent> handle5);
		for (int k = 0; k < componentsInChildrenNonAlloc2.Count; k++)
		{
			if (!(componentsInChildrenNonAlloc2[k] is FxGameObject) && (!(componentsInChildrenNonAlloc2[k] is MonoBehaviour monoBehaviour) || (monoBehaviour.enabled && monoBehaviour.gameObject.activeInHierarchy)))
			{
				ApplyOwnerContextAndPlay(componentsInChildrenNonAlloc2[k], parent);
			}
		}
		handle5.Return();
	}

	public static void Play(GameObject effect, Vector3 position, Quaternion? rotation, NetworkIdentity parent = null)
	{
		if (!(effect == null))
		{
			if (!rotation.HasValue)
			{
				rotation = ManagerBase<CameraManager>.instance.entityCamAngleRotation;
			}
			effect.transform.SetPositionAndRotation(position, rotation.Value);
			Play_Imp(effect, parent, isPlayingNew: false);
		}
	}

	public static void Play(GameObject effect, Entity attach, Vector3 position, Quaternion? rotation, NetworkIdentity parent = null)
	{
		if (effect == null)
		{
			return;
		}
		if (!rotation.HasValue)
		{
			rotation = ManagerBase<CameraManager>.instance.entityCamAngleRotation;
		}
		effect.transform.SetPositionAndRotation(position, rotation.Value);
		if ((UnityEngine.Object)(object)attach == null)
		{
			Play_Imp(effect, null, isPlayingNew: false);
			return;
		}
		ListReturnHandle<IAttachableToEntity> handle;
		foreach (IAttachableToEntity item in effect.GetComponentsInChildrenNonAlloc(true, out handle))
		{
			item.OnAttachToEntity(attach);
		}
		handle.Return();
		Play_Imp(effect, parent, isPlayingNew: false);
	}

	public static void Play(GameObject effect, Entity attach, NetworkIdentity parent = null)
	{
		if (effect == null)
		{
			return;
		}
		if ((UnityEngine.Object)(object)attach == null)
		{
			Play_Imp(effect, null, isPlayingNew: false);
			return;
		}
		ListReturnHandle<IAttachableToEntity> handle;
		foreach (IAttachableToEntity item in effect.GetComponentsInChildrenNonAlloc(true, out handle))
		{
			item.OnAttachToEntity(attach);
		}
		handle.Return();
		Play_Imp(effect, parent, isPlayingNew: false);
	}

	public static void PlayCastEffect(GameObject effect, CastInfo info, CastMethodType method, float duration = -1f, NetworkIdentity parent = null)
	{
		if (!(effect == null))
		{
			IAttachableToEntity[] componentsInChildren = effect.GetComponentsInChildren<IAttachableToEntity>(includeInactive: true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].OnAttachToEntity(info.caster);
			}
			FxCastTelegraph[] componentsInChildren2 = effect.GetComponentsInChildren<FxCastTelegraph>(includeInactive: true);
			for (int i = 0; i < componentsInChildren2.Length; i++)
			{
				componentsInChildren2[i].Setup(method, info, duration);
			}
			EnsureSourceBaked(effect);
			Play_Imp(effect, parent, isPlayingNew: false);
		}
	}

	public static void Stop(GameObject effect)
	{
		if (effect == null)
		{
			return;
		}
		if (effect.TryGetComponent<FxPlayPlan>(out var component))
		{
			ParticleSystem[] rootParticleSystems = component.rootParticleSystems;
			for (int i = 0; i < rootParticleSystems.Length; i++)
			{
				rootParticleSystems[i].Stop();
			}
			ParticleSystem[] clearSelfOnStopParticleSystems = component.clearSelfOnStopParticleSystems;
			if (clearSelfOnStopParticleSystems != null)
			{
				for (int j = 0; j < clearSelfOnStopParticleSystems.Length; j++)
				{
					clearSelfOnStopParticleSystems[j].Clear(false);
				}
			}
			ParticleSystem[] clearWithChildrenOnStopParticleSystems = component.clearWithChildrenOnStopParticleSystems;
			if (clearWithChildrenOnStopParticleSystems != null)
			{
				for (int k = 0; k < clearWithChildrenOnStopParticleSystems.Length; k++)
				{
					clearWithChildrenOnStopParticleSystems[k].Clear(true);
				}
			}
		}
		else
		{
			StopAllParticleSystems(effect.transform);
		}
		StopAllVisualEffects(effect.transform);
		ListReturnHandle<IEffectComponent> handle;
		foreach (IEffectComponent item in effect.GetComponentsInChildrenNonAlloc(true, out handle))
		{
			item.Stop();
		}
		handle.Return();
	}

	public static void ApplySpeedMultiplier(GameObject effect, float multiplier)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		if (effect == null)
		{
			return;
		}
		_lastAppliedSpeedMul[effect] = multiplier;
		ListReturnHandle<ParticleSystem> handle;
		foreach (ParticleSystem item in effect.GetComponentsInChildrenNonAlloc(out handle))
		{
			MainModule main = item.main;
			if (!_psBaseSimSpeed.TryGetValue(item, out var value))
			{
				value = main.simulationSpeed;
				_psBaseSimSpeed[item] = value;
			}
			main.simulationSpeed = value * multiplier;
		}
		handle.Return();
		ListReturnHandle<IEffectWithSpeed> handle2;
		foreach (IEffectWithSpeed item2 in effect.GetComponentsInChildrenNonAlloc(out handle2))
		{
			item2.ApplySpeedMultiplier(multiplier);
		}
		handle2.Return();
	}

	private static void EnsureSameFrame()
	{
		if (_effectsInThisFrameCount != Time.frameCount)
		{
			_effectsInThisFrame.Clear();
			_effectsInThisFrameCount = Time.frameCount;
		}
	}

	private static bool CheckLimit(GameObject effect, NetworkIdentity parent)
	{
		EnsureSameFrame();
		int key = HashCode.Combine<GameObject, NetworkIdentity>(effect, parent);
		if (_effectsInThisFrame.TryGetValue(key, out var value))
		{
			if (value > 7)
			{
				return false;
			}
			_effectsInThisFrame[key] = value + 1;
		}
		else
		{
			_effectsInThisFrame[key] = 1;
		}
		return true;
	}

	private static bool CheckLimit(GameObject effect, Vector3 position, Quaternion rotation, NetworkIdentity parent)
	{
		EnsureSameFrame();
		int key = HashCode.Combine<GameObject, NetworkIdentity, int, int, int, int>(effect, parent, Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y), Mathf.RoundToInt(position.z), Mathf.RoundToInt(rotation.eulerAngles.y));
		if (_effectsInThisFrame.TryGetValue(key, out var value))
		{
			if (value > 4)
			{
				return false;
			}
			_effectsInThisFrame[key] = value + 1;
		}
		else
		{
			_effectsInThisFrame[key] = 1;
		}
		return true;
	}

	private static bool CheckLimit(GameObject effect, Entity attach, NetworkIdentity parent)
	{
		EnsureSameFrame();
		int key = HashCode.Combine<GameObject, Entity, NetworkIdentity>(effect, attach, parent);
		if (_effectsInThisFrame.TryGetValue(key, out var value))
		{
			if (value > 3)
			{
				return false;
			}
			_effectsInThisFrame[key] = value + 1;
		}
		else
		{
			_effectsInThisFrame[key] = 1;
		}
		if (!CheckLimit(effect, parent))
		{
			return false;
		}
		return true;
	}

	internal static void RegisterHandlers()
	{
		NetworkClient.RegisterHandler<PlayEffectMessage>((Action<PlayEffectMessage>)HandleEffectMessage, true);
		NetworkClient.RegisterHandler<PlayPositionedEffectMessage>((Action<PlayPositionedEffectMessage>)HandleEffectMessage, true);
		NetworkClient.RegisterHandler<PlayAttachedEffectMessage>((Action<PlayAttachedEffectMessage>)HandleEffectMessage, true);
		NetworkClient.RegisterHandler<StopEffectMessage>((Action<StopEffectMessage>)HandleEffectMessage, true);
		NetworkClient.RegisterHandler<PlayCastEffectMessage>((Action<PlayCastEffectMessage>)HandleEffectMessage, true);
		NetworkClient.RegisterHandler<ApplySpeedMultiplierToEffectMessage>((Action<ApplySpeedMultiplierToEffectMessage>)HandleEffectMessage, true);
	}

	public static string GetPath(Transform root, Transform target)
	{
		int instanceID = target.GetInstanceID();
		if (_pathCache.TryGetValue(instanceID, out var value))
		{
			return value;
		}
		_getPathSb.Clear();
		int num = 0;
		Transform transform = target;
		while (true)
		{
			num++;
			if (num > 100)
			{
				Debug.LogError("Cannot get relative name, parent tree of '" + target.name + "' is too deep!");
				return null;
			}
			if (transform == null)
			{
				Debug.LogError("Cannot get relative name, maybe '" + target.name + "' is not parent of '" + root.name + "'?");
				return null;
			}
			if (transform == root)
			{
				_getPathSb.Insert(0, '/');
				string text = _getPathSb.ToString();
				_pathCache[instanceID] = text;
				return text;
			}
			string name = transform.name;
			if (name.Contains("/"))
			{
				break;
			}
			_getPathSb.Insert(0, '/');
			_getPathSb.Insert(0, name);
			transform = transform.parent;
		}
		Debug.LogError("Cannot get relative name, name contains '/' character!");
		return null;
	}

	public static GameObject ResolvePath(Transform root, string path, bool warnOnFail = true)
	{
		if (path == "/")
		{
			return root.gameObject;
		}
		string text = path.Substring(1);
		Transform transform = root.Find(text);
		if (transform == null)
		{
			if (warnOnFail)
			{
				Debug.LogError("Cannot resolve relative name: " + text);
			}
			return null;
		}
		return transform.gameObject;
	}

	private static uint FnvStep(uint hash, string name)
	{
		hash = (hash ^ 0x2F) * 16777619;
		for (int i = 0; i < name.Length; i++)
		{
			hash = (hash ^ name[i]) * 16777619;
		}
		return hash;
	}

	private static int FoldPathId(uint h)
	{
		if (h == 0 || h == 2147483648u)
		{
			h ^= 0x9E3779B9u;
		}
		return (int)h;
	}

	public static int GetPathId(Transform root, Transform target)
	{
		if (target == root)
		{
			return 0;
		}
		int instanceID = target.GetInstanceID();
		if (_pathIdCache.TryGetValue(instanceID, out var value))
		{
			return value;
		}
		uint h = HashUpToRoot(root, target, 0, out var ok);
		int num = (ok ? FoldPathId(h) : int.MinValue);
		_pathIdCache[instanceID] = num;
		return num;
	}

	private static uint HashUpToRoot(Transform root, Transform cursor, int depth, out bool ok)
	{
		if (cursor == root)
		{
			ok = true;
			return 2166136261u;
		}
		if (cursor == null || depth > 100)
		{
			Debug.LogError("Cannot get path id: tree too deep or root is not an ancestor of '" + root.name + "'.");
			ok = false;
			return 0u;
		}
		string name = cursor.name;
		if (name.IndexOf('/') >= 0)
		{
			Debug.LogError("Cannot get path id: name '" + name + "' contains '/'.");
			ok = false;
			return 0u;
		}
		uint hash = HashUpToRoot(root, cursor.parent, depth + 1, out ok);
		if (!ok)
		{
			return 0u;
		}
		return FnvStep(hash, name);
	}

	public static GameObject ResolvePathById(NetworkIdentity parent, int id)
	{
		switch (id)
		{
		case 0:
			return ((Component)(object)parent).gameObject;
		case int.MinValue:
			return null;
		default:
		{
			ulong sourceKey = GetSourceKey(parent);
			if (sourceKey != 0L && !IsSourceSkinnable(parent, sourceKey) && ResolveViaSourcePath(parent, sourceKey, id, out var go))
			{
				return go;
			}
			Dictionary<int, Transform> dictionary = parent.fxPathTable;
			if (dictionary == null)
			{
				dictionary = (parent.fxPathTable = new Dictionary<int, Transform>());
			}
			if (dictionary.TryGetValue(id, out var value) && value != null)
			{
				return value.gameObject;
			}
			Transform transform = LiveFindByHash(((Component)(object)parent).transform, 2166136261u, id);
			if (transform != null)
			{
				dictionary[id] = transform;
				return transform.gameObject;
			}
			return null;
		}
		}
	}

	private static Transform LiveFindByHash(Transform node, uint nodeHash, int id)
	{
		int childCount = node.childCount;
		for (int i = 0; i < childCount; i++)
		{
			Transform child = node.GetChild(i);
			if (child.name.IndexOf('/') < 0)
			{
				uint num = FnvStep(nodeHash, child.name);
				if (FoldPathId(num) == id)
				{
					return child;
				}
				Transform transform = LiveFindByHash(child, num, id);
				if (transform != null)
				{
					return transform;
				}
			}
		}
		return null;
	}

	private static ulong GetSourceKey(NetworkIdentity parent)
	{
		if (parent.sceneId == 0L)
		{
			return parent.assetId;
		}
		return parent.sceneId;
	}

	private static bool IsSourceSkinnable(NetworkIdentity parent, ulong key)
	{
		if (_sourceSkinnable.TryGetValue(key, out var value))
		{
			return value;
		}
		value = (UnityEngine.Object)(object)((Component)(object)parent).GetComponentInChildren<EntityVisual>(true) != null;
		_sourceSkinnable[key] = value;
		return value;
	}

	private static bool ResolveViaSourcePath(NetworkIdentity parent, ulong key, int id, out GameObject go)
	{
		go = null;
		if (!_sourcePathCache.TryGetValue(key, out var value))
		{
			value = new Dictionary<int, int[]>();
			_sourcePathCache[key] = value;
		}
		if (value.TryGetValue(id, out var value2))
		{
			Transform transform = ((Component)(object)parent).transform;
			for (int i = 0; i < value2.Length; i++)
			{
				if (value2[i] >= transform.childCount)
				{
					return false;
				}
				transform = transform.GetChild(value2[i]);
			}
			go = transform.gameObject;
			return true;
		}
		_indexPathBuffer.Clear();
		Transform transform2 = LiveFindByHashWithPath(((Component)(object)parent).transform, 2166136261u, id, _indexPathBuffer);
		if (transform2 == null)
		{
			return false;
		}
		value[id] = _indexPathBuffer.ToArray();
		go = transform2.gameObject;
		return true;
	}

	private static Transform LiveFindByHashWithPath(Transform node, uint nodeHash, int id, List<int> path)
	{
		int childCount = node.childCount;
		for (int i = 0; i < childCount; i++)
		{
			Transform child = node.GetChild(i);
			if (child.name.IndexOf('/') < 0)
			{
				uint num = FnvStep(nodeHash, child.name);
				path.Add(i);
				if (FoldPathId(num) == id)
				{
					return child;
				}
				Transform transform = LiveFindByHashWithPath(child, num, id, path);
				if (transform != null)
				{
					return transform;
				}
				path.RemoveAt(path.Count - 1);
			}
		}
		return null;
	}

	private static void PlayAllVisualEffects(Transform transform)
	{
		ListReturnHandle<VisualEffect> handle;
		foreach (VisualEffect item in ((Component)transform).GetComponentsInChildrenNonAlloc(out handle))
		{
			if (((Behaviour)(object)item).isActiveAndEnabled)
			{
				item.Play();
			}
		}
		handle.Return();
	}

	private static void StopAllVisualEffects(Transform transform)
	{
		ListReturnHandle<VisualEffect> handle;
		foreach (VisualEffect item in ((Component)transform).GetComponentsInChildrenNonAlloc(out handle))
		{
			item.Stop();
		}
		handle.Return();
	}

	private static void PlayAllParticleSystems(Transform transform)
	{
		if (!transform.gameObject.activeInHierarchy)
		{
			return;
		}
		if (transform.TryGetComponent<ParticleSystem>(out var component))
		{
			component.Play();
			return;
		}
		for (int i = 0; i < transform.childCount; i++)
		{
			PlayAllParticleSystems(transform.GetChild(i));
		}
	}

	public static ParticleSystem[] BakeResetParticleSystems(GameObject go)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		ParticleSystem[] componentsInChildren = go.GetComponentsInChildren<ParticleSystem>(includeInactive: true);
		List<ParticleSystem> list = new List<ParticleSystem>(componentsInChildren.Length);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			MainModule main = componentsInChildren[i].main;
			if (!main.playOnAwake)
			{
				list.Add(componentsInChildren[i]);
			}
		}
		return list.ToArray();
	}

	public static void StopResetParticleSystems(ParticleSystem[] particleSystems)
	{
		if (particleSystems == null)
		{
			return;
		}
		for (int i = 0; i < particleSystems.Length; i++)
		{
			if ((UnityEngine.Object)(object)particleSystems[i] != null)
			{
				particleSystems[i].Stop(false, (ParticleSystemStopBehavior)0);
			}
		}
	}

	private static void StopAllParticleSystems(Transform transform)
	{
		if (transform.TryGetComponent<ParticleSystem>(out var component))
		{
			component.Stop();
			ListReturnHandle<ParticleSystem> handle;
			foreach (ParticleSystem item in ((Component)transform).GetComponentsInChildrenNonAlloc(out handle))
			{
				if (((Component)(object)item).TryGetComponent(out FxParticleSystem component2))
				{
					if (component2.clearParticlesOnStop == FxParticleSystem.ClearParticlesBehavior.ClearSelf)
					{
						item.Clear(false);
					}
					else if (component2.clearParticlesOnStop == FxParticleSystem.ClearParticlesBehavior.ClearWithChildren)
					{
						item.Clear(true);
					}
				}
			}
			handle.Return();
		}
		else
		{
			for (int i = 0; i < transform.childCount; i++)
			{
				StopAllParticleSystems(transform.GetChild(i));
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool AssertServerActive()
	{
		if (!NetworkServer.active)
		{
			Debug.LogException(new InvalidOperationException("You can only play networked effects on server"));
			return false;
		}
		return true;
	}

	private static (GradientColorKey[] orig, GradientColorKey[] work) GetGradientColorKeys(Gradient g)
	{
		if (!_gradientKeyCache.TryGetValue(g, out (GradientColorKey[], GradientColorKey[]) value))
		{
			GradientColorKey[] colorKeys = g.colorKeys;
			value = (colorKeys, new GradientColorKey[colorKeys.Length]);
			_gradientKeyCache[g] = value;
		}
		return value;
	}

	public static void ChangeColorRecursively(GameObject gameObject, float? hue = null, float saturationMult = 1f, float valueMult = 1f, float alphaMult = 1f)
	{
		_psLights.Clear();
		try
		{
			List<ParticleSystem> componentsInChildrenNonAlloc = gameObject.GetComponentsInChildrenNonAlloc(out ListReturnHandle<ParticleSystem> handle);
			List<Light> componentsInChildrenNonAlloc2 = gameObject.GetComponentsInChildrenNonAlloc(out ListReturnHandle<Light> handle2);
			List<FxEntityColor> componentsInChildrenNonAlloc3 = gameObject.GetComponentsInChildrenNonAlloc(out ListReturnHandle<FxEntityColor> handle3);
			List<FxMeshTrail> componentsInChildrenNonAlloc4 = gameObject.GetComponentsInChildrenNonAlloc(out ListReturnHandle<FxMeshTrail> handle4);
			List<FxEntityShell> componentsInChildrenNonAlloc5 = gameObject.GetComponentsInChildrenNonAlloc(out ListReturnHandle<FxEntityShell> handle5);
			foreach (ParticleSystem item in componentsInChildrenNonAlloc)
			{
				ChangeColorObject(item, hue, saturationMult, valueMult, alphaMult);
			}
			foreach (Light item2 in componentsInChildrenNonAlloc2)
			{
				ChangeColorObject(item2, hue, saturationMult, valueMult, alphaMult);
			}
			foreach (FxEntityColor item3 in componentsInChildrenNonAlloc3)
			{
				ChangeColorObject(item3, hue, saturationMult, valueMult, alphaMult);
			}
			foreach (FxMeshTrail item4 in componentsInChildrenNonAlloc4)
			{
				ChangeColorObject(item4, hue, saturationMult, valueMult, alphaMult);
			}
			foreach (FxEntityShell item5 in componentsInChildrenNonAlloc5)
			{
				ChangeColorObject(item5, hue, saturationMult, valueMult, alphaMult);
			}
			handle.Return();
			handle2.Return();
			handle3.Return();
			handle4.Return();
			handle5.Return();
		}
		finally
		{
			_psLights.Clear();
		}
	}

	public static void ReplaceMaterialsRecursively(GameObject gameObject, Material from, Material to)
	{
		if (from == null || to == null)
		{
			return;
		}
		ListReturnHandle<Renderer> handle;
		foreach (Renderer item in gameObject.GetComponentsInChildrenNonAlloc(true, out handle))
		{
			if (item == null)
			{
				continue;
			}
			Material[] sharedMaterials = item.sharedMaterials;
			bool flag = false;
			for (int i = 0; i < sharedMaterials.Length; i++)
			{
				if (!(sharedMaterials[i] != from))
				{
					sharedMaterials[i] = to;
					flag = true;
				}
			}
			if (flag)
			{
				item.sharedMaterials = sharedMaterials;
			}
			ParticleSystemRenderer val = (ParticleSystemRenderer)(object)((item is ParticleSystemRenderer) ? item : null);
			if (val != null && val.trailMaterial == from)
			{
				val.trailMaterial = to;
			}
		}
		handle.Return();
	}

	private static Gradient CloneGradient(Gradient g)
	{
		if (g == null)
		{
			return null;
		}
		return new Gradient
		{
			mode = g.mode,
			colorKeys = g.colorKeys,
			alphaKeys = g.alphaKeys
		};
	}

	public static FxColorSnapshot CaptureColorsRecursively(GameObject gameObject)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Invalid comparison between Unknown and I4
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Invalid comparison between Unknown and I4
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		FxColorSnapshot fxColorSnapshot = new FxColorSnapshot();
		ParticleSystem[] componentsInChildren = gameObject.GetComponentsInChildren<ParticleSystem>(includeInactive: true);
		foreach (ParticleSystem val in componentsInChildren)
		{
			fxColorSnapshot.ps.Add(val);
			List<MinMaxGradient> psStart = fxColorSnapshot.psStart;
			MainModule main = val.main;
			psStart.Add(main.startColor);
			List<MinMaxGradient> psColLife = fxColorSnapshot.psColLife;
			ColorOverLifetimeModule colorOverLifetime = val.colorOverLifetime;
			psColLife.Add(colorOverLifetime.color);
			List<MinMaxGradient> psColSpeed = fxColorSnapshot.psColSpeed;
			ColorBySpeedModule colorBySpeed = val.colorBySpeed;
			psColSpeed.Add(colorBySpeed.color);
			CustomDataModule customData = val.customData;
			bool flag = (int)customData.GetMode((ParticleSystemCustomData)0) == 2;
			fxColorSnapshot.psHasCustom1.Add(flag);
			fxColorSnapshot.psCustom1.Add(flag ? customData.GetColor((ParticleSystemCustomData)0) : default(MinMaxGradient));
			bool flag2 = (int)customData.GetMode((ParticleSystemCustomData)1) == 2;
			fxColorSnapshot.psHasCustom2.Add(flag2);
			fxColorSnapshot.psCustom2.Add(flag2 ? customData.GetColor((ParticleSystemCustomData)1) : default(MinMaxGradient));
		}
		Light[] componentsInChildren2 = gameObject.GetComponentsInChildren<Light>(includeInactive: true);
		foreach (Light light in componentsInChildren2)
		{
			fxColorSnapshot.lights.Add(light);
			fxColorSnapshot.lightColors.Add(light.color);
		}
		FxEntityColor[] componentsInChildren3 = gameObject.GetComponentsInChildren<FxEntityColor>(includeInactive: true);
		foreach (FxEntityColor fxEntityColor in componentsInChildren3)
		{
			fxColorSnapshot.entityColors.Add(fxEntityColor);
			fxColorSnapshot.ecBase.Add(fxEntityColor.baseColor);
			fxColorSnapshot.ecEmission.Add(fxEntityColor.emission);
		}
		FxMeshTrail[] componentsInChildren4 = gameObject.GetComponentsInChildren<FxMeshTrail>(includeInactive: true);
		foreach (FxMeshTrail fxMeshTrail in componentsInChildren4)
		{
			fxColorSnapshot.trails.Add(fxMeshTrail);
			fxColorSnapshot.trailGrads.Add(CloneGradient(fxMeshTrail.trailGradient));
		}
		FxEntityShell[] componentsInChildren5 = gameObject.GetComponentsInChildren<FxEntityShell>(includeInactive: true);
		foreach (FxEntityShell fxEntityShell in componentsInChildren5)
		{
			fxColorSnapshot.shells.Add(fxEntityShell);
			fxColorSnapshot.shellColors.Add(fxEntityShell.color);
		}
		return fxColorSnapshot;
	}

	public static void RestoreColorsRecursively(FxColorSnapshot snap)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		if (snap == null)
		{
			return;
		}
		for (int i = 0; i < snap.ps.Count; i++)
		{
			ParticleSystem val = snap.ps[i];
			if (!((UnityEngine.Object)(object)val == null))
			{
				MainModule main = val.main;
				main.startColor = snap.psStart[i];
				ColorOverLifetimeModule colorOverLifetime = val.colorOverLifetime;
				colorOverLifetime.color = snap.psColLife[i];
				ColorBySpeedModule colorBySpeed = val.colorBySpeed;
				colorBySpeed.color = snap.psColSpeed[i];
				CustomDataModule customData = val.customData;
				if (snap.psHasCustom1[i])
				{
					customData.SetColor((ParticleSystemCustomData)0, snap.psCustom1[i]);
				}
				if (snap.psHasCustom2[i])
				{
					customData.SetColor((ParticleSystemCustomData)1, snap.psCustom2[i]);
				}
			}
		}
		for (int j = 0; j < snap.lights.Count; j++)
		{
			if (snap.lights[j] != null)
			{
				snap.lights[j].color = snap.lightColors[j];
			}
		}
		for (int k = 0; k < snap.entityColors.Count; k++)
		{
			if (snap.entityColors[k] != null)
			{
				snap.entityColors[k].baseColor = snap.ecBase[k];
				snap.entityColors[k].emission = snap.ecEmission[k];
			}
		}
		for (int l = 0; l < snap.trails.Count; l++)
		{
			if (snap.trails[l] != null)
			{
				snap.trails[l].trailGradient = CloneGradient(snap.trailGrads[l]);
			}
		}
		for (int m = 0; m < snap.shells.Count; m++)
		{
			if (snap.shells[m] != null)
			{
				snap.shells[m].color = snap.shellColors[m];
			}
		}
	}

	public static void ChangeColorObject(object obj, float? hue = null, float saturationMult = 1f, float valueMult = 1f, float alphaMult = 1f)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Invalid comparison between Unknown and I4
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Invalid comparison between Unknown and I4
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		ParticleSystem val = (ParticleSystem)((obj is ParticleSystem) ? obj : null);
		if (val != null)
		{
			MainModule main = val.main;
			main.startColor = EditColor(main.startColor, hue, saturationMult, valueMult, alphaMult);
			ColorOverLifetimeModule colorOverLifetime = val.colorOverLifetime;
			colorOverLifetime.color = EditColor(colorOverLifetime.color, hue, saturationMult, valueMult, alphaMult);
			ColorBySpeedModule colorBySpeed = val.colorBySpeed;
			colorBySpeed.color = EditColor(colorBySpeed.color, hue, saturationMult, valueMult, alphaMult);
			CustomDataModule customData = val.customData;
			if ((int)customData.GetMode((ParticleSystemCustomData)0) == 2)
			{
				customData.SetColor((ParticleSystemCustomData)0, EditColor(customData.GetColor((ParticleSystemCustomData)0), hue, saturationMult, valueMult, alphaMult));
			}
			if ((int)customData.GetMode((ParticleSystemCustomData)1) == 2)
			{
				customData.SetColor((ParticleSystemCustomData)1, EditColor(customData.GetColor((ParticleSystemCustomData)1), hue, saturationMult, valueMult, alphaMult));
			}
			LightsModule lights = val.lights;
			if (lights.light != null && lights.useParticleColor)
			{
				_psLights.Add(lights.light);
			}
		}
		if (obj is Light light)
		{
			if (_psLights.Contains(light))
			{
				return;
			}
			light.color = EditColor(light.color, hue, saturationMult, valueMult, alphaMult);
		}
		if (obj is FxEntityColor fxEntityColor)
		{
			fxEntityColor.baseColor = EditColor(fxEntityColor.baseColor, hue, saturationMult, valueMult, alphaMult);
			fxEntityColor.emission = EditColor(fxEntityColor.emission, hue, saturationMult, valueMult, alphaMult);
		}
		if (obj is FxMeshTrail fxMeshTrail)
		{
			fxMeshTrail.trailGradient = EditColor(fxMeshTrail.trailGradient, hue, saturationMult, valueMult, alphaMult);
		}
		if (obj is FxEntityShell fxEntityShell)
		{
			fxEntityShell.color = EditColor(fxEntityShell.color, hue, saturationMult, valueMult, alphaMult);
		}
	}

	public static void TintRecursively(GameObject gameObject, Color color)
	{
		ListReturnHandle<Component> handle;
		foreach (Component item in gameObject.GetComponentsInChildrenNonAlloc(true, out handle))
		{
			TintObject(item, color, dontLogWarning: true);
		}
		handle.Return();
	}

	public static void TintObject(object obj, Color color, bool dontLogWarning = false)
	{
		color.a = 1f;
		ParticleSystem val = (ParticleSystem)((obj is ParticleSystem) ? obj : null);
		if (val != null)
		{
			val.TintMainColor(color);
			return;
		}
		if (obj is Light light)
		{
			light.color *= color;
			return;
		}
		Image val2 = (Image)((obj is Image) ? obj : null);
		if (val2 != null)
		{
			((Graphic)val2).color = ((Graphic)val2).color * color;
		}
		else if (obj is FxEntityColor fxEntityColor)
		{
			fxEntityColor.baseColor *= color;
			fxEntityColor.emission *= color;
		}
		else if (obj is FxMeshTrail fxMeshTrail)
		{
			Gradient gradient = new Gradient();
			gradient.mode = fxMeshTrail.trailGradient.mode;
			gradient.alphaKeys = fxMeshTrail.trailGradient.alphaKeys;
			GradientColorKey[] colorKeys = fxMeshTrail.trailGradient.colorKeys;
			for (int i = 0; i < colorKeys.Length; i++)
			{
				colorKeys[i].color *= color;
			}
			gradient.colorKeys = colorKeys;
			fxMeshTrail.trailGradient = gradient;
		}
		else if (!dontLogWarning)
		{
			Debug.LogWarning($"{obj.GetType()} is not tint-able: {obj}");
		}
	}

	public static Color EditColor(Color color, float? newHue, float sm, float vm, float am)
	{
		float a = color.a;
		Color.RGBToHSV(color, out var H, out var S, out var V);
		Color result = Color.HSVToRGB(newHue ?? H, S * sm, V * vm, hdr: true);
		result.a = Mathf.Clamp01(a * am);
		return result;
	}

	public static Gradient EditColor(Gradient gradient, float? newHue, float sm, float vm, float am)
	{
		Gradient gradient2 = new Gradient();
		gradient2.mode = gradient.mode;
		GradientColorKey[] colorKeys = gradient.colorKeys;
		for (int i = 0; i < colorKeys.Length; i++)
		{
			colorKeys[i].color = EditColor(colorKeys[i].color, newHue, sm, vm, am);
		}
		gradient2.colorKeys = colorKeys;
		gradient2.alphaKeys = gradient.alphaKeys;
		return gradient2;
	}

	public static MinMaxGradient EditColor(MinMaxGradient gradient, float? newHue, float sm, float vm, float am)
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		gradient.colorMin = EditColor(gradient.colorMin, newHue, sm, vm, am);
		gradient.colorMax = EditColor(gradient.colorMax, newHue, sm, vm, am);
		if (gradient.gradientMin != null)
		{
			EditColorInPlace(gradient.gradientMin, newHue, sm, vm, am);
		}
		if (gradient.gradientMax != null)
		{
			EditColorInPlace(gradient.gradientMax, newHue, sm, vm, am);
		}
		return gradient;
	}

	private static void EditColorInPlace(Gradient gradient, float? newHue, float sm, float vm, float am)
	{
		(GradientColorKey[] orig, GradientColorKey[] work) gradientColorKeys = GetGradientColorKeys(gradient);
		GradientColorKey[] item = gradientColorKeys.orig;
		GradientColorKey[] item2 = gradientColorKeys.work;
		for (int i = 0; i < item.Length; i++)
		{
			item2[i] = new GradientColorKey(EditColor(item[i].color, newHue, sm, vm, am), item[i].time);
		}
		gradient.colorKeys = item2;
	}
}
