public class Rev_EnterPureWhiteDream : DewReverieItem
{
	public override int grantedStardust => 120;

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		AchOnGameConcluded((DewGameResult r) =>
		{
			if (r.result == DewGameResult.ResultType.PureWhiteDream)
			{
				Complete();
			}
		});
	}
}
