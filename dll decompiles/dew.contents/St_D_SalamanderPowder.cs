using System;
using Mirror;
using UnityEngine;

public class St_D_SalamanderPowder : SkillTrigger
{
	public DewCollider range;

	public AbilityTargetValidator hittable;

	public GameObject explodeEffect;

	public GameObject chargeEffectOnWeapon;

	public GameObject chargeEffectOnlyOwner;

	public ScalingValue atkSpdPercentage;

	private StatBonus _bonus;

	protected override void OnEquip(Entity newOwner)
	{
		base.OnEquip(newOwner);
		if (((NetworkBehaviour)this).isServer)
		{
			newOwner.EntityEvent_OnAttackFired += new Action<EventInfoAttackFired>(CheckCritical);
			UpdateStatBonus();
		}
	}

	protected override void OnUnequip(Entity formerOwner)
	{
		base.OnUnequip(formerOwner);
		if (((NetworkBehaviour)this).isServer)
		{
			formerOwner.EntityEvent_OnAttackFired -= new Action<EventInfoAttackFired>(CheckCritical);
			if (_bonus != null)
			{
				formerOwner.Status.RemoveStatBonus(_bonus);
				_bonus = null;
			}
		}
	}

	protected override void OnLevelChange(int oldLevel, int newLevel)
	{
		base.OnLevelChange(oldLevel, newLevel);
		if (((NetworkBehaviour)this).isServer)
		{
			UpdateStatBonus();
		}
	}

	private void UpdateStatBonus()
	{
		if (!((UnityEngine.Object)(object)owner == null))
		{
			if (_bonus == null)
			{
				_bonus = new StatBonus();
				owner.Status.AddStatBonus(_bonus);
			}
			_bonus.attackSpeedPercentage = GetValue(atkSpdPercentage);
		}
	}

	private void CheckCritical(EventInfoAttackFired obj)
	{
		fillAmount = obj.everyFourAttackNormalizedProgress;
		if (obj.isNextAttackFourthAttack)
		{
			AttackCriticalEffect be = new AttackCriticalEffect();
			be.onUse = () =>
			{
				FxStopNetworked(chargeEffectOnWeapon);
				be.parent.DestroyIfActive();
			};
			CreateBasicEffect(owner, be, float.PositiveInfinity, "SalamanderCrit");
			FxPlayNetworked(chargeEffectOnWeapon, owner);
			FxPlayNewNetworked(chargeEffectOnlyOwner, owner);
		}
		if (!obj.isThisAttackFourthAttack)
		{
			return;
		}
		obj.instance.ActorEvent_OnAttackHit += (Action<EventInfoAttackHit>)((EventInfoAttackHit hit) =>
		{
			range.transform.position = hit.victim.position;
			ListReturnHandle<Entity> handle;
			foreach (Entity entity in range.GetEntities(out handle, hittable, owner))
			{
				if (!((UnityEngine.Object)(object)entity != (UnityEngine.Object)(object)hit.victim) || !entity.AI.excludeFromAutoTargeting)
				{
					CreateAbilityInstance<Ai_D_SalamanderPowder_Projectile>(hit.victim.Visual.GetCenterPosition(), Quaternion.identity, new CastInfo(owner, entity));
				}
			}
			handle.Return();
			FxPlayNewNetworked(explodeEffect, hit.victim);
		});
	}

	private void MirrorProcessed()
	{
	}
}
