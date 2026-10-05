using Mirror;

public class Se_R_ShadowWalk_Disappear : StatusEffect
{
	public float disappearDuration = 0.25f;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			info.caster.Visual.DisableRenderers();
			DoUncollidable();
			DoInvulnerable();
			SetTimer(disappearDuration);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			info.caster.Visual.EnableRenderers();
		}
	}

	private void MirrorProcessed()
	{
	}
}
