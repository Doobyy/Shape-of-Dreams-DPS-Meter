using System;
using Mirror;
using UnityEngine;

public class Se_Star_Yubar_F_CC_DamageOverTime : StarEffect
{
	public override Type heroType => typeof(Hero_Yubar);

	public override Type skillType => typeof(St_R_Cataclysm);

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
		if (obj.instance is Ai_R_Cataclysm_Meteor ai_R_Cataclysm_Meteor)
		{
			ai_R_Cataclysm_Meteor.spawnBurnInstance = true;
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
