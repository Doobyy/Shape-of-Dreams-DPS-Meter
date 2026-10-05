using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Mirror;
using Newtonsoft.Json;
using UnityEngine;

public static class DewPersistence
{
	public class NetworkBehaviourConverter : JsonConverter
	{
		public readonly Dictionary<uint, NetworkIdentity> prevNetIdMap = new Dictionary<uint, NetworkIdentity>();

		public override bool CanConvert(Type objectType)
		{
			return objectType.IsSubclassOf(typeof(NetworkBehaviour));
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Invalid comparison between Unknown and I4
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Invalid comparison between Unknown and I4
			//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
			if ((int)reader.TokenType == 11)
			{
				return null;
			}
			if ((int)reader.TokenType == 9)
			{
				string[] array = reader.Value.ToString().Split("|", StringSplitOptions.None);
				uint num = uint.Parse(array[0]);
				string arg = array[1];
				if (prevNetIdMap.Count > 0)
				{
					if (prevNetIdMap.TryGetValue(num, out var value))
					{
						return ((Component)(object)value).GetComponent(objectType);
					}
				}
				else
				{
					if (NetworkServer.active && NetworkServer.spawned.TryGetValue(num, out var value2))
					{
						return ((Component)(object)value2).GetComponent(objectType);
					}
					if (NetworkClient.spawned.TryGetValue(num, out var value3))
					{
						return ((Component)(object)value3).GetComponent(objectType);
					}
				}
				Debug.LogWarning($"Could not resolve previous netId {num} of type {arg}");
				return null;
			}
			throw new JsonSerializationException($"Unexpected token {reader.TokenType} when parsing {objectType.Name}.");
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			NetworkBehaviour val = (NetworkBehaviour)((value is NetworkBehaviour) ? value : null);
			if (val == null || (UnityEngine.Object)(object)val == null || (UnityEngine.Object)(object)val.netIdentity == null || !val.isClient || val is Actor { isActive: false })
			{
				writer.WriteNull();
			}
			else
			{
				writer.WriteValue($"{val.netId}|{((object)val).GetType().Name}");
			}
		}
	}

	public class AssetConverter : JsonConverter
	{
		private static readonly Type[] SupportedTypes = new Type[1] { typeof(Zone) };

		public override bool CanConvert(Type objectType)
		{
			for (int i = 0; i < SupportedTypes.Length; i++)
			{
				if (SupportedTypes[i] == objectType)
				{
					return true;
				}
			}
			return objectType.IsSubclassOf(typeof(ScriptableObject));
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Invalid comparison between Unknown and I4
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Invalid comparison between Unknown and I4
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			if ((int)reader.TokenType == 11)
			{
				return null;
			}
			if ((int)reader.TokenType == 9)
			{
				return DewResources.Convert(DewResources.GetByGuid(reader.Value.ToString()), objectType);
			}
			throw new JsonSerializationException($"Unexpected token {reader.TokenType} when parsing {objectType.Name}.");
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			if (!(value is UnityEngine.Object obj) || obj == null)
			{
				writer.WriteNull();
				return;
			}
			string guidOfAsset = DewResources.GetGuidOfAsset(obj);
			if (string.IsNullOrEmpty(guidOfAsset))
			{
				writer.WriteNull();
			}
			else
			{
				writer.WriteValue(guidOfAsset);
			}
		}
	}

	public class GameData
	{
		public const int CurrentSaveVersion = 1;

		public int saveVersion;

		public bool shouldShowVersionWarning;

		public List<PlayerData> players = new List<PlayerData>();

		public Dictionary<string, Dictionary<string, string>> managerPersistentData = new Dictionary<string, Dictionary<string, string>>();

		public List<GeneralData> actors = new List<GeneralData>();

		public Dictionary<string, string> serverActorData = new Dictionary<string, string>();

		public LoadNodeSettings loadNodeSettings;

		public bool isMultiplayer;

		public DewLobbyType lobbyType;

		public bool Validate()
		{
			if (players.Count == 0)
			{
				return false;
			}
			if (loadNodeSettings == null)
			{
				return false;
			}
			if (serverActorData == null)
			{
				serverActorData = new Dictionary<string, string>();
			}
			if (actors.Find((GeneralData a) => a.resId == "GameMod_MirageSkin") == null)
			{
				actors.Add(new GeneralData
				{
					resType = ResourceLinkBy.Type,
					resId = "GameMod_MirageSkin",
					persistenceData = new Dictionary<string, string>(),
					prevNetId = 9999u
				});
			}
			if (saveVersion == 0)
			{
				shouldShowVersionWarning = true;
				saveVersion = 1;
			}
			return true;
		}

		public int GetElapsedGameTimeSeconds()
		{
			try
			{
				string s = managerPersistentData["PlayGameManager"]["PlayGameManager::elapsedGameTime"];
				if (!float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
				{
					float.TryParse(s, NumberStyles.Float, CultureInfo.CurrentCulture, out result);
				}
				return (int)result;
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
				return 0;
			}
		}

		public string GetReadableDescription()
		{
			try
			{
				PlayerData playerData = players.Find((PlayerData p) => p.playerGuid == DewSave.profileMain.guid);
				if (playerData == null)
				{
					playerData = players[0];
				}
				int elapsedGameTimeSeconds = GetElapsedGameTimeSeconds();
				int num = elapsedGameTimeSeconds / 60;
				int num2 = elapsedGameTimeSeconds % 60;
				int.Parse(managerPersistentData["ZoneManager"]["ZoneManager::currentZoneIndex"]);
				DewDifficultySettings dewDifficultySettings = FromJson<DewDifficultySettings>(managerPersistentData["PlayGameManager"]["PlayGameManager::difficulty"]);
				Zone zone = ((!(loadNodeSettings.newZone != null)) ? FromJson<Zone>(managerPersistentData["ZoneManager"]["ZoneManager::currentZone"]) : loadNodeSettings.newZone);
				DewLocalization.GetUIValue(zone.name + "_Name");
				DewLocalization.GetUIValue("Difficulty_" + dewDifficultySettings.name + "_Name");
				return string.Format("{0} {1:00}:{2:00}", DewLocalization.GetUIValue(playerData.heroType + "_Name"), num, num2);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
				return "???";
			}
		}

		public string GetReadableGameType()
		{
			if (isMultiplayer)
			{
				return DewLocalization.GetUIValue($"Lobby_Type_{lobbyType}");
			}
			return DewLocalization.GetUIValue("Title_DreamAlone");
		}
	}

