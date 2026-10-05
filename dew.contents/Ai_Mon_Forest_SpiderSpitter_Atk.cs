public class Ai_Mon_Forest_SpiderSpitter_Atk : StandardProjectile
{
	public ScalingValue dmgFactor;

	public override bool reuseInRoom => true;

	protected override void OnEntity(EntityHit hit)
	{
		base.OnEntity(hit);
		Damage(dmgFactor).SetOriginPosition(info.caster.position).Dispatch(hit.entity);
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
