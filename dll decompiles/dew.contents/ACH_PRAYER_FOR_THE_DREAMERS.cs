using System;
using UnityEngine;

[AchUnlockOnComplete(typeof(LucidDream_FalseLifeline))]
public class ACH_PRAYER_FOR_THE_DREAMERS : DewAchievementItem
{
	private const int RequiredCount = 12;

	private const float HealthThreshold = 0.4f;

	[SaveVar(SaveVarFlags.Default)]
	private int _currentCount;

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

	private void ClientEventOnRoomLoaded(EventInfoLoadRoom obj)
	{
		if (obj.isTraveling && !SingletonDewNetworkBehaviour<Room>.instance.isRevisit && !(hero.normalizedHealth > 0.4f))
		{
			_currentCount++;
			if (_currentCount >= 12)
			{
				Complete();
			}
		}
	}
}
