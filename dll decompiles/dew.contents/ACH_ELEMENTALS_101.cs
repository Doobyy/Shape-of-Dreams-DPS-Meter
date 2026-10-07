[AchUnlockOnComplete(typeof(Gem_E_Inversion))]
public class ACH_ELEMENTALS_101 : DewAchievementItem
{
	private const int RequiredKills = 70;

	[AchPersistentVar]
	private int _killCount;

	public override int GetMaxProgress()
	{
		return 70;
	}

	public override int GetCurrentProgress()
	{
		return _killCount;
	}

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		AchOnKillOrAssist((EventInfoKill kill) =>
		{
			EntityStatus status = kill.victim.Status;
			if (status.fireStack > 0 && status.lightStack > 0 && status.darkStack > 0 && status.hasCold)
			{
				_killCount++;
				if (_killCount >= 70)
				{
					Complete();
				}
			}
		});
	}
}
