using System;
using System.Collections.Generic;
using UnityEngine;

[AchUnlockOnComplete(typeof(St_Q_Reduction))]
public class ACH_IMMORTALITY_ACHIEVED : DewAchievementItem
{
	private const float CountTimeframe = 5f;

	private const float RequiredCastCount = 9f;

	private readonly List<float> _castTimings = new List<float>();

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		if (((object)DewPlayer.local.hero).GetType() != typeof(Hero_Aurena))
		{
			return;
		}
		DewPlayer.local.hero.ClientHeroEvent_OnSkillUse += new Action<EventInfoSkillUse>(ClientHeroEventOnSkillUse);
		AchSetInterval(() =>
		{
			while (_castTimings.Count > 0 && Time.time - _castTimings[0] > 5f)
			{
				_castTimings.RemoveAt(0);
			}
			if ((float)_castTimings.Count >= 9f)
			{
				_castTimings.Clear();
				Complete();
			}
		}, 1f);
	}

	private void ClientHeroEventOnSkillUse(EventInfoSkillUse obj)
	{
		if (obj.skill is St_Q_GoldenBurst)
		{
			_castTimings.Add(Time.time);
		}
	}

	public override void OnStopLocalClient()
	{
		base.OnStopLocalClient();
		if ((UnityEngine.Object)(object)DewPlayer.local != null && (UnityEngine.Object)(object)DewPlayer.local.hero != null)
		{
			DewPlayer.local.hero.ClientHeroEvent_OnSkillUse -= new Action<EventInfoSkillUse>(ClientHeroEventOnSkillUse);
		}
	}
}
