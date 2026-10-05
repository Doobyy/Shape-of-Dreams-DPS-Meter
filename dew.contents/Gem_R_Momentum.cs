using System;
using Mirror;
using UnityEngine;

public class Gem_R_Momentum : Gem
{
	public ScalingValue reductionAmount;

	public override void OnEquipGem(Hero newOwner)
	{
		base.OnEquipGem(newOwner);
		if (((NetworkBehaviour)this).isServer)
		{
			newOwner.EntityEvent_OnAttackEffectTriggered += new Action<EventInfoAttackEffect>(ReduceCooldown);
		}
	}

	public override void OnUnequipGem(Hero oldOwner)
	{
		base.OnUnequipGem(oldOwner);
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)oldOwner != null)
		{
			oldOwner.EntityEvent_OnAttackEffectTriggered -= new Action<EventInfoAttackEffect>(ReduceCooldown);
		}
	}

	private void ReduceCooldown(EventInfoAttackEffect obj)
	{
		if (!((UnityEngine.Object)(object)skill == null))
		{
			ApplyCooldownReduction(skill, GetValue(reductionAmount) * obj.strength);
			NotifyUse();
		}
	}

	private void MirrorProcessed()
	{
	}
}
