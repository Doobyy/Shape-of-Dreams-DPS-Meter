using Mirror;

public class Se_Gem_R_Epiphany_ApBuff : StatusEffect
{
	public ScalingValue apEmpowerAmount;

	public float duration;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DoStatBonus(new StatBonus
			{
				abilityPowerFlat = GetValue(apEmpowerAmount)
			});
			SetTimer(duration);
			ShowOnScreenTimer("Gem_R_Epiphany");
		}
	}

	private void MirrorProcessed()
	{
	}
}
