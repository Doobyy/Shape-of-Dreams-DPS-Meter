using Mirror;

public class Ai_Mon_Special_BossPolaris_Monster_SlashDash : DashAttackInstance
{
	private Se_GenericEffectContainer _unstoppableEffect;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			CreateBasicEffect(info.caster, new UnstoppableEffect(), float.PositiveInfinity, "SlashDash").DestroyOnDestroy(this);
		}
	}

	protected override void OnHit(Entity entity)
	{
		base.OnHit(entity);
		CreateStatusEffect<Se_MiniBoss_BloodThorn_Bleeding>(entity);
	}

	private void MirrorProcessed()
	{
	}
}
