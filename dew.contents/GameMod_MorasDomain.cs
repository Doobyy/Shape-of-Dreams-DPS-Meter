using UnityEngine;

public class GameMod_MorasDomain : GameModifierBase
{
	public Rift_Sidetrack normalRift;

	public Rift_Sidetrack bossRift;

	public Vector2Int huntedRoomIndexRange;

	private int _visitedHuntedRooms;

	protected override void OnCreate()
	{
	}

	protected override void OnDestroyActor()
	{
	}

	private void ClientEventOnZoneLoaded(EventInfoLoadZone obj)
	{
	}

	private void ClientEventOnRoomLoaded(EventInfoLoadRoom _)
	{
	}

	private void MirrorProcessed()
	{
	}
}
