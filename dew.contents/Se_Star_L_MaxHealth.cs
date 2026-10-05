using Mirror;

public class Se_Star_L_MaxHealth : StarEffect
{
	public StarScalingValue healthAmount;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DoStatBonus(new StatBonus
			{
				maxHealthFlat = GetValue(healthAmount)
			});
		}
	}

	private void MirrorProcessed()
	{
	}
}
