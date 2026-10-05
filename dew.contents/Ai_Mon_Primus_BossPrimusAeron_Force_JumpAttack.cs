using System.Collections;
using Mirror;
using UnityEngine;

public class Ai_Mon_Primus_BossPrimusAeron_Force_JumpAttack : AbilityInstance
{
	public float dashDuration = 0.75f;

	public int coneCount = 5;

	public float destGap = 2f;

	public float postDelay = 2f;

	public override bool reuseInRoom => true;

	protected override IEnumerator OnCreateSequenced()
	{
		position = info.point;
		if (((NetworkBehaviour)this).isServer)
		{
			DestroyOnDeath(info.caster);
			CreateBasicEffect(info.caster, new UnstoppableEffect(), float.PositiveInfinity).DestroyOnDestroy(this);
			DestroyOnCondition(() => ((Mon_Primus_BossPrimusAeron)info.caster).phase != Mon_Primus_BossPrimusAeron.PhaseType.Force);
			Vector3 destination;
			if (Vector3.Distance(info.caster.agentPosition, info.point) < destGap)
			{
				destination = info.point;
			}
			else
			{
				Vector3 vector = info.point - info.caster.agentPosition;
				destination = info.caster.agentPosition + vector.normalized * (vector.magnitude - destGap);
			}
			info.caster.Control.StartDaze(dashDuration + postDelay);
			info.caster.Control.StartDisplacement(new DispByDestination
			{
				destination = destination,
				isFriendly = true,
				duration = dashDuration,
				canGoOverTerrain = true,
				isCanceledByCC = true,
				affectedByMovementSpeed = false,
				ease = DewEase.EaseOutQuad,
				onCancel = DestroyIfActive,
				onFinish = Finish,
				rotateForward = true
			});
		}
		yield break;
	}

	private void Finish()
	{
		CreateAbilityInstance<Ai_Mon_Primus_BossPrimusAeron_Force_JumpAttack_CircleInstance>(info.point, null, new CastInfo(info.caster));
		float num = Random.Range(0f, 360f);
		for (int i = 0; i < coneCount; i++)
		{
			Quaternion value = Quaternion.Euler(0f, num + (float)i * 360f / (float)coneCount, 0f);
			CreateAbilityInstance<Ai_Mon_Primus_BossPrimusAeron_Force_JumpAttack_ConeInstance>(info.point, value, new CastInfo(info.caster));
		}
		DestroyIfActive();
	}

	private void MirrorProcessed()
	{
	}
}
