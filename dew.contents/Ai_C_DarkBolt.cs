using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Ai_C_DarkBolt : StandardProjectile
{
	public ScalingValue damage;

	public float attackEffectStrength = 1f;

	public int penetrationCount = 1;

	public float ampEmpoweredDmg;

	public ScalingValue empowerChance;

	public float empoweredCooldownReductionRatio;

	public GameObject fxEmpowered;

	public GameObject fxEmpoweredHit;

	private int _hitCount;

	[SyncVar]
	private bool _isEmpowered;

	private GameObject _baseEffectOnFly;

	public override bool reuseInRoom => true;

	public bool Network_isEmpowered
	{
		get
		{
			return _isEmpowered;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref _isEmpowered, 524288uL, (Action<bool, bool>)null);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		_baseEffectOnFly = effectOnFly;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		_hitCount = 0;
		effectOnFly = _baseEffectOnFly;
	}

	protected override void OnCreate()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			Network_isEmpowered = UnityEngine.Random.value * 100f < GetValue(empowerChance);
			if (_isEmpowered && (UnityEngine.Object)(object)firstTrigger != null)
			{
				ApplyCooldownReductionByRatio(firstTrigger, empoweredCooldownReductionRatio);
			}
		}
		if (_isEmpowered)
		{
			effectOnFly = fxEmpowered;
		}
		base.OnCreate();
	}

	protected override void OnEntity(EntityHit hit)
	{
		base.OnEntity(hit);
		DamageData damageData = Damage(damage).SetDirection(rotation).DoAttackEffect(AttackEffectType.Others, attackEffectStrength).SetElemental(ElementalType.Dark);
		if (_isEmpowered)
		{
			FxPlayNewNetworked(fxEmpoweredHit, hit.entity);
			damageData.ApplyAmplification(ampEmpoweredDmg);
			damageData.SetAttr(DamageAttribute.IsCrit);
		}
		damageData.Dispatch(hit.entity);
		_hitCount++;
		if (_hitCount > penetrationCount)
		{
			Destroy();
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
			NetworkWriterExtensions.WriteBool(writer, _isEmpowered);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x80000L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, _isEmpowered);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref _isEmpowered, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x80000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref _isEmpowered, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
		}
	}
}
