using System;
using System.Collections.Generic;
using System.Linq;
using DewInternal;
using Mirror;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public static class DewResources
{
	private struct PreloadRuleEntry
	{
		public MonoBehaviour owner;

		public Action<PreloadInterface> onPreload;
	}

	public const string MainDatabase = "MainResources";

	private static DewResourceDatabase _database;

	private static Material _transparentMat;

	private static Dictionary<string, AsyncOperationHandle<UnityEngine.Object>> _loadedObjects = new Dictionary<string, AsyncOperationHandle<UnityEngine.Object>>();

	public static bool EnableVerboseLogging = false;

	private static PreloadInterface _preloadInterface = new PreloadInterface();

	private static readonly Dictionary<Type, ResourceLinkBy> _linkTypeByType = new Dictionary<Type, ResourceLinkBy>();

	private static List<PreloadRuleEntry> _preloadRules = new List<PreloadRuleEntry>();

	private static Burst[] _burstsBuffer = new Burst[64];

	public static SafeAction onVariantsCleared;

	public static SafeAction<string> onAssetVariantsDestroyed;

	private static Dictionary<int, ResourceVariantProcessor> _processors = new Dictionary<int, ResourceVariantProcessor>();

	private static Dictionary<string, Dictionary<VariantDef, (UnityEngine.Object, SafeAction)>> _variants = new Dictionary<string, Dictionary<VariantDef, (UnityEngine.Object, SafeAction)>>();

	private static Dictionary<(Type, VariantDef, bool), UnityEngine.Object> _byTypeCache = new Dictionary<(Type, VariantDef, bool), UnityEngine.Object>();

	private static int _nextVariantId = 1;

	private static Dictionary<string, int> _skinNameToVarId = new Dictionary<string, int>();

	public static Transform variantsParent;

	private static Dictionary<UnityEngine.Object, SafeAction> _objectReferenceSetters = new Dictionary<UnityEngine.Object, SafeAction>();

	public static SafeAction onRepairMissingReferences;

	public static int vQualityAdjusted;

	public static int vOtherPlayersTonedDown;

	public static DewResourceDatabase database
	{
		get
		{
			if (!(UnityEngine.Object)(object)_database)
			{
				_database = Resources.Load<DewResourceDatabase>("MainResources");
				if ((bool)(UnityEngine.Object)(object)_database)
				{
					_database.InitForRuntime();
				}
				else
				{
					Debug.LogError("[DewResources] Database not found. Please build one by accessing top toolbar's Build > Resource Database (Full)");
				}
			}
			return _database;
		}
	}

	public static Material transparentMat
	{
		get
		{
			if (_transparentMat == null)
			{
				_transparentMat = Resources.Load<Material>("matTransparent");
			}
			return _transparentMat;
		}
	}

	public static IReadOnlyCollection<string> loadedGuids => _loadedObjects.Keys;

	public static T Convert<T>(UnityEngine.Object obj) where T : UnityEngine.Object
	{
		if (obj is T)
		{
			return (T)obj;
		}
		if (typeof(Component).IsAssignableFrom(typeof(T)) && obj is GameObject gameObject && gameObject.TryGetComponent<T>(out var component))
		{
			return component;
		}
		return null;
	}

	public static UnityEngine.Object Convert(UnityEngine.Object obj, Type type)
	{
		if (type.IsInstanceOfType(obj))
		{
			return obj;
		}
		if (obj is GameObject gameObject && gameObject.TryGetComponent(type, out var component))
		{
			return component;
		}
		return null;
	}

	public static bool TryConvert<T>(UnityEngine.Object obj, out T result) where T : UnityEngine.Object
	{
		result = Convert<T>(obj);
		return result != null;
	}

	public static bool TryConvert(UnityEngine.Object obj, Type type, out UnityEngine.Object result)
	{
		result = Convert(obj, type);
		return result != null;
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void OnInitAddressables()
	{
		UnloadUnused();
	}

	public static UnityEngine.Object Load(string guid, ResourceLoadSettings settings = default(ResourceLoadSettings))
	{
		if (settings.loadLight && database.heavyToLightGuidMap.TryGetValue(guid, out var value))
		{
			guid = value;
		}
		UnityEngine.Object obj = LoadRaw(guid, settings.loadLight);
		if (obj == null)
		{
			return null;
		}
		return GetVariant(guid, obj, settings.varDef);
	}

	private static UnityEngine.Object LoadRaw(string guid, bool loadLight)
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Invalid comparison between Unknown and I4
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Invalid comparison between Unknown and I4
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		if (string.IsNullOrEmpty(guid))
		{
			return null;
		}
		if (!_loadedObjects.TryGetValue(guid, out var value))
		{
			try
			{
				value = Addressables.LoadAssetAsync<UnityEngine.Object>((object)guid);
			}
			catch (Exception exception)
			{
				Debug.LogError("[DewResources] Exception occurred while trying to load " + guid);
				Debug.LogException(exception);
				return null;
			}
			_loadedObjects.Add(guid, value);
		}
		if ((int)value.Status == 1)
		{
			if (value.Result == null)
			{
				Debug.LogError("[DewResources] Loaded " + guid + " is null");
				return null;
			}
			return value.Result;
		}
		if ((int)value.Status == 2)
		{
			return null;
		}
		UnityEngine.Object obj = value.WaitForCompletion();
		if (value.Result == null)
		{
			Debug.LogError("[DewResources] Freshly loaded " + guid + " is null");
			return null;
		}
		if (EnableVerboseLogging)
		{
			if (loadLight)
			{
				Debug.Log("+ [DewResources] Load(Light): " + obj);
			}
			else
			{
				Debug.Log("+ [DewResources] Loaded: " + obj);
			}
		}
		return obj;
	}

	public static uint GetNetworkAssetId(string guid)
	{
		UnityEngine.Object obj = Load(guid);
		GameObject gameObject = obj as GameObject;
		if (gameObject == null && obj is Component component)
		{
			gameObject = component.gameObject;
		}
		if (gameObject != null && gameObject.TryGetComponent<NetworkIdentity>(out var component2))
		{
			return component2.assetId;
		}
		return 0u;
	}

	public static void Preload(string guid)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		if (_loadedObjects.TryGetValue(guid, out var value))
		{
			return;
		}
		value = Addressables.LoadAssetAsync<UnityEngine.Object>((object)guid);
		if (EnableVerboseLogging)
		{
			value.Completed += (AsyncOperationHandle<UnityEngine.Object> o) =>
			{
				if (o.Result != null)
				{
					Debug.Log("+ [DewResources] Preload " + o.Result);
				}
				else
				{
					Debug.Log("+ [DewResources] Preload " + guid);
				}
			};
		}
		_loadedObjects.Add(guid, value);
	}

	public static void WaitForPendingLoads()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Invalid comparison between Unknown and I4
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Invalid comparison between Unknown and I4
		AsyncOperationHandle<UnityEngine.Object>[] array = _loadedObjects.Values.ToArray();
		int num = 0;
		for (int i = 0; i < array.Length; i++)
		{
			AsyncOperationHandle<UnityEngine.Object> val = array[i];
			if (val.IsValid() && (int)val.Status != 1 && (int)val.Status != 2)
			{
				num++;
				try
				{
					val.WaitForCompletion();
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
				}
			}
		}
		if (num > 0)
		{
			Debug.Log($"[DewResources] WaitForPendingLoads: completed {num} in-flight load(s)");
		}
	}

	public static void UnloadUnused()
	{
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			string[] array = _loadedObjects.Keys.ToArray();
			int num = 0;
			int num2 = 0;
			_preloadInterface._guids.Clear();
			for (int num3 = _preloadRules.Count - 1; num3 >= 0; num3--)
			{
				PreloadRuleEntry preloadRuleEntry = _preloadRules[num3];
				if (preloadRuleEntry.owner == null)
				{
					_preloadRules.RemoveAt(num3);
				}
				else
				{
					try
					{
						preloadRuleEntry.onPreload(_preloadInterface);
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
					}
				}
			}
			string[] array2 = array;
			foreach (string text in array2)
			{
				if (_preloadInterface._guids.Contains(text))
				{
					continue;
				}
				num++;
				if (EnableVerboseLogging)
				{
					if (_loadedObjects[text].Result != null)
					{
						Debug.Log("[DewResources] - Unload " + _loadedObjects[text].Result);
					}
					else
					{
						Debug.Log("[DewResources] - Unload " + text);
					}
				}
				Addressables.Release<UnityEngine.Object>(_loadedObjects[text]);
				_loadedObjects.Remove(text);
				if (!_variants.TryGetValue(text, out var value))
				{
					continue;
				}
				foreach (KeyValuePair<VariantDef, (UnityEngine.Object, SafeAction)> item in value)
				{
					item.Value.Item2?.Invoke();
					if (item.Value.Item1 != null)
					{
						UnityEngine.Object.Destroy(item.Value.Item1);
					}
				}
				value.Clear();
				_variants.Remove(text);
				onAssetVariantsDestroyed?.Invoke(text);
			}
			if (num > 0)
			{
				_byTypeCache.Clear();
			}
			foreach (string guid in _preloadInterface._guids)
			{
				if (!array.Contains(guid))
				{
					num2++;
					Preload(guid);
				}
			}
			if (num == 0 && num2 == 0)
			{
				if (array.Length != 0)
				{
					Debug.Log($"[DewResources] Maintenance Finished: {array.Length} (Nothing changed)");
				}
			}
			else
			{
				Debug.Log($"[DewResources] Maintenance Finished: {array.Length} -> {loadedGuids.Count} (-{num}) (+{num2})");
			}
		}
		catch (Exception exception2)
		{
			Debug.LogException(exception2);
		}
	}

	public static string FindRoomNameBySubstring(string name)
	{
		foreach (string sceneName in database.sceneNames)
		{
			if (sceneName.StartsWith("Room_", StringComparison.InvariantCultureIgnoreCase) && sceneName.ToLower().Contains(name.ToLower()))
			{
				return sceneName;
			}
		}
		return null;
	}

	public static IEnumerable<UnityEngine.Object> FindAllByTypeSubstring(string substring, ResourceLoadSettings settings = default(ResourceLoadSettings))
	{
		foreach (KeyValuePair<string, string> item in database.typeNameToGuid)
		{
			if (item.Key.ToLower().Contains(substring.ToLower()))
			{
				UnityEngine.Object byGuid = GetByGuid<UnityEngine.Object>(item.Value, settings);
				if (!(byGuid == null))
				{
					yield return byGuid;
				}
			}
		}
	}

	public static IEnumerable<TFilter> FindAllByTypeSubstring<TFilter>(string substring, ResourceLoadSettings settings = default(ResourceLoadSettings)) where TFilter : UnityEngine.Object
	{
		foreach (KeyValuePair<string, string> item in database.typeNameToGuid)
		{
			if (item.Key.ToLower().Contains(substring.ToLower()) && TryConvert<TFilter>(GetByGuid(item.Value, settings), out var result))
			{
				yield return result;
			}
		}
	}

	public static UnityEngine.Object FindOneByTypeSubstring(string substring, ResourceLoadSettings settings = default(ResourceLoadSettings))
	{
		IEnumerator<UnityEngine.Object> enumerator = FindAllByTypeSubstring(substring, settings).GetEnumerator();
		enumerator.MoveNext();
		UnityEngine.Object current = enumerator.Current;
		enumerator.Dispose();
		return current;
	}

	public static TFilter FindOneByTypeSubstring<TFilter>(string substring, ResourceLoadSettings settings = default(ResourceLoadSettings)) where TFilter : UnityEngine.Object
	{
		IEnumerator<TFilter> enumerator = FindAllByTypeSubstring<TFilter>(substring, settings).GetEnumerator();
		enumerator.MoveNext();
		TFilter current = enumerator.Current;
		enumerator.Dispose();
		return current;
	}

	public static void PreloadAllByNameSubstring(string substring)
	{
		string value = substring.ToLower();
		foreach (KeyValuePair<string, string> item in database.nameToGuid)
		{
			if (item.Key.ToLower().Contains(value))
			{
				Preload(item.Value);
			}
		}
	}

	public static IEnumerable<UnityEngine.Object> FindAllByNameSubstring(string substring, ResourceLoadSettings settings = default(ResourceLoadSettings))
	{
		foreach (KeyValuePair<string, string> item in database.nameToGuid)
		{
			if (item.Key.ToLower().Contains(substring.ToLower()))
			{
				UnityEngine.Object byGuid = GetByGuid(item.Value, settings);
				if (!(byGuid == null))
				{
					yield return byGuid;
				}
			}
		}
	}

	public static IEnumerable<TFilter> FindAllByNameSubstring<TFilter>(string substring, ResourceLoadSettings settings = default(ResourceLoadSettings)) where TFilter : UnityEngine.Object
	{
		foreach (KeyValuePair<string, string> item in database.nameToGuid)
		{
			if (item.Key.ToLower().Contains(substring.ToLower()) && TryConvert<TFilter>(GetByGuid(item.Value, settings), out var result))
			{
				yield return result;
			}
		}
	}

	public static UnityEngine.Object FindOneByIdSubstring(string substring, ResourceLoadSettings settings = default(ResourceLoadSettings))
	{
		IEnumerator<UnityEngine.Object> enumerator = FindAllByNameSubstring(substring, settings).GetEnumerator();
		enumerator.MoveNext();
		UnityEngine.Object current = enumerator.Current;
		enumerator.Dispose();
		return current;
	}

	public static TFilter FindOneByIdSubstring<TFilter>(string substring, ResourceLoadSettings settings = default(ResourceLoadSettings)) where TFilter : UnityEngine.Object
	{
		IEnumerator<TFilter> enumerator = FindAllByNameSubstring<TFilter>(substring, settings).GetEnumerator();
		enumerator.MoveNext();
		TFilter current = enumerator.Current;
		enumerator.Dispose();
		return current;
	}

	public static IEnumerable<T> FindAllByType<T>(ResourceLoadSettings settings = default(ResourceLoadSettings)) where T : UnityEngine.Object
	{
		foreach (KeyValuePair<string, Type> item in database.typeNameToType)
		{
			if (typeof(T).IsAssignableFrom(item.Value) && database.typeToGuid.ContainsKey(item.Value))
			{
				T byType = GetByType<T>(item.Value, settings);
				if (!(byType == null))
				{
					yield return byType;
				}
			}
		}
	}

	public static IEnumerable<UnityEngine.Object> FindAllByType(Type type, ResourceLoadSettings settings = default(ResourceLoadSettings))
	{
		foreach (KeyValuePair<string, Type> item in database.typeNameToType)
		{
			if (type.IsAssignableFrom(item.Value) && database.typeToGuid.ContainsKey(item.Value))
			{
				UnityEngine.Object byType = GetByType(item.Value, settings);
				if (!(byType == null))
				{
					yield return byType;
				}
			}
		}
	}

	public static GameObject GetNetworkedPrefab(uint id, ResourceLoadSettings settings = default(ResourceLoadSettings))
	{
		if (database.netObjectAssetIdToGuid.TryGetValue(id, out var _))
		{
			return (GameObject)Load(database.netObjectAssetIdToGuid[id], settings);
		}
		Debug.LogError(string.Format("[DewResources] {0} failed with id: {1}", "GetNetworkedPrefab", id));
		return null;
	}

	public static T GetByType<T>(ResourceLoadSettings settings = default(ResourceLoadSettings)) where T : UnityEngine.Object
	{
		return (T)GetByType(typeof(T), settings);
	}

	public static UnityEngine.Object GetByType(Type type, ResourceLoadSettings settings = default(ResourceLoadSettings))
	{
		(Type, VariantDef, bool) key = (type, settings.varDef, settings.loadLight);
		if (_byTypeCache.TryGetValue(key, out var value) && value != null)
		{
			return value;
		}
		if (database.typeToGuid.TryGetValue(type, out var value2))
		{
			UnityEngine.Object obj = Convert(Load(value2, settings), type);
			if (obj != null)
			{
				_byTypeCache[key] = obj;
			}
			return obj;
		}
		Debug.LogError("[DewResources] GetByType failed with type: " + type.Name);
		return null;
	}

	public static T GetByType<T>(Type type, ResourceLoadSettings settings = default(ResourceLoadSettings)) where T : UnityEngine.Object
	{
		if (database.typeToGuid.TryGetValue(type, out var value))
		{
			return Convert<T>(Load(value, settings));
		}
		Debug.LogError("[DewResources] GetByType<" + typeof(T).Name + "> failed with type: " + type.Name);
		return null;
	}

	public static UnityEngine.Object GetByShortTypeName(string name, ResourceLoadSettings settings = default(ResourceLoadSettings))
	{
		if (database.typeNameToType.TryGetValue(name, out var value))
		{
			return GetByType(value, settings);
		}
		Debug.LogError("[DewResources] GetByShortTypeName failed with name: " + name);
		return null;
	}

	public static T GetByShortTypeName<T>(string name, ResourceLoadSettings settings = default(ResourceLoadSettings)) where T : UnityEngine.Object
	{
		return (T)GetByShortTypeName(name, settings);
	}

	public static T GetByGuid<T>(string guid, ResourceLoadSettings settings = default(ResourceLoadSettings)) where T : UnityEngine.Object
	{
		return Convert<T>(Load(guid, settings));
	}

	public static UnityEngine.Object GetByGuid(string guid, ResourceLoadSettings settings = default(ResourceLoadSettings))
	{
		return Load(guid, settings);
	}

	public static T GetByName<T>(string name, ResourceLoadSettings settings = default(ResourceLoadSettings)) where T : UnityEngine.Object
	{
		if (database.nameToGuid.TryGetValue(name, out var value))
		{
			return Convert<T>(Load(value, settings));
		}
		Debug.LogError("[DewResources] GetByName<" + typeof(T).Name + "> failed with name: " + name);
		return null;
	}

	public static UnityEngine.Object GetByName(string name, ResourceLoadSettings settings = default(ResourceLoadSettings))
	{
		if (database.nameToGuid.TryGetValue(name, out var value))
		{
			return Load(value, settings);
		}
		Debug.LogError("[DewResources] GetByName failed with name: " + name);
		return null;
	}

	public static string GetGuidOfAsset(UnityEngine.Object obj)
	{
		if (TryGetGuidOfAsset(obj, out var guid))
		{
			return guid;
		}
		Debug.LogError(string.Format("[DewResources] {0} failed with object: {1}", "GetGuidOfAsset", obj));
		return null;
	}

	public static bool TryGetGuidOfAsset(UnityEngine.Object obj, out string guid)
	{
		guid = null;
		if (obj is ILinkedByGuid linkedByGuid)
		{
			guid = linkedByGuid.resourceId;
			return true;
		}
		if (database.objectToGuidFallback.TryGetValue(obj, out guid))
		{
			return true;
		}
		if (database.nameToGuid.TryGetValue(obj.name, out guid))
		{
			return true;
		}
		if (database.typeToGuid.TryGetValue(obj.GetType(), out guid))
		{
			return true;
		}
		return false;
	}

	public static List<string> GetAllDependencies(out ListReturnHandle<string> handle, string objName)
	{
		List<string> list = DewPool.GetList(out handle);
		Add(objName);
		return list;
		void Add(string n)
		{
			if (!database.dependencyConnections.TryGetValue(n, out var value))
			{
				return;
			}
			foreach (string item in value)
			{
				if (!(item == objName) && !list.Contains(item))
				{
					list.Add(item);
					Add(item);
				}
			}
		}
	}

	public static ResourceLinkBy GetLinkType(Type objType)
	{
		if (objType == null)
		{
			Debug.LogWarning("Received null for GetLinkType target");
			return ResourceLinkBy.None;
		}
		if (_linkTypeByType.TryGetValue(objType, out var value))
		{
			return value;
		}
		ResourceLinkBy resourceLinkBy = ResourceLinkBy.None;
		bool flag = false;
		Type type = objType;
		while (type != null)
		{
			object[] customAttributes = type.GetCustomAttributes(typeof(DewResourceLinkAttribute), inherit: false);
			if (customAttributes.Length != 0)
			{
				resourceLinkBy = ((DewResourceLinkAttribute)customAttributes[0]).by;
				flag = true;
				break;
			}
			type = type.BaseType;
		}
		if (!flag)
		{
			foreach (var resourceLinkType in Dew.resourceLinkTypes)
			{
				if (!(objType != resourceLinkType.Item1) || objType.IsSubclassOf(resourceLinkType.Item1))
				{
					resourceLinkBy = resourceLinkType.Item2.by;
					break;
				}
			}
		}
		_linkTypeByType[objType] = resourceLinkBy;
		return resourceLinkBy;
	}

	public static string GetLinkId(UnityEngine.Object obj)
	{
		if (obj == null)
		{
			Debug.LogWarning("Received null for GetLinkId target");
			return null;
		}
		switch (GetLinkType(obj.GetType()))
		{
		case ResourceLinkBy.None:
			Debug.LogWarning("GetLinkId target " + obj.GetType().Name + " has no link type");
			return null;
		case ResourceLinkBy.Type:
			return obj.GetType().Name;
		case ResourceLinkBy.Guid:
		{
			if (obj is ILinkedByGuid linkedByGuid)
			{
				return linkedByGuid.resourceId;
			}
			if (database.objectToGuidFallback.TryGetValue(obj, out var value))
			{
				return value;
			}
			Debug.LogWarning("GetLinkId target " + obj.name + " (" + obj.GetType().Name + ") is guid-linked but there is no way to resolve it");
			return null;
		}
		case ResourceLinkBy.Name:
			return Dew.GetOriginalName(obj.name);
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public static T GetByLinkTypeAndId<T>(ResourceLinkBy type, string id) where T : UnityEngine.Object
	{
		return type switch
		{
			ResourceLinkBy.None => null, 
			ResourceLinkBy.Type => GetByShortTypeName<T>(id), 
			ResourceLinkBy.Guid => GetByGuid<T>(id), 
			ResourceLinkBy.Name => GetByName<T>(id), 
			_ => throw new ArgumentOutOfRangeException("type", type, null), 
		};
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void OnInitPreload()
	{
		_preloadRules.Clear();
	}

	public static void AddPreloadRule(MonoBehaviour owner, Action<PreloadInterface> onPreload)
	{
		if (owner == null)
		{
			return;
		}
		_preloadRules.Add(new PreloadRuleEntry
		{
			owner = owner,
			onPreload = onPreload
		});
		try
		{
			string[] array = _loadedObjects.Keys.ToArray();
			int num = 0;
			_preloadInterface._guids.Clear();
			onPreload(_preloadInterface);
			foreach (string guid in _preloadInterface._guids)
			{
				if (!array.Contains(guid))
				{
					num++;
					Preload(guid);
				}
			}
			if (num == 0)
			{
				if (array.Length != 0)
				{
					Debug.Log($"[DewResources] Preload Rule Added by {owner.name}: {array.Length} (Nothing changed)");
				}
			}
			else
			{
				Debug.Log($"[DewResources] Preload Rule Added by {owner.name}: {array.Length} -> {loadedGuids.Count} (+{num})");
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	public static VariantDef GetSuggestedVarDef(Actor parentActor, Type childType)
	{
		VariantDef data = default;
		if (childType != null && childType.IsSubclassOf(typeof(AbilityInstance)))
		{
			data = data.Add(vQualityAdjusted);
		}
		if ((UnityEngine.Object)(object)parentActor != null)
		{
			parentActor.ProcessSpawnedChildVarDefProcessor(ref data, childType);
		}
		return data;
	}

	public static ResourceLoadSettings GetSuggestedResourceLoadSettings(Actor parentActor, Type childType)
	{
		return new ResourceLoadSettings
		{
			varDef = GetSuggestedVarDef(parentActor, childType)
		};
	}

	public static int GetVariantIdForSkin(string skinName)
	{
		if (_skinNameToVarId.TryGetValue(skinName, out var value))
		{
			return value;
		}
		value = GetNextVariantId();
		_skinNameToVarId[skinName] = value;
		return value;
	}

	public static int GetNextVariantId()
	{
		return _nextVariantId++;
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void OnInit_Variants()
	{
		_nextVariantId = 1;
		_processors = new Dictionary<int, ResourceVariantProcessor>();
		_variants = new Dictionary<string, Dictionary<VariantDef, (UnityEngine.Object, SafeAction)>>();
		_byTypeCache = new Dictionary<(Type, VariantDef, bool), UnityEngine.Object>();
		_skinNameToVarId.Clear();
		Application.quitting += ApplicationOnquitting;
		onVariantsCleared = null;
		onAssetVariantsDestroyed = null;
		onRepairMissingReferences = null;
	}

	public static void RegisterVariantProcessor(int id, ResourceVariantProcessor processor)
	{
		if (_processors.ContainsKey(id))
		{
			Debug.LogWarning("Duplicate variant id");
		}
		else
		{
			_processors.Add(id, processor);
		}
	}

	public static void UnregisterVariantProcessor(int id)
	{
		if (!_processors.Remove(id))
		{
			Debug.Log($"Variant with id {id} not found");
		}
	}

	private static UnityEngine.Object GetVariant(string guid, UnityEngine.Object obj, VariantDef varDef)
	{
		if (obj == null)
		{
			Debug.LogError($"[DewResources] Got null object for getting variant of {guid} {varDef}");
			return null;
		}
		if (varDef == default(VariantDef) && !DoesAnyJsonOverrideApply(guid))
		{
			return obj;
		}
		database.lightToHeavyGuidMap.TryGetValue(guid, out var value);
		if (!_variants.TryGetValue(guid, out var value2))
		{
			value2 = new Dictionary<VariantDef, (UnityEngine.Object, SafeAction)>();
			_variants[guid] = value2;
		}
		bool flag = false;
		if (!value2.TryGetValue(varDef, out var value3))
		{
			flag = true;
		}
		else if (value3.Item1 == null)
		{
			Debug.LogWarning($"[DewResources] {varDef} of {obj} got destroyed, which shouldn't happen. Calling cleanup and recreating...");
			try
			{
				value3.Item2?.Invoke();
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
			flag = true;
		}
		if (flag)
		{
			if (obj is GameObject original)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(original, GetVariantsParent());
				gameObject.name = obj.name;
				SafeAction item = null;
				item += Process(varDef.id0, gameObject);
				item += Process(varDef.id1, gameObject);
				item += Process(varDef.id2, gameObject);
				item += Process(varDef.id3, gameObject);
				bool flag2 = NetworkServer.active || (!NetworkServer.active && !NetworkClient.active && !NetworkClient.isConnecting);
				if (flag2 && DewMod.currentJsonOverrideProcessorsLocal.TryGetValue(guid, out var value4))
				{
					value4.Invoke(gameObject);
				}
				if (DewMod.currentJsonOverrideProcessorsFromServer.TryGetValue(guid, out var value5))
				{
					value5.Invoke(gameObject);
				}
				if (!string.IsNullOrEmpty(value))
				{
					if (flag2 && DewMod.currentJsonOverrideProcessorsLocal.TryGetValue(value, out var value6))
					{
						value6.Invoke(gameObject);
					}
					if (DewMod.currentJsonOverrideProcessorsFromServer.TryGetValue(value, out var value7))
					{
						value7.Invoke(gameObject);
					}
				}
				value3 = (gameObject, item);
			}
			else
			{
				UnityEngine.Object obj2 = UnityEngine.Object.Instantiate(obj);
				obj2.name = obj.name;
				UnityEngine.Object.DontDestroyOnLoad(obj2);
				SafeAction item2 = null;
				item2 += Process(varDef.id0, obj2);
				item2 += Process(varDef.id1, obj2);
				item2 += Process(varDef.id2, obj2);
				item2 += Process(varDef.id3, obj2);
				value3 = (obj2, item2);
			}
			if (value3.Item1 == null)
			{
				Debug.LogError($"[DewResources] {varDef} of {obj} got destroyed while being processed");
			}
			value2[varDef] = value3;
		}
		return value3.Item1;
		static Action Process(int id, UnityEngine.Object target)
		{
			if (id <= 0)
			{
				return null;
			}
			try
			{
				return _processors[id](target);
			}
			catch (Exception exception2)
			{
				Debug.LogException(exception2);
			}
			return null;
		}
	}

	private static bool DoesAnyJsonOverrideApply(string guid)
	{
		if (string.IsNullOrEmpty(guid))
		{
			return false;
		}
		bool shouldUseLocal = NetworkServer.active || (!NetworkServer.active && !NetworkClient.active && !NetworkClient.isConnecting);
		if (HasJsonOverrideFor(guid, shouldUseLocal))
		{
			return true;
		}
		if (database.lightToHeavyGuidMap.TryGetValue(guid, out var value) && HasJsonOverrideFor(value, shouldUseLocal))
		{
			return true;
		}
		return false;
	}

	private static bool HasJsonOverrideFor(string guid, bool shouldUseLocal)
	{
		if (string.IsNullOrEmpty(guid))
		{
			return false;
		}
		if (shouldUseLocal && DewMod.currentJsonOverrideProcessorsLocal.ContainsKey(guid))
		{
			return true;
		}
		return DewMod.currentJsonOverrideProcessorsFromServer.ContainsKey(guid);
	}

	private static Transform GetVariantsParent()
	{
		if (variantsParent == null)
		{
			GameObject gameObject = new GameObject("Resource Variants");
			UnityEngine.Object.DontDestroyOnLoad(gameObject);
			gameObject.SetActive(value: false);
			variantsParent = gameObject.transform;
		}
		return variantsParent;
	}

	public static int ClearVariantsOfAsset(string guid, VariantDef? target, bool repairReferences)
	{
		_byTypeCache.Clear();
		if (database.heavyToLightGuidMap.TryGetValue(guid, out var value) && guid != value)
		{
			ClearVariantsOfAsset(value, target, repairReferences);
		}
		int num = 0;
		if (string.IsNullOrEmpty(guid) || !_variants.TryGetValue(guid, out var value2))
		{
			return num;
		}
		if (repairReferences)
		{
			RepairMissingReferences_Prepare();
		}
		(UnityEngine.Object, SafeAction) value3;
		if (!target.HasValue)
		{
			foreach (KeyValuePair<VariantDef, (UnityEngine.Object, SafeAction)> item in value2)
			{
				num++;
				item.Value.Item2?.Invoke();
				if (item.Value.Item1 != null)
				{
					UnityEngine.Object.DestroyImmediate(item.Value.Item1);
				}
			}
			value2.Clear();
			_variants.Remove(guid);
		}
		else if (value2.TryGetValue(target.Value, out value3))
		{
			num++;
			value3.Item2?.Invoke();
			if (value3.Item1 != null)
			{
				UnityEngine.Object.DestroyImmediate(value3.Item1);
			}
			value2.Remove(target.Value);
		}
		if (repairReferences)
		{
			RepairMissingReferences_Repair();
		}
		return num;
	}

	public static int ClearVariantsOfVarDef(VariantDef target, bool repairReferences)
	{
		if (repairReferences)
		{
			RepairMissingReferences_Prepare();
		}
		int num = 0;
		ListReturnHandle<string> handle;
		foreach (string item in _variants.Keys.ToListNonAlloc(out handle))
		{
			num += ClearVariantsOfAsset(item, target, repairReferences: false);
		}
		onVariantsCleared.Invoke();
		handle.Return();
		if (repairReferences)
		{
			RepairMissingReferences_Repair();
		}
		return num;
	}

	public static int ClearAllVariants(bool repairReferences)
	{
		if (repairReferences)
		{
			RepairMissingReferences_Prepare();
		}
		int num = 0;
		ListReturnHandle<string> handle;
		foreach (string item in _variants.Keys.ToListNonAlloc(out handle))
		{
			num += ClearVariantsOfAsset(item, null, repairReferences: false);
		}
		onVariantsCleared.Invoke();
		handle.Return();
		if (repairReferences)
		{
			RepairMissingReferences_Repair();
		}
		return num;
	}

	public static void RepairMissingReferences_Prepare()
	{
	}

	public static void RepairMissingReferences_Repair()
	{
	}

	private static void ApplicationOnquitting()
	{
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
	private static void OnInit_vQualityAdjusted()
	{
		vQualityAdjusted = GetNextVariantId();
		RegisterVariantProcessor(vQualityAdjusted, QualityAdjustedProcessor);
	}

	private static Action QualityAdjustedProcessor(UnityEngine.Object o)
	{
		if (!(o is GameObject gameObject))
		{
			return null;
		}
		gameObject.name += "(Adjusted)";
		Quality3Levels quality = ((ManagerBase<GraphicsManager>.instance != null) ? ManagerBase<GraphicsManager>.instance.currentEffectQuality : Quality3Levels.High);
		List<GameObject> list = new List<GameObject>();
		DewEffect.BakeQualityScaling(gameObject, quality, buildPlan: false, applyScaling: true, list);
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i] != null)
			{
				UnityEngine.Object.DestroyImmediate(list[i]);
			}
		}
		Transform transform = gameObject.transform;
		for (int j = 0; j < transform.childCount; j++)
		{
			Transform child = transform.GetChild(j);
			if (!(child.GetComponentInChildren<DewCollider>(includeInactive: true) != null))
			{
				DewEffect.BakeQualityScaling(child.gameObject, quality, buildPlan: true, applyScaling: false);
			}
		}
		FxCameraShake[] componentsInChildren = gameObject.GetComponentsInChildren<FxCameraShake>(includeInactive: true);
		for (int k = 0; k < componentsInChildren.Length; k++)
		{
			componentsInChildren[k].CreateImpulseSource();
		}
		return null;
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
	private static void OnInit_vTonedDown()
	{
		vOtherPlayersTonedDown = GetNextVariantId();
		RegisterVariantProcessor(vOtherPlayersTonedDown, TonedDownProcessor);
		onVariantsCleared += (Action)(() =>
		{
			Actor[] array = UnityEngine.Object.FindObjectsOfType<Actor>(false);
			foreach (Actor actor in array)
			{
				if (((Component)(object)actor).gameObject.name.Contains("(Other Players Toned Down)"))
				{
					ListReturnHandle<Renderer> handle;
					foreach (Renderer item in ((Component)(object)actor).GetComponentsInChildrenNonAlloc(true, out handle))
					{
						Material[] sharedMaterials = item.sharedMaterials;
						for (int j = 0; j < sharedMaterials.Length; j++)
						{
							if (sharedMaterials[j] == null)
							{
								sharedMaterials[j] = transparentMat;
							}
						}
						item.sharedMaterials = sharedMaterials;
					}
					handle.Return();
				}
			}
		});
	}

	private static Action TonedDownProcessor(UnityEngine.Object o)
	{
		//IL_0449: Unknown result type (might be due to invalid IL or missing references)
		//IL_044e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0452: Unknown result type (might be due to invalid IL or missing references)
		//IL_0468: Unknown result type (might be due to invalid IL or missing references)
		//IL_046d: Unknown result type (might be due to invalid IL or missing references)
		//IL_047e: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_052f: Unknown result type (might be due to invalid IL or missing references)
		if (!(o is GameObject gameObject))
		{
			return null;
		}
		if (gameObject.GetComponent<IOtherPlayersTonedDownDisable>() != null)
		{
			return null;
		}
		IOtherPlayersTonedDownLimit component = gameObject.GetComponent<IOtherPlayersTonedDownLimit>();
		gameObject.name += "(Other Players Toned Down)";
		List<Material> newMats = DewPool.GetList(out ListReturnHandle<Material> handle);
		ReduceOtherPlayerEffectsStrength reduceOtherPlayerEffectsStrength = DewSave.profileMain.gameplay.reduceOtherPlayerEffectsStrength;
		if (component != null && reduceOtherPlayerEffectsStrength > component.maxReduction)
		{
			reduceOtherPlayerEffectsStrength = component.maxReduction;
		}
		float alphaMult;
		float num;
		float num2;
		switch (reduceOtherPlayerEffectsStrength)
		{
		case ReduceOtherPlayerEffectsStrength.Low:
			alphaMult = 1f;
			num = 1f;
			num2 = 1f;
			break;
		case ReduceOtherPlayerEffectsStrength.Medium:
			alphaMult = 0.7f;
			num = 0.6f;
			num2 = 1f;
			break;
		case ReduceOtherPlayerEffectsStrength.High:
			alphaMult = 0.45f;
			num = 0.45f;
			num2 = 0.8f;
			break;
		case ReduceOtherPlayerEffectsStrength.VeryHigh:
			alphaMult = 0.25f;
			num = 0.3f;
			num2 = 0.6f;
			break;
		case ReduceOtherPlayerEffectsStrength.Hide:
			alphaMult = 0f;
			num = 0f;
			num2 = 0f;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		ListReturnHandle<Renderer> handle2;
		foreach (Renderer item in gameObject.GetComponentsInChildrenNonAlloc(true, out handle2))
		{
			if (reduceOtherPlayerEffectsStrength == ReduceOtherPlayerEffectsStrength.Hide)
			{
				item.enabled = false;
			}
			Material[] mats = item.sharedMaterials;
			int i;
			for (i = 0; i < mats.Length; i++)
			{
				if (!(mats[i] == null))
				{
					mats[i] = UnityEngine.Object.Instantiate(mats[i]);
					newMats.Add(mats[i]);
					DoFloat("_Cutoff");
					if (mats[i].HasProperty("_Surface") && Mathf.Abs(mats[i].GetFloat("_Surface")) < 0.1f)
					{
						DoColor("_EmissionColor");
					}
					else if (!DoFloat("_Alpha") && !DoFloat("Vector1_2C5A3101") && !DoFloat("Vector1_ba2f839299ad461eb6b76fbb90d387aa") && !DoFloat("_Opacity") && !DoFloat("_Opacity") && !DoColorAlpha("_BaseColor") && !DoColorAlpha("_Color") && !DoFloat("_FinalOpacityPower") && !DoFloat("_ColorFactor"))
					{
						DoFloat("_Multiplier");
					}
				}
			}
			item.sharedMaterials = mats;
			bool DoColor(string prop)
			{
				if (mats[i].HasProperty(prop))
				{
					Color color = mats[i].GetColor(prop);
					float a = color.a;
					color *= alphaMult;
					color.a = a;
					mats[i].SetColor(prop, color);
					return true;
				}
				return false;
			}
			bool DoColorAlpha(string prop)
			{
				if (mats[i].HasProperty(prop))
				{
					Color color = mats[i].GetColor(prop);
					color.a *= alphaMult;
					mats[i].SetColor(prop, color);
					return true;
				}
				return false;
			}
			bool DoFloat(string prop)
			{
				if (mats[i].HasProperty(prop))
				{
					mats[i].SetFloat(prop, mats[i].GetFloat(prop) * alphaMult);
					return true;
				}
				return false;
			}
		}
		handle2.Return();
		ListReturnHandle<Light> handle3;
		foreach (Light item2 in gameObject.GetComponentsInChildrenNonAlloc(true, out handle3))
		{
			item2.intensity *= num;
			if (reduceOtherPlayerEffectsStrength == ReduceOtherPlayerEffectsStrength.Hide)
			{
				item2.range = 0f;
			}
		}
		handle3.Return();
		if (reduceOtherPlayerEffectsStrength == ReduceOtherPlayerEffectsStrength.Hide)
		{
			ListReturnHandle<FxPointLight> handle4;
			foreach (FxPointLight item3 in gameObject.GetComponentsInChildrenNonAlloc(true, out handle4))
			{
				item3.intensityMultiplier = 0f;
				item3.rangeMultiplier = 0f;
			}
			handle4.Return();
		}
		ListReturnHandle<FxEntityColor> handle5;
		foreach (FxEntityColor item4 in gameObject.GetComponentsInChildrenNonAlloc(true, out handle5))
		{
			item4.emission *= alphaMult;
		}
		handle5.Return();
		if (num2 < 0.99f)
		{
			ListReturnHandle<ParticleSystem> handle6;
			foreach (ParticleSystem item5 in gameObject.GetComponentsInChildrenNonAlloc(true, out handle6))
			{
				MainModule main = item5.main;
				float num3 = GetAvg(main.startLifetime);
				float b = 2f / num3;
				EmissionModule emission = item5.emission;
				if (reduceOtherPlayerEffectsStrength == ReduceOtherPlayerEffectsStrength.Hide)
				{
					emission.enabled = false;
				}
				float num4 = GetAvg(emission.rateOverTime);
				float num5 = Mathf.Max(num4 * num2, b);
				if (num4 > num5)
				{
					emission.rateOverTimeMultiplier *= num5 / num4;
				}
				int bursts = emission.GetBursts(_burstsBuffer);
				for (int j = 0; j < bursts; j++)
				{
					MinMaxCurve count = _burstsBuffer[j].count;
					count.curveMultiplier *= num2;
					if (count.constantMin > 5f)
					{
						count.constantMin *= num2;
					}
					if (count.constantMax > 5f)
					{
						count.constantMax *= num2;
					}
					_burstsBuffer[j].count = count;
				}
				emission.SetBursts(_burstsBuffer, bursts);
			}
			handle6.Return();
		}
		return () =>
		{
			foreach (Material item6 in newMats)
			{
				if (!(item6 == null))
				{
					UnityEngine.Object.DestroyImmediate(item6);
				}
			}
			handle.Return();
		};
		static float GetAvg(MinMaxCurve curve)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Expected I4, but got Unknown
			ParticleSystemCurveMode mode = curve.mode;
			return (int)mode switch
			{
				0 => curve.constant, 
				1 => curve.curveMultiplier, 
				2 => curve.curveMultiplier, 
				3 => (curve.constantMin + curve.constantMax) * 0.5f, 
				_ => throw new ArgumentOutOfRangeException(), 
			};
		}
	}
}
