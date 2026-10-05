using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class St_D_HeartOfThePack : SkillTrigger
{
	private Dictionary<Entity, float> _lastApplyTime = new Dictionary<Entity, float>();

	public float perTargetCooldown => 0.25f;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded += new Action<EventInfoLoadRoom>(ClientEventOnRoomLoaded);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)NetworkedManagerBase<ZoneManager>.instance != null)
		{
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded -= new Action<EventInfoLoadRoom>(ClientEventOnRoomLoaded);
		}
	}

	private void ClientEventOnRoomLoaded(EventInfoLoadRoom obj)
	{
		_lastApplyTime.Clear();
	}

	protected override void OnEquip(Entity newOwner)
	{
		base.OnEquip(newOwner);
		if (((NetworkBehaviour)this).isServer)
		{
			newOwner.ActorEvent_OnDoHeal += new Action<EventInfoHeal>(ActorEventOnDoHeal);
			newOwner.ActorEvent_OnGiveShield += new Action<EventInfoShield>(ActorEventOnGiveShield);
			newOwner.ClientActorEvent_OnCreate += new Action<Actor>(ClientActorEventOnCreate);
		}
	}

	private void ActorEventOnGiveShield(EventInfoShield obj)
	{
		if (!owner.CheckEnemyOrNeutral(obj.target))
		{
			TryApplyCharge(obj.target);
		}
	}

	private void ActorEventOnDoHeal(EventInfoHeal obj)
	{
		if (!owner.CheckEnemyOrNeutral(obj.target))
		{
			TryApplyCharge(obj.target);
		}
	}

	private void ClientActorEventOnCreate(Actor obj)
	{
		if (obj is StatusEffect statusEffect && !(obj is Se_D_HeartOfThePack_Bond) && statusEffect.isBeneficialBuff && !owner.CheckEnemyOrNeutral(statusEffect.victim))
		{
			TryApplyCharge(statusEffect.victim);
		}
	}

	private void TryApplyCharge(Entity target)
	{
		if (!_lastApplyTime.TryGetValue(target, out var value) || !(Time.time - value < perTargetCooldown))
		{
			Se_D_HeartOfThePack_Bond se_D_HeartOfThePack_Bond = target.Status.FindStatusEffect((Se_D_HeartOfThePack_Bond candidate) => candidate.IsDescendantOf(this));
			if ((UnityEngine.Object)(object)se_D_HeartOfThePack_Bond != null)
			{
				se_D_HeartOfThePack_Bond.ResetTimer();
				return;
			}
			CreateStatusEffect<Se_D_HeartOfThePack_Bond>(target, new CastInfo(owner));
			_lastApplyTime[target] = Time.time;
		}
	}

	protected override void OnUnequip(Entity formerOwner)
	{
		base.OnUnequip(formerOwner);
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)formerOwner != null)
		{
			formerOwner.ActorEvent_OnDoHeal -= new Action<EventInfoHeal>(ActorEventOnDoHeal);
			formerOwner.ActorEvent_OnGiveShield -= new Action<EventInfoShield>(ActorEventOnGiveShield);
			formerOwner.ClientActorEvent_OnCreate -= new Action<Actor>(ClientActorEventOnCreate);
		}
	}

	private void MirrorProcessed()
	{
	}
}
