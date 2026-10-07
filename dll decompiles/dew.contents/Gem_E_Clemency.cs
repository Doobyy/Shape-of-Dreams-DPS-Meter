using System;
using Mirror;
using UnityEngine;

public class Gem_E_Clemency : Gem
{
	public GameObject healEffect;

	public ScalingValue conversionRatio;

	public override void OnEquipSkill(SkillTrigger newSkill)
	{
		base.OnEquipSkill(newSkill);
		if (((NetworkBehaviour)this).isServer)
		{
			newSkill.ActorEvent_OnDealDamage += new Action<EventInfoDamage>(ConvertToHeal);
		}
	}

	private void ConvertToHeal(EventInfoDamage obj)
	{
		if (!((UnityEngine.Object)(object)owner == null) && !obj.chain.DidReact(this) && owner.CheckEnemyOrNeutral(obj.victim))
		{
			Heal(obj.damage.amount * GetValue(conversionRatio)).SetAmountOrigin(obj.damage).Dispatch(owner, obj.chain.New(this));
			FxPlayNewNetworked(healEffect, owner);
			NotifyUse();
		}
	}

	public override void OnUnequipSkill(SkillTrigger oldSkill)
	{
		base.OnUnequipSkill(oldSkill);
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)oldSkill != null)
		{
			oldSkill.ActorEvent_OnDealDamage -= new Action<EventInfoDamage>(ConvertToHeal);
		}
	}

	private void MirrorProcessed()
	{
	}
}
