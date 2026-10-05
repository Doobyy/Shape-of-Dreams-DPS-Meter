using System;
using System.Collections;
using System.Globalization;
using Mirror;
using UnityEngine;
using UnityEngine.CrashReportHandler;

public class InGameCrashReportManager : ManagerBase<InGameCrashReportManager>
{
	public enum CustomParamKey
	{
		cheated,
		hero,
		room,
		room_index,
		room_mods,
		current_zone_cleared_nodes,
		cleared_combat_rooms,
		zone,
		zone_index,
		players,
		network,
		elapsed_seconds,
		equipment,
		local_hero_health_ratio
	}

	public override bool shouldRegisterUpdates => false;

	private void Set(CustomParamKey key, string value)
	{
		CrashReportHandler.SetUserMetadata(key.ToString(), value);
	}

	private void Set(CustomParamKey key, float value)
	{
		CrashReportHandler.SetUserMetadata(key.ToString(), value.ToString(CultureInfo.InvariantCulture));
	}

	private void Set(CustomParamKey key, int value)
	{
		CrashReportHandler.SetUserMetadata(key.ToString(), value.ToString(CultureInfo.InvariantCulture));
	}

	private void Start()
	{
		GameManager.CallOnReady(() =>
		{
			DewPlayer.onHumanPlayerAdded += new Action<DewPlayer>(UpdatePlayerCount);
			DewPlayer.onHumanPlayerRemoved += new Action<DewPlayer>(UpdatePlayerCount);
			NetworkedManagerBase<ConsoleManager>.instance.ClientEvent_OnCheatEnabledChanged += new Action<bool>(UpdateCheated);
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnZoneLoaded += new Action<EventInfoLoadZone>(UpdateZone);
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded += new Action<EventInfoLoadRoom>(UpdateRoom);
			DewPlayer.local.ClientEvent_OnHeroChanged += new Action<Hero, Hero>(UpdateHero);
			UpdatePlayerCount(null);
			Set(CustomParamKey.cheated, 0);
			UpdateZone(default);
			UpdateRoom(default);
			UpdateHero(null, null);
			if (NetworkServer.dontListen)
			{
				Set(CustomParamKey.network, "solo");
			}
			else
			{
				Set(CustomParamKey.network, NetworkServer.active ? "host" : "client");
			}
			StartCoroutine(Routine());
		});
		IEnumerator Routine()
		{
			while (true)
			{
				if ((UnityEngine.Object)(object)NetworkedManagerBase<GameManager>.softInstance != null)
				{
					Set(CustomParamKey.elapsed_seconds, (int)NetworkedManagerBase<GameManager>.softInstance.elapsedGameTime);
				}
				if ((UnityEngine.Object)(object)DewPlayer.local != null && (UnityEngine.Object)(object)DewPlayer.local.hero != null)
				{
					Set(CustomParamKey.local_hero_health_ratio, DewPlayer.local.hero.currentHealth / DewPlayer.local.hero.maxHealth);
				}
				yield return new WaitForSeconds(1f);
			}
		}
	}

	private void UpdateHero(Hero _, Hero to)
	{
		try
		{
			if (!((UnityEngine.Object)(object)to == null))
			{
				Set(CustomParamKey.hero, ((object)DewPlayer.local.hero).GetType().Name);
			}
		}
		catch (Exception message)
		{
			Debug.Log(message);
		}
	}

	private void UpdateRoom(EventInfoLoadRoom _)
	{
		try
		{
			if (!((UnityEngine.Object)(object)DewPlayer.local.hero == null))
			{
				string text = "";
				int count = NetworkedManagerBase<ZoneManager>.instance.currentNode.modifiers.Count;
				for (int i = 0; i < count; i++)
				{
					ModifierData modifierData = NetworkedManagerBase<ZoneManager>.instance.currentNode.modifiers[i];
					text = ((i != count - 1) ? (text + modifierData.type.Replace("RoomMod_", "") + ",") : (text + modifierData.type.Replace("RoomMod_", "")));
				}
				Set(CustomParamKey.room, ((UnityEngine.Object)(object)SingletonDewNetworkBehaviour<Room>.instance).name);
				Set(CustomParamKey.room_index, NetworkedManagerBase<ZoneManager>.instance.currentRoomIndex);
				Set(CustomParamKey.current_zone_cleared_nodes, NetworkedManagerBase<ZoneManager>.instance.currentZoneClearedNodes);
				Set(CustomParamKey.cleared_combat_rooms, NetworkedManagerBase<ZoneManager>.instance.clearedCombatRooms);
				Set(CustomParamKey.room_mods, text);
				Set(CustomParamKey.equipment, new AnalyticsEquipmentData(DewPlayer.local.hero).ToBase64());
			}
		}
		catch (Exception message)
		{
			Debug.Log(message);
		}
	}

	private void UpdateZone(EventInfoLoadZone obj)
	{
		try
		{
			Set(CustomParamKey.zone, NetworkedManagerBase<ZoneManager>.instance.currentZone.name);
			Set(CustomParamKey.zone_index, NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex);
		}
		catch (Exception message)
		{
			Debug.Log(message);
		}
	}

	private void UpdateCheated(bool obj)
	{
		try
		{
			if (obj)
			{
				Set(CustomParamKey.cheated, 1);
			}
		}
		catch (Exception message)
		{
			Debug.Log(message);
		}
	}

	private void UpdatePlayerCount(DewPlayer _)
	{
		try
		{
			Set(CustomParamKey.players, DewPlayer.gamePlayers.Count);
		}
		catch (Exception message)
		{
			Debug.Log(message);
		}
	}

	private void OnDestroy()
	{
		DewPlayer.onHumanPlayerAdded -= new Action<DewPlayer>(UpdatePlayerCount);
		DewPlayer.onHumanPlayerRemoved -= new Action<DewPlayer>(UpdatePlayerCount);
		try
		{
			CustomParamKey[] array = (CustomParamKey[])Enum.GetValues(typeof(CustomParamKey));
			foreach (CustomParamKey key in array)
			{
				Set(key, null);
			}
		}
		catch (Exception message)
		{
			Debug.Log(message);
		}
	}
}
