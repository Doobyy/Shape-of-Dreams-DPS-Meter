using System;
using UnityEngine;

[AchUnlockOnComplete(typeof(St_L_MentalCorruption))]
public class ACH_BLACKCAT_DEJAVU : DewAchievementItem
{
	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnActorAdd += new Action<Actor>(OnActorAdd);
		foreach (Actor allActor in NetworkedManagerBase<ActorManager>.instance.allActors)
		{
			OnActorAdd(allActor);
		}
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
		if (obj is Shrine_LoopCat shrine_LoopCat)
		{
			shrine_LoopCat.ClientEvent_OnSuccessfulUse += new Action<Entity>(OnUseShrineLoopCat);
		}
	}

	private void OnUseShrineLoopCat(Entity e)
	{
		Complete();
	}
}
