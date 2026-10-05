public class Ai_Mon_SnowMountain_Scavenger_Throw : StandardProjectile
{
	public ScalingValue damage;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (info.caster is Mon_SnowMountain_Scavenger mon_SnowMountain_Scavenger)
		{
			mon_SnowMountain_Scavenger.HideWeaponTemporarilyLocal();
		}
	}

	protected override void OnEntity(EntityHit hit)
	{
		base.OnEntity(hit);
		Damage(damage).SetOriginPosition(info.caster.position).Dispatch(hit.entity);
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
