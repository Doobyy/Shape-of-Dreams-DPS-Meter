using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Gem_R_Snow : Gem
{
	public ScalingValue shieldAmount;

	public float rechargeTime = 5f;

	public float range = 5f;

	public float shootIntervalMax = 1.5f;

	public float shootIntervalMin = 0.2f;

	public float maxSpeedShieldHpRatio = 1f;

	public AbilityTargetValidator targets;

	private float _lastShootTime;

	private float _lastDamageTime;

	private Se_GenericShield_Stacking _shield;

	private float _maxShield;

	private float _maxShieldLastUpdateTime;

	public override void OnEquipGem(Hero newOwner)
	{
		base.OnEquipGem(newOwner);
		if (((NetworkBehaviour)this).isServer)
		{
			_shield = CreateStatusEffect(newOwner, new CastInfo(newOwner), (Se_GenericShield_Stacking se) =>
			{
				se.timeout = float.PositiveInfinity;
			});
			newOwner.EntityEvent_OnTakeDamage += new Action<EventInfoDamage>(EntityEventOnTakeDamage);
		}
	}

	public override void OnUnequipGem(Hero oldOwner)
	{
		base.OnUnequipGem(oldOwner);
		if (((NetworkBehaviour)this).isServer)
		{
			if (!_shield.IsNullOrInactive())
			{
				_shield.Destroy();
				_shield = null;
			}
			if ((UnityEngine.Object)(object)oldOwner != null)
			{
				oldOwner.EntityEvent_OnTakeDamage -= new Action<EventInfoDamage>(EntityEventOnTakeDamage);
			}
		}
	}

	private void EntityEventOnTakeDamage(EventInfoDamage obj)
	{
		_lastDamageTime = Time.time;
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!((NetworkBehaviour)this).isServer || !isValid || owner.isKnockedOut || owner.Status.hasStealth)
		{
			return;
		}
		if (Time.time - _maxShieldLastUpdateTime > 0.25f)
		{
			_maxShield = GetValue(shieldAmount);
			HealData data = new HealData(_maxShield).SetActor(this);
			dealtShieldProcessor.Process(ref data, this, owner);
			owner.takenShieldProcessor.Process(ref data, this, owner);
			_maxShield = new FinalHealData(data, float.PositiveInfinity).amount;
			_maxShieldLastUpdateTime = Time.time;
		}
		if (!_shield.IsNullOrInactive() && Time.time - _lastDamageTime > rechargeTime && _shield.shield.amount < _maxShield)
		{
			_shield.AddAmount(_maxShield * dt);
			if (_shield.shield.amount > _maxShield)
			{
				_shield.shield.amount = _maxShield;
			}
		}
		if (owner.Status.currentShield <= 1f)
		{
			return;
		}
		float num = Mathf.Lerp(shootIntervalMax, shootIntervalMin, owner.Status.currentShield / owner.Status.maxHealth / maxSpeedShieldHpRatio);
		if (Time.time - _lastShootTime > num)
		{
			List<Entity> list = DewPhysics.OverlapCircleAllEntities(out var handle, owner.agentPosition, range, targets, owner);
			if (list.Count > 0)
			{
				_lastShootTime = Time.time;
				CreateAbilityInstance<Ai_Gem_R_Snow_Projectile>(owner.position, Quaternion.identity, new CastInfo(owner, list[UnityEngine.Random.Range(0, list.Count)]));
				NotifyUse();
			}
			else
			{
				_lastShootTime = Time.time - num * 0.5f;
			}
			handle.Return();
		}
	}

	private void MirrorProcessed()
	{
	}
}
