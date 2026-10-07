using System;
using UnityEngine;

public class Ai_R_ShadowWalk_Projectile : StandardProjectile
{
	public float procCoefficient = 1f;

	public ScalingValue damage;

	public float deviateMag = 3f;

	public float attackEffectStrength;

	public GameObject fxEmpoweredHit;

	[NonSerialized]
	public bool isEmpoweredByCrit;

	private Vector3 _deviateVector;

	public override bool reuseInRoom => true;

	protected override void OnDisable()
	{
		base.OnDisable();
		isEmpoweredByCrit = false;
	}

	protected override void OnCreate()
	{
		_deviateVector = UnityEngine.Random.insideUnitSphere * deviateMag;
		base.OnCreate();
	}

	protected override Vector3 PositionSolver(float dt)
	{
		return base.PositionSolver(dt) + Mathf.Sin(normalizedPosition * (float)Math.PI) * _deviateVector;
	}

	protected override void OnEntity(EntityHit hit)
	{
		base.OnEntity(hit);
		DamageData damageData = Damage(damage, procCoefficient).SetDirection(rotation).DoAttackEffect(AttackEffectType.Others, attackEffectStrength).SetElemental(ElementalType.Dark);
		if (isEmpoweredByCrit)
		{
			FxPlayNewNetworked(fxEmpoweredHit, hit.entity);
			damageData.SetAttr(DamageAttribute.IsCrit);
		}
		damageData.Dispatch(hit.entity);
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
