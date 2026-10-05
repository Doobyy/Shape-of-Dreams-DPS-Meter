using System;
using Mirror;
using UnityEngine;

public class Se_Mon_Special_BossPolaris_Adaptation : StatusEffect
{
	[NonSerialized]
	public StatBonus bonus;

	[Space]
	public float desiredCombatTime = 90f;

	public int maxHealthPercentageMin = 15;

	public int maxHealthPercentageMax = 300;

	[Space]
	public float tooLowDamageFactorThresholdMin = 1f;

	public float tooLowDamageFactorThresholdMax = 6f;

	public int speedPercentageMin = 5;

	public int speedPercentageMax = 30;

	[Space]
	public float tooHighDamageFactorThresholdMin = 12f;

	public float tooHighDamageFactorThresholdMax = 20f;

	public int damagePercentageMin = 5;

	public int damagePercentageMax = 150;

	private float _lastDamageTime = float.NegativeInfinity;

	private float _elapsedCombatTime;

	private float _dealtDamageFactorNormalized;

	public new Mon_Special_BossPolaris victim => base.victim as Mon_Special_BossPolaris;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			_elapsedCombatTime = 0f;
			_dealtDamageFactorNormalized = 0f;
			bonus = DoStatBonus();
			victim.EntityEvent_OnTakeDamage += new Action<EventInfoDamage>(EntityEventOnTakeDamage);
			victim.dealtDamageProcessor.Add(Processor, -100);
		}
	}

	private void Processor(ref DamageData data, Actor actor, Entity target)
	{
		if (victim.CheckEnemyOrNeutral(target) && !(actor is ElementalStatusEffect) && !(actor is Se_Mon_Special_BossPolaris_CleansingFlame))
		{
			float num = data.originalAmount / victim.Status.abilityPower;
			int aliveHeroCount = Dew.GetAliveHeroCount();
			_dealtDamageFactorNormalized += num / (float)aliveHeroCount;
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
		if (((NetworkBehaviour)this).isServer && victim.mainPhase == Mon_Special_BossPolaris.MainPhase.Holy && !victim.Status.hasDamageImmunity && !ManagerBase<CameraManager>.instance.isPlayingCutscene && Time.time - _lastDamageTime < 8f)
		{
			_elapsedCombatTime += dt;
		}
	}

	public void ApplyAdaptationStats()
	{
		Debug.Log($"[Adaptation] Combat Time: {_elapsedCombatTime:#,##0.0} seconds");
		Debug.Log($"[Adaptation] Dealt Damage Factor Normalized: {_dealtDamageFactorNormalized:#,##0.0}");
		float maxHealthPercentage = Mathf.Clamp(desiredCombatTime / _elapsedCombatTime * 100f - 100f, maxHealthPercentageMin, maxHealthPercentageMax) * 0.8f;
		float t = Mathf.Clamp01((_dealtDamageFactorNormalized - tooLowDamageFactorThresholdMin) / (tooLowDamageFactorThresholdMax - tooLowDamageFactorThresholdMin));
		t = Mathf.Lerp(speedPercentageMax, speedPercentageMin, t);
		float t2 = Mathf.Clamp01((_dealtDamageFactorNormalized - tooHighDamageFactorThresholdMin) / (tooHighDamageFactorThresholdMax - tooHighDamageFactorThresholdMin));
		t2 = Mathf.Lerp(damagePercentageMin, damagePercentageMax, t2);
		bonus.maxHealthPercentage = maxHealthPercentage;
		bonus.attackDamagePercentage = t2;
		bonus.abilityPowerPercentage = t2;
		bonus.movementSpeedPercentage = t;
		bonus.attackSpeedPercentage = t;
	}

	private void MirrorProcessed()
	{
	}
}
