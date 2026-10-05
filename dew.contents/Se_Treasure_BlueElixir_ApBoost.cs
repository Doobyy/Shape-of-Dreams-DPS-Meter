using Mirror;

public class Se_Treasure_BlueElixir_ApBoost : TempEffect
{
	public float bonusRatio = 0.3f;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DoStatBonus(new StatBonus
			{
				abilityPowerPercentage = bonusRatio * 100f
			});
		}
	}

	private void MirrorProcessed()
	{
	}
}
