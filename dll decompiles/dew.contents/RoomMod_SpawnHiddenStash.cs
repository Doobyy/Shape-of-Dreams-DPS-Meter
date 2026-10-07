using Mirror;

public class RoomMod_SpawnHiddenStash : RoomModifierBase
{
	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			PlaceShrine<Shrine_HiddenStash>(new PlaceShrineSettings
			{
				removeModifierOnUse = true,
				lockedUntilCleared = true,
				spawnOnLastSection = true
			});
		}
	}

	private void MirrorProcessed()
	{
	}
}
