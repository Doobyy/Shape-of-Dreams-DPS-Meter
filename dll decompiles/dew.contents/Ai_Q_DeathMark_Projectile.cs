using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_Q_DeathMark_Projectile : StandardProjectile
{
	public ScalingValue damage;

	public float damageAmpGainOnFirstHit;

	public GameObject fxAmpGain;

	public GameObject fxAmpLoss;

	public float firstHitCooldownReductionRatio = 0.4f;

	[NonSerialized]
	public float lostAmpRatioOnCompleteMiss;

	private List<Entity> _hitEntities = new List<Entity>();

	private bool _didAnyHit;

	private bool _didGiveAmp;

	private float _initialDamageAmpGainOnFirstHit;

	public float firstHitCooldownReduction => firstHitCooldownReductionRatio;

	public List<Entity> HitEntities => _hitEntities;

	public override bool reuseInRoom => true;

	protected override void Awake()
	{
		base.Awake();
		_initialDamageAmpGainOnFirstHit = damageAmpGainOnFirstHit;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		_hitEntities.Clear();
		_didAnyHit = false;
		_didGiveAmp = false;
		damageAmpGainOnFirstHit = _initialDamageAmpGainOnFirstHit;
		lostAmpRatioOnCompleteMiss = 0f;
	}

	protected override void OnEntity(EntityHit hit)
	{
		base.OnEntity(hit);
		St_Q_DeathMark st_Q_DeathMark = firstTrigger as St_Q_DeathMark;
		Damage(damage).SetDirection(info.forward).SetElemental(ElementalType.Dark).ApplyAmplification(((UnityEngine.Object)(object)st_Q_DeathMark) ? st_Q_DeathMark.currentDamageAmp : 0f)
			.DoAttackEffect(AttackEffectType.Others)
			.Dispatch(hit.entity);
		if (!_didAnyHit)
		{
			_didAnyHit = true;
			if ((bool)(UnityEngine.Object)(object)st_Q_DeathMark)
			{
				ApplyCooldownReductionByRatio(firstTrigger, firstHitCooldownReduction);
			}
		}
		if ((bool)(UnityEngine.Object)(object)st_Q_DeathMark && !_didGiveAmp && !hit.entity.Status.hasDamageImmunity && info.caster is Hero { isInCombat: not false })
		{
			_didGiveAmp = true;
			st_Q_DeathMark.NetworkcurrentDamageAmp = st_Q_DeathMark.currentDamageAmp + damageAmpGainOnFirstHit;
			FxPlayNetworked(fxAmpGain, info.caster);
		}
		_hitEntities.Add(hit.entity);
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		if (!_didAnyHit && firstTrigger is St_Q_DeathMark st_Q_DeathMark && lostAmpRatioOnCompleteMiss > 0.0001f)
		{
			st_Q_DeathMark.NetworkcurrentDamageAmp = st_Q_DeathMark.currentDamageAmp * (1f - lostAmpRatioOnCompleteMiss);
			FxPlayNetworked(fxAmpLoss, info.caster);
		}
		for (int num = _hitEntities.Count - 1; num >= 0; num--)
		{
			if (!_hitEntities[num].IsNullInactiveDeadOrKnockedOut())
			{
				CreateStatusEffect<Se_Q_DeathMark_Marked>(_hitEntities[num], new CastInfo(info.caster, _hitEntities[num]));
				break;
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
