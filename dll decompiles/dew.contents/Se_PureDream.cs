using System;
using Mirror;
using UnityEngine;

public class Se_PureDream : StatusEffect
{
	public StatBonus statBonus;

	public GameObject fxdreamExplosion;

	public bool hideModelOnDeath;

	public Vector2Int dreamDustDropAmount;

	public float lesserDreamDustMultiplier;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (victim.Visual.model.fxDeath != null)
		{
			victim.Visual.model.fxDeath = null;
		}
		if (((NetworkBehaviour)this).isServer)
		{
			DoStatBonus(statBonus);
			victim.EntityEvent_OnDeath += new Action<EventInfoKill>(EntityEventOnDeath);
		}
	}

	private void EntityEventOnDeath(EventInfoKill obj)
	{
		FxPlayNewNetworked(fxdreamExplosion, victim);
		if (hideModelOnDeath)
		{
			victim.Visual.DisableRenderers();
		}
		float num = UnityEngine.Random.Range(dreamDustDropAmount.x, dreamDustDropAmount.y + 1);
		if (victim is Monster { type: Monster.MonsterType.Lesser })
		{
			num *= lesserDreamDustMultiplier;
		}
		NetworkedManagerBase<PickupManager>.instance.DropDreamDust(isGivenByOtherPlayer: false, DewMath.RandomRoundToInt(num), victim.position);
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
