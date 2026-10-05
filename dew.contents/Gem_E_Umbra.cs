using System;
using Mirror;
using UnityEngine;

public class Gem_E_Umbra : Gem
{
	public ScalingValue dmgAmpCritConversionRatio;

	public ScalingValue conversionRatio;

	public GameObject hitEffect;

	public GameObject healEffect;

	public override void OnEquipSkill(SkillTrigger newSkill)
	{
		base.OnEquipSkill(newSkill);
		if (((NetworkBehaviour)this).isServer)
		{
			newSkill.dealtDamageProcessor.Add(Amplify);
			newSkill.ActorEvent_OnDealDamage += new Action<EventInfoDamage>(ConvertToHeal);
		}
	}

	public override void OnUnequipSkill(SkillTrigger oldSkill)
	{
		base.OnUnequipSkill(oldSkill);
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)oldSkill != null)
		{
			oldSkill.dealtDamageProcessor.Remove(Amplify);
			oldSkill.ActorEvent_OnDealDamage -= new Action<EventInfoDamage>(ConvertToHeal);
		}
	}

	private void Amplify(ref DamageData data, Actor actor, Entity target)
	{
		if (!owner.CheckEnemyOrNeutral(target))
		{
			return;
		}
		FxPlayNewNetworked(hitEffect, target);
		if (!data.IsAmountModifiedBy(this))
		{
			float num = owner.Status.critChance * GetValue(dmgAmpCritConversionRatio);
			if (num > 0.25f)
			{
				data.SetAttr(DamageAttribute.IsCrit);
			}
			data.ApplyAmplification(num);
			data.SetAmountModifiedBy(this);
		}
	}

	private void ConvertToHeal(EventInfoDamage obj)
	{
		if (!((UnityEngine.Object)(object)owner == null) && !obj.chain.DidReact(this) && owner.CheckEnemyOrNeutral(obj.victim) && obj.damage.elemental == ElementalType.Dark)
		{
			Heal(obj.damage.amount * GetValue(conversionRatio)).SetAmountOrigin(obj.damage).Dispatch(owner, obj.chain.New(this));
			FxPlayNewNetworked(healEffect, owner);
			NotifyUse();
		}
	}

	private void MirrorProcessed()
	{
	}
}
