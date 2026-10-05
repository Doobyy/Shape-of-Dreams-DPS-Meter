using System;
using Mirror;

public class Se_Star_Vesper_D_AtkSpd : StarEffect
{
	public StarScalingValue bonusAmount;

	public override Type heroType => typeof(Hero_Vesper);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DoStatBonus(new StatBonus
			{
				attackSpeedPercentage = GetValue(bonusAmount)
			});
		}
	}

	private void MirrorProcessed()
	{
	}
}
