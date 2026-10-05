using System;
using Mirror;
using UnityEngine;

public class Se_Star_Aurena_I_GoldOnMerchantDamage : StarEffect
{
	public int goldPerHit = 4;

	public StarScalingValue maxGold;

	public GameObject fxHit;

	public override Type heroType => typeof(Hero_Aurena);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			hero.ActorEvent_OnDealDamage += new Action<EventInfoDamage>(ActorEventOnDealDamage);
		}
	}

	private void ActorEventOnDealDamage(EventInfoDamage obj)
	{
		if (obj.victim is PropEnt_Merchant_Jonas)
		{
			int dataOrDefault = obj.victim.persistentData.GetDataOrDefault("Se_Star_Aurena_I_GoldOnMerchantDamage", "takenGold", player.guid, 0);
			int valueInt = GetValueInt(maxGold);
			if (dataOrDefault < valueInt)
			{
				int num = Mathf.Min(goldPerHit, valueInt - dataOrDefault);
				dataOrDefault += num;
				NetworkedManagerBase<PickupManager>.instance.DropGold(isKillGold: false, isGivenByOtherPlayer: false, num, obj.victim.agentPosition, hero);
				FxPlayNewNetworked(fxHit, obj.victim);
				obj.victim.persistentData.SetData("Se_Star_Aurena_I_GoldOnMerchantDamage", "takenGold", player.guid, dataOrDefault);
			}
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

	private void MirrorProcessed()
	{
	}
}
