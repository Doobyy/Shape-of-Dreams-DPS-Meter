using UnityEngine;

[AchUnlockOnComplete(typeof(LucidDream_BonVoyage))]
public class ACH_HUNTERS_WHAT_HUNTERS : DewAchievementItem
{
	private const int RequiredEnterCount = 3;

	[AchPersistentVar]
	private int _enterPureWhiteDreamCount;

	[SaveVar(SaveVarFlags.Default)]
	private bool _didFail;

	public override int GetMaxProgress()
	{
		return 3;
	}

	public override int GetCurrentProgress()
	{
		return _enterPureWhiteDreamCount;
	}

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		AchOnKillOrAssist((EventInfoKill k) =>
		{
			if ((Object)(object)k.victim != null && k.victim is Monster { isHunter: not false })
			{
				_didFail = true;
			}
		});
		AchOnGameConcluded((DewGameResult res) =>
		{
			if (res.result.IsWin() && !_didFail)
			{
				_enterPureWhiteDreamCount++;
				if (_enterPureWhiteDreamCount >= 3)
				{
					Complete();
				}
			}
		});
	}
}
