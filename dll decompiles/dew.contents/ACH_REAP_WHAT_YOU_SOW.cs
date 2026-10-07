using System;
using UnityEngine;

[AchUnlockOnComplete(typeof(Gem_E_Overload))]
public class ACH_REAP_WHAT_YOU_SOW : DewAchievementItem
{
	private const int RequiredActivateCount = 12;

	[AchPersistentVar]
	private int _disintegrationCount;

	public override int GetMaxProgress()
	{
		return 12;
	}

	public override int GetCurrentProgress()
	{
		return _disintegrationCount;
	}

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnActorAdd += new Action<Actor>(OnActorAdd);
	}

	public override void OnStopLocalClient()
	{
		base.OnStopLocalClient();
		if (!((UnityEngine.Object)(object)NetworkedManagerBase<ActorManager>.instance == null))
		{
			NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnActorAdd -= new Action<Actor>(OnActorAdd);
		}
	}

	private void OnActorAdd(Actor obj)
	{
		if (obj is Shrine_Disintegration shrine_Disintegration)
		{
			shrine_Disintegration.ClientEvent_OnSuccessfulUse += new Action<Entity>(OnUseShrineDisintegration);
		}
	}

	private void OnUseShrineDisintegration(Entity e)
	{
		if (!e.IsNullInactiveDeadOrKnockedOut() && !((UnityEngine.Object)(object)e.owner != (UnityEngine.Object)(object)DewPlayer.local))
		{
			_disintegrationCount++;
			if (_disintegrationCount >= 12)
			{
				Complete();
			}
		}
	}
}
