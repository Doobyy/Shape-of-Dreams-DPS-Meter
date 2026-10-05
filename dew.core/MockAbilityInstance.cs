using Mirror;

public class MockAbilityInstance : AbilityInstance
{
	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			Destroy();
		}
	}

	private void MirrorProcessed()
	{
	}
}
