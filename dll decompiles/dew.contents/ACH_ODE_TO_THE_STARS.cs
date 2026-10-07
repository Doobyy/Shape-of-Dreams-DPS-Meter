using UnityEngine;

[AchUnlockOnComplete(typeof(St_D_ConvergencePoint))]
public class ACH_ODE_TO_THE_STARS : DewAchievementItem
{
	private const int RequiredAttackDamage = 55;

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		if (!(((object)DewPlayer.local.hero).GetType() != typeof(Hero_Yubar)))
		{
			AchCompleteWhen(() => Mathf.RoundToInt(DewPlayer.local.hero.Status.attackDamage) >= 55, 1f);
		}
	}
}
