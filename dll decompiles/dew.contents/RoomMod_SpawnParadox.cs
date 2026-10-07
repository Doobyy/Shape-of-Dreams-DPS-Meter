public class RoomMod_SpawnParadox : RoomModifierBase
{
	public override void OnStartServer()
	{
		base.OnStartServer();
		SingletonDewNetworkBehaviour<Room>.instance.rewards.DisableRegularRewards();
		PlaceShrine<Shrine_Paradox>(new PlaceShrineSettings
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
