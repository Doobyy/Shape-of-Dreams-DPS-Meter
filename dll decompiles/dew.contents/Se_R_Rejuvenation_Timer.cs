using Mirror;

public class Se_R_Rejuvenation_Timer : StatusEffect
{
	public float duration = 1.5f;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			SetTimer(duration);
			ShowOnScreenTimer("Gem_R_Rejuvenation");
		}
	}

	private void MirrorProcessed()
	{
	}
}
