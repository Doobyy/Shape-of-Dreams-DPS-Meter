using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Ge_Shrine_Anitya : GameEffect
{
	public int countForAction = 3;

	[SerializeField]
	[SaveVar(SaveVarFlags.Default)]
	[SyncVar]
	public int count;

	public int Networkcount
	{
		get
		{
			return count;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<int>(value, ref count, 8uL, (Action<int, int>)null);
		}
	}

	public static bool IsDoubleSpawn()
	{
		Ge_Shrine_Anitya ge_Shrine_Anitya = Dew.FindActorOfType<Ge_Shrine_Anitya>();
		if ((UnityEngine.Object)(object)ge_Shrine_Anitya == null)
		{
			return false;
		}
		return ge_Shrine_Anitya.count >= ge_Shrine_Anitya.countForAction;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnZoneLoadStarted += new Action<EventInfoLoadZone>(OnZoneLoadStarted);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)NetworkedManagerBase<ZoneManager>.instance != null)
		{
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnZoneLoadStarted -= new Action<EventInfoLoadZone>(OnZoneLoadStarted);
		}
	}

	private void OnZoneLoadStarted(EventInfoLoadZone obj)
	{
		if (obj.isTraveling)
		{
			Destroy();
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
			NetworkWriterExtensions.WriteInt(writer, count);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 8L) != 0L)
		{
			NetworkWriterExtensions.WriteInt(writer, count);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref count, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 8L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref count, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
		}
	}
}
