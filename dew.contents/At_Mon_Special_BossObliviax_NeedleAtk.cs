public class At_Mon_Special_BossObliviax_NeedleAtk : AbilityTrigger
{
	public override void OnCastStart(int configIndex, CastInfo info)
	{
		base.OnCastStart(configIndex, info);
		owner.Control.Rotate(ManagerBase<CameraManager>.instance.entityCamAngle + 180f + 40f, immediately: false);
	}

	private void MirrorProcessed()
	{
	}
}
