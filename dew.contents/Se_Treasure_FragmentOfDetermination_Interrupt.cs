using Mirror;

[SaveActor(true)]
public class Se_Treasure_FragmentOfDetermination_Interrupt : StatusEffect
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
			victim.Status.SetHealth(victim.Status.maxHealth * 0.25f);
			if (!victim.Status.HasStatusEffect<Se_Treasure_FragmentOfDetermination_Invulnerable>())
			{
				CreateStatusEffect(victim, (Se_Treasure_FragmentOfDetermination_Invulnerable se) =>
				{
					se.timerCustomNameKey = "Treasure_FragmentOfDetermination";
				});
				Dew.CallDelayed(() =>
				{
					if (!this.IsNullOrInactive())
					{
						Destroy();
					}
				});
			}
		}, -100);
	}

	private void MirrorProcessed()
	{
	}
}
