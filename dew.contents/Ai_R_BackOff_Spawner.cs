using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_R_BackOff_Spawner : AbilityInstance
{
	public ChargingChannelData channel;

	public float fullChargeRadiusMult;

	public float fullChargeAngleMult;

	public float fullGraceThreshold;

	public float buffRadius = 4f;

	private ChargingChannel _channel;

	private float _initRadius;

	private float _initAngle;

	[NonSerialized]
	public bool disableArmorAndShieldToSelf;

	[NonSerialized]
	private float _baseChargeFullDuration;

	public override bool reuseInRoom => true;

	protected override void Awake()
	{
		base.Awake();
		_baseChargeFullDuration = channel.chargeFullDuration;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		channel.chargeFullDuration = _baseChargeFullDuration;
		disableArmorAndShieldToSelf = false;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		List<Entity> list = DewPhysics.OverlapCircleAllEntities(out var handle, info.caster.agentPosition, buffRadius, tvDefaultUsefulEffectTargets);
		AbilityTrigger ft = firstTrigger;
		foreach (Entity item in list)
		{
			if (!disableArmorAndShieldToSelf || !((UnityEngine.Object)(object)item == (UnityEngine.Object)(object)info.caster))
			{
				Se_R_BackOff_Shielded se_R_BackOff_Shielded = item.Status.FindStatusEffect((Se_R_BackOff_Shielded se) => (UnityEngine.Object)(object)se.firstTrigger == (UnityEngine.Object)(object)ft);
				if (!se_R_BackOff_Shielded.IsNullOrInactive())
				{
					se_R_BackOff_Shielded.Destroy();
				}
				CreateStatusEffect<Se_R_BackOff_Shielded>(item);
			}
		}
		handle.Return();
		CreateBasicEffect(info.caster, new UnstoppableEffect(), 3600f).DestroyOnDestroy(this);
		channel.chargeFullDuration /= Mathf.Max(1f, info.caster.Status.attackSpeedMultiplier);
		_channel = channel.Get(this).SetInitialInfo(info).OnCast((ChargingChannel _) =>
		{
			Complete();
		})
			.OnCancel((ChargingChannel _) =>
			{
				Complete();
			})
			.OnComplete((ChargingChannel _) =>
			{
				Complete();
			})
			.Dispatch(info.caster, firstTrigger);
		_initRadius = channel.castMethod._radius;
		_initAngle = channel.castMethod._angle;
		ResetCooldown(info.caster.Ability.attackAbility);
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer)
		{
			_channel.castMethod._radius = _initRadius * Mathf.Lerp(1f, fullChargeRadiusMult, _channel.chargeAmount);
			_channel.castMethod._angle = _initAngle * Mathf.Lerp(1f, fullChargeAngleMult, _channel.chargeAmount);
			_channel.UpdateCastMethod();
		}
	}

	private void Complete()
	{
		CreateAbilityInstance(info.caster.position, _channel.castInfo.rotation, new CastInfo(info.caster, _channel.castInfo.angle), (Ai_R_BackOff_Damage p) =>
		{
			float num = _channel.chargeAmount;
			if (num > fullGraceThreshold)
			{
				num = 1f;
			}
			p.Networkradius = _initRadius * Mathf.Lerp(1f, fullChargeRadiusMult, num);
			p.Networkangle = _initAngle * Mathf.Lerp(1f, fullChargeAngleMult, num);
			p.NetworkchargeAmount = num;
		});
		info.caster.Control.Rotate(_channel.castInfo.rotation, immediately: true, 1f);
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
