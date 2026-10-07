using Mirror;
using UnityEngine;

public class RoomMod_SpawnMiniBoss : RoomModifierBase, IPreemptiveMiniBossSection
{
	private RoomSection _section;

	public RoomSection GetTargetSection(Room room)
	{
		if ((Object)(object)room == null)
		{
			return null;
		}
		Rift_RoomExit exitPortal = Rift_RoomExit.instance;
		if ((Object)(object)exitPortal == null)
		{
			return null;
		}
		return Dew.SelectBestWithScore(room.sections, (RoomSection section, int i) => 0f - Vector2.Distance(((Component)(object)exitPortal).transform.position.ToXY(), section.transform.position.ToXY()));
	}

	public int GetMiniBossCount()
	{
		return 1;
	}

	public override void OnStartServer()
	{
		base.OnStartServer();
		_section = GetTargetSection(SingletonDewNetworkBehaviour<Room>.instance);
		if (!(_section == null) && !_section.monsters.spawnMiniBossInstead)
		{
			_section.monsters.spawnMiniBossInstead = true;
			_section.monsters.miniBossCount = GetMiniBossCount();
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && _section != null)
		{
			_section.monsters.spawnMiniBossInstead = false;
			_section = null;
		}
	}

	private void MirrorProcessed()
	{
	}
}
