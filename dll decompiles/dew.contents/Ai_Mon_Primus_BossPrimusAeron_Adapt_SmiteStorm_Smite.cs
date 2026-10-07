using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Ai_Mon_Primus_BossPrimusAeron_Adapt_SmiteStorm_Smite : InstantDamageInstance
{
	[NonSerialized]
	[SyncVar]
	public bool disableSound;

	[NonSerialized]
	[SyncVar]
	public float sizeMultiplier;

	private Vector3 _pristineStartEffectScale;

	private Vector3 _pristineStartEffectNoStopScale;

	private Vector3 _pristineMainEffectScale;

	private Vector3 _pristineMainEffectAfterDelayScale;

	private DewAudioSource[] _pristineAudios;

	private bool[] _pristineAudioEnabled;

	public override bool reuseInRoom => true;

	public bool NetworkdisableSound
	{
		get
		{
			return disableSound;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref disableSound, 128uL, (Action<bool, bool>)null);
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
		if (startEffect != null)
		{
			_pristineStartEffectScale = startEffect.transform.localScale;
		}
		if (startEffectNoStop != null)
		{
			_pristineStartEffectNoStopScale = startEffectNoStop.transform.localScale;
		}
		if (mainEffect != null)
		{
			_pristineMainEffectScale = mainEffect.transform.localScale;
		}
		if (mainEffectAfterDelay != null)
		{
			_pristineMainEffectAfterDelayScale = mainEffectAfterDelay.transform.localScale;
		}
		_pristineAudios = ((Component)(object)this).GetComponentsInChildren<DewAudioSource>(true);
		_pristineAudioEnabled = new bool[_pristineAudios.Length];
		for (int i = 0; i < _pristineAudios.Length; i++)
		{
			_pristineAudioEnabled[i] = _pristineAudios[i].enabled;
		}
	}

	protected override void OnCreate()
	{
		if (startEffect != null)
		{
			startEffect.transform.localScale *= sizeMultiplier;
		}
		if (startEffectNoStop != null)
		{
			startEffectNoStop.transform.localScale *= sizeMultiplier;
		}
		if (mainEffect != null)
		{
			mainEffect.transform.localScale *= sizeMultiplier;
		}
		if (mainEffectAfterDelay != null)
		{
			mainEffectAfterDelay.transform.localScale *= sizeMultiplier;
		}
		if (disableSound)
		{
			ListReturnHandle<DewAudioSource> handle;
			foreach (DewAudioSource item in ((Component)(object)this).GetComponentsInChildrenNonAlloc(out handle))
			{
				item.enabled = false;
			}
			handle.Return();
		}
		base.OnCreate();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (startEffect != null)
		{
			startEffect.transform.localScale = _pristineStartEffectScale;
		}
		if (startEffectNoStop != null)
		{
			startEffectNoStop.transform.localScale = _pristineStartEffectNoStopScale;
		}
		if (mainEffect != null)
		{
			mainEffect.transform.localScale = _pristineMainEffectScale;
		}
		if (mainEffectAfterDelay != null)
		{
			mainEffectAfterDelay.transform.localScale = _pristineMainEffectAfterDelayScale;
		}
		if (_pristineAudios == null)
		{
			return;
		}
		for (int i = 0; i < _pristineAudios.Length; i++)
		{
			if (_pristineAudios[i] != null)
			{
				_pristineAudios[i].enabled = _pristineAudioEnabled[i];
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
			NetworkWriterExtensions.WriteBool(writer, disableSound);
			NetworkWriterExtensions.WriteFloat(writer, sizeMultiplier);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x80L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, disableSound);
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
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref disableSound, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref sizeMultiplier, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x80L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref disableSound, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
		}
		if ((num & 0x100L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref sizeMultiplier, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
	}
}
