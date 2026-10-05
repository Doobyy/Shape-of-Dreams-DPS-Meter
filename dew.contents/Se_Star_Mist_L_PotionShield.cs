using System;
using Mirror;
using UnityEngine;

public class Se_Star_Mist_L_PotionShield : StarEffect
{
	public StarScalingValue shieldRatio;

	public override Type heroType => typeof(Hero_Mist);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			victim.EntityEvent_OnTakeHeal += new Action<EventInfoHeal>(EntityEventOnTakeHeal);
		}
	}

	private void EntityEventOnTakeHeal(EventInfoHeal obj)
	{
		if (!(obj.actor is Se_GenericHealOverTime) || !(obj.actor.parentActor is Ai_RegenOrb_Projectile))
		{
			return;
		}
		float am = (obj.amount + obj.discardedAmount) * GetValue(shieldRatio);
		if (!victim.Status.TryGetStatusEffect<Se_Star_Mist_L_PotionShield_PersistentShield>(out var shield))
		{
			shield = CreateStatusEffect<Se_Star_Mist_L_PotionShield_PersistentShield>(victim);
		}
		Dew.CallDelayed(() =>
		{
			if (!shield.IsNullOrInactive())
			{
				shield.shield.AddAmount(am);
			}
		});
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)victim != null)
		{
			victim.EntityEvent_OnTakeHeal -= new Action<EventInfoHeal>(EntityEventOnTakeHeal);
		}
	}

	private void MirrorProcessed()
	{
	}
}
