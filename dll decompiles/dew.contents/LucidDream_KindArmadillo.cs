using System;
using Mirror;
using UnityEngine;

public class LucidDream_KindArmadillo : LucidDream
{
	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnActorAdd += new Action<Actor>(OnActorAdd);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (bool)(UnityEngine.Object)(object)NetworkedManagerBase<ActorManager>.instance)
		{
			NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnActorAdd -= new Action<Actor>(OnActorAdd);
		}
	}

	private void OnActorAdd(Actor obj)
	{
		if (!(obj is MirageSkinEffect mirageSkinEffect) || obj is Se_MirageSkin_Armor)
		{
			return;
		}
		Entity victim = mirageSkinEffect.victim;
		if (!((UnityEngine.Object)(object)victim == null))
		{
			float shieldAmount = victim.Status.mirageSkinInitAmount;
			mirageSkinEffect.Destroy();
			CreateStatusEffect(victim, new CastInfo(victim), (Se_MirageSkin_Armor e) =>
			{
				e.customAmount = shieldAmount;
			});
		}
	}

	private void MirrorProcessed()
	{
	}
}
