public class RoomMod_SpawnDisintegration : RoomModifierBase
{
	public override void OnStartServer()
	{
		base.OnStartServer();
		PlaceShrine<Shrine_Disintegration>(new PlaceShrineSettings
		{
			removeModifierOnUse = false
		});
	}

	private void MirrorProcessed()
	{
	}
}
