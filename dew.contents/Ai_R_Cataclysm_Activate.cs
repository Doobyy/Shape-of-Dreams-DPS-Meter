using Mirror;

public class Ai_R_Cataclysm_Activate : AbilityInstance
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
