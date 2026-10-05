public class Ai_Gem_R_Snow_Projectile : StandardProjectile
{
	public ScalingValue damage;

	public override bool reuseInRoom => true;

	protected override void OnEntity(EntityHit hit)
	{
		base.OnEntity(hit);
		Damage(damage, 0.5f).SetElemental(ElementalType.Cold).Dispatch(hit.entity, chain);
	}

	private void MirrorProcessed()
	{
	}
}
