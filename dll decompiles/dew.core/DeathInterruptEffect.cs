using System;

public class DeathInterruptEffect : BasicEffect
{
	public Action<EventInfoKill> onInterrupt;

	public int priority;

	public override BasicEffectMask mask => BasicEffectMask.DeathInterrupt;
}
