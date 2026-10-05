using Mirror;

public class Se_Star_D_LowerASHigherAD : StarEffect
{
	public float atkSpdReductionPercentage;

	public StarScalingValue adBonus;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DoStatBonus(new StatBonus
			{
				attackSpeedPercentage = 0f - atkSpdReductionPercentage,
				attackDamageFlat = GetValue(adBonus)
			});
		}
	}

	private void MirrorProcessed()
	{
	}
}
