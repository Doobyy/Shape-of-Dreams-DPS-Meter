using System;
using Mirror;
using UnityEngine;

public class Se_D_AstridsMasterpieceEnGarde : StatusEffect
{
	public ScalingValue damage;

	public ScalingValue shieldAmount;

	public float shieldDuration = 1.5f;

	public float radius = 9f;

	public float checkInterval = 0.5f;

	public GameObject[] hitEffectsByCharge;

	public GameObject fxActivateOnSelf;

	public bool setCritMarker;

	private float _lastCheckTime;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			victim.EntityEvent_OnAttackEffectTriggered += new Action<EventInfoAttackEffect>(CheckExposed);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)victim != null)
		{
			victim.EntityEvent_OnAttackEffectTriggered -= new Action<EventInfoAttackEffect>(CheckExposed);
		}
	}

	private void CheckExposed(EventInfoAttackEffect obj)
	{
		Se_D_AstridsMasterpieceEnGarde_Exposed se_D_AstridsMasterpieceEnGarde_Exposed = null;
		foreach (StatusEffect statusEffect in obj.victim.Status.statusEffects)
		{
			if (statusEffect is Se_D_AstridsMasterpieceEnGarde_Exposed se_D_AstridsMasterpieceEnGarde_Exposed2 && (UnityEngine.Object)(object)statusEffect.parentActor == (UnityEngine.Object)(object)this)
			{
				se_D_AstridsMasterpieceEnGarde_Exposed = se_D_AstridsMasterpieceEnGarde_Exposed2;
				break;
			}
		}
		if ((UnityEngine.Object)(object)se_D_AstridsMasterpieceEnGarde_Exposed == null || se_D_AstridsMasterpieceEnGarde_Exposed.chargeCount <= 0)
		{
			return;
		}
		se_D_AstridsMasterpieceEnGarde_Exposed.chargeCount--;
		DamageData damageData = Damage(damage).ApplyStrength(obj.strength).SetOriginPosition(victim.position);
		if (setCritMarker)
		{
			damageData.SetAttr(DamageAttribute.IsCrit);
		}
		damageData.Dispatch(obj.victim);
		GiveShield(victim, GetValue(shieldAmount) * obj.strength, shieldDuration, isDecay: true);
		FxPlayNetworked(fxActivateOnSelf, victim);
		FxPlayNetworked(hitEffectsByCharge[se_D_AstridsMasterpieceEnGarde_Exposed.chargeCount], obj.victim);
		foreach (AbilityTrigger value in victim.Ability.abilities.Values)
		{
			if (value is St_Q_Lunge st_Q_Lunge)
			{
				ApplyCooldownReduction(st_Q_Lunge, st_Q_Lunge.reducedCooldownByMeister * obj.strength);
			}
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer && Time.time - _lastCheckTime > checkInterval)
		{
			_lastCheckTime = Time.time;
			CheckAndApplyExposed();
		}
	}

	private void CheckAndApplyExposed()
	{
		ListReturnHandle<Entity> handle;
		foreach (Entity item in DewPhysics.OverlapCircleAllEntities(out handle, victim.agentPosition, radius, tvDefaultHarmfulEffectTargets))
		{
			if (item.Visual.isSpawning || item.Status.hasInvulnerable)
			{
				continue;
			}
			bool flag = false;
			foreach (StatusEffect statusEffect in item.Status.statusEffects)
			{
				if (statusEffect is Se_D_AstridsMasterpieceEnGarde_Exposed && (UnityEngine.Object)(object)statusEffect.parentActor == (UnityEngine.Object)(object)this)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				CreateStatusEffect<Se_D_AstridsMasterpieceEnGarde_Exposed>(item, new CastInfo(victim));
			}
		}
		handle.Return();
	}

	private void MirrorProcessed()
	{
	}
}
