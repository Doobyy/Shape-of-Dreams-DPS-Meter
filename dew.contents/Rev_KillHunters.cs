public class Rev_KillHunters : DewReverieItem
{
	[AchPersistentVar]
	private int _killCount;

	public override int grantedStardust => 60;

	public override int GetCurrentProgress()
	{
		return _killCount;
	}

	public override int GetMaxProgress()
	{
		return 40;
	}

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		AchOnKillOrAssist((EventInfoKill k) =>
		{
			if (k.victim is Monster { isHunter: not false })
			{
				_killCount++;
				if (_killCount >= 40)
				{
					Complete();
				}
			}
		});
	}
}
