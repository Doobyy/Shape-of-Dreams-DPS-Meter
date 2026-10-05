using System;
using Mirror;
using UnityEngine;

public class LucidDream_BonVoyage : LucidDream
{
	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			NetworkedManagerBase<ZoneManager>.instance.isHuntAdvanceDisabled = true;
			NetworkedManagerBase<ZoneManager>.instance.hunterStartNodeIndex = -1;
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded += new Action<EventInfoLoadRoom>(ClientEventOnRoomLoaded);
		}
	}

	private void ClientEventOnRoomLoaded(EventInfoLoadRoom obj)
	{
		NetworkedManagerBase<ZoneManager>.instance.isHuntAdvanceDisabled = true;
		NetworkedManagerBase<ZoneManager>.instance.hunterStartNodeIndex = -1;
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)NetworkedManagerBase<ZoneManager>.instance != null)
		{
			NetworkedManagerBase<ZoneManager>.instance.isHuntAdvanceDisabled = false;
			NetworkedManagerBase<ZoneManager>.instance.hunterStartNodeIndex = 0;
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded -= new Action<EventInfoLoadRoom>(ClientEventOnRoomLoaded);
		}
	}

	private void MirrorProcessed()
	{
	}
}
