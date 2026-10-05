using Mirror;

public class Ai_MirageSkin_Oppression_Explode : InstantDamageInstance
{
	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DestroyOnDestroy(parentActor);
		}
	}

	protected override void ActiveFrameUpdate()
	{
		base.ActiveFrameUpdate();
		position = info.caster.agentPosition;
	}

	private void MirrorProcessed()
	{
	}
}
