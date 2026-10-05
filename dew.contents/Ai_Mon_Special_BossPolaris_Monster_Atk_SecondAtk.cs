using Mirror;

public class Ai_Mon_Special_BossPolaris_Monster_Atk_SecondAtk : InstantDamageInstance
{
	public float postDaze = 1.25f;

	protected override void OnCreate()
	{
		base.OnCreate();
		_ = ((NetworkBehaviour)this).isServer;
	}

	protected override void OnAfterDelay()
	{
		base.OnAfterDelay();
		info.caster.Control.StartDisplacement(new DispByDestination
		{
			duration = 0.5f / info.caster.Status.attackSpeedMultiplier,
			affectedByMovementSpeed = false,
			canGoOverTerrain = false,
			destination = info.caster.agentPosition + info.forward * 1.5f,
			ease = DewEase.EaseOutQuad,
			isCanceledByCC = true,
			isFriendly = true
		});
	}

	protected override void ActiveFrameUpdate()
	{
		base.ActiveFrameUpdate();
		position = info.caster.agentPosition;
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && !info.caster.IsNullInactiveDeadOrKnockedOut())
		{
			info.caster.Control.StartDaze(postDaze / info.caster.Status.attackSpeedMultiplier);
		}
	}

	private void MirrorProcessed()
	{
	}
}
