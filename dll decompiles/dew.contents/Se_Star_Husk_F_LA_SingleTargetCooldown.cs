using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Se_Star_Husk_F_LA_SingleTargetCooldown : StarEffect
{
	public float cooldownReduction = 0.4f;

	public GameObject fxActivate;

	private AbilityInstance _subscribedInstance;

	private Action<EventInfoDamage> _onInstanceDealDamage;

	private Action<Actor> _onInstanceDestroyed;

	public override Type heroType => typeof(Hero_Husk);

	public override Type skillType => typeof(St_Q_Laceration);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			hero.ActorEvent_OnAbilityInstanceBeforePrepare += new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
		}
	}

	private void ActorEventOnAbilityInstanceBeforePrepare(EventInfoAbilityInstance obj)
	{
		AbilityInstance instance = obj.instance;
		if (!(instance is Ai_Q_Laceration_Circle) && !(instance is Ai_Q_Laceration_Dash))
		{
			return;
		}
		List<Entity> hitTargets = new List<Entity>();
		_subscribedInstance = obj.instance;
		_onInstanceDealDamage = (EventInfoDamage dmg) =>
		{
			if (!((UnityEngine.Object)(object)dmg.actor != (UnityEngine.Object)(object)obj.instance))
			{
				hitTargets.Add(dmg.victim);
			}
		};
		_onInstanceDestroyed = (Actor _) =>
		{
			if (hitTargets.Count == 1)
			{
				FxPlayNetworked(fxActivate, hero);
				obj.instance.ApplyCooldownReductionByRatio(obj.instance.firstTrigger, cooldownReduction);
			}
		};
		obj.instance.ActorEvent_OnDealDamage += _onInstanceDealDamage;
		obj.instance.ClientActorEvent_OnDestroyed += _onInstanceDestroyed;
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			if ((UnityEngine.Object)(object)hero != null)
			{
				hero.ActorEvent_OnAbilityInstanceBeforePrepare -= new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
			}
			if ((UnityEngine.Object)(object)_subscribedInstance != null)
			{
				_subscribedInstance.ActorEvent_OnDealDamage -= _onInstanceDealDamage;
				_subscribedInstance.ClientActorEvent_OnDestroyed -= _onInstanceDestroyed;
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
