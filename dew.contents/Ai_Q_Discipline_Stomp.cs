using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Ai_Q_Discipline_Stomp : InstantDamageInstance
{
	[NonSerialized]
	[SyncVar]
	public float radiusMultiplier = 1f;

	[NonSerialized]
	public float damageMultiplier = 1f;

	[NonSerialized]
	public float stunDuration;

	[NonSerialized]
	public new float knockupAmount;

	public GameObject fxCritEffect;

	private Vector3 _baseRangeScale;

	private Vector3 _baseStartEffectNoStopScale;

	public override bool reuseInRoom => true;

	public float NetworkradiusMultiplier
	{
		get
		{
			return radiusMultiplier;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref radiusMultiplier, 128uL, (Action<float, float>)null);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		_baseRangeScale = range.transform.localScale;
		_baseStartEffectNoStopScale = startEffectNoStop.transform.localScale;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		NetworkradiusMultiplier = 1f;
		damageMultiplier = 1f;
		stunDuration = 0f;
		knockupAmount = 0f;
	}

	protected override void OnCreate()
	{
		range.transform.localScale = _baseRangeScale * radiusMultiplier;
		startEffectNoStop.transform.localScale = _baseStartEffectNoStopScale * radiusMultiplier;
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer && damageMultiplier > 1.5f)
		{
			FxPlayNetworked(fxCritEffect);
		}
	}

	protected override void OnBeforeDispatchDamage(ref DamageData dmg, Entity target)
	{
		base.OnBeforeDispatchDamage(ref dmg, target);
		if (strengthMultiplier > 1.01f)
		{
			dmg.SetAttr(DamageAttribute.IsCrit);
		}
		dmg.ApplyRawMultiplier(damageMultiplier);
	}

	protected override void OnHit(Entity entity)
	{
		base.OnHit(entity);
		if (stunDuration > 0f)
		{
			CreateBasicEffect(entity, new StunEffect(), stunDuration);
		}
		if (knockupAmount > 0f)
		{
			entity.Visual.KnockUp(knockupAmount, isFriendly: false);
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
			NetworkWriterExtensions.WriteFloat(writer, radiusMultiplier);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x80L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, radiusMultiplier);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref radiusMultiplier, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x80L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref radiusMultiplier, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
	}
}
