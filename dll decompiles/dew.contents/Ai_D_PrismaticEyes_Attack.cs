public class Ai_D_PrismaticEyes_Attack : AttackProjectile
{
	public ScalingValue addedDamage;

	public override bool reuseInRoom => true;

	protected override bool detachEntityHitEffect => false;

	protected override void OnEntity(EntityHit hit)
	{
		base.OnEntity(hit);
		Damage(addedDamage).SetOriginPosition(info.caster.position).Dispatch(hit.entity);
	}

	private void MirrorProcessed()
	{
	}
}
