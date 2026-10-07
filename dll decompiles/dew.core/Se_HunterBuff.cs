using System;
using Mirror;
using UnityEngine;

public class Se_HunterBuff : StatusEffect, IOtherPlayersTonedDownDisable
{
	public float populationMultiplier = 2f;

	public float movementSpeedPercentage = 50f;

	public float attackSpeedPercentage = 50f;

	public float abilityHaste = 50f;

	public float shadowWalkFarChance = 0.25f;

	public float shadowWalkCloseChance = 0.25f;

	public Vector2 shadowWalkInterval;

	public AbilitySelfValidator shadowWalkValidator;

	public float shadowWalkDistanceThresholdNoAttack = 4f;

	public float shadowWalkDistanceThresholdOffset = 1f;

	[NonSerialized]
	public bool enableGoldAndExpDrops;

	private float _currentShadowWalkInterval;

	private float _lastShadowWalkTime;

	private EntityVisual.EntityDeathBehavior _prevDeathBehavior;

	private GameObject _prevFxDeath;

	private DewAudioClip _prevVoiceDeath;

	protected override void OnCreate()
	{
		base.OnCreate();
		Monster monster = victim as Monster;
		if ((UnityEngine.Object)(object)monster != null)
		{
			_prevDeathBehavior = monster.Visual.model.deathBehavior;
			_prevFxDeath = monster.Visual.model.fxDeath;
			_prevVoiceDeath = monster.Sound.voiceDeath;
			monster.Visual.model.deathBehavior = EntityVisual.EntityDeathBehavior.HideModel;
			monster.Visual.model.fxDeath = null;
			monster.Sound.voiceDeath = null;
		}
		if (((NetworkBehaviour)this).isServer)
		{
			if ((UnityEngine.Object)(object)monster != null)
			{
				monster.isHunter = true;
				monster.populationCost *= populationMultiplier;
			}
			victim.Status.AddStatBonus(new StatBonus
			{
				movementSpeedPercentage = movementSpeedPercentage,
				attackSpeedPercentage = attackSpeedPercentage,
				abilityHasteFlat = abilityHaste
			});
			_lastShadowWalkTime = Time.time;
			_currentShadowWalkInterval = UnityEngine.Random.Range(shadowWalkInterval.x, shadowWalkInterval.y);
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!((NetworkBehaviour)this).isServer || !(Time.time - _lastShadowWalkTime > _currentShadowWalkInterval))
		{
			return;
		}
		_lastShadowWalkTime = Time.time;
		_currentShadowWalkInterval = UnityEngine.Random.Range(shadowWalkInterval.x, shadowWalkInterval.y);
		if (victim.Control.IsActionBlocked(EntityControl.BlockableAction.Ability) != EntityControl.BlockStatus.Allowed || !shadowWalkValidator.Evaluate(victim) || (UnityEngine.Object)(object)victim.Control.attackTarget == null || victim.Control.ongoingChannels.Count > 0)
		{
			return;
		}
		float num = ((!((UnityEngine.Object)(object)victim.Ability.attackAbility != null)) ? shadowWalkDistanceThresholdNoAttack : (victim.Ability.attackAbility.currentConfig.effectiveRange + shadowWalkDistanceThresholdOffset));
		Vector3 b = victim.Control.attackTarget.position;
		if (Vector3.Distance(victim.position, b) < num)
		{
			if (UnityEngine.Random.value > shadowWalkCloseChance)
			{
				return;
			}
		}
		else if (UnityEngine.Random.value > shadowWalkFarChance)
		{
			return;
		}
		CreateAbilityInstance<Ai_HunterBuff_ShadowWalk>(victim.position, victim.rotation, new CastInfo(victim, victim.Control.attackTarget));
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (victim is Monster monster)
		{
			if (monster.Visual.model != null)
			{
				monster.Visual.model.deathBehavior = _prevDeathBehavior;
				monster.Visual.model.fxDeath = _prevFxDeath;
			}
			monster.Sound.voiceDeath = _prevVoiceDeath;
			if (((NetworkBehaviour)this).isServer)
			{
				monster.isHunter = false;
				monster.populationCost /= populationMultiplier;
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
