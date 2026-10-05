using Mirror;
using UnityEngine;

public class Mon_LavaLand_Magmadon : Monster, ISpawnableAsMiniBoss
{
	public override void OnStartServer()
	{
		base.OnStartServer();
		CreateBasicEffect(this, new UnstoppableEffect(), float.PositiveInfinity);
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
			if (AI.Helper_CanBeCast<At_Mon_LavaLand_Magmadon_Charge>() && AI.Helper_IsTargetInRange<At_Mon_LavaLand_Magmadon_Charge>())
			{
				AI.Helper_CastAbilityAuto<At_Mon_LavaLand_Magmadon_Charge>();
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
		if (((NetworkBehaviour)this).isServer)
		{
			ISpawnableAsMiniBoss.GiveGenericMiniBossBonus(this);
			Status.AddStatBonus(new StatBonus());
		}
	}

	private void MirrorProcessed()
	{
	}
}
