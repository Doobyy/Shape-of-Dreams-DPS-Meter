using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class ChatManager : NetworkedManagerBase<ChatManager>
{
	public struct Message
	{
		public MessageType type;

		public string content;

		public string[] args;

		public string itemType;

		public int itemLevel;

		public Cost itemPrice;

		public string itemCustomData;
	}

	public enum MessageType : byte
	{
		Raw,
		Chat,
		WorldEvent,
		Notice,
		UnlockedAchievement,
		AdvText
	}

	public const int ChatMessage_MaxLength = 100;

	public const int ChatMessage_RateLimitCount = 8;

	public const float ChatMessage_RateLimitTimeframe = 12f;

	public const float ChatMessage_RateLimitLockTime = 8f;

	public const float Emote_CooldownTime = 1.4f;

	public SafeAction<Message> ClientEvent_OnMessageReceived;

	public SafeAction<DewPlayer, string> ClientEvent_OnEmoteReceived;

	private List<ulong> _mutedPlayers = new List<ulong>();

	private Dictionary<int, float> _rates;

	private Dictionary<int, float> _lockChatTimes;

	private Dictionary<int, float> _lastEmoteTimes;

	private List<int> _keysBuffer;

	public const string ChatContentColorHex = "#e4edf0";

	public override void OnStartServer()
	{
		base.OnStartServer();
		_rates = new Dictionary<int, float>();
		_lockChatTimes = new Dictionary<int, float>();
		_lastEmoteTimes = new Dictionary<int, float>();
		_keysBuffer = new List<int>();
	}

	public override void LogicUpdate(float dt)
	{
		base.LogicUpdate(dt);
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		float maxDelta = dt * 8f / 12f;
		_keysBuffer.Clear();
		Extensions.CopyTo<int>((IEnumerable<int>)_rates.Keys, _keysBuffer);
		foreach (int item in _keysBuffer)
		{
			float num = Mathf.MoveTowards(_rates[item], 0f, maxDelta);
			if (num < 0.0001f)
			{
				_rates.Remove(item);
			}
			else
			{
				_rates[item] = num;
			}
		}
	}

	[Server]
	public bool IsChatLocked(NetworkConnectionToClient connection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Boolean ChatManager::IsChatLocked(Mirror.NetworkConnectionToClient)' called when server was not active");
			return default;
		}
		if (_lockChatTimes.TryGetValue(((NetworkConnection)connection).connectionId, out var value) && Time.time - value < 8f)
		{
			return true;
		}
		return false;
	}

	[Server]
	public bool IncrementRateAndCheck(NetworkConnectionToClient connection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Boolean ChatManager::IncrementRateAndCheck(Mirror.NetworkConnectionToClient)' called when server was not active");
			return default;
		}
		if (NetworkServer.dontListen || NetworkServer.connections.Count <= 1)
		{
			return true;
		}
		if (IsChatLocked(connection))
		{
			return false;
		}
		if (_rates.TryGetValue(((NetworkConnection)connection).connectionId, out var value))
		{
			_rates[((NetworkConnection)connection).connectionId] = value + 1f;
		}
		else
		{
			_rates[((NetworkConnection)connection).connectionId] = 1f;
		}
		if (_rates[((NetworkConnection)connection).connectionId] > 8f)
		{
			_lockChatTimes[((NetworkConnection)connection).connectionId] = Time.time;
			return false;
		}
		return true;
	}

	[Command(requiresAuthority = false)]
	public void CmdSendEmote(string emoteName, NetworkConnectionToClient sender = null)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteString((NetworkWriter)(object)val, emoteName);
		((NetworkBehaviour)this).SendCommandInternal("System.Void ChatManager::CmdSendEmote(System.String,Mirror.NetworkConnectionToClient)", -948322391, (NetworkWriter)(object)val, 0, false);
		NetworkWriterPool.Return(val);
	}

	[Command(requiresAuthority = false)]
	public void CmdSendChatMessage(string content, NetworkConnectionToClient sender = null)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteString((NetworkWriter)(object)val, content);
		((NetworkBehaviour)this).SendCommandInternal("System.Void ChatManager::CmdSendChatMessage(System.String,Mirror.NetworkConnectionToClient)", 973844096, (NetworkWriter)(object)val, 0, false);
		NetworkWriterPool.Return(val);
	}

	[Command(requiresAuthority = false)]
	public void CmdSendChatMessageTargeted(string content, List<uint> targets, NetworkConnectionToClient sender = null)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteString((NetworkWriter)(object)val, content);
		GeneratedNetworkCode._Write_System_002ECollections_002EGeneric_002EList_00601_003CSystem_002EUInt32_003E((NetworkWriter)(object)val, targets);
		((NetworkBehaviour)this).SendCommandInternal("System.Void ChatManager::CmdSendChatMessageTargeted(System.String,System.Collections.Generic.List`1<System.UInt32>,Mirror.NetworkConnectionToClient)", -394338220, (NetworkWriter)(object)val, 0, false);
		NetworkWriterPool.Return(val);
	}

	[TargetRpc]
	public void TargetBroadcastMessage(NetworkConnectionToClient target, Message message)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_ChatManager_002FMessage((NetworkWriter)(object)val, message);
		((NetworkBehaviour)this).SendTargetRPCInternal((NetworkConnection)(object)target, "System.Void ChatManager::TargetBroadcastMessage(Mirror.NetworkConnectionToClient,ChatManager/Message)", 2031378885, (NetworkWriter)(object)val, 0);
		NetworkWriterPool.Return(val);
	}

	[Command(requiresAuthority = false)]
	public void CmdSendPingMessage(string advMessage, NetworkConnectionToClient sender = null)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteString((NetworkWriter)(object)val, advMessage);
		((NetworkBehaviour)this).SendCommandInternal("System.Void ChatManager::CmdSendPingMessage(System.String,Mirror.NetworkConnectionToClient)", 2029415206, (NetworkWriter)(object)val, 0, false);
		NetworkWriterPool.Return(val);
	}

	[Command(requiresAuthority = false)]
	public void CmdSendAchievementMessage(string achKey, NetworkConnectionToClient sender = null)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteString((NetworkWriter)(object)val, achKey);
		((NetworkBehaviour)this).SendCommandInternal("System.Void ChatManager::CmdSendAchievementMessage(System.String,Mirror.NetworkConnectionToClient)", 1471701451, (NetworkWriter)(object)val, 0, false);
		NetworkWriterPool.Return(val);
	}

	public void SendChatLockedNotice(NetworkConnectionToClient target)
	{
		SendChatMessage((NetworkConnection)(object)target, new Message
		{
			type = MessageType.Notice,
			content = "Chat_Notice_WaitBeforeChat"
		});
	}

	public bool IsPlayerMuted(DewPlayer player)
	{
		return IsPlayerMutedBySteamId(player.steamId.m_SteamID);
	}

	public bool IsPlayerMutedBySteamId(ulong playerSteamId)
	{
		return _mutedPlayers.Contains(playerSteamId);
	}

	public bool IsPlayerMutedByNetIds(ulong playeNetId)
	{
		return _mutedPlayers.Contains(playeNetId);
	}

	public bool IsPlayerMutedByNetId(uint playerNetId)
	{
		DewPlayer dewPlayer = DewPlayer.allHumanPlayers.Find((DewPlayer p) => ((NetworkBehaviour)p).netId == playerNetId);
		if ((UnityEngine.Object)(object)dewPlayer == null)
		{
			return true;
		}
		return IsPlayerMuted(dewPlayer);
	}

	[ClientRpc]
	public void BroadcastMessage(Message message)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_ChatManager_002FMessage((NetworkWriter)(object)val, message);
		((NetworkBehaviour)this).SendRPCInternal("System.Void ChatManager::BroadcastMessage(ChatManager/Message)", -216182835, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[ClientRpc]
	public void BroadcastAdvText(string advText)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteString((NetworkWriter)(object)val, advText);
		((NetworkBehaviour)this).SendRPCInternal("System.Void ChatManager::BroadcastAdvText(System.String)", 943283089, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[ClientRpc]
	public void RpcShowEmote(string emoteName, DewPlayer sender)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteString((NetworkWriter)(object)val, emoteName);
		NetworkWriterExtensions.WriteNetworkBehaviour((NetworkWriter)(object)val, (NetworkBehaviour)(object)sender);
		((NetworkBehaviour)this).SendRPCInternal("System.Void ChatManager::RpcShowEmote(System.String,DewPlayer)", 783443695, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	public void ShowMessageLocally(Message message)
	{
		try
		{
			ClientEvent_OnMessageReceived?.Invoke(message);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception, (UnityEngine.Object)(object)this);
		}
	}

	[TargetRpc]
	public void SendChatMessage(NetworkConnection target, Message message)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_ChatManager_002FMessage((NetworkWriter)(object)val, message);
		((NetworkBehaviour)this).SendTargetRPCInternal(target, "System.Void ChatManager::SendChatMessage(Mirror.NetworkConnection,ChatManager/Message)", -901792903, (NetworkWriter)(object)val, 0);
		NetworkWriterPool.Return(val);
	}

	private static string SanitizeUserInput(string message)
	{
		if (message.Length > 100)
		{
			message = message.Substring(0, 100);
		}
		message = CaseInsensitiveReplace(message, "<noparse>", "");
		message = CaseInsensitiveReplace(message, "</noparse>", "");
		return message;
	}

	private static string CaseInsensitiveReplace(string message, string target, string replacement)
	{
		message = Regex.Replace(message, Regex.Escape(target), replacement.Replace("$", "$$"), RegexOptions.IgnoreCase);
		return message;
	}

	public void MutePlayer(DewPlayer player)
	{
		if (!_mutedPlayers.Contains(player.steamId.m_SteamID))
		{
			_mutedPlayers.Add(player.steamId.m_SteamID);
			if (!_mutedPlayers.Contains(((NetworkBehaviour)player).netId))
			{
				_mutedPlayers.Add(((NetworkBehaviour)player).netId);
				ClientEvent_OnMessageReceived?.Invoke(new Message
				{
					type = MessageType.Notice,
					content = "Chat_Notice_PlayerMuted",
					args = new string[1] { DewAdvText.Adv_DescribedPlayerName(player) }
				});
			}
		}
	}

	public void UnmutePlayer(DewPlayer player)
	{
		if (_mutedPlayers.Remove(player.steamId.m_SteamID) && _mutedPlayers.Remove(((NetworkBehaviour)player).netId))
		{
			ClientEvent_OnMessageReceived?.Invoke(new Message
			{
				type = MessageType.Notice,
				content = "Chat_Notice_PlayerUnmuted",
				args = new string[1] { DewAdvText.Adv_DescribedPlayerName(player) }
			});
		}
	}

	public static string GetDescribedPlayerName(DewPlayer player)
	{
		if ((UnityEngine.Object)(object)player.hero == null)
		{
			return player.playerName;
		}
		return player.playerName + " (" + DewLocalization.GetUIValue(((object)player.hero).GetType().Name + "_Name") + ")";
	}

	public static string GetColoredDescribedPlayerName(DewPlayer player)
	{
		if ((UnityEngine.Object)(object)player.hero == null)
		{
			return player.playerName;
		}
		return "<color=" + GetPlayerColorHex(player) + ">" + player.playerName + " (" + DewLocalization.GetUIValue(((object)player.hero).GetType().Name + "_Name") + ")</color>";
	}

	public static string GetPlayerColorHex(DewPlayer player = null)
	{
		return "#70d4ff";
	}

	public static string GetFormattedChatContent(string playerName, string content)
	{
		return "<color=" + GetPlayerColorHex() + ">" + playerName + ":</color> <color=#e4edf0>" + content + "</color>";
	}

	public static string GetColoredSkillName(string typeName, int level)
	{
		string skillLevelTemplate = DewLocalization.GetSkillLevelTemplate(level);
		string skillName = DewLocalization.GetSkillName(DewLocalization.GetSkillKey(typeName), 0);
		string text = string.Format(skillLevelTemplate, skillName);
		string rarityColorHex = Dew.GetRarityColorHex(DewResources.GetByShortTypeName<SkillTrigger>(typeName, default(ResourceLoadSettings)).rarity);
		return "<color=" + rarityColorHex + ">" + text + "</color>";
	}

	public static string GetColoredGemName(string typeName, int quality)
	{
		Gem.QualityType qualityType = Gem.GetQualityType(quality);
		string gemName = DewLocalization.GetGemName(DewLocalization.GetGemKey(typeName));
		string text = string.Format(DewLocalization.GetUIValue("InGame_Tooltip_GemName_" + qualityType), $"{gemName} {quality}%");
		string rarityColorHex = Dew.GetRarityColorHex(DewResources.GetByShortTypeName<Gem>(typeName, default(ResourceLoadSettings)).rarity);
		return "<color=" + rarityColorHex + ">" + text + "</color>";
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_CmdSendEmote__String__NetworkConnectionToClient(string emoteName, NetworkConnectionToClient sender)
	{
		DewPlayer player = sender.GetPlayer();
		if ((UnityEngine.Object)(object)player == null)
		{
			return;
		}
		float value;
		if (!IncrementRateAndCheck(sender))
		{
			SendChatLockedNotice(sender);
		}
		else if (!_lastEmoteTimes.TryGetValue(((NetworkConnection)sender).connectionId, out value) || !(Time.unscaledTime - value < 1.4f))
		{
			_lastEmoteTimes[((NetworkConnection)sender).connectionId] = Time.unscaledTime;
			if (!(DewResources.GetByName<Emote>(emoteName) == null) && player.IsAllowedToUseItem(emoteName))
			{
				RpcShowEmote(emoteName, player);
			}
		}
	}

	protected static void InvokeUserCode_CmdSendEmote__String__NetworkConnectionToClient(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSendEmote called on client.");
		}
		else
		{
			((ChatManager)(object)obj).UserCode_CmdSendEmote__String__NetworkConnectionToClient(NetworkReaderExtensions.ReadString(reader), senderConnection);
		}
	}

	protected void UserCode_CmdSendChatMessage__String__NetworkConnectionToClient(string content, NetworkConnectionToClient sender)
	{
		if (content == null)
		{
			return;
		}
		content = content.Trim();
		if (content.Length > 0)
		{
			if (!IncrementRateAndCheck(sender))
			{
				SendChatLockedNotice(sender);
				return;
			}
			Message message = new Message
			{
				type = MessageType.Chat,
				content = SanitizeUserInput(content),
				args = new string[1] { ((NetworkBehaviour)sender.GetPlayer()).netId.ToString() }
			};
			BroadcastMessage(message);
		}
	}

	protected static void InvokeUserCode_CmdSendChatMessage__String__NetworkConnectionToClient(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSendChatMessage called on client.");
		}
		else
		{
			((ChatManager)(object)obj).UserCode_CmdSendChatMessage__String__NetworkConnectionToClient(NetworkReaderExtensions.ReadString(reader), senderConnection);
		}
	}

	protected void UserCode_CmdSendChatMessageTargeted__String__List_00601__NetworkConnectionToClient(string content, List<uint> targets, NetworkConnectionToClient sender)
	{
		if (sender == null || content == null || targets == null || targets.Count == 0)
		{
			return;
		}
		content = content.Trim();
		if (content.Length <= 0)
		{
			return;
		}
		if (!IncrementRateAndCheck(sender))
		{
			SendChatLockedNotice(sender);
			return;
		}
		Message message = new Message
		{
			type = MessageType.Chat,
			content = SanitizeUserInput(content),
			args = new string[1] { ((NetworkBehaviour)sender.GetPlayer()).netId.ToString() }
		};
		HashSet<NetworkConnectionToClient> hashSet = new HashSet<NetworkConnectionToClient>();
		foreach (uint target in targets)
		{
			if (NetworkServer.spawned.TryGetValue(target, out var value))
			{
				NetworkConnectionToClient connectionToClient = value.connectionToClient;
				if (connectionToClient != null)
				{
					hashSet.Add(connectionToClient);
				}
			}
		}
		hashSet.Add(sender);
		foreach (NetworkConnectionToClient item in hashSet)
		{
			TargetBroadcastMessage(item, message);
		}
	}

	protected static void InvokeUserCode_CmdSendChatMessageTargeted__String__List_00601__NetworkConnectionToClient(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSendChatMessageTargeted called on client.");
		}
		else
		{
			((ChatManager)(object)obj).UserCode_CmdSendChatMessageTargeted__String__List_00601__NetworkConnectionToClient(NetworkReaderExtensions.ReadString(reader), GeneratedNetworkCode._Read_System_002ECollections_002EGeneric_002EList_00601_003CSystem_002EUInt32_003E(reader), senderConnection);
		}
	}

	protected void UserCode_TargetBroadcastMessage__NetworkConnectionToClient__Message(NetworkConnectionToClient target, Message message)
	{
		if ((message.type != MessageType.Chat || message.args.Length != 1 || !uint.TryParse(message.args[0], NumberStyles.Any, CultureInfo.InvariantCulture, out var result) || !IsPlayerMutedByNetId(result)) && (message.type != MessageType.UnlockedAchievement || message.args.Length != 2 || !uint.TryParse(message.args[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var result2) || !IsPlayerMutedByNetId(result2)))
		{
			if (message.type == MessageType.Chat)
			{
				message.content = "<noparse>" + DewSafety.FilterProfanityIfEnabled(message.content) + "</noparse>";
			}
			ShowMessageLocally(message);
		}
	}

	protected static void InvokeUserCode_TargetBroadcastMessage__NetworkConnectionToClient__Message(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TargetBroadcastMessage called on server.");
		}
		else
		{
			((ChatManager)(object)obj).UserCode_TargetBroadcastMessage__NetworkConnectionToClient__Message((NetworkConnectionToClient)(object)NetworkClient.connection, GeneratedNetworkCode._Read_ChatManager_002FMessage(reader));
		}
	}

	protected void UserCode_CmdSendPingMessage__String__NetworkConnectionToClient(string advMessage, NetworkConnectionToClient sender)
	{
		if (advMessage == null)
		{
			return;
		}
		advMessage = advMessage.Trim();
		if (advMessage.Length > 0)
		{
			if (!IncrementRateAndCheck(sender))
			{
				SendChatLockedNotice(sender);
				return;
			}
			Message message = new Message
			{
				type = MessageType.AdvText,
				content = DewAdvText.Adv_ChatContent(sender.GetPlayer(), advMessage),
				args = new string[1] { ((NetworkBehaviour)sender.GetPlayer()).netId.ToString() }
			};
			BroadcastMessage(message);
		}
	}

	protected static void InvokeUserCode_CmdSendPingMessage__String__NetworkConnectionToClient(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSendPingMessage called on client.");
		}
		else
		{
			((ChatManager)(object)obj).UserCode_CmdSendPingMessage__String__NetworkConnectionToClient(NetworkReaderExtensions.ReadString(reader), senderConnection);
		}
	}

	protected void UserCode_CmdSendAchievementMessage__String__NetworkConnectionToClient(string achKey, NetworkConnectionToClient sender)
	{
		if (!IncrementRateAndCheck(sender))
		{
			SendChatLockedNotice(sender);
		}
		else if (Dew.achievementsByName.ContainsKey(achKey))
		{
			Message message = new Message
			{
				type = MessageType.UnlockedAchievement,
				args = new string[2]
				{
					achKey,
					((NetworkBehaviour)sender.GetPlayer()).netId.ToString()
				}
			};
			BroadcastMessage(message);
		}
	}

	protected static void InvokeUserCode_CmdSendAchievementMessage__String__NetworkConnectionToClient(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSendAchievementMessage called on client.");
		}
		else
		{
			((ChatManager)(object)obj).UserCode_CmdSendAchievementMessage__String__NetworkConnectionToClient(NetworkReaderExtensions.ReadString(reader), senderConnection);
		}
	}

	protected void UserCode_BroadcastMessage__Message(Message message)
	{
		if ((message.type != MessageType.Chat || message.args.Length != 1 || !uint.TryParse(message.args[0], NumberStyles.Any, CultureInfo.InvariantCulture, out var result) || !IsPlayerMutedByNetId(result)) && (message.type != MessageType.UnlockedAchievement || message.args.Length != 2 || !uint.TryParse(message.args[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var result2) || !IsPlayerMutedByNetId(result2)))
		{
			if (message.type == MessageType.Chat)
			{
				message.content = "<noparse>" + DewSafety.FilterProfanityIfEnabled(message.content) + "</noparse>";
			}
			ShowMessageLocally(message);
		}
	}

	protected static void InvokeUserCode_BroadcastMessage__Message(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC BroadcastMessage called on server.");
		}
		else
		{
			((ChatManager)(object)obj).UserCode_BroadcastMessage__Message(GeneratedNetworkCode._Read_ChatManager_002FMessage(reader));
		}
	}

	protected void UserCode_BroadcastAdvText__String(string advText)
	{
		ShowMessageLocally(new Message
		{
			type = MessageType.AdvText,
			content = advText
		});
	}

	protected static void InvokeUserCode_BroadcastAdvText__String(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC BroadcastAdvText called on server.");
		}
		else
		{
			((ChatManager)(object)obj).UserCode_BroadcastAdvText__String(NetworkReaderExtensions.ReadString(reader));
		}
	}

	protected void UserCode_RpcShowEmote__String__DewPlayer(string emoteName, DewPlayer sender)
	{
		if (IsPlayerMuted(sender) || !sender.IsAllowedToUseItem(emoteName))
		{
			return;
		}
		try
		{
			ClientEvent_OnEmoteReceived?.Invoke(sender, emoteName);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception, (UnityEngine.Object)(object)this);
		}
	}

	protected static void InvokeUserCode_RpcShowEmote__String__DewPlayer(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcShowEmote called on server.");
		}
		else
		{
			((ChatManager)(object)obj).UserCode_RpcShowEmote__String__DewPlayer(NetworkReaderExtensions.ReadString(reader), NetworkReaderExtensions.ReadNetworkBehaviour<DewPlayer>(reader));
		}
	}

	protected void UserCode_SendChatMessage__NetworkConnection__Message(NetworkConnection target, Message message)
	{
		try
		{
			ClientEvent_OnMessageReceived?.Invoke(message);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception, (UnityEngine.Object)(object)this);
		}
	}

	protected static void InvokeUserCode_SendChatMessage__NetworkConnection__Message(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC SendChatMessage called on server.");
		}
		else
		{
			((ChatManager)(object)obj).UserCode_SendChatMessage__NetworkConnection__Message(NetworkClient.connection, GeneratedNetworkCode._Read_ChatManager_002FMessage(reader));
		}
	}

	static ChatManager()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected Obj, but got Unknown
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Expected Obj, but got Unknown
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Expected Obj, but got Unknown
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Expected Obj, but got Unknown
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Expected Obj, but got Unknown
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Expected Obj, but got Unknown
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Expected Obj, but got Unknown
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Expected Obj, but got Unknown
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Expected Obj, but got Unknown
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterCommand(typeof(ChatManager), "System.Void ChatManager::CmdSendEmote(System.String,Mirror.NetworkConnectionToClient)", (RemoteCallDelegate)InvokeUserCode_CmdSendEmote__String__NetworkConnectionToClient, false);
		RemoteProcedureCalls.RegisterCommand(typeof(ChatManager), "System.Void ChatManager::CmdSendChatMessage(System.String,Mirror.NetworkConnectionToClient)", (RemoteCallDelegate)InvokeUserCode_CmdSendChatMessage__String__NetworkConnectionToClient, false);
		RemoteProcedureCalls.RegisterCommand(typeof(ChatManager), "System.Void ChatManager::CmdSendChatMessageTargeted(System.String,System.Collections.Generic.List`1<System.UInt32>,Mirror.NetworkConnectionToClient)", (RemoteCallDelegate)InvokeUserCode_CmdSendChatMessageTargeted__String__List_00601__NetworkConnectionToClient, false);
		RemoteProcedureCalls.RegisterCommand(typeof(ChatManager), "System.Void ChatManager::CmdSendPingMessage(System.String,Mirror.NetworkConnectionToClient)", (RemoteCallDelegate)InvokeUserCode_CmdSendPingMessage__String__NetworkConnectionToClient, false);
		RemoteProcedureCalls.RegisterCommand(typeof(ChatManager), "System.Void ChatManager::CmdSendAchievementMessage(System.String,Mirror.NetworkConnectionToClient)", (RemoteCallDelegate)InvokeUserCode_CmdSendAchievementMessage__String__NetworkConnectionToClient, false);
		RemoteProcedureCalls.RegisterRpc(typeof(ChatManager), "System.Void ChatManager::BroadcastMessage(ChatManager/Message)", (RemoteCallDelegate)InvokeUserCode_BroadcastMessage__Message);
		RemoteProcedureCalls.RegisterRpc(typeof(ChatManager), "System.Void ChatManager::BroadcastAdvText(System.String)", (RemoteCallDelegate)InvokeUserCode_BroadcastAdvText__String);
		RemoteProcedureCalls.RegisterRpc(typeof(ChatManager), "System.Void ChatManager::RpcShowEmote(System.String,DewPlayer)", (RemoteCallDelegate)InvokeUserCode_RpcShowEmote__String__DewPlayer);
		RemoteProcedureCalls.RegisterRpc(typeof(ChatManager), "System.Void ChatManager::TargetBroadcastMessage(Mirror.NetworkConnectionToClient,ChatManager/Message)", (RemoteCallDelegate)InvokeUserCode_TargetBroadcastMessage__NetworkConnectionToClient__Message);
		RemoteProcedureCalls.RegisterRpc(typeof(ChatManager), "System.Void ChatManager::SendChatMessage(Mirror.NetworkConnection,ChatManager/Message)", (RemoteCallDelegate)InvokeUserCode_SendChatMessage__NetworkConnection__Message);
	}
}
