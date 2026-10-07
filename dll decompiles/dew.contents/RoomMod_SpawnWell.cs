public class RoomMod_SpawnWell : RoomModifierBase
{
	public override void OnStartServer()
	{
		base.OnStartServer();
		PlaceShrine<Shrine_UpgradeWell>(new PlaceShrineSettings
		{
			removeModifierOnUse = false
		});
	}

	private void MirrorProcessed()
	{
	}
}
