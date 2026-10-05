using Mirror;
using UnityEngine;

public class Gem_E_Inversion : Gem
{
	public ScalingValue dmgAmp;

	public ScalingValue cooldownAmp;

	private SkillBonus _bonus;

	public override void OnEquipSkill(SkillTrigger newSkill)
	{
		base.OnEquipSkill(newSkill);
		if (((NetworkBehaviour)this).isServer)
		{
			newSkill.dealtDamageProcessor.Add(InvertAndAmp, -1000);
			_bonus = newSkill.AddSkillBonus(new SkillBonus
			{
				cooldownMultiplier = 1f + GetValue(cooldownAmp)
			});
		}
	}

	protected override void OnQualityChange(int oldQuality, int newQuality)
	{
		base.OnQualityChange(oldQuality, newQuality);
		if (((NetworkBehaviour)this).isServer && _bonus != null)
		{
			_bonus.cooldownMultiplier = 1f + GetValue(cooldownAmp);
		}
	}

	public override void OnUnequipSkill(SkillTrigger oldSkill)
	{
		base.OnUnequipSkill(oldSkill);
		if (((NetworkBehaviour)this).isServer && !((Object)(object)oldSkill == null))
		{
			oldSkill.dealtDamageProcessor.Remove(InvertAndAmp);
			if (_bonus != null)
			{
				_bonus.Stop();
				_bonus = null;
			}
		}
	}

	private void InvertAndAmp(ref DamageData data, Actor actor, Entity target)
	{
		if (!data.IsAmountModifiedBy(this) && owner.CheckEnemyOrNeutral(target))
		{
			data.SetAmountModifiedBy(this);
			if (data.elemental == ElementalType.Fire)
			{
				data.SetElemental(ElementalType.Cold);
			}
			else if (data.elemental == ElementalType.Cold)
			{
				data.SetElemental(ElementalType.Fire);
			}
			else if (data.elemental == ElementalType.Dark)
			{
				data.SetElemental(ElementalType.Light);
			}
			else if (data.elemental == ElementalType.Light)
			{
				data.SetElemental(ElementalType.Dark);
			}
			data.ApplyAmplification(GetValue(dmgAmp));
		}
	}

	private void MirrorProcessed()
	{
	}
}
