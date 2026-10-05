using System;
using Mirror;
using UnityEngine;

public class Se_Star_D_TempAbilityPower : StarEffect
{
	public int buffedZoneCount = 2;

	public StarScalingValue bonusStatAmount;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DoStatBonus(new StatBonus
			{
				abilityPowerFlat = GetValue(bonusStatAmount)
			});
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnZoneLoaded += new Action<EventInfoLoadZone>(ClientEventOnZoneLoaded);
			ClientEventOnZoneLoaded(default);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)NetworkedManagerBase<ZoneManager>.instance != null)
		{
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnZoneLoaded -= new Action<EventInfoLoadZone>(ClientEventOnZoneLoaded);
		}
	}

	private void ClientEventOnZoneLoaded(EventInfoLoadZone obj)
	{
		if (NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex >= buffedZoneCount)
		{
			DestroyIfActive();
		}
	}

	private void MirrorProcessed()
	{
	}
}
