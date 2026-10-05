using Mirror;

public class Ai_MirageSkin_Pulverization_Stomp : InstantDamageInstance
{
	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DestroyOnDestroy(parentActor);
			info.caster.Control.CancelOngoingChannels();
			info.caster.Control.StartDaze(damageDelay + 0.5f);
			info.caster.Control.Rotate(rotation, immediately: false, 0.5f);
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
