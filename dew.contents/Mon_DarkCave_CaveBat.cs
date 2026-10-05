using UnityEngine;

public class Mon_DarkCave_CaveBat : Monster
{
	public float dashChancePerSecond;

	public override void OnStartServer()
	{
		base.OnStartServer();
		AddData(new LavaLand_Lava.Ad_Lava
		{
			isImmune = true
		});
	}

	protected override void AIUpdate(ref EntityAIContext context)
	{
		base.AIUpdate(ref context);
		if (!((Object)(object)context.targetEnemy == null))
		{
			if (Random.value < dashChancePerSecond * context.deltaTime && AI.Helper_CanBeCast<At_Mon_DarkCave_CaveBat_Dash>() && AI.Helper_IsTargetInRange<At_Mon_DarkCave_CaveBat_Dash>())
			{
				AI.Helper_CastAbilityAuto<At_Mon_DarkCave_CaveBat_Dash>();
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
