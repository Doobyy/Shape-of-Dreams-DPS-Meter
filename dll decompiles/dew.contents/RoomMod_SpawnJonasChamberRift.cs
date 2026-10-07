using System.Linq;
using Mirror;

public class RoomMod_SpawnJonasChamberRift : RoomModifierBase
{
	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer && isNewInstance)
		{
			string item = "Rift_Sidetrack_TheChamberOfJonas";
			if (!SingletonDewNetworkBehaviour<Room>.instance.rifts.openedSidetrackRifts.Contains(item))
			{
				SingletonDewNetworkBehaviour<Room>.instance.rifts.openedSidetrackRifts.Add(item);
			}
		}
	}

	public override bool IsAvailableInGame()
	{
		if (base.IsAvailableInGame())
		{
			return DewPlayer.gamePlayers.Any((DewPlayer p) => p.hasPolarisEndingUnlocked);
		}
		return false;
	}

	private void MirrorProcessed()
	{
	}
}
