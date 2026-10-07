using Mirror;

public class Se_RoomMod_WarpingField_Warp : StatusEffect
{
	public float duration = 0.35f;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			if (victim is Hero)
			{
				DoUncollidable();
				DoInvulnerable();
			}
			victim.Control.StartDisplacement(new DispByDestination
			{
				destination = info.point,
				isFriendly = true,
				affectedByMovementSpeed = false,
				isCanceledByCC = false,
				rotateForward = false,
				canGoOverTerrain = true,
				duration = duration,
				onCancel = DestroyIfActive,
				onFinish = DestroyIfActive,
				ease = DewEase.EaseOutQuad
			});
		}
	}

	private void MirrorProcessed()
	{
	}
}
