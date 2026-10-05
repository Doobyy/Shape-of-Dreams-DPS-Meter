using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Gem_E_Omega : Gem
{
	public int maxChargeCount;

	public ScalingValue maxChargeAmount;

	public GameObject fxCharge;

	public GameObject fxChargeComplete;

	public GameObject fxActivate;

	public GameObject fxActivateMax;

	public GameObject fxCast;

	public GameObject fxCastMax;

	[SyncVar]
	private float _currentChargeRate;

	private float _lastChargeTime;

	public float chargeRate => GetValue(maxChargeAmount) / (float)maxChargeCount;

	public float Network_currentChargeRate
	{
		get
		{
			return _currentChargeRate;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref _currentChargeRate, 262144uL, (Action<float, float>)null);
		}
	}

	public override void OnEquipGem(Hero newOwner)
	{
		base.OnEquipGem(newOwner);
		if (((NetworkBehaviour)this).isServer)
		{
			_lastChargeTime = 0f;
			Network_currentChargeRate = 0f;
			numberDisplay = 0;
		}
	}

	public override void OnEquipSkill(SkillTrigger newSkill)
	{
		base.OnEquipSkill(newSkill);
		if (((NetworkBehaviour)this).isServer)
		{
			_lastChargeTime = 0f;
			Network_currentChargeRate = 0f;
			numberDisplay = 0;
		}
	}

	public override void OnUnequipSkill(SkillTrigger oldSkill)
	{
		base.OnUnequipSkill(oldSkill);
		if (((NetworkBehaviour)this).isServer)
		{
			_lastChargeTime = 0f;
			Network_currentChargeRate = 0f;
			numberDisplay = 0;
		}
	}

	protected override void OnCastCompleteBeforePrepare(EventInfoCast info)
	{
		base.OnCastCompleteBeforePrepare(info);
		if ((UnityEngine.Object)(object)owner == null || !info.trigger.configs[info.configIndex].canConsumeCastBonus)
		{
			return;
		}
		bool isFullyCharged = numberDisplay == maxChargeCount;
		FxPlayNewNetworked(isFullyCharged ? fxCastMax : fxCast, owner);
		float amp = _currentChargeRate;
		info.instance.dealtDamageProcessor.Add(delegate(ref DamageData data, Actor actor, Entity target)
		{
			if (isValid && !data.IsAmountModifiedBy(this) && !((UnityEngine.Object)(object)owner == null) && owner.CheckEnemyOrNeutral(target))
			{
				data.ApplyAmplification(amp);
				data.SetAttr(DamageAttribute.IsCrit);
				data.SetAmountModifiedBy(this);
				GameObject effect = (isFullyCharged ? fxActivateMax : fxActivate);
				FxPlayNewNetworked(effect, target);
			}
		});
		NotifyUse();
		Network_currentChargeRate = 0f;
		_lastChargeTime = 0f;
		numberDisplay = 0;
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		_lastChargeTime += dt;
		if (_lastChargeTime < 0.99f)
		{
			return;
		}
		_lastChargeTime = 0f;
		if (owner.IsNullInactiveDeadOrKnockedOut() || (UnityEngine.Object)(object)skill == null || skill.currentCharges[0] <= 0)
		{
			return;
		}
		float value = GetValue(maxChargeAmount);
		if (!(_currentChargeRate >= value))
		{
			FxPlayNewNetworked(fxCharge, owner);
			Network_currentChargeRate = _currentChargeRate + chargeRate;
			numberDisplay = Mathf.FloorToInt(_currentChargeRate / GetValue(maxChargeAmount) * (float)maxChargeCount);
			if (_currentChargeRate >= value)
			{
				FxPlayNewNetworked(fxChargeComplete, owner);
				numberDisplay = maxChargeCount;
				Network_currentChargeRate = value;
			}
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
			NetworkWriterExtensions.WriteFloat(writer, _currentChargeRate);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x40000L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, _currentChargeRate);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _currentChargeRate, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x40000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _currentChargeRate, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
	}
}
