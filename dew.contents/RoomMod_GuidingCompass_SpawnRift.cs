using Mirror;

public class RoomMod_GuidingCompass_SpawnRift : RoomModifierBase
{
	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			string item = "Rift_Sidetrack_BossPolaris";
			if (!SingletonDewNetworkBehaviour<Room>.instance.rifts.openedSidetrackRifts.Contains(item))
			{
				SingletonDewNetworkBehaviour<Room>.instance.rifts.openedSidetrackRifts.Add(item);
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
