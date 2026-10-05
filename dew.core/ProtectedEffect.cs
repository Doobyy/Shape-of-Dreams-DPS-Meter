using System;

public class ProtectedEffect : BasicEffect
{
	public Action<EventInfoDamageNegatedByImmunity> onDamageNegated;

	public override BasicEffectMask mask => BasicEffectMask.Protected;
}
