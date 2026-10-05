public class At_Mon_Primus_BossPrimusAeron_Rage_MassDelusion : AbilityTrigger
{
	public override void OnCastStart(int configIndex, CastInfo info)
	{
		base.OnCastStart(configIndex, info);
		owner.Control.Rotate(180f + ManagerBase<CameraManager>.instance.entityCamAngle, immediately: false);
	}

	private void MirrorProcessed()
	{
	}
}
