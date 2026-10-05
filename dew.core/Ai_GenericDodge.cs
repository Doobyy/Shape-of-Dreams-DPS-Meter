using Mirror;
using UnityEngine;

public class Ai_GenericDodge : AbilityInstance
{
	public float speed = 12f;

	public DewEase ease;

	public float uncollidableRatio = 0.7f;

	public float minDistance = 2f;

	public float animSpeedMultiplier = 1f;

	public bool rotateForward;

	public DewAnimationClip anim;

	public bool doAttackReset;

	protected override void OnCreate()
	{
		base.OnCreate();
		((Component)(object)this).transform.rotation = Quaternion.LookRotation(info.point - info.caster.agentPosition).Flattened();
		if (((NetworkBehaviour)this).isServer)
		{
			if (doAttackReset && (Object)(object)info.caster != null)
			{
				ResetCooldown(info.caster.Ability.attackAbility);
			}
			Vector3 validAgentDestination_LinearSweep = Dew.GetValidAgentDestination_LinearSweep(info.caster.agentPosition, info.point);
			Vector3 vector = validAgentDestination_LinearSweep - info.caster.agentPosition;
			float magnitude = vector.magnitude;
			if (magnitude < minDistance)
			{
				vector = vector.normalized * minDistance;
				validAgentDestination_LinearSweep = info.caster.agentPosition + vector;
				validAgentDestination_LinearSweep = Dew.GetValidAgentDestination_LinearSweep(info.caster.agentPosition, validAgentDestination_LinearSweep);
				magnitude = minDistance;
			}
			float num = magnitude / speed;
			if (anim != null && !ShouldSkipAnimation(info.caster))
			{
				info.caster.Animation.PlayAbilityAnimation(anim, animSpeedMultiplier * anim.entries[0].duration / num);
			}
			CreateBasicEffect(info.caster, new UncollidableEffect(), num * uncollidableRatio, "dodge_uncol");
			info.caster.Control.StartDisplacement(new DispByDestination
			{
				destination = validAgentDestination_LinearSweep,
				duration = num,
				ease = ease,
				isFriendly = true,
				isDodging = true,
				onCancel = Destroy,
				onFinish = Destroy,
				rotateForward = (rotateForward && !ShouldSkipAnimation(info.caster)),
				canGoOverTerrain = false,
				isCanceledByCC = false
			});
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && !info.caster.IsNullOrInactive())
		{
			info.caster.Animation.StopAbilityAnimation(anim);
		}
	}

	public static bool ShouldSkipAnimation(Entity entity)
	{
		if (entity.Control.IsActionBlocked(EntityControl.BlockableAction.Ability) != EntityControl.BlockStatus.Allowed)
		{
			return entity.Animation.abilityAnimStatus.isPlaying;
		}
		return false;
	}

	private void MirrorProcessed()
	{
	}
}
