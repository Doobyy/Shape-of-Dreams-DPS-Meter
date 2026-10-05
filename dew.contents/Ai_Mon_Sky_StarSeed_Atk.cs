using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Ai_Mon_Sky_StarSeed_Atk : InstantDamageInstance
{
	[NonSerialized]
	[SyncVar]
	public bool isMegaExplosion;

	private bool _baseCaptured;

	private Vector3 _baseRangeScale;

	private Vector3 _baseStartEffectScale;

	private DewAudioSource[] _audioSources;

	private float[] _baseAudioPitch;

	public override bool reuseInRoom => true;

	public bool NetworkisMegaExplosion
	{
		get
		{
			return isMegaExplosion;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref isMegaExplosion, 128uL, (Action<bool, bool>)null);
		}
	}

	protected override void OnCreate()
	{
		if (!_baseCaptured)
		{
			_baseRangeScale = range.transform.localScale;
			_baseStartEffectScale = startEffectNoStop.transform.localScale;
			List<DewAudioSource> componentsInChildrenNonAlloc = startEffectNoStop.GetComponentsInChildrenNonAlloc(out ListReturnHandle<DewAudioSource> handle);
			_audioSources = new DewAudioSource[componentsInChildrenNonAlloc.Count];
			_baseAudioPitch = new float[componentsInChildrenNonAlloc.Count];
			for (int i = 0; i < componentsInChildrenNonAlloc.Count; i++)
			{
				_audioSources[i] = componentsInChildrenNonAlloc[i];
				_baseAudioPitch[i] = componentsInChildrenNonAlloc[i].pitchMultiplier;
			}
			handle.Return();
			_baseCaptured = true;
		}
		if (isMegaExplosion)
		{
			range.transform.localScale = _baseRangeScale * 1.5f;
			startEffectNoStop.transform.localScale = _baseStartEffectScale * 1.5f;
			for (int j = 0; j < _audioSources.Length; j++)
			{
				_audioSources[j].pitchMultiplier = _baseAudioPitch[j] * 0.7f;
			}
		}
		else
		{
			range.transform.localScale = _baseRangeScale;
			startEffectNoStop.transform.localScale = _baseStartEffectScale;
			for (int k = 0; k < _audioSources.Length; k++)
			{
				_audioSources[k].pitchMultiplier = _baseAudioPitch[k];
			}
		}
		base.OnCreate();
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteBool(writer, isMegaExplosion);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x80L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, isMegaExplosion);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref isMegaExplosion, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x80L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref isMegaExplosion, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
		}
	}
}
