using System;
using Mirror;
using UnityEngine;

public class Se_Star_Yubar_F_EI_NoRootMoreDamage : StarEffect
{
	public float damageAmp;

	public override Type heroType => typeof(Hero_Yubar);

	public override Type skillType => typeof(St_Q_EtherealInfluence);

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
		if (obj.instance is Ai_Q_EtherealInfluence ai_Q_EtherealInfluence)
		{
			ai_Q_EtherealInfluence.disableRoot = true;
			ai_Q_EtherealInfluence.explosionDamageMultiplier *= 1f + damageAmp;
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
