using System;
using Mirror;
using UnityEngine;

public class Se_Star_Yubar_I_DisintegrateDreamDust : StarEffect
{
	public StarScalingValue convertMultiplier;

	public override Type heroType => typeof(Hero_Yubar);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnActorAdd += new Action<Actor>(OnActorAdd);
		}
	}

	private void OnActorAdd(Actor obj)
	{
		if (obj is Shrine_Disintegration shrine_Disintegration)
		{
			shrine_Disintegration.dreamDustRewardees[victim] = GetValue(convertMultiplier);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)NetworkedManagerBase<ActorManager>.instance != null)
		{
			NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnActorAdd -= new Action<Actor>(OnActorAdd);
		}
	}

	private void MirrorProcessed()
	{
	}
}
