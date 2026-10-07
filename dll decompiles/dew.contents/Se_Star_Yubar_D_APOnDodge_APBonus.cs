using Mirror;

public class Se_Star_Yubar_D_APOnDodge_APBonus : StatusEffect
{
	public float duration;

	public StarScalingValue apPercentage;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			SetTimer(duration);
			DoStatBonus(new StatBonus
			{
				abilityPowerPercentage = GetValueInt(apPercentage)
			});
		}
	}

	private void MirrorProcessed()
	{
	}
}
