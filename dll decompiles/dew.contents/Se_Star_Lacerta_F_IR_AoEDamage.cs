using System;
using Mirror;
using UnityEngine;

public class Se_Star_Lacerta_F_IR_AoEDamage : StarEffect
{
	public override Type heroType => typeof(Hero_Lacerta);

	public override Type skillType => typeof(St_Q_IncendiaryRounds);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			victim.ActorEvent_OnAbilityInstanceBeforePrepare += new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)victim != null)
		{
			victim.ActorEvent_OnAbilityInstanceBeforePrepare -= new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
		}
	}

	private void ActorEventOnAbilityInstanceBeforePrepare(EventInfoAbilityInstance obj)
	{
		if (obj.instance is Ai_Q_IncendiaryRounds_Attack ai_Q_IncendiaryRounds_Attack)
		{
			ai_Q_IncendiaryRounds_Attack.doAreaOfEffectDamage = true;
		}
	}

	private void MirrorProcessed()
	{
	}
}
