using Mirror;
using UnityEngine;

public class Mon_Despair_ParalyticFly : Monster, ISpawnableAsMiniBoss
{
	public float castParalyzeChancePerSecond = 0.2f;

	protected override void AIUpdate(ref EntityAIContext context)
	{
		base.AIUpdate(ref context);
		if (!((Object)(object)context.targetEnemy == null))
		{
			if (AI.Helper_CanBeCast<At_Mon_Despair_ParalyticFly_Melee>() && AI.Helper_IsTargetInRange<At_Mon_Despair_ParalyticFly_Melee>())
			{
				AI.Helper_CastAbilityAuto<At_Mon_Despair_ParalyticFly_Melee>();
			}
			else if (Random.value < castParalyzeChancePerSecond * context.deltaTime && AI.Helper_CanBeCast<At_Mon_Despair_ParalyticFly_Paralyze>() && AI.Helper_IsTargetInRange<At_Mon_Despair_ParalyticFly_Paralyze>())
			{
				AI.Helper_CastAbilityAuto<At_Mon_Despair_ParalyticFly_Paralyze>();
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
		Ability.GetAbility<At_Mon_Despair_ParalyticFly_Paralyze>().configs[0].channel.duration *= 0.5f;
		if (((NetworkBehaviour)this).isServer)
		{
			ISpawnableAsMiniBoss.GiveGenericMiniBossBonus(this);
			castParalyzeChancePerSecond = 1f;
			Ability.GetAbility<At_Mon_Despair_ParalyticFly_Atk>().spawnAdditionalProjectile = true;
			Ability.GetAbility<At_Mon_Despair_ParalyticFly_Paralyze>().configs[0].cooldownTime *= 0.4f;
		}
	}

	private void MirrorProcessed()
	{
	}
}
