using System;
using Mirror;

public class Se_Star_D_LowerAPScalingAD : StarEffect
{
	public float apReductionPercentage;

	public StarScalingValue adBonusPerLevel;

	private StatBonus _bonus;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			_bonus = DoStatBonus(new StatBonus
			{
				abilityPowerPercentage = 0f - apReductionPercentage
			});
			hero.ClientHeroEvent_OnLevelChanged += new Action<EventInfoHeroLevelUp>(ClientHeroEventOnLevelChanged);
			UpdateStatBonus();
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && !hero.IsNullOrInactive())
		{
			hero.ClientHeroEvent_OnLevelChanged -= new Action<EventInfoHeroLevelUp>(ClientHeroEventOnLevelChanged);
		}
	}

	private void ClientHeroEventOnLevelChanged(EventInfoHeroLevelUp obj)
	{
		UpdateStatBonus();
	}

	private void UpdateStatBonus()
	{
		float attackDamageFlat = (float)(victim.level - 1) * GetValue(adBonusPerLevel);
		_bonus.attackDamageFlat = attackDamageFlat;
	}

	private void MirrorProcessed()
	{
	}
}
