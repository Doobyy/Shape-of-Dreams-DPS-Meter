using Mirror;

public class Se_Curse_BrainFog : CurseStatusEffect
{
	public float[] reducedHpPercentage;

	public float[] reducedAdPercentage;

	public float[] reducedApPercentage;

	public float[] reducedArmorPercentage;

	public float[] reducedAttackSpeedPercentage;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DoStatBonus(new StatBonus
			{
				maxHealthPercentage = 0f - GetValue(reducedHpPercentage),
				attackDamagePercentage = 0f - GetValue(reducedAdPercentage),
				abilityPowerPercentage = 0f - GetValue(reducedApPercentage),
				attackSpeedPercentage = 0f - GetValue(reducedAttackSpeedPercentage)
			});
			DoArmorReduction(GetValue(reducedArmorPercentage));
		}
	}

	private void MirrorProcessed()
	{
	}
}
