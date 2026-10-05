using UnityEngine;

public class Special_RoomAnnouncer : SingletonBehaviour<Special_RoomAnnouncer>
{
	public string key = "";

	[Space]
	public bool announceEvent = true;

	public Color color;

	public Sprite sprite;

	public float spriteScale = 1f;

	[Space]
	public bool setRoomName = true;

	public bool showZoneIndex;

	public int zoneIndexOffset;
}
