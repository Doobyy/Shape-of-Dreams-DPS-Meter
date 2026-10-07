public class Rev_KillHeroicBosses : DewReverieItem
{
	[AchPersistentVar]
	private int _killCount;

	public override int grantedStardust => 55;

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
