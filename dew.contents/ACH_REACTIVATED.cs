using System;
using UnityEngine;

[AchUnlockOnComplete(typeof(Hero_Husk))]
public class ACH_REACTIVATED : DewAchievementItem
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
		if ((UnityEngine.Object)(object)NetworkedManagerBase<ActorManager>.instance != null)
		{
			NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnActorAdd -= new Action<Actor>(OnActorAdd);
		}
	}

	private void OnActorAdd(Actor obj)
	{
		if (obj is Shrine_BrokenWoodenDoll shrine_BrokenWoodenDoll)
		{
			shrine_BrokenWoodenDoll.ClientEvent_OnDollReactivated += new Action(ClientEventOnDollReactivated);
		}
	}

	private void ClientEventOnDollReactivated()
	{
		Complete();
	}
}
