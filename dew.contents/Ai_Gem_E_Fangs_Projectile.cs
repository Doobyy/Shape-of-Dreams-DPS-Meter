public class Ai_Gem_E_Fangs_Projectile : StandardProjectile
{
	public ScalingValue damage;

	public override bool reuseInRoom => true;

	protected override void OnDisable()
	{
		base.OnDisable();
		onEntity = null;
	}

	protected override void OnEntity(EntityHit hit)
	{
		base.OnEntity(hit);
		Damage(damage).SetOriginPosition(info.caster.position).Dispatch(hit.entity);
	}

	private void MirrorProcessed()
	{
	}
}
