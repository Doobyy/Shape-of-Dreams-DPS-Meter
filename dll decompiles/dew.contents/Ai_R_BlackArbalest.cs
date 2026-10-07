using Mirror;
using UnityEngine;

public class Ai_R_BlackArbalest : StandardProjectile
{
	public ScalingValue hitDamage;

	public float ampPerHit;

	public bool doSelfKnockback;

	public float attackEffectStrength = 1f;

	public Dash selfKnockback;

	public bool doTargetKnockback;

	public Knockback targetKnockback;

	public ScalingValue empowerChance;

	public float empoweredAmpDamage;

	public int empoweredDarkStackCount;

	public GameObject fxEmpoweredCast;

	public GameObject fxEmpoweredHit;

	private bool _isEmpowered;

	private int _prevHitCount;

	private float _baseInitialSpeed;

	public override bool reuseInRoom => true;

	protected override void Awake()
	{
		base.Awake();
		_baseInitialSpeed = initialSpeed;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		initialSpeed = _baseInitialSpeed;
		_isEmpowered = false;
		_prevHitCount = 0;
	}

	protected override void OnPrepare()
	{
		base.OnPrepare();
		_isEmpowered = Random.value * 100f < GetValue(empowerChance);
		if (_isEmpowered)
		{
			initialSpeed *= 1.5f;
		}
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			if (doSelfKnockback)
			{
				selfKnockback.ApplyByDirection(info.caster, -info.forward);
			}
			if (_isEmpowered)
			{
				FxPlayNetworked(fxEmpoweredCast, info.caster);
			}
		}
	}

	protected override void OnEntity(EntityHit hit)
	{
		base.OnEntity(hit);
		DamageData damageData = Damage(hitDamage).SetDirection(rotation).DoAttackEffect(AttackEffectType.Others, attackEffectStrength).SetElemental(ElementalType.Dark);
		if (_prevHitCount >= 2)
		{
			damageData.SetAttr(DamageAttribute.IsCrit);
		}
		damageData.ApplyAmplification(ampPerHit * (float)_prevHitCount);
		if (_isEmpowered)
		{
			FxPlayNewNetworked(fxEmpoweredHit, hit.entity);
			damageData.SetAttr(DamageAttribute.IsCrit);
			damageData.ApplyAmplification(empoweredAmpDamage);
			damageData.SetOverrideElementalStacks(empoweredDarkStackCount);
		}
		damageData.Dispatch(hit.entity);
		if (doTargetKnockback)
		{
			targetKnockback.ApplyWithDirection(rotation, hit.entity);
		}
		_prevHitCount++;
	}

	private void MirrorProcessed()
	{
	}
}
