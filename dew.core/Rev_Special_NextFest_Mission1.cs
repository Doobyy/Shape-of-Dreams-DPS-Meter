using System;

public class Rev_Special_NextFest_Mission1 : DewSpecialReverieItem
{
	[AchPersistentVar]
	private int _reachCount;

	public override int grantedStardust => 0;

	public override bool excludeFromPool => true;

	public override string[] grantedItems => new string[3] { "Emote_NextFest_LacertaAngry", "Acc_NextFest_LeafPuppyHat", "Nametag_NextFest_VikingSword" };

	public override Type nextReverie => typeof(Rev_Special_NextFest_Mission2);

	public override int GetCurrentProgress()
	{
		return _reachCount;
	}

	public override int GetMaxProgress()
	{
		return 6;
	}

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		AchOnGameConcluded((DewGameResult r) =>
		{
			if (r.result == DewGameResult.ResultType.PureWhiteDream)
			{
				_reachCount++;
				if (_reachCount >= 6)
				{
					Complete();
				}
			}
		});
	}
}
