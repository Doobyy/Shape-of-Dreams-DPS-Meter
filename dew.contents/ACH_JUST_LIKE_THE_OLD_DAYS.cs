[AchUnlockOnComplete(typeof(St_Q_IncendiaryRounds))]
public class ACH_JUST_LIKE_THE_OLD_DAYS : DewAchievementItem
{
	private const int RequiredBossKills = 5;

	[AchPersistentVar]
	private int _bossKills;

	public override int GetMaxProgress()
	{
		return 5;
	}

	public override int GetCurrentProgress()
	{
		return _bossKills;
	}

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		if (((object)DewPlayer.local.hero).GetType() != typeof(Hero_Lacerta))
		{
			return;
		}
		AchOnKillOrAssist((EventInfoKill kill) =>
		{
			if (kill.victim is Monster { type: Monster.MonsterType.Boss })
			{
				_bossKills++;
				if (_bossKills >= 5)
				{
					Complete();
				}
			}
		});
	}
}
