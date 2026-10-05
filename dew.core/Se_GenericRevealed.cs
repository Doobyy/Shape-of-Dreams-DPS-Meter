using Mirror;

public class Se_GenericRevealed : StatusEffect
{
	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DoReveal();
			DestroyOnDeath(victim, includeKnockOuts: true);
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer && !victim.Status.isInvisibilityRevealed)
		{
			Destroy();
		}
	}

	private void MirrorProcessed()
	{
	}
}
