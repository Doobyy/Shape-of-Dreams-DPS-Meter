[AchUnlockOnComplete(typeof(Gem_E_Obsidian))]
public class ACH_MINE_AND_CRAFT : DewAchievementItem
{
	private const int RequiredKills = 3;

	[AchPersistentVar]
	private int _currentKills;

	public override int GetMaxProgress()
	{
		return 3;
	}

	public override int GetCurrentProgress()
	{
		return _currentKills;
	}

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		AchOnKillOrAssist((EventInfoKill kill) =>
		{
			if (kill.victim is PropEnt_Stone_PitchBlackOre)
			{
				_currentKills++;
				if (_currentKills >= 3)
				{
					Complete();
				}
			}
		});
	}
}
