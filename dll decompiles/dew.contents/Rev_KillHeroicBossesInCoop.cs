public class Rev_KillHeroicBossesInCoop : DewReverieItem
{
	[AchPersistentVar]
	private int _killCount;

	public override int grantedStardust => 75;

	public override int GetCurrentProgress()
	{
		return _killCount;
	}

	public override int GetMaxProgress()
	{
		return 4;
	}

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		if (DewPlayer.gamePlayers.Count <= 1)
		{
			return;
		}
		AchOnKillOrAssist((EventInfoKill k) =>
		{
			if (k.victim is BossMonster)
			{
				_killCount++;
				if (_killCount >= 4)
				{
					Complete();
				}
			}
		});
	}
}
