using Mirror;

public class RoomMod_SpawnGuidance : RoomModifierBase
{
	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			PlaceShrine<Shrine_Guidance>(new PlaceShrineSettings
			{
				removeModifierOnUse = true
			});
		}
	}

	private void MirrorProcessed()
	{
	}
}
