public class At_Mon_Forest_BossDemon_Spawn : AbilityTrigger
{
	public override void OnCastStart(int configIndex, CastInfo info)
	{
		base.OnCastStart(configIndex, info);
		info.caster.Control.Rotate(ManagerBase<CameraManager>.instance.entityCamAngle + 180f, immediately: false);
	}

	private void MirrorProcessed()
	{
	}
}
