using System;
using UnityEngine;

[AchUnlockOnComplete(typeof(St_R_ChainReaction))]
public class ACH_OVERFLOWING_HEALS : DewAchievementItem
{
	private const float StartHealthThreshold = 0.4f;

	private const float RequiredHealRatio = 0.6f;

	private float _lastLowHealthTime;

	private float _achCompleteHealthCheckThresholdTime;

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		if (((object)DewPlayer.local.hero).GetType() != typeof(Hero_Aurena))
		{
			return;
		}
		AchSetInterval(() =>
		{
			if (hero.normalizedHealth < 0.4f)
			{
				_lastLowHealthTime = Time.time;
			}
			if (Time.time < _achCompleteHealthCheckThresholdTime && hero.normalizedHealth >= 0.999f)
			{
				Complete();
			}
		}, 0.1f);
		NetworkedManagerBase<ClientEventManager>.instance.OnTakeHeal += new Action<EventInfoHeal>(OnTakeHeal);
	}

	public override void OnStopLocalClient()
	{
		base.OnStopLocalClient();
		if ((UnityEngine.Object)(object)NetworkedManagerBase<ClientEventManager>.instance != null)
		{
			NetworkedManagerBase<ClientEventManager>.instance.OnTakeHeal -= new Action<EventInfoHeal>(OnTakeHeal);
		}
	}

	private void OnTakeHeal(EventInfoHeal obj)
	{
		if (!(Time.time - _lastLowHealthTime > 1f) && !((UnityEngine.Object)(object)obj.actor.FindFirstOfType<Se_R_DangerousTheory>() == null))
		{
			_achCompleteHealthCheckThresholdTime = Time.time + 0.5f;
		}
	}
}