	public class AchievementsData
	{
		public string runId;

		public Dictionary<string, string> persistenceData = new Dictionary<string, string>();
	}

	public class PlayerData
	{
		public uint playerPrevNetId;

		public uint heroPrevNetId;

		public string playerGuid;

		public string playerName;

		public string heroType;

		public bool isHeroKnockedOut;

		public Dictionary<string, string> playerPersistenceData;

		public Dictionary<string, string> heroPersistenceData;

		public List<SkillData> skills = new List<SkillData>();

		public List<GemData> gems = new List<GemData>();

		public List<GeneralData> statusEffects = new List<GeneralData>();
	}

	public class SkillData : GeneralData
	{
		public HeroSkillLocation location;

		public override GeneralData Capture(Actor target)
		{
			base.Capture(target);
			location = ((SkillTrigger)target).skillType;
			return this;
		}
	}

	public class GemData : GeneralData
	{
		public GemLocation location;

		public override GeneralData Capture(Actor target)
		{
			base.Capture(target);
			location = ((Gem)target).location;
			return this;
		}
	}

	public class StatusEffectData : GeneralData
	{
	}

	public class GeneralData
	{
		public ResourceLinkBy resType;

		public string resId;

		public uint prevNetId;

		public Dictionary<string, string> persistenceData;

		public virtual GeneralData Capture(Actor target)
		{
			resType = DewResources.GetLinkType(((object)target).GetType());
			resId = DewResources.GetLinkId((UnityEngine.Object)(object)target);
			prevNetId = ((NetworkBehaviour)target).netId;
			persistenceData = SerializeGameObject(((Component)(object)target).gameObject);
			return this;
		}

		public virtual T GetPrefab<T>() where T : UnityEngine.Object
		{
			return DewResources.GetByLinkTypeAndId<T>(resType, resId);
		}
	}

	public class RoomData
	{
		public List<RoomActorData> actors;

		public List<RoomSavedObjectData> savedObjects;

		public Dictionary<string, string> persistenceData;
	}

	public class RoomSavedObjectData
	{
		public string guid;

		public string scenePath;

		public Dictionary<string, string> persistenceData;

		public RoomSavedObjectData Capture(RoomSavedObject target)
		{
			guid = target.guid;
			scenePath = target.transform.GetScenePath();
			persistenceData = SerializeGameObject(target.gameObject);
			return this;
		}
	}

	public class RoomActorData : GeneralData
	{
		public ulong sceneId;

		public override GeneralData Capture(Actor target)
		{
			base.Capture(target);
			sceneId = ((NetworkBehaviour)target).netIdentity.sceneId;
			return this;
		}
	}

	private static readonly JsonSerializerSettings _defaultSettings = GetNewSettings();

	private static Dictionary<Type, Dictionary<object, Attribute>> _attributesCache = new Dictionary<Type, Dictionary<object, Attribute>>();

	private static Dictionary<Type, (FieldInfo[], PropertyInfo[], MethodInfo[])> _relevantMembers = new Dictionary<Type, (FieldInfo[], PropertyInfo[], MethodInfo[])>();

	private static readonly HeroSkillLocation[] _savedSkills = new HeroSkillLocation[6]
	{
		HeroSkillLocation.Q,
		HeroSkillLocation.W,
		HeroSkillLocation.E,
		HeroSkillLocation.R,
		HeroSkillLocation.Identity,
		HeroSkillLocation.Movement
	};

	public static Dictionary<string, string> SerializeGameObject(GameObject gobj)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		ListReturnHandle<Component> handle;
		foreach (Component item in gobj.GetComponentsNonAlloc(out handle))
		{
			if (!(item == null))
			{
				Serialize(item, dictionary, item.GetType().Name + "::");
			}
		}
		handle.Return();
		return dictionary;
	}

