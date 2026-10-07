using Mirror;

public class RoomMod_SmallSoul : RoomModifierBase
{
	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			PlaceShrine<Shrine_SmallSoul>(new PlaceShrineSettings
			{
				removeModifierOnUse = true
			});
		}
	}

	private void MirrorProcessed()
	{
	}
}
