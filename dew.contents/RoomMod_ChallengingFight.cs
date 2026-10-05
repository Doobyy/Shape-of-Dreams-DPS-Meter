using Mirror;
using UnityEngine;

public class RoomMod_ChallengingFight : RoomModifierBase
{
	public float addedMirageChance = 0.2f;

	public float spawnedPopMult = 1.25f;

	private RoomSection _section;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		Rift_RoomExit exitPortal = Rift_RoomExit.instance;
		if (!((Object)(object)exitPortal == null))
		{
			_section = Dew.SelectBestWithScore(SingletonDewNetworkBehaviour<Room>.instance.sections, (RoomSection section, int i) => 0f - Vector2.Distance(((Component)(object)exitPortal).transform.position.ToXY(), section.transform.position.ToXY()));
			if (!(_section == null) && !_section.monsters.spawnMiniBossInstead)
			{
				_section.monsters.spawnMiniBossInstead = true;
				_section.monsters.miniBossCount = 2;
				SingletonDewNetworkBehaviour<Room>.instance.monsters.addedMirageChance += addedMirageChance;
				SingletonDewNetworkBehaviour<Room>.instance.monsters.spawnedPopMultiplier *= spawnedPopMult;
			}
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			if (_section != null)
			{
				_section.monsters.spawnMiniBossInstead = false;
				_section = null;
			}
			if ((Object)(object)SingletonDewNetworkBehaviour<Room>.instance != null)
			{
				SingletonDewNetworkBehaviour<Room>.instance.monsters.addedMirageChance -= addedMirageChance;
				SingletonDewNetworkBehaviour<Room>.instance.monsters.spawnedPopMultiplier /= spawnedPopMult;
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
