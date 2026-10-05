[AchUnlockOnComplete(typeof(St_D_ScarOfTheWind))]
public class ACH_I_AM_SHELL : DewAchievementItem
{
	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		if (((object)DewPlayer.local.hero).GetType() != typeof(Hero_Husk))
		{
			return;
		}
		AchOnGameConcluded((DewGameResult res) =>
		{
			if (res.result == DewGameResult.ResultType.PureWhiteDream)
			{
				Complete();
			}
		});
	}
}
