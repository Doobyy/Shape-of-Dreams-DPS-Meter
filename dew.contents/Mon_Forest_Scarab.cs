using UnityEngine;

public class Mon_Forest_Scarab : Monster
{
	protected override void AIUpdate(ref EntityAIContext context)
	{
		base.AIUpdate(ref context);
		if (!((Object)(object)context.targetEnemy == null))
		{
			AI.Helper_ChaseTarget();
		}
	}

	private void MirrorProcessed()
	{
	}
}
