using System;
using System.Collections.Generic;
using System.Diagnostics;
using Mirror;
using UnityEngine;

public class SpawnManager : ManagerBase<SpawnManager>
{
	[NonSerialized]
	public bool usePooling;

	[NonSerialized]
	public bool prewarmEnabled = true;

	[NonSerialized]
	public bool prewarmContributorsEnabled = true;

	[NonSerialized]
	public bool showPoolInfo;

	[NonSerialized]
	public int lastClearedZoneIndex = -1;

	public Dictionary<GameObject, ObjectPoolItem> pool = new Dictionary<GameObject, ObjectPoolItem>();

	public Dictionary<GameObject, ObjectPoolItem> activeInstanceToPool = new Dictionary<GameObject, ObjectPoolItem>();

	public HashSet<GameObject> prewarmedPrefabs = new HashSet<GameObject>();

	private readonly HashSet<GameObject> _parkedInactive = new HashSet<GameObject>();

	public bool previousRoomWasBoss;

	private static readonly Dictionary<int, bool> _reuseInRoomPrefabCache = new Dictionary<int, bool>();

	private static readonly Dictionary<int, bool> _isMonsterAbilityCache = new Dictionary<int, bool>(256);

	private const int kReuseInRoomDropAfterRooms = 3;

	private const int kReuseInRoomMonsterPrewarmCap = 30;

	private void Park(ObjectPoolItem data, GameObject gobj)
	{
		data.inactiveInstances.Add(gobj);
		_parkedInactive.Add(gobj);
	}

	private void DestroyParkedFrom(ObjectPoolItem data, int keepCount)
	{
		for (int num = data.inactiveInstances.Count - 1; num >= keepCount; num--)
		{
			GameObject gameObject = data.inactiveInstances[num];
			data.inactiveInstances.RemoveAt(num);
			_parkedInactive.Remove(gameObject);
			if (gameObject != null)
			{
				UnityEngine.Object.Destroy(gameObject);
			}
		}
	}

