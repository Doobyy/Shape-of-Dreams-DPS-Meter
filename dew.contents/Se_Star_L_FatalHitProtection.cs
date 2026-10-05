using Mirror;

public class Se_Star_L_FatalHitProtection : StarEffect
{
	public StarScalingValue invulTime;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			victim.CreateStatusEffect(victim, new CastInfo(victim), (Se_FatalHitProtection_Interrupt se) =>
			{
				se.SetStack(1);
				se.invulTime = GetValue(invulTime);
			});
			Destroy();
		}
	}

	private void MirrorProcessed()
	{
	}
}
