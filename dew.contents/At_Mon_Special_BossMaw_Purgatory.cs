public class At_Mon_Special_BossMaw_Purgatory : AbilityTrigger
{
	public override void OnCastStart(int configIndex, CastInfo info)
	{
		base.OnCastStart(configIndex, info);
		if ((bool)SingletonBehaviour<Room_BossArena>.instance)
		{
			owner.Control.Rotate(ManagerBase<CameraManager>.instance.entityCamAngle + 180f, immediately: false);
			owner.Control.StartDisplacement(new DispByDestination
			{
				isCanceledByCC = true,
				isFriendly = true,
				destination = SingletonBehaviour<Room_BossArena>.instance.center,
				canGoOverTerrain = true,
				ease = DewEase.EaseInOutQuad,
				rotateForward = false,
				duration = configs[0].channel.duration
			});
		}
	}

	private void MirrorProcessed()
	{
	}
}
