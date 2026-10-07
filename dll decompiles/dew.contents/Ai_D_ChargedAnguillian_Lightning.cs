public class Ai_D_ChargedAnguillian_Lightning : StandardProjectile, ACH_THEYRE_JUST_BIG_CATS.ILightingActor
{
	public ScalingValue damage;

	public override bool reuseInRoom => true;

	protected override void OnEntity(EntityHit hit)
	{
		base.OnEntity(hit);
		Damage(damage).SetOriginPosition(info.caster.agentPosition).SetElemental(ElementalType.Light).Dispatch(hit.entity);
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
