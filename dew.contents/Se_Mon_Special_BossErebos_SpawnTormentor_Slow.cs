using Mirror;

public class Se_Mon_Special_BossErebos_SpawnTormentor_Slow : StatusEffect
{
	public float slowAmount;

	public float duration;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DoSlow(slowAmount);
			SetTimer(duration);
		}
	}

	private void MirrorProcessed()
	{
	}
}
