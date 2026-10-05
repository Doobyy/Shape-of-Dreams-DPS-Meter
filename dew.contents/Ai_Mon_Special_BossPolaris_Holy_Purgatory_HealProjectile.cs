public class Ai_Mon_Special_BossPolaris_Holy_Purgatory_HealProjectile : StandardProjectile
{
	public float maxHpRatio;

	protected override void OnEntity(EntityHit hit)
	{
		base.OnEntity(hit);
		Heal(maxHpRatio * hit.entity.Status.maxHealth).Dispatch(hit.entity);
	}

	private void MirrorProcessed()
	{
	}
}
