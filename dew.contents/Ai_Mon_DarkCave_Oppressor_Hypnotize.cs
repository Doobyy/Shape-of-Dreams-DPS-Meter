using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Ai_Mon_DarkCave_Oppressor_Hypnotize : InstantDamageInstance
{
	private class Ad_LastHypnotizeStun
	{
		public float time;
	}

	public float slowAmount;

	public float slowDuration;

	public bool isSlowDecay;

	public float stunDuration;

	[NonSerialized]
	[SyncVar]
	public float scaleMultiplier = 1f;

	private bool _baseScaleCached;

	private Vector3 _rangeBaseScale;

	private Vector3 _startEffectNoStopBaseScale;

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
			_startEffectNoStopBaseScale = startEffectNoStop.transform.localScale;
			_baseScaleCached = true;
		}
		range.transform.localScale = _rangeBaseScale * scaleMultiplier;
		startEffectNoStop.transform.localScale = _startEffectNoStopBaseScale * scaleMultiplier;
		base.OnCreate();
	}

	protected override void OnHit(Entity entity)
	{
		base.OnHit(entity);
		if (entity.Status.darkStack >= 5)
		{
			if (!entity.TryGetData<Ad_LastHypnotizeStun>(out var data))
			{
				data = new Ad_LastHypnotizeStun();
				entity.AddData(data);
			}
			if (Time.time - data.time > 2f)
			{
				CreateBasicEffect(entity, new StunEffect(), stunDuration, "OppressorHypnotizeStun", DuplicateEffectBehavior.UsePrevious);
				if (!entity.Status.hasCrowdControlImmunity)
				{
					data.time = Time.time;
				}
			}
		}
		else
		{
			CreateBasicEffect(entity, new SlowEffect
			{
				decay = isSlowDecay,
				strength = slowAmount
			}, slowDuration, "OppressorHypnotizeSlow", DuplicateEffectBehavior.UsePrevious);
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
