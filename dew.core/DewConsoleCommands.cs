using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using BRGInstancedRenderer;
using DewInternal;
using Epic.OnlineServices;
using Epic.OnlineServices.Auth;
using Epic.OnlineServices.Connect;
using EpicTransport;
using IngameDebugConsole;
using Mirror;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityJSON;
using VolumetricFogAndMist2;

public static class DewConsoleCommands
{
	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static Func<Entity, float> _003C_003E9__22_0;

		public static Func<KeyValuePair<Type, string>, string> _003C_003E9__59_1;

		public static Func<KeyValuePair<string, string>, string> _003C_003E9__59_3;

		public static Action<string> _003C_003E9__59_5;

		public static Func<KeyValuePair<Type, string>, string> _003C_003E9__60_1;

		public static Func<KeyValuePair<string, string>, string> _003C_003E9__60_3;

		public static Action<string> _003C_003E9__60_5;

		public static OnDeleteDeviceIdCallback _003C_003E9__64_0;

		public static OnDeletePersistentAuthCallback _003C_003E9__64_1;

		public static Action<DewMessageSettings.ButtonType> _003C_003E9__76_0;

		public static Func<Terrain, bool> _003C_003E9__103_0;

		internal float _003CSelNewest_003Eb__22_0(Entity e)
		{
			return e.creationTime;
		}

		internal string _003CResLoad_003Eb__59_1(KeyValuePair<Type, string> p)
		{
			return p.Value;
		}

		internal string _003CResLoad_003Eb__59_3(KeyValuePair<string, string> p)
		{
			return p.Value;
		}

		internal void _003CResLoad_003Eb__59_5(string guid)
		{
			DewResources.Load(guid);
		}

		internal string _003CResPreload_003Eb__60_1(KeyValuePair<Type, string> p)
		{
			return p.Value;
		}

		internal string _003CResPreload_003Eb__60_3(KeyValuePair<string, string> p)
		{
			return p.Value;
		}

		internal void _003CResPreload_003Eb__60_5(string guid)
		{
			DewResources.Preload(guid);
		}

