[AchUnlockOnComplete(typeof(St_D_ChargedAnguillian))]
public class ACH_FROM_ABYSS_TO_SUMMIT : DewAchievementItem
{
	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		if (!(hero is Hero_Cetus))
		{
			return;
		}
		AchOnGameConcluded((DewGameResult r) =>
		{
			if (r.result == DewGameResult.ResultType.PureWhiteDream)
			{
				Complete();
			}
		});
	}
}
