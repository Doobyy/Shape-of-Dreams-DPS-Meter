using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Ai_Mon_Special_BossPolaris_Holy_Atk_Explosion : InstantDamageInstance
{
	[NonSerialized]
	[SyncVar]
	public float sizeMultiplier = 1f;

	private Vector3 _baseStartEffectScale;

	private Vector3 _baseRangeScale;

	private bool _cachedBaseScales;

	public float NetworksizeMultiplier
	{
		get
		{
			return sizeMultiplier;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref sizeMultiplier, 128uL, (Action<float, float>)null);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		if (!_cachedBaseScales)
		{
			if ((bool)startEffectNoStop)
			{
				_baseStartEffectScale = startEffectNoStop.transform.localScale;
			}
			_baseRangeScale = range.transform.localScale;
			_cachedBaseScales = true;
		}
	}

	protected override void OnCreate()
	{
		if ((bool)startEffectNoStop)
		{
			startEffectNoStop.transform.localScale = _baseStartEffectScale * sizeMultiplier;
		}
		range.transform.localScale = _baseRangeScale * sizeMultiplier;
		base.OnCreate();
	}

	protected override void OnHit(Entity entity)
	{
		base.OnHit(entity);
		if (!entity.Status.hasDamageImmunity)
		{
			if (entity.Status.TryGetStatusEffect<Se_Mon_Special_BossPolaris_CleansingFlame>(out var effect))
			{
				effect.ResetTimer();
			}
			else
			{
				CreateStatusEffect<Se_Mon_Special_BossPolaris_CleansingFlame>(entity);
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
			NetworkWriterExtensions.WriteFloat(writer, sizeMultiplier);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x80L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, sizeMultiplier);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref sizeMultiplier, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x80L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref sizeMultiplier, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
	}
}
