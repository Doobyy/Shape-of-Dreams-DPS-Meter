public class RoomMod_SpawnEntanglement : RoomModifierBase
{
	public override void OnStartServer()
	{
		base.OnStartServer();
		PlaceShrine<Shrine_Entanglement>(new PlaceShrineSettings
		{
			removeModifierOnUse = false
		});
	}

	private void MirrorProcessed()
	{
	}
}
