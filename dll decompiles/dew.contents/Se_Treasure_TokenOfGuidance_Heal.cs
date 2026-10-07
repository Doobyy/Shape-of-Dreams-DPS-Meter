using System;
using Mirror;
using UnityEngine;

public class Se_Treasure_TokenOfGuidance_Heal : TempEffect
{
	public float missingHpRatio = 0.4f;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
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
		if (!obj.isTraveling)
		{
			return;
		}
		NetworkedManagerBase<ZoneManager>.instance.CallOnReadyAfterTransition(() =>
		{
			CreateStatusEffect(victim, (Se_GenericHealOverTime se) =>
			{
				se.Setup(victim.Status.missingHealth * missingHpRatio, 0.2f, 6);
			});
		});
	}

	private void MirrorProcessed()
	{
	}
}
