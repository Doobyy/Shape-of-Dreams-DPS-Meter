using System;
using Mirror;
using UnityEngine;

public class Se_Star_Nachia_I_ForestShopDiscount : StarEffect
{
	public StarScalingValue discountAmount;

	private bool _didApply;

	public override Type heroType => typeof(Hero_Nachia);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnZoneLoaded += new Action<EventInfoLoadZone>(ClientEventOnZoneLoaded);
			ClientEventOnZoneLoaded(new EventInfoLoadZone
			{
				to = ((NetworkedManagerBase<ZoneManager>.instance.currentZone != null) ? NetworkedManagerBase<ZoneManager>.instance.currentZone.name : "")
			});
		}
	}

	private void ClientEventOnZoneLoaded(EventInfoLoadZone obj)
	{
		if (!((UnityEngine.Object)(object)player == null))
		{
			bool flag = obj.to == "Zone_Forest";
			if (flag && !_didApply)
			{
				_didApply = true;
				player.buyPriceMultiplier *= 1f - GetValue(discountAmount);
			}
			else if (!flag && _didApply)
			{
				_didApply = false;
				player.buyPriceMultiplier /= 1f - GetValue(discountAmount);
			}
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			if ((UnityEngine.Object)(object)NetworkedManagerBase<ZoneManager>.instance != null)
			{
				NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnZoneLoaded -= new Action<EventInfoLoadZone>(ClientEventOnZoneLoaded);
			}
			ClientEventOnZoneLoaded(default);
		}
	}

	private void MirrorProcessed()
	{
	}
}
