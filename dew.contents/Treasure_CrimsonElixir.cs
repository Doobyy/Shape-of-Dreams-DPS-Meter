using Mirror;

public class Treasure_CrimsonElixir : Treasure
{
	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			CreateStatusEffect<Se_Treasure_CrimsonElixir_AdBoost>(hero, new CastInfo(hero));
			Destroy();
		}
	}

	private void MirrorProcessed()
	{
	}
}
