using Mirror;
using UnityEngine;

public class Gem_C_Sulfur : Gem
{
	public ScalingValue ampAmount;

	public GameObject hitEffect;

	public ScalingValue fireAmpBonus;

	private StatBonus _stats;

	public override void OnEquipGem(Hero newOwner)
	{
		base.OnEquipGem(newOwner);
		if (((NetworkBehaviour)this).isServer)
		{
			_stats = newOwner.Status.AddStatBonus(new StatBonus
			{
				fireEffectAmpFlat = GetValue(fireAmpBonus)
			});
		}
	}

	protected override void OnQualityChange(int oldQuality, int newQuality)
	{
		base.OnQualityChange(oldQuality, newQuality);
		if (((NetworkBehaviour)this).isServer && _stats != null)
		{
			_stats.fireEffectAmpFlat = GetValue(fireAmpBonus);
		}
	}

	public override void OnUnequipGem(Hero oldOwner)
	{
		base.OnUnequipGem(oldOwner);
		if (((NetworkBehaviour)this).isServer && (Object)(object)oldOwner != null && _stats != null)
		{
			oldOwner.Status.RemoveStatBonus(_stats);
			_stats = null;
		}
	}

	public override void OnEquipSkill(SkillTrigger newSkill)
	{
		base.OnEquipSkill(newSkill);
		if (((NetworkBehaviour)this).isServer)
		{
			newSkill.dealtDamageProcessor.Add(Setablazee, -2000);
		}
	}

	public override void OnUnequipSkill(SkillTrigger oldSkill)
	{
		base.OnUnequipSkill(oldSkill);
		if (((NetworkBehaviour)this).isServer && !((Object)(object)oldSkill == null))
		{
			oldSkill.dealtDamageProcessor.Remove(Setablazee);
		}
	}

	private void Setablazee(ref DamageData data, Actor actor, Entity target)
	{
		if (owner.CheckEnemyOrNeutral(target))
		{
			data.SetElemental(ElementalType.Fire);
			NotifyUse();
			if (!data.IsAmountModifiedBy(this))
			{
				data.ApplyAmplification(GetValue(ampAmount));
				data.SetAmountModifiedBy(this);
				FxPlayNewNetworked(hitEffect, target);
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
