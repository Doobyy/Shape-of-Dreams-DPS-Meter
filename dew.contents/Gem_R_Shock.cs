using System;
using Mirror;
using UnityEngine;

public class Gem_R_Shock : Gem
{
	public GameObject fxActivate;

	public int baseHitCount = 2;

	public float critChanceForAdditionalHit;

	public int currentHitCount
	{
		get
		{
			if (!((UnityEngine.Object)(object)owner == null))
			{
				return Mathf.FloorToInt((owner.Status.critChance + 0.1f) / critChanceForAdditionalHit) + baseHitCount;
			}
			return baseHitCount;
		}
	}

	public override void OnEquipGem(Hero newOwner)
	{
		base.OnEquipGem(newOwner);
		if (((NetworkBehaviour)this).isServer)
		{
			newOwner.EntityEvent_OnAttackFired += new Action<EventInfoAttackFired>(CheckCritical);
		}
	}

	public override void OnUnequipGem(Hero oldOwner)
	{
		base.OnUnequipGem(oldOwner);
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)oldOwner != null)
		{
			oldOwner.EntityEvent_OnAttackFired -= new Action<EventInfoAttackFired>(CheckCritical);
		}
	}

	private void CheckCritical(EventInfoAttackFired obj)
	{
		if (!obj.isThisAttackFourthAttack)
		{
			return;
		}
		obj.instance.ActorEvent_OnAttackEffectTriggered += (Action<EventInfoAttackEffect>)((EventInfoAttackEffect hit) =>
		{
			if (hit.type == AttackEffectType.BasicAttackMain && isValid)
			{
				CreateAbilityInstance(owner.Visual.GetCenterPosition(), Quaternion.identity, new CastInfo(owner, hit.victim), (Ai_Gem_R_Shock_Lightning ai) =>
				{
					ai.maxHitCount = currentHitCount;
				});
			}
		});
		FxPlayNewNetworked(fxActivate, owner);
		NotifyUse();
	}

	private void MirrorProcessed()
	{
	}
}
