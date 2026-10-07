using System;
using UnityEngine;

[AchUnlockOnComplete(typeof(LucidDream_EmbraceMortality))]
public class ACH_EMBRACE_MORTALITY : DewAchievementItem
{
	private const int RequiredCurseCount = 6;

	[SaveVar(SaveVarFlags.Default)]
	private int _curseCount;

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnActorAdd += new Action<Actor>(OnActorAdd);
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
		if (obj is CurseStatusEffect curseStatusEffect && !((UnityEngine.Object)(object)curseStatusEffect.victim != (UnityEngine.Object)(object)hero) && !curseStatusEffect.disableStartNotification)
		{
			_curseCount++;
			if (_curseCount >= 6)
			{
				Complete();
			}
		}
	}
}
