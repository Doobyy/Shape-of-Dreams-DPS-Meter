using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Se_R_DangerousTheory : StatusEffect
{
	public GameObject fxActivateStrong;

	public float duration = 4f;

	public Knockback knockback;

	public ScalingValue damageAmount;

	public ScalingValue healAmount;

	public float bossAmp = 1f;

	public float lowAmp = 0.5f;

	public GameObject fxHit;

	public GameObject fxHeal;

	public float cooldownReductionRatio = 0.2f;

	[NonSerialized]
	public int empowerCount = 1;

	private bool _isEmpowered;

	private ScalingValue _baseHealAmount;

	private readonly List<(AbilityInstance instance, Action<EventInfoAttackEffect> handler)> _attackEffectSubscriptions = new List<(AbilityInstance, Action<EventInfoAttackEffect>)>();

	private Action<EventInfoAttackFired> _cachedOnAttackFiredBeforePrepare;

	public override bool reuseInRoom => true;

	protected override void Awake()
	{
		base.Awake();
		_baseHealAmount = healAmount;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		healAmount = _baseHealAmount;
		empowerCount = 1;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			float num = 0.5f;
			if (firstTrigger is St_R_DangerousTheory st_R_DangerousTheory)
			{
				num = st_R_DangerousTheory.healHpThreshold;
			}
			CreateAbilityInstance(position, null, new CastInfo(info.caster, info.point), (Ai_M_FeatheryDash a) =>
			{
				a.speed *= 1.1f;
			});
			_isEmpowered = victim.currentHealth / victim.maxHealth - 0.02f < num;
			if (_isEmpowered && (UnityEngine.Object)(object)firstTrigger != null)
			{
				ApplyCooldownReductionByRatio(firstTrigger, cooldownReductionRatio);
			}
			if (_isEmpowered)
			{
				FxPlayNetworked(fxActivateStrong, victim);
			}
			ResetCooldown(victim.Ability.attackAbility);
			DoAttackCritical(null);
			victim.EntityEvent_OnAttackFiredBeforePrepare += new Action<EventInfoAttackFired>(EntityEventOnAttackFiredBeforePrepare);
			SetTimer(duration);
			ShowOnScreenTimer();
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		if ((UnityEngine.Object)(object)victim != null)
		{
			victim.EntityEvent_OnAttackFiredBeforePrepare -= new Action<EventInfoAttackFired>(EntityEventOnAttackFiredBeforePrepare);
		}
		foreach (var attackEffectSubscription in _attackEffectSubscriptions)
		{
			if ((UnityEngine.Object)(object)attackEffectSubscription.instance != null)
			{
				attackEffectSubscription.instance.ActorEvent_OnAttackEffectTriggered -= attackEffectSubscription.handler;
			}
		}
		_attackEffectSubscriptions.Clear();
	}

	private void EntityEventOnAttackFiredBeforePrepare(EventInfoAttackFired obj)
	{
		if (!isActive)
		{
			return;
		}
		Action<EventInfoAttackEffect> action = (EventInfoAttackEffect effect) =>
		{
			if (effect.type != AttackEffectType.Others)
			{
				knockback.ApplyWithOrigin(info.caster.position, effect.victim);
				DamageData damageData = Damage(damageAmount).ApplyStrength(effect.strength);
				HealData healData = Heal(GetValue(healAmount) * effect.strength);
				if (effect.victim.IsAnyBoss())
				{
					damageData.ApplyRawMultiplier(1f + bossAmp);
					damageData.SetAttr(DamageAttribute.IsCrit);
					healData.ApplyAmplification(bossAmp);
					healData.SetCrit();
					healData.SetCanMerge();
				}
				if (_isEmpowered)
				{
					damageData.ApplyRawMultiplier(1f + lowAmp);
					damageData.SetAttr(DamageAttribute.IsCrit);
				}
				damageData.Dispatch(effect.victim);
				if (_isEmpowered && !effect.victim.Status.hasDamageImmunity)
				{
					FxPlayNewNetworked(fxHeal, victim);
					healData.Dispatch(victim);
				}
				FxPlayNewNetworked(fxHit, effect.victim);
			}
		};
		obj.instance.ActorEvent_OnAttackEffectTriggered += action;
		_attackEffectSubscriptions.Add((obj.instance, action));
		empowerCount--;
		if (empowerCount <= 0)
		{
			DestroyIfActive();
		}
		else
		{
			ResetTimer();
		}
	}

	private void MirrorProcessed()
	{
	}
}
