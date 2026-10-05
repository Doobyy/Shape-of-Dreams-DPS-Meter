using Mirror;
using UnityEngine;

public class Gem_E_Virtuousness : Gem
{
	public int requiredQualityPerCharge;

	private SkillBonus _bonus;

	public int addedChargeInt => Mathf.FloorToInt(1 + quality / requiredQualityPerCharge);

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
			_bonus.addedCharge = addedChargeInt;
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
			_bonus.addedCharge = addedChargeInt;
		}
	}

	private void MirrorProcessed()
	{
	}
}
