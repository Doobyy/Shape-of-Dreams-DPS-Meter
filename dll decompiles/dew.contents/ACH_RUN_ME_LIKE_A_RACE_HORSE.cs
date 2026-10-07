[AchUnlockOnComplete(typeof(St_R_FlamingWhip))]
public class ACH_RUN_ME_LIKE_A_RACE_HORSE : DewAchievementItem
{
	private const int RequiredFireStack = 30;

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		AchOnKillOrAssist((EventInfoKill k) =>
		{
			if (DewPlayer.local.hero.CheckEnemyOrNeutral(k.victim) && k.victim.Status.fireStack >= 30)
			{
				Complete();
			}
		});
	}
}
