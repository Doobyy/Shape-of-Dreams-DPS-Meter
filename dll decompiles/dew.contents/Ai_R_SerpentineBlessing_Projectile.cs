using System;

public class Ai_R_SerpentineBlessing_Projectile : StandardProjectile
{
	public ScalingValue damage;

	public ScalingValue healMaxHpRatio;

	public float stunDuration;

	[NonSerialized]
	public float strength;

	[NonSerialized]
	public Entity healTarget;

	public override bool reuseInRoom => true;

	protected override void OnEntity(EntityHit hit)
	{
		base.OnEntity(hit);
		CreateBasicEffect(hit.entity, new StunEffect(), stunDuration * strength);
		Damage(damage).ApplyStrength(strength).Dispatch(hit.entity, chain);
		Heal(GetValue(healMaxHpRatio) * healTarget.maxHealth * strength).Dispatch(healTarget, chain);
	}

	private void MirrorProcessed()
	{
	}
}
