using UnityEngine;

public class Ai_Mon_Despair_ParalyticFly_Paralyze : StandardProjectile
{
	public ScalingValue endDamage;

	public float stunDuration;

	public Knockback Knockback;

	public override bool reuseInRoom => true;

	protected override void OnEntity(EntityHit hit)
	{
		base.OnEntity(hit);
		Vector3 normalized = (hit.entity.position - info.caster.position).normalized;
		CreateDamage(DamageData.SourceType.Default, endDamage).SetDirection(normalized).SetOriginPosition(info.caster.position).Dispatch(hit.entity);
		CreateBasicEffect(hit.entity, new StunEffect(), stunDuration, "paralyticfly_stun");
		CreateStatusEffect<Se_Mon_Despair_ParalyticFly_Paralyze>(hit.entity);
		Knockback.ApplyWithDirection(normalized, hit.entity);
	}

	private void MirrorProcessed()
	{
	}
}
