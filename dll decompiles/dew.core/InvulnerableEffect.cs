using System;

public class InvulnerableEffect : BasicEffect
{
	public Action<EventInfoDamageNegatedByImmunity> onDamageNegated;

	public override BasicEffectMask mask => BasicEffectMask.Invulnerable;
}
