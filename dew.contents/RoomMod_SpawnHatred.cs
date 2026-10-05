public class RoomMod_SpawnHatred : RoomModifierBase
{
	public override void OnStartServer()
	{
		base.OnStartServer();
		SingletonDewNetworkBehaviour<Room>.instance.rewards.DisableRegularRewards();
		PlaceShrine<Shrine_Hatred>(new PlaceShrineSettings
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
