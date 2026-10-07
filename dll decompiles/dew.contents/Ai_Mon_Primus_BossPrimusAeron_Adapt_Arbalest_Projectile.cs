public class Ai_Mon_Primus_BossPrimusAeron_Adapt_Arbalest_Projectile : StandardProjectile
{
	public ScalingValue damage;

	public Knockback knockback;

	public override bool reuseInRoom => true;

	protected override void OnEntity(EntityHit hit)
	{
		base.OnEntity(hit);
		Damage(damage).SetElemental(ElementalType.Dark).SetDirection(info.forward).Dispatch(hit.entity);
		knockback.ApplyWithDirection(info.forward, hit.entity);
	}

	private void MirrorProcessed()
	{
	}
}
