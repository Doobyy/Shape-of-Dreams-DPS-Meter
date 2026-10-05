using Mirror;

public class Treasure_TokenOfGuidance : Treasure
{
	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			CreateStatusEffect<Se_Treasure_TokenOfGuidance_Heal>(hero, new CastInfo(hero));
			Destroy();
		}
	}

	private void MirrorProcessed()
	{
	}
}
