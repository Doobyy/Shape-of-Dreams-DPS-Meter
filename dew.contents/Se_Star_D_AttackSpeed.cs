using Mirror;

public class Se_Star_D_AttackSpeed : StarEffect
{
	public StarScalingValue bonusAmount;

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
