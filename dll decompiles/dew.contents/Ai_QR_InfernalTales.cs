using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Ai_QR_InfernalTales : TickDamageInstance
{
	[NonSerialized]
	[SyncVar]
	public bool doInnerhits;

	[NonSerialized]
	[SyncVar]
	public float sizeMultiplier = 1f;

	public GameObject fxInnerHitLoop;

	[NonSerialized]
	private GameObject _baseStartEffect;

	[NonSerialized]
	private Vector3 _baseStartEffectScale;

	[NonSerialized]
	private Vector3 _baseStartEffectNoStopScale;

	[NonSerialized]
	private Vector3 _baseInnerHitLoopScale;

	[NonSerialized]
	private Vector3 _baseRangeScale;

	[NonSerialized]
	private ScalingValue _baseDmgFactor;

	[NonSerialized]
	private int _baseTicks;

	[NonSerialized]
	private DewCollider.ColliderShape _baseShape;

	public override bool reuseInRoom => true;

	public bool NetworkdoInnerhits
	{
		get
		{
			return doInnerhits;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref doInnerhits, 128uL, (Action<bool, bool>)null);
		}
	}

	public float NetworksizeMultiplier
	{
		get
		{
			return sizeMultiplier;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref sizeMultiplier, 256uL, (Action<float, float>)null);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		_baseStartEffect = startEffect;
		_baseStartEffectScale = startEffect.transform.localScale;
		_baseStartEffectNoStopScale = ((startEffectNoStop != null) ? startEffectNoStop.transform.localScale : Vector3.one);
		_baseInnerHitLoopScale = ((fxInnerHitLoop != null) ? fxInnerHitLoop.transform.localScale : Vector3.one);
		_baseRangeScale = range.transform.localScale;
		_baseDmgFactor = dmgFactor;
		_baseTicks = ticks;
		_baseShape = range.shape;
	}

	protected override void OnCreate()
	{
		if (doInnerhits)
		{
			startEffect = fxInnerHitLoop;
			range.shape = DewCollider.ColliderShape.Circle;
			range.UpdateProxyCollider();
		}
		else
		{
			startEffect = _baseStartEffect;
			if (range.shape != _baseShape)
			{
				range.shape = _baseShape;
				range.UpdateProxyCollider();
			}
		}
		if (startEffectNoStop != null)
		{
			startEffectNoStop.transform.localScale = _baseStartEffectNoStopScale * sizeMultiplier;
		}
		startEffect.transform.localScale = (doInnerhits ? _baseInnerHitLoopScale : _baseStartEffectScale) * sizeMultiplier;
		range.transform.localScale = _baseRangeScale * sizeMultiplier;
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer && info.caster is Hero_Bismuth hero_Bismuth)
		{
			hero_Bismuth.book.RpcBookCast(hero_Bismuth.book.bookTransform.rotation.Flattened() * Quaternion.Euler(-90f, 0f, 0f));
			hero_Bismuth.SpendAttack();
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		NetworksizeMultiplier = 1f;
		NetworkdoInnerhits = false;
		dmgFactor = _baseDmgFactor;
		ticks = _baseTicks;
	}

	public override void FrameUpdate()
	{
		base.FrameUpdate();
		if (!((UnityEngine.Object)(object)info.caster == null))
		{
			position = info.caster.Visual.GetCenterPosition();
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
			NetworkWriterExtensions.WriteBool(writer, doInnerhits);
			NetworkWriterExtensions.WriteFloat(writer, sizeMultiplier);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x80L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, doInnerhits);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x100L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, sizeMultiplier);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref doInnerhits, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref sizeMultiplier, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x80L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref doInnerhits, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
		}
		if ((num & 0x100L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref sizeMultiplier, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
	}
}
