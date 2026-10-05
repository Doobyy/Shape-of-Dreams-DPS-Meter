using Mirror;

public class Se_Gem_C_Wind : StatusEffect
{
	public ScalingValue hasteAmount;

	public float duration;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DoHaste(GetValue(hasteAmount));
			SetTimer(duration);
		}
	}

	private void MirrorProcessed()
	{
	}
}
