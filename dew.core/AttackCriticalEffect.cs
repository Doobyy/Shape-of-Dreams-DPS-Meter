using System;

public class AttackCriticalEffect : BasicEffect
{
	public Action onUse;

	public override BasicEffectMask mask => BasicEffectMask.AttackCritical;
}
