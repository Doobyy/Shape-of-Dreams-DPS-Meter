using System;
using System.Collections.Generic;
using UnityEngine;

public class Mon_Special_BossObliviax : BossMonster, IPrewarmMonsterContributor
{
	public float skillChance = 0.2f;

	public float shadowWalkFarChance = 0.25f;

	public float shadowWalkCloseChance = 0.25f;

	public float shadowWalkDistanceThresholdOffset = 1f;

	public Vector2 shadowWalkInterval;

	public float backStepChance;

	internal float _chainAttackRequestTime = float.NegativeInfinity;

	private float _currentShadowWalkInterval;

	private float _lastShadowWalkTime;

	public bool isDashAtkAvailable => true;

	public bool isTurnAtkAvailable => true;

	public bool isTurretModeAvailable => true;

	public bool isNeedleAtkAvailable => normalizedHealth < 0.6666f;

	public bool isArtilleryAvailable => normalizedHealth < 0.7111f;

	public bool isBackstepAvailable => normalizedHealth < 0.4555f;

	public void ContributeMonsterPrewarm(Dictionary<Monster, int> counts, int instanceCount)
	{
		Mon_Special_ObliviaxHallucination byType = DewResources.GetByType<Mon_Special_ObliviaxHallucination>(default(ResourceLoadSettings));
		Ai_Mon_Special_BossObliviax_ChargeSequence_Spawner byType2 = DewResources.GetByType<Ai_Mon_Special_BossObliviax_ChargeSequence_Spawner>(default(ResourceLoadSettings));
		if (!((UnityEngine.Object)(object)byType == null) && !((UnityEngine.Object)(object)byType2 == null))
		{
			int num = ((DewPlayer.gamePlayers == null) ? 1 : DewPlayer.gamePlayers.Count);
			int num2 = byType2.ssWaveCount * (byType2.ssAtkCountPerWave + Mathf.Max(0, num - 1));
			counts.TryGetValue(byType, out var value);
			counts[byType] = value + num2 * 3 * instanceCount;
		}
	}

	public override void OnStartServer()
	{
		base.OnStartServer();
		CreateBasicEffect(this, new UnstoppableEffect(), float.PositiveInfinity);
		_lastShadowWalkTime = Time.time;
		_currentShadowWalkInterval = UnityEngine.Random.Range(shadowWalkInterval.x, shadowWalkInterval.y);
		isHunter = true;
	}

	protected override void AIUpdate(ref EntityAIContext context)
	{
		base.AIUpdate(ref context);
		if (AI.Helper_CanBeCast<At_Mon_Special_BossObliviax_Laugh>() && NetworkedManagerBase<GameManager>.instance.isGameConcluded)
		{
			AI.Helper_CastAbilityAuto<At_Mon_Special_BossObliviax_Laugh>();
		}
		else
		{
			if ((UnityEngine.Object)(object)context.targetEnemy == null)
			{
				return;
			}
			float num = skillChance * context.deltaTime;
			if (Time.time - _chainAttackRequestTime < 1.5f && AI.Helper_CanBeCast<At_Mon_Special_BossObliviax_ShadowWalk>() && AI.Helper_IsTargetInRange<At_Mon_Special_BossObliviax_ShadowWalk>())
			{
				ResetCooldown(Ability.attackAbility);
				AI.Helper_CastAbilityAuto<At_Mon_Special_BossObliviax_ShadowWalk>();
				_chainAttackRequestTime = float.NegativeInfinity;
				return;
			}
			if (isDashAtkAvailable && UnityEngine.Random.value < num && AI.Helper_CanBeCast<At_Mon_Special_BossObliviax_DashAtk>() && AI.Helper_IsTargetInRange<At_Mon_Special_BossObliviax_DashAtk>())
			{
				AI.Helper_CastAbilityAuto<At_Mon_Special_BossObliviax_DashAtk>();
				return;
			}
			if (isTurnAtkAvailable && UnityEngine.Random.value < num && AI.Helper_CanBeCast<At_Mon_Special_BossObliviax_TurnAtk>() && AI.Helper_IsTargetInRange<At_Mon_Special_BossObliviax_TurnAtk>())
			{
				AI.Helper_CastAbilityAuto<At_Mon_Special_BossObliviax_TurnAtk>();
				return;
			}
			if (isTurretModeAvailable && UnityEngine.Random.value < num && AI.Helper_CanBeCast<At_Mon_Special_BossObliviax_TurretMode>())
			{
				Entity entity = null;
				float num2 = 0f;
				foreach (DewPlayer gamePlayer in DewPlayer.gamePlayers)
				{
					if (!gamePlayer.hero.IsNullInactiveDeadOrKnockedOut())
					{
						float num3 = Vector3.Distance(gamePlayer.hero.GetAIPosition(this), position);
						if (num3 > num2)
						{
							num2 = num3;
							entity = gamePlayer.hero;
						}
					}
				}
				if (!entity.IsNullInactiveDeadOrKnockedOut())
				{
					AI.Helper_CastAbility<At_Mon_Special_BossObliviax_TurretMode>(new CastInfo(this, entity));
				}
				return;
			}
			if (isNeedleAtkAvailable && UnityEngine.Random.value < num && AI.Helper_CanBeCast<At_Mon_Special_BossObliviax_NeedleAtk>() && AI.Helper_IsTargetInRange<At_Mon_Special_BossObliviax_NeedleAtk>() && normalizedHealth > 0.125f)
			{
				AI.Helper_CastAbilityAuto<At_Mon_Special_BossObliviax_NeedleAtk>();
				return;
			}
			if (isArtilleryAvailable && UnityEngine.Random.value < num && AI.Helper_CanBeCast<At_Mon_Special_BossObliviax_Artillery>() && AI.Helper_IsTargetInRange<At_Mon_Special_BossObliviax_Artillery>())
			{
				AI.Helper_CastAbilityAuto<At_Mon_Special_BossObliviax_Artillery>();
				return;
			}
			if (isBackstepAvailable && UnityEngine.Random.value < num * 2f && AI.Helper_CanBeCast<At_Mon_Special_BossObliviax_BackStep>() && AI.Helper_IsTargetInRange<At_Mon_Special_BossObliviax_BackStep>())
			{
				AI.Helper_CastAbilityAuto<At_Mon_Special_BossObliviax_BackStep>();
				return;
			}
			Entity targetEnemy = context.targetEnemy;
			if (Time.time - _lastShadowWalkTime > _currentShadowWalkInterval)
			{
				_lastShadowWalkTime = Time.time;
				_currentShadowWalkInterval = UnityEngine.Random.Range(shadowWalkInterval.x, shadowWalkInterval.y);
				float num4 = Ability.attackAbility.currentConfig.effectiveRange + shadowWalkDistanceThresholdOffset;
				Vector3 aIAgentPosition = targetEnemy.GetAIAgentPosition(this);
				if (Vector3.Distance(agentPosition, aIAgentPosition) < num4)
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
				AI.Helper_CastAbility<At_Mon_Special_BossObliviax_ShadowWalk>(new CastInfo(this, targetEnemy));
			}
			else
			{
				AI.Helper_ChaseTarget();
			}
		}
	}

	public override Type GetUniqueReward()
	{
		return typeof(St_U_ShoutOfOblivion);
	}

	private void MirrorProcessed()
	{
	}
}
