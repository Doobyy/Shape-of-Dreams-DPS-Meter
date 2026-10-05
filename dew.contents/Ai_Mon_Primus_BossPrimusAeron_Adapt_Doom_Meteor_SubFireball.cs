public class Ai_Mon_Primus_BossPrimusAeron_Adapt_Doom_Meteor_SubFireball : StandardProjectile
{
	public ScalingValue subDamage;

	public override bool reuseInRoom => true;

	public override bool reuseInRoomSkipPrewarmCap => true;

	protected override void OnEntity(EntityHit hit)
	{
		base.OnEntity(hit);
		Damage(subDamage).SetElemental(ElementalType.Fire).SetDirection(info.forward).Dispatch(hit.entity);
	}

	private void MirrorProcessed()
	{
	}
}
