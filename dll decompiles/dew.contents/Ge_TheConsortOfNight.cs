using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Ge_TheConsortOfNight : GameEffect
{
	public Sprite sprite;

	public Color color;

	public int countForAction = 3;

	public SafeAction Event_OnCountIncrease;

	[SerializeField]
	[SaveVar(SaveVarFlags.Default)]
	[SyncVar(hook = "OnCountIncrease")]
	public int count;

	public Action<int, int> _Mirror_SyncVarHookDelegate_count;

	public int Networkcount
	{
		get
		{
			return count;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<int>(value, ref count, 8uL, _Mirror_SyncVarHookDelegate_count);
		}
	}

	public static bool EnableSpawnErebos()
	{
		Ge_TheConsortOfNight ge_TheConsortOfNight = Dew.FindActorOfType<Ge_TheConsortOfNight>();
		if ((UnityEngine.Object)(object)ge_TheConsortOfNight == null)
		{
			return false;
		}
		return ge_TheConsortOfNight.count >= ge_TheConsortOfNight.countForAction;
	}

	private void OnCountIncrease(int oldVal, int newVal)
	{
		if (!((NetworkBehaviour)this).isServer || oldVal >= countForAction)
		{
			return;
		}
		try
		{
			Event_OnCountIncrease?.Invoke();
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		OnRoomLoadStarted(new EventInfoLoadRoom
		{
			toIndex = NetworkedManagerBase<ZoneManager>.instance.currentNodeIndex
		});
		NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoadStarted += new Action<EventInfoLoadRoom>(OnRoomLoadStarted);
		if (((NetworkBehaviour)this).isServer)
		{
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnZoneLoadStarted += new Action<EventInfoLoadZone>(OnZoneLoadStarted);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if ((UnityEngine.Object)(object)NetworkedManagerBase<ZoneManager>.instance != null)
		{
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoadStarted -= new Action<EventInfoLoadRoom>(OnRoomLoadStarted);
			if (((NetworkBehaviour)this).isServer)
			{
				NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnZoneLoadStarted -= new Action<EventInfoLoadZone>(OnZoneLoadStarted);
			}
		}
	}

	private void OnZoneLoadStarted(EventInfoLoadZone obj)
	{
		if (obj.isTraveling)
		{
			Destroy();
		}
	}

	private void OnRoomLoadStarted(EventInfoLoadRoom obj)
	{
		if (EnableSpawnErebos() && NetworkedManagerBase<ZoneManager>.instance.nodes[obj.toIndex].type == WorldNodeType.ExitBoss && SingletonBehaviour<Special_RoomAnnouncer>.instance == null)
		{
			Special_RoomAnnouncer special_RoomAnnouncer = ((Component)(object)this).gameObject.AddComponent<Special_RoomAnnouncer>();
			special_RoomAnnouncer.announceEvent = true;
			special_RoomAnnouncer.color = color;
			special_RoomAnnouncer.key = "Special_TheConsortOfNight_Warning";
			special_RoomAnnouncer.sprite = sprite;
		}
	}

	public Ge_TheConsortOfNight()
	{
		_Mirror_SyncVarHookDelegate_count = OnCountIncrease;
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
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref count, _Mirror_SyncVarHookDelegate_count, NetworkReaderExtensions.ReadInt(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 8L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref count, _Mirror_SyncVarHookDelegate_count, NetworkReaderExtensions.ReadInt(reader));
		}
	}
}
