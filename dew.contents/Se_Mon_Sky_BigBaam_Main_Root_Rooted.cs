using Mirror;

public class Se_Mon_Sky_BigBaam_Main_Root_Rooted : StatusEffect
{
	public float duration;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DoRoot();
			SetTimer(duration);
		}
	}

	private void MirrorProcessed()
	{
	}
}
