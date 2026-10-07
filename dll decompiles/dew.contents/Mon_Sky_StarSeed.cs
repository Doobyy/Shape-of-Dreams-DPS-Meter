using Mirror;
using UnityEngine;

public class Mon_Sky_StarSeed : Monster, ISpawnableAsMiniBoss
{
	public float selfDestructTime = 6f;

	public bool canSelfDestruct;

	protected override void AIUpdate(ref EntityAIContext context)
	{
		base.AIUpdate(ref context);
		if (canSelfDestruct && Time.time - creationTime > selfDestructTime && AI.Helper_CanBeCast<At_Mon_Sky_StarSeed_SelfDestruct>())
		{
			AI.Helper_CastAbilityAuto<At_Mon_Sky_StarSeed_SelfDestruct>();
		}
		else if (!((Object)(object)context.targetEnemy == null))
		{
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
			At_Mon_Sky_StarSeed_Atk ability = Ability.GetAbility<At_Mon_Sky_StarSeed_Atk>();
			ability.configs[0].channel.duration *= 1.55f;
			ability.NetworkisMegaExplosion = true;
			Status.AddStatBonus(new StatBonus
			{
				movementSpeedPercentage = 100f
			});
		}
	}

	private void MirrorProcessed()
	{
	}
}
