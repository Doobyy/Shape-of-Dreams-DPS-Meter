using System;
using System.Collections;
using Mirror;
using UnityEngine;

public class Se_Mon_Primus_BossPrimusAeron_Adaptation : StatusEffect
{
	public enum StatType
	{
		MaxHealthPercentage,
		Speed,
		Damage
	}

	[NonSerialized]
	public StatBonus bonus;

	public float adaptiveDamageMaxHp = 0.02f;

	public float adaptiveDamageCurrentHealth = 0.04f;

	public float adaptiveDamageCurrentShield = 0.1f;

	[Space]
	public float desiredCombatTime = 90f;

	public int maxHealthPercentageMin = 15;

	public int maxHealthPercentageMax = 300;

	public float hpPercentagePerChaos = 15f;

	[Space]
	public float tooLowDamageFactorThresholdMin = 1f;

	public float tooLowDamageFactorThresholdMax = 6f;

	public int speedPercentageMin = 5;

	public int speedPercentageMax = 30;

	public float speedPercentagePerChaos = 5f;

	[Space]
	public float tooHighDamageFactorThresholdMin = 12f;

	public float tooHighDamageFactorThresholdMax = 20f;

	public int damagePercentageMin = 5;

	public int damagePercentageMax = 150;

	public float damagePercentagePerChaos = 10f;

	private float _lastDamageTime = float.NegativeInfinity;

	private float _elapsedCombatTime;

	private float _dealtDamageFactorNormalized;

	public new Mon_Primus_BossPrimusAeron victim => base.victim as Mon_Primus_BossPrimusAeron;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			_elapsedCombatTime = 0f;
			_dealtDamageFactorNormalized = 0f;
			bonus = DoStatBonus();
			victim.EntityEvent_OnTakeDamage += new Action<EventInfoDamage>(EntityEventOnTakeDamage);
			victim.dealtDamageProcessor.Add(Processor);
		}
	}

	private void Processor(ref DamageData data, Actor actor, Entity target)
	{
		if (!victim.CheckEnemyOrNeutral(target))
		{
			return;
		}
		if (victim.phase == Mon_Primus_BossPrimusAeron.PhaseType.InTransition)
		{
			data.ApplyRawMultiplier(0f);
		}
		else if (!(actor is ElementalStatusEffect))
		{
			float num = data.originalAmount / victim.Status.abilityPower;
			if (victim.phase == Mon_Primus_BossPrimusAeron.PhaseType.Force)
			{
				int aliveHeroCount = Dew.GetAliveHeroCount();
				_dealtDamageFactorNormalized += num / (float)aliveHeroCount;
			}
			data.AddFlatAmount((target.maxHealth * adaptiveDamageMaxHp + target.currentHealth * adaptiveDamageCurrentHealth + target.Status.currentShield * adaptiveDamageCurrentShield) * num);
		}
	}

	private void EntityEventOnTakeDamage(EventInfoDamage obj)
	{
		_lastDamageTime = Time.time;
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)victim != null)
		{
			victim.EntityEvent_OnTakeDamage -= new Action<EventInfoDamage>(EntityEventOnTakeDamage);
			victim.dealtDamageProcessor.Remove(Processor);
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer && victim.phase == Mon_Primus_BossPrimusAeron.PhaseType.Force && !victim.Status.hasDamageImmunity && !ManagerBase<CameraManager>.instance.isPlayingCutscene && Time.time - _lastDamageTime < 8f)
		{
			_elapsedCombatTime += dt;
		}
	}

	public IEnumerator GiveAdaptationOrbsRoutine()
	{
		Debug.Log($"[Adaptation] Combat Time: {_elapsedCombatTime:#,##0.0} seconds");
		Debug.Log($"[Adaptation] Dealt Damage Factor Normalized: {_dealtDamageFactorNormalized:#,##0.0}");
		float maxHpPercentage = Mathf.Clamp(desiredCombatTime / _elapsedCombatTime * 100f - 100f, maxHealthPercentageMin, maxHealthPercentageMax) * 0.8f;
		float speedPercentage = Mathf.Lerp(t: Mathf.Clamp01((_dealtDamageFactorNormalized - tooLowDamageFactorThresholdMin) / (tooLowDamageFactorThresholdMax - tooLowDamageFactorThresholdMin)), a: speedPercentageMax, b: speedPercentageMin);
		float damagePercentage = Mathf.Lerp(t: Mathf.Clamp01((_dealtDamageFactorNormalized - tooHighDamageFactorThresholdMin) / (tooHighDamageFactorThresholdMax - tooHighDamageFactorThresholdMin)), a: damagePercentageMin, b: damagePercentageMax);
		while (maxHpPercentage > 0f && !this.IsNullOrInactive())
		{
			GiveOrb(StatType.MaxHealthPercentage, hpPercentagePerChaos);
			maxHpPercentage -= hpPercentagePerChaos;
			yield return new WaitForSeconds(0.1f);
		}
		while (damagePercentage > 0f && !this.IsNullOrInactive())
		{
			GiveOrb(StatType.Damage, damagePercentagePerChaos);
			damagePercentage -= damagePercentagePerChaos;
			yield return new WaitForSeconds(0.125f);
		}
		while (speedPercentage > 0f && !this.IsNullOrInactive())
		{
			GiveOrb(StatType.Speed, speedPercentagePerChaos);
			speedPercentage -= speedPercentagePerChaos;
			yield return new WaitForSeconds(0.155f);
		}
		void GiveOrb(StatType type, float amount)
		{
			CreateAbilityInstance(victim.agentPosition, null, new CastInfo(victim), (Ai_Mon_Primus_BossPrimusAeron_Adaptation_Orb ai) =>
			{
				ai.Networktype = type;
				ai.amount = amount;
			});
		}
	}

	private void MirrorProcessed()
	{
	}
}
