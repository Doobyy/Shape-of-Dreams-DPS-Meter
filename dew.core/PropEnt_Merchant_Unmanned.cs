using Mirror;

public class PropEnt_Merchant_Unmanned : PropEnt_Merchant_Jonas
{
	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			CreateBasicEffect(this, new UntargetableEffect(), float.PositiveInfinity);
			CreateBasicEffect(this, new InvisibleEffect
			{
				ignoreReveal = true
			}, float.PositiveInfinity);
			CreateBasicEffect(this, new InvulnerableEffect(), float.PositiveInfinity);
			CreateBasicEffect(this, new UncollidableEffect(), float.PositiveInfinity);
		}
	}

	private void MirrorProcessed()
	{
	}
}
