using System;
using Mirror;
using UnityEngine;

public class Se_Star_Yubar_F_EI_BiggerExplosion : StarEffect
{
	public float scaleAmp = 0.4f;

	public float reducedCooldownRatioPerHit = 0.1f;

	public override Type heroType => typeof(Hero_Yubar);

	public override Type skillType => typeof(St_Q_EtherealInfluence);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			victim.ActorEvent_OnAbilityInstanceBeforePrepare += new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
			victim.ActorEvent_OnDealDamage += new Action<EventInfoDamage>(ActorEventOnDealDamage);
		}
	}

	private void ActorEventOnAbilityInstanceBeforePrepare(EventInfoAbilityInstance obj)
	{
		if (obj.instance is Ai_Q_EtherealInfluence ai_Q_EtherealInfluence)
		{
			ai_Q_EtherealInfluence.NetworkexplosionScaleMultiplier = 1f + scaleAmp;
		}
	}

	private void ActorEventOnDealDamage(EventInfoDamage obj)
	{
		if (obj.actor is Ai_Q_EtherealInfluence { firstTrigger: var abilityTrigger } && (bool)(UnityEngine.Object)(object)abilityTrigger)
		{
			ApplyCooldownReductionByRatio(abilityTrigger, reducedCooldownRatioPerHit);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)victim != null)
		{
			victim.ActorEvent_OnAbilityInstanceBeforePrepare -= new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
			victim.ActorEvent_OnDealDamage -= new Action<EventInfoDamage>(ActorEventOnDealDamage);
		}
	}

	private void MirrorProcessed()
	{
	}
}
