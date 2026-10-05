using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using IngameDebugConsole;
using Mirror;
using Mono.CecilX;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Steamworks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class DewMod
{
	public class FieldsOnlyResolver : DefaultContractResolver
	{
		protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			JsonProperty val = ((DefaultContractResolver)this).CreateProperty(member, memberSerialization);
			if (member.DeclaringType == typeof(JsonOverrideItem))
			{
				return val;
			}
			using (IEnumerator<Attribute> enumerator = member.GetCustomAttributes(typeof(JsonPropertyAttribute)).GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					_ = enumerator.Current;
					return val;
				}
			}
			if (!(member is FieldInfo fieldInfo) || IgnoredFieldsJsonOverride.Contains(member.Name) || !AllowedValueTypesForJsonOverride.Contains(fieldInfo.FieldType))
			{
				val.ShouldSerialize = (object _) => false;
			}
			return val;
		}
	}

	private static class JsonOverrideHelper
	{
		private static readonly BindingFlags FieldBindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

		public static void Apply(object root, string path, string jsonValue)
		{
			string[] path2 = path.Split('.', StringSplitOptions.None);
			SetValueRecursive(root, path2, 0, jsonValue);
		}

		private static object SetValueRecursive(object currentObject, string[] path, int depth, string jsonValue)
		{
			if (currentObject == null)
			{
				throw new NullReferenceException("Cannot apply override because object at path '" + string.Join(".", path, 0, depth) + "' is null.");
			}
			string memberName = path[depth];
			if (depth == path.Length - 1)
			{
				SetFinalValue(currentObject, memberName, jsonValue);
				return currentObject;
			}
			(object value, Action<object> setter) nextObjectAndSetter = GetNextObjectAndSetter(currentObject, memberName);
			object item = nextObjectAndSetter.value;
			Action<object> item2 = nextObjectAndSetter.setter;
			object obj = SetValueRecursive(item, path, depth + 1, jsonValue);
			item2(obj);
			return currentObject;
		}

		private static (object value, Action<object> setter) GetNextObjectAndSetter(object source, string memberName)
		{
			if (int.TryParse(memberName, out var index))
			{
				Array array = source as Array;
				if (array != null)
				{
					if (index < 0 || index >= array.Length)
					{
						throw new IndexOutOfRangeException($"Index {index} is out of range for array with {array.Length} elements.");
					}
					return (value: array.GetValue(index), setter: (object newValue) =>
					{
						array.SetValue(newValue, index);
					});
				}
				IList list = source as IList;
				if (list != null)
				{
					if (index < 0 || index >= list.Count)
					{
						throw new IndexOutOfRangeException($"Index {index} is out of range for list with {list.Count} elements.");
					}
					return (value: list[index], setter: (object newValue) =>
					{
						list[index] = newValue;
					});
				}
				throw new InvalidOperationException("Member '" + memberName + "' is an index, but the target of type '" + source.GetType().Name + "' is not a list or array.");
			}
			FieldInfo fieldInfo = source.GetType().GetField(memberName, FieldBindingFlags);
			if (fieldInfo != null)
			{
				return (value: fieldInfo.GetValue(source), setter: (object newValue) =>
				{
					fieldInfo.SetValue(source, newValue);
				});
			}
			PropertyInfo propInfo = source.GetType().GetProperty(memberName, FieldBindingFlags);
			if (propInfo != null)
			{
				return (value: propInfo.GetValue(source), setter: (object newValue) =>
				{
					propInfo.SetValue(source, newValue);
				});
			}
			throw new MissingFieldException("Field or property '" + memberName + "' not found on type '" + source.GetType().Name + "'.");
		}

		private static void SetFinalValue(object container, string memberName, string jsonValue)
		{
			if (int.TryParse(memberName, out var result))
			{
				if (container is Array array)
				{
					if (result < 0 || result >= array.Length)
					{
						throw new IndexOutOfRangeException($"Cannot set value at index {result} for an array of size {array.Length}.");
					}
					Type elementType = array.GetType().GetElementType();
					if (elementType == null)
					{
						throw new InvalidOperationException("Could not determine element type of the array.");
					}
					if (elementType == typeof(string) && !jsonValue.TrimStart().StartsWith("\""))
					{
						jsonValue = JsonConvert.ToString(jsonValue);
					}
					object value = JsonConvert.DeserializeObject(jsonValue, elementType);
					array.SetValue(value, result);
					return;
				}
				if (container is IList list)
				{
					if (result < 0 || result >= list.Count)
					{
						throw new IndexOutOfRangeException($"Cannot set value at index {result} for a list of size {list.Count}.");
					}
					Type type = (list.GetType().IsGenericType ? list.GetType().GetGenericArguments()[0] : list.GetType().GetElementType());
					if (type == null)
					{
						throw new InvalidOperationException("Could not determine item type of the list.");
					}
					string text = jsonValue;
					if (type == typeof(string) && !text.TrimStart().StartsWith("\""))
					{
						text = JsonConvert.ToString(jsonValue);
					}
					object value2 = JsonConvert.DeserializeObject(text, type);
					list[result] = value2;
					return;
				}
			}
			FieldInfo field = container.GetType().GetField(memberName, FieldBindingFlags);
			PropertyInfo property = container.GetType().GetProperty(memberName, FieldBindingFlags);
			if (field == null && property == null)
			{
				throw new MissingFieldException("Final member '" + memberName + "' not found on type '" + container.GetType().Name + "'.");
			}
			Type type2 = ((field != null) ? field.FieldType : property.PropertyType);
			if (type2 == typeof(string) && !jsonValue.TrimStart().StartsWith("\""))
			{
				jsonValue = JsonConvert.ToString(jsonValue);
			}
			object value3 = JsonConvert.DeserializeObject(jsonValue, type2);
			if (field != null)
			{
				field.SetValue(container, value3);
			}
			else
			{
				property.SetValue(container, value3);
			}
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass64_0
	{
		public UniTaskCompletionSource<SubmitItemUpdateResult_t> source;

		public UGCUpdateHandle_t handle;

		internal void _003CUpdateItem_003Eb__0(SubmitItemUpdateResult_t res, bool failure)
		{
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			if (failure)
			{
				source.TrySetException((Exception)new SteamException("Creation failed for unknown reason"));
			}
			else
			{
				source.TrySetResult(res);
			}
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CCreateItem_003Ed__63 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder<CreateItemResult_t> _003C_003Et__builder;

		private Awaiter<CreateItemResult_t> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_007c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			//IL_0088: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0099: Unknown result type (might be due to invalid IL or missing references)
			//IL_009e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			CreateItemResult_t result;
			try
			{
				Awaiter<CreateItemResult_t> val2;
				if (num != 0)
				{
					SteamAPICall_t val = SteamUGC.CreateItem(SteamUtils.GetAppID(), (EWorkshopFileType)0);
					UniTaskCompletionSource<CreateItemResult_t> source = new UniTaskCompletionSource<CreateItemResult_t>();
					CallResult<CreateItemResult_t>.Create((APIDispatchDelegate<CreateItemResult_t>)((CreateItemResult_t res, bool failure) =>
					{
						//IL_0021: Unknown result type (might be due to invalid IL or missing references)
						if (failure)
						{
							source.TrySetException((Exception)new SteamException("Creation failed for unknown reason"));
						}
						else
						{
							source.TrySetResult(res);
						}
					})).Set(val, (APIDispatchDelegate<CreateItemResult_t>)null);
					val2 = source.Task.GetAwaiter();
					if (!val2.IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val2;
						_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter<CreateItemResult_t>, _003CCreateItem_003Ed__63>(ref val2, ref this);
						return;
					}
				}
				else
				{
					val2 = _003C_003Eu__1;
					_003C_003Eu__1 = default;
					num = (_003C_003E1__state = -1);
				}
				result = val2.GetResult();
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003C_003Et__builder.SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003C_003Et__builder.SetResult(result);
		}

		void IAsyncStateMachine.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			this.MoveNext();
		}

		[DebuggerHidden]
		private void SetStateMachine(IAsyncStateMachine stateMachine)
		{
			_003C_003Et__builder.SetStateMachine(stateMachine);
		}

		void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
		{
			//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
			this.SetStateMachine(stateMachine);
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CDeleteItem_003Ed__65 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder<DeleteItemResult_t> _003C_003Et__builder;

		public PublishedFileId_t id;

		private Awaiter<DeleteItemResult_t> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_007c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			//IL_0088: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0099: Unknown result type (might be due to invalid IL or missing references)
			//IL_009e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			DeleteItemResult_t result;
			try
			{
				Awaiter<DeleteItemResult_t> val2;
				if (num != 0)
				{
					SteamAPICall_t val = SteamUGC.DeleteItem(id);
					UniTaskCompletionSource<DeleteItemResult_t> source = new UniTaskCompletionSource<DeleteItemResult_t>();
					CallResult<DeleteItemResult_t>.Create((APIDispatchDelegate<DeleteItemResult_t>)((DeleteItemResult_t res, bool failure) =>
					{
						//IL_0021: Unknown result type (might be due to invalid IL or missing references)
						if (failure)
						{
							source.TrySetException((Exception)new SteamException("Deletion failed for unknown reason"));
						}
						else
						{
							source.TrySetResult(res);
						}
					})).Set(val, (APIDispatchDelegate<DeleteItemResult_t>)null);
					val2 = source.Task.GetAwaiter();
					if (!val2.IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val2;
						_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter<DeleteItemResult_t>, _003CDeleteItem_003Ed__65>(ref val2, ref this);
						return;
					}
				}
				else
				{
					val2 = _003C_003Eu__1;
					_003C_003Eu__1 = default;
					num = (_003C_003E1__state = -1);
				}
				result = val2.GetResult();
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003C_003Et__builder.SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003C_003Et__builder.SetResult(result);
		}

		void IAsyncStateMachine.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			this.MoveNext();
		}

		[DebuggerHidden]
		private void SetStateMachine(IAsyncStateMachine stateMachine)
		{
			_003C_003Et__builder.SetStateMachine(stateMachine);
		}

		void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
		{
			//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
			this.SetStateMachine(stateMachine);
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CUpdateItem_003Ed__64 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder<SubmitItemUpdateResult_t> _003C_003Et__builder;

		public PublishedFileId_t id;

		public ModItem item;

		public bool isNew;

		private Awaiter<SubmitItemUpdateResult_t> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_013e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0143: Unknown result type (might be due to invalid IL or missing references)
			//IL_014b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_015c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0161: Unknown result type (might be due to invalid IL or missing references)
			//IL_0086: Unknown result type (might be due to invalid IL or missing references)
			//IL_018b: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00da: Unknown result type (might be due to invalid IL or missing references)
			//IL_0103: Unknown result type (might be due to invalid IL or missing references)
			//IL_0108: Unknown result type (might be due to invalid IL or missing references)
			//IL_010c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0111: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_0126: Unknown result type (might be due to invalid IL or missing references)
			//IL_0128: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			SubmitItemUpdateResult_t result;
			try
			{
				Awaiter<SubmitItemUpdateResult_t> val2;
				if (num != 0)
				{
					_003C_003Ec__DisplayClass64_0 CS_0024_003C_003E8__locals12 = new _003C_003Ec__DisplayClass64_0();
					CS_0024_003C_003E8__locals12.handle = SteamUGC.StartItemUpdate(SteamUtils.GetAppID(), id);
					SteamUGC.SetItemTitle(CS_0024_003C_003E8__locals12.handle, item.metadata.name);
					SteamUGC.SetItemDescription(CS_0024_003C_003E8__locals12.handle, item.description);
					SteamUGC.SetItemContent(CS_0024_003C_003E8__locals12.handle, item.path);
					if (!string.IsNullOrEmpty(item.previewPath))
					{
						SteamUGC.SetItemPreview(CS_0024_003C_003E8__locals12.handle, item.previewPath);
					}
					if (isNew)
					{
						SteamUGC.SetItemVisibility(CS_0024_003C_003E8__locals12.handle, (ERemoteStoragePublishedFileVisibility)2);
					}
					SteamAPICall_t val = SteamUGC.SubmitItemUpdate(CS_0024_003C_003E8__locals12.handle, (string)null);
					CS_0024_003C_003E8__locals12.source = new UniTaskCompletionSource<SubmitItemUpdateResult_t>();
					CallResult<SubmitItemUpdateResult_t>.Create((APIDispatchDelegate<SubmitItemUpdateResult_t>)((SubmitItemUpdateResult_t res, bool failure) =>
					{
						//IL_0021: Unknown result type (might be due to invalid IL or missing references)
						if (failure)
						{
							CS_0024_003C_003E8__locals12.source.TrySetException((Exception)new SteamException("Creation failed for unknown reason"));
						}
						else
						{
							CS_0024_003C_003E8__locals12.source.TrySetResult(res);
						}
					})).Set(val, (APIDispatchDelegate<SubmitItemUpdateResult_t>)null);
					Coroutiner coroutiner = new GameObject().AddComponent<Coroutiner>();
					UnityEngine.Object.DontDestroyOnLoad(coroutiner);
					coroutiner.StartCoroutine(CS_0024_003C_003E8__locals12._003CUpdateItem_003Eg__Routine_007C1());
					val2 = CS_0024_003C_003E8__locals12.source.Task.GetAwaiter();
					if (!val2.IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val2;
						_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter<SubmitItemUpdateResult_t>, _003CUpdateItem_003Ed__64>(ref val2, ref this);
						return;
					}
				}
				else
				{
					val2 = _003C_003Eu__1;
					_003C_003Eu__1 = default;
					num = (_003C_003E1__state = -1);
				}
				result = val2.GetResult();
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003C_003Et__builder.SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003C_003Et__builder.SetResult(result);
		}

		void IAsyncStateMachine.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			this.MoveNext();
		}

		[DebuggerHidden]
		private void SetStateMachine(IAsyncStateMachine stateMachine)
		{
			_003C_003Et__builder.SetStateMachine(stateMachine);
		}

		void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
		{
			//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
			this.SetStateMachine(stateMachine);
		}
	}

	public static List<ModItem> allMods = new List<ModItem>();

	public static List<LoadedModInstance> loadedInstances = new List<LoadedModInstance>();

	public static SafeAction onModLoadStarted;

	public static SafeAction onModLoadEnded;

	private static List<FileSystemWatcher> _autoReloadWatchers = new List<FileSystemWatcher>();

	private static Coroutine _autoReloadCheckerRoutine;

	private static string _lastRequestedAutoReloadPath;

	private static float _lastReqeustedAutoReloadTime;

	private static readonly HashSet<string> AllowedPrefixesForJsonOverride = new HashSet<string>
	{
		"At_", "St_", "Ai_", "Se_", "Mon_", "Ge_", "Gem_", "Hero_", "LucidDream_", "RoomMod_",
		"Shrine_", "Treasure_"
	};

	private static readonly HashSet<Type> AllowedValueTypesForJsonOverride = new HashSet<Type>
	{
		typeof(int),
		typeof(float),
		typeof(bool),
		typeof(ScalingValue),
		typeof(StarScalingValue),
		typeof(Vector2),
		typeof(Vector3),
		typeof(Vector4),
		typeof(ElementalType),
		typeof(DamageData.SourceType),
		typeof(DamageAttribute),
		typeof(LevelScaling),
		typeof(string),
		typeof(BaseStats),
		typeof(BonusStats),
		typeof(NodeModifierVisibility),
		typeof(ModifierSpawnType),
		typeof(ScaleWithDifficultyMode),
		typeof(MapItemVisibility),
		typeof(int[]),
		typeof(float[]),
		typeof(bool[]),
		typeof(string[]),
		typeof(ScalingValue[]),
		typeof(StarScalingValue[]),
		typeof(Vector2[]),
		typeof(Vector3[]),
		typeof(Vector4[]),
		typeof(List<int>),
		typeof(List<float>),
		typeof(List<bool>),
		typeof(List<string>),
		typeof(List<ScalingValue>),
		typeof(List<StarScalingValue>),
		typeof(List<Vector2>),
		typeof(List<Vector3>),
		typeof(List<Vector4>),
		typeof(TriggerConfig),
		typeof(TriggerConfig[]),
		typeof(CastMethodData),
		typeof(HeroConstellationSettings),
		typeof(Dash),
		typeof(Knockback)
	};

	private static readonly HashSet<string> IgnoredFieldsJsonOverride = new HashSet<string> { "syncInterval", "budgetCost", "excludeFromPool" };

	public static Dictionary<string, Dictionary<string, string>> jsonOverrideTargets = new Dictionary<string, Dictionary<string, string>>();

	public static Dictionary<string, SafeAction<GameObject>> currentJsonOverrideProcessorsLocal = new Dictionary<string, SafeAction<GameObject>>();

	public static Dictionary<string, SafeAction<GameObject>> currentJsonOverrideProcessorsFromServer = new Dictionary<string, SafeAction<GameObject>>();

	public static List<JsonOverrideItem> allJsonOverrideItemsLocal = new List<JsonOverrideItem>();

	private static bool _isRegisteringJsonOverride = false;

	private static bool _willRegisterAcrossMultipleFrames = false;

	public static bool isGameplayAltered => loadedInstances.Any((LoadedModInstance i) => i.isAlteringGameplay);

	public static bool isAnyModActive
	{
		get
		{
			if (loadedInstances.Count <= 0)
			{
				if (DewSave.platformSettings != null)
				{
					return DewSave.platformSettings.activeMods.Any((string id) => allMods.Any((ModItem m) => m.state == ModItemState.Ok && m.metadata.id == id));
				}
				return false;
			}
			return true;
		}
	}

	public static bool isLoadingMod { get; private set; }

	public static bool isAutoReloadEnabled { get; private set; }

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void OnInit_Reset()
	{
		try
		{
			OnInit_JsonOverride();
		}
		catch (Exception exception)
		{
			UnityEngine.Debug.LogException(exception);
		}
		onModLoadStarted = null;
		onModLoadEnded = null;
		allMods = new List<ModItem>();
		loadedInstances = new List<LoadedModInstance>();
		isLoadingMod = false;
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	private static void OnInit()
	{
		Refresh();
		Application.quitting += OnQuit;
		DewSave.onSettingsChanged += new Action(OnSettingsChanged);
		Dew.GetCoroutiner().StartCoroutine(Routine());
		static IEnumerator Routine()
		{
			HashSet<string> disallowedScenes = new HashSet<string> { "Splash", "Intro" };
			while (disallowedScenes.Contains(SceneManager.GetActiveScene().name) || ManagerBase<TransitionManager>.instance.state == TransitionManager.StateType.Loading)
			{
				yield return null;
			}
			if (isAnyModActive)
			{
				DewSave.platformSettings.gameplay.enableCrossPlay = false;
			}
			if (DewSave.platformSettings.activeMods.Count != 0)
			{
				float remainingTime = 2f;
				ManagerBase<TransitionManager>.instance.SetBusy(value: true);
				string template = DewLocalization.GetUIValue("ModManager_AboutToLoadModCancelPrompt");
				while (remainingTime > 0f)
				{
					if (Input.GetKey(KeyCode.Q) || DewInput.GetButton((GamepadButtonEx?)GamepadButtonEx.North))
					{
						ManagerBase<TransitionManager>.instance.SetBusy(value: false);
						ManagerBase<MessageManager>.instance.ShowMessageLocalized("ModManager_LoadModCanceled");
						yield break;
					}
					ManagerBase<TransitionManager>.instance.UpdateLoadingStatus($"{string.Format(template, GamepadButtonEx.North.GetReadableText())}\n({remainingTime:#,##0.0})");
					remainingTime -= Time.unscaledDeltaTime;
					yield return null;
				}
				ManagerBase<TransitionManager>.instance.SetBusy(value: false);
				ReloadFromActiveMods();
			}
		}
	}

	private static void OnQuit()
	{
		Application.quitting -= OnQuit;
		UnloadAll();
		DisableAutoReload();
	}

	public static void Refresh()
	{
		allMods = new List<ModItem>();
		if (DewBuildProfile.current.HasFeature(BuildFeatureTag.Booth))
		{
			return;
		}
		foreach (string allLocalModDirectory in GetAllLocalModDirectories())
		{
			AddAllModsInDirectory(allLocalModDirectory);
		}
		if (DewBuildProfile.current.platform == PlatformType.STEAM && DewSteam.isInitialized)
		{
			AddSteamSubscribedMods();
		}
		allMods = GetSortedModsList(allMods);
	}

	public static List<string> GetAllLocalModDirectories()
	{
		List<string> list = new List<string>();
		if (!string.IsNullOrEmpty(DewLaunchOptions.modDir) && Check(DewLaunchOptions.modDir))
		{
			return list;
		}
		Check("../Mods");
		Check("Mods");
		return list;
		bool Check(string dir)
		{
			if (!Directory.Exists(dir))
			{
				return false;
			}
			list.Add(dir);
			return true;
		}
	}

	public static void ReloadFromActiveMods()
	{
		Dew.GetCoroutiner().StartCoroutine(Routine());
		static IEnumerator Routine()
		{
			if (loadedInstances.Count != 0 || DewSave.platformSettings.activeMods.Count != 0)
			{
				isLoadingMod = true;
				onModLoadStarted?.Invoke();
				ManagerBase<TransitionManager>.instance.SetBusy(value: true);
				ManagerBase<TransitionManager>.instance.UpdateLoadingStatus(DewLocalization.GetUIValue("Loading_LoadingMods") ?? "");
				yield return null;
				StartRegisterJsonOverride(willRegisterAcrossMultipleFrames: true);
				yield return null;
				UnloadAll();
				DewResources.RepairMissingReferences_Repair();
				yield return null;
				for (int i = 0; i < DewSave.platformSettings.activeMods.Count; i++)
				{
					string id = DewSave.platformSettings.activeMods[i];
					string text = id;
					ModItem modItem = allMods.Find((ModItem item) => item.metadata.id == id);
					if (modItem != null)
					{
						text = modItem.metadata.name;
					}
					ManagerBase<TransitionManager>.instance.UpdateLoadingStatus(string.Format("{0} ({1}/{2})\n{3}", DewLocalization.GetUIValue("Loading_LoadingMods"), i + 1, DewSave.platformSettings.activeMods.Count, text));
					yield return null;
					try
					{
						Load(id);
					}
					catch (Exception exception)
					{
						UnityEngine.Debug.LogException(exception);
					}
				}
				EndRegisterJsonOverride();
				ManagerBase<TransitionManager>.instance.SetBusy(value: false);
				isLoadingMod = false;
				onModLoadEnded?.Invoke();
				if (NetworkServer.active || NetworkClient.active)
				{
					ManagerBase<GlobalUIManager>.instance.ShowDevText(DewLocalization.GetUIValue("ModManager_WarningInGameOrLobby"));
				}
			}
		}
	}

	private static void AddSteamSubscribedMods()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		PublishedFileId_t[] array = new PublishedFileId_t[1024];
		uint subscribedItems = SteamUGC.GetSubscribedItems(array, (uint)array.Length);
		ulong num = default;
		string modPath = default;
		uint num2 = default;
		for (int i = 0; i < subscribedItems; i++)
		{
			EItemState val = (EItemState)SteamUGC.GetItemState(array[i]);
			if (((Enum)val).HasFlag((Enum)(object)(EItemState)4))
			{
				SteamUGC.GetItemInstallInfo(array[i], ref num, ref modPath, 1024u, ref num2);
				AddMod(modPath, ModSourceType.Steam, array[i].m_PublishedFileId);
			}
		}
	}

	public static void Load(string id)
	{
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		ModItem modItem = allMods.Find((ModItem item) => item.metadata.id == id && item.state == ModItemState.Ok);
		if (modItem == null)
		{
			return;
		}
		GameObject gameObject = new GameObject(modItem.metadata.id);
		gameObject.SetActive(value: false);
		UnityEngine.Object.DontDestroyOnLoad(gameObject);
		LoadedModInstance loadedModInstance = new LoadedModInstance
		{
			mod = modItem,
			container = gameObject
		};
		loadedInstances.Add(loadedModInstance);
		try
		{
			string text = Path.Join(string.op_Implicit(modItem.path), string.op_Implicit("overrides"));
			if (Directory.Exists(text))
			{
				loadedModInstance.isAlteringGameplay = true;
				RegisterJsonOverridesInDirectory(text);
			}
		}
		catch (Exception arg)
		{
			UnityEngine.Debug.LogError($"[DewMod] Failed to register JSON overrides for {modItem.metadata.id} due to exception: {arg}");
		}
		string[] assemblyPaths = modItem.assemblyPaths;
		foreach (string text2 in assemblyPaths)
		{
			if (text2.EndsWith(".dll"))
			{
				try
				{
					byte[] array = File.ReadAllBytes(text2);
					AssemblyDefinition val = AssemblyDefinition.ReadAssembly((Stream)new MemoryStream(array));
					try
					{
						AssemblyNameDefinition name = val.Name;
						((AssemblyNameReference)name).Name = ((AssemblyNameReference)name).Name + "_" + DateTime.Now.Ticks;
						using MemoryStream memoryStream = new MemoryStream();
						val.Write((Stream)memoryStream);
						array = memoryStream.ToArray();
					}
					finally
					{
						((IDisposable)val)?.Dispose();
					}
					foreach (Type item in from t in Assembly.Load(array).GetTypes()
						where t != typeof(ModBehaviour) && typeof(ModBehaviour).IsAssignableFrom(t)
						select t)
					{
						try
						{
							if (item.IsAbstract)
							{
								continue;
							}
							ModBehaviour modBehaviour = (ModBehaviour)gameObject.AddComponent(item);
							modBehaviour.instance = loadedModInstance;
							if (modBehaviour.modConfigFields.Length != 0)
							{
								try
								{
									modBehaviour.LoadConfigsToDisk();
								}
								catch (Exception exception)
								{
									UnityEngine.Debug.LogException(exception);
								}
							}
							MethodInfo[] methods = item.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
							foreach (MethodInfo methodInfo in methods)
							{
								Attribute[] array2 = methodInfo.GetCustomAttributes(typeof(ModBehaviour.ConsoleCommandAttribute)).ToArray();
								if (array2.Length != 0)
								{
									ModBehaviour.ConsoleCommandAttribute consoleCommandAttribute = (ModBehaviour.ConsoleCommandAttribute)array2[0];
									modBehaviour.RegisterConsoleCommand(methodInfo, consoleCommandAttribute.CustomName, consoleCommandAttribute.Description);
								}
							}
							UnityEngine.Debug.Log("[DewMod] Initialized " + modItem.metadata.id + "::" + item.Name);
						}
						catch (Exception arg2)
						{
							UnityEngine.Debug.LogError($"[DewMod] Failed to load {modItem.metadata.id}::{item.Name} due to exception: {arg2}");
						}
					}
				}
				catch (Exception arg3)
				{
					UnityEngine.Debug.LogError($"[DewMod] Failed to load {modItem.metadata.id} due to exception: {arg3}");
				}
			}
			else
			{
				UnityEngine.Debug.LogError("[DewMod] Unknown assembly type: " + modItem.metadata.id + "::" + Path.GetFileName(text2));
			}
		}
		UnityEngine.Debug.Log("[DewMod] Activating " + modItem.metadata.id);
		gameObject.SetActive(value: true);
	}

	private static void UnloadAll()
	{
		for (int num = loadedInstances.Count - 1; num >= 0; num--)
		{
			try
			{
				LoadedModInstance loadedModInstance = loadedInstances[num];
				if (!(loadedModInstance.container == null))
				{
					UnityEngine.Object.Destroy(loadedModInstance.container);
					foreach (string registeredCommand in loadedModInstance.registeredCommands)
					{
						DebugLogConsole.RemoveCommand(registeredCommand);
					}
					ModBehaviour[] components = loadedModInstance.container.GetComponents<ModBehaviour>();
					for (int i = 0; i < components.Length; i++)
					{
						foreach (ModBehaviour.ManagerPatchItem managerPatch in components[i]._managerPatches)
						{
							try
							{
								if (managerPatch.hasStarted)
								{
									managerPatch.hasStarted = false;
									managerPatch.onCleanup?.Invoke();
									managerPatch.onCleanup = null;
								}
							}
							catch (Exception exception)
							{
								UnityEngine.Debug.LogException(exception);
							}
						}
					}
				}
			}
			catch (Exception exception2)
			{
				UnityEngine.Debug.LogException(exception2);
			}
		}
		loadedInstances = new List<LoadedModInstance>();
		foreach (KeyValuePair<string, SafeAction<GameObject>> item in currentJsonOverrideProcessorsLocal)
		{
			DewResources.ClearVariantsOfAsset(item.Key, null, repairReferences: false);
		}
		currentJsonOverrideProcessorsLocal = new Dictionary<string, SafeAction<GameObject>>();
		allJsonOverrideItemsLocal = new List<JsonOverrideItem>();
	}

	internal static void NotifyManagerLifecycle(Type type, bool enable)
	{
		foreach (LoadedModInstance loadedInstance in loadedInstances)
		{
			ModBehaviour[] components = loadedInstance.container.GetComponents<ModBehaviour>();
			for (int i = 0; i < components.Length; i++)
			{
				foreach (ModBehaviour.ManagerPatchItem managerPatch in components[i]._managerPatches)
				{
					if (!managerPatch.type.IsAssignableFrom(type))
					{
						continue;
					}
					try
					{
						if (enable)
						{
							managerPatch.hasStarted = true;
							Action action = managerPatch.onStart?.Invoke();
							if (action != null)
							{
								managerPatch.onCleanup = (Action)Delegate.Combine(managerPatch.onCleanup, action);
							}
						}
						else
						{
							managerPatch.hasStarted = false;
							managerPatch.onCleanup?.Invoke();
							managerPatch.onCleanup = null;
						}
					}
					catch (Exception exception)
					{
						UnityEngine.Debug.LogException(exception);
					}
				}
			}
		}
	}

	private static void AddMod(string modPath, ModSourceType source, ulong publishedFileId = 0uL)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		string path = Path.Join(string.op_Implicit(modPath), string.op_Implicit("about"), string.op_Implicit("metadata.json"));
		if (!File.Exists(path) || Directory.Exists(Path.Join(string.op_Implicit(modPath), string.op_Implicit(".template.config"))))
		{
			return;
		}
		ModItem item = new ModItem
		{
			path = Path.GetFullPath(modPath),
			source = source,
			publishedFileId = publishedFileId
		};
		allMods.Add(item);
		try
		{
			string path2 = Path.Join(string.op_Implicit(modPath), string.op_Implicit("about"), string.op_Implicit("preview.png"));
			if (File.Exists(path2))
			{
				item.previewPath = Path.GetFullPath(path2);
			}
			string path3 = Path.Join(string.op_Implicit(modPath), string.op_Implicit("about"), string.op_Implicit("icon.png"));
			if (File.Exists(path3))
			{
				item.iconPath = Path.GetFullPath(path3);
				byte[] array = File.ReadAllBytes(item.iconPath);
				Texture2D texture2D = new Texture2D(2, 2);
				ImageConversion.LoadImage(texture2D, array);
				item.icon = texture2D;
			}
			string path4 = Path.Join(string.op_Implicit(modPath), string.op_Implicit("about"), string.op_Implicit("description.txt"));
			if (File.Exists(path4))
			{
				item.description = File.ReadAllText(path4);
				item.descriptionRichText = ConvertBBToRichText(item.description);
			}
			else
			{
				item.description = "No description provided.";
				item.descriptionRichText = "No description provided.";
			}
			string path5 = Path.Join(string.op_Implicit(modPath), string.op_Implicit("about"), string.op_Implicit("publishedfileid.txt"));
			if (File.Exists(path5) && item.publishedFileId == 0L)
			{
				item.publishedFileId = ulong.Parse(File.ReadAllText(path5));
			}
			item.metadata = JsonUtility.FromJson<ModMetadata>(File.ReadAllText(path));
			item.assemblyPaths = (from p in Directory.GetFiles(modPath, "*.*", SearchOption.AllDirectories)
				select ConvertToRelativeForwardSlashPath(p, modPath) into text
				where item.metadata.assemblies.Any((string pattern) => text.EqualsWildcard(pattern))
				select text).Distinct().Select((string rel) =>
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				return Path.GetFullPath(Path.Join(string.op_Implicit(modPath), string.op_Implicit(rel)));
			}).ToArray();
			AssertRequiredFieldMetadata("id");
			AssertRequiredFieldMetadata("name");
			AssertRequiredFieldMetadata("author");
			if (item.state == ModItemState.Ok && allMods.Find((ModItem m) => m.metadata.id == item.metadata.id && m != item) != null)
			{
				item.state = ModItemState.IdConflict;
			}
		}
		catch (Exception exception)
		{
			UnityEngine.Debug.LogException(exception);
			item.state = ModItemState.InvalidMetadata;
		}
		void AssertRequiredFieldMetadata(string fieldName)
		{
			FieldInfo field = item.metadata.GetType().GetField(fieldName);
			object value = field.GetValue(item.metadata);
			if (value == null || (value is string[] array2 && array2.Length == 0))
			{
				item.state = ModItemState.InvalidMetadata;
				if (field.FieldType == typeof(string))
				{
					field.SetValue(item, "???");
				}
			}
		}
	}

	private static void AddAllModsInDirectory(string modsDir)
	{
		if (Directory.Exists(modsDir))
		{
			string[] directories = Directory.GetDirectories(modsDir, "*", SearchOption.TopDirectoryOnly);
			for (int i = 0; i < directories.Length; i++)
			{
				AddMod(directories[i], ModSourceType.Local, 0uL);
			}
		}
	}

	public static List<ModItem> GetSortedModsList(List<ModItem> list)
	{
		return (from i in list
			orderby 0 - i.state, i.source, i.metadata.name
			select i).ToList();
	}

	private static string ConvertToRelativeForwardSlashPath(string fullPath, string parentPath)
	{
		if (!parentPath.EndsWith(Path.DirectorySeparatorChar.ToString()) && !parentPath.EndsWith(Path.AltDirectorySeparatorChar.ToString()))
		{
			parentPath += Path.DirectorySeparatorChar;
		}
		fullPath = Path.GetFullPath(fullPath);
		parentPath = Path.GetFullPath(parentPath);
		Uri uri = new Uri(parentPath);
		Uri uri2 = new Uri(fullPath);
		return Uri.UnescapeDataString(uri.MakeRelativeUri(uri2).ToString()).Replace('\\', '/');
	}

	private static string ConvertBBToRichText(string str)
	{
		if (string.IsNullOrEmpty(str))
		{
			return str;
		}
		str = ConvertOrderedLists(str);
		str = Regex.Replace(str, "\\[h1\\](.*?)\\[/h1\\]", "<color=white><size=150%><b>$1</b></size></color>", RegexOptions.IgnoreCase);
		str = Regex.Replace(str, "\\[h2\\](.*?)\\[/h2\\]", "<color=white><size=130%><b>$1</b></size></color>", RegexOptions.IgnoreCase);
		str = Regex.Replace(str, "\\[h3\\](.*?)\\[/h3\\]", "<color=white><size=115%><b>$1</b></size></color>", RegexOptions.IgnoreCase);
		str = Regex.Replace(str, "\\[h4\\](.*?)\\[/h4\\]", "<color=white><size=105%><b>$1</b></size></color>", RegexOptions.IgnoreCase);
		str = Regex.Replace(str, "\\[h5\\](.*?)\\[/h5\\]", "<color=white><size=100%><b>$1</b></size></color>", RegexOptions.IgnoreCase);
		str = Regex.Replace(str, "\\[h6\\](.*?)\\[/h6\\]", "<color=white><size=95%><b>$1</b></size></color>", RegexOptions.IgnoreCase);
		str = Regex.Replace(str, "\\[b\\](.*?)\\[/b\\]", "<b>$1</b>", RegexOptions.IgnoreCase);
		str = Regex.Replace(str, "\\[i\\](.*?)\\[/i\\]", "<i>$1</i>", RegexOptions.IgnoreCase);
		str = Regex.Replace(str, "\\[u\\](.*?)\\[/u\\]", "<u>$1</u>", RegexOptions.IgnoreCase);
		str = Regex.Replace(str, "\\[strike\\](.*?)\\[/strike\\]", "<s>$1</s>", RegexOptions.IgnoreCase);
		str = Regex.Replace(str, "\\[url=([^\\]]+)\\](.*?)\\[/url\\]", "$2", RegexOptions.IgnoreCase);
		str = Regex.Replace(str, "\\[url\\](.*?)\\[/url\\]", "$1", RegexOptions.IgnoreCase);
		str = Regex.Replace(str, "\\[spoiler\\](.*?)\\[/spoiler\\]", "$1", RegexOptions.IgnoreCase);
		str = ConvertUnorderedLists(str);
		str = Regex.Replace(str, "\\[hr\\]\\s*\\[/hr\\]", "", RegexOptions.IgnoreCase);
		str = Regex.Replace(str, "\\[hr/\\]", "", RegexOptions.IgnoreCase);
		str = Regex.Replace(str, "\\[hr\\]", "", RegexOptions.IgnoreCase);
		str = RemoveTables(str);
		str = Regex.Replace(str, "\\[img\\].*?\\[/img\\]", "", RegexOptions.IgnoreCase);
		str = Regex.Replace(str, "\\[IMG\\].*?\\[/IMG\\]", "", RegexOptions.IgnoreCase);
		str = Regex.Replace(str, "\\[/?[^\\]]*\\]", "");
		str = Regex.Replace(str, "\\n\\s*\\n\\s*\\n", "\n\n");
		str = str.Trim();
		return str;
	}

	private static string ConvertOrderedLists(string input)
	{
		return Regex.Replace(input, "\\[olist\\](.*?)\\[/olist\\]", (Match match) =>
		{
			MatchCollection matchCollection = Regex.Matches(match.Groups[1].Value, "\\[\\*\\]\\s*(.*?)(?=\\[\\*\\]|$)", RegexOptions.Singleline);
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < matchCollection.Count; i++)
			{
				string text = matchCollection[i].Groups[1].Value.Trim();
				if (!string.IsNullOrEmpty(text))
				{
					stringBuilder.AppendLine($"{i + 1}. {text}");
				}
			}
			return stringBuilder.ToString().TrimEnd();
		}, RegexOptions.IgnoreCase | RegexOptions.Singleline);
	}

	private static string ConvertUnorderedLists(string input)
	{
		return Regex.Replace(input, "\\[list\\](.*?)\\[/list\\]", (Match match) =>
		{
			MatchCollection matchCollection = Regex.Matches(match.Groups[1].Value, "\\[\\*\\]\\s*(.*?)(?=\\[\\*\\]|$)", RegexOptions.Singleline);
			StringBuilder stringBuilder = new StringBuilder();
			foreach (Match item in matchCollection)
			{
				string text = item.Groups[1].Value.Trim();
				if (!string.IsNullOrEmpty(text))
				{
					stringBuilder.AppendLine("• " + text);
				}
			}
			return stringBuilder.ToString().TrimEnd();
		}, RegexOptions.IgnoreCase | RegexOptions.Singleline);
	}

	private static string RemoveTables(string input)
	{
		return Regex.Replace(input, "\\[table\\](.*?)\\[/table\\]", (Match match) =>
		{
			string input2 = Regex.Replace(match.Groups[1].Value, "\\[/?t[hrd]\\]", " ", RegexOptions.IgnoreCase);
			input2 = Regex.Replace(input2, "\\s+", " ");
			input2 = input2.Trim();
			return (!string.IsNullOrEmpty(input2)) ? input2 : "";
		}, RegexOptions.IgnoreCase | RegexOptions.Singleline);
	}

	public static void OpenConfigWindow(LoadedModInstance mod)
	{
		UI_Window newWindow = UnityEngine.Object.Instantiate(DewGUI.widgetWindow, DewGUI.canvasTransform);
		newWindow.isDraggable = false;
		newWindow.enableBackdrop = true;
		newWindow.SetWidth(1350f);
		CustomLogicBehavior customLogicBehavior = newWindow.gameObject.AddComponent<CustomLogicBehavior>();
		customLogicBehavior.onFrameUpdate = (Action)Delegate.Combine(customLogicBehavior.onFrameUpdate, (Action)(() =>
		{
			if (isLoadingMod)
			{
				UnityEngine.Object.Destroy(newWindow.gameObject);
			}
		}));
		ScrollRect val = UnityEngine.Object.Instantiate<ScrollRect>(DewGUI.widgetScrollRect, newWindow.transform);
		VerticalLayoutGroup val2 = val.content.gameObject.AddComponent<VerticalLayoutGroup>();
		DewGUI.ApplyLayoutGroupDefaultSettings(val2, (TextAnchor)0);
		((LayoutGroup)val2).padding = new RectOffset(30, 30, 20, 20);
		val.SetExpandWidth<ScrollRect>(true);
		val.SetHeight<ScrollRect>(1000f);
		ModBehaviour[] components = mod.container.GetComponents<ModBehaviour>();
		SafeAction onReset = new SafeAction();
		SafeAction onRevert = new SafeAction();
		SafeAction onBeforeApply = new SafeAction();
		SafeAction onAfterApply = new SafeAction();
		SafeAction onDirty = new SafeAction();
		try
		{
			ModBehaviour[] array = components;
			foreach (ModBehaviour m in array)
			{
				FieldInfo[] modConfigFields = m.modConfigFields;
				foreach (FieldInfo fieldInfo in modConfigFields)
				{
					ModConfig config = (ModConfig)fieldInfo.GetValue(m);
					ModConfig localState = config.Clone();
					ModConfig defaultState = (ModConfig)Activator.CreateInstance(fieldInfo.FieldType);
					localState.BuildWidgets(val.content.transform, out var onChanged, out var requestUpdate);
					onChanged.Add(() =>
					{
						onDirty.Invoke();
					});
					onReset.Add(() =>
					{
						defaultState.CopyTo(localState);
						requestUpdate.Invoke();
					});
					onRevert.Add(() =>
					{
						config.CopyTo(localState);
						requestUpdate.Invoke();
					});
					onBeforeApply.Add(() =>
					{
						localState.CopyTo(config);
					});
				}
				onAfterApply.Add(() =>
				{
					m.OnConfigChanged();
					m.SaveConfigsToDisk();
				});
			}
		}
		catch (Exception exception)
		{
			UnityEngine.Debug.LogException(exception);
		}
		RefValue<bool> isDirty = new RefValue<bool>(v: false);
		HorizontalLayoutGroup val3 = DewGUI.CreateHorizontalLayoutGroup(newWindow.transform, (TextAnchor)4);
		((HorizontalOrVerticalLayoutGroup)val3).spacing = 30f;
		((LayoutGroup)val3).padding.top = 35;
		((LayoutGroup)val3).padding.left = 15;
		((LayoutGroup)val3).padding.right = 15;
		Button val4 = UnityEngine.Object.Instantiate<Button>(DewGUI.widgetButton, ((Component)(object)val3).transform).SetTextLocalized<Button>("Generic_Back");
		UI_BackButtonHandler uI_BackButtonHandler = ((Component)(object)val4).gameObject.AddComponent<UI_BackButtonHandler>();
		uI_BackButtonHandler.priority = 200;
		uI_BackButtonHandler.enabled = false;
		uI_BackButtonHandler.enabled = true;
		DewGUI.CreateHorizontalFlexibleSpace(((Component)(object)val3).transform);
		Button val5 = UnityEngine.Object.Instantiate<Button>(DewGUI.widgetButton, ((Component)(object)val3).transform).SetTextLocalized<Button>("Settings_ResetToDefaults");
		Button revertButton = UnityEngine.Object.Instantiate<Button>(DewGUI.widgetButton, ((Component)(object)val3).transform).SetTextLocalized<Button>("Settings_Undo");
		Button applyButton = UnityEngine.Object.Instantiate<Button>(DewGUI.widgetButton, ((Component)(object)val3).transform).SetTextLocalized<Button>("Settings_Apply");
		((UnityEvent)(object)val5.onClick).AddListener((UnityAction)(() =>
		{
			onReset.Invoke();
			isDirty.value = true;
			UpdateButtonStates();
		}));
		((UnityEvent)(object)revertButton.onClick).AddListener((UnityAction)(() =>
		{
			onRevert.Invoke();
			isDirty.value = false;
			UpdateButtonStates();
		}));
		((UnityEvent)(object)applyButton.onClick).AddListener((UnityAction)(() =>
		{
			onBeforeApply.Invoke();
			isDirty.value = false;
			UpdateButtonStates();
			onAfterApply.Invoke();
		}));
		((UnityEvent)(object)val4.onClick).AddListener((UnityAction)(() =>
		{
			if ((bool)isDirty)
			{
				ManagerBase<MessageManager>.instance.ShowMessage(new DewMessageSettings
				{
					rawContent = DewLocalization.GetUIValue("Settings_ConfirmUnsavedChanges"),
					buttons = (DewMessageSettings.ButtonType.Yes | DewMessageSettings.ButtonType.Cancel),
					defaultButton = DewMessageSettings.ButtonType.Cancel,
					onClose = (DewMessageSettings.ButtonType b) =>
					{
						if (b == DewMessageSettings.ButtonType.Yes)
						{
							UnityEngine.Object.Destroy(newWindow.gameObject);
						}
					}
				});
			}
			else
			{
				UnityEngine.Object.Destroy(newWindow.gameObject);
			}
		}));
		onDirty.Add(() =>
		{
			isDirty.value = true;
			UpdateButtonStates();
		});
		UpdateButtonStates();
		void UpdateButtonStates()
		{
			((Selectable)revertButton).interactable = isDirty;
			((Selectable)applyButton).interactable = isDirty;
		}
	}

	public static void EnableAutoReload()
	{
		if (isAutoReloadEnabled)
		{
			return;
		}
		isAutoReloadEnabled = true;
		_lastRequestedAutoReloadPath = null;
		ManagerBase<GlobalUIManager>.instance.ShowDevText(DewLocalization.GetUIValue("ModManager_AutoReload_Activated"));
		foreach (string allLocalModDirectory in GetAllLocalModDirectories())
		{
			FileSystemWatcher fileSystemWatcher = new FileSystemWatcher
			{
				Path = Path.GetFullPath(allLocalModDirectory),
				NotifyFilter = (NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.Size | NotifyFilters.LastWrite | NotifyFilters.CreationTime),
				Filter = "*.*",
				IncludeSubdirectories = true
			};
			fileSystemWatcher.Changed += HandleFileSystemEvent;
			fileSystemWatcher.Created += HandleFileSystemEvent;
			fileSystemWatcher.Deleted += HandleFileSystemEvent;
			fileSystemWatcher.Renamed += HandleFileSystemEvent;
			fileSystemWatcher.EnableRaisingEvents = true;
			_autoReloadWatchers.Add(fileSystemWatcher);
		}
		if (_autoReloadCheckerRoutine != null)
		{
			Dew.GetCoroutiner().StopCoroutine(_autoReloadCheckerRoutine);
			_autoReloadCheckerRoutine = null;
		}
		_lastReqeustedAutoReloadTime = 0f;
		_autoReloadCheckerRoutine = Dew.GetCoroutiner().StartCoroutine(CheckRoutine());
		static IEnumerator CheckRoutine()
		{
			while (true)
			{
				if (_lastRequestedAutoReloadPath != null)
				{
					_lastReqeustedAutoReloadTime = Time.unscaledTime;
					_lastRequestedAutoReloadPath = null;
				}
				if (_lastReqeustedAutoReloadTime != 0f && Time.unscaledTime - _lastReqeustedAutoReloadTime > 0.5f && !isLoadingMod && ManagerBase<TransitionManager>.instance.state == TransitionManager.StateType.Normal && Application.isFocused)
				{
					_lastReqeustedAutoReloadTime = 0f;
					Refresh();
					ReloadFromActiveMods();
				}
				yield return null;
			}
		}
	}

	private static void HandleFileSystemEvent(object sender, FileSystemEventArgs args)
	{
		_lastRequestedAutoReloadPath = args.FullPath;
	}

	public static void DisableAutoReload()
	{
		if (!isAutoReloadEnabled)
		{
			return;
		}
		isAutoReloadEnabled = false;
		ManagerBase<GlobalUIManager>.instance.ShowDevText(DewLocalization.GetUIValue("ModManager_AutoReload_Deactivated"));
		foreach (FileSystemWatcher autoReloadWatcher in _autoReloadWatchers)
		{
			autoReloadWatcher.EnableRaisingEvents = false;
			autoReloadWatcher.Dispose();
		}
		if (_autoReloadCheckerRoutine != null)
		{
			Dew.GetCoroutiner().StopCoroutine(_autoReloadCheckerRoutine);
			_autoReloadCheckerRoutine = null;
		}
		_autoReloadWatchers.Clear();
	}

	private static void OnSettingsChanged()
	{
		if (DewSave.platformSettings.gameplay.enableDeveloperMode && DewSave.platformSettings.enableAutoReloadMods)
		{
			EnableAutoReload();
		}
		else
		{
			DisableAutoReload();
		}
	}

	private static void OnInit_JsonOverride()
	{
		jsonOverrideTargets = new Dictionary<string, Dictionary<string, string>>();
		currentJsonOverrideProcessorsLocal = new Dictionary<string, SafeAction<GameObject>>();
		currentJsonOverrideProcessorsFromServer = new Dictionary<string, SafeAction<GameObject>>();
		allJsonOverrideItemsLocal = new List<JsonOverrideItem>();
		_isRegisteringJsonOverride = false;
		_willRegisterAcrossMultipleFrames = false;
	}

	public static void BuildJsonOverrideOriginal(string typeName)
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Expected Obj, but got Unknown
		try
		{
			if (string.IsNullOrEmpty(typeName))
			{
				return;
			}
			jsonOverrideTargets[typeName] = null;
			string valueOrDefault = CollectionExtensions.GetValueOrDefault<string, string>((IReadOnlyDictionary<string, string>)DewResources.database.typeNameToGuid, typeName);
			if (string.IsNullOrEmpty(valueOrDefault) || !AllowedPrefixesForJsonOverride.Any((string prefix) => typeName.StartsWith(prefix)))
			{
				return;
			}
			GameObject prefab = DewResources.Convert<GameObject>(DewResources.Load(valueOrDefault));
			Dictionary<string, object> dict = new Dictionary<string, object>();
			JsonSerializerSettings settings = new JsonSerializerSettings
			{
				ContractResolver = (IContractResolver)(object)new FieldsOnlyResolver(),
				ReferenceLoopHandling = (ReferenceLoopHandling)2
			};
			TryProcess<Actor>();
			TryProcess<EntityAI>();
			TryProcess<EntityStatus>();
			TryProcess<EntityControl>();
			if (dict.Count > 0)
			{
				JObject val = JObject.FromObject((object)dict);
				bool flag = true;
				while (flag)
				{
					flag = FlattenJObject(val);
				}
				jsonOverrideTargets[typeName] = ((JToken)val).ToObject<Dictionary<string, string>>();
			}
			void Process(Component component)
			{
				Dictionary<string, object> dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(JsonConvert.SerializeObject((object)component, settings));
				string text = ((component.GetType().Name != prefab.name) ? (component.GetType().Name + "::") : "");
				foreach (KeyValuePair<string, object> item in dictionary)
				{
					dict[text + item.Key] = item.Value;
				}
			}
			void TryProcess<T>() where T : Component
			{
				if (prefab.TryGetComponent<T>(out var component))
				{
					Process(component);
				}
			}
		}
		catch (Exception exception)
		{
			UnityEngine.Debug.LogException(exception);
		}
		static bool FlattenJObject(JObject target)
		{
			bool result = false;
			List<KeyValuePair<string, JToken>> list = new List<KeyValuePair<string, JToken>>();
			foreach (KeyValuePair<string, JToken> item2 in target)
			{
				list.Add(item2);
			}
			foreach (KeyValuePair<string, JToken> item3 in list)
			{
				JToken value = item3.Value;
				JObject val2 = (JObject)(object)((value is JObject) ? value : null);
				if (val2 != null)
				{
					result = true;
					target.Remove(item3.Key);
					foreach (KeyValuePair<string, JToken> item4 in val2)
					{
						target.Add(item3.Key + "." + item4.Key, item4.Value);
					}
				}
				JToken value2 = item3.Value;
				JArray val3 = (JArray)(object)((value2 is JArray) ? value2 : null);
				if (val3 != null)
				{
					result = true;
					target.Remove(item3.Key);
					for (int i = 0; i < ((JContainer)val3).Count; i++)
					{
						target.Add($"{item3.Key}.{i}", val3[i]);
					}
				}
			}
			return result;
		}
	}

	public static void BuildJsonOverrideOriginalsFull()
	{
		jsonOverrideTargets.Clear();
		foreach (KeyValuePair<string, string> item in DewResources.database.typeNameToGuid)
		{
			BuildJsonOverrideOriginal(item.Key);
		}
	}

	public static void StartRegisterJsonOverride(bool willRegisterAcrossMultipleFrames = false)
	{
		if (_isRegisteringJsonOverride)
		{
			UnityEngine.Debug.LogError("[DewMod-Override] Tried to start register json override twice");
			return;
		}
		DewResources.RepairMissingReferences_Prepare();
		ManagerBase<TransitionManager>.instance.SetBusy(value: true);
		_isRegisteringJsonOverride = true;
		_willRegisterAcrossMultipleFrames = willRegisterAcrossMultipleFrames;
	}

	public static void EndRegisterJsonOverride()
	{
		if (!_isRegisteringJsonOverride)
		{
			UnityEngine.Debug.LogError("[DewMod-Override] Tried to end register json override without starting one");
			return;
		}
		DewResources.RepairMissingReferences_Repair();
		ManagerBase<TransitionManager>.instance.SetBusy(value: false);
		_isRegisteringJsonOverride = false;
		_willRegisterAcrossMultipleFrames = false;
	}

	private static bool AssertDidStartRegisteringJsonOverride()
	{
		if (!_isRegisteringJsonOverride)
		{
			UnityEngine.Debug.LogError("[DewMod-Override] You must call StartRegisterJsonOverride before registering overrides, and call EndRegisterJsonOverride after everything has been registered.");
			return false;
		}
		return true;
	}

	public static void RegisterJsonOverridesInDirectory(string dir)
	{
		if (!AssertDidStartRegisteringJsonOverride())
		{
			return;
		}
		try
		{
			foreach (string item in Directory.EnumerateFiles(dir, "*.json", SearchOption.AllDirectories))
			{
				RegisterJsonOverride(item);
			}
		}
		catch (Exception arg)
		{
			UnityEngine.Debug.LogError($"[DewMod-Override] Loading override files in directory '{dir}' failed due to exception: {arg}");
		}
	}

	public static void RegisterJsonOverride(string path)
	{
		if (!AssertDidStartRegisteringJsonOverride())
		{
			return;
		}
		try
		{
			foreach (JsonOverrideItem item in JsonConvert.DeserializeObject<List<JsonOverrideItem>>(File.ReadAllText(path)))
			{
				RegisterJsonOverride(item);
			}
		}
		catch (Exception arg)
		{
			UnityEngine.Debug.LogError($"[DewMod-Override] Loading override file '{path}' failed due to exception: {arg}");
		}
	}

	public static void RegisterJsonOverride(JsonOverrideItem item, bool isFromServer = false)
	{
		if (!AssertDidStartRegisteringJsonOverride())
		{
			return;
		}
		if (string.IsNullOrEmpty(item.target))
		{
			UnityEngine.Debug.LogError("[DewMod-Override] Tried to register override with null or empty target");
			return;
		}
		if (!jsonOverrideTargets.ContainsKey(item.target))
		{
			BuildJsonOverrideOriginal(item.target);
		}
		if (!jsonOverrideTargets.TryGetValue(item.target, out var value) || value == null)
		{
			UnityEngine.Debug.LogError("[DewMod-Override] " + item.target + " is not a valid register override target");
			return;
		}
		Type typeFromShortName = Dew.GetTypeFromShortName(item.target);
		if (typeFromShortName == null)
		{
			UnityEngine.Debug.LogError("[DewMod-Override] While trying to register, resolve type for '" + item.target + "' failed");
			return;
		}
		if (!DewResources.database.typeNameToGuid.TryGetValue(item.target, out var value2))
		{
			UnityEngine.Debug.LogError("[DewMod-Override] While trying to register, find guid for '" + item.target + "' failed");
			return;
		}
		DewResources.ClearVariantsOfAsset(value2, null, repairReferences: false);
		Dictionary<string, SafeAction<GameObject>> dictionary = (isFromServer ? currentJsonOverrideProcessorsFromServer : currentJsonOverrideProcessorsLocal);
		if (!dictionary.TryGetValue(item.target, out var value3))
		{
			value3 = (dictionary[value2] = new SafeAction<GameObject>());
		}
		if (!isFromServer)
		{
			allJsonOverrideItemsLocal.Add(item);
		}
		foreach (KeyValuePair<string, string> @override in item.overrides)
		{
			try
			{
				if (!value.ContainsKey(@override.Key))
				{
					UnityEngine.Debug.LogError("[DewMod-Override] " + item.target + "::" + @override.Key + " is not a valid register override key.");
					continue;
				}
				string key = @override.Key;
				if (!key.Contains("::"))
				{
					key = typeFromShortName.Name + "::" + key;
				}
				string[] array = key.Split(new string[1] { "::" }, StringSplitOptions.None);
				string text = array[0];
				string fieldPath = array[1];
				Type componentType = Dew.GetTypeFromShortName(text);
				if (componentType == null)
				{
					UnityEngine.Debug.LogError("[DewMod-Override] Could not find type '" + text + "' for override key '" + key + "'.");
					continue;
				}
				string capturedOverrideValue = @override.Value;
				value3 += (Action<GameObject>)((GameObject gobj) =>
				{
					try
					{
						Component component = gobj.GetComponent(componentType);
						if (component == null)
						{
							UnityEngine.Debug.LogWarning("[DewMod-Override] Component '" + componentType.Name + "' not found on '" + gobj.name + "' when applying override for '" + key + "'. Skipping.");
						}
						else
						{
							JsonOverrideHelper.Apply(component, fieldPath, capturedOverrideValue);
						}
					}
					catch (Exception arg)
					{
						UnityEngine.Debug.LogError($"[DewMod-Override] Runtime error applying override '{key}' to '{gobj.name}': {arg}");
					}
				});
			}
			catch (Exception ex)
			{
				UnityEngine.Debug.LogError("[DewMod-Override] Registration failed for " + item.target + "::" + @override.Key + " due to an exception: " + ex.Message);
			}
		}
		if (_willRegisterAcrossMultipleFrames)
		{
			DewResources.RepairMissingReferences_Repair();
		}
	}

	[AsyncStateMachine(typeof(_003CCreateItem_003Ed__63))]
	public static UniTask<CreateItemResult_t> CreateItem()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		_003CCreateItem_003Ed__63 obj = default;
		obj._003C_003Et__builder = AsyncUniTaskMethodBuilder<CreateItemResult_t>.Create();
		obj._003C_003E1__state = -1;
		obj._003C_003Et__builder.Start<_003CCreateItem_003Ed__63>(ref obj);
		return obj._003C_003Et__builder.Task;
	}

	[AsyncStateMachine(typeof(_003CUpdateItem_003Ed__64))]
	public static UniTask<SubmitItemUpdateResult_t> UpdateItem(PublishedFileId_t id, ModItem item, bool isNew)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		_003CUpdateItem_003Ed__64 obj = default;
		obj._003C_003Et__builder = AsyncUniTaskMethodBuilder<SubmitItemUpdateResult_t>.Create();
		obj.id = id;
		obj.item = item;
		obj.isNew = isNew;
		obj._003C_003E1__state = -1;
		obj._003C_003Et__builder.Start<_003CUpdateItem_003Ed__64>(ref obj);
		return obj._003C_003Et__builder.Task;
	}

	[AsyncStateMachine(typeof(_003CDeleteItem_003Ed__65))]
	public static UniTask<DeleteItemResult_t> DeleteItem(PublishedFileId_t id)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		_003CDeleteItem_003Ed__65 obj = default;
		obj._003C_003Et__builder = AsyncUniTaskMethodBuilder<DeleteItemResult_t>.Create();
		obj.id = id;
		obj._003C_003E1__state = -1;
		obj._003C_003Et__builder.Start<_003CDeleteItem_003Ed__65>(ref obj);
		return obj._003C_003Et__builder.Task;
	}

	private static string FormatFileSize(ulong bytes)
	{
		if (bytes >= 1048576)
		{
			return $"{(double)bytes / 1048576.0:F2} MB";
		}
		if (bytes >= 1024)
		{
			return $"{(double)bytes / 1024.0:F2} KB";
		}
		return $"{bytes} B";
	}
}
