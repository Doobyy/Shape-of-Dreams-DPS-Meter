using System;
using Mirror;
using UnityEngine;

public class Se_Star_Aurena_I_PotionGold : StarEffect
{
	public StarScalingValue goldAmount;

	public override Type heroType => typeof(Hero_Aurena);

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
		if (obj.actor is Se_GenericHealOverTime && obj.actor.parentActor is Ai_RegenOrb_Projectile)
		{
			NetworkedManagerBase<PickupManager>.instance.DropGold(isKillGold: false, isGivenByOtherPlayer: false, GetValueInt(goldAmount), hero.position, hero);
		}
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