	private static void Serialize(object obj, Dictionary<string, string> dict, string keyPrefix = null)
	{
		(FieldInfo[], PropertyInfo[], MethodInfo[]) relevantMembers = GetRelevantMembers(obj.GetType());
		FieldInfo[] item = relevantMembers.Item1;
		foreach (FieldInfo fieldInfo in item)
		{
			try
			{
				dict[keyPrefix + fieldInfo.Name] = ToJson(fieldInfo.GetValue(obj));
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
		PropertyInfo[] item2 = relevantMembers.Item2;
		foreach (PropertyInfo propertyInfo in item2)
		{
			try
			{
				dict[keyPrefix + propertyInfo.Name] = ToJson(propertyInfo.GetValue(obj));
			}
			catch (Exception exception2)
			{
				Debug.LogException(exception2);
			}
		}
	}

	public static void DeserializeOnGameObject(GameObject gobj, Dictionary<string, string> data, JsonSerializerSettings settings, SaveVarFlags required = SaveVarFlags.Default, SaveVarFlags excluded = SaveVarFlags.Default)
	{
		if (data == null || gobj == null)
		{
			return;
		}
		ListReturnHandle<Component> handle;
		foreach (Component item in gobj.GetComponentsNonAlloc(out handle))
		{
			if (!(item == null))
			{
				DeserializeOn(item, data, settings, item.GetType().Name + "::", required, excluded);
			}
		}
		handle.Return();
	}

	private static bool ShouldReuseList(Type fieldType)
	{
		if (!fieldType.IsGenericType)
		{
			return false;
		}
		return fieldType.GetGenericTypeDefinition() == typeof(SyncList<>);
	}

	private static bool ShouldReuseDictionary(Type fieldType)
	{
		if (!fieldType.IsGenericType)
		{
			return false;
		}
		return fieldType.GetGenericTypeDefinition() == typeof(SyncDictionary<, >);
	}

	private static void DeserializeOn(object obj, Dictionary<string, string> data, JsonSerializerSettings settings, string keyPrefix = null, SaveVarFlags required = SaveVarFlags.Default, SaveVarFlags excluded = SaveVarFlags.Default)
	{
		if (obj == null)
		{
			return;
		}
		(FieldInfo[], PropertyInfo[], MethodInfo[]) relevantMembers = GetRelevantMembers(obj.GetType());
		SaveVarAttribute attribute = GetAttribute<SaveVarAttribute>(obj.GetType());
		FieldInfo[] item = relevantMembers.Item1;
		foreach (FieldInfo fieldInfo in item)
		{
			try
			{
				SaveVarAttribute attribute2 = GetAttribute(fieldInfo, attribute);
				if ((required != SaveVarFlags.Default && !attribute2.flags.HasFlag(required)) || (excluded & attribute2.flags) != 0 || !data.TryGetValue(keyPrefix + fieldInfo.Name, out var value))
				{
					continue;
				}
				object obj2 = FromJson(value, fieldInfo.FieldType, settings);
				if (ShouldReuseList(fieldInfo.FieldType))
				{
					object value2 = fieldInfo.GetValue(obj);
					_ = fieldInfo.FieldType.GetInterfaces().First((Type type4) => type4.IsGenericType && type4.GetGenericTypeDefinition() == typeof(IList<>)).GetGenericArguments()[0];
					fieldInfo.FieldType.GetMethod("Clear").Invoke(value2, null);
					MethodInfo method = fieldInfo.FieldType.GetMethod("Add");
					foreach (object item4 in (IEnumerable)obj2)
					{
						method.Invoke(value2, new object[1] { item4 });
					}
				}
				else if (ShouldReuseDictionary(fieldInfo.FieldType))
				{
					object value3 = fieldInfo.GetValue(obj);
					Type type = fieldInfo.FieldType.GetInterfaces().First((Type type4) => type4.IsGenericType && type4.GetGenericTypeDefinition() == typeof(IDictionary<, >));
					Type type2 = type.GetGenericArguments()[0];
					Type type3 = type.GetGenericArguments()[1];
					fieldInfo.FieldType.GetMethod("Clear").Invoke(value3, null);
					MethodInfo method2 = fieldInfo.FieldType.GetMethod("Add", new Type[2] { type2, type3 });
					foreach (DictionaryEntry item5 in (IDictionary)obj2)
					{
						method2.Invoke(value3, new object[2] { item5.Key, item5.Value });
					}
				}
				else
				{
					fieldInfo.SetValue(obj, obj2);
				}
			}
			catch (Exception exception)
			{
				Debug.LogWarning($"Exception ocurred while deserializing on {fieldInfo.FieldType.Name} {obj}::{fieldInfo.Name}");
				Debug.LogException(exception);
			}
		}
		PropertyInfo[] item2 = relevantMembers.Item2;
		foreach (PropertyInfo propertyInfo in item2)
		{
			try
			{
				SaveVarAttribute attribute3 = GetAttribute(propertyInfo, attribute);
				if ((required == SaveVarFlags.Default || attribute3.flags.HasFlag(required)) && (excluded & attribute3.flags) == 0 && data.TryGetValue(keyPrefix + propertyInfo.Name, out var value4))
				{
					object value5 = FromJson(value4, propertyInfo.PropertyType, settings);
					propertyInfo.SetValue(obj, value5);
				}
			}
			catch (Exception exception2)
			{
				Debug.LogException(exception2);
			}
		}
		MethodInfo[] item3 = relevantMembers.Item3;
		foreach (MethodInfo methodInfo in item3)
		{
			try
			{
				LoadCallbackAttribute attribute4 = GetAttribute<LoadCallbackAttribute>(methodInfo, null);
				if (attribute4 != null && (required == SaveVarFlags.Default || attribute4.flags.HasFlag(required)) && (excluded & attribute4.flags) == 0)
				{
					methodInfo.Invoke(obj, null);
				}
			}
			catch (Exception exception3)
			{
				Debug.LogException(exception3);
			}
		}
	}

	public static string ToJson(object obj, JsonSerializerSettings settings = null)
	{
		if (settings == null)
		{
			settings = _defaultSettings;
		}
		return JsonConvert.SerializeObject(obj, settings);
	}

	public static object FromJson(string json, Type type, JsonSerializerSettings settings = null)
	{
		if (settings == null)
		{
			settings = _defaultSettings;
		}
		return JsonConvert.DeserializeObject(json, type, settings);
	}

	public static T FromJson<T>(string json, JsonSerializerSettings settings = null)
	{
		if (settings == null)
		{
			settings = _defaultSettings;
		}
		return JsonConvert.DeserializeObject<T>(json, settings);
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void OnInit()
	{
		_attributesCache = new Dictionary<Type, Dictionary<object, Attribute>>();
		_relevantMembers = new Dictionary<Type, (FieldInfo[], PropertyInfo[], MethodInfo[])>();
	}

	private static T GetAttribute<T>(Type from) where T : class
	{
		if (!_attributesCache.TryGetValue(typeof(T), out var value))
		{
			value = new Dictionary<object, Attribute>();
			_attributesCache[typeof(T)] = value;
		}
		if (!value.TryGetValue(from, out var value2))
		{
			value2 = Attribute.GetCustomAttribute(from, typeof(T));
		}
		return value2 as T;
	}

	private static T GetAttribute<T>(MemberInfo from, T defaultAttr) where T : class
	{
		if (!_attributesCache.TryGetValue(typeof(T), out var value))
		{
			value = new Dictionary<object, Attribute>();
			_attributesCache[typeof(T)] = value;
		}
		if (!value.TryGetValue(from, out var value2))
		{
			value2 = Attribute.GetCustomAttribute(from, typeof(T));
		}
		return (value2 as T) ?? defaultAttr;
	}

	private static (FieldInfo[], PropertyInfo[], MethodInfo[]) GetRelevantMembers(Type type)
	{
		if (!_relevantMembers.ContainsKey(type))
		{
			SaveVarAttribute attribute = GetAttribute<SaveVarAttribute>(type);
			List<FieldInfo> list = new List<FieldInfo>();
			List<PropertyInfo> list2 = new List<PropertyInfo>();
			List<MethodInfo> list3 = new List<MethodInfo>();
			foreach (FieldInfo allField in GetAllFields(type))
			{
				if (GetAttribute(allField, attribute) != null)
				{
					list.Add(allField);
				}
			}
			foreach (PropertyInfo allProperty in GetAllProperties(type))
			{
				if (GetAttribute(allProperty, attribute) != null)
				{
					list2.Add(allProperty);
				}
			}
			foreach (MethodInfo allMethod in GetAllMethods(type))
			{
				if (GetAttribute<LoadCallbackAttribute>(allMethod, null) != null)
				{
					list3.Add(allMethod);
				}
			}
			_relevantMembers[type] = (list.ToArray(), list2.ToArray(), list3.ToArray());
		}
		return _relevantMembers[type];
	}

	private static List<FieldInfo> GetAllFields(Type type)
	{
		List<FieldInfo> list = new List<FieldInfo>();
		while (type != null && type != typeof(object))
		{
			FieldInfo[] fields = type.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			foreach (FieldInfo item in fields)
			{
				list.Add(item);
			}
			type = type.BaseType;
		}
		return list;
	}

	private static List<PropertyInfo> GetAllProperties(Type type)
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		while (type != null && type != typeof(object))
		{
			PropertyInfo[] properties = type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			foreach (PropertyInfo item in properties)
			{
				list.Add(item);
			}
			type = type.BaseType;
		}
		return list;
	}

	private static List<MethodInfo> GetAllMethods(Type type)
	{
		List<MethodInfo> list = new List<MethodInfo>();
		list.AddRange(type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));
		Type baseType = type.BaseType;
		while (baseType != null && baseType != typeof(object))
		{
			list.AddRange(baseType.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));
			baseType = baseType.BaseType;
		}
		return list;
	}

