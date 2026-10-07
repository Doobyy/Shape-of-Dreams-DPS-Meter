using Mirror;
using UnityEngine;

public class Ai_Mon_SnowMountain_Scavenger_Jump : AbilityInstance
{
	public GameObject fxFly;

	public GameObject fxLand;

	public DewAnimationClip animLand;

	public float duration;

	public float minRange = 3f;

	public float afterLandDelay = 0.5f;

	public float jumpStrength = 0.5f;

	protected override void OnCreate()
	{
		base.OnCreate();
		FxPlay(fxFly, info.caster);
		if (((NetworkBehaviour)this).isServer)
		{
			Vector3 vector = info.point;
			if (Vector3.Distance(info.caster.position, vector) < minRange)
			{
				vector = info.caster.position + (vector - info.caster.position).normalized * minRange;
			}
			Vector3 validAgentDestination_LinearSweep = Dew.GetValidAgentDestination_LinearSweep(info.caster.agentPosition, vector);
			info.caster.Visual.KnockUp(jumpStrength, isFriendly: true);
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
					info.caster.Animation.PlayAbilityAnimation(animLand);
					FxPlayNetworked(fxLand, info.caster);
					Destroy();
				},
				rotateForward = true
			});
		}
	}

	private void MirrorProcessed()
	{
	}
}
