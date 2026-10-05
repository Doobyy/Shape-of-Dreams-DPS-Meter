using System;
using Mirror;
using UnityEngine;

public class Se_Star_L_PeriodicScalingArmor : StarEffect
{
	public int zoneInterval = 3;

	public StarScalingValue bonusPerZoneInterval;

	private StatBonus _bonus;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			_bonus = DoStatBonus();
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnZoneLoaded += new Action<EventInfoLoadZone>(ClientEventOnZoneLoaded);
			UpdateStatBonus();
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && !((UnityEngine.Object)(object)NetworkedManagerBase<ZoneManager>.instance == null))
		{
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnZoneLoaded -= new Action<EventInfoLoadZone>(ClientEventOnZoneLoaded);
		}
	}

	private void ClientEventOnZoneLoaded(EventInfoLoadZone obj)
	{
		UpdateStatBonus();
	}

	private void UpdateStatBonus()
	{
		float num = 0f;
		int num2 = NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex + 1;
		if (!((UnityEngine.Object)(object)NetworkedManagerBase<ZoneManager>.instance == null))
		{
			num = GetValue(bonusPerZoneInterval) * (float)(num2 / zoneInterval);
			_bonus.armorFlat = num;
		}
	}

	private void MirrorProcessed()
	{
	}
}
