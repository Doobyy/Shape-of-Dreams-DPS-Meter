[AchUnlockOnComplete(typeof(St_Q_MoonlightPact))]
public class ACH_FULL_MOON : DewAchievementItem
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
		if (!(hero is Hero_Nachia))
		{
			return;
		}
		AchOnGameConcluded((DewGameResult r) =>
		{
			if (r.result.IsWin())
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
