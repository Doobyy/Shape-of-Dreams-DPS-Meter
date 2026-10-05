using System;
using System.Linq;
using Mirror;
using UnityEngine;

[SaveActor(true)]
public class Se_Gem_L_Culinary_Stack : StackedStatusEffect
{
	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			showIcon = true;
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded += new Action<EventInfoLoadRoom>(OnRoomLoaded);
			TrySpawnCookingPot();
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)NetworkedManagerBase<ZoneManager>.instance != null)
		{
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded -= new Action<EventInfoLoadRoom>(OnRoomLoaded);
		}
	}

	private void OnRoomLoaded(EventInfoLoadRoom obj)
	{
		TrySpawnCookingPot();
	}

	private void TrySpawnCookingPot()
	{
		if (!Shrine_CookingPot.instance.IsNullOrInactive())
		{
			return;
		}
		PropEnt_Merchant_Jonas jonas = NetworkedManagerBase<ActorManager>.instance.allActors.OfType<PropEnt_Merchant_Jonas>().FirstOrDefault();
		if (!((UnityEngine.Object)(object)jonas == null))
		{
			Vector3 normalized = (Rift.instance.position - ((Component)(object)jonas).transform.position).normalized;
			Dew.CreateActor((((Component)(object)jonas).transform.position + normalized * 1.5f).WithZ(((Component)(object)jonas).transform.position.z), null, null, (Shrine_CookingPot pot) =>
			{
				pot.potOwnJonas = jonas;
			});
		}
	}

	private void MirrorProcessed()
	{
	}
}
