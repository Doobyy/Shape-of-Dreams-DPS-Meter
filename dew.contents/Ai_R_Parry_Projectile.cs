using UnityEngine;

public class Ai_R_Parry_Projectile : StandardProjectile
{
	public ScalingValue damage;

	public bool alwaysKnockback;

	public Knockback knockback;

	public float procCoefficient = 1f;

	public float stunDuration;

	public bool doAttackEffect;

	public override bool reuseInRoom => true;

	protected override void OnEntity(EntityHit hit)
	{
		base.OnEntity(hit);
		Damage(damage, procCoefficient).SetDirection(rotation).DoAttackEffect(AttackEffectType.Others, doAttackEffect ? 1f : 0f).Dispatch(hit.entity);
		if (hit.entity.Control.isDashing || alwaysKnockback)
		{
			knockback.ApplyWithDirection(((Component)(object)info.caster).transform.forward, hit.entity);
		}
		if (stunDuration > 0f)
		{
			CreateBasicEffect(hit.entity, new StunEffect(), stunDuration, "parry_stun");
		}
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
