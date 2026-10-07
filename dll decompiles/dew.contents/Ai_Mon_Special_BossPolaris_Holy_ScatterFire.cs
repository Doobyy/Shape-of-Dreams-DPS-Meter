public class Ai_Mon_Special_BossPolaris_Holy_ScatterFire : InstantDamageInstance
{
	protected override void OnHit(Entity entity)
	{
		base.OnHit(entity);
		if (!entity.Status.hasDamageImmunity)
		{
			if (entity.Status.TryGetStatusEffect<Se_Mon_Special_BossPolaris_CleansingFlame>(out var effect))
			{
				effect.ResetTimer();
			}
			else
			{
				CreateStatusEffect<Se_Mon_Special_BossPolaris_CleansingFlame>(entity);
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
