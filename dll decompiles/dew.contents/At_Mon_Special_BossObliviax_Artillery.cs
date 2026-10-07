public class At_Mon_Special_BossObliviax_Artillery : AbilityTrigger
{
	public override void OnCastStart(int configIndex, CastInfo info)
	{
		base.OnCastStart(configIndex, info);
		owner.Control.Rotate(ManagerBase<CameraManager>.instance.entityCamAngle + 140f, immediately: false);
	}

	private void MirrorProcessed()
	{
	}
}
