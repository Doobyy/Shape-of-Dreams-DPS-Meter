using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Se_D_ChargedAnguillian : StackedStatusEffect
{
	public ScalingValue gainedAp;

	public float apAmpPerStack = 0.1f;

	public int lightningMaxTargets = 3;

	public float lightningRadius = 3.5f;

	public GameObject fxLightningActive;

	public GameObject fxLightningShot;

	private float _lastLightningAttackTime;

	private StatBonus _bonus;

	private Action<EventInfoDamage> _cachedActorEventOnDealDamage;

	private Action<EventInfoDamage> _cachedEntityEventOnTakeDamage;

	public override bool reuseInRoom => true;

	public float lightningInterval => Mathf.Max(0.5f, 1.5f - 0.1f * (float)(skillLevel - 1));

	protected override void OnCreate()
	{
		base.OnCreate();
		_lastLightningAttackTime = 0f;
		if (((NetworkBehaviour)this).isServer)
		{
			victim.Visual.genericStackIndicatorValue = stack;
			victim.Visual.genericStackIndicatorMax = maxStack;
			victim.ActorEvent_OnDealDamage += new Action<EventInfoDamage>(ActorEventOnDealDamage);
			victim.EntityEvent_OnTakeDamage += new Action<EventInfoDamage>(EntityEventOnTakeDamage);
			_bonus = DoStatBonus();
			UpdateStatBonus();
		}
	}

	private void EntityEventOnTakeDamage(EventInfoDamage obj)
	{
		AddStack();
	}

	private void ActorEventOnDealDamage(EventInfoDamage obj)
	{
		if (!((UnityEngine.Object)(object)obj.victim == (UnityEngine.Object)(object)victim) && obj.damage.elemental == ElementalType.Cold)
		{
			AddStack();
		}
	}

	private void UpdateStatBonus()
	{
		_bonus.abilityPowerFlat = GetValue(gainedAp) * (1f + apAmpPerStack * (float)stack);
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (bool)(UnityEngine.Object)(object)victim)
		{
			victim.Visual.genericStackIndicatorValue = 0;
			victim.Visual.genericStackIndicatorMax = 0;
			victim.ActorEvent_OnDealDamage -= new Action<EventInfoDamage>(ActorEventOnDealDamage);
			victim.EntityEvent_OnTakeDamage -= new Action<EventInfoDamage>(EntityEventOnTakeDamage);
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		if (parentActor is SkillTrigger skillTrigger)
		{
			if (stack == 0)
			{
				skillTrigger.fillAmount = 0f;
			}
			else
			{
				skillTrigger.fillAmount = remainingDecayTime / decayTime;
			}
		}
		if (stack >= maxStack && !(Time.time - _lastLightningAttackTime < lightningInterval))
		{
			_lastLightningAttackTime = Time.time;
			FxPlayNetworked(fxLightningActive, victim);
			List<Entity> list = DewPhysics.OverlapCircleAllEntities(out var handle, victim.agentPosition, lightningRadius, tvDefaultHarmfulEffectTargets, new CollisionCheckSettings
			{
				sortComparer = CollisionCheckSettings.Random
			});
			int num = Mathf.Min(list.Count, lightningMaxTargets);
			for (int i = 0; i < num; i++)
			{
				CreateAbilityInstance<Ai_D_ChargedAnguillian_Lightning>(victim.agentPosition, null, new CastInfo(victim, list[i]));
			}
			if (num > 0)
			{
				FxPlayNetworked(fxLightningShot, victim);
			}
			handle.Return();
		}
	}

	protected override void OnStackChange(int oldStack, int newStack)
	{
		base.OnStackChange(oldStack, newStack);
		if (((NetworkBehaviour)this).isServer)
		{
			victim.Visual.genericStackIndicatorValue = newStack;
			UpdateStatBonus();
		}
	}

	private void MirrorProcessed()
	{
	}
}
