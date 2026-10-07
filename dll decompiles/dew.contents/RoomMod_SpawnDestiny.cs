public class RoomMod_SpawnDestiny : RoomModifierBase
{
	public override void OnStartServer()
	{
		base.OnStartServer();
		PlaceShrine<Shrine_Destiny>(new PlaceShrineSettings
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
