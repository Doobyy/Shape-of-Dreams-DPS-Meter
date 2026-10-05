using System;
using Mirror;
using UnityEngine;

public class Ge_Shrine_PotOfGreed : GameEffect
{
	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			if (isNewInstance)
			{
				NetworkedManagerBase<ChatManager>.instance.BroadcastMessage(new ChatManager.Message
				{
					type = ChatManager.MessageType.Notice,
					content = "Chat_Notice_PotOfGreedChallengeAccepted"
				});
			}
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded += new Action<EventInfoLoadRoom>(ClientEventOnRoomLoaded);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)NetworkedManagerBase<ZoneManager>.instance != null)
		{
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded -= new Action<EventInfoLoadRoom>(ClientEventOnRoomLoaded);
		}
	}

	private void ClientEventOnRoomLoaded(EventInfoLoadRoom obj)
	{
		if (obj.isTraveling && !SingletonDewNetworkBehaviour<Room>.instance.isRevisit && !SingletonDewNetworkBehaviour<Room>.instance.didClearRoom && !(SingletonBehaviour<Special_RoomAnnouncer>.instance != null) && !NetworkedManagerBase<ZoneManager>.instance.currentNode.HasMainModifier() && !NetworkedManagerBase<ZoneManager>.instance.isSidetracking && NetworkedManagerBase<ZoneManager>.instance.currentNode.type == WorldNodeType.Combat)
		{
			NetworkedManagerBase<ZoneManager>.instance.AddModifier<RoomMod_ChallengingFight>(NetworkedManagerBase<ZoneManager>.instance.currentNodeIndex);
			Destroy();
		}
	}

	private void MirrorProcessed()
	{
	}
}
