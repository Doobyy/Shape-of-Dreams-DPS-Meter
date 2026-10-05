using System;
using Mirror;
using UnityEngine;

public class Mon_SnowMountain_IceElemental : Monster, ISpawnableAsMiniBoss
{
	public float doubleSwipeChancePerSecond;

	public float iceBreakerChancePerSecond;

	protected override StaggerSettings GetStaggerSettings()
	{
		StaggerSettings staggerSettings = base.GetStaggerSettings();
		staggerSettings.canStaggerWhileChanneling = false;
		return staggerSettings;
	}

	protected override void AIUpdate(ref EntityAIContext context)
	{
		base.AIUpdate(ref context);
		if (!((UnityEngine.Object)(object)context.targetEnemy == null))
		{
			if (AI.Helper_CanBeCast<At_Mon_SnowMountain_IceElemental_DoubleSwipe>() && AI.Helper_IsTargetInRange<At_Mon_SnowMountain_IceElemental_DoubleSwipe>() && UnityEngine.Random.value < context.deltaTime * doubleSwipeChancePerSecond)
			{
				AI.Helper_CastAbilityAuto<At_Mon_SnowMountain_IceElemental_DoubleSwipe>();
			}
			else if (AI.Helper_CanBeCast<At_Mon_SnowMountain_IceElemental_IceBreaker>() && AI.Helper_IsTargetInRange<At_Mon_SnowMountain_IceElemental_IceBreaker>() && UnityEngine.Random.value < context.deltaTime * iceBreakerChancePerSecond)
			{
				AI.Helper_CastAbilityAuto<At_Mon_SnowMountain_IceElemental_IceBreaker>();
			}
			else
			{
				AI.Helper_ChaseTarget();
			}
		}
	}

	public void OnBeforeSpawnAsMiniBoss()
	{
	}

	public void OnCreateAsMiniBoss()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		ISpawnableAsMiniBoss.GiveGenericMiniBossBonus(this);
		TriggerConfig obj = Ability.attackAbility.configs[0];
		obj.castMethod._length = 9f;
		AbilityTrigger.PredictionSettings predictionSettings = obj.predictionSettings;
		predictionSettings.useCustomParameters = true;
		predictionSettings.type = AbilityTrigger.PredictionSettings.ModelType.SpeedAcceleration;
		predictionSettings.initSpeed = 15f;
		predictionSettings.targetSpeed = 15f;
		predictionSettings.acceleration = 0f;
		obj.predictionSettings = predictionSettings;
		ActorEvent_OnAbilityInstanceCreated += (Action<EventInfoAbilityInstance>)((EventInfoAbilityInstance eventInfoAbilityInstance) =>
		{
			if (eventInfoAbilityInstance.instance is Ai_Mon_SnowMountain_IceElemental_Atk)
			{
				eventInfoAbilityInstance.instance.CreateAbilityInstance<Ai_Mon_SnowMountain_IceElemental_MiniBossAtkFlavour>(eventInfoAbilityInstance.instance.position, null, eventInfoAbilityInstance.instance.info);
			}
		});
	}

	private void MirrorProcessed()
	{
	}
}
