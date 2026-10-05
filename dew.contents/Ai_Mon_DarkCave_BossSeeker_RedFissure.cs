using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Ai_Mon_DarkCave_BossSeeker_RedFissure : InstantDamageInstance
{
	[NonSerialized]
	[SyncVar]
	public float scaleMultiplier = 1f;

	public float stunDuration = 2f;

	private bool _baseScaleCached;

	private Vector3 _rangeBaseScale;

	private Vector3 _startEffectBaseScale;

	private Vector3 _startEffectNoStopBaseScale;

	private Vector3 _mainEffectBaseScale;

	private Vector3 _mainEffectAfterDelayBaseScale;

	public override bool reuseInRoom => true;

	public float NetworkscaleMultiplier
	{
		get
		{
			return scaleMultiplier;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref scaleMultiplier, 128uL, (Action<float, float>)null);
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		NetworkscaleMultiplier = 1f;
	}

	protected override void OnCreate()
	{
		if (!_baseScaleCached)
		{
			_rangeBaseScale = range.transform.localScale;
			if (startEffect != null)
			{
				_startEffectBaseScale = startEffect.transform.localScale;
			}
			if (startEffectNoStop != null)
			{
				_startEffectNoStopBaseScale = startEffectNoStop.transform.localScale;
			}
			if (mainEffect != null)
			{
				_mainEffectBaseScale = mainEffect.transform.localScale;
			}
			if (mainEffectAfterDelay != null)
			{
				_mainEffectAfterDelayBaseScale = mainEffectAfterDelay.transform.localScale;
			}
			_baseScaleCached = true;
		}
		range.transform.localScale = _rangeBaseScale * scaleMultiplier;
		if (startEffect != null)
		{
			startEffect.transform.localScale = _startEffectBaseScale * scaleMultiplier;
		}
		if (startEffectNoStop != null)
		{
			startEffectNoStop.transform.localScale = _startEffectNoStopBaseScale * scaleMultiplier;
		}
		if (mainEffect != null)
		{
			mainEffect.transform.localScale = _mainEffectBaseScale * scaleMultiplier;
		}
		if (mainEffectAfterDelay != null)
		{
			mainEffectAfterDelay.transform.localScale = _mainEffectAfterDelayBaseScale * scaleMultiplier;
		}
		position = info.point;
		base.OnCreate();
	}

	protected override void OnHit(Entity entity)
	{
		base.OnHit(entity);
		ApplyElemental(ElementalType.Fire, entity);
		CreateBasicEffect(entity, new StunEffect(), stunDuration, "fissureStun", DuplicateEffectBehavior.UsePrevious);
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteFloat(writer, scaleMultiplier);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x80L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, scaleMultiplier);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref scaleMultiplier, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x80L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref scaleMultiplier, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
	}
}
