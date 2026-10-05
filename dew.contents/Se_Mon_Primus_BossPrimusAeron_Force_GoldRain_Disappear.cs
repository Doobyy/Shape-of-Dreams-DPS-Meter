using Mirror;

public class Se_Mon_Primus_BossPrimusAeron_Force_GoldRain_Disappear : StatusEffect
{
	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DoUntargetable();
			DoInvulnerable();
			DoUncollidable();
			DoInvisible(ignoreReveal: true);
			DestroyOnCondition(() => ((Mon_Primus_BossPrimusAeron)victim).phase != Mon_Primus_BossPrimusAeron.PhaseType.Force);
		}
	}

	private void MirrorProcessed()
	{
	}
}
