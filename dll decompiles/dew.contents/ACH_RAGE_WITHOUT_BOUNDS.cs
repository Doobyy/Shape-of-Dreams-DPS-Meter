using System;
using UnityEngine;

[AchUnlockOnComplete(typeof(Hero_Vesper))]
public class ACH_RAGE_WITHOUT_BOUNDS : DewAchievementItem
{
	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded += new Action<EventInfoLoadRoom>(ClientEventOnRoomLoaded);
	}

	public override void OnStopLocalClient()
	{
		base.OnStopLocalClient();
		if ((UnityEngine.Object)(object)NetworkedManagerBase<ZoneManager>.instance != null)
		{
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded -= new Action<EventInfoLoadRoom>(ClientEventOnRoomLoaded);
		}
	}

	private void ClientEventOnRoomLoaded(EventInfoLoadRoom _)
	{
		GameManager.CallOnReady(() =>
		{
			if (NetworkedManagerBase<ZoneManager>.instance.currentZone.name.Contains("Sky", StringComparison.InvariantCultureIgnoreCase))
			{
				Complete();
			}
		});
	}
}
