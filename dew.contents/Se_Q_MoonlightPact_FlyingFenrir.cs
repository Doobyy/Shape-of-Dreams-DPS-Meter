using Mirror;

public class Se_Q_MoonlightPact_FlyingFenrir : StatusEffect
{
	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DoUncollidable();
			DoInvulnerable();
		}
	}

	private void MirrorProcessed()
	{
	}
}
