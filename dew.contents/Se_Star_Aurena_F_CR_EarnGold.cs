using System;
using Mirror;
using UnityEngine;

public class Se_Star_Aurena_F_CR_EarnGold : StarEffect
{
	public int earnedGoldPerSecond = 2;

	public override Type heroType => typeof(Hero_Aurena);

	public override Type skillType => typeof(St_R_ChainReaction);

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
		if (obj.actor is Ai_R_ChainReaction ai_R_ChainReaction)
		{
			int num = DewMath.RandomRoundToInt((float)earnedGoldPerSecond * ai_R_ChainReaction.tickInterval);
			if (num > 0)
			{
				NetworkedManagerBase<PickupManager>.instance.DropGold(isKillGold: false, isGivenByOtherPlayer: false, num, obj.victim.position, hero);
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
