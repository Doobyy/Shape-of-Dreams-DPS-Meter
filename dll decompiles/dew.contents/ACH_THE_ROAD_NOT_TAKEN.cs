[AchUnlockOnComplete(typeof(Gem_L_Liberty))]
public class ACH_THE_ROAD_NOT_TAKEN : DewAchievementItem
{
	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		AchOnGameConcluded((DewGameResult res) =>
		{
			if (res.result == DewGameResult.ResultType.StarlessPath)
			{
				Complete();
			}
		});
	}
}
