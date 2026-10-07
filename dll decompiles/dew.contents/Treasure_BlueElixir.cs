using Mirror;

public class Treasure_BlueElixir : Treasure
{
	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			CreateStatusEffect<Se_Treasure_BlueElixir_ApBoost>(hero, new CastInfo(hero));
			Destroy();
		}
	}

	private void MirrorProcessed()
	{
	}
}
