using Mirror;

public class Se_RoomMod_InkStrikeWarning_Slow : StatusEffect
{
	public float duration;

	public float slowAmount;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DestroyOnDeath(victim);
			DoSlow(slowAmount);
			SetTimer(duration);
		}
	}

	private void MirrorProcessed()
	{
	}
}
