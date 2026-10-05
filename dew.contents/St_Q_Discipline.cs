using System;
using Mirror;
using UnityEngine;

public class St_Q_Discipline : SkillTrigger
{
	public float cooldownReductionRatio;

	protected override void OnEquip(Entity newOwner)
	{
		base.OnEquip(newOwner);
		if (((NetworkBehaviour)this).isServer)
		{
			newOwner.EntityEvent_OnAttackEffectTriggered += new Action<EventInfoAttackEffect>(EntityEventOnAttackEffectTriggered);
		}
	}

	protected override void OnUnequip(Entity formerOwner)
	{
		base.OnUnequip(formerOwner);
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)formerOwner != null)
		{
			formerOwner.EntityEvent_OnAttackEffectTriggered -= new Action<EventInfoAttackEffect>(EntityEventOnAttackEffectTriggered);
		}
	}

	private void EntityEventOnAttackEffectTriggered(EventInfoAttackEffect obj)
	{
		ApplyCooldownReductionByRatio(this, obj.strength * cooldownReductionRatio);
	}

	private void MirrorProcessed()
	{
	}
}
