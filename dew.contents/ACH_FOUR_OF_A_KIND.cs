using System;
using UnityEngine;

[AchUnlockOnComplete(typeof(St_L_CoinExplosion))]
public class ACH_FOUR_OF_A_KIND : DewAchievementItem
{
	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		AchOnGameConcluded((DewGameResult res) =>
		{
			if (res.result.IsWin() && !((UnityEngine.Object)(object)hero.Skill.Q == null) && !((UnityEngine.Object)(object)hero.Skill.W == null) && !((UnityEngine.Object)(object)hero.Skill.E == null) && !((UnityEngine.Object)(object)hero.Skill.R == null))
			{
				Type type = ((object)hero.Skill.Q).GetType();
				if (!(((object)hero.Skill.W).GetType() != type) && !(((object)hero.Skill.E).GetType() != type) && !(((object)hero.Skill.R).GetType() != type))
				{
					Complete();
				}
			}
		});
	}
}
