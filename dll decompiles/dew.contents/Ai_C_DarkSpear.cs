using Mirror;
using UnityEngine;

public class Ai_C_DarkSpear : InstantDamageInstance
{
	public float stunDuration;

	public bool displaceToCenter;

	public float displaceDuration;

	public float randomMagnitude;

	public DewEase ease;

	public float attackEffectStrength;

	public float damageToShieldAmp;

	public ScalingValue empowerChance;

	public ScalingValue healAmountByEmpowered;

	public float dmgAmpByEmpowered;

	public GameObject fxEmpoweredCast;

	public GameObject fxEmpoweredHit;

	private bool _isEmpowered;

	public override bool reuseInRoom => true;

	protected override void OnDisable()
	{
		base.OnDisable();
		_isEmpowered = false;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			_isEmpowered = Random.value * 100f < GetValue(empowerChance);
			if (_isEmpowered)
			{
				FxPlayNetworked(fxEmpoweredCast, info.caster);
			}
		}
	}

	protected override void OnBeforeDispatchDamage(ref DamageData dmg, Entity target)
	{
		base.OnBeforeDispatchDamage(ref dmg, target);
		dmg.DoAttackEffect(AttackEffectType.Others, attackEffectStrength);
		if (target.Status.currentShield > 0.0001f)
		{
			dmg.ApplyDamageToShieldMultiplier(1f + damageToShieldAmp);
			dmg.SetAttr(DamageAttribute.IsCrit);
		}
		if (_isEmpowered)
		{
			dmg.ApplyAmplification(dmgAmpByEmpowered);
			dmg.SetAttr(DamageAttribute.IsCrit);
		}
	}

	protected override void OnHit(Entity entity)
	{
		base.OnHit(entity);
		if (stunDuration > 0f)
		{
			CreateBasicEffect(entity, new StunEffect(), stunDuration, "darkspear_stun");
		}
		if (displaceToCenter)
		{
			float num = Vector3.Dot(entity.agentPosition - info.caster.position, info.forward);
			Vector3 end = info.caster.position + info.forward * num + Random.insideUnitSphere * randomMagnitude;
			end = Dew.GetValidAgentDestination_Closest(info.caster.position, end);
			entity.Control.StartDisplacement(new DispByDestination
			{
				canGoOverTerrain = false,
				affectedByMovementSpeed = false,
				destination = end,
				duration = displaceDuration,
				ease = ease,
				isFriendly = false,
				rotateForward = false,
				isCanceledByCC = false
			});
		}
		if (_isEmpowered)
		{
			FxPlayNewNetworked(fxEmpoweredHit, entity);
			Heal(healAmountByEmpowered).SetCrit().SetCanMerge().Dispatch(info.caster);
		}
	}

	private void MirrorProcessed()
	{
	}
}
