using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Se_Star_Nachia_L_AllyUnstoppable : StarEffect
{
	private Dictionary<Entity, float> _lastApplyTime = new Dictionary<Entity, float>();

	public StarScalingValue unstoppableTime;

	public float perTargetCooldown = 5f;

	public float ampToSummons = 0.7f;

	public override Type heroType => typeof(Hero_Nachia);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded += new Action<EventInfoLoadRoom>(ClientEventOnRoomLoaded);
			hero.ActorEvent_OnDoHeal += new Action<EventInfoHeal>(ActorEventOnDoHeal);
			hero.ActorEvent_OnGiveShield += new Action<EventInfoShield>(ActorEventOnGiveShield);
			hero.ClientActorEvent_OnCreate += new Action<Actor>(ClientActorEventOnCreate);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			if ((UnityEngine.Object)(object)NetworkedManagerBase<ZoneManager>.instance != null)
			{
				NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded -= new Action<EventInfoLoadRoom>(ClientEventOnRoomLoaded);
			}
			if ((UnityEngine.Object)(object)hero != null)
			{
				hero.ActorEvent_OnDoHeal -= new Action<EventInfoHeal>(ActorEventOnDoHeal);
				hero.ActorEvent_OnGiveShield -= new Action<EventInfoShield>(ActorEventOnGiveShield);
				hero.ClientActorEvent_OnCreate -= new Action<Actor>(ClientActorEventOnCreate);
			}
		}
	}

	private void ClientEventOnRoomLoaded(EventInfoLoadRoom obj)
	{
		_lastApplyTime.Clear();
	}

	private void ActorEventOnGiveShield(EventInfoShield obj)
	{
		if (!hero.CheckEnemyOrNeutral(obj.target))
		{
			TryApplyCharge(obj.target);
		}
	}

	private void ActorEventOnDoHeal(EventInfoHeal obj)
	{
		if (!hero.CheckEnemyOrNeutral(obj.target))
		{
			TryApplyCharge(obj.target);
		}
	}

	private void ClientActorEventOnCreate(Actor obj)
	{
		if (obj is StatusEffect statusEffect && !(obj is Se_D_HeartOfThePack_Bond) && statusEffect.isBeneficialBuff && !hero.CheckEnemyOrNeutral(statusEffect.victim))
		{
			TryApplyCharge(statusEffect.victim);
		}
	}

	private void TryApplyCharge(Entity target)
	{
		if (!_lastApplyTime.TryGetValue(target, out var value) || !(Time.time - value < perTargetCooldown))
		{
			float num = ((target is Summon) ? (1f + ampToSummons) : 1f);
			CreateBasicEffect(target, new UnstoppableEffect(), GetValue(unstoppableTime) * num);
			_lastApplyTime[target] = Time.time;
		}
	}

	private void MirrorProcessed()
	{
	}
}
