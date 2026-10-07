using System;
using UnityEngine;

public class Ai_R_Scattershot_Projectile : StandardProjectile
{
	public ScalingValue damage;

	public float attackEffectStrength = 0.5f;

	public float damageIncrByEmpoweredCrit;

	public GameObject fxEmpowered;

	public GameObject fxEmpoweredHit;

	[NonSerialized]
	public bool isEmpoweredByCrit;

	private GameObject _baseEffectOnFly;

	public override bool reuseInRoom => true;

	protected override void Awake()
	{
		base.Awake();
		_baseEffectOnFly = effectOnFly;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		effectOnFly = _baseEffectOnFly;
	}

	protected override void OnPrepare()
	{
		base.OnPrepare();
		if (isEmpoweredByCrit)
		{
			effectOnFly = fxEmpowered;
		}
	}

	protected override void OnEntity(EntityHit hit)
	{
		base.OnEntity(hit);
		DamageData damageData = Damage(damage).SetDirection(rotation).DoAttackEffect(AttackEffectType.Others, attackEffectStrength).SetElemental(ElementalType.Dark)
			.SetAttr(DamageAttribute.ForceMergeNumber);
		if (isEmpoweredByCrit)
		{
			FxPlayNewNetworked(fxEmpoweredHit, hit.entity);
			damageData.ApplyAmplification(damageIncrByEmpoweredCrit);
			damageData.SetAttr(DamageAttribute.IsCrit);
		}
		damageData.Dispatch(hit.entity);
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
