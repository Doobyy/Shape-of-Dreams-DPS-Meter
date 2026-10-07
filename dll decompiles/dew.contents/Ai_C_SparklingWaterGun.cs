public class Ai_C_SparklingWaterGun : StandardProjectile
{
	public ScalingValue healAmount;

	public float damageAmount = 1f;

	public float stunDuration = 1.5f;

	public float summonAmp = 0.4f;

	public override bool reuseInRoom => true;

	protected override void OnEntity(EntityHit hit)
	{
		base.OnEntity(hit);
		if (info.caster.CheckEnemyOrNeutral(hit.entity))
		{
			PureDamage(damageAmount).SetDirection(info.forward).Dispatch(hit.entity);
			CreateBasicEffect(hit.entity, new StunEffect(), stunDuration);
			return;
		}
		HealData healData = Heal(healAmount);
		if (hit.entity is Summon)
		{
			healData.ApplyAmplification(summonAmp);
			healData.SetCrit();
		}
		healData.Dispatch(hit.entity);
		if (hit.entity.Status.TryGetStatusEffect<Se_Elm_Fire>(out var effect))
		{
			effect.Destroy();
		}
	}

	private void MirrorProcessed()
	{
	}
}
