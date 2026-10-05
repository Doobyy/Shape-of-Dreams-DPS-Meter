using System;
using UnityEngine;

[AchUnlockOnComplete(typeof(LucidDream_FishScales))]
public class ACH_BARE_SKIN_SUPREMACY : DewAchievementItem
{
	private const int RequiredCount = 2;

	[SaveVar(SaveVarFlags.Default)]
	private bool _didFail;

	[AchPersistentVar]
	private int _currentCount;

	public override int GetMaxProgress()
	{
		return 2;
	}

	public override int GetCurrentProgress()
	{
		return _currentCount;
	}

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		AchOnGameConcluded((DewGameResult res) =>
		{
			if (res.result.IsWin() && !_didFail)
			{
				_currentCount++;
				if (_currentCount >= 2)
				{
					Complete();
				}
			}
		});
		NetworkedManagerBase<ClientEventManager>.instance.OnTakeShield += new Action<EventInfoShield>(OnTakeShield);
	}

	private void OnTakeShield(EventInfoShield obj)
	{
		if (!(obj.finalAmount < 1f) && ((UnityEngine.Object)(object)obj.target == (UnityEngine.Object)(object)hero || obj.statusEffect.IsDescendantOf(hero)))
		{
			_didFail = true;
		}
	}

	public override void OnStopLocalClient()
	{
		base.OnStopLocalClient();
		if ((UnityEngine.Object)(object)NetworkedManagerBase<ClientEventManager>.instance != null)
		{
			NetworkedManagerBase<ClientEventManager>.instance.OnTakeShield -= new Action<EventInfoShield>(OnTakeShield);
		}
	}
}
