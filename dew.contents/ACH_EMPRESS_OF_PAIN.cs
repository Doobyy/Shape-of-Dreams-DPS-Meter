using System;
using UnityEngine;

[AchUnlockOnComplete(typeof(LucidDream_MarshOfDestiny))]
public class ACH_EMPRESS_OF_PAIN : DewAchievementItem
{
	private const int RequiredCount = 3;

	[AchPersistentVar]
	private int _totalCount;

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
		if (obj is CurseStatusEffect curseStatusEffect && !((UnityEngine.Object)(object)curseStatusEffect.victim != (UnityEngine.Object)(object)hero) && !curseStatusEffect.disableStartNotification && NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex >= 4)
		{
			_totalCount++;
			if (_totalCount >= 3)
			{
				Complete();
			}
		}
	}
}
