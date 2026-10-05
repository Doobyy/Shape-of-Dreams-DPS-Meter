using Mirror;

public class Se_Mon_Primus_BossPrimusAeron_Adapt_Doom_Initial : StatusEffect
{
	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DoInvulnerable();
			DestroyOnCondition(() => ((Mon_Primus_BossPrimusAeron)info.caster).phase != Mon_Primus_BossPrimusAeron.PhaseType.Adapt);
			info.caster.Control.Rotate(180f + ManagerBase<CameraManager>.instance.entityCamAngle, immediately: false);
			info.caster.Control.StartDisplacement(new DispByDestination
			{
				destination = SingletonBehaviour<Room_BossArena>.instance.center,
				duration = 1.5f,
				ease = DewEase.EaseOutQuad,
				isFriendly = true,
				onCancel = Destroy,
				onFinish = () =>
				{
					CreateStatusEffect<Se_Mon_Primus_BossPrimusAeron_Adapt_Doom_Ongoing>(victim);
					Destroy();
				},
				rotateForward = false,
				canGoOverTerrain = true
			});
		}
	}

	private void MirrorProcessed()
	{
	}
}
