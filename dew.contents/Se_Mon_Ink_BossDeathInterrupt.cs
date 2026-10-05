using Mirror;

public class Se_Mon_Ink_BossDeathInterrupt : StatusEffect
{
	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DoDeathInterrupt((EventInfoKill _) =>
			{
				info.caster.Status.SetHealth(1f);
			}, -100);
		}
	}

	private void MirrorProcessed()
	{
	}
}
