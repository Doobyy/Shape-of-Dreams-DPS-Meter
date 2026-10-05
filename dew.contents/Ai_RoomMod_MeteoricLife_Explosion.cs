using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Ai_RoomMod_MeteoricLife_Explosion : InstantDamageInstance
{
	public GameObject[] scaleObjects;

	[SyncVar]
	internal float size;

	public override bool reuseInRoom => true;

	public float Networksize
	{
		get
		{
			return size;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref size, 128uL, (Action<float, float>)null);
		}
	}

	protected override void OnCreate()
	{
		GameObject[] array = scaleObjects;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].transform.localScale = Vector3.one * size;
		}
		base.OnCreate();
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)info.caster != null)
		{
			info.caster.Visual.DisableRenderers();
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
			NetworkWriterExtensions.WriteFloat(writer, size);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x80L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, size);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref size, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x80L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref size, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
	}
}
