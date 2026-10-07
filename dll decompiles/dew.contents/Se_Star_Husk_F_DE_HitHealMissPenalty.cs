using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Se_Star_Husk_F_DE_HitHealMissPenalty : StarEffect
{
	public float healAmpByDmg;

	public ScalingValue penaltyAmp;

	private readonly List<(Actor target, Action<EventInfoDamage> onDeal, Action<Actor> onDestroyed)> _projectileSubscriptions = new List<(Actor, Action<EventInfoDamage>, Action<Actor>)>();

	public override Type heroType => typeof(Hero_Husk);

	public override Type skillType => typeof(St_Q_DeathMark);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			victim.EntityEvent_OnCastCompleteBeforePrepare += new Action<EventInfoCast>(EntityEventOnCastCompleteBeforePrepare);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		if ((UnityEngine.Object)(object)victim != null)
		{
			victim.EntityEvent_OnCastCompleteBeforePrepare -= new Action<EventInfoCast>(EntityEventOnCastCompleteBeforePrepare);
		}
		foreach (var projectileSubscription in _projectileSubscriptions)
		{
			if ((UnityEngine.Object)(object)projectileSubscription.target != null)
			{
				projectileSubscription.target.ActorEvent_OnDealDamage -= projectileSubscription.onDeal;
				projectileSubscription.target.ClientActorEvent_OnDestroyed -= projectileSubscription.onDestroyed;
			}
		}
		_projectileSubscriptions.Clear();
	}

	private void EntityEventOnCastCompleteBeforePrepare(EventInfoCast castObj)
	{
		if (!(castObj.instance is Ai_Q_DeathMark_Projectile))
		{
			return;
		}
		RefValue<bool> isHit = new RefValue<bool>(v: false);
		AbilityInstance instance = castObj.instance;
		Action<EventInfoDamage> action = (EventInfoDamage deal) =>
		{
			if (!(((object)deal.actor).GetType() != typeof(Ai_Q_DeathMark_Projectile)) && !isHit.value)
			{
				isHit.value = true;
				Heal(deal.damage.amount * healAmpByDmg).Dispatch(victim);
			}
		};
		Action<Actor> action2 = (Actor _) =>
		{
			if (!isHit.value)
			{
				PureDamage(penaltyAmp * victim.maxHealth).SetAttr(DamageAttribute.IgnoreShield).Dispatch(victim);
			}
		};
		instance.ActorEvent_OnDealDamage += action;
		instance.ClientActorEvent_OnDestroyed += action2;
		_projectileSubscriptions.Add((instance, action, action2));
	}

	private void MirrorProcessed()
	{
	}
}
