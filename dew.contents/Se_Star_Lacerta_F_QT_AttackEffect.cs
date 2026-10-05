using System;
using Mirror;
using UnityEngine;

public class Se_Star_Lacerta_F_QT_AttackEffect : StarEffect
{
	public override Type heroType => typeof(Hero_Lacerta);

	public override Type skillType => typeof(St_R_QuickTrigger);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			victim.ActorEvent_OnAbilityInstanceBeforePrepare += new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
		}
	}

	private void ActorEventOnAbilityInstanceBeforePrepare(EventInfoAbilityInstance obj)
	{
		if (obj.instance is Ai_R_QuickTrigger_Projectile ai_R_QuickTrigger_Projectile)
		{
			ai_R_QuickTrigger_Projectile.doAttackEffect = true;
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

	private void MirrorProcessed()
	{
	}
}
