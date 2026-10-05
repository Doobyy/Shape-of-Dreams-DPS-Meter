public class Ai_Mon_Ink_GhostBlade_SwiftStep_Projectile : StandardProjectile
{
	public Knockback knockback;

	public ScalingValue dmgFactor;

	public override bool reuseInRoom => true;

	protected override void OnEntity(EntityHit hit)
	{
		base.OnEntity(hit);
		knockback.ApplyWithOrigin(hit.point, hit.entity);
		CreateDamage(DamageData.SourceType.Default, dmgFactor).SetElemental(ElementalType.Dark).SetOriginPosition(hit.point).Dispatch(hit.entity);
	}

	private void MirrorProcessed()
	{
	}
}
