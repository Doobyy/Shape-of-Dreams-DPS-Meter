[AchUnlockOnComplete(typeof(LucidDream_BlandStarSoup))]
public class ACH_NOT_EVEN_A_SCRATCH : DewAchievementItem
{
	private const int RequiredFireStack = 10;

	private const float CheckInterval = 0.5f;

	private const float RequiredTime = 30f;

	private const int RequiredElementalCount = 3;

	[AchPersistentVar]
	private float _accumulatedTime;

	public override int GetMaxProgress()
	{
		return 30;
	}

	public override int GetCurrentProgress()
	{
		return (int)_accumulatedTime;
	}

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		AchSetInterval(() =>
		{
			EntityStatus status = DewPlayer.local.hero.Status;
			if (status.fireStack >= 10 || HasThreeOrMoreElementals(status))
			{
				_accumulatedTime += 0.5f;
				if (_accumulatedTime >= 30f)
				{
					Complete();
				}
			}
		}, 0.5f);
		static bool HasThreeOrMoreElementals(EntityStatus s)
		{
			return (s.hasCold ? 1 : 0) + ((s.fireStack > 0) ? 1 : 0) + ((s.lightStack > 0) ? 1 : 0) + ((s.darkStack > 0) ? 1 : 0) >= 3;
		}
	}
}
