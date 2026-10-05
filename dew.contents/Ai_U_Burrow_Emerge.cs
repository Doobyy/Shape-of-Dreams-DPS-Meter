using Mirror;

public class Ai_U_Burrow_Emerge : InstantDamageInstance
{
	public float stunDuration;

	public float dazeDuration = 0.1f;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			info.caster.Control.StartDaze(dazeDuration);
		}
	}

	protected override void OnHit(Entity entity)
	{
		base.OnHit(entity);
		CreateBasicEffect(entity, new StunEffect(), stunDuration);
	}

	private void MirrorProcessed()
	{
	}
}
