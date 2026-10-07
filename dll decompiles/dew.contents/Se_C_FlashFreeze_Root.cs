using Mirror;

public class Se_C_FlashFreeze_Root : StatusEffect
{
	public float rootDuration;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DoRoot();
			SetTimer(rootDuration);
		}
	}

	private void MirrorProcessed()
	{
	}
}
