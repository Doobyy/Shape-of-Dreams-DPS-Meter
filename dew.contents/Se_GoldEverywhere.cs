using System;
using Mirror;
using UnityEngine;

public class Se_GoldEverywhere : StatusEffect
{
	public StatBonus statBonus;

	public GameObject fxGoldExplosion;

	public bool hideModelOnDeath;

	public Vector2Int goldDropAmount;

	public float lesserGoldMultiplier;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DoStatBonus(statBonus);
			victim.EntityEvent_OnDeath += new Action<EventInfoKill>(EntityEventOnDeath);
		}
	}

	private void EntityEventOnDeath(EventInfoKill obj)
	{
		FxPlayNewNetworked(fxGoldExplosion, victim);
		if (hideModelOnDeath)
		{
			victim.Visual.DisableRenderers();
		}
		float num = UnityEngine.Random.Range(goldDropAmount.x, goldDropAmount.y + 1);
		if (victim is Monster { type: Monster.MonsterType.Lesser })
		{
			num *= lesserGoldMultiplier;
		}
		NetworkedManagerBase<PickupManager>.instance.DropGold(isKillGold: true, isGivenByOtherPlayer: false, DewMath.RandomRoundToInt(num), victim.position);
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && !((UnityEngine.Object)(object)victim == null))
		{
			victim.EntityEvent_OnDeath -= new Action<EventInfoKill>(EntityEventOnDeath);
		}
	}

	private void MirrorProcessed()
	{
	}
}
