using Mirror;
using UnityEngine;

public class Ai_Mon_Despair_DreadBug_JumpBack : AbilityInstance
{
	public GameObject flyEffect;

	public GameObject landEffect;

	public float duration;

	public float backJumpDis;

	public float afterLandDelay = 0.5f;

	public float jumpStrength = 0.5f;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		FxPlay(flyEffect, info.caster);
		if (((NetworkBehaviour)this).isServer && !info.target.IsNullInactiveDeadOrKnockedOut())
		{
			Vector3 normalized = (info.caster.agentPosition - info.target.agentPosition).normalized;
			Vector3 end = info.caster.agentPosition + normalized * backJumpDis;
			Vector3 validAgentDestination_LinearSweep = Dew.GetValidAgentDestination_LinearSweep(info.caster.agentPosition, end);
			info.caster.Visual.KnockUp(jumpStrength, isFriendly: true);
			info.caster.Control.RotateTowards(info.target, immediately: true, duration);
			info.caster.Control.StartDisplacement(new DispByDestination
			{
				affectedByMovementSpeed = true,
				canGoOverTerrain = false,
				destination = validAgentDestination_LinearSweep,
				duration = duration,
				ease = DewEase.Linear,
				isCanceledByCC = true,
				isFriendly = true,
				onCancel = Destroy,
				onFinish = () =>
				{
					info.caster.Control.StartDaze(afterLandDelay);
					FxPlayNetworked(landEffect, info.caster);
					Destroy();
				},
				rotateForward = false
			});
		}
	}

	private void MirrorProcessed()
	{
	}
}
