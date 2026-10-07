using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Sum_Q_SylvanCall_LeafHound : Summon
{
	public GameObject leafPuppyHatObject;

	[SyncVar]
	private bool _hasLeafPuppyHat;

	public bool Network_hasLeafPuppyHat
	{
		get
		{
			return _hasLeafPuppyHat;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref _hasLeafPuppyHat, 256uL, (Action<bool, bool>)null);
		}
	}

	protected override void OnPrepare()
	{
		base.OnPrepare();
		Network_hasLeafPuppyHat = UnityEngine.Random.value < 0.02f;
	}

	protected override void OnCreate()
	{
		leafPuppyHatObject.SetActive(_hasLeafPuppyHat);
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			Ability.attackAbility.currentConfig.postDelay = 0f;
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
			NetworkWriterExtensions.WriteBool(writer, _hasLeafPuppyHat);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x100L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, _hasLeafPuppyHat);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref _hasLeafPuppyHat, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x100L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref _hasLeafPuppyHat, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
		}
	}
}
