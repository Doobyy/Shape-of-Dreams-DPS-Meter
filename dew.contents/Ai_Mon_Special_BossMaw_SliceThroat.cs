using Mirror;
using UnityEngine;

public class Ai_Mon_Special_BossMaw_SliceThroat : AbilityInstance
{
	public float dashMinDistance;

	public float dashLengthOffset;

	public float dashSpeed;

	public DewEase dashEase;

	public GameObject fxDash;

	public float atkDelay;

	public float postDelay;

	public DewAnimationClip atkPrepareAnim;

	public DewAnimationClip atkAnim;

	public GameObject fxAtkPrepare;

	public GameObject fxAtkTelegraph;

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
			float duration = (info.caster.agentPosition - validAgentDestination_LinearSweep).magnitude / dashSpeed;
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
		Vector3 pos = info.caster.position;
		Quaternion rot = Quaternion.Euler(0f, AbilityTrigger.PredictAngle_Simple(info.caster, NetworkedManagerBase<GameManager>.instance.GetPredictionStrength(), info.target, pos, atkDelay), 0f);
		info.caster.Control.Rotate(rot, immediately: true);
		FxPlayNetworked(fxAtkPrepare, info.caster);
		FxPlayNetworked(fxAtkTelegraph, pos, rot);
		info.caster.Animation.PlayAbilityAnimation(atkPrepareAnim);
		info.caster.Control.StartChannel(new Channel
		{
			duration = atkDelay,
			blockedActions = Channel.BlockedAction.Everything,
			onCancel = () =>
			{
				FxStopNetworked(fxAtkPrepare);
				FxStopNetworked(fxAtkTelegraph);
				DestroyIfActive();
			},
			onComplete = () =>
			{
				FxStopNetworked(fxAtkPrepare);
				FxStopNetworked(fxAtkTelegraph);
				info.caster.Animation.PlayAbilityAnimation(atkAnim);
				CreateAbilityInstance<Ai_Mon_Special_BossMaw_SliceThroat_Atk>(pos, rot, new CastInfo(info.caster));
				info.caster.Control.StartDaze(postDelay);
				DestroyIfActive();
			}
		}).AddValidation(new AbilitySelfValidator());
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			FxStopNetworked(fxAtkPrepare);
			FxStopNetworked(fxAtkTelegraph);
			FxStopNetworked(fxDash);
		}
	}

	private void MirrorProcessed()
	{
	}
}
