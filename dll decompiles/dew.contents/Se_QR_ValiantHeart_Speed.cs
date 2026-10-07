using Mirror;

public class Se_QR_ValiantHeart_Speed : StatusEffect
{
	public ScalingValue speedAmount = "20 10x";

	public float speedDuration = 1f;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			SetTimer(speedDuration);
			DoSpeed(GetValue(speedAmount));
		}
	}

	private void MirrorProcessed()
	{
	}
}
