using Mirror;

public class Se_R_DarkGrenade_Stun : StatusEffect
{
	public float stunDuration = 1f;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			SetTimer(stunDuration);
			DoStun();
		}
	}

	private void MirrorProcessed()
	{
	}
}
