using System;
using System.Collections;
using Mirror;
using UnityEngine;

public class Ai_Mon_Ink_BossDarkMoon_Blade : AbilityInstance
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

	public GameObject fxStart;

	public float atkPrepareDuration;

	public DewAnimationClip animAtkCast;

	public float postAttackDelay = 1f;

	[Space(15f)]
	public DewAnimationClip animWhiteNightSpawn;

	public GameObject fxWhiteNightSpawn;

	[Space(15f)]
	public float rageAtkDuration;

	[NonSerialized]
	public bool isWhiteNightSpawn;

	[NonSerialized]
	public float whiteNightSpawnDelay;

	[NonSerialized]
	public Vector3 forcedDest;

	private bool _isRage;

	private bool _isRendererOff;

	private float _basePostAttackDelay;

	private float _baseAtkPrepareDuration;

	protected override void Awake()
	{
		base.Awake();
		_basePostAttackDelay = postAttackDelay;
		_baseAtkPrepareDuration = atkPrepareDuration;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		postAttackDelay = _basePostAttackDelay;
		atkPrepareDuration = _baseAtkPrepareDuration;
	}

	protected override IEnumerator OnCreateSequenced()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			DestroyOnDeath(info.caster);
			if (isWhiteNightSpawn)
			{
				FxPlayNetworked(fxWhiteNightSpawn, info.caster);
				info.caster.Animation.PlayAbilityAnimation(animWhiteNightSpawn);
				info.caster.Control.StartDaze(whiteNightSpawnDelay);
				yield return new SI.WaitForSeconds(whiteNightSpawnDelay);
			}
			FxPlayNetworked(fxStart, info.caster);
			Mon_Ink_BossDarkMoon mon_Ink_BossDarkMoon = (Mon_Ink_BossDarkMoon)info.caster;
			_isRage = mon_Ink_BossDarkMoon._isRage;
			if (_isRage)
			{
				atkPrepareDuration = rageAtkDuration;
			}
			FxApplySpeedMultiplierNetworked(fxAtkPrepareTelegraph, 1f / atkPrepareDuration);
			FxPlayNetworked(fxDash, info.caster);
			if (!mon_Ink_BossDarkMoon._isSolo)
			{
				postAttackDelay *= 1.5f;
			}
			Vector3 vector = info.target.GetAIAgentPosition(info.caster) - info.caster.agentPosition;
			vector = vector.normalized * (vector.magnitude + dashLengthOffset);
			if (vector.magnitude < dashMinDistance)
			{
				vector = vector.normalized * dashMinDistance;
			}
			Vector3 validAgentDestination_Closest = Dew.GetValidAgentDestination_Closest(info.caster.agentPosition, info.caster.agentPosition + vector);
			if (isWhiteNightSpawn)
			{
				validAgentDestination_Closest = forcedDest;
			}
			float duration = (info.caster.agentPosition - validAgentDestination_Closest).magnitude / dashSpeed;
			info.caster.Visual.DisableRenderers();
			_isRendererOff = true;
			info.caster.Animation.PlayAbilityAnimation(animDash);
			info.caster.Control.StartDisplacement(new DispByDestination
			{
				duration = duration,
				destination = validAgentDestination_Closest,
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
		info.caster.Visual.EnableRenderers();
		_isRendererOff = false;
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
				if ((UnityEngine.Object)(object)firstTrigger != null)
				{
					firstTrigger.SetCooldownTime(0, 1f);
				}
				DestroyIfActive();
			},
			onComplete = () =>
			{
				FxStopNetworked(fxAtkPrepareAttached);
				FxStopNetworked(fxAtkPrepareTelegraph);
				info.caster.Animation.PlayAbilityAnimation(animAtkCast, speed);
				CreateAbilityInstance(pos, rot, new CastInfo(info.caster, info.target), (Ai_Mon_Ink_BossDarkMoon_Blade_Instance b) =>
				{
					b._isRage = _isRage;
				});
				info.caster.Control.StartDaze(postAttackDelay);
				DestroyIfActive();
			}
		}.AddValidation(new AbilitySelfValidator()));
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && !((UnityEngine.Object)(object)info.caster == null) && _isRendererOff)
		{
			info.caster.Visual.EnableRenderers();
		}
	}

	private void MirrorProcessed()
	{
	}
}
