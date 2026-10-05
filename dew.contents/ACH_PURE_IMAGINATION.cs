using System;
using UnityEngine;

[AchUnlockOnComplete(typeof(Hero_Yubar))]
public class ACH_PURE_IMAGINATION : DewAchievementItem
{
	private const int RequiredGainAmount = 450;

	[AchPersistentVar]
	private int _gainedDreamDust;

	public override int GetMaxProgress()
	{
		return 450;
	}

	public override int GetCurrentProgress()
	{
		return _gainedDreamDust;
	}

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		DewPlayer.local.ClientEvent_OnEarnDreamDust += new Action<int>(ClientEventOnEarnDreamDust);
	}

	public override void OnStopLocalClient()
	{
		base.OnStopLocalClient();
		if ((UnityEngine.Object)(object)DewPlayer.local != null)
		{
			DewPlayer.local.ClientEvent_OnEarnDreamDust -= new Action<int>(ClientEventOnEarnDreamDust);
		}
	}

	private void ClientEventOnEarnDreamDust(int obj)
	{
		_gainedDreamDust += obj;
		if (_gainedDreamDust >= 450)
		{
			Complete();
		}
	}
}
