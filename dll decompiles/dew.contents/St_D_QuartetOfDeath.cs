using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class St_D_QuartetOfDeath : SkillTrigger
{
	public GameObject fxCharged;

	public GameObject fxChargedOnlyOwner;

	private List<Se_D_QuartetOfDeath_DeathMark> _marks = new List<Se_D_QuartetOfDeath_DeathMark>();

	private bool _isExploded;

	protected override void OnEquip(Entity newOwner)
	{
		base.OnEquip(newOwner);
		if (((NetworkBehaviour)this).isServer)
		{
			newOwner.EntityEvent_OnAttackFired += new Action<EventInfoAttackFired>(CheckFourth);
			newOwner.EntityEvent_OnAttackHit += new Action<EventInfoAttackHit>(OnAttackHit);
			newOwner.ClientActorEvent_OnDestroyed += new Action<Actor>(OnDestroyed);
		}
	}

	protected override void OnUnequip(Entity formerOwner)
	{
		base.OnUnequip(formerOwner);
		if (((NetworkBehaviour)this).isServer)
		{
			formerOwner.EntityEvent_OnAttackFired -= new Action<EventInfoAttackFired>(CheckFourth);
			formerOwner.EntityEvent_OnAttackHit -= new Action<EventInfoAttackHit>(OnAttackHit);
			formerOwner.ClientActorEvent_OnDestroyed -= new Action<Actor>(OnDestroyed);
			FxStopNetworked(fxCharged);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			FxStopNetworked(fxCharged);
		}
	}

	private void OnDestroyed(Actor actor)
	{
		if (((NetworkBehaviour)this).isServer)
		{
			FxStopNetworked(fxCharged);
		}
	}

	private void CheckFourth(EventInfoAttackFired obj)
	{
		fillAmount = obj.everyFourAttackNormalizedProgress;
		_isExploded = obj.isThisAttackFourthAttack;
		if (obj.isNextAttackFourthAttack)
		{
			FxPlayNetworked(fxCharged, owner);
			FxPlayNewNetworked(fxChargedOnlyOwner, owner);
		}
		if (!obj.isThisAttackFourthAttack)
		{
			return;
		}
		FxStopNetworked(fxCharged);
		if (_marks.Count == 0)
		{
			return;
		}
		foreach (Se_D_QuartetOfDeath_DeathMark mark in _marks)
		{
			if (!mark.victim.IsNullInactiveDeadOrKnockedOut())
			{
				mark.OnExplosion();
			}
		}
		_marks.Clear();
		if (owner.Status.TryGetStatusEffect<Se_D_QuartetOfDeath_Buff>(out var effect))
		{
			effect.StackAndRefresh(1);
			return;
		}
		CreateStatusEffect(owner, new CastInfo(owner), (Se_D_QuartetOfDeath_Buff buff) =>
		{
			buff.currentStack = 1;
		});
	}

	private void OnAttackHit(EventInfoAttackHit obj)
	{
		if (!_isExploded)
		{
			if (obj.victim.Status.TryGetStatusEffect<Se_D_QuartetOfDeath_DeathMark>(out var effect))
			{
				effect.RefreshEffect();
				return;
			}
			effect = CreateStatusEffect<Se_D_QuartetOfDeath_DeathMark>(obj.victim, new CastInfo(owner, obj.victim));
			_marks.Add(effect);
		}
	}

	private void MirrorProcessed()
	{
	}
}
