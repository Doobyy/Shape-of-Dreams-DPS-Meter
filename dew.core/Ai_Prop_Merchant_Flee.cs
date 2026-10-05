using Mirror;

public class Ai_Prop_Merchant_Flee : AbilityInstance
{
	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			info.caster.Destroy();
			Destroy();
		}
	}

	private void MirrorProcessed()
	{
	}
}
