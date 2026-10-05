using System;
using Mirror;
using UnityEngine;

public class St_E_Harvest : SkillTrigger
{
	protected override void OnEquip(Entity newOwner)
	{
		base.OnEquip(newOwner);
		if (((NetworkBehaviour)this).isServer)
		{
			newOwner.EntityEvent_OnAttackEffectTriggered += new Action<EventInfoAttackEffect>(EntityEventOnAttackEffectTriggered);
		}
	}

	private void EntityEventOnAttackEffectTriggered(EventInfoAttackEffect obj)
	{
		Se_E_Harvest_OnVictim se_E_Harvest_OnVictim = obj.victim.Status.FindStatusEffect((Se_E_Harvest_OnVictim se) => (UnityEngine.Object)(object)se.parentActor == (UnityEngine.Object)(object)this);
		if ((UnityEngine.Object)(object)se_E_Harvest_OnVictim == null)
		{
			se_E_Harvest_OnVictim = CreateStatusEffect<Se_E_Harvest_OnVictim>(obj.victim, new CastInfo(owner));
		}
		Se_E_Harvest_OnVictim se_E_Harvest_OnVictim2 = se_E_Harvest_OnVictim;
		se_E_Harvest_OnVictim2.NetworkeffectCount = se_E_Harvest_OnVictim2.effectCount + 1;
		Se_E_Harvest_OnVictim se_E_Harvest_OnVictim3 = se_E_Harvest_OnVictim;
		se_E_Harvest_OnVictim3.NetworktotalStrength = se_E_Harvest_OnVictim3.totalStrength + obj.strength;
	}

	protected override void OnUnequip(Entity formerOwner)
	{
		base.OnUnequip(formerOwner);
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		if ((UnityEngine.Object)(object)formerOwner != null)
		{
			formerOwner.EntityEvent_OnAttackEffectTriggered -= new Action<EventInfoAttackEffect>(EntityEventOnAttackEffectTriggered);
		}
		Actor[] array = children.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] is Se_E_Harvest_OnVictim se_E_Harvest_OnVictim)
			{
				se_E_Harvest_OnVictim.Destroy();
			}
		}
	}

	public override bool CanBeReserved()
	{
		if (base.CanBeReserved())
		{
			return IsThereAnyChild();
		}
		return false;
	}

	public override bool CanBeCast()
	{
		if (base.CanBeCast())
		{
			return IsThereAnyChild();
		}
		return false;
	}

	private bool IsThereAnyChild()
	{
		foreach (Actor child in children)
		{
			if (child is Se_E_Harvest_OnVictim)
			{
				return true;
			}
		}
		return false;
	}

	private void MirrorProcessed()
	{
	}
}
