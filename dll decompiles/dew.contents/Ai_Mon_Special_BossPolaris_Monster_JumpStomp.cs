using Mirror;

public class Ai_Mon_Special_BossPolaris_Monster_JumpStomp : AbilityInstance
{
	public float dashDuration = 0.25f;

	public float postDaze = 1.25f;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			info.caster.Control.StartDisplacement(new DispByDestination
			{
				duration = dashDuration,
				ease = DewEase.Linear,
				destination = info.point,
				canGoOverTerrain = true,
				affectedByMovementSpeed = false,
				isCanceledByCC = false,
				isFriendly = true,
				rotateSmoothly = false,
				onFinish = () =>
				{
					Destroy();
					CreateAbilityInstance<Ai_Mon_Special_BossPolaris_Monster_JumpStomp_Explosion>(info.point, null, new CastInfo(info.caster));
					info.caster.Control.StartDaze(postDaze);
					info.caster.Control.Rotate(ManagerBase<CameraManager>.instance.entityCamAngle + 180f, immediately: true, 1f);
				},
				onCancel = Destroy
			});
			CreateBasicEffect(info.caster, new UnstoppableEffect(), float.PositiveInfinity, "JumpStomp").DestroyOnDestroy(this);
		}
	}

	private void MirrorProcessed()
	{
	}
}
