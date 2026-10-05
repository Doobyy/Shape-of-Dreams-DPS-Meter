public class RoomMod_SpawnAscension : RoomModifierBase
{
	public override void OnStartServer()
	{
		base.OnStartServer();
		PlaceShrine<Shrine_Ascension>(new PlaceShrineSettings
		{
			spawnOnLastSection = true,
			lockedUntilCleared = true,
			removeModifierOnUse = false
		});
	}

	private void MirrorProcessed()
	{
	}
}
