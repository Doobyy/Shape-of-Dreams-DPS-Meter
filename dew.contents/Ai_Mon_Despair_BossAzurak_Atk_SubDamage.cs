using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Ai_Mon_Despair_BossAzurak_Atk_SubDamage : InstantDamageInstance
{
	[NonSerialized]
	[SyncVar]
	public bool playSounds;

	private DewAudioSource[] _audios;

	public override bool reuseInRoom => true;

	public bool NetworkplaySounds
	{
		get
		{
			return playSounds;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref playSounds, 128uL, (Action<bool, bool>)null);
		}
	}

	protected override void OnCreate()
	{
		if (_audios == null)
		{
			_audios = ((Component)(object)this).GetComponentsInChildren<DewAudioSource>(true);
		}
		for (int i = 0; i < _audios.Length; i++)
		{
			if (_audios[i] != null)
			{
				_audios[i].enabled = playSounds;
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
			NetworkWriterExtensions.WriteBool(writer, playSounds);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x80L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, playSounds);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref playSounds, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x80L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref playSounds, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
		}
	}
}
