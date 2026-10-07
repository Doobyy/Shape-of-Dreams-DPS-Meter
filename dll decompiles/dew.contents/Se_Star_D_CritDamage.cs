using Mirror;

public class Se_Star_D_CritDamage : StarEffect
{
	public StarScalingValue bonusAmount;

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
