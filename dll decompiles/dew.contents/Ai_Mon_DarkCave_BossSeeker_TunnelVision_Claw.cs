using Mirror;

public class Ai_Mon_DarkCave_BossSeeker_TunnelVision_Claw : InstantDamageInstance
{
	public DewAnimationClip animPrepare;

	public DewAnimationClip animClaw;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			info.caster.Animation.PlayAbilityAnimation(animPrepare);
		}
	}

	protected override void OnAfterDelay()
	{
		base.OnAfterDelay();
		if (!info.caster.IsNullOrInactive())
		{
			info.caster.Animation.PlayAbilityAnimation(animClaw);
		}
	}

	private void MirrorProcessed()
	{
	}
}