	public static AchievementsData SerializeAchievementData()
	{
		AchievementsData achievementsData = new AchievementsData
		{
			runId = NetworkedManagerBase<GameManager>.instance.runId
		};
		foreach (DewAchievementItem trackedAchievement in ManagerBase<AchievementManager>.instance.trackedAchievements)
		{
			Serialize(trackedAchievement, achievementsData.persistenceData, trackedAchievement.GetType().Name + "::");
		}
		return achievementsData;
	}

	public static void ApplyAchievementData(AchievementsData data)
	{
		foreach (DewAchievementItem trackedAchievement in ManagerBase<AchievementManager>.instance.trackedAchievements)
		{
			DeserializeOn(trackedAchievement, data.persistenceData, null, trackedAchievement.GetType().Name + "::");
		}
	}

	public static GameData SerializeGameData(LoadNodeSettings settings)
	{
		if (!NetworkServer.active)
		{
			throw new InvalidOperationException();
		}
		GameData data = new GameData
		{
			saveVersion = 1,
			loadNodeSettings = settings,
			isMultiplayer = (DewNetworkManager.startSettings.networkMode != DewNetworkMode.Singleplayer),
			lobbyType = DewNetworkManager.startSettings.lobbyType
		};
		ProcessManagers(ManagerBase<GameLogicPackage>.instance.transform);
		ProcessManagers(ManagerBase<NetworkLogicPackage>.instance.transform);
		data.serverActorData = SerializeGameObject(((Component)(object)NetworkedManagerBase<ActorManager>.instance.serverActor).gameObject);
		foreach (DewPlayer gamePlayer in DewPlayer.gamePlayers)
		{
			try
			{
				if (!((UnityEngine.Object)(object)gamePlayer.hero == null))
				{
					data.players.Add(SerializePlayerData(gamePlayer));
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
		foreach (Actor allActor in NetworkedManagerBase<ActorManager>.instance.allActors)
		{
			try
			{
				if (!(allActor is StatusEffect) && GetAttribute<SaveActorAttribute>(((object)allActor).GetType()) != null)
				{
					data.actors.Add(new GeneralData().Capture(allActor));
				}
			}
			catch (Exception exception2)
			{
				Debug.LogException(exception2);
			}
		}
		data.actors = data.actors.OrderBy((GeneralData d) => d.prevNetId).ToList();
		return data;
		void ProcessManagers(Transform packageTransform)
		{
			foreach (Transform item in packageTransform)
			{
				try
				{
					Component component = FindManagerComponent(item.gameObject);
					if (!(component == null))
					{
						Dictionary<string, string> dictionary = SerializeGameObject(item.gameObject);
						if (dictionary.Count > 0)
						{
							data.managerPersistentData[component.GetType().Name] = dictionary;
						}
					}
				}
				catch (Exception exception3)
				{
					Debug.LogException(exception3);
				}
			}
		}
	}

	public static void ApplyGameData(GameData data, Action onFinish = null, JsonSerializerSettings settings = null)
	{
		if (settings == null)
		{
			settings = GetNewSettings();
		}
		NetworkBehaviourConverter conv = (NetworkBehaviourConverter)(object)settings.Converters[0];
		Coroutiner coroutiner = new GameObject().AddComponent<Coroutiner>();
		UnityEngine.Object.DontDestroyOnLoad(coroutiner);
		coroutiner.StartCoroutine(Routine());
		void ProcessManagers(Transform packageTransform)
		{
			foreach (Transform item in packageTransform)
			{
				try
				{
					Component component = FindManagerComponent(item.gameObject);
					if (!(component == null))
					{
						string name = component.GetType().Name;
						if (data.managerPersistentData.TryGetValue(name, out var value))
						{
							DeserializeOnGameObject(item.gameObject, value, settings);
						}
					}
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
				}
			}
		}
		IEnumerator Routine()
		{
			try
			{
				if ((UnityEngine.Object)(object)SingletonDewNetworkBehaviour<Room>.instance != null)
				{
					((MonoBehaviour)(object)SingletonDewNetworkBehaviour<Room>.instance.monsters).StopAllCoroutines();
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
			try
			{
				NetworkedManagerBase<ActorManager>.instance.ClearSceneIdOfAllActors();
				Actor[] array = NetworkedManagerBase<ActorManager>.instance.allActors.ToArray();
				foreach (Actor actor in array)
				{
					if (!actor.IsNullOrInactive() && !(actor is ServerActor))
					{
						actor.Destroy();
					}
				}
			}
			catch (Exception exception2)
			{
				Debug.LogException(exception2);
			}
			ProcessManagers(ManagerBase<GameLogicPackage>.instance.transform);
			ProcessManagers(ManagerBase<NetworkLogicPackage>.instance.transform);
			try
			{
				if (data.serverActorData != null)
				{
					DeserializeOnGameObject(((Component)(object)NetworkedManagerBase<ActorManager>.instance.serverActor).gameObject, data.serverActorData, settings);
				}
			}
			catch (Exception exception3)
			{
				Debug.LogException(exception3);
			}
			List<(uint, Action)> list = new List<(uint, Action)>();
			if (DewPlayer.gamePlayers.Count == 1 && data.players.Count == 1)
			{
				try
				{
					ApplyPlayerData(DewPlayer.gamePlayers[0], data.players[0], list, settings);
				}
				catch (Exception exception4)
				{
					Debug.LogException(exception4);
				}
			}
			else
			{
				foreach (DewPlayer p in DewPlayer.gamePlayers)
				{
					try
					{
						PlayerData playerData = data.players.Find((PlayerData d) => d.playerGuid == p.guid);
						if (playerData != null)
						{
							ApplyPlayerData(p, playerData, list, settings);
						}
					}
					catch (Exception exception5)
					{
						Debug.LogException(exception5);
					}
				}
			}
			foreach (GeneralData actor2 in data.actors)
			{
				try
				{
					GeneralData ad = actor2;
					Actor prefab = ad.GetPrefab<Actor>();
					if ((UnityEngine.Object)(object)prefab == null)
					{
						Debug.LogWarning("Unable to resolve actor type: " + ad.resId);
					}
					else
					{
						Actor newActor = Dew.InstantiateActor_Imp(prefab, Vector3.zero, null, null);
						conv.prevNetIdMap[ad.prevNetId] = ((NetworkBehaviour)newActor).netIdentity;
						((NetworkBehaviour)newActor).netIdentity.netId = NetworkIdentity.GetNextNetworkId();
						((NetworkBehaviour)newActor).netIdentity.isSpawnedInAdvance = true;
						NetworkServer.spawned[((NetworkBehaviour)newActor).netIdentity.netId] = ((NetworkBehaviour)newActor).netIdentity;
						list.Add((ad.prevNetId, () =>
						{
							Dew.PrepareAndSpawnActor_Imp(newActor, (Actor actor2) =>
							{
								actor2.isNewInstance = false;
								DeserializeOnGameObject(((Component)(object)actor2).gameObject, ad.persistenceData, settings, SaveVarFlags.Default, SaveVarFlags.ApplyAfterCreation | SaveVarFlags.ApplyAfterFrameDelay);
							});
							DeserializeOnGameObject(((Component)(object)newActor).gameObject, ad.persistenceData, settings, SaveVarFlags.ApplyAfterCreation);
							Dew.CallDelayed(() =>
							{
								DeserializeOnGameObject(((Component)(object)newActor).gameObject, ad.persistenceData, settings, SaveVarFlags.ApplyAfterFrameDelay);
							});
						}));
					}
				}
				catch (Exception exception6)
				{
					Debug.LogException(exception6);
				}
			}
			foreach (var item2 in list.OrderBy(((uint, Action) t) => t.Item1))
			{
				try
				{
					item2.Item2?.Invoke();
				}
				catch (Exception exception7)
				{
					Debug.LogException(exception7);
				}
			}
			yield return null;
			yield return new WaitForSecondsRealtime(0.15f);
			try
			{
				LoadNodeSettings loadNodeSettings = data.loadNodeSettings;
				if (loadNodeSettings == null)
				{
					loadNodeSettings = new LoadNodeSettings
					{
						from = -1,
						to = NetworkedManagerBase<ZoneManager>.instance.currentNodeIndex,
						advanceTurn = false,
						isSidetrackTransition = false,
						newZone = NetworkedManagerBase<ZoneManager>.instance.currentZone,
						isLoadingFromSave = true,
						isTravelingRoom = false,
						isTravelingZone = false
					};
				}
				else
				{
					loadNodeSettings = loadNodeSettings.Clone();
					loadNodeSettings.from = -1;
					if (loadNodeSettings.newZone == null)
					{
						loadNodeSettings.newZone = NetworkedManagerBase<ZoneManager>.instance.currentZone;
					}
					loadNodeSettings.isLoadingFromSave = true;
				}
				NetworkedManagerBase<ZoneManager>.instance.LoadNode(loadNodeSettings);
			}
			catch (Exception exception8)
			{
				Debug.LogException(exception8);
			}
			yield return new WaitWhile(() => NetworkedManagerBase<ZoneManager>.instance.isInRoomTransition);
			try
			{
				DewResources.UnloadUnused();
				DewResources.WaitForPendingLoads();
			}
			catch (Exception exception9)
			{
				Debug.LogException(exception9);
			}
			UnityEngine.Object.Destroy(coroutiner.gameObject);
			onFinish?.Invoke();
		}
	}

	private static Component FindManagerComponent(GameObject go)
	{
		ListReturnHandle<Component> handle;
		foreach (Component item in go.GetComponentsNonAlloc(out handle))
		{
			if (item == null)
			{
				continue;
			}
			Type type = item.GetType();
			while (type != null && type != typeof(object))
			{
				if (type.IsGenericType)
				{
					Type genericTypeDefinition = type.GetGenericTypeDefinition();
					if (genericTypeDefinition == typeof(ManagerBase<>) || genericTypeDefinition == typeof(NetworkedManagerBase<>))
					{
						handle.Return();
						return item;
					}
				}
				type = type.BaseType;
			}
		}
		handle.Return();
		return null;
	}

	public static RoomData SerializeRoomData()
	{
		RoomData roomData = new RoomData();
		roomData.persistenceData = SerializeGameObject(((Component)(object)SingletonDewNetworkBehaviour<Room>.instance).gameObject);
		roomData.actors = new List<RoomActorData>();
		foreach (Actor allActor in NetworkedManagerBase<ActorManager>.instance.allActors)
		{
			try
			{
				if (allActor.isActive && allActor.isDestroyedOnRoomChange && allActor.ShouldBeSavedWithRoom() && !allActor.IsExcludedFromRoomSave())
				{
					RoomActorData roomActorData = new RoomActorData();
					roomActorData.Capture(allActor);
					roomData.actors.Add(roomActorData);
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
		roomData.savedObjects = new List<RoomSavedObjectData>();
		RoomSavedObject[] array = UnityEngine.Object.FindObjectsOfType<RoomSavedObject>(includeInactive: true);
		foreach (RoomSavedObject roomSavedObject in array)
		{
			try
			{
				if (!(roomSavedObject == null))
				{
					RoomSavedObjectData item = new RoomSavedObjectData().Capture(roomSavedObject);
					roomData.savedObjects.Add(item);
				}
			}
			catch (Exception exception2)
			{
				Debug.LogException(exception2);
			}
		}
		return roomData;
	}

	public static void ApplyRoomDataBeforeSpawnObjects(RoomData data, JsonSerializerSettings settings = null)
	{
		if (settings == null)
		{
			settings = GetNewSettings();
		}
		DeserializeOnGameObject(((Component)(object)SingletonDewNetworkBehaviour<Room>.instance).gameObject, data.persistenceData, settings);
		NetworkBehaviourConverter networkBehaviourConverter = (NetworkBehaviourConverter)(object)settings.Converters[0];
		if (data.actors != null)
		{
			Actor[] array = UnityEngine.Object.FindObjectsOfType<Actor>(true);
			foreach (RoomActorData actor3 in data.actors)
			{
				try
				{
					if (actor3.sceneId == 0L)
					{
						continue;
					}
					Actor actor = null;
					Actor[] array2 = array;
					foreach (Actor actor2 in array2)
					{
						if (((Component)(object)actor2).GetComponent<NetworkIdentity>().sceneId == actor3.sceneId)
						{
							actor = actor2;
							break;
						}
					}
					if ((UnityEngine.Object)(object)actor == null)
					{
						Debug.LogWarning($"Could not find scene actor to load in: {actor3.resId} ({actor3.sceneId})");
						continue;
					}
					actor.isNewInstance = false;
					DeserializeOnGameObject(((Component)(object)actor).gameObject, actor3.persistenceData, settings, SaveVarFlags.Default, SaveVarFlags.ApplyAfterCreation | SaveVarFlags.ApplyAfterFrameDelay);
					networkBehaviourConverter.prevNetIdMap[actor3.prevNetId] = ((NetworkBehaviour)actor).netIdentity;
				}
				catch (Exception exception)
				{
					Debug.LogError($"Exception occured while trying to load in scene actor {actor3.resId} ({actor3.sceneId}) before spawn objects");
					Debug.LogException(exception);
				}
			}
		}
		if (data.savedObjects == null)
		{
			return;
		}
		RoomSavedObject[] array3 = UnityEngine.Object.FindObjectsOfType<RoomSavedObject>(includeInactive: true);
		foreach (RoomSavedObjectData objData in data.savedObjects)
		{
			try
			{
				if (string.IsNullOrEmpty(objData.guid))
				{
					continue;
				}
				RoomSavedObject targetObject = null;
				RoomSavedObject[] array4 = array3;
				foreach (RoomSavedObject roomSavedObject in array4)
				{
					if (!(roomSavedObject.guid != objData.guid))
					{
						targetObject = roomSavedObject;
						break;
					}
				}
				if (targetObject == null)
				{
					array4 = array3;
					foreach (RoomSavedObject roomSavedObject2 in array4)
					{
						if (!(roomSavedObject2.transform.GetScenePath() != objData.scenePath))
						{
							targetObject = roomSavedObject2;
							break;
						}
					}
				}
				if (targetObject == null)
				{
					Debug.LogWarning("Could not find scene object to load in: " + objData.scenePath + " (" + objData.guid + ")");
				}
				else
				{
					DeserializeOnGameObject(targetObject.gameObject, objData.persistenceData, settings, SaveVarFlags.Default, SaveVarFlags.ApplyAfterFrameDelay);
					Dew.CallDelayed(() =>
					{
						DeserializeOnGameObject(targetObject.gameObject, objData.persistenceData, settings, SaveVarFlags.ApplyAfterFrameDelay);
					});
				}
			}
			catch (Exception exception2)
			{
				Debug.LogError("Exception occured while trying to load in scene object " + objData.scenePath + " (" + objData.guid + ") before spawn objects");
				Debug.LogException(exception2);
			}
		}
	}

	public static void ApplyRoomDataAfterSpawnObjects(RoomData data, JsonSerializerSettings settings = null)
	{
		if (settings == null)
		{
			settings = GetNewSettings();
		}
		DeserializeOnGameObject(((Component)(object)SingletonDewNetworkBehaviour<Room>.instance).gameObject, data.persistenceData, settings);
		NetworkBehaviourConverter networkBehaviourConverter = (NetworkBehaviourConverter)(object)settings.Converters[0];
		foreach (RoomActorData ad in data.actors)
		{
			try
			{
				Actor targetActor = null;
				if (ad.sceneId != 0L)
				{
					foreach (Actor allActor in NetworkedManagerBase<ActorManager>.instance.allActors)
					{
						if (((NetworkBehaviour)allActor).netIdentity.sceneId == ad.sceneId)
						{
							targetActor = allActor;
							break;
						}
					}
					if (!((UnityEngine.Object)(object)targetActor == null))
					{
						goto IL_01e0;
					}
					Debug.LogWarning($"Could not find scene actor to load in: {ad.resId} ({ad.sceneId})");
					continue;
				}
				Actor prefab = ((GeneralData)ad).GetPrefab<Actor>();
				if ((UnityEngine.Object)(object)prefab == null)
				{
					Debug.LogWarning($"Unable to resolve actor type: {ad.resId} ({ad.sceneId})");
					continue;
				}
				targetActor = Dew.CreateActor(prefab, Vector3.zero, null, null, (Actor actor) =>
				{
					actor.isNewInstance = false;
					DeserializeOnGameObject(((Component)(object)actor).gameObject, ad.persistenceData, settings, SaveVarFlags.Default, SaveVarFlags.ApplyAfterCreation | SaveVarFlags.ApplyAfterFrameDelay);
				});
				goto IL_01e0;
				IL_01e0:
				networkBehaviourConverter.prevNetIdMap[ad.prevNetId] = ((NetworkBehaviour)targetActor).netIdentity;
				DeserializeOnGameObject(((Component)(object)targetActor).gameObject, ad.persistenceData, settings, SaveVarFlags.ApplyAfterCreation);
				Dew.CallDelayed(() =>
				{
					DeserializeOnGameObject(((Component)(object)targetActor).gameObject, ad.persistenceData, settings, SaveVarFlags.ApplyAfterFrameDelay);
				});
			}
			catch (Exception exception)
			{
				Debug.LogError($"Exception occured while trying to load in scene actor {ad.resId} ({ad.sceneId}) after spawn objects");
				Debug.LogException(exception);
			}
		}
	}

	public static PlayerData SerializePlayerData(DewPlayer player)
	{
		PlayerData playerData = new PlayerData();
		playerData.playerPrevNetId = ((NetworkBehaviour)player).netId;
		playerData.heroPrevNetId = ((NetworkBehaviour)player.hero).netId;
		playerData.playerGuid = player.guid;
		playerData.playerName = player.playerName;
		playerData.heroType = ((object)player.hero).GetType().Name;
		playerData.isHeroKnockedOut = player.hero.isKnockedOut;
		playerData.playerPersistenceData = SerializeGameObject(((Component)(object)player).gameObject);
		playerData.heroPersistenceData = SerializeGameObject(((Component)(object)player.hero).gameObject);
		HeroSkillLocation[] savedSkills = _savedSkills;
		foreach (HeroSkillLocation type in savedSkills)
		{
			if (player.hero.Skill.TryGetSkill(type, out var skill))
			{
				SkillData skillData = new SkillData();
				skillData.Capture(skill);
				playerData.skills.Add(skillData);
			}
		}
		foreach (KeyValuePair<GemLocation, Gem> gem in player.hero.Skill.gems)
		{
			GemData gemData = new GemData();
			gemData.Capture(gem.Value);
			playerData.gems.Add(gemData);
		}
		foreach (StatusEffect statusEffect in player.hero.Status.statusEffects)
		{
			SaveActorAttribute attribute = GetAttribute<SaveActorAttribute>(((object)statusEffect).GetType());
			if (attribute != null && attribute.shouldBeSaved)
			{
				StatusEffectData statusEffectData = new StatusEffectData();
				statusEffectData.Capture(statusEffect);
				playerData.statusEffects.Add(statusEffectData);
			}
		}
		return playerData;
	}

	public static void ApplyPlayerData(DewPlayer player, PlayerData data, List<(uint, Action)> batchActions = null, JsonSerializerSettings settings = null)
	{
		if (settings == null)
		{
			settings = GetNewSettings();
		}
		NetworkBehaviourConverter conv = (NetworkBehaviourConverter)(object)settings.Converters[0];
		DeserializeOnGameObject(((Component)(object)player).gameObject, data.playerPersistenceData, settings);
		Hero byShortTypeName = DewResources.GetByShortTypeName<Hero>(data.heroType, default(ResourceLoadSettings));
		if ((UnityEngine.Object)(object)byShortTypeName == null)
		{
			return;
		}
		if ((UnityEngine.Object)(object)player.hero != null)
		{
			player.hero.Destroy();
		}
		conv.prevNetIdMap[data.playerPrevNetId] = ((NetworkBehaviour)player).netIdentity;
		bool flag = batchActions == null;
		if (batchActions == null)
		{
			batchActions = new List<(uint, Action)>();
		}
		Hero hero = Dew.InstantiateActor_Imp(byShortTypeName, Vector3.zero, null, null);
		conv.prevNetIdMap[data.heroPrevNetId] = ((NetworkBehaviour)hero).netIdentity;
		((NetworkBehaviour)hero).netIdentity.netId = NetworkIdentity.GetNextNetworkId();
		((NetworkBehaviour)hero).netIdentity.isSpawnedInAdvance = true;
		NetworkServer.spawned[((NetworkBehaviour)hero).netIdentity.netId] = ((NetworkBehaviour)hero).netIdentity;
		batchActions.Add((data.heroPrevNetId, () =>
		{
			player.hero = Dew.PrepareAndSpawnHero_Imp(hero, player, 1, new HeroLoadoutData
			{
				skillQ = -1,
				skillR = -1,
				skillTrait = -1,
				skillMovement = -1
			}, null, (Hero h) =>
			{
				h.isNewInstance = false;
				DeserializeOnGameObject(((Component)(object)h).gameObject, data.heroPersistenceData, settings, SaveVarFlags.Default, SaveVarFlags.ApplyAfterCreation | SaveVarFlags.ApplyAfterFrameDelay);
			});
			DeserializeOnGameObject(((Component)(object)player.hero).gameObject, data.heroPersistenceData, settings, SaveVarFlags.ApplyAfterCreation);
			Dew.CallDelayed(() =>
			{
				DeserializeOnGameObject(((Component)(object)player.hero).gameObject, data.heroPersistenceData, settings, SaveVarFlags.ApplyAfterFrameDelay);
			});
			HeroSkillLocation[] savedSkills = _savedSkills;
			foreach (HeroSkillLocation loc in savedSkills)
			{
				try
				{
					if (player.hero.Skill.TryGetSkill(loc, out var skill))
					{
						player.hero.Skill.UnequipSkill(loc, Vector3.zero, ignoreCanReplace: true);
						skill.Destroy();
					}
					SkillData skillData = data.skills.Find((SkillData s) => s.location == loc);
					if (skillData != null)
					{
						SkillTrigger prefab2 = ((GeneralData)skillData).GetPrefab<SkillTrigger>();
						if (!((UnityEngine.Object)(object)prefab2 == null))
						{
							SkillTrigger newSkill = Dew.CreateSkillTrigger(prefab2, Vector3.zero, 1, null, (SkillTrigger s) =>
							{
								s.isNewInstance = false;
								DeserializeOnGameObject(((Component)(object)s).gameObject, skillData.persistenceData, settings, SaveVarFlags.Default, SaveVarFlags.ApplyAfterCreation | SaveVarFlags.ApplyAfterFrameDelay);
							});
							conv.prevNetIdMap[skillData.prevNetId] = ((NetworkBehaviour)newSkill).netIdentity;
							DeserializeOnGameObject(((Component)(object)newSkill).gameObject, skillData.persistenceData, settings, SaveVarFlags.ApplyAfterCreation);
							Dew.CallDelayed(() =>
							{
								DeserializeOnGameObject(((Component)(object)newSkill).gameObject, skillData.persistenceData, settings, SaveVarFlags.ApplyAfterFrameDelay);
							});
							player.hero.Skill.EquipSkill(loc, newSkill, ignoreCanReplace: true);
						}
					}
				}
				catch (Exception exception3)
				{
					Debug.LogException(exception3);
				}
			}
			foreach (GemData gemData in data.gems)
			{
				try
				{
					Gem prefab3 = ((GeneralData)gemData).GetPrefab<Gem>();
					if (!((UnityEngine.Object)(object)prefab3 == null))
					{
						Gem newGem = Dew.CreateGem(prefab3, Vector3.zero, 10, null, (Gem g) =>
						{
							g.isNewInstance = false;
							DeserializeOnGameObject(((Component)(object)g).gameObject, gemData.persistenceData, settings, SaveVarFlags.Default, SaveVarFlags.ApplyAfterCreation | SaveVarFlags.ApplyAfterFrameDelay);
						});
						conv.prevNetIdMap[gemData.prevNetId] = ((NetworkBehaviour)newGem).netIdentity;
						DeserializeOnGameObject(((Component)(object)newGem).gameObject, gemData.persistenceData, settings, SaveVarFlags.ApplyAfterCreation);
						Dew.CallDelayed(() =>
						{
							DeserializeOnGameObject(((Component)(object)newGem).gameObject, gemData.persistenceData, settings, SaveVarFlags.ApplyAfterFrameDelay);
						});
						player.hero.Skill.EquipGem(gemData.location, newGem);
					}
				}
				catch (Exception exception4)
				{
					Debug.LogException(exception4);
				}
			}
			player.controllingEntity = player.hero;
			if (data.isHeroKnockedOut)
			{
				player.hero.CreateStatusEffect(player.hero, new CastInfo(player.hero), (Se_HeroKnockedOut se) =>
				{
					se.isNewInstance = false;
				});
				player.hero.Control.Teleport(new Vector3(-5000f, -5000f, -5000f));
			}
		}));
		foreach (GeneralData statusEffect in data.statusEffects)
		{
			try
			{
				GeneralData seData = statusEffect;
				StatusEffect prefab = seData.GetPrefab<StatusEffect>();
				if ((UnityEngine.Object)(object)prefab == null)
				{
					continue;
				}
				StatusEffect newSe = Dew.InstantiateActor_Imp(prefab, Vector3.zero, null, null);
				conv.prevNetIdMap[seData.prevNetId] = ((NetworkBehaviour)newSe).netIdentity;
				((NetworkBehaviour)newSe).netIdentity.netId = NetworkIdentity.GetNextNetworkId();
				((NetworkBehaviour)newSe).netIdentity.isSpawnedInAdvance = true;
				NetworkServer.spawned[((NetworkBehaviour)newSe).netIdentity.netId] = ((NetworkBehaviour)newSe).netIdentity;
				batchActions.Add((seData.prevNetId, () =>
				{
					Entity victim = FromJson<Entity>(seData.persistenceData[seData.resId + "::victim"], settings);
					CastInfo info = FromJson<CastInfo>(seData.persistenceData[seData.resId + "::info"], settings);
					Dew.PrepareAndSpawnStatusEffect_Imp(newSe, victim, null, info, (StatusEffect se) =>
					{
						se.isNewInstance = false;
						DeserializeOnGameObject(((Component)(object)se).gameObject, seData.persistenceData, settings, SaveVarFlags.Default, SaveVarFlags.ApplyAfterCreation | SaveVarFlags.ApplyAfterFrameDelay);
					});
					DeserializeOnGameObject(((Component)(object)newSe).gameObject, seData.persistenceData, settings, SaveVarFlags.ApplyAfterCreation);
					Dew.CallDelayed(() =>
					{
						DeserializeOnGameObject(((Component)(object)newSe).gameObject, seData.persistenceData, settings, SaveVarFlags.ApplyAfterFrameDelay);
					});
				}));
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
		if (!flag)
		{
			return;
		}
		foreach (var item in batchActions.OrderBy(((uint, Action) t) => t.Item1))
		{
			try
			{
				item.Item2?.Invoke();
			}
			catch (Exception exception2)
			{
				Debug.LogException(exception2);
			}
		}
	}

	public static T CreateActor<T>(GeneralData data, List<(uint, Action)> batchActions = null, JsonSerializerSettings settings = null) where T : Actor
	{
		if (settings == null)
		{
			settings = GetNewSettings();
		}
		NetworkBehaviourConverter networkBehaviourConverter = (NetworkBehaviourConverter)(object)settings.Converters[0];
		bool flag = batchActions == null;
		if (batchActions == null)
		{
			batchActions = new List<(uint, Action)>();
		}
		T prefab = data.GetPrefab<T>();
		if ((UnityEngine.Object)(object)prefab == null)
		{
			return null;
		}
		T newActor = Dew.InstantiateActor_Imp(prefab, Vector3.zero, null, null);
		networkBehaviourConverter.prevNetIdMap[data.prevNetId] = ((NetworkBehaviour)newActor).netIdentity;
		((NetworkBehaviour)newActor).netIdentity.netId = NetworkIdentity.GetNextNetworkId();
		((NetworkBehaviour)newActor).netIdentity.isSpawnedInAdvance = true;
		NetworkServer.spawned[((NetworkBehaviour)newActor).netIdentity.netId] = ((NetworkBehaviour)newActor).netIdentity;
		batchActions.Add((data.prevNetId, () =>
		{
			Dew.PrepareAndSpawnActor_Imp(newActor, (T se) =>
			{
				se.isNewInstance = false;
				DeserializeOnGameObject(((Component)(object)se).gameObject, data.persistenceData, settings, SaveVarFlags.Default, SaveVarFlags.ApplyAfterCreation | SaveVarFlags.ApplyAfterFrameDelay);
			});
			DeserializeOnGameObject(((Component)(object)newActor).gameObject, data.persistenceData, settings, SaveVarFlags.ApplyAfterCreation);
			Dew.CallDelayed(() =>
			{
				DeserializeOnGameObject(((Component)(object)newActor).gameObject, data.persistenceData, settings, SaveVarFlags.ApplyAfterFrameDelay);
			});
		}));
		if (flag)
		{
			foreach (var item in batchActions.OrderBy(((uint, Action) t) => t.Item1))
			{
				try
				{
					item.Item2?.Invoke();
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
				}
			}
		}
		return newActor;
	}

	public static JsonSerializerSettings GetNewSettings()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected Obj, but got Unknown
		return new JsonSerializerSettings
		{
			ReferenceLoopHandling = (ReferenceLoopHandling)1,
			Converters = new List<JsonConverter>
			{
				(JsonConverter)(object)new NetworkBehaviourConverter(),
				(JsonConverter)(object)new AssetConverter()
			}
		};
	}
}
