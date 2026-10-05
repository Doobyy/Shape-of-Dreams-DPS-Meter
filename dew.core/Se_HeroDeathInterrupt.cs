using Mirror;

public class Se_HeroDeathInterrupt : StatusEffect
{
	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		DoDeathInterrupt((EventInfoKill _) =>
		{
			victim.Status.SetHealth(0.01f);
			if (NetworkedManagerBase<GameManager>.instance.difficulty.enableBleedOuts)
			{
				if (!victim.Status.HasStatusEffect<Se_HeroKnockedOut>() && !victim.Status.HasStatusEffect<Se_HeroBleedingOut>())
				{
					CreateStatusEffect<Se_HeroBleedingOut>(victim);
				}
			}
			else if (!victim.Status.HasStatusEffect<Se_HeroKnockedOut>())
			{
				CreateStatusEffect<Se_HeroKnockedOut>(victim);
			}
		}, 1000);
	}

	private void MirrorProcessed()
	{
	}
}
