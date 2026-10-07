using System;
using UnityEngine;

[AchUnlockOnComplete(typeof(Hero_Cetus))]
public class ACH_LEVIATHAN_ON_LAND : DewAchievementItem
{
	private const int RequiredActivationCount = 8;

	[AchPersistentVar]
	private int _currentActivationCount;

	public override int GetMaxProgress()
	{
		return 8;
	}

	public override int GetCurrentProgress()
	{
		return _currentActivationCount;
	}

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
		if (!(obj is Shrine_PileOfSnow shrine_PileOfSnow))
		{
			return;
		}
		shrine_PileOfSnow.ClientEvent_OnSuccessfulUse += (Action<Entity>)((Entity _) =>
		{
			_currentActivationCount++;
			if (8 <= _currentActivationCount)
			{
				Complete();
			}
		});
	}
}
