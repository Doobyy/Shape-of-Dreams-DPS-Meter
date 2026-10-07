using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_Mon_Ink_BossWhiteNight_AltAtk_Ready : AbilityInstance
{
	public float randomPosMagnitude;

	public float atkDelay;

	public float postDelay;

	public DewEase dashEase;

	public GameObject fxDash;

	public GameObject fxCast;

	public DewAnimationClip atkClip;

	public float spawnRadius;

	public float targetPointMagnitude;

	public float spawnedAtkDelay;

	public float disappearDelayOnComplete;

	public float interval;

	public GameObject fxSpawn;

	public GameObject fxSpawnedAtk;

	public GameObject fxDisappear;

	public GameObject fxDisableRenderer;

	[NonSerialized]
	public bool isHallucination;

	private bool _isRage;

	private Channel _channel;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		DestroyOnDeath(info.caster);
		if (isHallucination)
		{
			HallucinationRoutine();
			return;
		}
		_channel = info.caster.Control.StartChannel(new Channel
		{
			blockedActions = Channel.BlockedAction.Everything,
			isAttack = true,
			duration = float.PositiveInfinity
		}).AddValidation(new AbilitySelfValidator());
		Mon_Ink_BossWhiteNight mon_Ink_BossWhiteNight = (Mon_Ink_BossWhiteNight)info.caster;
		List<Mon_Ink_BossWhiteNightHallucination> hallucinations = mon_Ink_BossWhiteNight._Hallucinations;
		_isRage = mon_Ink_BossWhiteNight._isRage;
		Vector3 dest = info.caster.agentPosition + UnityEngine.Random.insideUnitCircle.ToXZ().normalized * randomPosMagnitude;
		dest = Dew.GetValidAgentDestination_Closest(info.caster.agentPosition, dest);
		float speed = info.caster.Status.attackSpeedMultiplier;
		float num = Mathf.Max(atkDelay / speed, 0.4f);
		DewEffect.ApplySpeedMultiplierNetworked(((NetworkBehaviour)this).netIdentity, fxCast, 1f / num);
		FxPlayNetworked(fxDash, info.caster);
		FxPlayNetworked(fxCast, info.caster);
		Hero target = Dew.GetClosestAliveHero(info.caster.agentPosition, fallbackToDead: true, info.caster);
		Vector3 targetPoint = AbilityTrigger.PredictPoint_Simple(info.caster, NetworkedManagerBase<GameManager>.instance.GetPredictionStrength(), target, atkDelay / speed);
		if (_isRage && hallucinations.Count > 0)
		{
			((MonoBehaviour)(object)this).StartCoroutine(Routine());
		}
		info.caster.Control.RotateTowards(targetPoint, immediately: false, 1f);
		info.caster.Control.StartDisplacement(new DispByDestination
		{
			affectedByMovementSpeed = false,
			canGoOverTerrain = true,
			destination = dest,
			isCanceledByCC = true,
			isFriendly = true,
			duration = num,
			ease = dashEase,
			rotateForward = false,
			onCancel = () =>
			{
				FxStopNetworked(fxDash);
				FxStopNetworked(fxCast);
				firstTrigger.SetCooldownTime(0, 1f);
				DestroyIfActive();
			},
			onFinish = () =>
			{
				FxStopNetworked(fxDash);
				FxStopNetworked(fxCast);
				Quaternion value = Quaternion.LookRotation((targetPoint - dest).normalized);
				info.caster.Animation.PlayAbilityAnimation(atkClip, speed);
				CreateAbilityInstance<Ai_Mon_Ink_BossWhiteNight_AltAtk>(info.caster.agentPosition, value, new CastInfo(info.caster, CastInfo.GetAngle(value)));
				info.caster.Control.StartDaze(postDelay);
				DestroyIfActive();
			}
		});
		void HallucinationRoutine()
		{
			FxStopNetworked(fxDisappear);
			FxPlayNetworked(fxSpawn, info.caster);
			info.caster.Visual.EnableRenderers();
			info.caster.Control.StartChannel(new Channel
			{
				blockedActions = Channel.BlockedAction.Everything,
				duration = spawnedAtkDelay,
				onCancel = () =>
				{
					FxPlayNetworked(fxDisappear, info.caster);
					info.caster.Visual.DisableRenderers();
				},
				onComplete = () =>
				{
					((MonoBehaviour)(object)this).StartCoroutine(SpawnedRoutine());
				}
			});
		}
		IEnumerator Routine()
		{
			foreach (Mon_Ink_BossWhiteNightHallucination item in hallucinations)
			{
				Vector3 aIAgentPosition = target.GetAIAgentPosition(info.caster);
				Vector3 end = aIAgentPosition + UnityEngine.Random.insideUnitCircle.ToXZ() * spawnRadius;
				end = Dew.GetValidAgentDestination_Closest(aIAgentPosition, end);
				Quaternion value = Quaternion.LookRotation((targetPoint + UnityEngine.Random.insideUnitCircle.ToXZ() * targetPointMagnitude - end).normalized);
				if (!item.IsNullOrInactive())
				{
					Teleport(item, end);
					item.Control.Rotate(value, immediately: true);
					CreateAbilityInstance(item.agentPosition, value, new CastInfo(item, CastInfo.GetAngle(value)), (Ai_Mon_Ink_BossWhiteNight_AltAtk_Ready b) =>
					{
						b.isHallucination = true;
					});
					yield return new WaitForSeconds(interval);
				}
			}
		}
		IEnumerator SpawnedRoutine()
		{
			FxPlayNetworked(fxSpawnedAtk, info.caster);
			FxPlayNetworked(fxDisappear, info.caster);
			CreateAbilityInstance<Ai_Mon_Ink_BossWhiteNight_AltAtk>(info.caster.agentPosition, info.caster.rotation, new CastInfo(info.caster, CastInfo.GetAngle(info.forward)));
			yield return new WaitForSeconds(disappearDelayOnComplete);
			info.caster.Visual.DisableRenderers();
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		isHallucination = false;
		_isRage = false;
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			FxStopNetworked(fxDash);
			FxStopNetworked(fxCast);
			FxStopNetworked(fxSpawn);
			FxStopNetworked(fxDisappear);
			if (_channel != null)
			{
				_channel.Cancel();
				_channel = null;
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
