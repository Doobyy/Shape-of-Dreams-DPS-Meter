using Mirror;

public class Ai_M_DreamyWaltz : Ai_GenericDodge
{
	public override bool reuseInRoom => true;

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && !info.caster.IsNullInactiveDeadOrKnockedOut())
		{
			CreateAbilityInstance<Ai_M_DreamyWaltz_BuffExplosion>(info.caster.position, null, new CastInfo(info.caster));
		}
	}

	private void MirrorProcessed()
	{
	}
}