	protected override void Awake()
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Expected Obj, but got Unknown
		//IL_0053: Expected Obj, but got Unknown
		base.Awake();
		AdaptiveAbilityBaseline.Clear();
		DewEffect.ClearPool();
		CommandMarker.ClearPool();
		foreach (KeyValuePair<uint, string> item in DewResources.database.netObjectAssetIdToGuid)
		{
			NetworkClient.RegisterSpawnHandler(item.Key, (SpawnHandlerDelegate)SpawnFromDewDatabaseHandler, (UnSpawnDelegate)CustomDespawnHandler);
		}
	}

	private GameObject SpawnFromDewDatabaseHandler(SpawnMessage msg)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		Actor parentActor = null;
		if (msg.parentActorNetId != 0)
		{
			NetworkIdentity spawnedInServerOrClient = Utils.GetSpawnedInServerOrClient(msg.parentActorNetId);
			if ((bool)(UnityEngine.Object)(object)spawnedInServerOrClient)
			{
				parentActor = ((Component)(object)spawnedInServerOrClient).GetComponent<Actor>();
			}
		}
		GameObject gameObject = null;
		if (DewResources.database.netObjectAssetIdToGuid.TryGetValue(msg.assetId, out var value) && DewResources.database.guidToType.TryGetValue(value, out var value2) && typeof(Actor).IsAssignableFrom(value2))
		{
			gameObject = DewResources.GetNetworkedPrefab(msg.assetId, new ResourceLoadSettings
			{
				varDef = DewResources.GetSuggestedVarDef(parentActor, value2)
			});
		}
		if (gameObject == null)
		{
			gameObject = DewResources.GetNetworkedPrefab(msg.assetId);
		}
		if (gameObject == null)
		{
			UnityEngine.Debug.LogError($"Failed to get networked prefab of assetId: {msg.assetId}");
			return null;
		}
		if (!pool.ContainsKey(gameObject))
		{
			ObjectPoolItem objectPoolItem = new ObjectPoolItem();
			Actor actor = ResolveActorComponent(gameObject);
			objectPoolItem.isMonster = actor is Monster;
			objectPoolItem.isPickup = actor is PickupInstance;
			objectPoolItem.reuseInRoom = (UnityEngine.Object)(object)actor != null && actor.reuseInRoom;
			pool[gameObject] = objectPoolItem;
		}
		GameObject gameObject2 = Create(gameObject, msg.position, msg.rotation, null);
		gameObject2.transform.localScale = msg.scale;
		gameObject2.name = gameObject.name;
		return gameObject2;
	}

	public static T Create<T>(T prefab, Vector3 position, Quaternion rotation) where T : UnityEngine.Object
	{
		return Create(prefab, position, rotation, null);
	}

	public static T Create<T>(T prefab, Vector3 position, Quaternion rotation, Transform parent) where T : UnityEngine.Object
	{
		return CreateInternal(prefab, position, rotation, parent);
	}

	private static Actor ResolveActorComponent(UnityEngine.Object prefab)
	{
		if (prefab is Actor result)
		{
			return result;
		}
		if (prefab is GameObject gameObject)
		{
			return gameObject.GetComponent<Actor>();
		}
		if (prefab is Component component)
		{
			return component.GetComponent<Actor>();
		}
		return null;
	}

	private static bool IsReuseInRoomPrefab(GameObject prefabGobj, UnityEngine.Object prefab)
	{
		int instanceID = prefabGobj.GetInstanceID();
		if (_reuseInRoomPrefabCache.TryGetValue(instanceID, out var value))
		{
			return value;
		}
		Actor actor = ResolveActorComponent(prefab);
		bool flag = (UnityEngine.Object)(object)actor != null && actor.reuseInRoom;
		_reuseInRoomPrefabCache[instanceID] = flag;
		return flag;
	}

	public static bool TryGetReuseInRoomActiveCount(GameObject prefabGobj, out int activeCount)
	{
		activeCount = 0;
		if (ManagerBase<SpawnManager>.instance == null || prefabGobj == null)
		{
			return false;
		}
		if (!ManagerBase<SpawnManager>.instance.pool.TryGetValue(prefabGobj, out var value))
		{
			return false;
		}
		if (!value.reuseInRoom)
		{
			return false;
		}
		activeCount = value.activeInstances;
		return true;
	}

	private static T Create_StatusEffect<T>(T prefab, Vector3 position, Quaternion rotation, Transform parent) where T : UnityEngine.Object
	{
		return CreateInternal(prefab, position, rotation, parent);
	}

	private static T Create_Pickup<T>(T prefab, Vector3 position, Quaternion rotation, Transform parent) where T : UnityEngine.Object
	{
		return CreateInternal(prefab, position, rotation, parent);
	}

	private static T Create_DamageInstance<T>(T prefab, Vector3 position, Quaternion rotation, Transform parent) where T : UnityEngine.Object
	{
		return CreateInternal(prefab, position, rotation, parent);
	}

	private static T Create_Ability<T>(T prefab, Vector3 position, Quaternion rotation, Transform parent) where T : UnityEngine.Object
	{
		return CreateInternal(prefab, position, rotation, parent);
	}

	private static T Create_Hero<T>(T prefab, Vector3 position, Quaternion rotation, Transform parent) where T : UnityEngine.Object
	{
		return CreateInternal(prefab, position, rotation, parent);
	}

	private static T Create_Monster<T>(T prefab, Vector3 position, Quaternion rotation, Transform parent) where T : UnityEngine.Object
	{
		return CreateInternal(prefab, position, rotation, parent);
	}

	private static T Create_Entity<T>(T prefab, Vector3 position, Quaternion rotation, Transform parent) where T : UnityEngine.Object
	{
		return CreateInternal(prefab, position, rotation, parent);
	}

	private static T Create_Actor<T>(T prefab, Vector3 position, Quaternion rotation, Transform parent) where T : UnityEngine.Object
	{
		return CreateInternal(prefab, position, rotation, parent);
	}

	private static T Create_Generic<T>(T prefab, Vector3 position, Quaternion rotation, Transform parent) where T : UnityEngine.Object
	{
		return CreateInternal(prefab, position, rotation, parent);
	}

	public static bool IsMonsterAbilityPrefab(GameObject prefab)
	{
		if (prefab == null)
		{
			return false;
		}
		int instanceID = prefab.GetInstanceID();
		if (_isMonsterAbilityCache.TryGetValue(instanceID, out var value))
		{
			return value;
		}
		string text = prefab.name;
		bool flag = !string.IsNullOrEmpty(text) && (text.StartsWith("Ai_Mon_", StringComparison.Ordinal) || text.StartsWith("Se_Mon_", StringComparison.Ordinal));
		_isMonsterAbilityCache[instanceID] = flag;
		return flag;
	}

	private static T CreateInternal<T>(T prefab, Vector3 position, Quaternion rotation, Transform parent) where T : UnityEngine.Object
	{
		GameObject gameObject = null;
		if (prefab is GameObject gameObject2)
		{
			gameObject = gameObject2;
		}
		else
		{
			if (!(prefab is Component component))
			{
				throw new InvalidOperationException("Provided type is neither a GameObject nor a Component");
			}
			gameObject = component.gameObject;
		}
		SpawnManager spawnManager = ManagerBase<SpawnManager>.instance;
		if (spawnManager == null)
		{
			return UnityEngine.Object.Instantiate(prefab, position, rotation, parent);
		}
		if (!spawnManager.usePooling && !spawnManager.pool.ContainsKey(gameObject) && !IsReuseInRoomPrefab(gameObject, prefab))
		{
			return UnityEngine.Object.Instantiate(prefab, position, rotation, parent);
		}
		if (!spawnManager.pool.TryGetValue(gameObject, out var value))
		{
			value = new ObjectPoolItem();
			Actor actor = ResolveActorComponent(prefab);
			value.isMonster = actor is Monster;
			value.isPickup = actor is PickupInstance;
			value.reuseInRoom = (UnityEngine.Object)(object)actor != null && actor.reuseInRoom;
			spawnManager.pool[gameObject] = value;
		}
		value.activeInstances++;
		value.roomsSinceUsed = 0;
		if (value.activeInstances > value.peakActiveThisRoom)
		{
			value.peakActiveThisRoom = value.activeInstances;
		}
		if (value.activeInstances > value.maxActiveObserved)
		{
			value.maxActiveObserved = value.activeInstances;
		}
		while (value.inactiveInstances.Count > 0)
		{
			GameObject gameObject3 = value.inactiveInstances[value.inactiveInstances.Count - 1];
			value.inactiveInstances.RemoveAt(value.inactiveInstances.Count - 1);
			spawnManager._parkedInactive.Remove(gameObject3);
			if (gameObject3 == null)
			{
				UnityEngine.Debug.LogError("Pooled instance of '" + gameObject.name + "' was destroyed while parked in the pool - dropping it");
				continue;
			}
			spawnManager.activeInstanceToPool[gameObject3] = value;
			gameObject3.transform.SetPositionAndRotation(position, rotation);
			gameObject3.SetActive(value: true);
			value.lastReuseUnscaledTime = Time.unscaledTime;
			if (gameObject3 is T result)
			{
				return result;
			}
			return gameObject3.GetComponent<T>();
		}
		GameObject gameObject4 = UnityEngine.Object.Instantiate(gameObject, position, rotation, parent);
		spawnManager.activeInstanceToPool[gameObject4] = value;
		value.lastInstantiateUnscaledTime = Time.unscaledTime;
		if (!(prefab is GameObject))
		{
			return gameObject4.GetComponent<T>();
		}
		return gameObject4 as T;
	}

	public static void Prewarm<T>(T prefab, int count) where T : UnityEngine.Object
	{
		if (!(ManagerBase<SpawnManager>.instance == null) && ManagerBase<SpawnManager>.instance.prewarmEnabled)
		{
			Actor actor = ResolveActorComponent(prefab);
			if (actor is StatusEffect)
			{
				Prewarm_StatusEffect(prefab, count);
			}
			else if (actor is PickupInstance)
			{
				Prewarm_Pickup(prefab, count);
			}
			else if (actor is DamageInstance)
			{
				Prewarm_DamageInstance(prefab, count);
			}
			else if (actor is AbilityInstance)
			{
				Prewarm_Ability(prefab, count);
			}
			else if (actor is Hero)
			{
				Prewarm_Hero(prefab, count);
			}
			else if (actor is Monster)
			{
				Prewarm_Monster(prefab, count);
			}
			else if (actor is Entity)
			{
				Prewarm_Entity(prefab, count);
			}
			else if ((UnityEngine.Object)(object)actor != null)
			{
				Prewarm_Actor(prefab, count);
			}
			else
			{
				Prewarm_Generic(prefab, count);
			}
		}
	}

	private static void Prewarm_StatusEffect<T>(T prefab, int count) where T : UnityEngine.Object
	{
		PrewarmInternal(prefab, count);
	}

	private static void Prewarm_Pickup<T>(T prefab, int count) where T : UnityEngine.Object
	{
		PrewarmInternal(prefab, count);
	}

	private static void Prewarm_DamageInstance<T>(T prefab, int count) where T : UnityEngine.Object
	{
		PrewarmInternal(prefab, count);
	}

	private static void Prewarm_Ability<T>(T prefab, int count) where T : UnityEngine.Object
	{
		PrewarmInternal(prefab, count);
	}

	private static void Prewarm_Hero<T>(T prefab, int count) where T : UnityEngine.Object
	{
		PrewarmInternal(prefab, count);
	}

	private static void Prewarm_Monster<T>(T prefab, int count) where T : UnityEngine.Object
	{
		PrewarmInternal(prefab, count);
	}

	private static void Prewarm_Entity<T>(T prefab, int count) where T : UnityEngine.Object
	{
		PrewarmInternal(prefab, count);
	}

	private static void Prewarm_Actor<T>(T prefab, int count) where T : UnityEngine.Object
	{
		PrewarmInternal(prefab, count);
	}

	private static void Prewarm_Generic<T>(T prefab, int count) where T : UnityEngine.Object
	{
		PrewarmInternal(prefab, count);
	}

	private static void PrewarmInternal<T>(T prefab, int count) where T : UnityEngine.Object
	{
		if (ManagerBase<SpawnManager>.instance == null || count <= 0)
		{
			return;
		}
		GameObject gameObject = null;
		if (prefab is GameObject gameObject2)
		{
			gameObject = gameObject2;
		}
		else
		{
			if (!(prefab is Component component))
			{
				throw new InvalidOperationException("Provided type is neither a GameObject nor a Component");
			}
			gameObject = component.gameObject;
		}
		ManagerBase<SpawnManager>.instance.prewarmedPrefabs.Add(gameObject);
		DewEffect.PrewarmBakeInPlaceEffects(gameObject);
		if (!ManagerBase<SpawnManager>.instance.pool.TryGetValue(gameObject, out var value))
		{
			value = new ObjectPoolItem();
			Actor actor = ResolveActorComponent(prefab);
			value.isMonster = actor is Monster;
			value.isPickup = actor is PickupInstance;
			value.reuseInRoom = (UnityEngine.Object)(object)actor != null && actor.reuseInRoom;
			ManagerBase<SpawnManager>.instance.pool[gameObject] = value;
		}
		if (value.reuseInRoom && IsMonsterAbilityPrefab(gameObject))
		{
			Actor actor2 = ResolveActorComponent(gameObject);
			if ((UnityEngine.Object)(object)actor2 == null || !actor2.reuseInRoomSkipPrewarmCap)
			{
				int num = Mathf.Max(30, value.maxActiveObserved);
				if (count > num)
				{
					count = num;
				}
			}
		}
		int num2 = count - value.inactiveInstances.Count;
		if (num2 <= 0)
		{
			if (value.reuseInRoom && IsMonsterAbilityPrefab(gameObject))
			{
				int keepCount = Mathf.Max(0, count - value.activeInstances);
				ManagerBase<SpawnManager>.instance.DestroyParkedFrom(value, keepCount);
			}
			return;
		}
		for (int i = 0; i < num2; i++)
		{
			GameObject gameObject3 = UnityEngine.Object.Instantiate(gameObject, Vector3.zero, Quaternion.identity);
			if (gameObject3.TryGetComponent<Entity>(out var component2))
			{
				if ((UnityEngine.Object)(object)component2.Visual != null)
				{
					try
					{
						component2.Visual.PreloadVisuals();
					}
					catch (Exception exception)
					{
						UnityEngine.Debug.LogException(exception);
					}
				}
				if ((UnityEngine.Object)(object)component2.Control != null)
				{
					try
					{
						component2.Control.PreloadStart();
					}
					catch (Exception exception2)
					{
						UnityEngine.Debug.LogException(exception2);
					}
				}
			}
			UnityEngine.Object.DontDestroyOnLoad(gameObject3);
			gameObject3.SetActive(value: false);
			ManagerBase<SpawnManager>.instance.Park(value, gameObject3);
			value.lastInstantiateUnscaledTime = Time.unscaledTime;
		}
	}

	public static void Destroy(GameObject gobj)
	{
		if (gobj == null)
		{
			UnityEngine.Debug.LogWarning("Object provided for destruction is null");
			return;
		}
		if (ManagerBase<SpawnManager>.instance == null)
		{
			UnityEngine.Object.Destroy(gobj);
			return;
		}
		if (!ManagerBase<SpawnManager>.instance.activeInstanceToPool.TryGetValue(gobj, out var value))
		{
			if (ManagerBase<SpawnManager>.instance._parkedInactive.Contains(gobj))
			{
				UnityEngine.Debug.LogWarning($"Tried to return '{gobj.name}' to the pool twice (no-op)\n{new StackTrace(1, fNeedFileInfo: false)}");
			}
			else
			{
				UnityEngine.Object.Destroy(gobj);
			}
			return;
		}
		if (!ManagerBase<SpawnManager>.instance.usePooling && !value.reuseInRoom)
		{
			value.activeInstances--;
			ManagerBase<SpawnManager>.instance.activeInstanceToPool.Remove(gobj);
			UnityEngine.Object.Destroy(gobj);
			return;
		}
		value.lastDestroyUnscaledTime = Time.unscaledTime;
		value.activeInstances--;
		ManagerBase<SpawnManager>.instance.Park(value, gobj);
		if (gobj.TryGetComponent<Actor>(out var component))
		{
			component.ResetParticleSystemsForReuse();
		}
		gobj.SetActive(value: false);
		ManagerBase<SpawnManager>.instance.activeInstanceToPool.Remove(gobj);
	}

	public static bool IsParkedInPool(GameObject gobj)
	{
		if (ManagerBase<SpawnManager>.instance == null || gobj == null)
		{
			return false;
		}
		return ManagerBase<SpawnManager>.instance._parkedInactive.Contains(gobj);
	}

	public static void SetReuseInRoom(GameObject prefabGobj, bool value)
	{
		if (!(ManagerBase<SpawnManager>.instance == null) && !(prefabGobj == null) && ManagerBase<SpawnManager>.instance.pool.TryGetValue(prefabGobj, out var value2))
		{
			value2.reuseInRoom = value;
		}
	}

	public static void InvalidateInstance(Component activeInstance)
	{
		if (!(activeInstance == null))
		{
			InvalidateInstance(activeInstance.gameObject);
		}
	}

	public static void InvalidateInstance(GameObject activeInstance)
	{
		if (!(ManagerBase<SpawnManager>.instance == null) && !(activeInstance == null) && ManagerBase<SpawnManager>.instance.activeInstanceToPool.TryGetValue(activeInstance, out var value))
		{
			value.activeInstances--;
			ManagerBase<SpawnManager>.instance.activeInstanceToPool.Remove(activeInstance);
		}
	}

	public static void ReleaseInactivePool(GameObject prefab)
	{
		if (!(ManagerBase<SpawnManager>.instance == null) && !(prefab == null) && ManagerBase<SpawnManager>.instance.pool.TryGetValue(prefab, out var value))
		{
			ManagerBase<SpawnManager>.instance.DestroyParkedFrom(value, 0);
			if (value.activeInstances == 0)
			{
				ManagerBase<SpawnManager>.instance.pool.Remove(prefab);
			}
		}
	}

	public static int GetMonsterPoolCount()
	{
		if (ManagerBase<SpawnManager>.instance == null)
		{
			return 0;
		}
		int num = 0;
		foreach (KeyValuePair<GameObject, ObjectPoolItem> item in ManagerBase<SpawnManager>.instance.pool)
		{
			ObjectPoolItem value = item.Value;
			bool flag = value.isMonster;
			if (!flag && item.Key != null && item.Key.TryGetComponent<Monster>(out var _))
			{
				value.isMonster = true;
				flag = true;
			}
			if (flag)
			{
				num += value.activeInstances + value.inactiveInstances.Count;
			}
		}
		return num;
	}

	public static int GetPickupPoolCount()
	{
		if (ManagerBase<SpawnManager>.instance == null)
		{
			return 0;
		}
		int num = 0;
		foreach (KeyValuePair<GameObject, ObjectPoolItem> item in ManagerBase<SpawnManager>.instance.pool)
		{
			ObjectPoolItem value = item.Value;
			bool flag = value.isPickup;
			if (!flag && item.Key != null && item.Key.TryGetComponent<PickupInstance>(out var _))
			{
				value.isPickup = true;
				flag = true;
			}
			if (flag)
			{
				num += value.activeInstances + value.inactiveInstances.Count;
			}
		}
		return num;
	}

	public static void ReleaseInactivePoolsExcept(HashSet<GameObject> keepPrefabs)
	{
		if (ManagerBase<SpawnManager>.instance == null)
		{
			return;
		}
		ManagerBase<SpawnManager>.instance.prewarmedPrefabs.Clear();
		List<GameObject> list = null;
		Dictionary<ObjectPoolItem, int> dictionary = new Dictionary<ObjectPoolItem, int>();
		foreach (KeyValuePair<GameObject, ObjectPoolItem> item in ManagerBase<SpawnManager>.instance.activeInstanceToPool)
		{
			if (item.Key == null)
			{
				if (list == null)
				{
					list = new List<GameObject>();
				}
				list.Add(item.Key);
				item.Value.activeInstances--;
				dictionary.TryGetValue(item.Value, out var value);
				dictionary[item.Value] = value + 1;
			}
		}
		if (list != null)
		{
			for (int i = 0; i < list.Count; i++)
			{
				ManagerBase<SpawnManager>.instance.activeInstanceToPool.Remove(list[i]);
			}
		}
		List<GameObject> list2 = null;
		foreach (KeyValuePair<GameObject, ObjectPoolItem> item2 in ManagerBase<SpawnManager>.instance.pool)
		{
			bool flag = keepPrefabs?.Contains(item2.Key) ?? false;
			if (item2.Value.reuseInRoom && item2.Key != null)
			{
				item2.Value.roomsSinceUsed++;
				if (flag || item2.Value.roomsSinceUsed < 3)
				{
					continue;
				}
			}
			else if (flag)
			{
				continue;
			}
			ObjectPoolItem value2 = item2.Value;
			ManagerBase<SpawnManager>.instance.DestroyParkedFrom(value2, 0);
			if (value2.activeInstances == 0)
			{
				if (list2 == null)
				{
					list2 = new List<GameObject>();
				}
				list2.Add(item2.Key);
			}
		}
		if (list2 != null)
		{
			for (int j = 0; j < list2.Count; j++)
			{
				ManagerBase<SpawnManager>.instance.pool.Remove(list2[j]);
			}
		}
	}

	public static void ClearMonsterAbilityPools()
	{
		if (ManagerBase<SpawnManager>.instance == null)
		{
			return;
		}
		List<GameObject> list = null;
		foreach (KeyValuePair<GameObject, ObjectPoolItem> item in ManagerBase<SpawnManager>.instance.pool)
		{
			if (item.Key == null || !IsMonsterAbilityPrefab(item.Key))
			{
				continue;
			}
			ObjectPoolItem value = item.Value;
			ManagerBase<SpawnManager>.instance.DestroyParkedFrom(value, 0);
			value.roomsSinceUsed = 0;
			ManagerBase<SpawnManager>.instance.prewarmedPrefabs.Remove(item.Key);
			if (value.activeInstances == 0)
			{
				if (list == null)
				{
					list = new List<GameObject>();
				}
				list.Add(item.Key);
			}
		}
		if (list != null)
		{
			for (int i = 0; i < list.Count; i++)
			{
				ManagerBase<SpawnManager>.instance.pool.Remove(list[i]);
			}
		}
		_reuseInRoomPrefabCache.Clear();
		_isMonsterAbilityCache.Clear();
	}

	private void CustomDespawnHandler(GameObject gobj)
	{
		List<ICustomDestroyRoutine> componentsNonAlloc = gobj.GetComponentsNonAlloc(out ListReturnHandle<ICustomDestroyRoutine> handle);
		if (componentsNonAlloc.Count == 0)
		{
			Destroy(gobj);
		}
		else
		{
			foreach (ICustomDestroyRoutine item in componentsNonAlloc)
			{
				item.CustomDestroyRoutine();
			}
		}
		handle.Return();
	}

	public static int DropPoolsOfDestroyedPrefabs()
	{
		if (ManagerBase<SpawnManager>.instance == null)
		{
			return 0;
		}
		List<GameObject> list = null;
		int num = 0;
		foreach (KeyValuePair<GameObject, ObjectPoolItem> item in ManagerBase<SpawnManager>.instance.pool)
		{
			if (!(item.Key != null))
			{
				ObjectPoolItem value = item.Value;
				num += value.inactiveInstances.Count;
				ManagerBase<SpawnManager>.instance.DestroyParkedFrom(value, 0);
				if (value.activeInstances == 0)
				{
					(list ?? (list = new List<GameObject>())).Add(item.Key);
				}
			}
		}
		if (list != null)
		{
			for (int i = 0; i < list.Count; i++)
			{
				ManagerBase<SpawnManager>.instance.pool.Remove(list[i]);
				ManagerBase<SpawnManager>.instance.prewarmedPrefabs.Remove(list[i]);
			}
		}
		return num;
	}
}
