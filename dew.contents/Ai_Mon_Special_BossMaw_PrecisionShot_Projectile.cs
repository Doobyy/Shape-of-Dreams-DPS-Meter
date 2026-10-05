public class Ai_Mon_Special_BossMaw_PrecisionShot_Projectile : StandardProjectile
{
	public ScalingValue damage;

	public Knockback knockback;

	public float stunDuration;

	protected override void OnEntity(EntityHit hit)
	{
		base.OnEntity(hit);
		Damage(damage).SetDirection(info.forward).SetAttr(DamageAttribute.IsCrit).Dispatch(hit.entity);
		knockback.ApplyWithDirection(info.forward, hit.entity);
		CreateBasicEffect(hit.entity, new StunEffect(), stunDuration, "stun_mawprecision");
	}

	private void MirrorProcessed()
	{
	}
}
