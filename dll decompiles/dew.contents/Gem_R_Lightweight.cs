using Mirror;
using UnityEngine;

public class Gem_R_Lightweight : Gem
{
	public ScalingValue skillHaste;

	public float damageReduction = 0.4f;

	public GameObject fxOnCastSkill;

	private SkillBonus _bonus;

	public float reducedRatio => 1f - 1f / (1f + GetValue(skillHaste) * 0.01f);

	public override void OnEquipSkill(SkillTrigger newSkill)
	{
		base.OnEquipSkill(newSkill);
		if (((NetworkBehaviour)this).isServer)
		{
			_bonus = new SkillBonus
			{
				cooldownMultiplier = 1f - reducedRatio
			};
			newSkill.AddSkillBonus(_bonus);
			newSkill.dealtDamageProcessor.Add(ReduceDamage);
		}
	}

	public override void OnUnequipSkill(SkillTrigger oldSkill)
	{
		base.OnUnequipSkill(oldSkill);
		if (((NetworkBehaviour)this).isServer)
		{
			if ((Object)(object)oldSkill != null)
			{
				oldSkill.RemoveSkillBonus(_bonus);
				oldSkill.dealtDamageProcessor.Remove(ReduceDamage);
			}
			_bonus = null;
		}
	}

	protected override void OnCastComplete(EventInfoCast info)
	{
		base.OnCastComplete(info);
		FxPlayNetworked(fxOnCastSkill, owner);
	}

	protected override void OnQualityChange(int oldQuality, int newQuality)
	{
		base.OnQualityChange(oldQuality, newQuality);
		if (((NetworkBehaviour)this).isServer)
		{
			_bonus.cooldownMultiplier = 1f - reducedRatio;
		}
	}

	private void ReduceDamage(ref DamageData data, Actor actor, Entity target)
	{
		if (!data.IsAmountModifiedBy(this))
		{
			data.ApplyReduction(damageReduction);
			data.SetAmountModifiedBy(this);
		}
	}

	private void MirrorProcessed()
	{
	}
}
