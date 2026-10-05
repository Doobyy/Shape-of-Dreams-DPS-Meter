public class Ai_Mon_Special_BossMaw_Purgatory_Heal : StandardProjectile
{
	public float healAmount;

	protected override void OnEntity(EntityHit hit)
	{
		base.OnEntity(hit);
		Heal(info.caster.Status.missingHealth * healAmount).Dispatch(hit.entity);
	}

	private void MirrorProcessed()
	{
	}
}
