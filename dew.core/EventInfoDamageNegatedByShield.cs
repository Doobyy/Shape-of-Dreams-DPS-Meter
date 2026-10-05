using System;

public struct EventInfoDamageNegatedByShield
{
	public Actor actor;

	[NonSerialized]
	public ShieldEffect shield;

	public Entity victim;

	public float negatedAmount;

	public FinalDamageData damage;
}
