using Mirror;

public class Se_Star_D_FireAmp : StarEffect
{
	public StarScalingValue fireAmp;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DoStatBonus(new StatBonus
			{
				fireEffectAmpFlat = GetValue(fireAmp)
			});
		}
	}

	private void MirrorProcessed()
	{
	}
}
