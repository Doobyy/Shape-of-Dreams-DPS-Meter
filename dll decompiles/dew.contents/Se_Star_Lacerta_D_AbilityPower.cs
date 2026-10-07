using System;
using Mirror;

public class Se_Star_Lacerta_D_AbilityPower : StarEffect
{
	public StarScalingValue bonusAmount;

	public override Type heroType => typeof(Hero_Lacerta);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DoStatBonus(new StatBonus
			{
				abilityPowerFlat = GetValue(bonusAmount)
			});
		}
	}

	private void MirrorProcessed()
	{
	}
}
