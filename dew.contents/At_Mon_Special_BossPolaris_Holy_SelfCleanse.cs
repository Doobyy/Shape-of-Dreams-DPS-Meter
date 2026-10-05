public class At_Mon_Special_BossPolaris_Holy_SelfCleanse : AbilityTrigger
{
	public override void OnCastStart(int configIndex, CastInfo info)
	{
		base.OnCastStart(configIndex, info);
		info.caster.Control.Rotate(ManagerBase<CameraManager>.instance.entityCamAngle + 180f, immediately: false, 1f);
	}

	private void MirrorProcessed()
	{
	}
}
