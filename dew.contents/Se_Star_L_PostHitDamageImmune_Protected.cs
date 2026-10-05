using Mirror;

public class Se_Star_L_PostHitDamageImmune_Protected : StatusEffect
{
	public float protectedDuration = 1f;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			SetTimer(protectedDuration);
			DoProtected(null);
		}
	}

	private void MirrorProcessed()
	{
	}
}
