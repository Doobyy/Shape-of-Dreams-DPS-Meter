using Mirror;

[SaveActor(true)]
public class Se_FatalHitProtection_Interrupt : StackedStatusEffect
{
	[SaveVar(SaveVarFlags.Default)]
	public float invulTime { get; set; }

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
			if (!victim.Status.HasStatusEffect<Se_FatalHitProtection_Invulnerable>())
			{
				CreateStatusEffect(victim, (Se_FatalHitProtection_Invulnerable se) =>
				{
					se.duration = invulTime;
				});
				Dew.CallDelayed(() =>
				{
					if (!this.IsNullOrInactive())
					{
						RemoveStack();
					}
				});
			}
		}, 0);
	}

	private void MirrorProcessed()
	{
	}
}
