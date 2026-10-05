using Mirror;

public class Se_L_ButchersStrike_KillTimer : StatusEffect
{
	public float killGracePeriod = 3f;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			SetTimer(killGracePeriod);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && victim.IsNullInactiveDeadOrKnockedOut())
		{
			CreateAbilityInstance<Ai_L_BloodStrike_Meat>(victim.agentPosition, null, new CastInfo(info.caster));
		}
	}

	private void MirrorProcessed()
	{
	}
}
