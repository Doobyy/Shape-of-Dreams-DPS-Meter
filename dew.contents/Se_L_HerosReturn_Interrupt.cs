using Mirror;

public class Se_L_HerosReturn_Interrupt : StatusEffect
{
	public float preCastSelfInvulDuration = 0.75f;

	protected override void OnCreate()
	{
		base.OnCreate();
		St_L_HerosReturn skill = (St_L_HerosReturn)firstTrigger;
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		DoDeathInterrupt((EventInfoKill _) =>
		{
			if (!skill.IsNullOrInactive())
			{
				victim.Status.SetHealth(1f);
				CreateBasicEffect(victim, new InvulnerableEffect(), preCastSelfInvulDuration);
				skill.TriggerAutoCast();
			}
		}, 660);
	}

	private void MirrorProcessed()
	{
	}
}
