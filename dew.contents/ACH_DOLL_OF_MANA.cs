using UnityEngine;

[AchUnlockOnComplete(typeof(St_R_Deception))]
public class ACH_DOLL_OF_MANA : DewAchievementItem
{
	private const int RequiredAbilityPower = 100;

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		if (!(((object)DewPlayer.local.hero).GetType() != typeof(Hero_Husk)))
		{
			AchCompleteWhen(() => Mathf.RoundToInt(DewPlayer.local.hero.Status.abilityPower) >= 100, 1f);
		}
	}
}
