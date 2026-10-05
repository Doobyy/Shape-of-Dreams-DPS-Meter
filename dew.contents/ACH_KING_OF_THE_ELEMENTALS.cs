using System;

[AchUnlockOnComplete(typeof(St_Q_Fleche))]
public class ACH_KING_OF_THE_ELEMENTALS : DewAchievementItem
{
	private const int KilledElementals = 75;

	[AchPersistentVar]
	private int _kills;

	public override int GetCurrentProgress()
	{
		return _kills;
	}

	public override int GetMaxProgress()
	{
		return 75;
	}

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		if (((object)DewPlayer.local.hero).GetType() != typeof(Hero_Mist))
		{
			return;
		}
		AchOnKillOrAssist((EventInfoKill k) =>
		{
			if (k.victim is Monster monster && (((object)monster).GetType().Name.Contains("Elemental", StringComparison.InvariantCultureIgnoreCase) || ((object)monster).GetType().Name.Contains("Treant", StringComparison.InvariantCultureIgnoreCase)))
			{
				_kills++;
				if (_kills >= 75)
				{
					Complete();
				}
			}
		});
	}
}
