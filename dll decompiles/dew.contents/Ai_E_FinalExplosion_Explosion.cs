using Mirror;
using UnityEngine;

public class Ai_E_FinalExplosion_Explosion : InstantDamageInstance
{
	public ScalingValue selfDamageRatio;

	public float selfDamageRatioMax = 0.8f;

	public float stunDuration = 4f;

	public float deadAmp = 1f;

	public float selfDamageRatioCalculated => Mathf.Min(GetValue(selfDamageRatio), selfDamageRatioMax);

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			PureDamage(selfDamageRatioCalculated * info.caster.maxHealth).SetAttr(DamageAttribute.IgnoreArmor).SetAttr(DamageAttribute.IgnoreShield).SetAttr(DamageAttribute.IgnoreDamageImmunity)
				.Dispatch(info.caster);
			CreateBasicEffect(info.caster, new SlowEffect
			{
				decay = true,
				strength = 100f
			}, 1f);
		}
	}

	protected override void OnHit(Entity entity)
	{
		base.OnHit(entity);
		CreateBasicEffect(entity, new StunEffect(), stunDuration);
	}

	protected override void OnBeforeDispatchDamage(ref DamageData dmg, Entity target)
	{
		base.OnBeforeDispatchDamage(ref dmg, target);
		if (info.caster.IsNullInactiveDeadOrKnockedOut())
		{
			dmg.ApplyAmplification(deadAmp);
		}
	}

	private void MirrorProcessed()
	{
	}
}
