using System;
using Mirror;
using UnityEngine;

public class Se_R_Composure_Buff : StatusEffect
{
	public ScalingValue critBonus;

	public float duration;

	private Action<EventInfoDamage> _onTakeDamageCached;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DoStatBonus(new StatBonus
			{
				critChanceFlat = GetValue(critBonus)
			});
			SetTimer(duration);
			victim.EntityEvent_OnTakeDamage += new Action<EventInfoDamage>(EntityEventOnTakeDamage);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)victim != null)
		{
			victim.EntityEvent_OnTakeDamage -= new Action<EventInfoDamage>(EntityEventOnTakeDamage);
		}
	}

	private void EntityEventOnTakeDamage(EventInfoDamage obj)
	{
		Entity entity = obj.actor.firstEntity;
		if (!entity.IsNullInactiveDeadOrKnockedOut() && victim.CheckEnemyOrNeutral(entity))
		{
			Destroy();
		}
	}

	private void MirrorProcessed()
	{
	}
}
