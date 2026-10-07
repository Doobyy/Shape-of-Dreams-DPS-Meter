using Mirror;

public class Se_Gem_C_Quicksilver : StatusEffect
{
	public ScalingValue speedAmount;

	public bool isDecay;

	public float duration;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DoSpeed(GetValue(speedAmount)).decay = isDecay;
			SetTimer(duration);
		}
	}

	private void MirrorProcessed()
	{
	}
}
