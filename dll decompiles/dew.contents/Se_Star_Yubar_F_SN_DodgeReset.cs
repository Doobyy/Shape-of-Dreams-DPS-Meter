using System;
using Mirror;
using UnityEngine;

public class Se_Star_Yubar_F_SN_DodgeReset : StarEffect
{
	public float reducedCooldownRatioOnHit = 0.08f;

	public float nextCastDamageAmpOnHit = 0.1f;

	[SaveVar(SaveVarFlags.Default)]
	private float _nextCastDamageAmp;

	public override Type heroType => typeof(Hero_Yubar);

	public override Type skillType => typeof(St_Q_SuperNova);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			hero.ActorEvent_OnDealDamage += new Action<EventInfoDamage>(ActorEventOnDealDamage);
			hero.EntityEvent_OnCastCompleteBeforePrepare += new Action<EventInfoCast>(EntityEventOnCastCompleteBeforePrepare);
			UpdateNumberDisplay();
		}
	}

	private void EntityEventOnCastCompleteBeforePrepare(EventInfoCast obj)
	{
		if (obj.instance is Ai_Q_SuperNova ai_Q_SuperNova && !(_nextCastDamageAmp < 0.0001f))
		{
			ai_Q_SuperNova.damage *= 1f + _nextCastDamageAmp;
			_nextCastDamageAmp = 0f;
			UpdateNumberDisplay();
		}
	}

	private void ActorEventOnDealDamage(EventInfoDamage obj)
	{
		if (obj.actor is Ai_Q_SuperNova)
		{
			AbilityTrigger abilityTrigger = obj.actor.firstTrigger;
			if ((bool)(UnityEngine.Object)(object)abilityTrigger && abilityTrigger is SkillTrigger trigger)
			{
				ApplyCooldownReductionByRatio(trigger, reducedCooldownRatioOnHit);
				_nextCastDamageAmp += nextCastDamageAmpOnHit;
				UpdateNumberDisplay();
			}
		}
	}

	private void UpdateNumberDisplay()
	{
		numberDisplay = Mathf.RoundToInt(_nextCastDamageAmp / nextCastDamageAmpOnHit);
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (bool)(UnityEngine.Object)(object)hero)
		{
			hero.ActorEvent_OnDealDamage -= new Action<EventInfoDamage>(ActorEventOnDealDamage);
			hero.EntityEvent_OnCastCompleteBeforePrepare -= new Action<EventInfoCast>(EntityEventOnCastCompleteBeforePrepare);
		}
	}

	private void MirrorProcessed()
	{
	}
}
