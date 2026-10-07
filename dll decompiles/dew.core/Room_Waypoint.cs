using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Room_Waypoint : DewNetworkBehaviour, IPlayerPathablePoint
{
	public GameObject lockedEffect;

	public GameObject unlockedEffect;

	public DewCollider dewCollider;

	[CompilerGenerated]
	[SyncVar(hook = "OnIsUnlockedChanged")]
	private bool isUnlocked__BackingField;

	private RoomSection _section;

	public Action<bool, bool> _Mirror_SyncVarHookDelegate__003CisUnlocked_003Ek__BackingField;

	public bool isUnlocked
	{
		[CompilerGenerated]
		get
		{
			return isUnlocked__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			Network_003CisUnlocked_003Ek__BackingField = value;
		}
	}

	Vector3 IPlayerPathablePoint.pathablePosition => ((Component)(object)this).transform.position;

	public bool Network_003CisUnlocked_003Ek__BackingField
	{
		get
		{
			return isUnlocked__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref isUnlocked__BackingField, 1uL, _Mirror_SyncVarHookDelegate__003CisUnlocked_003Ek__BackingField);
		}
	}

	public override void OnStart()
	{
		base.OnStart();
		OnIsUnlockedChanged(oldVal: false, isUnlocked);
	}

	public override void OnLateStartServer()
	{
		base.OnLateStartServer();
		_section = SingletonDewNetworkBehaviour<Room>.instance.GetSectionFromWorldPos(((Component)(object)this).transform.position);
		if (_section != null)
		{
			_section.monsters.onClearCombatArea.AddListener(Unlock);
		}
		dewCollider.receiveEntityCallbacks = true;
		dewCollider.onEntityEnter.AddListener((Entity e) =>
		{
			if (!isUnlocked && e is Hero && (!(_section != null) || (!_section.monsters.isCombatActive && (!_section.monsters.isMarkedAsCombatArea || _section.monsters.didClearCombatArea))))
			{
				Unlock();
			}
		});
		dewCollider.UpdateProxyCollider();
	}

	[Server]
	public void Unlock()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Room_Waypoint::Unlock()' called when server was not active");
			return;
		}
		Network_003CisUnlocked_003Ek__BackingField = true;
		SingletonDewNetworkBehaviour<Room>.instance.AddUnlockedWaypoint(this);
	}

	[Server]
	public void Lock()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Room_Waypoint::Lock()' called when server was not active");
			return;
		}
		Network_003CisUnlocked_003Ek__BackingField = true;
		SingletonDewNetworkBehaviour<Room>.instance.RemoveUnlockedWaypoint(this);
	}

	private void OnIsUnlockedChanged(bool oldVal, bool newVal)
	{
		if (newVal)
		{
			FxPlay(unlockedEffect);
			FxStop(lockedEffect);
		}
		else
		{
			FxPlay(lockedEffect);
			FxStop(unlockedEffect);
		}
	}

	public Room_Waypoint()
	{
		_Mirror_SyncVarHookDelegate__003CisUnlocked_003Ek__BackingField = OnIsUnlockedChanged;
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		((NetworkBehaviour)this).SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteBool(writer, isUnlocked__BackingField);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 1L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, isUnlocked__BackingField);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		((NetworkBehaviour)this).DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref isUnlocked__BackingField, _Mirror_SyncVarHookDelegate__003CisUnlocked_003Ek__BackingField, NetworkReaderExtensions.ReadBool(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 1L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref isUnlocked__BackingField, _Mirror_SyncVarHookDelegate__003CisUnlocked_003Ek__BackingField, NetworkReaderExtensions.ReadBool(reader));
		}
	}
}
