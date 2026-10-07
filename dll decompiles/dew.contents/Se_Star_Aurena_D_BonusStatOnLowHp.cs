using System;
using Mirror;

public class Se_Star_Aurena_D_BonusStatOnLowHp : HealthThresholdBonusStarEffect
{
	public StarScalingValue statPercentage;

	private StatBonus _bonus;

	public override Type heroType => typeof(Hero_Aurena);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			_bonus = DoStatBonus();
		}
	}

	public override void OnHealthStateChanged()
	{
		base.OnHealthStateChanged();
		if (((NetworkBehaviour)this).isServer && _bonus != null)
		{
			if (isLowHealth)
			{
				_bonus.abilityPowerPercentage = GetValue(statPercentage);
				_bonus.attackDamagePercentage = GetValue(statPercentage);
			}
			else
			{
				_bonus.abilityPowerPercentage = 0f;
				_bonus.attackDamagePercentage = 0f;
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
