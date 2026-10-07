public class Ai_Mon_LavaLand_InfernoSpider_LavaAtk : StandardProjectile
{
	public ScalingValue dmgFactor;

	public override bool reuseInRoom => true;

	protected override void OnEntity(EntityHit hit)
	{
		base.OnEntity(hit);
		Damage(dmgFactor).SetOriginPosition(info.caster.position).SetElemental(ElementalType.Fire).Dispatch(hit.entity);
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
