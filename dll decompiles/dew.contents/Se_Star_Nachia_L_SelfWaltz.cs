using System;
using Mirror;

public class Se_Star_Nachia_L_SelfWaltz : StarEffect
{
	public StarScalingValue selfMultiplier;

	public override Type heroType => typeof(Hero_Nachia);

	public override bool isMovementSkillType => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			hero.ActorEvent_OnAbilityInstanceBeforePrepare += new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			hero.ActorEvent_OnAbilityInstanceBeforePrepare -= new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
		}
	}

	private void ActorEventOnAbilityInstanceBeforePrepare(EventInfoAbilityInstance obj)
	{
		if (obj.instance is Ai_M_DreamyWaltz_BuffExplosion ai_M_DreamyWaltz_BuffExplosion)
		{
			ai_M_DreamyWaltz_BuffExplosion.selfMultiplier = GetValue(selfMultiplier);
		}
	}

	private void MirrorProcessed()
	{
	}
}
