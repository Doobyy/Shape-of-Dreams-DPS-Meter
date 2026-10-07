using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

[DewResourceLink(ResourceLinkBy.None)]
public abstract class Room_Trap_Toggleable : Actor, IToggleableTrap, IBanRoomNodesNearby, IBanCampsNearby
{
	public GameObject fxOn;

	public GameObject fxOff;

	[CompilerGenerated]
	[SyncVar(hook = "OnIsOnChanged")]
	private bool isOn__BackingField;

	public Action<bool, bool> _Mirror_SyncVarHookDelegate__003CisOn_003Ek__BackingField;

	public bool isOn
	{
		[CompilerGenerated]
		get
		{
			return isOn__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			Network_003CisOn_003Ek__BackingField = value;
		}
	}

	public float startTime { get; private set; } = float.NegativeInfinity;

	public bool Network_003CisOn_003Ek__BackingField
	{
		get
		{
			return isOn__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref isOn__BackingField, 8uL, _Mirror_SyncVarHookDelegate__003CisOn_003Ek__BackingField);
		}
	}

	private void OnIsOnChanged(bool oldV, bool newV)
	{
		if (newV)
		{
			FxPlay(fxOn);
			FxStop(fxOff);
		}
		else
		{
			FxPlay(fxOff);
			FxStop(fxOn);
		}
	}

	protected virtual void OnStartTrapServer()
	{
	}

	protected virtual void OnStopTrapServer()
	{
	}

	[Server]
	public void StartTrap()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Room_Trap_Toggleable::StartTrap()' called when server was not active");
		}
		else if (!isOn)
		{
			Network_003CisOn_003Ek__BackingField = true;
			startTime = Time.time;
			OnStartTrapServer();
		}
	}

	[Server]
	public void StopTrap()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Room_Trap_Toggleable::StopTrap()' called when server was not active");
		}
		else if (isOn)
		{
			Network_003CisOn_003Ek__BackingField = false;
			OnStopTrapServer();
		}
	}

	protected Room_Trap_Toggleable()
	{
		_Mirror_SyncVarHookDelegate__003CisOn_003Ek__BackingField = OnIsOnChanged;
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteBool(writer, isOn__BackingField);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 8L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, isOn__BackingField);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref isOn__BackingField, _Mirror_SyncVarHookDelegate__003CisOn_003Ek__BackingField, NetworkReaderExtensions.ReadBool(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 8L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref isOn__BackingField, _Mirror_SyncVarHookDelegate__003CisOn_003Ek__BackingField, NetworkReaderExtensions.ReadBool(reader));
		}
	}
}
