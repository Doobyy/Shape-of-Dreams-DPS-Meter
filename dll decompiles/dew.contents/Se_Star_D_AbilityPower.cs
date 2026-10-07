using Mirror;

public class Se_Star_D_AbilityPower : StarEffect
{
	public StarScalingValue bonusAmount;

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
