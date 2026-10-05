using System;
using UnityEngine;

[AchUnlockOnComplete(typeof(Gem_R_Adventure))]
public class ACH_NOVICE_TREASURE_HUNTER : DewAchievementItem
{
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
		if (obj is Shrine_HiddenStash shrine_HiddenStash)
		{
			shrine_HiddenStash.ClientEvent_OnSuccessfulUse += new Action<Entity>(OnUseShrineLoopCat);
		}
	}

	private void OnUseShrineLoopCat(Entity e)
	{
		Complete();
	}
}
