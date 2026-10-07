using System;
using UnityEngine;

[AchUnlockOnComplete(typeof(LucidDream_WILD))]
public class ACH_FEELS_LIKE_HOME : DewAchievementItem
{
	private const int RequiredVisitCount = 12;

	[SaveVar(SaveVarFlags.Default)]
	private int _visitCount;

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded += new Action<EventInfoLoadRoom>(ClientEventOnRoomLoaded);
	}

	private void ClientEventOnRoomLoaded(EventInfoLoadRoom obj)
	{
		if (!obj.isTraveling)
		{
			return;
		}
		NetworkedManagerBase<ZoneManager>.instance.CallOnReadyAfterTransition(() =>
		{
			if (NetworkedManagerBase<ZoneManager>.instance.currentNode.HasModifier<RoomMod_Hunted>() || NetworkedManagerBase<ZoneManager>.instance.currentNode.HasModifier<RoomMod_Ambush>())
			{
				_visitCount++;
				if (_visitCount >= 12)
				{
					Complete();
				}
			}
		});
	}

	public override void OnStopLocalClient()
	{
		base.OnStopLocalClient();
		if ((UnityEngine.Object)(object)NetworkedManagerBase<ZoneManager>.instance != null)
		{
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded -= new Action<EventInfoLoadRoom>(ClientEventOnRoomLoaded);
		}
	}
}
