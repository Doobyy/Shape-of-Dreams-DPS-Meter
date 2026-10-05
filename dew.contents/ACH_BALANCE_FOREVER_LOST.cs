using System;
using UnityEngine;

[AchUnlockOnComplete(typeof(St_E_SliceThroat))]
public class ACH_BALANCE_FOREVER_LOST : DewAchievementItem
{
	private int _killCount;

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		if ((UnityEngine.Object)(object)NetworkedManagerBase<ZoneManager>.instance != null)
		{
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded += new Action<EventInfoLoadRoom>(ClientEventOnRoomLoaded);
		}
		AchOnKillOrAssist((EventInfoKill k) =>
		{
			Entity victim = k.victim;
			if (victim is Mon_Ink_BossWhiteNight || victim is Mon_Ink_BossDarkMoon)
			{
				_killCount++;
				if (_killCount >= 2)
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

	private void ClientEventOnRoomLoaded(EventInfoLoadRoom obj)
	{
		_killCount = 0;
	}
}
