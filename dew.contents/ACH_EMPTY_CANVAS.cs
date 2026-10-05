[AchUnlockOnComplete(typeof(Gem_L_PureWhite))]
public class ACH_EMPTY_CANVAS : DewAchievementItem
{
	private const int RequiredEnterCount = 2;

	[AchPersistentVar]
	private int _enterCount;

	public override int GetMaxProgress()
	{
		return 2;
	}

	public override int GetCurrentProgress()
	{
		return _enterCount;
	}

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		AchOnGameConcluded((DewGameResult r) =>
		{
			if (r.result == DewGameResult.ResultType.PureWhiteDream)
			{
				_enterCount++;
				if (_enterCount >= 2)
				{
					Complete();
				}
			}
		});
	}
}
