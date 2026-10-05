using Mirror;
using UnityEngine;

public class Ai_Mon_SnowMountain_BossSkoll_Atk : AbilityInstance
{
	public DewAnimationClip animDash;

	public GameObject fxDash;

	public float dashMinDistance;

	public float dashLengthOffset;

	public DewEase dashEase;

	public float dashSpeed;

	public DewAnimationClip animAtkPrepare;

	public GameObject fxAtkPrepareAttached;

	public GameObject fxAtkPrepareTelegraph;

	public float atkPrepareDuration;

	public DewAnimationClip animAtkCast;

	public float swipeChance = 0.25f;

	public float postAttackBeforeSwipeDelay = 0.35f;

	public float postAttackDelay = 1f;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DestroyOnDeath(info.caster);
			FxPlayNetworked(fxDash, info.caster);
			Vector3 vector = info.target.GetAIAgentPosition(info.caster) - info.caster.agentPosition;
			vector = vector.normalized * (vector.magnitude + dashLengthOffset);
			if (vector.magnitude < dashMinDistance)
			{
				vector = vector.normalized * dashMinDistance;
			}
			Vector3 validAgentDestination_LinearSweep = Dew.GetValidAgentDestination_LinearSweep(info.caster.agentPosition, info.caster.agentPosition + vector);
			float duration = (info.caster.agentPosition - validAgentDestination_LinearSweep).magnitude / dashSpeed / info.caster.Status.attackSpeedMultiplier;
			info.caster.Animation.PlayAbilityAnimation(animDash);
			info.caster.Control.StartDisplacement(new DispByDestination
			{
				duration = duration,
				destination = validAgentDestination_LinearSweep,
				ease = dashEase,
				affectedByMovementSpeed = true,
				rotateSmoothly = false,
				isFriendly = true,
				canGoOverTerrain = true,
				rotateForward = true,
				isCanceledByCC = false,
				onCancel = () =>
				{
					FxStopNetworked(fxDash);
					DestroyIfActive();
				},
				onFinish = () =>
				{
					FxStopNetworked(fxDash);
					ChannelAttack();
				}
			});
		}
	}

	private void ChannelAttack()
	{
		float speed = info.caster.Status.attackSpeedMultiplier;
		Vector3 pos = info.caster.position;
		Quaternion rot = Quaternion.Euler(0f, AbilityTrigger.PredictAngle_Simple(info.caster, NetworkedManagerBase<GameManager>.instance.GetPredictionStrength(), info.target, pos, atkPrepareDuration / speed), 0f);
		info.caster.Control.Rotate(rot, immediately: true);
		FxPlayNetworked(fxAtkPrepareAttached, info.caster);
		FxPlayNetworked(fxAtkPrepareTelegraph, pos, rot);
		info.caster.Animation.PlayAbilityAnimation(animAtkPrepare, speed);
		info.caster.Control.StartChannel(new Channel
		{
			duration = atkPrepareDuration / speed,
			blockedActions = Channel.BlockedAction.Everything,
			onCancel = () =>
			{
				FxStopNetworked(fxAtkPrepareAttached);
				FxStopNetworked(fxAtkPrepareTelegraph);
				DestroyIfActive();
				firstTrigger.SetCooldownTime(0, 1f);
			},
			onComplete = () =>
			{
				FxStopNetworked(fxAtkPrepareAttached);
				FxStopNetworked(fxAtkPrepareTelegraph);
				info.caster.Animation.PlayAbilityAnimation(animAtkCast, speed);
				CreateAbilityInstance<Ai_Mon_SnowMountain_BossSkoll_Atk_Damage>(pos, rot, new CastInfo(info.caster));
				if (Random.value < swipeChance * (0.5f + NetworkedManagerBase<GameManager>.instance.difficulty.specialSkillChanceMultiplier * 0.5f))
				{
					info.caster.Control.StartDaze(postAttackBeforeSwipeDelay);
					((Mon_SnowMountain_BossSkoll)info.caster).AllowSwipe();
				}
				else
				{
					info.caster.Control.StartDaze(postAttackDelay / info.caster.Status.attackSpeedMultiplier);
				}
				DestroyIfActive();
			}
		}.AddValidation(new AbilitySelfValidator()));
	}

	private void MirrorProcessed()
	{
	}
}
