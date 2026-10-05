using System;
using Mirror;
using UnityEngine;

public class Se_Star_Cetus_L_HealOnApDamage : StarEffect
{
	public StarScalingValue healAmount;

	public override Type heroType => typeof(Hero_Cetus);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			hero.ActorEvent_OnDealDamage += new Action<EventInfoDamage>(ActorEventOnDealDamage);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)hero != null)
		{
			hero.ActorEvent_OnDealDamage -= new Action<EventInfoDamage>(ActorEventOnDealDamage);
		}
	}

	private void ActorEventOnDealDamage(EventInfoDamage obj)
	{
		if (!obj.chain.DidReact(this) && obj.damage.type == DamageData.SourceType.Magic && hero.CheckEnemyOrNeutral(obj.victim))
		{
			Heal(obj.damage.amount * GetValue(healAmount)).Dispatch(hero, obj.chain.New(this));
		}
	}

	private void MirrorProcessed()
	{
	}
}
