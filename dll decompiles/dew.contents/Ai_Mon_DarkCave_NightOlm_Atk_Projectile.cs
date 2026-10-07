using UnityEngine;

public class Ai_Mon_DarkCave_NightOlm_Atk_Projectile : StandardProjectile
{
	public class Ad_NightOlmAtk
	{
		public Entity caster;

		public float hitTime;
	}

	public float minHitInterval;

	public GameObject fxHit;

	public ScalingValue dmgFactor;

	public Knockback Knockback;

	public override bool reuseInRoom => true;

	protected override void OnEntity(EntityHit hit)
	{
		base.OnEntity(hit);
		if (hit.entity.TryFindData((Ad_NightOlmAtk a) => (Object)(object)a.caster == (Object)(object)info.caster, out var result))
		{
			if (Time.time - result.hitTime < minHitInterval)
			{
				return;
			}
			result.hitTime = Time.time;
		}
		else
		{
			hit.entity.AddData(new Ad_NightOlmAtk
			{
				caster = info.caster,
				hitTime = Time.time
			});
		}
		FxPlayNewNetworked(fxHit, hit.entity);
		CreateDamage(DamageData.SourceType.Default, dmgFactor).SetElemental(ElementalType.Dark).SetAttr(DamageAttribute.DamageOverTime).SetOriginPosition(info.caster.position)
			.Dispatch(hit.entity);
		Knockback.ApplyWithDirection(info.forward, hit.entity);
	}

	private void MirrorProcessed()
	{
	}
}
