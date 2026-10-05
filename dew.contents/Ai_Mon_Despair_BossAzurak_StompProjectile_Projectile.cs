public class Ai_Mon_Despair_BossAzurak_StompProjectile_Projectile : StandardProjectile
{
	public ScalingValue dmgFactor;

	public ScalingValue tickDmgFactor;

	public Knockback knockback;

	public override bool reuseInRoom => true;

	protected override void OnEntity(EntityHit hit)
	{
		base.OnEntity(hit);
		CreateDamage(DamageData.SourceType.Default, dmgFactor).SetOriginPosition(position).SetDirection(info.forward).Dispatch(hit.entity);
		knockback.ApplyWithOrigin(position, hit.entity);
		if (hit.entity.Status.TryGetStatusEffect<Se_Mon_Despair_ParalyticFly_Atk_Instance>(out var effect))
		{
			effect.ResetTimer();
			return;
		}
		CreateStatusEffect(hit.entity, info, (Se_Mon_Despair_ParalyticFly_Atk_Instance b) =>
		{
			b.tickDmgFactor = tickDmgFactor;
		});
	}

	private void MirrorProcessed()
	{
	}
}
