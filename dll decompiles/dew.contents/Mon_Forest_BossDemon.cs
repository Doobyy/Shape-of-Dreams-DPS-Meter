using System;
using Mirror;
using UnityEngine;

public class Mon_Forest_BossDemon : BossMonster
{
	public float startAbilityCooldown;

	public float delayedAtkChance = 0.35f;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		CreateBasicEffect(this, new UnstoppableEffect(), float.PositiveInfinity);
		foreach (PoolAbilityInfo ability in selectedPool.abilities)
		{
			AbilityTrigger triggerInstance = ability.triggerInstance;
			if (!(triggerInstance is At_Mon_Forest_BossDemon_DelayedAtk))
			{
				At_Mon_Forest_BossDemon_MainSkill trigger2;
				if (!(triggerInstance is At_Mon_Forest_BossDemon_MainSkill))
				{
					if (triggerInstance is At_Mon_Forest_BossDemon_Spawn && Ability.TryGetAbility<At_Mon_Forest_BossDemon_MainSkill>(out var trigger))
					{
						trigger.SetChargeAll(0);
						trigger.SetCooldownTimeAll(startAbilityCooldown);
					}
				}
				else if (Ability.TryGetAbility<At_Mon_Forest_BossDemon_MainSkill>(out trigger2))
				{
					trigger2.SetChargeAll(0);
					trigger2.SetCooldownTimeAll(startAbilityCooldown);
				}
			}
			else
			{
				ability.chance = delayedAtkChance * NetworkedManagerBase<GameManager>.instance.GetSpecialSkillChanceMultiplier();
			}
		}
	}

	protected override void AIUpdate(ref EntityAIContext context)
	{
		base.AIUpdate(ref context);
		if (!((UnityEngine.Object)(object)context.targetEnemy == null))
		{
			AI.Helper_ChaseTarget();
		}
	}

	public override Type GetUniqueReward()
	{
		return typeof(St_U_Hysteria);
	}

	private void MirrorProcessed()
	{
	}
}
