using System;
using Mirror;
using UnityEngine;

public class Se_Star_Aurena_L_SpeedOnDamage : StarEffect
{
	public StarScalingValue speedAmount;

	public override Type heroType => typeof(Hero_Aurena);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			hero.EntityEvent_OnTakeDamage += new Action<EventInfoDamage>(EntityEventOnTakeDamage);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)hero != null)
		{
			hero.EntityEvent_OnTakeDamage -= new Action<EventInfoDamage>(EntityEventOnTakeDamage);
		}
	}

	private void EntityEventOnTakeDamage(EventInfoDamage obj)
	{
		if (hero.Status.TryGetStatusEffect<Se_Star_Aurena_L_SpeedOnDamage_Speed>(out var effect))
		{
			effect.ResetTimer();
			return;
		}
		CreateStatusEffect(hero, (Se_Star_Aurena_L_SpeedOnDamage_Speed se) =>
		{
			se.amount = GetValue(speedAmount);
		});
	}

	private void MirrorProcessed()
	{
	}
}
