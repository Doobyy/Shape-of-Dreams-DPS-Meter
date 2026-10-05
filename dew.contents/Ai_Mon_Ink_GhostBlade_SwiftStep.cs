using System.Collections;
using Mirror;
using UnityEngine;

public class Ai_Mon_Ink_GhostBlade_SwiftStep : AbilityInstance
{
	public DewAnimationClip startAnimation;

	public GameObject fxSmoke;

	public float duration;

	public float backDistance;

	public DewEase ease;

	public bool doInvulnerable;

	public bool doUncollidable;

	public override bool reuseInRoom => true;

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		DestroyOnDeath(info.caster);
		Vector3 normalized = (info.target.agentPosition - info.caster.agentPosition).normalized;
		FxPlayNetworked(fxSmoke, info.caster.agentPosition, Quaternion.LookRotation(normalized, Vector3.up));
		info.caster.Visual.DisableRenderers();
		info.caster.Control.StartDaze(duration);
		info.caster.Animation.PlayAbilityAnimation(startAnimation);
		Vector3 vector = info.target.agentPosition + normalized * backDistance;
		vector = Dew.GetPositionOnGround(vector);
		vector = Dew.GetValidAgentDestination_Closest(info.caster.agentPosition, vector);
		if (doInvulnerable)
		{
			CreateBasicEffect(info.caster, new InvulnerableEffect(), duration, "ghostblade_invul");
		}
		if (doUncollidable)
		{
			CreateBasicEffect(info.caster, new UncollidableEffect(), duration, "ghostblade_uncol");
		}
		ActorRef<Actor> selfRef = this;
		info.caster.Control.StartDisplacement(new DispByDestination
		{
			affectedByMovementSpeed = false,
			canGoOverTerrain = true,
			destination = vector,
			duration = duration,
			ease = ease,
			isCanceledByCC = false,
			isFriendly = true,
			onCancel = () =>
			{
				if ((Object)(object)selfRef.Get() != null)
				{
					DestroyIfActive();
				}
			},
			onFinish = () =>
			{
				if (!((Object)(object)selfRef.Get() == null))
				{
					float angle = CastInfo.GetAngle(info.target.agentPosition - info.caster.agentPosition);
					CreateAbilityInstance<Ai_Mon_Ink_GhostBlade_SwiftStep_Atk>(info.caster.position, Quaternion.Euler(0f, angle, 0f), new CastInfo(info.caster, angle));
					DestroyIfActive();
				}
			},
			rotateForward = false
		});
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (Object)(object)info.caster != null)
		{
			if (!info.target.IsNullOrInactive())
			{
				info.caster.Control.RotateTowards(info.target, immediately: true);
			}
			info.caster.Visual.EnableRenderers();
		}
	}

	private void MirrorProcessed()
	{
	}
}
