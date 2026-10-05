using System;
using Mirror;
using UnityEngine;

[SaveActor(true)]
public class TempEffect : StackedStatusEffect
{
	public int riftTravelCount = 4;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			SetStack(riftTravelCount);
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
		if (obj.isTraveling)
		{
			NetworkedManagerBase<ZoneManager>.instance.CallOnReadyAfterTransition(() =>
			{
				RemoveStack();
			});
		}
	}

	private void MirrorProcessed()
	{
	}
}
