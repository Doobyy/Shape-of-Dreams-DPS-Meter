[AchUnlockOnComplete(typeof(St_R_FrozenFists))]
public class ACH_THE_CHILL_WASHES_AWAY_SINS : DewAchievementItem
{
	private const int RequiredKillCount = 120;

	[AchPersistentVar]
	private int _currentActivationCount;

	public override int GetMaxProgress()
	{
		return 120;
	}

	public override int GetCurrentProgress()
	{
		return _currentActivationCount;
	}

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		if (((object)hero).GetType() != typeof(Hero_Cetus))
		{
			return;
		}
		AchOnKillOrAssist((EventInfoKill kill) =>
		{
			if (kill.victim is Monster && NetworkedManagerBase<ZoneManager>.instance.currentZone.name.Contains("SnowMountain"))
			{
				_currentActivationCount++;
				if (120 <= _currentActivationCount)
				{
					Complete();
				}
			}
		});
	}
}
