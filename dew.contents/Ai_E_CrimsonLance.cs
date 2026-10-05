using UnityEngine;

public class Ai_E_CrimsonLance : StandardProjectile
{
	public ScalingValue damage = "6ad";

	public float bossBonusDamageHpRatio = 0.15f;

	public GameObject fxHitCrit;

	public override bool reuseInRoom => true;

	protected override void OnEntity(EntityHit hit)
	{
		base.OnEntity(hit);
		DamageData damageData = Damage(damage).SetElemental(ElementalType.Dark).SetDirection(info.forward);
		if (hit.entity is BossMonster bossMonster)
		{
			damageData.AddFlatAmount(bossBonusDamageHpRatio * hit.entity.maxHealth);
			damageData.SetAttr(DamageAttribute.IsCrit);
			if ((Object)(object)firstTrigger != null)
			{
				firstTrigger.Destroy();
				info.caster.CreateStatusEffect(bossMonster, new CastInfo(info.caster, info.angle), (Se_E_CrimsonLance_BossDrop se) =>
				{
					se.skillLevel = skillLevel;
				});
				FxPlayNewNetworked(fxHitCrit, bossMonster);
			}
			Destroy();
		}
		damageData.Dispatch(hit.entity);
	}

	private void MirrorProcessed()
	{
	}
}
