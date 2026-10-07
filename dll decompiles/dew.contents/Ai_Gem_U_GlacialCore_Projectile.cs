using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Ai_Gem_U_GlacialCore_Projectile : StandardProjectile
{
	[SyncVar]
	internal float _procCoefficient;

	internal float _damageAmount;

	private bool _baseScalesCached;

	private Vector3 _effectOnFlyBaseScale;

	private Vector3 _effectOnEntityBaseScale;

	public override bool reuseInRoom => true;

	public float Network_procCoefficient
	{
		get
		{
			return _procCoefficient;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref _procCoefficient, 524288uL, (Action<float, float>)null);
		}
	}

	protected override void OnCreate()
	{
		if (!_baseScalesCached)
		{
			_effectOnFlyBaseScale = effectOnFly.transform.localScale;
			_effectOnEntityBaseScale = effectOnEntity.transform.localScale;
			_baseScalesCached = true;
		}
		effectOnFly.transform.localScale = _effectOnFlyBaseScale * _procCoefficient;
		effectOnEntity.transform.localScale = _effectOnEntityBaseScale * _procCoefficient;
		base.OnCreate();
	}

	protected override void OnEntity(EntityHit hit)
	{
		base.OnEntity(hit);
		DefaultDamage(_damageAmount, _procCoefficient).SetOriginPosition(info.caster.position).SetElemental(ElementalType.Cold).SetAttr(DamageAttribute.ForceMergeNumber)
			.Dispatch(hit.entity, chain);
		Destroy();
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteFloat(writer, _procCoefficient);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x80000L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, _procCoefficient);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _procCoefficient, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x80000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _procCoefficient, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
	}
}
