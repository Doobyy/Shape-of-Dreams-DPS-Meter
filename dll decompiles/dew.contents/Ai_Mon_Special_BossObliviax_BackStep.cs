using System.Collections;
using Mirror;
using UnityEngine;

public class Ai_Mon_Special_BossObliviax_BackStep : AbilityInstance
{
	public float startDelay;

	public float postDelay;

	public float moveDistance;

	public float moveDuration;

	public DewEase ease;

	public DewAnimationClip startAnimClip;

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		CreateBasicEffect(info.caster, new UnstoppableEffect(), 10f, "ObliviaxUnstoppable").DestroyOnDestroy(this);
		DestroyOnDeath(info.caster);
		info.caster.Control.StartDaze(startDelay);
		info.caster.Control.RotateTowards(info.target, immediately: true);
		yield return new SI.WaitForSeconds(startDelay);
		if (Vector3.Distance(info.caster.position, info.target.GetAIPosition(info.caster)) < 8f)
		{
			info.caster.Animation.PlayAbilityAnimation(startAnimClip);
			Vector3 end = info.caster.agentPosition + (info.caster.agentPosition - info.target.GetAIAgentPosition(info.caster)).normalized * moveDistance;
			end = Dew.GetValidAgentDestination_LinearSweep(info.caster.agentPosition, end);
			info.caster.Control.StartDisplacement(new DispByDestination
			{
				affectedByMovementSpeed = false,
				canGoOverTerrain = false,
				destination = end,
				duration = moveDuration,
				ease = ease,
				isCanceledByCC = false,
				isFriendly = true,
				rotateForward = false,
				onFinish = () =>
				{
					info.caster.Control.RotateTowards(info.target, immediately: true);
					CreateAbilityInstance<Ai_Mon_Special_BossObliviax_BackStep_Instance>(info.caster.position, null, info);
				}
			});
		}
		else
		{
			info.caster.Control.RotateTowards(info.target, immediately: true);
			CreateAbilityInstance<Ai_Mon_Special_BossObliviax_BackStep_Instance>(info.caster.position, null, info);
		}
		info.caster.Control.StartDaze(postDelay);
		yield return new SI.WaitForSeconds(postDelay);
		Destroy();
	}

	private void Rotate()
	{
		float initAngle = info.caster.Control.desiredAngle;
		float angle = 0f;
		float angularVelocity = 180f / moveDuration;
		((MonoBehaviour)(object)this).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			while (angle < 180f)
			{
				angle += angularVelocity * Time.deltaTime;
				info.caster.Control.Rotate(initAngle + angle, immediately: false);
				yield return null;
			}
			info.caster.Control.Rotate(initAngle + 180f, immediately: false);
		}
	}

	private void MirrorProcessed()
	{
	}
}
