using Mirror;
using UnityEngine;

public class Ai_L_CoinExplosion_Coin : StandardProjectile
{
	public ScalingValue goldCostRatio;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			info.caster.owner.SpendGold(Mathf.Max(1, DewMath.RandomRoundToInt((float)info.caster.owner.gold * GetValue(goldCostRatio))));
		}
	}

	protected override void OnEntity(EntityHit hit)
	{
		base.OnEntity(hit);
		CreateAbilityInstance<Ai_L_CoinExplosion_Explosion>(position, null, new CastInfo(info.caster));
		Destroy();
	}

	protected override void OnComplete()
	{
		base.OnComplete();
		CreateAbilityInstance<Ai_L_CoinExplosion_Explosion>(position, null, new CastInfo(info.caster));
	}

	private void MirrorProcessed()
	{
	}
}
