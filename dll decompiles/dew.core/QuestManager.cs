using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class QuestManager : NetworkedManagerBase<QuestManager>
{
	public SafeAction<DewQuest> ClientEvent_OnQuestAdded;

	public SafeAction<DewQuest> ClientEvent_OnQuestRemoved;

	[NonSerialized]
	public List<DewQuest> activeQuests = new List<DewQuest>();

	public SafeAction<string, DewPlayer, Vector3> ClientEvent_OnArtifactPickedUp;

	public SafeAction<string> ClientEvent_OnArtifactRemoved;

	public SafeAction<string, bool> ClientEvent_OnArtifactAppraised;

	[CompilerGenerated]
	[SyncVar(hook = "OnCurrentArtifactChanged")]
	private string currentArtifact__BackingField;

	[NonSerialized]
	[SaveVar(SaveVarFlags.Default)]
	public bool didCollectArtifactThisLoop;

	[NonSerialized]
	[SaveVar(SaveVarFlags.Default)]
	public List<string> undiscoveredArtifacts = new List<string>();

	public Action<string, string> _Mirror_SyncVarHookDelegate__003CcurrentArtifact_003Ek__BackingField;

	public Type[] artifactPool { get; private set; }

	[SaveVar(SaveVarFlags.Default)]
	public string currentArtifact
	{
		[CompilerGenerated]
		get
		{
			return currentArtifact__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			Network_003CcurrentArtifact_003Ek__BackingField = value;
		}
	}

	public string Network_003CcurrentArtifact_003Ek__BackingField
	{
		get
		{
			return currentArtifact__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<string>(value, ref currentArtifact__BackingField, 1uL, _Mirror_SyncVarHookDelegate__003CcurrentArtifact_003Ek__BackingField);
		}
	}

	public override void OnStartServer()
	{
		base.OnStartServer();
		DoArtifactStartServer();
	}

	public override void OnStartClient()
	{
		base.OnStartClient();
		DoArtifactStartClient();
		ClientEvent_OnQuestAdded += (Action<DewQuest>)((DewQuest quest) =>
		{
			if (quest.isNewInstance)
			{
				((MonoBehaviour)(object)this).StartCoroutine(Routine());
			}
			IEnumerator Routine()
			{
				yield return new WaitForSecondsRealtime(0.5f);
				if (!quest.IsNullOrInactive() && quest.IsVisibleLocally())
				{
					NetworkedManagerBase<ChatManager>.instance.ShowMessageLocally(new ChatManager.Message
					{
						type = ChatManager.MessageType.Notice,
						content = "Chat_QuestStarted",
						args = new string[1] { quest.questTitleRaw }
					});
				}
			}
		});
		ClientEvent_OnQuestRemoved += (Action<DewQuest>)((DewQuest quest) =>
		{
			if (quest.IsVisibleLocally())
			{
				if (quest.state == QuestState.Completed)
				{
					NetworkedManagerBase<ChatManager>.instance.ShowMessageLocally(new ChatManager.Message
					{
						type = ChatManager.MessageType.Notice,
						content = "Chat_QuestCompleted",
						args = new string[1] { quest.questTitleRaw }
					});
				}
				else if (quest.state == QuestState.Failed)
				{
					string text = quest.questTitleRaw;
					if (quest.failReason != FailReason.NotSpecified)
					{
						text = text + " (" + DewLocalization.GetUIValue("InGame_Quest_Message_QuestFailed_Reason_" + quest.failReason) + ")";
					}
					NetworkedManagerBase<ChatManager>.instance.ShowMessageLocally(new ChatManager.Message
					{
						type = ChatManager.MessageType.Notice,
						content = "Chat_QuestFailed",
						args = new string[1] { text }
					});
				}
			}
		});
	}

	public bool TryGetQuest(Type type, out DewQuest quest)
	{
		quest = null;
		foreach (DewQuest activeQuest in activeQuests)
		{
			if (type.IsInstanceOfType(activeQuest))
			{
				quest = activeQuest;
				return true;
			}
		}
		return false;
	}

	public bool TryGetQuest<T>(out T quest) where T : DewQuest
	{
		bool result = TryGetQuest(typeof(T), out var quest2);
		quest = (T)quest2;
		return result;
	}

	public bool HasQuest<T>() where T : DewQuest
	{
		T quest;
		return TryGetQuest<T>(out quest);
	}

	public bool HasQuest(Type type)
	{
		DewQuest quest;
		return TryGetQuest(type, out quest);
	}

	[Server]
	public T StartQuest<T>(Action<T> beforePrepare = null) where T : DewQuest
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'T QuestManager::StartQuest(System.Action`1<T>)' called when server was not active");
			return null;
		}
		return StartQuest(DewResources.GetByType<T>(default(ResourceLoadSettings)), beforePrepare);
	}

	[Server]
	public T StartQuest<T>(T prefab, Action<T> beforePrepare = null) where T : DewQuest
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'T QuestManager::StartQuest(T,System.Action`1<T>)' called when server was not active");
			return null;
		}
		return Dew.CreateActor(prefab, Vector3.zero, Quaternion.identity, null, beforePrepare);
	}

	private void OnCurrentArtifactChanged(string from, string to)
	{
		if (from != null && to == null)
		{
			Color color = (DewResources.GetByShortTypeName<Artifact>(from, default(ResourceLoadSettings)).mainColor + Color.white) * 0.5f;
			color.a = 1f;
			InGameUIManager.instance.ShowWorldPopMessage(new WorldMessageSetting
			{
				color = color,
				rawText = string.Format(DewLocalization.GetUIValue("InGame_Message_ArtifactRemoved"), DewLocalization.GetArtifactName(DewLocalization.GetArtifactKey(from))),
				worldPosGetter = DewPlayer.local.hero.Visual.GetCenterPosition
			});
			try
			{
				ClientEvent_OnArtifactRemoved.Invoke(from);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
	}

	private void DoArtifactStartServer()
	{
		NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnLoopStarted += (Action)(() =>
		{
			didCollectArtifactThisLoop = false;
		});
		artifactPool = Dew.allArtifacts.Where((Type t) => Dew.IsArtifactIncludedInGame(t.Name) && !Dew.IsExcludedFromPool(t.Name)).ToArray();
	}

	private void DoArtifactStartClient()
	{
		string[] list = DewSave.profileMain.artifacts.Keys.Where((string key) => DewSave.profileMain.artifacts[key].status != UnlockStatus.Complete && Dew.IsArtifactIncludedInGame(key)).ToArray();
		AddToUndiscoveredArtifacts(list);
		InGameUIManager.instance.onStateChanged += (Action<string, string>)((string from, string to) =>
		{
			if (!((UnityEngine.Object)(object)DewPlayer.local == null))
			{
				DewPlayer.local.isReadingArtifactStory = to == "Artifact";
			}
		});
		if (currentArtifact != null)
		{
			ClientEvent_OnArtifactPickedUp?.Invoke(currentArtifact, null, Vector3.zero);
		}
	}

	[Server]
	public void PickUpArtifact(string artifactName, DewPlayer player, Vector3 worldPos)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void QuestManager::PickUpArtifact(System.String,DewPlayer,UnityEngine.Vector3)' called when server was not active");
			return;
		}
		Network_003CcurrentArtifact_003Ek__BackingField = artifactName;
		RpcInvokePickUpArtifact(artifactName, player, worldPos);
	}

	[Server]
	public void RemoveArtifact()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void QuestManager::RemoveArtifact()' called when server was not active");
		}
		else if (currentArtifact != null)
		{
			Network_003CcurrentArtifact_003Ek__BackingField = null;
		}
	}

	[ClientRpc]
	private void RpcInvokePickUpArtifact(string artifactName, DewPlayer player, Vector3 worldPos)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteString((NetworkWriter)(object)val, artifactName);
		NetworkWriterExtensions.WriteNetworkBehaviour((NetworkWriter)(object)val, (NetworkBehaviour)(object)player);
		NetworkWriterExtensions.WriteVector3((NetworkWriter)(object)val, worldPos);
		((NetworkBehaviour)this).SendRPCInternal("System.Void QuestManager::RpcInvokePickUpArtifact(System.String,DewPlayer,UnityEngine.Vector3)", 433620941, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[Command(requiresAuthority = false)]
	private void AddToUndiscoveredArtifacts(string[] list)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_System_002EString_005B_005D((NetworkWriter)(object)val, list);
		((NetworkBehaviour)this).SendCommandInternal("System.Void QuestManager::AddToUndiscoveredArtifacts(System.String[])", 1830451372, (NetworkWriter)(object)val, 0, false);
		NetworkWriterPool.Return(val);
	}

	[Server]
	public void DiscoverArtifactAndShowStory(string artifact, DewPlayer target = null)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void QuestManager::DiscoverArtifactAndShowStory(System.String,DewPlayer)' called when server was not active");
			return;
		}
		if ((UnityEngine.Object)(object)target != null && ((NetworkBehaviour)target).connectionToClient == null)
		{
			throw new InvalidOperationException();
		}
		if ((UnityEngine.Object)(object)target == null)
		{
			undiscoveredArtifacts.Remove(artifact);
			{
				foreach (DewPlayer gamePlayer in DewPlayer.gamePlayers)
				{
					TpcDiscoverArtifactAndShowStory(gamePlayer, artifact);
				}
				return;
			}
		}
		TpcDiscoverArtifactAndShowStory(target, artifact);
	}

	[TargetRpc]
	public void TpcDiscoverArtifactAndShowStory(NetworkConnectionToClient target, string artifact)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteString((NetworkWriter)(object)val, artifact);
		((NetworkBehaviour)this).SendTargetRPCInternal((NetworkConnection)(object)target, "System.Void QuestManager::TpcDiscoverArtifactAndShowStory(Mirror.NetworkConnectionToClient,System.String)", 1695452110, (NetworkWriter)(object)val, 0);
		NetworkWriterPool.Return(val);
	}

	private void DiscoverArtifactAndShowStoryLocal(string artifact)
	{
		ClientEvent_OnArtifactAppraised?.Invoke(artifact, DewSave.profileMain.artifacts[artifact].status != UnlockStatus.Complete);
		if (DewSave.profileMain.artifacts[artifact].status != UnlockStatus.Complete)
		{
			DewSave.profileMain.DiscoverArtifact(artifact);
			DewSave.SaveProfileMain();
		}
	}

	public QuestManager()
	{
		_Mirror_SyncVarHookDelegate__003CcurrentArtifact_003Ek__BackingField = OnCurrentArtifactChanged;
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcInvokePickUpArtifact__String__DewPlayer__Vector3(string artifactName, DewPlayer player, Vector3 worldPos)
	{
		ClientEvent_OnArtifactPickedUp?.Invoke(artifactName, player, worldPos);
	}

	protected static void InvokeUserCode_RpcInvokePickUpArtifact__String__DewPlayer__Vector3(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcInvokePickUpArtifact called on server.");
		}
		else
		{
			((QuestManager)(object)obj).UserCode_RpcInvokePickUpArtifact__String__DewPlayer__Vector3(NetworkReaderExtensions.ReadString(reader), NetworkReaderExtensions.ReadNetworkBehaviour<DewPlayer>(reader), NetworkReaderExtensions.ReadVector3(reader));
		}
	}

	protected void UserCode_AddToUndiscoveredArtifacts__String_005B_005D(string[] list)
	{
		foreach (string text in list)
		{
			if (Dew.IsArtifactIncludedInGame(text) && !undiscoveredArtifacts.Contains(text))
			{
				undiscoveredArtifacts.Add(text);
			}
		}
	}

	protected static void InvokeUserCode_AddToUndiscoveredArtifacts__String_005B_005D(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command AddToUndiscoveredArtifacts called on client.");
		}
		else
		{
			((QuestManager)(object)obj).UserCode_AddToUndiscoveredArtifacts__String_005B_005D(GeneratedNetworkCode._Read_System_002EString_005B_005D(reader));
		}
	}

	protected void UserCode_TpcDiscoverArtifactAndShowStory__NetworkConnectionToClient__String(NetworkConnectionToClient target, string artifact)
	{
		DiscoverArtifactAndShowStoryLocal(artifact);
	}

	protected static void InvokeUserCode_TpcDiscoverArtifactAndShowStory__NetworkConnectionToClient__String(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TpcDiscoverArtifactAndShowStory called on server.");
		}
		else
		{
			((QuestManager)(object)obj).UserCode_TpcDiscoverArtifactAndShowStory__NetworkConnectionToClient__String((NetworkConnectionToClient)(object)NetworkClient.connection, NetworkReaderExtensions.ReadString(reader));
		}
	}

	static QuestManager()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected Obj, but got Unknown
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Expected Obj, but got Unknown
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterCommand(typeof(QuestManager), "System.Void QuestManager::AddToUndiscoveredArtifacts(System.String[])", (RemoteCallDelegate)InvokeUserCode_AddToUndiscoveredArtifacts__String_005B_005D, false);
		RemoteProcedureCalls.RegisterRpc(typeof(QuestManager), "System.Void QuestManager::RpcInvokePickUpArtifact(System.String,DewPlayer,UnityEngine.Vector3)", (RemoteCallDelegate)InvokeUserCode_RpcInvokePickUpArtifact__String__DewPlayer__Vector3);
		RemoteProcedureCalls.RegisterRpc(typeof(QuestManager), "System.Void QuestManager::TpcDiscoverArtifactAndShowStory(Mirror.NetworkConnectionToClient,System.String)", (RemoteCallDelegate)InvokeUserCode_TpcDiscoverArtifactAndShowStory__NetworkConnectionToClient__String);
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		((NetworkBehaviour)this).SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteString(writer, currentArtifact__BackingField);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 1L) != 0L)
		{
			NetworkWriterExtensions.WriteString(writer, currentArtifact__BackingField);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		((NetworkBehaviour)this).DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<string>(ref currentArtifact__BackingField, _Mirror_SyncVarHookDelegate__003CcurrentArtifact_003Ek__BackingField, NetworkReaderExtensions.ReadString(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 1L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<string>(ref currentArtifact__BackingField, _Mirror_SyncVarHookDelegate__003CcurrentArtifact_003Ek__BackingField, NetworkReaderExtensions.ReadString(reader));
		}
	}
}
