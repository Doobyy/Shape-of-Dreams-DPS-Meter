using System;
using Mirror;
using UnityEngine;

public class St_R_Somersault : SkillTrigger
{
	public ScalingValue cooldownReduction;

	protected override void OnEquip(Entity newOwner)
	{
		base.OnEquip(newOwner);
		if (((NetworkBehaviour)this).isServer)
		{
			newOwner.ActorEvent_OnAttackEffectTriggered += new Action<EventInfoAttackEffect>(OnAttackEffectTriggered);
		}
	}

	protected override void OnUnequip(Entity former)
	{
		base.OnUnequip(former);
		if (((NetworkBehaviour)this).isServer && !((UnityEngine.Object)(object)former == null))
		{
			former.ActorEvent_OnAttackEffectTriggered -= new Action<EventInfoAttackEffect>(OnAttackEffectTriggered);
		}
	}

	private void OnAttackEffectTriggered(EventInfoAttackEffect obj)
	{
		ApplyCooldownReduction(this, GetValue(cooldownReduction));
	}

	private void MirrorProcessed()
	{
	}
}
