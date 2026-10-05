[AchUnlockOnComplete(typeof(Gem_R_Panic))]
public class ACH_EVERYONE_LETS_NOT_PANIC : DewAchievementItem
{
	private const int RequiredBossKills = 8;

	private const float HealthThreshold = 0.25f;

	[AchPersistentVar]
	private int _bossKills;

	public override int GetMaxProgress()
	{
		return 8;
	}

	public override int GetCurrentProgress()
	{
		return _bossKills;
	}

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		AchOnKillOrAssist((EventInfoKill kill) =>
		{
			if (kill.victim is BossMonster && !(DewPlayer.local.hero.normalizedHealth > 0.25f) && !DewPlayer.local.hero.IsNullInactiveDeadOrKnockedOut())
			{
				_bossKills++;
				if (_bossKills >= 8)
				{
					Complete();
				}
			}
		});
	}
}
