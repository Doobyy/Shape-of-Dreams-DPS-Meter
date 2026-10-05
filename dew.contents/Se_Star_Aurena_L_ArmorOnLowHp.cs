using System;
using Mirror;

public class Se_Star_Aurena_L_ArmorOnLowHp : HealthThresholdBonusStarEffect
{
	public StarScalingValue armorAmount;

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
			_bonus.armorFlat = (isLowHealth ? GetValue(armorAmount) : 0f);
		}
	}

	private void MirrorProcessed()
	{
	}
}
