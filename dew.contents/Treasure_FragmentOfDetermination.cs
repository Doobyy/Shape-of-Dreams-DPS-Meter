using Mirror;

public class Treasure_FragmentOfDetermination : Treasure
{
	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			CreateStatusEffect<Se_Treasure_FragmentOfDetermination_Interrupt>(hero, new CastInfo(hero));
			Destroy();
		}
	}

	private void MirrorProcessed()
	{
	}
}
