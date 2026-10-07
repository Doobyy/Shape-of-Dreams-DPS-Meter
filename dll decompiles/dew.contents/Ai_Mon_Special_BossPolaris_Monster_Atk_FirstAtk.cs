public class Ai_Mon_Special_BossPolaris_Monster_Atk_FirstAtk : InstantDamageInstance
{
	protected override void OnCreate()
	{
		base.OnCreate();
	}

	protected override void OnAfterDelay()
	{
		base.OnAfterDelay();
		FxStopNetworked(startEffect);
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

	private void MirrorProcessed()
	{
	}
}
