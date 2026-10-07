using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class Rift_RoomExit : Rift, IBanRoomNodesNearbyMultiple
{
	public new static Rift_RoomExit softInstance;

	[SyncVar]
	public int nextNodeIndex = -1;

	[SyncVar]
	public bool forceTravelToNextZone;

	public new static Rift_RoomExit instance => Dew.Helper_GetInstanceOfActor(ref softInstance);

	public int NetworknextNodeIndex
	{
		get
		{
			return nextNodeIndex;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<int>(value, ref nextNodeIndex, 64uL, (Action<int, int>)null);
		}
	}

	public bool NetworkforceTravelToNextZone
	{
		get
		{
			return forceTravelToNextZone;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref forceTravelToNextZone, 128uL, (Action<bool, bool>)null);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		softInstance = this;
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if ((UnityEngine.Object)(object)softInstance == (UnityEngine.Object)(object)this)
		{
			softInstance = null;
		}
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (isNewInstance || !SingletonDewNetworkBehaviour<Room>.instance.isRevisit || NetworkedManagerBase<ZoneManager>.instance.currentNode.type != WorldNodeType.ExitBoss)
		{
			return;
		}
		GameManager.CallOnReady(() =>
		{
			isOpen = true;
			isLocked = NetworkedManagerBase<ActorManager>.instance.allActors.Any((Actor a) => a is Shrine_BossSoul);
		});
	}

	protected override bool OnInteractRift(Hero hero)
	{
		TpcInteract(hero.owner);
		return true;
	}

	[TargetRpc]
	public void TpcInteract(NetworkConnectionToClient target)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendTargetRPCInternal((NetworkConnection)(object)target, "System.Void Rift_RoomExit::TpcInteract(Mirror.NetworkConnectionToClient)", -397471636, (NetworkWriter)(object)val, 0);
		NetworkWriterPool.Return(val);
	}

	public void GetSidetrackPortalPositions(List<(Vector3, Quaternion)> list)
	{
		Vector3 vector = ((Component)(object)this).transform.position;
		Vector3 riftNavPos = Dew.GetValidAgentPosition(vector);
		Vector3 center = vector + ((Component)(object)this).transform.forward * 8f;
		float baseAngle = ((Component)(object)this).transform.eulerAngles.y;
		if (list.Count <= 3 && TryPos(new Vector3(3.75f, 0f, 1f), out var pos, out var rot))
		{
			list.Add((pos, rot));
		}
		if (list.Count <= 3 && TryPos(new Vector3(-3.75f, 0f, 1f), out pos, out rot))
		{
			list.Add((pos, rot));
		}
		if (list.Count <= 3 && TryPos(new Vector3(6.75f, 0f, 2.75f), out pos, out rot))
		{
			list.Add((pos, rot));
		}
		if (list.Count <= 3 && TryPos(new Vector3(-6.75f, 0f, 2.75f), out pos, out rot))
		{
			list.Add((pos, rot));
		}
		if (list.Count <= 3 && TryPos(new Vector3(2.55f, 0f, 3.85f), out pos, out rot))
		{
			list.Add((pos, rot));
		}
		if (list.Count <= 3 && TryPos(new Vector3(-2.55f, 0f, 3.85f), out pos, out rot))
		{
			list.Add((pos, rot));
		}
		bool TryPos(Vector3 offset, out Vector3 reference, out Quaternion reference2)
		{
			//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bf: Invalid comparison between Unknown and I4
			Quaternion quaternion = Quaternion.Euler(0f, baseAngle, 0f);
			Vector3 b = (reference = center + quaternion * Vector3.forward * -8f + quaternion * offset);
			reference = Dew.GetPositionOnGround(b);
			reference = Dew.GetValidAgentPosition(reference, 1f);
			if (Vector3.Distance(reference, b) > 1f)
			{
				reference2 = Quaternion.identity;
				return false;
			}
			reference2 = Quaternion.LookRotation(center - reference);
			return (int)Dew.GetNavMeshPathStatus(riftNavPos, reference) == 0;
		}
	}

	public Vector3[] GetBanSpots()
	{
		List<(Vector3, Quaternion)> list = new List<(Vector3, Quaternion)>();
		GetSidetrackPortalPositions(list);
		Vector3[] array = new Vector3[list.Count + 1];
		array[0] = ((Component)(object)this).transform.position;
		for (int i = 0; i < list.Count; i++)
		{
			array[i + 1] = list[i].Item1;
		}
		return array;
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_TpcInteract__NetworkConnectionToClient(NetworkConnectionToClient target)
	{
		InGameUIManager.instance.currentMockExit = this as Rift_MockExit;
		if (nextNodeIndex >= 0)
		{
			ManagerBase<MessageManager>.instance.ShowMessage(new DewMessageSettings
			{
				rawContent = DewLocalization.GetUIValue("InGame_Message_RiftMoveToNextLocation"),
				buttons = (DewMessageSettings.ButtonType.Yes | DewMessageSettings.ButtonType.Cancel),
				defaultButton = DewMessageSettings.ButtonType.Cancel,
				validator = () => !NetworkedManagerBase<ZoneManager>.instance.isInAnyTransition && !NetworkedManagerBase<ZoneManager>.instance.isVoting,
				owner = (UnityEngine.Object)(object)this,
				onClose = (DewMessageSettings.ButtonType b) =>
				{
					if (b == DewMessageSettings.ButtonType.Yes)
					{
						NetworkedManagerBase<ZoneManager>.instance.TravelWithValidationAndConfirmation(() =>
						{
							NetworkedManagerBase<ZoneManager>.instance.CmdTravelToNode(nextNodeIndex);
						});
					}
				}
			});
			return;
		}
		if (NetworkedManagerBase<ZoneManager>.instance.currentNode.type != WorldNodeType.ExitBoss && !forceTravelToNextZone)
		{
			InGameUIManager.instance.isWorldDisplayed = WorldDisplayStatus.Shown;
			return;
		}
		int num = DewBuildProfile.current.content.zoneCountByTier.Sum();
		if ((DewBuildProfile.current.buildType == BuildType.DemoLite || DewBuildProfile.current.HasFeature(BuildFeatureTag.Booth)) && NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex >= num - 1)
		{
			ManagerBase<MessageManager>.instance.ShowMessage(new DewMessageSettings
			{
				rawContent = DewLocalization.GetUIValue("InGame_Message_DemoFinished"),
				buttons = (DewMessageSettings.ButtonType.Yes | DewMessageSettings.ButtonType.No),
				defaultButton = DewMessageSettings.ButtonType.No,
				destructiveConfirm = true,
				onClose = (DewMessageSettings.ButtonType b) =>
				{
					if (b == DewMessageSettings.ButtonType.Yes)
					{
						NetworkedManagerBase<ZoneManager>.instance.CmdTravelToNextZone();
					}
				},
				validator = () => InGameUIManager.ValidateInGameActionMessage()
			});
		}
		else if (NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex % num == num - 1)
		{
			ManagerBase<MessageManager>.instance.ShowMessage(new DewMessageSettings
			{
				rawContent = DewLocalization.GetUIValue("InGame_Message_DreamAgain"),
				buttons = (DewMessageSettings.ButtonType.Yes | DewMessageSettings.ButtonType.No),
				defaultButton = DewMessageSettings.ButtonType.No,
				destructiveConfirm = true,
				onClose = (DewMessageSettings.ButtonType b) =>
				{
					if (b == DewMessageSettings.ButtonType.Yes)
					{
						NetworkedManagerBase<ZoneManager>.instance.CmdTravelToNextZone();
					}
				},
				validator = () => InGameUIManager.ValidateInGameActionMessage()
			});
		}
		else
		{
			NetworkedManagerBase<ZoneManager>.instance.TravelWithValidationAndConfirmation(() =>
			{
				NetworkedManagerBase<ZoneManager>.instance.CmdTravelToNextZone();
			});
		}
	}

	protected static void InvokeUserCode_TpcInteract__NetworkConnectionToClient(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TpcInteract called on server.");
		}
		else
		{
			((Rift_RoomExit)(object)obj).UserCode_TpcInteract__NetworkConnectionToClient((NetworkConnectionToClient)(object)NetworkClient.connection);
		}
	}

	static Rift_RoomExit()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterRpc(typeof(Rift_RoomExit), "System.Void Rift_RoomExit::TpcInteract(Mirror.NetworkConnectionToClient)", (RemoteCallDelegate)InvokeUserCode_TpcInteract__NetworkConnectionToClient);
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteInt(writer, nextNodeIndex);
			NetworkWriterExtensions.WriteBool(writer, forceTravelToNextZone);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x40L) != 0L)
		{
			NetworkWriterExtensions.WriteInt(writer, nextNodeIndex);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x80L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, forceTravelToNextZone);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref nextNodeIndex, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref forceTravelToNextZone, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x40L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref nextNodeIndex, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
		}
		if ((num & 0x80L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref forceTravelToNextZone, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
		}
	}
}
