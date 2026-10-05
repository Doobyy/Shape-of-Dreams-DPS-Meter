using System;
using Mirror;
using UnityEngine;

public class Se_Star_L_HealOnAttack : StarEffect
{
	public StarScalingValue healRatio;

	public float critAmp = 1f;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			victim.EntityEvent_OnAttackHit += new Action<EventInfoAttackHit>(EntityEventOnAttackHit);
		}
	}

	private void EntityEventOnAttackHit(EventInfoAttackHit obj)
	{
		if (!obj.victim.Status.hasDamageImmunity)
		{
			float amount = victim.maxHealth * GetValue(healRatio) * obj.strength;
			HealData healData = Heal(amount).SetCanMerge();
			if (obj.isCrit)
			{
				healData.SetCrit();
				healData.ApplyAmplification(critAmp);
			}
			healData.Dispatch(victim);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)victim != null)
		{
			victim.EntityEvent_OnAttackHit -= new Action<EventInfoAttackHit>(EntityEventOnAttackHit);
		}
	}

	private void MirrorProcessed()
	{
	}
}
