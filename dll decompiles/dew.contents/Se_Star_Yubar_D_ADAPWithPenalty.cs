using System;
using Mirror;

public class Se_Star_Yubar_D_ADAPWithPenalty : StarEffect
{
	public StarScalingValue adAmount;

	public StarScalingValue apAmount;

	public float maxHealthPenaltyPercentage;

	public override Type heroType => typeof(Hero_Yubar);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DoStatBonus(new StatBonus
			{
				attackDamageFlat = GetValueInt(adAmount),
				abilityPowerFlat = GetValueInt(apAmount),
				maxHealthPercentage = 0f - maxHealthPenaltyPercentage
			});
		}
	}

	private void MirrorProcessed()
	{
	}
}
