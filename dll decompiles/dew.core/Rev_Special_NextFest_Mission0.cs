using System;

public class Rev_Special_NextFest_Mission0 : DewSpecialReverieItem
{
	[AchPersistentVar]
	private int _killCount;

	public override int grantedStardust => 0;

	public override bool excludeFromPool => true;

	public override string[] grantedItems => new string[3] { "Emote_NextFest_TheGreatDuelist", "Acc_NextFest_LSDLogo", "Nametag_NextFest_SidePuppy" };

	public override Type nextReverie => typeof(Rev_Special_NextFest_Mission1);

	public override int GetCurrentProgress()
	{
		return _killCount;
	}

	public override int GetMaxProgress()
	{
		return 8;
	}

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		AchOnKillOrAssist((EventInfoKill k) =>
		{
			if (k.victim is BossMonster)
			{
				_killCount++;
				if (_killCount >= 8)
				{
					Complete();
				}
			}
		});
	}
}
