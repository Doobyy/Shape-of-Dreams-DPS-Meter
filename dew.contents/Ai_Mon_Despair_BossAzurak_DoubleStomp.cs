using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_Mon_Despair_BossAzurak_DoubleStomp : AbilityInstance
{
	public int waveCount;

	public float waveInterval;

	public float doubleAtkInterval;

	public float finalAtkDelay;

	public float doubleAtkRandomDeviation;

	public float startDelay;

	public float postDelay;

	public GameObject fxDoubleAtk;

	public GameObject fxFinalAtk;

	public GameObject fxCast;

	public GameObject fxFinalCast;

	public DewAnimationClip castClip;

	public DewAnimationClip firstAtkClip;

	public DewAnimationClip finalCastClip;

	public DewAnimationClip finalAtkClip;

	private Channel _channel;

	private List<Ai_Mon_Despair_BossAzurak_DoubleStomp_Instance> _instances;

	private float _baseRotSmoothTime;

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		DestroyOnDeath(info.caster);
		CreateBasicEffect(info.caster, new UnstoppableEffect(), float.PositiveInfinity).DestroyOnDestroy(this);
		_instances = new List<Ai_Mon_Despair_BossAzurak_DoubleStomp_Instance>();
		_channel = info.caster.Control.StartChannel(new Channel
		{
			blockedActions = Channel.BlockedAction.Everything,
			duration = float.PositiveInfinity,
			onCancel = DestroyIfActive
		});
		_baseRotSmoothTime = info.caster.Control.rotationSmoothTime;
		info.caster.Control.rotationSmoothTime = 1f;
		FxPlayNetworked(fxCast, info.caster);
		yield return new SI.WaitForSeconds(startDelay);
		for (int i = 0; i < waveCount; i++)
		{
			FxStopNetworked(fxCast);
			info.caster.Animation.PlayAbilityAnimation(firstAtkClip);
			for (int j = 0; j < 2; j++)
			{
				FxPlayNewNetworked(fxDoubleAtk, info.caster.agentPosition + ((Component)(object)info.caster).transform.forward * 5f, null);
				foreach (Hero allHero in NetworkedManagerBase<ActorManager>.instance.allHeroes)
				{
					if (!allHero.IsNullInactiveDeadOrKnockedOut())
					{
						bool flag = Random.value < 0.7f;
						_ = info.caster.agentPosition;
						Vector3 vector;
						if (flag)
						{
							vector = AbilityTrigger.PredictPoint_Simple(info.caster, NetworkedManagerBase<GameManager>.instance.GetPredictionStrength(), allHero, 1f);
							vector += Random.insideUnitCircle.ToXZ() * doubleAtkRandomDeviation;
							vector = Dew.GetPositionOnGround(vector);
						}
						else
						{
							vector = allHero.GetAIAgentPosition(info.caster) + Random.insideUnitCircle.ToXZ() * doubleAtkRandomDeviation;
							vector = Dew.GetPositionOnGround(vector);
						}
						Ai_Mon_Despair_BossAzurak_DoubleStomp_Instance item = CreateAbilityInstance<Ai_Mon_Despair_BossAzurak_DoubleStomp_Instance>(vector + Vector3.up * 0.05f, null, new CastInfo(info.caster, vector));
						_instances.Add(item);
					}
				}
				yield return new SI.WaitForSeconds(doubleAtkInterval);
				info.caster.Control.RotateTowards(Dew.GetClosestAliveHero(info.caster.agentPosition, fallbackToDead: true, info.caster), immediately: false, waveInterval);
			}
			if (i < waveCount - 1)
			{
				FxPlayNetworked(fxCast, info.caster);
				info.caster.Animation.PlayAbilityAnimation(castClip);
			}
			yield return new SI.WaitForSeconds(waveInterval);
		}
		info.caster.Control.rotationSmoothTime = _baseRotSmoothTime;
		info.caster.Animation.PlayAbilityAnimation(finalCastClip);
		FxPlayNetworked(fxFinalCast, info.caster);
		yield return new SI.WaitForSeconds(finalAtkDelay);
		info.caster.Animation.PlayAbilityAnimation(finalAtkClip);
		FxStopNetworked(fxFinalCast);
		FxPlayNetworked(fxFinalAtk, info.caster);
		foreach (Ai_Mon_Despair_BossAzurak_DoubleStomp_Instance instance in _instances)
		{
			instance.DealDamageRoutine();
			yield return new SI.WaitForSeconds(0.01f);
		}
		yield return new SI.WaitForSeconds(postDelay);
		Destroy();
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		if (_channel != null)
		{
			_channel.Cancel();
			_channel = null;
		}
		info.caster.Control.rotationSmoothTime = _baseRotSmoothTime;
		FxStopNetworked(fxFinalCast);
		FxStopNetworked(fxCast);
		if (_instances == null || _instances.Count <= 0)
		{
			return;
		}
		foreach (Ai_Mon_Despair_BossAzurak_DoubleStomp_Instance instance in _instances)
		{
			instance.DestroyIfActive();
		}
		_instances.Clear();
	}

	private void MirrorProcessed()
	{
	}
}
