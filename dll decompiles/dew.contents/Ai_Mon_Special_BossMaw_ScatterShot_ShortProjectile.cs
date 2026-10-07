public class Ai_Mon_Special_BossMaw_ScatterShot_ShortProjectile : StandardProjectile
{
	public Knockback knockback;

	public ScalingValue dmgFactor;

	protected override void OnEntity(EntityHit hit)
	{
		base.OnEntity(hit);
		CreateDamage(DamageData.SourceType.Default, dmgFactor).SetDirection(rotation).SetElemental(ElementalType.Dark).Dispatch(hit.entity);
		knockback.ApplyWithDirection(rotation, hit.entity);
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
