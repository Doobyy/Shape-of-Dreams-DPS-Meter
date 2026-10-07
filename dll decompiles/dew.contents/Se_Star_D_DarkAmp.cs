using Mirror;

public class Se_Star_D_DarkAmp : StarEffect
{
	public StarScalingValue bonusAmpPerStack;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DoStatBonus(new StatBonus
			{
				darkEffectAmpFlat = GetValue(bonusAmpPerStack)
			});
		}
	}

	private void MirrorProcessed()
	{
	}
}
