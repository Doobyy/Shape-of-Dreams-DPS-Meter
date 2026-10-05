using System;
using Mirror;

public class Se_Star_Vesper_L_SlowSpeedMoreHealth : StarEffect
{
	public StarScalingValue healthBonusPercentage;

	public float movSpeedPenaltyPercentage = 15f;

	public override Type heroType => typeof(Hero_Vesper);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DoStatBonus(new StatBonus
			{
				maxHealthPercentage = GetValue(healthBonusPercentage),
				movementSpeedPercentage = 0f - movSpeedPenaltyPercentage
			});
		}
	}

	private void MirrorProcessed()
	{
	}
}
