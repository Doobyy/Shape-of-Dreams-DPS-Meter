using Mirror;
using UnityEngine;

public class Mon_Sky_BigBaam_Rooter : Mon_Sky_BigBaam_Base, ISpawnableAsMiniBoss
{
	protected override bool MainSkillRoutine()
	{
		if (AI.Helper_CanBeCast<At_Mon_Sky_BigBaam_Main_Root>() && (Object)(object)AI.context.targetEnemy != null)
		{
			if (AI.Helper_IsTargetInRange<At_Mon_Sky_BigBaam_Main_Root>())
			{
				AI.Helper_CastAbilityAuto<At_Mon_Sky_BigBaam_Main_Root>();
			}
			else
			{
				AI.Helper_ChaseTarget();
			}
			return true;
		}
		return false;
	}

	public void OnBeforeSpawnAsMiniBoss()
	{
	}

	public void OnCreateAsMiniBoss()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			ISpawnableAsMiniBoss.GiveGenericMiniBossBonus(this);
			Ability.GetAbility<At_Mon_Sky_BigBaam_BeamAtk>().spawnAdditionalProjectile = true;
			Ability.GetAbility<At_Mon_Sky_BigBaam_Main_Root>().configs[0].cooldownTime *= 0.5f;
		}
	}

	private void MirrorProcessed()
	{
	}
}
