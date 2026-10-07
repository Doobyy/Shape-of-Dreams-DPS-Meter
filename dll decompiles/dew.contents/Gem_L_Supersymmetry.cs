using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Gem_L_Supersymmetry : Gem
{
	public ScalingValue shieldHpRatio;

	public ScalingValue castDamageHpRatio;

	public float rechargeDelay = 4f;

	public GameObject fxCastDamage;

	public GameObject fxRechargeStart;

	public GameObject fxRechargeEnd;

	public GameObject fxBreak;

	public GameObject fxBreakFlavour;

	public GameObject fxLoop;

	private FakeMaxHealthEffect _fakeMaxHealth;

	private Se_GenericShield_Stacking _shield;

	private float _baseMaxHealth;

	private float _lastDamageTime;

	[SaveVar(SaveVarFlags.Default)]
	private float? _escrowedNormalizedHealth;

	private bool _wasKnockedOut;

	[SyncVar]
	private bool _isShieldBroken = true;

	private float _prevMaxShield;

	private float _maxShieldAmount;

	public float maxHealthBeforeReduction => _baseMaxHealth;

	public float currentShieldAmount
	{
		get
		{
			if (!_shield.IsNullOrInactive() && _shield.shield != null)
			{
				return _shield.shield.amount;
			}
			return 0f;
		}
	}

	public bool Network_isShieldBroken
	{
		get
		{
			return _isShieldBroken;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref _isShieldBroken, 262144uL, (Action<bool, bool>)null);
		}
	}

	public override void OnEquipGem(Hero newOwner)
	{
		base.OnEquipGem(newOwner);
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		_lastDamageTime = Time.time;
		_prevMaxShield = 0f;
		_maxShieldAmount = 0f;
		_fakeMaxHealth = new FakeMaxHealthEffect();
		CreateBasicEffect(newOwner, _fakeMaxHealth, float.PositiveInfinity);
		float valueOrDefault = _escrowedNormalizedHealth.GetValueOrDefault();
		if (!_escrowedNormalizedHealth.HasValue)
		{
			valueOrDefault = Mathf.Clamp01(newOwner.Status.normalizedHealth);
			_escrowedNormalizedHealth = valueOrDefault;
		}
		_wasKnockedOut = false;
		newOwner.Status.finalStatsProcessors.Add(FinalStatProcessor, 100);
		newOwner.Status.CalculateStats();
		newOwner.Status.SetHealth(1f);
		_shield = CreateStatusEffect(newOwner, new CastInfo(newOwner), (Se_GenericShield_Stacking se) =>
		{
			se.timeout = float.PositiveInfinity;
		});
		Se_GenericShield_Stacking shieldSe = _shield;
		Dew.CallDelayed(() =>
		{
			if (!((UnityEngine.Object)(object)shieldSe != (UnityEngine.Object)(object)_shield) && !shieldSe.IsNullOrInactive() && shieldSe.shield != null && _fakeMaxHealth != null)
			{
				shieldSe.shield.onDamageNegated += new Action<EventInfoDamageNegatedByShield>(OnDamageNegated);
				UpdateFakeMaxHealthAmount();
			}
		});
		newOwner.EntityEvent_OnTakeDamage += new Action<EventInfoDamage>(OnTakeDamage);
		newOwner.ClientHeroEvent_OnSkillUse += new Action<EventInfoSkillUse>(OnSkillUse);
		if (newOwner.Status.TryGetStatusEffect<Se_HeroOneShotProtection>(out var effect))
		{
			effect.DisableProtection();
		}
	}

	public override void OnUnequipGem(Hero oldOwner)
	{
		base.OnUnequipGem(oldOwner);
		if (!_isShieldBroken && !oldOwner.IsNullOrInactive() && (UnityEngine.Object)(object)oldOwner.owner != null && ((NetworkBehaviour)oldOwner.owner).isLocalPlayer)
		{
			FxPlay(fxBreakFlavour, oldOwner);
		}
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		if (_fakeMaxHealth != null && !_fakeMaxHealth.parent.IsNullOrInactive())
		{
			_fakeMaxHealth.parent.Destroy();
		}
		_fakeMaxHealth = null;
		if (!_shield.IsNullOrInactive())
		{
			_shield.Destroy();
		}
		_shield = null;
		if (!oldOwner.IsNullOrInactive())
		{
			oldOwner.Status.finalStatsProcessors.Remove(FinalStatProcessor);
			oldOwner.EntityEvent_OnTakeDamage -= new Action<EventInfoDamage>(OnTakeDamage);
			oldOwner.ClientHeroEvent_OnSkillUse -= new Action<EventInfoSkillUse>(OnSkillUse);
			oldOwner.Status.CalculateStats();
			oldOwner.Status.SetHealth(Mathf.Max(1f, (_escrowedNormalizedHealth ?? 1f) * oldOwner.Status.maxHealth));
			if (!_isShieldBroken)
			{
				FxPlayNewNetworked(fxBreak, oldOwner);
			}
			if (oldOwner.Status.TryGetStatusEffect<Se_HeroOneShotProtection>(out var effect))
			{
				effect.ReenableProtection();
			}
		}
		_escrowedNormalizedHealth = null;
		Network_isShieldBroken = true;
		FxStopNetworked(fxLoop);
	}

	private void OnDamageNegated(EventInfoDamageNegatedByShield obj)
	{
		UpdateFakeMaxHealthAmount();
		if (!_isShieldBroken && obj.shield.amount < 1f)
		{
			FxPlayNewNetworked(fxBreak, owner);
			FxStopNetworked(fxLoop);
			Network_isShieldBroken = true;
		}
	}

	private void FinalStatProcessor(ref FinalStats data)
	{
		_baseMaxHealth = data.maxHealth;
		data.maxHealth = 1f;
	}

	private void UpdateFakeMaxHealthAmount()
	{
		_fakeMaxHealth.strength = _maxShieldAmount - _shield.shield.amount;
	}

	private void OnTakeDamage(EventInfoDamage obj)
	{
		_lastDamageTime = Time.time;
	}

	private void OnSkillUse(EventInfoSkillUse obj)
	{
		HeroSkillLocation type = obj.type;
		if (type != HeroSkillLocation.Movement && type != HeroSkillLocation.Identity)
		{
			ApplyMaxShieldDelta();
			if (_shield.shield.amount > 0f)
			{
				PureDamage(_baseMaxHealth * GetValue(castDamageHpRatio)).SetAttr(DamageAttribute.IgnoreArmor | DamageAttribute.IgnoreDamageImmunity | DamageAttribute.DamageShieldOnly).Dispatch(owner);
				FxPlayNewNetworked(fxCastDamage, owner);
				UpdateFakeMaxHealthAmount();
			}
		}
	}

	private void ApplyMaxShieldDelta()
	{
		if (_shield.IsNullOrInactive() || _shield.shield == null)
		{
			return;
		}
		float maxShieldAmount = _maxShieldAmount;
		if (!Mathf.Approximately(maxShieldAmount, _prevMaxShield))
		{
			float num = maxShieldAmount - _prevMaxShield;
			if (num > 0f && !_isShieldBroken)
			{
				_shield.shield.amount += num;
			}
			_shield.shield.amount = Mathf.Min(_shield.shield.amount, maxShieldAmount);
			_prevMaxShield = maxShieldAmount;
			UpdateFakeMaxHealthAmount();
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!((NetworkBehaviour)this).isServer || !isValid)
		{
			return;
		}
		if (owner.isKnockedOut)
		{
			if (!_wasKnockedOut)
			{
				_wasKnockedOut = true;
				if (owner.Status.TryGetStatusEffect<Se_HeroKnockedOut>(out var effect))
				{
					_escrowedNormalizedHealth = Mathf.Clamp01(effect.reviveHealthMultiplier);
				}
			}
		}
		else
		{
			_wasKnockedOut = false;
		}
		if (_shield.IsNullOrInactive() || _shield.shield == null || owner.IsNullInactiveDeadOrKnockedOut())
		{
			return;
		}
		_maxShieldAmount = _baseMaxHealth * GetValue(shieldHpRatio);
		_maxShieldAmount = ProcessShieldAmount(_maxShieldAmount, owner);
		ApplyMaxShieldDelta();
		if (Time.time - _lastDamageTime > rechargeDelay && !Mathf.Approximately(_shield.shield.amount, _maxShieldAmount))
		{
			if (_isShieldBroken)
			{
				Network_isShieldBroken = false;
				FxPlayNewNetworked(fxRechargeStart, owner);
			}
			_shield.shield.amount = _maxShieldAmount;
			UpdateFakeMaxHealthAmount();
			NotifyUse();
			FxPlayNewNetworked(fxRechargeEnd, owner);
			FxPlayNetworked(fxLoop, owner);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			FxStopNetworked(fxLoop);
		}
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteBool(writer, _isShieldBroken);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x40000L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, _isShieldBroken);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref _isShieldBroken, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x40000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref _isShieldBroken, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
		}
	}
}
