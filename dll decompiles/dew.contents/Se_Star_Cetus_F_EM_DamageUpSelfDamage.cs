using System;
using Mirror;
using UnityEngine;

public class Se_Star_Cetus_F_EM_DamageUpSelfDamage : StarEffect
{
	public float damageAmp;

	public float takeDamageRatio;

	public override Type heroType => typeof(Hero_Cetus);

	public override Type skillType => typeof(St_Q_EmbracingTheChill);

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
		if (obj.instance is Ai_Q_EmbracingTheChill_AreaDoT ai_Q_EmbracingTheChill_AreaDoT)
		{
			ai_Q_EmbracingTheChill_AreaDoT.doSelfDamage = true;
			ai_Q_EmbracingTheChill_AreaDoT.damageAmp = damageAmp;
			ai_Q_EmbracingTheChill_AreaDoT.takeDamageRatio = takeDamageRatio;
		}
	}

	private void MirrorProcessed()
	{
	}
}
