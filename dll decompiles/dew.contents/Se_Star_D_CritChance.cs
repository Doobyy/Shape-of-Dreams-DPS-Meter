using Mirror;

public class Se_Star_D_CritChance : StarEffect
{
	public StarScalingValue bonusAmount;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DoStatBonus(new StatBonus
			{
				critChanceFlat = GetValue(bonusAmount)
			});
		}
	}

	private void MirrorProcessed()
	{
	}
}
