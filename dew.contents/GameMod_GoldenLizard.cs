using System;
using Mirror;
using UnityEngine;

public class GameMod_GoldenLizard : GameModifierBase
{
	public float defaultChance = 0.01f;

	[SaveVar(SaveVarFlags.Default)]
	private int _forcedSpawnZoneIndex = -1;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded += new Action<EventInfoLoadRoom>(ClientEventOnRoomLoaded);
		if (isNewInstance)
		{
			DateTime dateTime = new DateTime(2026, 2, 1);
			DateTime dateTime2 = new DateTime(2026, 4, 1);
			DateTime now = DateTime.Now;
			if (now >= dateTime && now <= dateTime2)
			{
				_forcedSpawnZoneIndex = UnityEngine.Random.Range(0, DewBuildProfile.current.content.zoneCountByTier.Count);
			}
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)NetworkedManagerBase<ZoneManager>.instance != null)
		{
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded -= new Action<EventInfoLoadRoom>(ClientEventOnRoomLoaded);
		}
	}

	private void ClientEventOnRoomLoaded(EventInfoLoadRoom obj)
	{
		if (obj.isTraveling && !NetworkedManagerBase<ZoneManager>.instance.currentZone.useSpecialGeneration && NetworkedManagerBase<ZoneManager>.instance.currentNode.type == WorldNodeType.Start && !SingletonDewNetworkBehaviour<Room>.instance.isRevisit && (!(UnityEngine.Random.value > defaultChance) || _forcedSpawnZoneIndex == NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex))
		{
			Dew.SpawnEntity<Mon_GoldenLizard_ElusiveLizard>(Rift.instance.position, Quaternion.Euler(0f, UnityEngine.Random.Range(0, 360), 0f), null, DewPlayer.environment, 1);
		}
	}

	private void MirrorProcessed()
	{
	}
}
