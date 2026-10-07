[AchUnlockOnComplete(typeof(LucidDream_GrievousWounds))]
public class ACH_VIVID_DREAM : DewAchievementItem
{
	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		AchOnGameConcluded((DewGameResult res) =>
		{
			if (res.result == DewGameResult.ResultType.PureWhiteDream && NetworkedManagerBase<GameManager>.instance.difficulty.name == "diffNightmare")
			{
				Complete();
			}
		});
	}
}
