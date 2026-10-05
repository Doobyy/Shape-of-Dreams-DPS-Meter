using System;
using UnityEngine;

public class Ai_R_ShadowOverdrive_Projectile : StandardProjectile
{
	public GameObject fxEmpoweredFly;

	public GameObject fxEmpoweredHit;

	public ScalingValue damage;

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
			effectOnFly = fxEmpoweredFly;
		}
	}

	protected override void OnEntity(EntityHit hit)
	{
		base.OnEntity(hit);
		DamageData damageData = Damage(damage).SetDirection(info.forward).SetElemental(ElementalType.Dark);
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
