using Mirror;
using UnityEngine;

public class Mon_Ink_GhostSpear : Monster, ISpawnableAsMiniBoss
{
	protected override void AIUpdate(ref EntityAIContext context)
	{
		base.AIUpdate(ref context);
		if (!((Object)(object)context.targetEnemy == null))
		{
			if (!AI.Helper_IsTargetInRange<At_Mon_Ink_GhostSpear_Atk>() && AI.Helper_CanBeCast<At_Mon_Ink_GhostSpear_Dash>())
			{
				AI.Helper_CastAbilityAuto<At_Mon_Ink_GhostSpear_Dash>();
			}
			AI.Helper_ChaseTarget();
		}
	}

	public void OnBeforeSpawnAsMiniBoss()
	{
	}

	public void OnCreateAsMiniBoss()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			ISpawnableAsMiniBoss.GiveGenericMiniBossBonus(this);
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
