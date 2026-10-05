using System.Collections;
using Mirror;
using UnityEngine;

public class Ai_Mon_LavaLand_FireElemental_Explosion : InstantDamageInstance
{
	public float slowDuration = 1.5f;

	public float slowStrength;

	public float knockupStrength = 1.5f;

	public override bool reuseInRoom => true;

	protected override void OnHit(Entity entity)
	{
		base.OnHit(entity);
		CreateBasicEffect(entity, new SlowEffect
		{
			strength = slowStrength
		}, slowDuration);
		if (!entity.Status.hasUnstoppable)
		{
			entity.Visual.KnockUp(knockupStrength, isFriendly: false);
		}
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		((Component)(object)this).transform.position = info.point;
	}

	protected override IEnumerator OnCreateSequenced()
	{
		yield return base.OnCreateSequenced();
		if (((NetworkBehaviour)this).isServer)
		{
			Vector3 positionOnGround = Dew.GetPositionOnGround(((Component)(object)this).transform.position);
			CreateAbilityInstance<Ai_Mon_LavaLand_FireElemental_ExplosionSub>(positionOnGround, null, new CastInfo(info.caster));
			Destroy();
		}
	}

	private void MirrorProcessed()
	{
	}
}
