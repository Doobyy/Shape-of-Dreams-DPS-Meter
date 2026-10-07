public class RoomMod_SpawnBlessedGuidance : RoomModifierBase
{
	public override void OnStartServer()
	{
		base.OnStartServer();
		PlaceShrine<Shrine_BlessedGuidance>(new PlaceShrineSettings
		{
			removeModifierOnUse = true
		});
	}

	private void MirrorProcessed()
	{
	}
}
