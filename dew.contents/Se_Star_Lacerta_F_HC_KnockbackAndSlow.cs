using System;
using Mirror;
using UnityEngine;

public class Se_Star_Lacerta_F_HC_KnockbackAndSlow : StarEffect
{
	public float slowAmount = 75f;

	public float slowDuration = 3f;

	public float scaleAmp = 0.25f;

	private Ai_Q_HandCannon _subscribedAi;

	private Action<EventInfoDamage> _onAiDealDamage;

	public override Type heroType => typeof(Hero_Lacerta);

	public override Type skillType => typeof(St_Q_HandCannon);

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
		if (((NetworkBehaviour)this).isServer)
		{
			if ((UnityEngine.Object)(object)victim != null)
			{
				victim.ActorEvent_OnAbilityInstanceBeforePrepare -= new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
			}
			if ((UnityEngine.Object)(object)_subscribedAi != null && _onAiDealDamage != null)
			{
				_subscribedAi.ActorEvent_OnDealDamage -= _onAiDealDamage;
			}
			_subscribedAi = null;
			_onAiDealDamage = null;
		}
	}

	private void ActorEventOnAbilityInstanceBeforePrepare(EventInfoAbilityInstance obj)
	{
		if (!(obj.instance is Ai_Q_HandCannon ai_Q_HandCannon))
		{
			return;
		}
		ai_Q_HandCannon.farRange.transform.localScale *= 1f + scaleAmp;
		ai_Q_HandCannon.closeRange.transform.localScale *= 1f + scaleAmp;
		ai_Q_HandCannon.startEffect.transform.localScale *= 1f + scaleAmp;
		if ((UnityEngine.Object)(object)_subscribedAi != null && _onAiDealDamage != null)
		{
			_subscribedAi.ActorEvent_OnDealDamage -= _onAiDealDamage;
		}
		_onAiDealDamage = (EventInfoDamage damage) =>
		{
			if (damage.actor is Ai_Q_HandCannon)
			{
				CreateBasicEffect(damage.victim, new SlowEffect
				{
					strength = slowAmount
				}, slowDuration);
			}
		};
		_subscribedAi = ai_Q_HandCannon;
		ai_Q_HandCannon.ActorEvent_OnDealDamage += _onAiDealDamage;
	}

	private void MirrorProcessed()
	{
	}
}
