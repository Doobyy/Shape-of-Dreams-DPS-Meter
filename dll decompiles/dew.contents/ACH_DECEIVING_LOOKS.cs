[AchUnlockOnComplete(typeof(St_D_BeautifulThreat))]
public class ACH_DECEIVING_LOOKS : DewAchievementItem
{
	private const int RequiredBossKills = 10;

	[AchPersistentVar]
	private int _bossKills;

	public override int GetMaxProgress()
	{
		return 10;
	}

	public override int GetCurrentProgress()
	{
		return _bossKills;
	}

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		if (((object)DewPlayer.local.hero).GetType() != typeof(Hero_Aurena))
		{
			return;
		}
		AchOnKillOrAssist((EventInfoKill kill) =>
		{
			if (kill.victim is Monster { type: Monster.MonsterType.Boss })
			{
				_bossKills++;
				if (_bossKills >= 10)
				{
					Complete();
				}
			}
		});
	}
}
