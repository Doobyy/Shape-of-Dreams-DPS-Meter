using Mirror;

public class Se_R_ShadowOverdrive_AtkSpd : StatusEffect
{
	public ScalingValue hasteAmount;

	public float hasteDuration = 3f;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DoHaste(GetValue(hasteAmount));
			SetTimer(hasteDuration);
			ShowOnScreenTimer();
		}
	}

	private void MirrorProcessed()
	{
	}
}
