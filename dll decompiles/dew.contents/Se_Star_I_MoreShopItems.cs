using System;
using Mirror;
using UnityEngine;

public class Se_Star_I_MoreShopItems : StarEffect
{
	public StarScalingValue addChance;

	private bool _didAdd;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			UpdateShopItemsCount();
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded += new Action<EventInfoLoadRoom>(ClientEventOnRoomLoaded);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			if ((UnityEngine.Object)(object)NetworkedManagerBase<ZoneManager>.instance != null)
			{
				NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded -= new Action<EventInfoLoadRoom>(ClientEventOnRoomLoaded);
			}
			if ((UnityEngine.Object)(object)player != null && _didAdd)
			{
				player.shopAddedItems--;
			}
		}
	}

	private void ClientEventOnRoomLoaded(EventInfoLoadRoom obj)
	{
		UpdateShopItemsCount();
	}

	private void UpdateShopItemsCount()
	{
		if (UnityEngine.Random.value < GetValue(addChance))
		{
			if (!_didAdd)
			{
				_didAdd = true;
				player.shopAddedItems++;
			}
		}
		else if (_didAdd)
		{
			_didAdd = false;
			player.shopAddedItems--;
		}
	}

	private void MirrorProcessed()
	{
	}
}
