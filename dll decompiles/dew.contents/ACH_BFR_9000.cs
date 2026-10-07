using UnityEngine;

[AchUnlockOnComplete(typeof(St_D_DoubleTap))]
public class ACH_BFR_9000 : DewAchievementItem
{
	private const int RequiredAttackDamage = 120;

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		if (!(((object)DewPlayer.local.hero).GetType() != typeof(Hero_Lacerta)))
		{
			AchCompleteWhen(() => Mathf.RoundToInt(DewPlayer.local.hero.Status.attackDamage) >= 120, 1f);
		}
	}
}
