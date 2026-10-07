using System;
using Mirror;
using UnityEngine;

public class Se_Star_L_PotionOnBuyOrSell : StarEffect
{
	public StarScalingValue maxUses;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			NetworkedManagerBase<ClientEventManager>.instance.OnItemBought += new Action<Hero, NetworkBehaviour>(OnItem);
			NetworkedManagerBase<ClientEventManager>.instance.OnItemSold += new Action<Hero, NetworkBehaviour>(OnItem);
		}
	}

	private void OnItem(Hero h, NetworkBehaviour arg2)
	{
		if ((UnityEngine.Object)(object)h != (UnityEngine.Object)(object)hero)
		{
			return;
		}
		PropEnt_Merchant_Base propEnt_Merchant_Base = Dew.FindActorOfType<PropEnt_Merchant_Base>();
		if (!((UnityEngine.Object)(object)propEnt_Merchant_Base == null))
		{
			int valueInt = GetValueInt(maxUses);
			int dataOrDefault = propEnt_Merchant_Base.persistentData.GetDataOrDefault("Se_Star_L_PotionOnBuyOrSell", "uses", player.guid, 0);
			if (dataOrDefault < valueInt)
			{
				propEnt_Merchant_Base.persistentData.SetData("Se_Star_L_PotionOnBuyOrSell", "uses", player.guid, dataOrDefault + 1);
				CreatePickupInstance<Pickup_RegenOrb>(propEnt_Merchant_Base.agentPosition, null, new CastInfo(hero));
			}
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)NetworkedManagerBase<ClientEventManager>.instance != null)
		{
			NetworkedManagerBase<ClientEventManager>.instance.OnItemBought -= new Action<Hero, NetworkBehaviour>(OnItem);
			NetworkedManagerBase<ClientEventManager>.instance.OnItemSold -= new Action<Hero, NetworkBehaviour>(OnItem);
		}
	}

	private void MirrorProcessed()
	{
	}
}
