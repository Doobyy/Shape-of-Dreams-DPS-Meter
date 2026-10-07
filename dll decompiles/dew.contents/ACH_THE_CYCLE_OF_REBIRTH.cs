using System;
using UnityEngine;

[AchUnlockOnComplete(typeof(St_D_CircleOfLife))]
public class ACH_THE_CYCLE_OF_REBIRTH : DewAchievementItem
{
	private bool _didFail;

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		if (!(hero is Hero_Nachia))
		{
			return;
		}
		AchOnTakeDamage((EventInfoDamage dmg) =>
		{
			if (dmg.actor is Mon_Ink_BossDarkMoon || dmg.actor.IsDescendantOf<Mon_Ink_BossDarkMoon>() || dmg.actor is Mon_Ink_BossWhiteNight || dmg.actor.IsDescendantOf<Mon_Ink_BossWhiteNight>())
			{
				_didFail = true;
			}
		});
		NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded += new Action<EventInfoLoadRoom>(ClientEventOnRoomLoaded);
		AchOnKillOrAssist((EventInfoKill k) =>
		{
			if (!_didFail)
			{
				Entity victim = k.victim;
				if (victim is Mon_Ink_BossDarkMoon || victim is Mon_Ink_BossWhiteNight)
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
		_didFail = false;
	}
}
