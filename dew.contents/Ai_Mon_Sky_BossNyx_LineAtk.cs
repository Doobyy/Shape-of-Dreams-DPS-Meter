using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Ai_Mon_Sky_BossNyx_LineAtk : InstantDamageInstance
{
	public GameObject fxStartAnim;

	public GameObject fxEndAnim;

	public GameObject fxTelegraph;

	private DewAudioSource[] _mainEffectAudioSources;

	private bool[] _mainEffectAudioEnabled;

	private FxCameraShake[] _mainEffectShakes;

	private bool[] _mainEffectShakeEnabled;

	[NonSerialized]
	public bool disableAnimations;

	[NonSerialized]
	[SyncVar]
	public bool disableAudioAndShake;

	public override bool reuseInRoom => true;

	public bool NetworkdisableAudioAndShake
	{
		get
		{
			return disableAudioAndShake;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref disableAudioAndShake, 128uL, (Action<bool, bool>)null);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		if (mainEffectAfterDelay != null)
		{
			_mainEffectAudioSources = mainEffectAfterDelay.GetComponents<DewAudioSource>();
			_mainEffectAudioEnabled = new bool[_mainEffectAudioSources.Length];
			for (int i = 0; i < _mainEffectAudioSources.Length; i++)
			{
				_mainEffectAudioEnabled[i] = _mainEffectAudioSources[i].enabled;
			}
			_mainEffectShakes = mainEffectAfterDelay.GetComponents<FxCameraShake>();
			_mainEffectShakeEnabled = new bool[_mainEffectShakes.Length];
			for (int j = 0; j < _mainEffectShakes.Length; j++)
			{
				_mainEffectShakeEnabled[j] = _mainEffectShakes[j].enabled;
			}
		}
	}

	protected override void OnCreate()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			CreateBasicEffect(info.caster, new UnstoppableEffect(), float.PositiveInfinity).DestroyOnDestroy(this);
			FxPlayNetworked(fxTelegraph);
			if (!disableAnimations)
			{
				FxPlayNetworked(fxStartAnim, info.caster);
			}
		}
		if (disableAudioAndShake)
		{
			List<DewAudioSource> componentsNonAlloc = mainEffectAfterDelay.GetComponentsNonAlloc(out ListReturnHandle<DewAudioSource> handle);
			List<FxCameraShake> componentsNonAlloc2 = mainEffectAfterDelay.GetComponentsNonAlloc(out ListReturnHandle<FxCameraShake> handle2);
			foreach (DewAudioSource item in componentsNonAlloc)
			{
				item.enabled = false;
			}
			foreach (FxCameraShake item2 in componentsNonAlloc2)
			{
				item2.enabled = false;
			}
			handle.Return();
			handle2.Return();
		}
		base.OnCreate();
	}

	protected override void OnAfterDelay()
	{
		base.OnAfterDelay();
		if (((NetworkBehaviour)this).isServer && !disableAnimations)
		{
			FxPlayNetworked(fxEndAnim, info.caster);
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		disableAnimations = false;
		NetworkdisableAudioAndShake = false;
		if (_mainEffectAudioSources != null)
		{
			for (int i = 0; i < _mainEffectAudioSources.Length; i++)
			{
				_mainEffectAudioSources[i].enabled = _mainEffectAudioEnabled[i];
			}
		}
		if (_mainEffectShakes != null)
		{
			for (int j = 0; j < _mainEffectShakes.Length; j++)
			{
				_mainEffectShakes[j].enabled = _mainEffectShakeEnabled[j];
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
			NetworkWriterExtensions.WriteBool(writer, disableAudioAndShake);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x80L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, disableAudioAndShake);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref disableAudioAndShake, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x80L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref disableAudioAndShake, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
		}
	}
}
