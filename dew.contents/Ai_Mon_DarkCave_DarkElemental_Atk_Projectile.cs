public class Ai_Mon_DarkCave_DarkElemental_Atk_Projectile : StandardProjectile
{
	public ScalingValue damage;

	public override bool reuseInRoom => true;

	protected override void OnEntity(EntityHit hit)
	{
		base.OnEntity(hit);
		Damage(damage).SetElemental(ElementalType.Dark).SetAttr(DamageAttribute.ForceMergeNumber).Dispatch(hit.entity);
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
