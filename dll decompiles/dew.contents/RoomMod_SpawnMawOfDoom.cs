public class RoomMod_SpawnMawOfDoom : RoomModifierBase
{
	public override void OnStartServer()
	{
		base.OnStartServer();
		SingletonDewNetworkBehaviour<Room>.instance.rewards.DisableRegularRewards();
		PlaceShrine<Shrine_MawOfDoom>(new PlaceShrineSettings
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
