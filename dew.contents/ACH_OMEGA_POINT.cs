using System;
using UnityEngine;

[AchUnlockOnComplete(typeof(Gem_E_Omega))]
public class ACH_OMEGA_POINT : DewAchievementItem
{
	[SaveVar(SaveVarFlags.Default)]
	private Type _skillType;

	[SaveVar(SaveVarFlags.Default)]
	private bool _hasFailed;

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		if ((UnityEngine.Object)(object)DewPlayer.local.hero == null)
		{
			return;
		}
		DewPlayer.local.hero.ClientHeroEvent_OnSkillUse += new Action<EventInfoSkillUse>(OnSkillUse);
		AchOnGameConcluded((DewGameResult r) =>
		{
			if (r.result.IsWin() && !_hasFailed)
			{
				Complete();
			}
		});
	}

	private void OnSkillUse(EventInfoSkillUse info)
	{
		if (!_hasFailed && !((UnityEngine.Object)(object)info.skill == null) && info.skill.skillType != HeroSkillLocation.Identity && info.skill.skillType != HeroSkillLocation.Movement && !(_skillType == ((object)info.skill).GetType()))
		{
			if (_skillType == null)
			{
				_skillType = ((object)info.skill).GetType();
			}
			else
			{
				_hasFailed = true;
			}
		}
	}

	public override void OnStopLocalClient()
	{
		base.OnStopLocalClient();
		if (!((UnityEngine.Object)(object)DewPlayer.local == null) && !((UnityEngine.Object)(object)DewPlayer.local.hero == null))
		{
			DewPlayer.local.hero.ClientHeroEvent_OnSkillUse -= new Action<EventInfoSkillUse>(OnSkillUse);
		}
	}
}
