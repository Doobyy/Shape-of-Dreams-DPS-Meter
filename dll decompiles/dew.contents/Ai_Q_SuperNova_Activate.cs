using Mirror;

public class Ai_Q_SuperNova_Activate : AbilityInstance
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
