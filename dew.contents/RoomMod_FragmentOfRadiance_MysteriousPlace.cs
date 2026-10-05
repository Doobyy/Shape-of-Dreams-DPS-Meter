using Mirror;

public class RoomMod_FragmentOfRadiance_MysteriousPlace : RoomModifierBase
{
	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer && isNewInstance)
		{
			SingletonDewNetworkBehaviour<Room>.instance.rifts.openedSidetrackRifts.Clear();
			SingletonDewNetworkBehaviour<Room>.instance.rifts.openedSidetrackRifts.Add("Rift_Sidetrack_FragmentOfRadiance");
		}
	}

	private void MirrorProcessed()
	{
	}
}
