using Mirror;
using UnityEngine;

public class Ai_Mon_Forest_SpiderSpitter_Backdash : AbilityInstance
{
	public GameObject flyEffect;

	public GameObject landEffect;

	public float duration = 0.5f;

	public float distance = 3f;

	public float minDistance = 2f;

	public float afterLandDelay = 0.5f;

	public float jumpStrength;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		FxPlay(flyEffect, info.caster);
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		Vector3 vector = Vector3.zero;
		float num = float.NegativeInfinity;
		for (float num2 = 0f; num2 <= 360f; num2 += 15f)
		{
			Vector3 end = info.caster.agentPosition + Quaternion.Euler(0f, num2, 0f) * Vector3.forward * distance;
			end = Dew.GetValidAgentDestination_LinearSweep(info.caster.agentPosition, end);
			float num3 = Vector3.Distance(info.target.agentPosition, end);
			if (num3 > num)
			{
				num = num3;
				vector = end;
			}
		}
		info.caster.Control.Rotate(info.caster.position - vector, immediately: false);
		info.caster.Visual.KnockUp(jumpStrength, isFriendly: true);
		info.caster.Control.StartDisplacement(new DispByDestination
		{
			affectedByMovementSpeed = true,
			canGoOverTerrain = false,
			destination = vector,
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

	private void MirrorProcessed()
	{
	}
}
