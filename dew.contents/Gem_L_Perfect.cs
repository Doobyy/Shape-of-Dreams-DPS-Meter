using UnityEngine;

public class Gem_L_Perfect : Gem
{
	public ScalingValue adAmount;

	public ScalingValue apAmount;

	public ScalingValue healthAmount;

	public ScalingValue attackSpeedAmount;

	public ScalingValue criticalChanceAmount;

	public ScalingValue skillHasteAmount;

	public override void OnEquipGem(Hero newOwner)
	{
		base.OnEquipGem(newOwner);
		UpdateStats();
	}

	protected override void OnQualityChange(int oldQuality, int newQuality)
	{
		base.OnQualityChange(oldQuality, newQuality);
		UpdateStats();
	}

	private void UpdateStats()
	{
		if (!((Object)(object)owner == null))
		{
			statBonus.attackDamageFlat = GetValue(adAmount);
			statBonus.abilityPowerFlat = GetValue(apAmount);
			statBonus.maxHealthFlat = GetValue(healthAmount);
			statBonus.attackSpeedPercentage = GetValue(attackSpeedAmount);
			statBonus.critChanceFlat = GetValue(criticalChanceAmount);
			statBonus.abilityHasteFlat = GetValue(skillHasteAmount);
		}
	}

	private void MirrorProcessed()
	{
	}
}
