using System;
using Mirror;
using UnityEngine;

public class Se_Star_I_MawDiscountAndProtection : StarEffect
{
	public StarScalingValue discountRatio;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnActorAdd += new Action<Actor>(OnAdd);
		foreach (Actor allActor in NetworkedManagerBase<ActorManager>.instance.allActors)
		{
			OnAdd(allActor);
		}
	}

	private void OnAdd(Actor obj)
	{
		if (!(obj is Shrine_MawOfDoom { destroyCustomActionsByPlayer: var destroyCustomActionsByPlayer } shrine_MawOfDoom))
		{
			return;
		}
		destroyCustomActionsByPlayer[player.guid] = (Actor target) =>
		{
			if (target is SkillTrigger skillTrigger)
			{
				skillTrigger.level = 1;
			}
			if (target is Gem gem)
			{
				gem.quality = 10;
			}
		};
		shrine_MawOfDoom.priceMultipliersByPlayer[player.guid] = 1f - GetValue(discountRatio);
	}

	private void OnCleanup(Actor obj)
	{
		if (obj is Shrine_MawOfDoom shrine_MawOfDoom && !((UnityEngine.Object)(object)player == null))
		{
			shrine_MawOfDoom.destroyCustomActionsByPlayer.Remove(player.guid);
			shrine_MawOfDoom.priceMultipliersByPlayer.Remove(player.guid);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (!((NetworkBehaviour)this).isServer || !((UnityEngine.Object)(object)NetworkedManagerBase<ActorManager>.instance != null))
		{
			return;
		}
		NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnActorAdd -= new Action<Actor>(OnAdd);
		foreach (Actor allActor in NetworkedManagerBase<ActorManager>.instance.allActors)
		{
			OnCleanup(allActor);
		}
	}

	private void MirrorProcessed()
	{
	}
}
