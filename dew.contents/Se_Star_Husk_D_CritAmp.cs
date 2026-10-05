using System;
using Mirror;

public class Se_Star_Husk_D_CritAmp : StarEffect
{
	public StarScalingValue bonusAmount;

	public override Type heroType => typeof(Hero_Husk);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DoStatBonus(new StatBonus
			{
				critAmpFlat = GetValue(bonusAmount)
			});
		}
	}

	private void MirrorProcessed()
	{
	}
}