		internal void _003CEOSDeletePersistentAuth_003Eb__64_0(ref DeleteDeviceIdCallbackInfo data)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Expected Obj, but got Unknown
			Log($"Delete device id returned: {data.ResultCode}");
			Log("Trying to create device id");
			CreateDeviceIdOptions val = default;
			val.DeviceModel = Utf8String.op_Implicit(EOSSDKComponent.Instance.deviceModel);
			EOSSDKComponent.GetConnectInterface().CreateDeviceId(ref val, (object)null, (OnCreateDeviceIdCallback)EOSSDKComponent.Instance.OnCreateDeviceId);
		}

		internal void _003CEOSDeletePersistentAuth_003Eb__64_1(ref DeletePersistentAuthCallbackInfo data)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			Log($"Delete persistent auth returned: {data.ResultCode}");
		}

		internal void _003CMessage_003Eb__76_0(DewMessageSettings.ButtonType b)
		{
			Debug.Log("Received " + b);
		}

		internal bool _003CToggleTerrainBRG_003Eb__103_0(Terrain t)
		{
			return (UnityEngine.Object)(object)((Component)(object)t).GetComponent<TerrainBRGRegisterer>() != null;
		}
	}

	public static bool bossRushMode;

	[ConsoleMethod("autoexec", "Add command to auto-exec of certain key", new string[] { })]
	public static void AutoExec(string key, string command)
	{
		if (TryResolveAutoExecKey(key, out var key2))
		{
			string text = PlayerPrefs.GetString("AutoExec_" + key2, "");
			text = text + "\n" + command;
			PlayerPrefs.SetString("AutoExec_" + key2, text);
			PlayerPrefs.Save();
			Log($"Added auto-exec \"{command}\" to {key2}");
			AutoExecList();
		}
	}

	[ConsoleMethod("autoexeclist", "List commands in all auto-exec", new string[] { })]
	public static void AutoExecList()
	{
		Enum.GetNames(typeof(ConsoleManager.AutoExecKey));
		ConsoleManager.AutoExecKey[] array = (ConsoleManager.AutoExecKey[])Enum.GetValues(typeof(ConsoleManager.AutoExecKey));
		for (int i = 0; i < array.Length; i++)
		{
			ConsoleManager.AutoExecKey autoExecKey = array[i];
			string text = PlayerPrefs.GetString("AutoExec_" + autoExecKey, "");
			Log($"Auto-exec of key {autoExecKey}:");
			string[] array2 = text.Split("\n", StringSplitOptions.None);
			foreach (string text2 in array2)
			{
				if (!string.IsNullOrWhiteSpace(text2.Trim()))
				{
					Log(" - " + text2);
				}
			}
		}
	}

	[ConsoleMethod("autoexecclear", "Clear auto-exec of certain key", new string[] { })]
	public static void AutoExecClear(string key)
	{
		if (TryResolveAutoExecKey(key, out var key2))
		{
			PlayerPrefs.SetString("AutoExec_" + key2, "");
			PlayerPrefs.Save();
			Log("Cleared auto-exec of " + key2);
		}
	}

	[ConsoleMethod("autoexecclearall", "Clear auto-exec of all keys", new string[] { })]
	public static void AutoExecClearAll()
	{
		ConsoleManager.AutoExecKey[] array = (ConsoleManager.AutoExecKey[])Enum.GetValues(typeof(ConsoleManager.AutoExecKey));
		ConsoleManager.AutoExecKey[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			ConsoleManager.AutoExecKey autoExecKey = array2[i];
			PlayerPrefs.SetString("AutoExec_" + autoExecKey, "");
		}
		PlayerPrefs.Save();
		Log("Cleared all auto-exec: " + array.JoinToString(", "));
	}

	private static bool TryResolveAutoExecKey(string str, out ConsoleManager.AutoExecKey key)
	{
		string[] names = Enum.GetNames(typeof(ConsoleManager.AutoExecKey));
		ConsoleManager.AutoExecKey[] array = (ConsoleManager.AutoExecKey[])Enum.GetValues(typeof(ConsoleManager.AutoExecKey));
		for (int i = 0; i < names.Length; i++)
		{
			if (names[i].Equals(str, StringComparison.InvariantCultureIgnoreCase))
			{
				key = array[i];
				return true;
			}
		}
		Log("Invalid key provided, possible keys: " + names.JoinToString(", "));
		key = ConsoleManager.AutoExecKey.Global;
		return false;
	}

	public static void Log(object message)
	{
		string message2 = message.ToString();
		if ((UnityEngine.Object)(object)NetworkedManagerBase<ConsoleManager>.instance == null || (UnityEngine.Object)(object)NetworkedManagerBase<ConsoleManager>.instance.executionContext.player == null)
		{
			Debug.Log(message2);
		}
		else
		{
			NetworkedManagerBase<ConsoleManager>.instance.executionContext.player.SendLog(message2);
		}
	}

	public static void LogWarning(object message)
	{
		string message2 = message.ToString();
		if ((UnityEngine.Object)(object)NetworkedManagerBase<ConsoleManager>.instance == null || (UnityEngine.Object)(object)NetworkedManagerBase<ConsoleManager>.instance.executionContext.player == null)
		{
			Debug.Log(message2);
		}
		else
		{
			NetworkedManagerBase<ConsoleManager>.instance.executionContext.player.SendLogWarning(message2);
		}
	}

	public static void LogError(object message)
	{
		string message2 = message.ToString();
		if ((UnityEngine.Object)(object)NetworkedManagerBase<ConsoleManager>.instance == null || (UnityEngine.Object)(object)NetworkedManagerBase<ConsoleManager>.instance.executionContext.player == null)
		{
			Debug.Log(message2);
		}
		else
		{
			NetworkedManagerBase<ConsoleManager>.instance.executionContext.player.SendLogError(message2);
		}
	}

	public static void PrintNoEntitySelected()
	{
		Log("No entity selected");
	}

	public static void PrintNoEntityControlled()
	{
		Log("No entity controlled");
	}

	public static Entity GetConsoleSelectedEntity()
	{
		if ((UnityEngine.Object)(object)NetworkedManagerBase<ConsoleManager>.instance.executionContext.player != null)
		{
			return NetworkedManagerBase<ConsoleManager>.instance.executionContext.selection;
		}
		return NetworkedManagerBase<ConsoleManager>.instance.localSelectedEntity;
	}

	public static bool TryGetConsoleSelectedEntity(out Entity ent)
	{
		ent = (((UnityEngine.Object)(object)NetworkedManagerBase<ConsoleManager>.instance.executionContext.player != null) ? NetworkedManagerBase<ConsoleManager>.instance.executionContext.selection : NetworkedManagerBase<ConsoleManager>.instance.localSelectedEntity);
		return (UnityEngine.Object)(object)ent != null;
	}

	public static Entity GetControllingEntity()
	{
		if ((UnityEngine.Object)(object)NetworkedManagerBase<ConsoleManager>.instance.executionContext.player != null)
		{
			return NetworkedManagerBase<ConsoleManager>.instance.executionContext.player.controllingEntity;
		}
		return ManagerBase<ControlManager>.instance.controllingEntity;
	}

	public static Vector3 GetCursorWorldPos()
	{
		if ((UnityEngine.Object)(object)NetworkedManagerBase<ConsoleManager>.instance.executionContext.player == null)
		{
			return ControlManager.GetWorldPositionOnGroundOnCursor();
		}
		return NetworkedManagerBase<ConsoleManager>.instance.executionContext.cursorWorldPos;
	}

	public static DewPlayer GetPlayer()
	{
		if ((UnityEngine.Object)(object)NetworkedManagerBase<ConsoleManager>.instance.executionContext.player == null)
		{
			return DewPlayer.local;
		}
		return NetworkedManagerBase<ConsoleManager>.instance.executionContext.player;
	}

	[DewConsoleMethod(/*Could not decode attribute arguments.*/)]
	public static void Bind(string key, string command)
	{
		ConsoleBindItemType consoleBindItemType = ConsoleBindItemType.Down;
		if (key.StartsWith("+"))
		{
			key = key.Substring(1);
			consoleBindItemType = ConsoleBindItemType.Down;
		}
		else if (key.StartsWith("-"))
		{
			key = key.Substring(1);
			consoleBindItemType = ConsoleBindItemType.Up;
		}
		if (TryResolveKeyCode(key, out var code))
		{
			NetworkedManagerBase<ConsoleManager>.instance.activeCommandBinds.Add(new ConsoleBindItem
			{
				type = consoleBindItemType,
				key = code,
				command = command
			});
			Log(string.Format("Added bind {0}{1}: {2}", (consoleBindItemType == ConsoleBindItemType.Down) ? "+" : "-", code, command));
		}
	}

	[DewConsoleMethod(/*Could not decode attribute arguments.*/)]
	public static void BindList()
	{
		if (NetworkedManagerBase<ConsoleManager>.instance.activeCommandBinds.Count == 0)
		{
			Log("No console command binds are active.");
			return;
		}
		Log($"Currently {NetworkedManagerBase<ConsoleManager>.instance.activeCommandBinds.Count} console command binds are active.");
		foreach (ConsoleBindItem activeCommandBind in NetworkedManagerBase<ConsoleManager>.instance.activeCommandBinds)
		{
			Log(string.Format(" - {0}{1}: {2}", (activeCommandBind.type == ConsoleBindItemType.Down) ? "+" : "-", activeCommandBind.key, activeCommandBind.command));
		}
	}

	[DewConsoleMethod(/*Could not decode attribute arguments.*/)]
	public static void Unbind(string key)
	{
		ConsoleBindItemType consoleBindItemType = ConsoleBindItemType.Down;
		if (key.StartsWith("+"))
		{
			key = key.Substring(1);
			consoleBindItemType = ConsoleBindItemType.Down;
		}
		else if (key.StartsWith("-"))
		{
			key = key.Substring(1);
			consoleBindItemType = ConsoleBindItemType.Up;
		}
		if (!TryResolveKeyCode(key, out var code))
		{
			return;
		}
		bool flag = false;
		for (int num = NetworkedManagerBase<ConsoleManager>.instance.activeCommandBinds.Count - 1; num >= 0; num--)
		{
			ConsoleBindItem consoleBindItem = NetworkedManagerBase<ConsoleManager>.instance.activeCommandBinds[num];
			if (consoleBindItem.key == code && consoleBindItem.type == consoleBindItemType)
			{
				flag = true;
				Log(string.Format("Removing {0}{1}: {2}", (consoleBindItem.type == ConsoleBindItemType.Down) ? "+" : "-", consoleBindItem.key, consoleBindItem.command));
				NetworkedManagerBase<ConsoleManager>.instance.activeCommandBinds.RemoveAt(num);
			}
		}
		if (!flag)
		{
			Log(string.Format("Could not find any binds on {0}{1}", (consoleBindItemType == ConsoleBindItemType.Down) ? "+" : "-", code));
		}
	}

	[DewConsoleMethod(/*Could not decode attribute arguments.*/)]
	public static void UnbindAll()
	{
		if (NetworkedManagerBase<ConsoleManager>.instance.activeCommandBinds.Count == 0)
		{
			Log("No console command binds to remove.");
			return;
		}
		Log($"Removed {NetworkedManagerBase<ConsoleManager>.instance.activeCommandBinds.Count} console command binds.");
		NetworkedManagerBase<ConsoleManager>.instance.activeCommandBinds.Clear();
	}

	private static bool TryResolveKeyCode(string str, out KeyCode code)
	{
		string[] names = Enum.GetNames(typeof(KeyCode));
		KeyCode[] array = (KeyCode[])Enum.GetValues(typeof(KeyCode));
		for (int i = 0; i < names.Length; i++)
		{
			if (names[i].Equals(str, StringComparison.InvariantCultureIgnoreCase))
			{
				code = array[i];
				return true;
			}
		}
		code = KeyCode.None;
		List<string> list = new List<string>();
		for (int j = 0; j < names.Length; j++)
		{
			if (names[j].Contains(str, StringComparison.InvariantCultureIgnoreCase))
			{
				list.Add(names[j]);
			}
		}
		if (list.Count == 0)
		{
			Log("Cannot find requested keycode.");
		}
		else
		{
			Log("Cannot find requested keycode, did you mean: " + list.JoinToString(", "));
		}
		return false;
	}

	[DewConsoleMethod(/*Could not decode attribute arguments.*/)]
	public static void TestMonsterSpawn(int count)
	{
		if (GetPlayer().hero.section == null)
		{
			Log("Need to be on section");
			return;
		}
		Mon_RedGiant byType = DewResources.GetByType<Mon_RedGiant>(default(ResourceLoadSettings));
		for (int i = 0; i < count; i++)
		{
			Vector3 item = SingletonDewNetworkBehaviour<Room>.instance.monsters.GetSpawnMonsterPosRot(new SpawnMonsterSettings
			{
				rule = SingletonDewNetworkBehaviour<Room>.instance.monsters.defaultRule,
				section = GetPlayer().hero.section
			}, byType).Item1;
			Debug.DrawLine(item, item + Vector3.up * 5f, Color.green, 3f);
		}
	}

	[DewConsoleMethod(/*Could not decode attribute arguments.*/)]
	public static void Sel(string substr)
	{
		foreach (Entity item in NetworkedManagerBase<ActorManager>.instance.allEntities.Reverse())
		{
			if (((object)item).GetType().Name.Contains(substr, StringComparison.InvariantCultureIgnoreCase))
			{
				NetworkedManagerBase<ConsoleManager>.instance.localSelectedEntity = item;
				Log("Selected " + item.GetActorReadableName());
				return;
			}
		}
		Log("No entity found with substr: " + substr);
	}

	[DewConsoleMethod(/*Could not decode attribute arguments.*/)]
	public static void SelNewest()
	{
		Entity entity = Dew.SelectBestWithScore(NetworkedManagerBase<ActorManager>.instance.allEntities, (Entity e) => e.creationTime);
		NetworkedManagerBase<ConsoleManager>.instance.localSelectedEntity = entity;
		Log("Selected " + entity.GetActorReadableName());
	}

	[DewConsoleMethod(/*Could not decode attribute arguments.*/)]
	public static void Sel()
	{
		Hero hero = DewPlayer.local.hero;
		NetworkedManagerBase<ConsoleManager>.instance.localSelectedEntity = hero;
		Log("Selected " + hero.GetActorReadableName());
	}

	[DewConsoleMethod(/*Could not decode attribute arguments.*/)]
	public static void RendererEnable()
	{
		if (!TryGetConsoleSelectedEntity(out var ent))
		{
			PrintNoEntitySelected();
		}
		else
		{
			ent.Visual.EnableRenderersLocal();
		}
	}

	[DewConsoleMethod(/*Could not decode attribute arguments.*/)]
	public static void RendererDisable()
	{
		if (!TryGetConsoleSelectedEntity(out var ent))
		{
			PrintNoEntitySelected();
		}
		else
		{
			ent.Visual.DisableRenderersLocal();
		}
	}

	[DewConsoleMethod("Show information about current lobby")]
	public static void LobbyInfo()
	{
		if (ManagerBase<LobbyManager>.instance.service.currentLobby == null)
		{
			Log("Not in any lobby");
		}
		else if (ManagerBase<LobbyManager>.instance.service.currentLobby != null)
		{
			Log("- " + ManagerBase<LobbyManager>.instance.service.currentLobby.GetType().Name);
			Type type = ManagerBase<LobbyManager>.instance.service.currentLobby.GetType();
			PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
			FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public);
			PropertyInfo[] array = properties;
			foreach (PropertyInfo propertyInfo in array)
			{
				Debug.Log($"{propertyInfo.Name}: {propertyInfo.GetValue(ManagerBase<LobbyManager>.instance.service.currentLobby)}");
			}
			FieldInfo[] array2 = fields;
			foreach (FieldInfo fieldInfo in array2)
			{
				Debug.Log($"{fieldInfo.Name}: {fieldInfo.GetValue(ManagerBase<LobbyManager>.instance.service.currentLobby)}");
			}
		}
	}

	[DewConsoleMethod("Show all matching localization data")]
	public static void Loc(string pattern)
	{
		Log("All matches for: " + pattern);
		Log("===============================");
		foreach (KeyValuePair<string, AchievementData> achievement in DewLocalization.data.achievements)
		{
			if (achievement.Key.EqualsWildcard(pattern, ignoreCase: true))
			{
				Log("Achievements::" + achievement.Key + ": " + achievement.Value.name);
			}
		}
		foreach (KeyValuePair<string, ArtifactData> artifact in DewLocalization.data.artifacts)
		{
			if (artifact.Key.EqualsWildcard(pattern, ignoreCase: true))
			{
				Log("Artifacts::" + artifact.Key + ": " + artifact.Value.name);
			}
		}
		foreach (KeyValuePair<string, SkillData> skill in DewLocalization.data.skills)
		{
			if (skill.Key.EqualsWildcard(pattern, ignoreCase: true))
			{
				Log("Skills::" + skill.Key + ": " + skill.Value.configs[0].name);
			}
		}
		foreach (KeyValuePair<string, ConversationData> conversation in DewLocalization.data.conversations)
		{
			if (conversation.Key.EqualsWildcard(pattern, ignoreCase: true))
			{
				Log($"Conversations::{conversation.Key}: {conversation.Value.lines[0]}");
			}
		}
		foreach (KeyValuePair<string, CurseData> curse in DewLocalization.data.curses)
		{
			if (curse.Key.EqualsWildcard(pattern, ignoreCase: true))
			{
				Log("Curses::" + curse.Key + ": " + curse.Value.name);
			}
		}
		foreach (KeyValuePair<string, GemData> gem in DewLocalization.data.gems)
		{
			if (gem.Key.EqualsWildcard(pattern, ignoreCase: true))
			{
				Log("Gems::" + gem.Key + ": " + gem.Value.name);
			}
		}
		foreach (KeyValuePair<string, StarData> star in DewLocalization.data.stars)
		{
			if (star.Key.EqualsWildcard(pattern, ignoreCase: true))
			{
				Log("Stars::" + star.Key + ": " + star.Value.name);
			}
		}
		foreach (KeyValuePair<string, string> tip in DewLocalization.data.tips)
		{
			if (tip.Key.EqualsWildcard(pattern, ignoreCase: true))
			{
				Log("Tips::" + tip.Key + ": " + tip.Value);
			}
		}
		foreach (KeyValuePair<string, TreasureData> treasure in DewLocalization.data.treasures)
		{
			if (treasure.Key.EqualsWildcard(pattern, ignoreCase: true))
			{
				Log("Treasures::" + treasure.Key + ": " + treasure.Value.name);
			}
		}
		foreach (KeyValuePair<string, string> item in DewLocalization.data.ui)
		{
			if (item.Key.EqualsWildcard(pattern, ignoreCase: true))
			{
				Log("UIs::" + item.Key + ": " + item.Value);
			}
		}
		Log("===============================");
	}

	[DewConsoleMethod(/*Could not decode attribute arguments.*/)]
	public static void NodeInfo()
	{
		Log(NetworkedManagerBase<ZoneManager>.instance.currentZone.name ?? "");
		Log($"- Index: {NetworkedManagerBase<ZoneManager>.instance.currentNodeIndex}");
		Log($"- Type: {NetworkedManagerBase<ZoneManager>.instance.currentNode.type}");
		Log($"- Status: {NetworkedManagerBase<ZoneManager>.instance.currentNode.status}");
		Log($"- Is Hunted: {NetworkedManagerBase<ZoneManager>.instance.isCurrentNodeHunted}");
		Log("- Room: " + NetworkedManagerBase<ZoneManager>.instance.currentNode.room);
		Log("- Room Override: " + NetworkedManagerBase<ZoneManager>.instance.currentNode.room);
		Log($"- Room Rot: {ManagerBase<CameraManager>.instance.entityCamAngle}°");
		Log($"- RoomArea: {SingletonDewNetworkBehaviour<Room>.instance.map.mapData.area}");
		Log($"- Mods: {NetworkedManagerBase<ZoneManager>.instance.currentNode.modifiers.Count} modifiers");
		foreach (ModifierData modifier in NetworkedManagerBase<ZoneManager>.instance.currentNode.modifiers)
		{
			Log("  - " + modifier.type + ": " + modifier.clientData);
		}
	}

	[DewConsoleMethod("Reset Good Prop Position Test")]
	public static void TestPropPosReset()
	{
		foreach (RoomSection section in SingletonDewNetworkBehaviour<Room>.instance.sections)
		{
			section.ResetUsedNodeIndices();
		}
		Transform[] array = UnityEngine.Object.FindObjectsOfType<Transform>();
		foreach (Transform transform in array)
		{
			if (!(transform.name != "TEST PROP POS"))
			{
				UnityEngine.Object.Destroy(transform.gameObject);
			}
		}
		Log("Reset prop position indices and removed test markers");
	}

	[DewConsoleMethod(/*Could not decode attribute arguments.*/)]
	public static void TestShrines()
	{
		if (!TryGetConsoleSelectedEntity(out var ent))
		{
			PrintNoEntitySelected();
		}
		else if (ent.section == null)
		{
			Log("Selected entity needs to be on a section");
		}
	}

	[DewConsoleMethod("Test Good Prop Position")]
	public static void TestPropPos(int count)
	{
		Log($"Testing {count} positions");
		for (int i = 0; i < count; i++)
		{
			bool flag = SingletonDewNetworkBehaviour<Room>.instance.props.TryGetGoodNodePosition(out var position);
			GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
			gameObject.name = "TEST PROP POS";
			gameObject.transform.position = position;
			gameObject.GetComponent<MeshRenderer>().material.SetColor("_BaseColor", flag ? Color.green : Color.red);
			if (!flag)
			{
				gameObject.transform.localScale = Vector3.Scale(gameObject.transform.localScale, new Vector3(0.5f, 1.5f, 0.5f));
			}
		}
	}

	[DewConsoleMethod("Test Good Prop Position at Current Section")]
	public static void TestPropPosSection(int count)
	{
		if (!TryGetConsoleSelectedEntity(out var ent))
		{
			PrintNoEntitySelected();
			return;
		}
		if (ent.section == null)
		{
			Log("Selected entity is not on a section");
			return;
		}
		Log($"Testing {count} positions in current section. We already have {ent.section._usedNodePositions.Count} positions used");
		for (int i = 0; i < count; i++)
		{
			bool flag = ent.section.TryGetGoodNodePosition(out var position);
			GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
			gameObject.name = "TEST PROP POS";
			gameObject.transform.position = position;
			gameObject.GetComponent<MeshRenderer>().material.SetColor("_BaseColor", flag ? Color.green : Color.red);
			if (!flag)
			{
				gameObject.transform.localScale = Vector3.Scale(gameObject.transform.localScale, new Vector3(0.5f, 1.5f, 0.5f));
			}
		}
	}

	[DewConsoleMethod(/*Could not decode attribute arguments.*/)]
	public static void NetId(uint id)
	{
		if (NetworkServer.spawned.TryGetValue(id, out var value))
		{
			Actor component = ((Component)(object)value).GetComponent<Actor>();
			if ((UnityEngine.Object)(object)component != null)
			{
				Log($"{id}: {component.GetActorReadableName()}");
			}
			else
			{
				Log($"{id}: {((UnityEngine.Object)(object)value).name}");
			}
		}
		else
		{
			Log($"Networked object of netId {id} not found");
		}
	}

	[DewConsoleMethod("Show information about network")]
	public static void NetStat()
	{
		if (NetworkServer.active)
		{
			Log("Server is active");
		}
		else if (NetworkClient.active)
		{
			Log("Client is active");
		}
		else
		{
			Log("No Network");
		}
		Log($"NetworkClient.isConnecting - {NetworkClient.isConnecting}");
		Log($"NetworkClient.isConnected - {NetworkClient.isConnected}");
		Log($"Round-trip Time: {NetworkTime.rtt * 1000.0:#,##0.0}ms");
	}

	[DewConsoleMethod(/*Could not decode attribute arguments.*/)]
	public static void SyncFixWarpDistance(float value)
	{
		EntityControl.SyncFixWarpDistance = value;
	}

	[DewConsoleMethod(/*Could not decode attribute arguments.*/)]
	public static void SyncFixSmoothTime(float value)
	{
		EntityControl.SyncFixSmoothTime = value;
	}

	[DewConsoleMethod(/*Could not decode attribute arguments.*/)]
	public static void SyncFixSnapshotLifetime(float value)
	{
		EntityControl.SyncFixSnapshotLifetime = value;
	}

	[DewConsoleMethod(/*Could not decode attribute arguments.*/)]
	public static void SyncAgentVelocityLifetime(float value)
	{
		EntityControl.SyncAgentVelocityLifetime = value;
	}

	[DewConsoleMethod(/*Could not decode attribute arguments.*/)]
	public static void SyncExtrapolateMaxTime(float value)
	{
		EntityControl.SyncExtrapolateMaxTime = value;
	}

	[DewConsoleMethod(/*Could not decode attribute arguments.*/)]
	public static void SyncExtrapolateStrength(float value)
	{
		EntityControl.SyncExtrapolateStrength = value;
	}

	[DewConsoleMethod(/*Could not decode attribute arguments.*/)]
	public static void NetCommandTest()
	{
		GetPlayer().CmdPing();
	}

	[DewConsoleMethod(/*Could not decode attribute arguments.*/)]
	public static void Plist()
	{
		Log($"- {GetIndex(DewPlayer.creep).ToString(),-4}: {((UnityEngine.Object)(object)DewPlayer.creep).name}");
		Log($"- {GetIndex(DewPlayer.environment).ToString(),-4}: {((UnityEngine.Object)(object)DewPlayer.environment).name}");
		Log($"- {GetIndex(DewPlayer.local).ToString(),-4}: {((UnityEngine.Object)(object)DewPlayer.local).name}");
		foreach (DewPlayer allHumanPlayer in DewPlayer.allHumanPlayers)
		{
			if (!((UnityEngine.Object)(object)allHumanPlayer == (UnityEngine.Object)(object)DewPlayer.local))
			{
				Log($"- {GetIndex(allHumanPlayer).ToString(),-4}: {((UnityEngine.Object)(object)allHumanPlayer).name}");
			}
		}
	}

	private static void ClearRelationship(DewPlayer a, DewPlayer b)
	{
		a.enemies.Remove(b);
		a.allies.Remove(b);
		a.neutrals.Remove(b);
		b.enemies.Remove(a);
		b.allies.Remove(a);
		b.neutrals.Remove(a);
	}

	private static bool ResolveTwoPlayersAndLog(uint a, uint b, out DewPlayer pa, out DewPlayer pb)
	{
		pa = ResolvePlayerByIndex(a);
		pb = ResolvePlayerByIndex(b);
		if ((UnityEngine.Object)(object)pa == null || (UnityEngine.Object)(object)pb == null)
		{
			Log("Player not found.");
			Plist();
			return false;
		}
		return true;
	}

	private static DewPlayer ResolvePlayerByIndex(uint index)
	{
		if (!NetworkServer.spawned.TryGetValue(index, out var value))
		{
			return null;
		}
		if (!((Component)(object)value).TryGetComponent(out DewPlayer component))
		{
			return null;
		}
		return component;
	}

	private static uint GetIndex(DewPlayer player)
	{
		return ((NetworkBehaviour)player).netId;
	}

	[DewConsoleMethod("List all achievements.")]
	public static void AchList()
	{
		int num = 0;
		foreach (KeyValuePair<string, DewProfile.AchievementData> achievement in DewSave.profileMain.achievements)
		{
			if (achievement.Value.isCompleted)
			{
				num++;
			}
		}
		Log($"Achievements({num}/{Dew.allAchievements.Count}):");
		if (ManagerBase<AchievementManager>.instance != null)
		{
			foreach (DewAchievementItem trackedAchievement in ManagerBase<AchievementManager>.instance.trackedAchievements)
			{
				Log($"[ ] {trackedAchievement.GetCurrentProgress()}/{trackedAchievement.GetMaxProgress()} {trackedAchievement.name}");
			}
		}
		else
		{
			foreach (KeyValuePair<string, DewProfile.AchievementData> achievement2 in DewSave.profileMain.achievements)
			{
				if (!achievement2.Value.isCompleted)
				{
					Log($"[ ] {achievement2.Value.currentProgress}/{achievement2.Value.maxProgress} {achievement2.Key} {JSON.Serialize((object)achievement2.Value.persistentVariables, (NodeOptions)0, (Serializer)null)}");
				}
			}
		}
		foreach (KeyValuePair<string, DewProfile.AchievementData> achievement3 in DewSave.profileMain.achievements)
		{
			if (achievement3.Value.isCompleted)
			{
				Log($"[v] {achievement3.Value.currentProgress}/{achievement3.Value.maxProgress} {achievement3.Key}");
			}
		}
	}

	[DewConsoleMethod("List all gems.")]
	public static void ListGems()
	{
		ListGems(null);
	}

	[DewConsoleMethod("List all gems with substring")]
	public static void ListGems(string substring)
	{
		Log("--------------------------------------");
		foreach (KeyValuePair<string, DewProfile.UnlockData> gem in DewSave.profileMain.gems)
		{
			string gemName = DewLocalization.GetGemName(DewLocalization.GetGemKey(gem.Key));
			if (string.IsNullOrEmpty(substring) || gem.Key.Contains(substring, StringComparison.InvariantCultureIgnoreCase) || gemName.Contains(substring, StringComparison.InvariantCultureIgnoreCase))
			{
				Log(gem.Key + " : " + gemName);
			}
		}
		Log("--------------------------------------");
	}

	[DewConsoleMethod("List all skills.")]
	public static void ListSkills()
	{
		ListSkills(null);
	}

	[DewConsoleMethod("List all skills with substring")]
	public static void ListSkills(string substring)
	{
		Log("--------------------------------------");
		foreach (KeyValuePair<string, DewProfile.UnlockData> skill in DewSave.profileMain.skills)
		{
			string skillName = DewLocalization.GetSkillName(DewLocalization.GetSkillKey(skill.Key), 0);
			if (string.IsNullOrEmpty(substring) || skill.Key.Contains(substring, StringComparison.InvariantCultureIgnoreCase) || skillName.Contains(substring, StringComparison.InvariantCultureIgnoreCase))
			{
				Log(skill.Key + " : " + skillName);
			}
		}
		Log("--------------------------------------");
	}

	[DewConsoleMethod("List all lucid dreams.")]
	public static void ListLucidDreams()
	{
		Log($"LucidDreams({DewSave.profileMain.GetUnlockedLucidDreamsCount()}/{Dew.allLucidDreams.Count}):");
		foreach (KeyValuePair<string, DewProfile.UnlockData> lucidDream in DewSave.profileMain.lucidDreams)
		{
			if (lucidDream.Value.status != UnlockStatus.Locked)
			{
				Log(lucidDream.Key ?? "");
			}
		}
		foreach (KeyValuePair<string, DewProfile.UnlockData> lucidDream2 in DewSave.profileMain.lucidDreams)
		{
			if (lucidDream2.Value.status == UnlockStatus.Locked)
			{
				Log("LOCKED " + lucidDream2.Key);
			}
		}
	}

	[DewConsoleMethod("Reset all tutorials.")]
	public static void TutReset()
	{
		if (ManagerBase<InGameTutorialManager>.instance != null)
		{
			ManagerBase<InGameTutorialManager>.instance.StopTutorials();
		}
		DewSave.profileMain.doneTutorials = new List<string>();
		DewSave.SaveProfileMain(immediate: true);
		if (ManagerBase<InGameTutorialManager>.instance != null)
		{
			ManagerBase<InGameTutorialManager>.instance.StartTutorials();
		}
		Log("Reset all tutorials.");
	}

	[DewConsoleMethod("List all tutorials.")]
	public static void TutList()
	{
		Log($"Tutorials({DewSave.profileMain.doneTutorials.Count}/{Dew.allTutorialItems.Count}):");
		foreach (string doneTutorial in DewSave.profileMain.doneTutorials)
		{
			Log("[v] " + doneTutorial);
		}
		foreach (Type allTutorialItem in Dew.allTutorialItems)
		{
			if (!DewSave.profileMain.doneTutorials.Contains(allTutorialItem.Name))
			{
				Log("[ ] " + allTutorialItem.Name);
			}
		}
	}

	[DewConsoleMethod(/*Could not decode attribute arguments.*/)]
	public static void Res()
	{
		Debug.Log($"DewResources has {DewResources.loadedGuids.Count} objects");
	}

	[DewConsoleMethod(/*Could not decode attribute arguments.*/)]
	public static void ResVerbose()
	{
		DewResources.EnableVerboseLogging = !DewResources.EnableVerboseLogging;
		Log("DewResources verbose logging is now " + (DewResources.EnableVerboseLogging ? "ON" : "OFF"));
	}

	[DewConsoleMethod(/*Could not decode attribute arguments.*/)]
	public static void ResUnloadUnused()
	{
		DewResources.UnloadUnused();
	}

	[DewConsoleMethod(/*Could not decode attribute arguments.*/)]
	public static void ResLoad(string pattern)
	{
		IEnumerable<string> first = from p in DewResources.database.typeToGuid
			where p.Key.Name.EqualsWildcard(pattern)
			select p.Value;
		IEnumerable<string> second = from p in DewResources.database.nameToGuid
			where p.Key.EqualsWildcard(pattern)
			select p.Value;
		string[] array = Enumerable.Concat(second: DewResources.database.allGuids.Where((string p) => p.EqualsWildcard(pattern)), first: first.Concat(second)).Distinct().ToArray();
		Log($"Selected {array.Length} objects");
		LinqExtensions.ForEach<string>((IEnumerable<string>)array, (Action<string>)((string guid) =>
		{
			DewResources.Load(guid);
		}));
		Res();
	}

	[DewConsoleMethod(/*Could not decode attribute arguments.*/)]
	public static void ResPreload(string pattern)
	{
		IEnumerable<string> first = from p in DewResources.database.typeToGuid
			where p.Key.Name.EqualsWildcard(pattern)
			select p.Value;
		IEnumerable<string> second = from p in DewResources.database.nameToGuid
			where p.Key.EqualsWildcard(pattern)
			select p.Value;
		string[] array = Enumerable.Concat(second: DewResources.database.allGuids.Where((string p) => p.EqualsWildcard(pattern)), first: first.Concat(second)).Distinct().ToArray();
		Log($"Selected {array.Length} objects");
		LinqExtensions.ForEach<string>((IEnumerable<string>)array, (Action<string>)((string guid) =>
		{
			DewResources.Preload(guid);
		}));
		Res();
	}

	[DewConsoleMethod(/*Could not decode attribute arguments.*/)]
	public static void ResClearVariants()
	{
		int num = DewResources.ClearAllVariants(repairReferences: true);
		Log($"Cleared {num} variants");
	}

	[DewConsoleMethod(/*Could not decode attribute arguments.*/)]
	public static void EOSSimulateAuthExpire()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		AuthExpirationCallbackInfo val = default;
		EOSSDKComponent.Instance.OnAuthExpiration(ref val);
	}

	[DewConsoleMethod(/*Could not decode attribute arguments.*/)]
	public static void EOSReset()
	{
		ManagerBase<EOSManager>.instance.ResetEOS();
	}

	[DewConsoleMethod(/*Could not decode attribute arguments.*/)]
	public static void EOSDeletePersistentAuth()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Expected Obj, but got Unknown
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected Obj, but got Unknown
		Log("Start delete persistent auth");
		DeleteDeviceIdOptions val = default;
		ConnectInterface connectInterface = EOSSDKComponent.GetConnectInterface();
		OnDeleteDeviceIdCallback val2 = _003C_003Ec._003C_003E9__64_0;
		if (val2 == null)
		{
			OnDeleteDeviceIdCallback val3 = delegate(ref DeleteDeviceIdCallbackInfo data)
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_0026: Unknown result type (might be due to invalid IL or missing references)
				//IL_0055: Unknown result type (might be due to invalid IL or missing references)
				//IL_005f: Expected Obj, but got Unknown
				Log($"Delete device id returned: {data.ResultCode}");
				Log("Trying to create device id");
				CreateDeviceIdOptions val8 = default;
				val8.DeviceModel = Utf8String.op_Implicit(EOSSDKComponent.Instance.deviceModel);
				EOSSDKComponent.GetConnectInterface().CreateDeviceId(ref val8, (object)null, (OnCreateDeviceIdCallback)EOSSDKComponent.Instance.OnCreateDeviceId);
			};
			_003C_003Ec._003C_003E9__64_0 = val3;
			val2 = val3;
		}
		connectInterface.DeleteDeviceId(ref val, (object)null, val2);
		DeletePersistentAuthOptions val4 = default;
		val4.RefreshToken = null;
		DeletePersistentAuthOptions val5 = val4;
		AuthInterface authInterface = EOSSDKComponent.GetAuthInterface();
		OnDeletePersistentAuthCallback val6 = _003C_003Ec._003C_003E9__64_1;
		if (val6 == null)
		{
			OnDeletePersistentAuthCallback val7 = delegate(ref DeletePersistentAuthCallbackInfo data)
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				Log($"Delete persistent auth returned: {data.ResultCode}");
			};
			_003C_003Ec._003C_003E9__64_1 = val7;
			val6 = val7;
		}
		authInterface.DeletePersistentAuth(ref val5, (object)null, val6);
	}

	[DewConsoleMethod(/*Could not decode attribute arguments.*/)]
	public static void CutscenePlay(string cutscene)
	{
		DewCutsceneDirector[] array = (from c in UnityEngine.Object.FindObjectsByType<DewCutsceneDirector>(FindObjectsSortMode.None)
			where ((UnityEngine.Object)(object)c).name.Contains(cutscene, StringComparison.InvariantCultureIgnoreCase)
			select c).ToArray();
		if (array.Length == 0)
		{
			Log("Cutscene of name '" + cutscene + "' not found");
			return;
		}
		if (ManagerBase<CameraManager>.instance.isPlayingCutscene)
		{
			Log("Already playing " + ((UnityEngine.Object)(object)ManagerBase<CameraManager>.instance.currentCutsceneDirector).name);
			return;
		}
		Log("Playing " + ((UnityEngine.Object)(object)array[0]).name);
		array[0].PlayNetworked();
	}

	[DewConsoleMethod(/*Could not decode attribute arguments.*/)]
	public static void LobbyDisableVersionCheck(int val)
	{
		if (!(ManagerBase<LobbyManager>.instance.service is LobbyServiceSteam))
		{
			Log("Current service is not Steam");
		}
	}

	[DewConsoleMethod(/*Could not decode attribute arguments.*/)]
	public static void TexQuality(int val)
	{
		QualitySettings.globalTextureMipmapLimit = val;
	}

	[DewConsoleMethod(/*Could not decode attribute arguments.*/)]
	public static void UIScale(float scale)
	{
		DewSave.profileMain.gameplay.uiScale = scale;
		DewSave.SaveProfileMain(immediate: true);
		DewSave.ApplySettings();
	}

	[DewConsoleMethod(/*Could not decode attribute arguments.*/)]
	public static void EffFailSelectiveVisibility()
	{
		FxSelectiveVisibility.forceFail = !FxSelectiveVisibility.forceFail;
		if (FxSelectiveVisibility.forceFail)
		{
			Log("Selective visibility check will now ALWAYS FAIL");
		}
		else
		{
			Log("Selective visibility check will now be performed normally");
		}
	}

	[DewConsoleMethod(/*Could not decode attribute arguments.*/)]
	public static void EffDisablePlay()
	{
		DewEffect.disablePlay = !DewEffect.disablePlay;
		if (DewEffect.disablePlay)
		{
			Log("DewEffect.Play is now disabled");
		}
		else
		{
			Log("DewEffect.Play is now enabled");
		}
	}

	[DewConsoleMethod(/*Could not decode attribute arguments.*/)]
	public static void EffDisablePlayNew()
	{
		DewEffect.disablePlayNew = !DewEffect.disablePlayNew;
		if (DewEffect.disablePlayNew)
		{
			Log("DewEffect.PlayNew is now disabled");
		}
		else
		{
			Log("DewEffect.PlayNew is now enabled");
		}
	}

	[DewConsoleMethod(/*Could not decode attribute arguments.*/)]
	public static void TestMusicLoop()
	{
		ManagerBase<MusicManager>.instance._source.time = ManagerBase<MusicManager>.instance._source.clip.length - 4f;
	}

	[DewConsoleMethod(/*Could not decode attribute arguments.*/)]
	public static void MusicPlay(string guid)
	{
		DewMusicItem byGuid = DewResources.GetByGuid<DewMusicItem>(guid);
		if (byGuid == null)
		{
			LogWarning("Music not found via substring: " + guid);
			return;
		}
		ManagerBase<MusicManager>.instance.Play(byGuid);
		Log("Playing music: " + byGuid.name);
	}

	[DewConsoleMethod(/*Could not decode attribute arguments.*/)]
	public static void MusicCrossfade(string guid)
	{
		MusicCrossfade(guid, 2f);
	}

	[DewConsoleMethod(/*Could not decode attribute arguments.*/)]
	public static void MusicCrossfade(string guid, float fadeTime)
	{
		DewMusicItem byGuid = DewResources.GetByGuid<DewMusicItem>(guid);
		if (byGuid == null)
		{
			LogWarning("Music not found via substring: " + guid);
			return;
		}
		ManagerBase<MusicManager>.instance.DoCrossFade(byGuid, fadeTime);
		Log($"Crossfading to: {byGuid.name} over {fadeTime} seconds.");
	}

	[DewConsoleMethod(/*Could not decode attribute arguments.*/)]
	public static void Message()
	{
		ManagerBase<MessageManager>.instance.ShowMessage(new DewMessageSettings
		{
			buttons = (DewMessageSettings.ButtonType.Yes | DewMessageSettings.ButtonType.No),
			rawContent = "* It's the end.",
			onClose = (DewMessageSettings.ButtonType b) =>
			{
				Debug.Log("Received " + b);
			}
		});
	}

	[DewConsoleMethod(/*Could not decode attribute arguments.*/)]
	public static void FogDownscaling(float value)
	{
		value = Mathf.Clamp(value, 1f, 8f);
		VolumetricFogManager[] array = UnityEngine.Object.FindObjectsOfType<VolumetricFogManager>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].downscaling = value;
		}
		Log("Set downscaling to " + value);
	}

	[DewConsoleMethod(/*Could not decode attribute arguments.*/)]
	public static void FogRaymarchQuality(int value)
	{
		value = Mathf.Clamp(value, 1, 16);
		VolumetricFog[] array = UnityEngine.Object.FindObjectsOfType<VolumetricFog>();
		foreach (VolumetricFog val in array)
		{
			val.profile.raymarchQuality = value;
			if (((Component)(object)val).gameObject.activeSelf)
			{
				((Component)(object)val).gameObject.SetActive(value: false);
				((Component)(object)val).gameObject.SetActive(value: true);
			}
		}
		Log("Set raymarch quality to " + value);
	}

	[DewConsoleMethod(/*Could not decode attribute arguments.*/)]
	public static void FogJitter(int value)
	{
		VolumetricFog[] array = UnityEngine.Object.FindObjectsOfType<VolumetricFog>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].profile.jittering = value;
		}
		Log("Set jittering to " + value);
	}

	[DewConsoleMethod(/*Could not decode attribute arguments.*/)]
	public static void FadeIn()
	{
		ManagerBase<TransitionManager>.instance.FadeIn();
	}

	[DewConsoleMethod(/*Could not decode attribute arguments.*/)]
	public static void FadeOut()
	{
		ManagerBase<TransitionManager>.instance.FadeOut(showTips: false);
	}

	[DewConsoleMethod(/*Could not decode attribute arguments.*/)]
	public static void TestException()
	{
		throw new Exception("This is a test exception");
	}

	[DewConsoleMethod(/*Could not decode attribute arguments.*/)]
	public static void Text(string txt)
	{
		InGameUIManager.instance.ShowWorldPopMessage(new WorldMessageSetting
		{
			rawText = txt,
			worldPos = GetCursorWorldPos()
		});
	}

	[DewConsoleMethod("Get info about current build profile.")]
	public static void Build()
	{
		DewBuildProfile current = DewBuildProfile.current;
		Log("Showing build profile used for '" + Application.version + "'");
		FieldInfo[] fields = typeof(DewBuildProfile).GetFields(BindingFlags.Instance | BindingFlags.Public);
		foreach (FieldInfo fieldInfo in fields)
		{
			object value = fieldInfo.GetValue(current);
			if (value is IList list)
			{
				Log($"- {fieldInfo.Name}: {fieldInfo.FieldType} ({list.Count} elements)");
				foreach (object item in list)
				{
					Log($"   * {item}");
				}
			}
			else
			{
				Log($"- {fieldInfo.Name}: {value}");
			}
		}
		Log("End of build profile info.");
	}

	[DewConsoleMethod(/*Could not decode attribute arguments.*/)]
	public static void Pop()
	{
		Log(string.Format("Pop: {0}/{1} {2}", NetworkedManagerBase<GameManager>.instance.spawnedPopulation, NetworkedManagerBase<GameManager>.instance.maxSpawnedPopulation, NetworkedManagerBase<GameManager>.instance.isSpawnOverPopulation ? "(Overpopulation)" : ""));
		Log($"Spawned Population Multiplier: {NetworkedManagerBase<GameManager>.instance.GetAdjustedMonsterSpawnPopulation(1f):0.00}x");
	}

	[DewConsoleMethod("Set max frames per second")]
	public static void Fps(int frameCount)
	{
		Application.targetFrameRate = frameCount;
	}

	[DewConsoleMethod("Enable or disable actor name change every logic update (Performance will suffer)")]
	public static void UsefulActorName(bool enable)
	{
		ActorManager.enableUsefulActorName = enable;
	}

	[DewConsoleMethod("Show FPS")]
	public static void ShowFPS(bool enable)
	{
		ManagerBase<GlobalLogicPackage>.instance.showFps = enable;
	}

	[DewConsoleMethod("Show FPS")]
	public static void ShowFPS()
	{
		ManagerBase<GlobalLogicPackage>.instance.showFps = !ManagerBase<GlobalLogicPackage>.instance.showFps;
	}

	[DewConsoleMethod("Change profile name")]
	public static void ProfileName(string newName)
	{
		DewSave.profileMain.name = newName;
		DewSave.SaveProfileMain(immediate: true);
	}

	[DewConsoleMethod("Show profile name")]
	public static void ProfileName()
	{
		Log(DewSave.profileMain.name);
	}

	[DewConsoleMethod("Save profile")]
	public static void ProfileSave()
	{
		DewSave.SaveProfileAll(immediate: true);
	}

	[DewConsoleMethod(/*Could not decode attribute arguments.*/)]
	public static void CinematicHelper()
	{
		CinematicCameraHelper cinematicCameraHelper = UnityEngine.Object.FindObjectOfType<CinematicCameraHelper>();
		if (cinematicCameraHelper == null)
		{
			Log("Cinematic helper not present");
			return;
		}
		cinematicCameraHelper.isCinematicHelperEnabled = !cinematicCameraHelper.isCinematicHelperEnabled;
		Transform[] array = UnityEngine.Object.FindObjectsOfType<Transform>(includeInactive: true);
		for (int i = 0; i < array.Length; i++)
		{
			ICinematicCameraHelperStateReceiver[] components = array[i].GetComponents<ICinematicCameraHelperStateReceiver>();
			foreach (ICinematicCameraHelperStateReceiver cinematicCameraHelperStateReceiver in components)
			{
				try
				{
					cinematicCameraHelperStateReceiver.OnCinematicCameraHelperChanged(cinematicCameraHelper.isCinematicHelperEnabled);
				}
				catch (Exception exception)
				{
					Debug.LogException(exception, cinematicCameraHelperStateReceiver as UnityEngine.Object);
				}
			}
		}
		Log("Cinematic helper is now " + (cinematicCameraHelper.isCinematicHelperEnabled ? "ON" : "OFF"));
	}

	[DewConsoleMethod("Change language")]
	public static void Lang(string substring)
	{
		string[] array = DewLocalization.buildData.dataByLanguage.Keys.ToArray();
		string text = "";
		string[] array2 = array;
		foreach (string text2 in array2)
		{
			if (text2.Contains(substring, StringComparison.OrdinalIgnoreCase))
			{
				text = text2;
			}
		}
		if (text == "")
		{
			Log("Requested language not found");
			return;
		}
		Log("Changing language from " + DewSave.profileMain.language + " to " + text);
		DewSave.profileMain.language = text;
		DewSave.SaveProfileMain(immediate: true);
		DewSave.ApplySettings();
	}

	[DewConsoleMethod("Toggle lang test")]
	public static void LangTest()
	{
		GameObject gameObject = GameObject.Find("LangTester");
		if (gameObject != null)
		{
			UnityEngine.Object.Destroy(gameObject);
			Log("Stopped lang test");
		}
		else
		{
			Coroutiner coroutiner = new GameObject("LangTester").AddComponent<Coroutiner>();
			UnityEngine.Object.DontDestroyOnLoad(coroutiner);
			coroutiner.StartCoroutine(Routine());
		}
		static IEnumerator Routine()
		{
			while (true)
			{
				SetLang("en-US");
				yield return new WaitForSeconds(0.3f);
				SetLang("ko-KR");
				yield return new WaitForSeconds(0.3f);
				SetLang("zh-CN");
				yield return new WaitForSeconds(0.3f);
				SetLang("es-MX");
				yield return new WaitForSeconds(0.3f);
				SetLang("ja-JP");
				yield return new WaitForSeconds(0.3f);
				SetLang("ru-RU");
				yield return new WaitForSeconds(0.3f);
			}
		}
		static void SetLang(string lang)
		{
			DewSave.profileMain.language = lang;
			Transform[] array = UnityEngine.Object.FindObjectsOfType<Transform>(includeInactive: true);
			for (int i = 0; i < array.Length; i++)
			{
				ILangaugeChangedCallback[] components = array[i].GetComponents<ILangaugeChangedCallback>();
				foreach (ILangaugeChangedCallback langaugeChangedCallback in components)
				{
					try
					{
						langaugeChangedCallback.OnLanguageChanged();
					}
					catch (Exception exception)
					{
						Debug.LogException(exception, langaugeChangedCallback as UnityEngine.Object);
					}
				}
			}
		}
	}

	[DewConsoleMethod(/*Could not decode attribute arguments.*/)]
	public static void Pool()
	{
		ManagerBase<SpawnManager>.instance.usePooling = !ManagerBase<SpawnManager>.instance.usePooling;
		Log("Pooling is now " + (ManagerBase<SpawnManager>.instance.usePooling ? "ON" : "OFF"));
	}

	[DewConsoleMethod(/*Could not decode attribute arguments.*/)]
	public static void PoolInfo()
	{
		ManagerBase<SpawnManager>.instance.showPoolInfo = !ManagerBase<SpawnManager>.instance.showPoolInfo;
		Log("Show pool info is now " + (ManagerBase<SpawnManager>.instance.usePooling ? "ON" : "OFF"));
	}

	[DewConsoleMethod(/*Could not decode attribute arguments.*/)]
	public static void AnalyticsUpsert()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		DewSave.UpsertProfile();
	}

	[DewConsoleMethod("Test adv text")]
	public static void Adv(string adv)
	{
		Log("Adv: " + adv);
		Log("Converted: " + DewAdvText.ConvertAdvToText(adv));
	}

	[DewConsoleMethod(/*Could not decode attribute arguments.*/)]
	public static void BakeMeshInterval(float interval)
	{
		Log($"BakeMeshInterval {Dew.BakeMeshInterval:#,##0.##} => {interval:#,##0.##}");
		Dew.BakeMeshInterval = interval;
	}

	[DewConsoleMethod(/*Could not decode attribute arguments.*/)]
	public static void BakeMeshInterval()
	{
		Log($"BakeMeshInterval {Dew.BakeMeshInterval:#,##0.##}");
	}

	[DewConsoleMethod(/*Could not decode attribute arguments.*/)]
	public static void AllowedQuality(int allowedLevel, float perfPressureStrength = 0f)
	{
		if (allowedLevel <= 0)
		{
			ManagerBase<GraphicsManager>.instance.effectQualityOverride = null;
			ManagerBase<GraphicsManager>.instance.perfPressureStrengthOverride = null;
		}
		else
		{
			ManagerBase<GraphicsManager>.instance.effectQualityOverride = (Quality3Levels)allowedLevel;
			ManagerBase<GraphicsManager>.instance.perfPressureStrengthOverride = perfPressureStrength;
		}
	}

	[DewConsoleMethod(/*Could not decode attribute arguments.*/)]
	public static void ToggleTerrainBRG()
	{
		List<Terrain> list = new List<Terrain>();
		GameObject[] rootGameObjects = SceneManager.GetActiveScene().GetRootGameObjects();
		foreach (GameObject gameObject in rootGameObjects)
		{
			list.AddRange(gameObject.GetComponentsInChildren<Terrain>());
		}
		if (list.Count == 0)
		{
			Log("No Terrain found in active scene");
			return;
		}
		bool flag = list.Any((Terrain t) => (UnityEngine.Object)(object)((Component)(object)t).GetComponent<TerrainBRGRegisterer>() != null);
		foreach (Terrain item in list)
		{
			TerrainBRGRegisterer component = ((Component)(object)item).GetComponent<TerrainBRGRegisterer>();
			if (flag)
			{
				if ((UnityEngine.Object)(object)component != null)
				{
					UnityEngine.Object.Destroy((UnityEngine.Object)(object)component);
				}
			}
			else
			{
				((Component)(object)item).gameObject.AddComponent<TerrainBRGRegisterer>();
			}
		}
		Log(string.Format("{0} TerrainBRGRegisterer on {1} terrain(s)", flag ? "Removed" : "Added", list.Count));
	}
}
