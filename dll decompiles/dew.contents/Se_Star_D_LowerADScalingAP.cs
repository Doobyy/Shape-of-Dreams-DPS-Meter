using System;
using Mirror;

public class Se_Star_D_LowerADScalingAP : StarEffect
{
	public float adReductionPercentage;

	public StarScalingValue apBonusPerLevel;

	private StatBonus _bonus;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			_bonus = DoStatBonus(new StatBonus
			{
				attackDamagePercentage = 0f - adReductionPercentage
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
		float abilityPowerFlat = (float)(victim.level - 1) * GetValue(apBonusPerLevel);
		_bonus.abilityPowerFlat = abilityPowerFlat;
	}

	private void MirrorProcessed()
	{
	}
}
