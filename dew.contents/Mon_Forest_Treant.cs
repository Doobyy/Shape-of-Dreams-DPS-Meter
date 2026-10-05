using UnityEngine;

public class Mon_Forest_Treant : Monster
{
	public float rageStartRange = 5f;

	public override void OnStartServer()
	{
		base.OnStartServer();
		CreateBasicEffect(this, new UnstoppableEffect(), float.PositiveInfinity);
	}

	protected override void AIUpdate(ref EntityAIContext context)
	{
		base.AIUpdate(ref context);
		if (!((Object)(object)context.targetEnemy == null))
		{
			if (AI.Helper_CanBeCast<At_Mon_Forest_Treant_PowerBomb>() && Vector3.Distance(position, context.targetEnemy.position) < rageStartRange)
			{
				AI.Helper_CastAbilityAuto<At_Mon_Forest_Treant_PowerBomb>();
			}
			else if (AI.Helper_CanBeCast<At_Mon_Forest_Treant_Rage>() && Vector3.Distance(position, context.targetEnemy.position) >= rageStartRange)
			{
				AI.Helper_CastAbilityAuto<At_Mon_Forest_Treant_Rage>();
			}
			else
			{
				AI.Helper_ChaseTarget();
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
