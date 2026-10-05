using Mirror;

public class Se_Star_D_LightAmp : StarEffect
{
	public StarScalingValue bonusAmpPerStack;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DoStatBonus(new StatBonus
			{
				lightEffectAmpFlat = GetValue(bonusAmpPerStack)
			});
		}
	}

	private void MirrorProcessed()
	{
	}
}
