using System.Collections;
using Mirror;
using UnityEngine;

public class Ai_Mon_Ink_BossWhiteNight_DestructionWave : AbilityInstance
{
	public DewAnimationClip startClip;

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
		Vector3 normalized = (info.target.GetAIAgentPosition(info.caster) - info.caster.agentPosition).normalized;
		FxPlayNetworked(fxSmoke, info.caster.agentPosition, Quaternion.LookRotation(normalized, Vector3.up));
		info.caster.Visual.DisableRenderers();
		info.caster.Control.StartDaze(duration + 0.5f);
		info.caster.Animation.PlayAbilityAnimation(startClip);
		Vector3 end = info.target.GetAIAgentPosition(info.caster) + normalized * backDistance;
		end = Dew.GetValidAgentDestination_Closest(info.caster.agentPosition, end);
		end = Dew.GetPositionOnGround(end);
		if (doInvulnerable)
		{
			CreateBasicEffect(info.caster, new InvulnerableEffect(), duration, "whitenight_invul");
		}
		if (doUncollidable)
		{
			CreateBasicEffect(info.caster, new UncollidableEffect(), duration, "whitenight_uncol");
		}
		info.caster.Control.StartDisplacement(new DispByDestination
		{
			affectedByMovementSpeed = false,
			canGoOverTerrain = true,
			destination = end,
			duration = duration,
			ease = ease,
			isCanceledByCC = false,
			isFriendly = true,
			onCancel = DestroyIfActive,
			onFinish = () =>
			{
				if (!info.target.IsNullOrInactive())
				{
					info.caster.Control.RotateTowards(info.target, immediately: true);
				}
				info.caster.Control.StartDaze(1f);
				CreateAbilityInstance<Ai_Mon_Ink_BossWhiteNight_DestructionWave_Wave>(info.caster.position, null, info);
				DestroyIfActive();
			},
			rotateForward = false
		});
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (Object)(object)info.caster != null)
		{
			info.caster.Visual.EnableRenderers();
		}
	}

	private void MirrorProcessed()
	{
	}
}
