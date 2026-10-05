public class Ai_E_VileStrike_Damage : InstantDamageInstance
{
	public float healthThreshold;

	public float dmgAmp;

	public float stunDuration;

	public override bool reuseInRoom => true;

	protected override void OnBeforeDispatchDamage(ref DamageData dmg, Entity target)
	{
		base.OnBeforeDispatchDamage(ref dmg, target);
		if (target.normalizedHealth < healthThreshold)
		{
			dmg.SetAttr(DamageAttribute.IsCrit);
			dmg.ApplyAmplification(dmgAmp);
		}
	}

	protected override void OnHit(Entity entity)
	{
		base.OnHit(entity);
		CreateBasicEffect(entity, new StunEffect(), stunDuration);
	}

	private void MirrorProcessed()
	{
	}
}
