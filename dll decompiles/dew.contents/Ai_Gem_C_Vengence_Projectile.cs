public class Ai_Gem_C_Vengence_Projectile : StandardProjectile
{
	public ScalingValue damage;

	public float procCoefficient;

	public override bool reuseInRoom => true;

	protected override void OnEntity(EntityHit hit)
	{
		base.OnEntity(hit);
		Damage(damage, procCoefficient).SetOriginPosition(info.caster.position).Dispatch(hit.entity);
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
