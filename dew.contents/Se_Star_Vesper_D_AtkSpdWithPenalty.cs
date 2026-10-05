using System;
using Mirror;

public class Se_Star_Vesper_D_AtkSpdWithPenalty : StarEffect
{
	public StarScalingValue bonusAtkSpd;

	public float healthPenaltyPercentage;

	public override Type heroType => typeof(Hero_Vesper);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DoStatBonus(new StatBonus
			{
				attackSpeedPercentage = GetValue(bonusAtkSpd),
				maxHealthPercentage = 0f - healthPenaltyPercentage
			});
		}
	}

	private void MirrorProcessed()
	{
	}
}
