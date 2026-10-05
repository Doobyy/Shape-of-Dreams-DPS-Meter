using UnityEngine;

public class RoomMod_StrayMemory_StartProp : RoomModifierBase
{
	public override void OnStartServer()
	{
		base.OnStartServer();
		if (!isNewInstance)
		{
			PlaceShrine<Shrine_StrayMemory_StartProp>(new PlaceShrineSettings
			{
				removeModifierOnUse = true
			});
		}
		else if (SingletonDewNetworkBehaviour<Room>.instance.didClearRoom)
		{
			SpawnShrine();
		}
		else
		{
			SingletonDewNetworkBehaviour<Room>.instance.onRoomClear.AddListener(SpawnShrine);
		}
	}

	public override void OnStopServer()
	{
		base.OnStopServer();
		if ((Object)(object)SingletonDewNetworkBehaviour<Room>.instance != null)
		{
			SingletonDewNetworkBehaviour<Room>.instance.onRoomClear.RemoveListener(SpawnShrine);
		}
	}

	private void SpawnShrine()
	{
		PlaceShrine<Shrine_StrayMemory_StartProp>(new PlaceShrineSettings
		{
			spawnOnLastSection = true,
			removeModifierOnUse = true
		});
	}

	private void MirrorProcessed()
	{
	}
}
