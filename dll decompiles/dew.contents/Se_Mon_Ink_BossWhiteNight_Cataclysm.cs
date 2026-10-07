using Mirror;

public class Se_Mon_Ink_BossWhiteNight_Cataclysm : StatusEffect
{
	public bool enableInvulnerable;

	private Channel _channel;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			_channel = info.caster.Control.StartChannel(new Channel
			{
				blockedActions = Channel.BlockedAction.Everything
			});
			if (enableInvulnerable)
			{
				DoInvulnerable();
			}
			DoStatBonus(new StatBonus
			{
				abilityHasteFlat = 50f
			});
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && _channel != null)
		{
			_channel.Cancel();
			_channel = null;
		}
	}

	private void MirrorProcessed()
	{
	}
}
