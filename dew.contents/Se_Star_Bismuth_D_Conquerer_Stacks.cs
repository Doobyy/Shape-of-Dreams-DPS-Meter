using System;
using Mirror;
using UnityEngine;

public class Se_Star_Bismuth_D_Conquerer_Stacks : StackedStatusEffect
{
	[NonSerialized]
	public float adPerStack;

	[NonSerialized]
	public float apPerStack;

	private StatBonus _bonus;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			_bonus = DoStatBonus();
			victim.EntityEvent_OnTakeDamage += new Action<EventInfoDamage>(EntityEventOnTakeDamage);
			victim.EntityEvent_OnAttackHit += new Action<EventInfoAttackHit>(EntityEventOnAttackHit);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)victim != null)
		{
			victim.EntityEvent_OnTakeDamage -= new Action<EventInfoDamage>(EntityEventOnTakeDamage);
			victim.EntityEvent_OnAttackHit -= new Action<EventInfoAttackHit>(EntityEventOnAttackHit);
		}
	}

	private void EntityEventOnTakeDamage(EventInfoDamage obj)
	{
		if (stack != 0)
		{
			Entity entity = obj.actor.firstEntity;
			if (!((UnityEngine.Object)(object)entity == null) && victim.CheckEnemyOrNeutral(entity))
			{
				SetStack(0);
			}
		}
	}

	private void EntityEventOnAttackHit(EventInfoAttackHit obj)
	{
		AddStack();
	}

	protected override void OnStackChange(int oldStack, int newStack)
	{
		base.OnStackChange(oldStack, newStack);
		if (((NetworkBehaviour)this).isServer && _bonus != null)
		{
			_bonus.attackDamageFlat = adPerStack * (float)newStack;
			_bonus.abilityPowerFlat = apPerStack * (float)newStack;
		}
	}

	private void MirrorProcessed()
	{
	}
}
