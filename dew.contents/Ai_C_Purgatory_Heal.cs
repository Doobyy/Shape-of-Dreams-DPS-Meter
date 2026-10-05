public class Ai_C_Purgatory_Heal : StandardProjectile
{
	public ScalingValue healAmount;

	public override bool reuseInRoom => true;

	protected override void OnEntity(EntityHit hit)
	{
		base.OnEntity(hit);
		Heal(healAmount).Dispatch(hit.entity);
	}

	private void MirrorProcessed()
	{
	}
}
