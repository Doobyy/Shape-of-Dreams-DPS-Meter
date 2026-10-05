using Mirror;
using UnityEngine;

public class Gem_C_Efficiency : Gem
{
	public ScalingValue skillHaste;

	private SkillBonus _bonus;

	public float reducedRatio => 1f - 1f / (1f + GetValue(skillHaste) * 0.01f);

	protected override void OnCastComplete(EventInfoCast info)
	{
		base.OnCastComplete(info);
		NotifyUse();
	}

	public override void OnEquipSkill(SkillTrigger newSkill)
	{
		base.OnEquipSkill(newSkill);
		if (((NetworkBehaviour)this).isServer)
		{
			_bonus = new SkillBonus();
			_bonus.cooldownMultiplier = 1f - reducedRatio;
			newSkill.AddSkillBonus(_bonus);
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
			}
			_bonus = null;
		}
	}

	protected override void OnQualityChange(int oldQuality, int newQuality)
	{
		base.OnQualityChange(oldQuality, newQuality);
		if (((NetworkBehaviour)this).isServer && _bonus != null)
		{
			_bonus.cooldownMultiplier = 1f - reducedRatio;
		}
	}

	private void MirrorProcessed()
	{
	}
}
