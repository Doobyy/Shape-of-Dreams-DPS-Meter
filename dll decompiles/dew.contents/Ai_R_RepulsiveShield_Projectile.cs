using UnityEngine;

public class Ai_R_RepulsiveShield_Projectile : StandardProjectile
{
	public ScalingValue damage;

	public Knockback knockback;

	public float procCoefficient = 0.5f;

	public override bool reuseInRoom => true;

	protected override void OnEntity(EntityHit hit)
	{
		base.OnEntity(hit);
		Damage(damage, procCoefficient).SetDirection(rotation).Dispatch(hit.entity);
		if (hit.entity.Control.isDashing)
		{
			knockback.ApplyWithDirection(((Component)(object)info.caster).transform.forward, hit.entity);
		}
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
