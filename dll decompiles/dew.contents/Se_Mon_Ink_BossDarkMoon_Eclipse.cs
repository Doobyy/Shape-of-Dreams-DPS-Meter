using Mirror;

public class Se_Mon_Ink_BossDarkMoon_Eclipse : StatusEffect
{
	public bool enableInvulnerable;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer && enableInvulnerable)
		{
			DoInvulnerable();
		}
	}

	private void MirrorProcessed()
	{
	}
}
