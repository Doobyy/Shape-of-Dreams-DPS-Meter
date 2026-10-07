using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public abstract class DewQuest : GameEffect
{
	public class ReachNodeGoal
	{
		public bool isActive = true;

		public NextGoalSettings settings;

		public bool waitingForZoneIndex;

		public int zoneIndex = -1;

		public int nodeIndex = -1;

		public string addedRoomName;

		public List<int> addedModifierIds = new List<int>();
	}

	public const string DefaultStep = "Default";

	public QuestType type;

	[CompilerGenerated]
	[SyncVar(hook = "OnProgressTypeChanged")]
	private QuestProgressType progressType__BackingField;

	[CompilerGenerated]
	[SyncVar(hook = "OnProgressChanged")]
	private string currentProgress__BackingField;

	private string _questTitleRaw;

	private string _questShortDescriptionRaw;

	private string _questDetailedDescriptionRaw;

	[CompilerGenerated]
	[SyncVar]
	private FailReason failReason__BackingField;

	[CompilerGenerated]
	[SyncVar]
	private QuestState state__BackingField;

	public SafeAction ClientQuestEvent_OnQuestUpdated;

	[CompilerGenerated]
	[SyncVar(hook = "OnStepChanged")]
	private string step__BackingField;

	private bool _isFirstStep = true;

	[NonSerialized]
	[SaveVar(SaveVarFlags.Default)]
	private List<ReachNodeGoal> _currentReachNodeGoals = new List<ReachNodeGoal>();

	private int _hydrationIndex;

	public Action<QuestProgressType, QuestProgressType> _Mirror_SyncVarHookDelegate__003CprogressType_003Ek__BackingField;

	public Action<string, string> _Mirror_SyncVarHookDelegate__003CcurrentProgress_003Ek__BackingField;

	public Action<string, string> _Mirror_SyncVarHookDelegate__003Cstep_003Ek__BackingField;

	public QuestProgressType progressType
	{
		[CompilerGenerated]
		get
		{
			return progressType__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CprogressType_003Ek__BackingField = value;
		}
	}

	public string currentProgress
	{
		[CompilerGenerated]
		get
		{
			return currentProgress__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CcurrentProgress_003Ek__BackingField = value;
		}
	}

	public string questTitleRaw
	{
		get
		{
			return _questTitleRaw;
		}
		set
		{
			_questTitleRaw = value;
			ClientQuestEvent_OnQuestUpdated?.Invoke();
		}
	}

	public string questShortDescriptionRaw
	{
		get
		{
			return _questShortDescriptionRaw;
		}
		set
		{
			_questShortDescriptionRaw = value;
			ClientQuestEvent_OnQuestUpdated?.Invoke();
		}
	}

	public string questDetailedDescriptionRaw
	{
		get
		{
			return _questDetailedDescriptionRaw;
		}
		set
		{
			_questDetailedDescriptionRaw = value;
			ClientQuestEvent_OnQuestUpdated?.Invoke();
		}
	}

	public FailReason failReason
	{
		[CompilerGenerated]
		get
		{
			return failReason__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CfailReason_003Ek__BackingField = value;
		}
	}

	public QuestState state
	{
		[CompilerGenerated]
		get
		{
			return state__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003Cstate_003Ek__BackingField = value;
		}
	}

	[SaveVar(SaveVarFlags.ApplyAfterCreation)]
	public string step
	{
		[CompilerGenerated]
		get
		{
			return step__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003Cstep_003Ek__BackingField = value;
		}
	}

	public QuestProgressType Network_003CprogressType_003Ek__BackingField
	{
		get
		{
			return progressType__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<QuestProgressType>(value, ref progressType__BackingField, 8uL, _Mirror_SyncVarHookDelegate__003CprogressType_003Ek__BackingField);
		}
	}

	public string Network_003CcurrentProgress_003Ek__BackingField
	{
		get
		{
			return currentProgress__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<string>(value, ref currentProgress__BackingField, 16uL, _Mirror_SyncVarHookDelegate__003CcurrentProgress_003Ek__BackingField);
		}
	}

	public FailReason Network_003CfailReason_003Ek__BackingField
	{
		get
		{
			return failReason__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<FailReason>(value, ref failReason__BackingField, 32uL, (Action<FailReason, FailReason>)null);
		}
	}

	public QuestState Network_003Cstate_003Ek__BackingField
	{
		get
		{
			return state__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<QuestState>(value, ref state__BackingField, 64uL, (Action<QuestState, QuestState>)null);
		}
	}

	public string Network_003Cstep_003Ek__BackingField
	{
		get
		{
			return step__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<string>(value, ref step__BackingField, 128uL, _Mirror_SyncVarHookDelegate__003Cstep_003Ek__BackingField);
		}
	}

	private void OnProgressTypeChanged(QuestProgressType _, QuestProgressType __)
	{
		ClientQuestEvent_OnQuestUpdated?.Invoke();
	}

	private void OnProgressChanged(string _, string __)
	{
		ClientQuestEvent_OnQuestUpdated?.Invoke();
	}

	private void OnStepChanged(string from, string to)
	{
		if (from != null)
		{
			try
			{
				if (NetworkServer.active)
				{
					OnStepEnded(from);
				}
				else
				{
					Dew.CallDelayed(() =>
					{
						OnStepEnded(from);
					});
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
		if (to == null)
		{
			return;
		}
		try
		{
			if (NetworkServer.active)
			{
				OnStepStarted(_isFirstStep && !isNewInstance);
				_isFirstStep = false;
				return;
			}
			Dew.CallDelayed(() =>
			{
				OnStepStarted(_isFirstStep && !isNewInstance);
				_isFirstStep = false;
			});
		}
		catch (Exception exception2)
		{
			Debug.LogException(exception2);
		}
	}

	public virtual string GetInitialStep()
	{
		return "Default";
	}

	protected virtual void OnStepStarted(bool isLoadedFromSave)
	{
		Debug.Log(((object)this).GetType().Name + " started step '" + step + "' " + (isLoadedFromSave ? "(LoadedFromSave)" : ""));
	}

	protected virtual void OnStepEnded(string formerStep)
	{
		Debug.Log(((object)this).GetType().Name + " ended step '" + formerStep + "'");
		OnStepEnded_ReachNode();
	}

	[Server]
	public void EndStep(string nextStep = null)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void DewQuest::EndStep(System.String)' called when server was not active");
		}
		else
		{
			Network_003Cstep_003Ek__BackingField = nextStep;
		}
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		NetworkedManagerBase<QuestManager>.instance.activeQuests.Add(this);
		if (string.IsNullOrEmpty(questTitleRaw))
		{
			questTitleRaw = DewLocalization.GetUIValue(((object)this).GetType().Name + "_Title");
		}
		if (string.IsNullOrEmpty(questShortDescriptionRaw))
		{
			questShortDescriptionRaw = DewLocalization.GetUIValue(((object)this).GetType().Name + "_Description");
		}
		NetworkedManagerBase<QuestManager>.instance.ClientEvent_OnQuestAdded?.Invoke(this);
		OnCreate_ReachNode();
		if (((NetworkBehaviour)this).isServer && isNewInstance)
		{
			Network_003Cstep_003Ek__BackingField = GetInitialStep();
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		NetworkedManagerBase<QuestManager>.instance.activeQuests.Remove(this);
		NetworkedManagerBase<QuestManager>.instance.ClientEvent_OnQuestRemoved?.Invoke(this);
		OnDestroyActor_ReachNode();
		if (((NetworkBehaviour)this).isServer && step != null)
		{
			EndStep();
		}
	}

	public virtual bool IsVisibleLocally()
	{
		return true;
	}

	[Server]
	public void CompleteQuest()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void DewQuest::CompleteQuest()' called when server was not active");
		}
		else if (isActive)
		{
			Network_003Cstate_003Ek__BackingField = QuestState.Completed;
			Destroy();
		}
	}

	[Server]
	public void FailQuest(FailReason reason)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void DewQuest::FailQuest(FailReason)' called when server was not active");
		}
		else if (isActive)
		{
			Network_003Cstate_003Ek__BackingField = QuestState.Failed;
			Network_003CfailReason_003Ek__BackingField = reason;
			Destroy();
		}
	}

	[ClientRpc]
	public void RpcInvokeQuestUpdated()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendRPCInternal("System.Void DewQuest::RpcInvokeQuestUpdated()", -1691655609, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[ClientRpc]
	public void SetLocalizedTitle(string key)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteString((NetworkWriter)(object)val, key);
		((NetworkBehaviour)this).SendRPCInternal("System.Void DewQuest::SetLocalizedTitle(System.String)", -147393230, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[ClientRpc]
	public void SetLocalizedDescription(string key)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteString((NetworkWriter)(object)val, key);
		((NetworkBehaviour)this).SendRPCInternal("System.Void DewQuest::SetLocalizedDescription(System.String)", -199031730, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	private void OnCreate_ReachNode()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded += new Action<EventInfoLoadRoom>(OnRoomLoaded_ReachNode);
		NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnNodesChanged += new Action(OnNodesChanged_ReachNode);
		foreach (ReachNodeGoal currentReachNodeGoal in _currentReachNodeGoals)
		{
			OnReachNodeGoalAdded(currentReachNodeGoal);
		}
	}

	private void OnDestroyActor_ReachNode()
	{
		if (((NetworkBehaviour)this).isServer && !((UnityEngine.Object)(object)NetworkedManagerBase<ZoneManager>.instance == null))
		{
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded -= new Action<EventInfoLoadRoom>(OnRoomLoaded_ReachNode);
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnNodesChanged -= new Action(OnNodesChanged_ReachNode);
		}
	}

	private void OnStepEnded_ReachNode()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		foreach (ReachNodeGoal currentReachNodeGoal in _currentReachNodeGoals)
		{
			if (NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex != currentReachNodeGoal.zoneIndex)
			{
				continue;
			}
			for (int i = 0; i < currentReachNodeGoal.settings.addedModifiers.Length; i++)
			{
				if (currentReachNodeGoal.settings.addedModifiers[i].revertOnStepEnd)
				{
					NetworkedManagerBase<ZoneManager>.instance.RemoveModifier(currentReachNodeGoal.nodeIndex, currentReachNodeGoal.addedModifierIds[i]);
				}
			}
		}
		_currentReachNodeGoals.Clear();
		_hydrationIndex = 0;
	}

	private void OnNodesChanged_ReachNode()
	{
		List<ReachNodeGoal> list = _currentReachNodeGoals.ToListNonAlloc(out var handle);
		for (int i = 0; i < list.Count; i++)
		{
			ReachNodeGoal reachNodeGoal = list[i];
			if (reachNodeGoal.isActive && reachNodeGoal.zoneIndex == NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex && reachNodeGoal.nodeIndex >= 0 && reachNodeGoal.nodeIndex < NetworkedManagerBase<ZoneManager>.instance.nodes.Count)
			{
				WorldNodeData n = NetworkedManagerBase<ZoneManager>.instance.nodes[reachNodeGoal.nodeIndex];
				if (reachNodeGoal.addedModifierIds.Count > 0 && reachNodeGoal.addedModifierIds.Any((int modId) => !n.HasModifier(modId)))
				{
					reachNodeGoal.settings.InvokeOnFail(n.HasModifier<RoomMod_Hunted>() ? FailReason.HunterOccupied : FailReason.NotSpecified);
					reachNodeGoal.isActive = false;
				}
				else if (reachNodeGoal.addedRoomName != null && n.room != reachNodeGoal.addedRoomName)
				{
					reachNodeGoal.settings.InvokeOnFail(FailReason.NotSpecified);
					reachNodeGoal.isActive = false;
				}
			}
		}
		handle.Return();
	}

	private void OnRoomLoaded_ReachNode(EventInfoLoadRoom obj)
	{
		List<ReachNodeGoal> list = _currentReachNodeGoals.ToListNonAlloc(out var handle);
		for (int num = list.Count - 1; num >= 0; num--)
		{
			ReachNodeGoal reachNodeGoal = list[num];
			if (!reachNodeGoal.isActive)
			{
				continue;
			}
			if (reachNodeGoal.zoneIndex != NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex && !reachNodeGoal.waitingForZoneIndex)
			{
				reachNodeGoal.settings.InvokeOnFail(FailReason.MissedDestination);
				reachNodeGoal.isActive = false;
			}
			else
			{
				if (reachNodeGoal.zoneIndex != NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex)
				{
					continue;
				}
				if (reachNodeGoal.waitingForZoneIndex)
				{
					if (NetworkedManagerBase<ZoneManager>.instance.currentZone.useSpecialGeneration)
					{
						reachNodeGoal.settings.InvokeOnFail(FailReason.MissedDestination);
						reachNodeGoal.isActive = false;
						continue;
					}
					reachNodeGoal.waitingForZoneIndex = false;
					NetworkedManagerBase<ZoneManager>.instance.TryGetNodeIndexForNextGoal(reachNodeGoal.settings.nodeIndexSettings, out reachNodeGoal.nodeIndex);
					ApplyOverrides(reachNodeGoal);
					OnReachNodeGoalAdded(reachNodeGoal);
				}
				if (reachNodeGoal.nodeIndex != NetworkedManagerBase<ZoneManager>.instance.currentNodeIndex && NetworkedManagerBase<ZoneManager>.instance.currentNode.type == WorldNodeType.ExitBoss)
				{
					reachNodeGoal.settings.InvokeOnFail(FailReason.MissedDestination);
					reachNodeGoal.isActive = false;
				}
				else if (reachNodeGoal.nodeIndex == NetworkedManagerBase<ZoneManager>.instance.currentNodeIndex)
				{
					reachNodeGoal.settings.InvokeOnReachDestination();
					reachNodeGoal.isActive = false;
				}
			}
		}
		handle.Return();
	}

	[Server]
	public void AddStepGoal_ReachNode(NextGoalSettings settings)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void DewQuest::AddStepGoal_ReachNode(NextGoalSettings)' called when server was not active");
		}
		else
		{
			if (!isActive)
			{
				return;
			}
			if (_currentReachNodeGoals.Count > _hydrationIndex)
			{
				_currentReachNodeGoals[_hydrationIndex].settings.onReachDestination = settings.onReachDestination;
				_currentReachNodeGoals[_hydrationIndex].settings.onFail = settings.onFail;
				_hydrationIndex++;
				return;
			}
			_hydrationIndex++;
			bool flag = !NetworkedManagerBase<ZoneManager>.instance.TryGetNodeIndexForNextGoal(settings.nodeIndexSettings, out var nodeIndex) || NetworkedManagerBase<ZoneManager>.instance.GetHunterProgress() > 0.55f;
			if (!settings.ignoreSuboptimalSituation & flag)
			{
				ReachNodeGoal reachNodeGoal = new ReachNodeGoal
				{
					zoneIndex = NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex + 1,
					settings = settings,
					waitingForZoneIndex = true
				};
				_currentReachNodeGoals.Add(reachNodeGoal);
				OnReachNodeGoalAdded(reachNodeGoal);
			}
			else
			{
				ReachNodeGoal reachNodeGoal2 = new ReachNodeGoal
				{
					settings = settings,
					zoneIndex = NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex,
					nodeIndex = nodeIndex
				};
				ApplyOverrides(reachNodeGoal2);
				_currentReachNodeGoals.Add(reachNodeGoal2);
				OnReachNodeGoalAdded(reachNodeGoal2);
			}
		}
	}

	private void ApplyOverrides(ReachNodeGoal g)
	{
		if (g.settings.addedModifiers != null)
		{
			for (int i = 0; i < g.settings.addedModifiers.Length; i++)
			{
				AddedModifierData addedModifierData = g.settings.addedModifiers[i];
				int item = NetworkedManagerBase<ZoneManager>.instance.AddModifier(g.nodeIndex, new ModifierData
				{
					type = addedModifierData.type,
					clientData = addedModifierData.clientData,
					isForceRevealed = addedModifierData.isForceRevealed
				});
				g.addedModifierIds.Add(item);
			}
		}
		if (!string.IsNullOrEmpty(g.settings.roomOverride))
		{
			NetworkedManagerBase<ZoneManager>.instance.SetRoomOverride(g.nodeIndex, g.settings.roomOverride);
			WorldNodeData worldNodeData = NetworkedManagerBase<ZoneManager>.instance.nodes[g.nodeIndex];
			if (worldNodeData.status == WorldNodeStatus.HasVisited)
			{
				worldNodeData.status = WorldNodeStatus.Revealed;
			}
			NetworkedManagerBase<ZoneManager>.instance.nodes[g.nodeIndex] = worldNodeData;
		}
	}

	private void OnReachNodeGoalAdded(ReachNodeGoal g)
	{
		if (!g.settings.dontChangeTitle)
		{
			SetLocalizedTitle(g.settings.localizedTitleKey);
		}
		if (!g.settings.dontChangeDescription)
		{
			SetLocalizedDescription((!g.waitingForZoneIndex) ? g.settings.localizedDescriptionKey : "Quest_TravelToNextWorld");
		}
	}

	protected DewQuest()
	{
		_Mirror_SyncVarHookDelegate__003CprogressType_003Ek__BackingField = OnProgressTypeChanged;
		_Mirror_SyncVarHookDelegate__003CcurrentProgress_003Ek__BackingField = OnProgressChanged;
		_Mirror_SyncVarHookDelegate__003Cstep_003Ek__BackingField = OnStepChanged;
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcInvokeQuestUpdated()
	{
		ClientQuestEvent_OnQuestUpdated?.Invoke();
	}

	protected static void InvokeUserCode_RpcInvokeQuestUpdated(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcInvokeQuestUpdated called on server.");
		}
		else
		{
			((DewQuest)(object)obj).UserCode_RpcInvokeQuestUpdated();
		}
	}

	protected void UserCode_SetLocalizedTitle__String(string key)
	{
		if (string.IsNullOrEmpty(key))
		{
			key = ((object)this).GetType().Name + "_Title";
		}
		questTitleRaw = DewLocalization.GetUIValue(key);
	}

	protected static void InvokeUserCode_SetLocalizedTitle__String(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC SetLocalizedTitle called on server.");
		}
		else
		{
			((DewQuest)(object)obj).UserCode_SetLocalizedTitle__String(NetworkReaderExtensions.ReadString(reader));
		}
	}

	protected void UserCode_SetLocalizedDescription__String(string key)
	{
		if (string.IsNullOrEmpty(key))
		{
			key = ((object)this).GetType().Name + "_Description";
		}
		questShortDescriptionRaw = DewLocalization.GetUIValue(key);
	}

	protected static void InvokeUserCode_SetLocalizedDescription__String(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC SetLocalizedDescription called on server.");
		}
		else
		{
			((DewQuest)(object)obj).UserCode_SetLocalizedDescription__String(NetworkReaderExtensions.ReadString(reader));
		}
	}

	static DewQuest()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Expected Obj, but got Unknown
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterRpc(typeof(DewQuest), "System.Void DewQuest::RpcInvokeQuestUpdated()", (RemoteCallDelegate)InvokeUserCode_RpcInvokeQuestUpdated);
		RemoteProcedureCalls.RegisterRpc(typeof(DewQuest), "System.Void DewQuest::SetLocalizedTitle(System.String)", (RemoteCallDelegate)InvokeUserCode_SetLocalizedTitle__String);
		RemoteProcedureCalls.RegisterRpc(typeof(DewQuest), "System.Void DewQuest::SetLocalizedDescription(System.String)", (RemoteCallDelegate)InvokeUserCode_SetLocalizedDescription__String);
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			GeneratedNetworkCode._Write_QuestProgressType(writer, progressType__BackingField);
			NetworkWriterExtensions.WriteString(writer, currentProgress__BackingField);
			GeneratedNetworkCode._Write_FailReason(writer, failReason__BackingField);
			GeneratedNetworkCode._Write_QuestState(writer, state__BackingField);
			NetworkWriterExtensions.WriteString(writer, step__BackingField);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 8L) != 0L)
		{
			GeneratedNetworkCode._Write_QuestProgressType(writer, progressType__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x10L) != 0L)
		{
			NetworkWriterExtensions.WriteString(writer, currentProgress__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x20L) != 0L)
		{
			GeneratedNetworkCode._Write_FailReason(writer, failReason__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x40L) != 0L)
		{
			GeneratedNetworkCode._Write_QuestState(writer, state__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x80L) != 0L)
		{
			NetworkWriterExtensions.WriteString(writer, step__BackingField);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<QuestProgressType>(ref progressType__BackingField, _Mirror_SyncVarHookDelegate__003CprogressType_003Ek__BackingField, GeneratedNetworkCode._Read_QuestProgressType(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<string>(ref currentProgress__BackingField, _Mirror_SyncVarHookDelegate__003CcurrentProgress_003Ek__BackingField, NetworkReaderExtensions.ReadString(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<FailReason>(ref failReason__BackingField, (Action<FailReason, FailReason>)null, GeneratedNetworkCode._Read_FailReason(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<QuestState>(ref state__BackingField, (Action<QuestState, QuestState>)null, GeneratedNetworkCode._Read_QuestState(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<string>(ref step__BackingField, _Mirror_SyncVarHookDelegate__003Cstep_003Ek__BackingField, NetworkReaderExtensions.ReadString(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 8L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<QuestProgressType>(ref progressType__BackingField, _Mirror_SyncVarHookDelegate__003CprogressType_003Ek__BackingField, GeneratedNetworkCode._Read_QuestProgressType(reader));
		}
		if ((num & 0x10L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<string>(ref currentProgress__BackingField, _Mirror_SyncVarHookDelegate__003CcurrentProgress_003Ek__BackingField, NetworkReaderExtensions.ReadString(reader));
		}
		if ((num & 0x20L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<FailReason>(ref failReason__BackingField, (Action<FailReason, FailReason>)null, GeneratedNetworkCode._Read_FailReason(reader));
		}
		if ((num & 0x40L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<QuestState>(ref state__BackingField, (Action<QuestState, QuestState>)null, GeneratedNetworkCode._Read_QuestState(reader));
		}
		if ((num & 0x80L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<string>(ref step__BackingField, _Mirror_SyncVarHookDelegate__003Cstep_003Ek__BackingField, NetworkReaderExtensions.ReadString(reader));
		}
	}
}
