using Mirror;

public class Se_Mon_Despair_BossAzurak_RoarAtk_SafeZone : StatusEffect
{
	public float duration;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DestroyOnDeath(victim);
			SetTimer(duration);
		}
	}

	private void MirrorProcessed()
	{
	}
}
