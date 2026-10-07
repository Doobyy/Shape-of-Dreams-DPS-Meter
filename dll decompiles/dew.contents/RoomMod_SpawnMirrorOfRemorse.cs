public class RoomMod_SpawnMirrorOfRemorse : RoomModifierBase
{
	public override void OnStartServer()
	{
		base.OnStartServer();
		PlaceShrine<Shrine_MirrorOfRemorse>(new PlaceShrineSettings
		{
			removeModifierOnUse = false
		});
	}

	private void MirrorProcessed()
	{
	}
}
