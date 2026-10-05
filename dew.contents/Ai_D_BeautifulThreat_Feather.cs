public class Ai_D_BeautifulThreat_Feather : StandardProjectile
{
	public ScalingValue healAmount;

	public ScalingValue damageAmount;

	public ScalingValue selfHealOnDamageAmount;

	public override bool reuseInRoom => true;

	protected override void OnPrepare()
	{
		base.OnPrepare();
	}

	protected override void OnEntity(EntityHit hit)
	{
		base.OnEntity(hit);
		if (info.caster.CheckEnemyOrNeutral(hit.entity))
		{
			Damage(damageAmount).SetOriginPosition(info.caster.position).SetAttr(DamageAttribute.ForceMergeNumber).Dispatch(hit.entity, chain);
			Heal(selfHealOnDamageAmount).SetCanMerge().Dispatch(info.caster, chain);
		}
		else
		{
			Heal(healAmount).SetCanMerge().Dispatch(hit.entity, chain);
		}
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
