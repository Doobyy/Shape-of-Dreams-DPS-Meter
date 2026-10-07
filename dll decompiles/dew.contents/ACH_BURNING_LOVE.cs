using System;
using UnityEngine;

[AchUnlockOnComplete(typeof(St_L_PyranasFireball))]
public class ACH_BURNING_LOVE : DewAchievementItem
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
		if (obj is Shrine_PyranasLove shrine_PyranasLove)
		{
			shrine_PyranasLove.ClientEvent_OnSuccessfulUse += new Action<Entity>(OnUseShrine);
		}
	}

	private void OnUseShrine(Entity e)
	{
		Complete();
	}
}
