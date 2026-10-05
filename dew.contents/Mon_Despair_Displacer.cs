using Mirror;
using UnityEngine;

public class Mon_Despair_Displacer : Monster, ISpawnableAsMiniBoss
{
	public float blinkChancePerSecond = 0.3f;

	public float blinkMinRange;

	public float spawnEggChancePerSecond = 0.2f;

	protected override void AIUpdate(ref EntityAIContext context)
	{
		base.AIUpdate(ref context);
		if (!((Object)(object)context.targetEnemy == null))
		{
			if (AI.Helper_CanBeCast<At_Mon_Despair_Displacer_Dash>() && AI.Helper_IsTargetInRange<At_Mon_Despair_Displacer_Dash>())
			{
				AI.Helper_CastAbilityAuto<At_Mon_Despair_Displacer_Dash>();
			}
			if (AI.Helper_IsTargetInRangeOfAttack())
			{
				AI.Helper_ChaseTarget();
			}
			else if (Random.value < spawnEggChancePerSecond * context.deltaTime && AI.Helper_CanBeCast<At_Mon_Despair_Displacer_SpawnEgg>())
			{
				AI.Helper_CastAbilityAuto<At_Mon_Despair_Displacer_SpawnEgg>();
			}
			else if (Random.value < blinkChancePerSecond * context.deltaTime && AI.Helper_CanBeCast<At_Mon_Despair_Displacer_Blink>() && AI.Helper_IsTargetInRange<At_Mon_Despair_Displacer_Blink>() && Vector2.Distance(context.targetEnemy.agentPosition.ToXY(), agentPosition.ToXY()) > blinkMinRange)
			{
				AI.Helper_CastAbilityAuto<At_Mon_Despair_Displacer_Blink>();
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
		At_Mon_Despair_Displacer_Dash ability = Ability.GetAbility<At_Mon_Despair_Displacer_Dash>();
		ability.configs[0].cooldownTime *= 2f;
		ability.configs[0].channel.duration *= 0.5f;
		ability.configs[0].addedCharges = 3;
		ability.configs[0].maxCharges = 3;
		if (((NetworkBehaviour)this).isServer)
		{
			ISpawnableAsMiniBoss.GiveGenericMiniBossBonus(this);
			spawnEggChancePerSecond = 1f;
			Ability.GetAbility<At_Mon_Despair_Displacer_SpawnEgg>().configs[0].cooldownTime = 1f;
			Status.AddStatBonus(new StatBonus
			{
				attackSpeedPercentage = 50f
			});
		}
	}

	private void MirrorProcessed()
	{
	}
}
