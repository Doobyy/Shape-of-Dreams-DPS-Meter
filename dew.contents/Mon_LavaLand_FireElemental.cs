using Mirror;
using UnityEngine;

public class Mon_LavaLand_FireElemental : Monster, ISpawnableAsMiniBoss
{
	public float backdashChance;

	public float triggerBackdashDist;

	public float triggerHealthDestruct;

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
			if ((Object)(object)LavaLand_Lava.instance != null && LavaLand_Lava.instance.IsEntityOnLava(this) && !LavaLand_Lava.instance.IsEntityOnLava(context.targetEnemy))
			{
				Control.MoveToDestination(context.targetEnemy.position, immediately: false);
			}
			else if (Random.value < backdashChance && AI.Helper_CanBeCast<At_Mon_LavaLand_FireElemental_BackDash>() && Vector3.Distance(position, context.targetEnemy.position) < triggerBackdashDist)
			{
				AI.Helper_CastAbility<At_Mon_LavaLand_FireElemental_BackDash>(new CastInfo(this, context.targetEnemy));
			}
			else if (normalizedHealth <= triggerHealthDestruct && AI.Helper_CanBeCast<At_Mon_LavaLand_FireElemental_Destruct>())
			{
				AI.Helper_CastAbilityAuto<At_Mon_LavaLand_FireElemental_Destruct>();
			}
			else if (AI.Helper_CanBeCast<At_Mon_LavaLand_FireElemental_Explosion>())
			{
				AI.Helper_CastAbilityAuto<At_Mon_LavaLand_FireElemental_Explosion>();
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
			Ability.GetAbility<At_Mon_LavaLand_FireElemental_Explosion>().configs[0].cooldownTime *= 0.5f;
			Ability.GetAbility<At_Mon_LavaLand_FireElemental_Destruct>().configs[0].maxCharges = 0;
			Ability.attackAbility.configs[0].postDelay = 0.25f;
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
