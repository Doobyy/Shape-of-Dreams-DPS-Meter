using System;
using System.Collections;
using Mirror;
using UnityEngine;

public class Se_Mon_Special_BossMaw_ShieldConversion : StatusEffect
{
	public float shieldMissingHealthRatio;

	public float rechargeTime;

	public float shieldRecoveryAmountPerSecond;

	private float _lastDamageTime;

	private float _maxShield;

	private float _maxShieldLastUpdateTime;

	private ActorRef<Se_GenericShield_Stacking> _shield;

	protected override IEnumerator OnCreateSequenced()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			DestroyOnDeath(info.caster);
			_shield = CreateStatusEffect(victim, new CastInfo(victim), (Se_GenericShield_Stacking se) =>
			{
				se.timeout = float.PositiveInfinity;
			});
			victim.EntityEvent_OnTakeDamage += new Action<EventInfoDamage>(EntityEventOnTakeDamage);
		}
		yield break;
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			if (!_shield.IsNullOrInactive())
			{
				_shield.Get().Destroy();
				_shield = null;
			}
			if ((UnityEngine.Object)(object)victim != null)
			{
				victim.EntityEvent_OnTakeDamage -= new Action<EventInfoDamage>(EntityEventOnTakeDamage);
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
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		if (Time.time - _maxShieldLastUpdateTime > 0.25f)
		{
			_maxShield = victim.Status.missingHealth * shieldMissingHealthRatio;
			HealData data = new HealData(_maxShield).SetActor(this);
			dealtShieldProcessor.Process(ref data, this, victim);
			victim.takenShieldProcessor.Process(ref data, this, victim);
			_maxShield = new FinalHealData(data, float.PositiveInfinity).amount;
			_maxShieldLastUpdateTime = Time.time;
		}
		if (_shield.IsNullOrInactive())
		{
			return;
		}
		Se_GenericShield_Stacking se_GenericShield_Stacking = _shield.Get();
		if (Time.time - _lastDamageTime > rechargeTime && se_GenericShield_Stacking.shield.amount < _maxShield)
		{
			se_GenericShield_Stacking.AddAmount(victim.Status.maxHealth * shieldRecoveryAmountPerSecond * dt);
			if (se_GenericShield_Stacking.shield.amount > _maxShield)
			{
				se_GenericShield_Stacking.shield.amount = _maxShield;
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
