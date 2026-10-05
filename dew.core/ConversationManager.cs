using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DewInternal;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class ConversationManager : NetworkedManagerBase<ConversationManager>
{
	private struct ActionResult
	{
		public string nextConversation;

		public int nextLineIndex;
	}

	public SafeAction<uint> ClientEvent_OnStartConversation;

	public SafeAction<uint> ClientEvent_OnConversationLineRequestedCompletion;

	public SafeAction<uint, ShownConversation> ClientEvent_OnConversationShowLineAndRequestUserInput;

	public SafeAction<uint> ClientEvent_OnStopConversation;

	public readonly Dictionary<uint, DewConversationSettings> convSettings = new Dictionary<uint, DewConversationSettings>();

	private uint _nextConversationId = 1u;

	private readonly Dictionary<uint, DewConversationExecutionContext> _convContexts = new Dictionary<uint, DewConversationExecutionContext>();

	private bool _didStartNewConversation;

	public bool hasOngoingLocalConversation => ongoingLocalConversation != null;

	public DewConversationSettings ongoingLocalConversation { get; private set; }

	public override void OnStartServer()
	{
		base.OnStartServer();
		DewPlayer.onGamePlayerRemoved += new Action<DewPlayer>(OnGamePlayerRemoved);
		NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoadStarted += (Action<EventInfoLoadRoom>)((EventInfoLoadRoom _) =>
		{
			StopAllConversations();
		});
	}

	public override void OnStopServer()
	{
		base.OnStopServer();
		DewPlayer.onGamePlayerRemoved -= new Action<DewPlayer>(OnGamePlayerRemoved);
	}

	private void OnGamePlayerRemoved(DewPlayer obj)
	{
		uint[] array = convSettings.Keys.ToArray();
		foreach (uint num in array)
		{
			if (!((UnityEngine.Object)(object)convSettings[num].player != (UnityEngine.Object)(object)obj))
			{
				StopConversation(num);
			}
		}
	}

	private bool ValidateConversationSettings(DewConversationSettings s)
	{
		string startConversationKey = s.startConversationKey;
		s.startConversationKey = ResolveConversationKey(startConversationKey);
		if (s.startConversationKey == null)
		{
			Debug.LogWarning("Conversation '" + startConversationKey + "' not found");
			return false;
		}
		if ((UnityEngine.Object)(object)s.player == null || !s.player.isHumanPlayer)
		{
			Debug.LogWarning("Invalid player for conversation '" + s.startConversationKey + "'");
			return false;
		}
		return true;
	}

	private string ResolveConversationKey(string patternKey, string fromKey = null)
	{
		string text = ((fromKey != null && fromKey.Contains(".")) ? fromKey.Substring(0, fromKey.LastIndexOf(".", StringComparison.InvariantCulture)) : null);
		List<string> list = new List<string>();
		if (text != null)
		{
			if (DewLocalization.GetConversationData(text + "." + patternKey) != null)
			{
				return text + "." + patternKey;
			}
			foreach (string key in DewLocalization.data.conversations.Keys)
			{
				if (key.EqualsWildcard(text + "." + patternKey))
				{
					list.Add(key);
				}
			}
			if (list.Count > 0)
			{
				return list[UnityEngine.Random.Range(0, list.Count)];
			}
			list.Clear();
		}
		if (DewLocalization.GetConversationData(patternKey) != null)
		{
			return patternKey;
		}
		foreach (string key2 in DewLocalization.data.conversations.Keys)
		{
			if (key2.EqualsWildcard(patternKey))
			{
				list.Add(key2);
			}
		}
		if (list.Count > 0)
		{
			return list[UnityEngine.Random.Range(0, list.Count)];
		}
		Debug.LogWarning("Failed to resolve conversation key: " + patternKey + " (Scope: " + text + ")");
		return null;
	}

	[Server]
	public uint StartConversation(DewConversationSettings s)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.UInt32 ConversationManager::StartConversation(DewConversationSettings)' called when server was not active");
			return default;
		}
		if (NetworkedManagerBase<ZoneManager>.instance.isInAnyTransition)
		{
			return 0u;
		}
		if (!ValidateConversationSettings(s))
		{
			return 0u;
		}
		s._seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
		uint nextConversationId = _nextConversationId;
		_nextConversationId++;
		DewConversationExecutionContext dewConversationExecutionContext = new DewConversationExecutionContext();
		_convContexts.Add(nextConversationId, dewConversationExecutionContext);
		dewConversationExecutionContext.coroutine = ((MonoBehaviour)(object)this).StartCoroutine(ConversationRoutine_Imp(nextConversationId, s, dewConversationExecutionContext));
		return nextConversationId;
	}

	public IEnumerator StartConversationRoutine(DewConversationSettings s)
	{
		return new WaitForPromise((Action resolve, Action<Exception> reject) =>
		{
			DewConversationSettings dewConversationSettings = s;
			dewConversationSettings.onStop = (Action)Delegate.Combine(dewConversationSettings.onStop, resolve);
			StartConversation(s);
		});
	}

	private IEnumerator ConversationRoutine_Imp(uint id, DewConversationSettings s, DewConversationExecutionContext context)
	{
		uint[] array = convSettings.Keys.ToArray();
		foreach (uint num in array)
		{
			DewConversationSettings c = convSettings[num];
			if ((UnityEngine.Object)(object)c.player == (UnityEngine.Object)(object)s.player || s.speakers.Any((Entity spk) => c.speakers.Contains(spk)))
			{
				StopConversation(num);
			}
		}
		Entity[] speakers = s.speakers;
		foreach (Entity entity in speakers)
		{
			NetworkedManagerBase<ActorManager>.instance.serverActor.CreateStatusEffect<Se_InConversation>(entity, new CastInfo(entity));
		}
		if (s.rotateTowardsCenter)
		{
			Vector3 zero = Vector3.zero;
			int num2 = 0;
			speakers = s.speakers;
			foreach (Entity entity2 in speakers)
			{
				if (!entity2.IsNullInactiveDeadOrKnockedOut())
				{
					zero += entity2.agentPosition;
					num2++;
				}
			}
			zero /= (float)num2;
			speakers = s.speakers;
			foreach (Entity entity3 in speakers)
			{
				if (!entity3.IsNullInactiveDeadOrKnockedOut())
				{
					entity3.Control.RotateTowards(zero, immediately: false);
				}
			}
		}
		AddConversationAndInvokeEvents(id, s);
		context.currentLineIndex = 0;
		context.currentKey = s.startConversationKey;
		context.currentData = DewLocalization.GetConversationData(context.currentKey);
		List<int> choices = new List<int>();
		while (context.currentData != null && context.currentLineIndex < context.currentData.lines.Length && context.currentLineIndex >= 0 && !s.speakers.Any((Entity ent) => ent.IsNullInactiveDeadOrKnockedOut()))
		{
			LineData currentLine = context.currentData.lines[context.currentLineIndex];
			if (currentLine.type == LineType.Say)
			{
				choices.Clear();
				for (int num3 = context.currentLineIndex + 1; num3 < context.currentData.lines.Length && context.currentData.lines[num3].type == LineType.Choice; num3++)
				{
					choices.Add(num3);
				}
				if (choices.Count > 0)
				{
					yield return ShowLineWithChoicesAndWaitForChoice(id, context.currentKey, context.currentLineIndex, choices);
					context.currentLineIndex = context.userInput;
				}
				else
				{
					yield return ShowLineAndWaitForAdvance(id, context.currentKey, context.currentLineIndex);
					context.currentLineIndex++;
				}
				continue;
			}
			if (currentLine.type == LineType.Choice || currentLine.type == LineType.Action)
			{
				ActionResult res = new ActionResult
				{
					nextLineIndex = -1
				};
				ExecuteActionString(id, currentLine.actionString, ref res);
				yield return null;
				if (res.nextConversation != null)
				{
					context.currentLineIndex = 0;
					context.currentKey = ResolveConversationKey(res.nextConversation, context.currentKey);
					context.currentData = DewLocalization.GetConversationData(context.currentKey);
				}
				else if (res.nextLineIndex >= 0)
				{
					context.currentLineIndex = res.nextLineIndex;
				}
				else if (currentLine.type == LineType.Choice)
				{
					context.currentLineIndex++;
					while (context.currentLineIndex < context.currentData.lines.Length && context.currentData.lines[context.currentLineIndex].type == LineType.Choice)
					{
						context.currentLineIndex++;
					}
				}
				else
				{
					context.currentLineIndex++;
				}
				continue;
			}
			throw new ArgumentOutOfRangeException();
		}
		StopConversation(id);
	}

	[Server]
	private void AddConversationAndInvokeEvents(uint id, DewConversationSettings s)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void ConversationManager::AddConversationAndInvokeEvents(System.UInt32,DewConversationSettings)' called when server was not active");
			return;
		}
		convSettings.Add(id, s);
		RpcAddConversationAndInvokeEvents(id, s);
	}

	[ClientRpc]
	private void RpcAddConversationAndInvokeEvents(uint id, DewConversationSettings s)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteUInt((NetworkWriter)(object)val, id);
		GeneratedNetworkCode._Write_DewConversationSettings((NetworkWriter)(object)val, s);
		((NetworkBehaviour)this).SendRPCInternal("System.Void ConversationManager::RpcAddConversationAndInvokeEvents(System.UInt32,DewConversationSettings)", -594341626, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	private void AddConversationAndInvokeEvents_Imp(uint id, DewConversationSettings s)
	{
		if (convSettings.TryGetValue(id, out var value))
		{
			s = value;
		}
		else
		{
			convSettings.Add(id, s);
		}
		if (((NetworkBehaviour)s.player).isLocalPlayer && !hasOngoingLocalConversation)
		{
			ongoingLocalConversation = s;
			ManagerBase<ControlManager>.instance.DisableCharacterControls();
		}
		try
		{
			ClientEvent_OnStartConversation?.Invoke(id);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	private IEnumerator ShowLineAndWaitForAdvance(uint id, string currentKey, int currentLine)
	{
		DewConversationExecutionContext context = _convContexts[id];
		context.waitingForUserInput = true;
		context.userInput = -1;
		RpcShowLineAndRequestUserInput(id, currentKey, currentLine, null);
		yield return new WaitWhile(() => context.waitingForUserInput);
	}

	private IEnumerator ShowLineWithChoicesAndWaitForChoice(uint id, string currentKey, int currentLine, List<int> choices)
	{
		DewConversationExecutionContext context = _convContexts[id];
		context.waitingForUserInput = true;
		context.userInput = -1;
		RpcShowLineAndRequestUserInput(id, currentKey, currentLine, choices.ToArray());
		yield return new WaitWhile(() => context.waitingForUserInput);
		if (!choices.Contains(context.userInput))
		{
			context.userInput = choices[0];
		}
	}

	[Server]
	public void StopAllConversations()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void ConversationManager::StopAllConversations()' called when server was not active");
			return;
		}
		uint[] array = convSettings.Keys.ToArray();
		foreach (uint id in array)
		{
			StopConversation(id);
		}
	}

	[Server]
	public void StopConversation(uint id)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void ConversationManager::StopConversation(System.UInt32)' called when server was not active");
			return;
		}
		if (!_convContexts.ContainsKey(id))
		{
			Debug.LogWarning($"Tried to stop conversation with non-existent id: {id}");
			return;
		}
		((MonoBehaviour)(object)this).StopCoroutine(_convContexts[id].coroutine);
		_convContexts.Remove(id);
		Entity[] speakers = convSettings[id].speakers;
		foreach (Entity entity in speakers)
		{
			if (!entity.IsNullInactiveDeadOrKnockedOut() && entity.Status.TryGetStatusEffect<Se_InConversation>(out var effect))
			{
				effect.Destroy();
			}
		}
		RpcStopConversation(id);
	}

	[ClientRpc]
	private void RpcStopConversation(uint id)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteUInt((NetworkWriter)(object)val, id);
		((NetworkBehaviour)this).SendRPCInternal("System.Void ConversationManager::RpcStopConversation(System.UInt32)", 324059125, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[Command(requiresAuthority = false)]
	public void CmdRequestLineCompletion(uint id, int lineIndex, NetworkConnectionToClient sender = null)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteUInt((NetworkWriter)(object)val, id);
		NetworkWriterExtensions.WriteInt((NetworkWriter)(object)val, lineIndex);
		((NetworkBehaviour)this).SendCommandInternal("System.Void ConversationManager::CmdRequestLineCompletion(System.UInt32,System.Int32,Mirror.NetworkConnectionToClient)", 1106884890, (NetworkWriter)(object)val, 0, false);
		NetworkWriterPool.Return(val);
	}

	[ClientRpc]
	private void RpcCompleteLine(uint id)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteUInt((NetworkWriter)(object)val, id);
		((NetworkBehaviour)this).SendRPCInternal("System.Void ConversationManager::RpcCompleteLine(System.UInt32)", 1426307149, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[ClientRpc]
	private void RpcShowLineAndRequestUserInput(uint id, string key, int lineIndex, int[] choices)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteUInt((NetworkWriter)(object)val, id);
		NetworkWriterExtensions.WriteString((NetworkWriter)(object)val, key);
		NetworkWriterExtensions.WriteInt((NetworkWriter)(object)val, lineIndex);
		GeneratedNetworkCode._Write_System_002EInt32_005B_005D((NetworkWriter)(object)val, choices);
		((NetworkBehaviour)this).SendRPCInternal("System.Void ConversationManager::RpcShowLineAndRequestUserInput(System.UInt32,System.String,System.Int32,System.Int32[])", 270071100, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[Command(requiresAuthority = false)]
	public void CmdDoUserInputOnConversation(uint id, int lineIndexFrom, int userInput, NetworkConnectionToClient sender = null)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteUInt((NetworkWriter)(object)val, id);
		NetworkWriterExtensions.WriteInt((NetworkWriter)(object)val, lineIndexFrom);
		NetworkWriterExtensions.WriteInt((NetworkWriter)(object)val, userInput);
		((NetworkBehaviour)this).SendCommandInternal("System.Void ConversationManager::CmdDoUserInputOnConversation(System.UInt32,System.Int32,System.Int32,Mirror.NetworkConnectionToClient)", -416962420, (NetworkWriter)(object)val, 0, false);
		NetworkWriterPool.Return(val);
	}

	[Server]
	private void ExecuteActionString(uint convId, string actionString, ref ActionResult result)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void ConversationManager::ExecuteActionString(System.UInt32,System.String,ConversationManager/ActionResult&)' called when server was not active");
		}
		else
		{
			if (string.IsNullOrWhiteSpace(actionString))
			{
				return;
			}
			actionString = actionString.Trim();
			if (!actionString.Contains(";"))
			{
				try
				{
					int num = actionString.IndexOf(" ", StringComparison.InvariantCulture);
					string text = ((num < 0) ? actionString : actionString.Substring(0, num));
					string text2 = ((num < 0) ? null : actionString.Substring(num + 1));
					if (text.Equals("start", StringComparison.InvariantCultureIgnoreCase))
					{
						result = default;
						result.nextConversation = text2;
					}
					else if (text.Equals("goto", StringComparison.InvariantCultureIgnoreCase))
					{
						result = default;
						result.nextLineIndex = int.Parse(text2);
					}
					else if (text.Equals("start", StringComparison.InvariantCultureIgnoreCase))
					{
						result = default;
						result.nextConversation = text2;
					}
					else if (text.Equals("call", StringComparison.InvariantCultureIgnoreCase))
					{
						if (text2 == null || convSettings[convId].callFunctions == null || !convSettings[convId].callFunctions.TryGetValue(text2, out var value))
						{
							Debug.LogWarning("Call function not found: " + text2);
						}
						else
						{
							value();
						}
					}
					else if (!text.Equals("end", StringComparison.InvariantCultureIgnoreCase))
					{
						throw new ArgumentOutOfRangeException();
					}
					return;
				}
				catch (Exception exception)
				{
					Debug.LogWarning("Exception occured while executing ActionString '" + actionString + "'");
					Debug.LogException(exception);
					result = default;
					result.nextLineIndex = -1;
					return;
				}
			}
			string[] array = actionString.Split(";", StringSplitOptions.None);
			foreach (string actionString2 in array)
			{
				ExecuteActionString(convId, actionString2, ref result);
			}
		}
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcAddConversationAndInvokeEvents__UInt32__DewConversationSettings(uint id, DewConversationSettings s)
	{
		AddConversationAndInvokeEvents_Imp(id, s);
	}

	protected static void InvokeUserCode_RpcAddConversationAndInvokeEvents__UInt32__DewConversationSettings(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcAddConversationAndInvokeEvents called on server.");
		}
		else
		{
			((ConversationManager)(object)obj).UserCode_RpcAddConversationAndInvokeEvents__UInt32__DewConversationSettings(NetworkReaderExtensions.ReadUInt(reader), GeneratedNetworkCode._Read_DewConversationSettings(reader));
		}
	}

	protected void UserCode_RpcStopConversation__UInt32(uint id)
	{
		if (!convSettings.ContainsKey(id))
		{
			return;
		}
		DewConversationSettings dewConversationSettings = convSettings[id];
		if (dewConversationSettings.isLocalAuthority && hasOngoingLocalConversation)
		{
			ongoingLocalConversation = null;
			ManagerBase<ControlManager>.instance.EnableCharacterControls();
		}
		try
		{
			dewConversationSettings.onStop?.Invoke();
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		convSettings.Remove(id);
		try
		{
			ClientEvent_OnStopConversation?.Invoke(id);
		}
		catch (Exception exception2)
		{
			Debug.LogException(exception2);
		}
	}

	protected static void InvokeUserCode_RpcStopConversation__UInt32(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcStopConversation called on server.");
		}
		else
		{
			((ConversationManager)(object)obj).UserCode_RpcStopConversation__UInt32(NetworkReaderExtensions.ReadUInt(reader));
		}
	}

	protected void UserCode_CmdRequestLineCompletion__UInt32__Int32__NetworkConnectionToClient(uint id, int lineIndex, NetworkConnectionToClient sender)
	{
		if (_convContexts.ContainsKey(id))
		{
			DewConversationExecutionContext dewConversationExecutionContext = _convContexts[id];
			DewConversationSettings dewConversationSettings = convSettings[id];
			if (!((UnityEngine.Object)(object)sender.GetPlayer() != (UnityEngine.Object)(object)dewConversationSettings.player) && dewConversationExecutionContext.currentLineIndex == lineIndex)
			{
				RpcCompleteLine(id);
			}
		}
	}

	protected static void InvokeUserCode_CmdRequestLineCompletion__UInt32__Int32__NetworkConnectionToClient(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdRequestLineCompletion called on client.");
		}
		else
		{
			((ConversationManager)(object)obj).UserCode_CmdRequestLineCompletion__UInt32__Int32__NetworkConnectionToClient(NetworkReaderExtensions.ReadUInt(reader), NetworkReaderExtensions.ReadInt(reader), senderConnection);
		}
	}

	protected void UserCode_RpcCompleteLine__UInt32(uint id)
	{
		try
		{
			ClientEvent_OnConversationLineRequestedCompletion?.Invoke(id);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	protected static void InvokeUserCode_RpcCompleteLine__UInt32(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcCompleteLine called on server.");
		}
		else
		{
			((ConversationManager)(object)obj).UserCode_RpcCompleteLine__UInt32(NetworkReaderExtensions.ReadUInt(reader));
		}
	}

	protected void UserCode_RpcShowLineAndRequestUserInput__UInt32__String__Int32__Int32_005B_005D(uint id, string key, int lineIndex, int[] choices)
	{
		ClientEvent_OnConversationShowLineAndRequestUserInput?.Invoke(id, new ShownConversation
		{
			key = key,
			lineIndex = lineIndex,
			choices = choices
		});
	}

	protected static void InvokeUserCode_RpcShowLineAndRequestUserInput__UInt32__String__Int32__Int32_005B_005D(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcShowLineAndRequestUserInput called on server.");
		}
		else
		{
			((ConversationManager)(object)obj).UserCode_RpcShowLineAndRequestUserInput__UInt32__String__Int32__Int32_005B_005D(NetworkReaderExtensions.ReadUInt(reader), NetworkReaderExtensions.ReadString(reader), NetworkReaderExtensions.ReadInt(reader), GeneratedNetworkCode._Read_System_002EInt32_005B_005D(reader));
		}
	}

	protected void UserCode_CmdDoUserInputOnConversation__UInt32__Int32__Int32__NetworkConnectionToClient(uint id, int lineIndexFrom, int userInput, NetworkConnectionToClient sender)
	{
		if (_convContexts.ContainsKey(id))
		{
			DewConversationSettings dewConversationSettings = convSettings[id];
			DewConversationExecutionContext dewConversationExecutionContext = _convContexts[id];
			if (!((UnityEngine.Object)(object)sender.GetPlayer() != (UnityEngine.Object)(object)dewConversationSettings.player) && dewConversationExecutionContext.waitingForUserInput && dewConversationExecutionContext.currentLineIndex == lineIndexFrom)
			{
				dewConversationExecutionContext.userInput = userInput;
				dewConversationExecutionContext.waitingForUserInput = false;
			}
		}
	}

	protected static void InvokeUserCode_CmdDoUserInputOnConversation__UInt32__Int32__Int32__NetworkConnectionToClient(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdDoUserInputOnConversation called on client.");
		}
		else
		{
			((ConversationManager)(object)obj).UserCode_CmdDoUserInputOnConversation__UInt32__Int32__Int32__NetworkConnectionToClient(NetworkReaderExtensions.ReadUInt(reader), NetworkReaderExtensions.ReadInt(reader), NetworkReaderExtensions.ReadInt(reader), senderConnection);
		}
	}

	static ConversationManager()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected Obj, but got Unknown
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Expected Obj, but got Unknown
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Expected Obj, but got Unknown
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Expected Obj, but got Unknown
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Expected Obj, but got Unknown
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterCommand(typeof(ConversationManager), "System.Void ConversationManager::CmdRequestLineCompletion(System.UInt32,System.Int32,Mirror.NetworkConnectionToClient)", (RemoteCallDelegate)InvokeUserCode_CmdRequestLineCompletion__UInt32__Int32__NetworkConnectionToClient, false);
		RemoteProcedureCalls.RegisterCommand(typeof(ConversationManager), "System.Void ConversationManager::CmdDoUserInputOnConversation(System.UInt32,System.Int32,System.Int32,Mirror.NetworkConnectionToClient)", (RemoteCallDelegate)InvokeUserCode_CmdDoUserInputOnConversation__UInt32__Int32__Int32__NetworkConnectionToClient, false);
		RemoteProcedureCalls.RegisterRpc(typeof(ConversationManager), "System.Void ConversationManager::RpcAddConversationAndInvokeEvents(System.UInt32,DewConversationSettings)", (RemoteCallDelegate)InvokeUserCode_RpcAddConversationAndInvokeEvents__UInt32__DewConversationSettings);
		RemoteProcedureCalls.RegisterRpc(typeof(ConversationManager), "System.Void ConversationManager::RpcStopConversation(System.UInt32)", (RemoteCallDelegate)InvokeUserCode_RpcStopConversation__UInt32);
		RemoteProcedureCalls.RegisterRpc(typeof(ConversationManager), "System.Void ConversationManager::RpcCompleteLine(System.UInt32)", (RemoteCallDelegate)InvokeUserCode_RpcCompleteLine__UInt32);
		RemoteProcedureCalls.RegisterRpc(typeof(ConversationManager), "System.Void ConversationManager::RpcShowLineAndRequestUserInput(System.UInt32,System.String,System.Int32,System.Int32[])", (RemoteCallDelegate)InvokeUserCode_RpcShowLineAndRequestUserInput__UInt32__String__Int32__Int32_005B_005D);
	}
}
