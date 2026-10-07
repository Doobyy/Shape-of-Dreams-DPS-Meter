using Mirror;

public class RoomMod_TheConsortOfNight_Raven : RoomModifierBase
{
	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer && isNewInstance)
		{
			PlaceShrine<Shrine_TheConsortOfNight>(new PlaceShrineSettings
			{
				lockedUntilCleared = false,
				spawnOnLastSection = true,
				removeModifierOnUse = true
			});
		}
	}

	private void MirrorProcessed()
	{
	}
}
