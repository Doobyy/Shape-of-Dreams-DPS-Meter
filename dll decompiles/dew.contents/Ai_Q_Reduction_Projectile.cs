public class Ai_Q_Reduction_Projectile : StandardProjectile
{
	public ScalingValue damageAmount;

	public ScalingValue healAmount;

	public override bool reuseInRoom => true;

	protected override void OnDisable()
	{
		base.OnDisable();
		collisionTargets.targets = EntityRelation.Neutral | EntityRelation.Enemy | EntityRelation.Ally;
	}

	protected override void OnEntity(EntityHit hit)
	{
		base.OnEntity(hit);
		if (info.caster.CheckEnemyOrNeutral(hit.entity))
		{
			Damage(damageAmount).SetDirection(info.forward).SetAttr(DamageAttribute.ForceMergeNumber).Dispatch(hit.entity);
		}
		else
		{
			Heal(healAmount).SetCanMerge().Dispatch(hit.entity);
		}
	}

	private void MirrorProcessed()
	{
	}
}
