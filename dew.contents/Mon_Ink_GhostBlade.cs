using System;
using Mirror;
using UnityEngine;

public class Mon_Ink_GhostBlade : Monster, ISpawnableAsMiniBoss
{
	public float swiftChance = 0.8f;

	protected override void AIUpdate(ref EntityAIContext context)
	{
		base.AIUpdate(ref context);
		if (!((UnityEngine.Object)(object)context.targetEnemy == null))
		{
			if (AI.Helper_CanBeCast<At_Mon_Ink_GhostBlade_SwiftStep>() && UnityEngine.Random.value < swiftChance)
			{
				AI.Helper_CastAbilityAuto<At_Mon_Ink_GhostBlade_SwiftStep>();
			}
			AI.Helper_ChaseTarget();
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
		ActorEvent_OnAbilityInstanceBeforePrepare += (Action<EventInfoAbilityInstance>)((EventInfoAbilityInstance obj) =>
		{
			if (obj.instance is Ai_Mon_Ink_GhostBlade_SwiftStep_Atk ai_Mon_Ink_GhostBlade_SwiftStep_Atk)
			{
				ai_Mon_Ink_GhostBlade_SwiftStep_Atk.addedProjectiles = 2;
			}
		});
		Status.AddStatBonus(new StatBonus
		{
			attackSpeedPercentage = 40f
		});
	}

	private void MirrorProcessed()
	{
	}
}
