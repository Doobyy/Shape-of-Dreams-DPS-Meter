using System;
using UnityEngine;

[AchUnlockOnComplete(typeof(LucidDream_SparklingDreamFlask))]
public class ACH_NO_NEED_FOR_YOUR_HELP : DewAchievementItem
{
	private const int RequiredEnterCount = 2;

	[SaveVar(SaveVarFlags.Default)]
	private bool _didFail;

	[AchPersistentVar]
	private int _enterCount;

	public override int GetCurrentProgress()
	{
		return _enterCount;
	}

	public override int GetMaxProgress()
	{
		return 2;
	}

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnActorAdd += new Action<Actor>(OnActorAdd);
		AchOnGameConcluded((DewGameResult res) =>
		{
			if (!_didFail && res.result.IsWin())
			{
				_enterCount++;
				if (_enterCount >= 2)
				{
					Complete();
				}
			}
		});
	}

	public override void OnStopLocalClient()
	{
		base.OnStopLocalClient();
		if ((UnityEngine.Object)(object)NetworkedManagerBase<ActorManager>.instance != null)
		{
			NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnActorAdd -= new Action<Actor>(OnActorAdd);
		}
	}

	private void OnActorAdd(Actor obj)
	{
		if (obj is Shrine_Guidance shrine_Guidance)
		{
			shrine_Guidance.ClientEvent_OnSuccessfulUse += (Action<Entity>)((Entity _) =>
			{
				_didFail = true;
			});
		}
	}
}
