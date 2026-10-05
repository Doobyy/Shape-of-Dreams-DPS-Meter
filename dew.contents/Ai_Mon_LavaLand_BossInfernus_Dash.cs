using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_Mon_LavaLand_BossInfernus_Dash : AbilityInstance
{
	public float distance;

	public float duration;

	public DewEase dashEase;

	public DewCollider range;

	public ScalingValue dmgFactor;

	public GameObject fxHit;

	public float delay;

	public float postDelay;

	public GameObject fxCast;

	public DewAnimationClip startClip;

	public DewAnimationClip endClip;

	public override bool reuseInRoom => true;

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		DestroyOnDeath(info.caster);
		CreateBasicEffect(info.caster, new UnstoppableEffect(), float.PositiveInfinity).DestroyOnDestroy(this);
		range.transform.position = info.caster.position;
		range.transform.rotation = info.caster.rotation;
		List<Entity> entities = range.GetEntities(out var handle, tvDefaultHarmfulEffectTargets);
		bool enableAfterAtk = entities.Count > 0;
		for (int i = 0; i < entities.Count; i++)
		{
			Entity entity = entities[i];
			CreateDamage(DamageData.SourceType.Default, dmgFactor).SetOriginPosition(info.caster.position).SetDirection(info.forward).Dispatch(entity);
			FxPlayNewNetworked(fxHit, entity);
			if (!entity.Status.hasCrowdControlImmunity)
			{
				Quaternion value = info.rotation;
				CreateAbilityInstance(entity.position, value, new CastInfo(info.caster, entity), (Ai_Mon_LavaLand_BossInfernus_WallStunKnockback b) =>
				{
					b.knockbackSpeed *= 1.5f;
					b.wallStunDmg *= 1.5f;
					b.additionalKnockbackDist = 4f;
				});
			}
		}
		handle.Return();
		Vector3 end = info.caster.agentPosition + info.forward * distance;
		end = Dew.GetValidAgentDestination_LinearSweep(info.caster.agentPosition, end);
		info.caster.Control.StartDisplacement(new DispByDestination
		{
			destination = end,
			duration = duration,
			ease = dashEase,
			isFriendly = true,
			onCancel = DestroyIfActive,
			onFinish = () =>
			{
				if (enableAfterAtk)
				{
					FxPlayNetworked(fxCast, info.caster);
					info.caster.Animation.PlayAbilityAnimation(startClip);
					info.caster.Control.StartChannel(new Channel
					{
						blockedActions = Channel.BlockedAction.Everything,
						duration = delay,
						isAttack = false,
						onCancel = () =>
						{
							FxStopNetworked(fxCast);
							DestroyIfActive();
						},
						onComplete = () =>
						{
							FxStopNetworked(fxCast);
							info.caster.Animation.PlayAbilityAnimation(endClip);
							info.caster.Control.StartDaze(postDelay * 1.5f);
							CreateAbilityInstance<Ai_Mon_LavaLand_BossInfernus_Swipe>(info.caster.agentPosition, null, new CastInfo(info.caster));
							DestroyIfActive();
						},
						uncancellableTime = 0.5f
					});
				}
				else
				{
					info.caster.Control.StartDaze(postDelay);
					DestroyIfActive();
				}
			}
		});
	}

	private void MirrorProcessed()
	{
	}
}
