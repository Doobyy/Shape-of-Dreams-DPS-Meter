using System;
using System.Collections.Generic;
using UnityEngine;

public class Ai_R_DarkGrenade_Shard : StandardProjectile
{
	public ScalingValue damage;

	public float attackEffectStrength = 0.5f;

	public float subsequentHitMultiplier = 0.6f;

	public GameObject fxEmpoweredHit;

	[NonSerialized]
	public HashSet<Entity> hitEntities;

	[NonSerialized]
	public Vector3 spawnPos;

	[NonSerialized]
	public bool isEmpowered;

	public override bool reuseInRoom => true;

	protected override void OnDisable()
	{
		base.OnDisable();
		hitEntities = null;
	}

	protected override void OnPrepare()
	{
		base.OnPrepare();
		SetCustomStartPosition(spawnPos);
	}

	protected override void OnEntity(EntityHit hit)
	{
		base.OnEntity(hit);
		DamageData damageData = Damage(damage).SetDirection(rotation).SetOriginPosition(spawnPos).SetElemental(ElementalType.Dark)
			.DoAttackEffect(AttackEffectType.Others, attackEffectStrength);
		if (hitEntities != null && !hitEntities.Add(hit.entity))
		{
			damageData.ApplyStrength(subsequentHitMultiplier);
		}
		damageData.Dispatch(hit.entity);
		if (isEmpowered)
		{
			FxPlayNewNetworked(fxEmpoweredHit, hit.entity);
			if (!hit.entity.Status.HasStatusEffect<Se_R_DarkGrenade_Stun>())
			{
				CreateStatusEffect<Se_R_DarkGrenade_Stun>(hit.entity, new CastInfo(info.caster));
			}
		}
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
