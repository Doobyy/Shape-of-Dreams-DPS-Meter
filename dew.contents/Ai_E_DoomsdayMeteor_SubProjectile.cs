public class Ai_E_DoomsdayMeteor_SubProjectile : StandardProjectile, IOtherPlayersTonedDownDisable
{
	public ScalingValue subDamage;

	public override bool reuseInRoom => true;

	protected override void OnEntity(EntityHit hit)
	{
		base.OnEntity(hit);
		Damage(subDamage).SetElemental(ElementalType.Fire).Dispatch(hit.entity);
	}

	private void MirrorProcessed()
	{
	}
}
