[AchUnlockOnComplete(typeof(Hero_Nachia))]
public class ACH_THE_TRUE_FOREST_SOVEREIGN : DewAchievementItem
{
	private const int RequiredKillCount = 12;

	[AchPersistentVar]
	private int _currentKillCount;

	public override int GetMaxProgress()
	{
		return 12;
	}

	public override int GetCurrentProgress()
	{
		return _currentKillCount;
	}

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		AchOnKillOrAssist((EventInfoKill k) =>
		{
			if (k.victim is Mon_Forest_BossDemon)
			{
				_currentKillCount++;
				if (_currentKillCount >= 12)
				{
					Complete();
				}
			}
		});
	}
}
