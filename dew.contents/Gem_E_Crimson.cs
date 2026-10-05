using System;
using Mirror;
using UnityEngine;

public class Gem_E_Crimson : Gem
{
	public GameObject fxActivate;

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
				CreateAbilityInstance(hit.victim.agentPosition, Quaternion.identity, new CastInfo(owner, hit.victim), (Ai_Gem_E_Crimson ai) =>
				{
					ai.NetworksizeMultiplier = 1f + owner.Status.critChance;
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
