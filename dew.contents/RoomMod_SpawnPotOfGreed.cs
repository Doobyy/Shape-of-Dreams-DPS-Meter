public class RoomMod_SpawnPotOfGreed : RoomModifierBase
{
	public override void OnStartServer()
	{
		base.OnStartServer();
		PlaceShrine<Shrine_PotOfGreed>(new PlaceShrineSettings
		{
			removeModifierOnUse = true
		});
	}

	private void MirrorProcessed()
	{
	}
}
