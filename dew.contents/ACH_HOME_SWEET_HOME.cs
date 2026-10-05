using System;

[AchUnlockOnComplete(typeof(St_Q_DeathMark))]
public class ACH_HOME_SWEET_HOME : DewAchievementItem
{
	private const float HealthThreshold = 0.5f;

	[SaveVar(SaveVarFlags.Default)]
	private bool _didFail;

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		if (((object)hero).GetType() != typeof(Hero_Husk))
		{
			return;
		}
		AchSetInterval(() =>
		{
			if (!_didFail && !NetworkedManagerBase<ZoneManager>.instance.isInAnyTransition && hero.normalizedHealth < 0.5f)
			{
				_didFail = true;
			}
		}, 1f);
		NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded += new Action<EventInfoLoadRoom>(ClientEventOnRoomLoaded);
	}

	public override void OnStopLocalClient()
	{
		base.OnStopLocalClient();
		NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded -= new Action<EventInfoLoadRoom>(ClientEventOnRoomLoaded);
	}

	private void ClientEventOnRoomLoaded(EventInfoLoadRoom _)
	{
		if (_didFail)
		{
			return;
		}
		GameManager.CallOnReady(() =>
		{
			if (NetworkedManagerBase<ZoneManager>.instance.currentZone.name.Contains("Ink", StringComparison.InvariantCultureIgnoreCase))
			{
				Complete();
			}
		});
	}
}
