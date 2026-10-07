using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Se_D_AstridsMasterpiecePriorite : StatusEffect
{
	public ScalingValue damage;

	public ScalingValue explosionDmg;

	public ScalingValue speedAmount;

	public float speedDuration = 1.5f;

	public float radius = 9f;

	public float dmgAmpMultiplier = 0.5f;

	public GameObject fxHit;

	public GameObject fxHitAudio;

	public GameObject fxExplosion;

	public GameObject fxActivateOnSelf;

	public bool setCritMarker;

	private ActorRef<Se_D_AstridsMasterpiecePriorite_Exposed> _currentExposed;

	private AbilityTrigger _trigger;

	private bool _isReset;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			victim.EntityEvent_OnAttackEffectTriggered += new Action<EventInfoAttackEffect>(CheckExposed);
			_trigger = firstTrigger;
		}
	}

	private void CheckExposed(EventInfoAttackEffect obj)
	{
		Se_D_AstridsMasterpiecePriorite_Exposed se_D_AstridsMasterpiecePriorite_Exposed = _currentExposed.Get();
		if ((UnityEngine.Object)(object)se_D_AstridsMasterpiecePriorite_Exposed == null || (UnityEngine.Object)(object)se_D_AstridsMasterpiecePriorite_Exposed.victim != (UnityEngine.Object)(object)obj.victim || !obj.victim.Status.TryGetStatusEffect<Se_D_AstridsMasterpiecePriorite_Exposed>(out var _) || se_D_AstridsMasterpiecePriorite_Exposed.chargeCount >= se_D_AstridsMasterpiecePriorite_Exposed.maxCharge - 1)
		{
			return;
		}
		se_D_AstridsMasterpiecePriorite_Exposed.chargeCount++;
		DamageData damageData = Damage(damage);
		if (se_D_AstridsMasterpiecePriorite_Exposed.chargeCount < se_D_AstridsMasterpiecePriorite_Exposed.maxCharge - 1)
		{
			FxPlayNetworked(fxHit, obj.victim);
			DewAudioSource component = fxHitAudio.GetComponent<DewAudioSource>();
			component.volumeMultiplier = 1f + 0.15f * (float)se_D_AstridsMasterpiecePriorite_Exposed.chargeCount;
			component.pitchMultiplier = 1f + -0.1f * (float)se_D_AstridsMasterpiecePriorite_Exposed.chargeCount;
			FxPlayNetworked(fxHitAudio, obj.victim);
			damageData.ApplyAmplification(dmgAmpMultiplier * (float)se_D_AstridsMasterpiecePriorite_Exposed.chargeCount);
		}
		else
		{
			se_D_AstridsMasterpiecePriorite_Exposed.Destroy();
			FxPlayNetworked(fxExplosion, obj.victim);
			DewAudioSource component2 = fxHitAudio.GetComponent<DewAudioSource>();
			component2.volumeMultiplier = 1f + 0.15f * (float)se_D_AstridsMasterpiecePriorite_Exposed.chargeCount;
			component2.pitchMultiplier = 1f + -0.1f * (float)se_D_AstridsMasterpiecePriorite_Exposed.chargeCount;
			FxPlayNetworked(fxHitAudio, obj.victim);
			damageData = Damage(explosionDmg);
			if (setCritMarker)
			{
				damageData.SetAttr(DamageAttribute.IsCrit);
			}
		}
		damageData.ApplyStrength(obj.strength).SetOriginPosition(victim.position).Dispatch(obj.victim);
		CreateBasicEffect(victim, new SpeedEffect
		{
			strength = GetValue(speedAmount) * obj.strength
		}, speedDuration, "astrids_speed");
		FxPlayNetworked(fxActivateOnSelf, victim);
		foreach (AbilityTrigger value in victim.Ability.abilities.Values)
		{
			if (value is St_Q_Lunge st_Q_Lunge)
			{
				ApplyCooldownReduction(st_Q_Lunge, st_Q_Lunge.reducedCooldownByMeister * obj.strength);
			}
		}
	}

	public void Reset()
	{
		ResetCooldown(_trigger, ignoreCanReceiveCooldown: true);
		if (!_currentExposed.IsNullOrInactive())
		{
			_isReset = true;
			_currentExposed.Get().Destroy();
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer && !((UnityEngine.Object)(object)_trigger == null) && _currentExposed.IsNullOrInactive())
		{
			CheckAndApplyExposed();
		}
	}

	private void CheckAndApplyExposed()
	{
		List<Entity> list = DewPhysics.OverlapCircleAllEntities(out var handle, victim.agentPosition, radius, tvDefaultHarmfulEffectTargets);
		float num = 0f;
		Entity a = null;
		foreach (Entity item in list)
		{
			if (!item.Visual.isSpawning && !item.Status.hasInvulnerable && item.GetRelation(victim) == EntityRelation.Enemy && item.maxHealth > num)
			{
				a = item;
			}
		}
		handle.Return();
		if (a.IsNullOrInactive())
		{
			return;
		}
		_currentExposed = CreateStatusEffect<Se_D_AstridsMasterpiecePriorite_Exposed>(a, new CastInfo(victim));
		_currentExposed.Get().ClientActorEvent_OnDestroyed += (Action<Actor>)((Actor _) =>
		{
			if (((NetworkBehaviour)this).isServer && !((UnityEngine.Object)(object)_trigger == null))
			{
				if (_isReset)
				{
					_isReset = false;
				}
				else
				{
					_trigger.SetCharge(0, 0);
					_trigger.fillAmount = 0f;
				}
			}
		});
		_trigger.fillAmount = 1f;
		_trigger.SetCharge(0, 1);
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			if ((UnityEngine.Object)(object)victim != null)
			{
				victim.EntityEvent_OnAttackEffectTriggered -= new Action<EventInfoAttackEffect>(CheckExposed);
			}
			if (!_currentExposed.IsNullOrInactive())
			{
				_currentExposed.Get().Destroy();
			}
			_currentExposed = null;
		}
	}

	private void MirrorProcessed()
	{
	}
}
