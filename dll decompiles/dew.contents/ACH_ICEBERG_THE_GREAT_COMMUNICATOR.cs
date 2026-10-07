using UnityEngine;

[AchUnlockOnComplete(typeof(St_Q_BigBorealChunk))]
public class ACH_ICEBERG_THE_GREAT_COMMUNICATOR : DewAchievementItem
{
	private const int RequiredAbilityPower = 200;

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		if (!(((object)hero).GetType() != typeof(Hero_Cetus)))
		{
			AchCompleteWhen(() => Mathf.RoundToInt(hero.Status.abilityPower) >= 200, 1f);
		}
	}
}
