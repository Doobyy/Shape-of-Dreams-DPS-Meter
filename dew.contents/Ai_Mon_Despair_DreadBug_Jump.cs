using Mirror;
using UnityEngine;

public class Ai_Mon_Despair_DreadBug_Jump : AbilityInstance
{
	public GameObject flyEffect;

	public GameObject landEffect;

	public float duration;

	public float minRange = 3f;

	public float afterLandDelay = 0.5f;

	public float jumpStrength = 0.5f;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		FxPlay(flyEffect, info.caster);
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
					FxPlayNetworked(landEffect, info.caster);
					info.caster.Control.StartDaze(afterLandDelay);
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
