using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class GenericTransformSync : DewNetworkBehaviour
{
	public enum SyncMode
	{
		Default
	}

	public SyncMode mode;

	private static readonly Vector3 NaNPosition = new Vector3(float.NaN, float.NaN, float.NaN);

	private static readonly Quaternion NaNRotation = new Quaternion(float.NaN, float.NaN, float.NaN, float.NaN);

	[SyncVar]
	private Vector3 _pos = NaNPosition;

	[SyncVar]
	private Quaternion _rot = NaNRotation;

	public Vector3 Network_pos
	{
		get
		{
			return _pos;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<Vector3>(value, ref _pos, 1uL, (Action<Vector3, Vector3>)null);
		}
	}

	public Quaternion Network_rot
	{
		get
		{
			return _rot;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<Quaternion>(value, ref _rot, 2uL, (Action<Quaternion, Quaternion>)null);
		}
	}

	public override void OnStopServer()
	{
		base.OnStopServer();
		Network_pos = NaNPosition;
		Network_rot = NaNRotation;
	}

	public override void OnStopClient()
	{
		base.OnStopClient();
		Network_pos = NaNPosition;
		Network_rot = NaNRotation;
	}

	public override void LogicUpdate(float dt)
	{
		base.LogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer)
		{
			Network_pos = ((Component)(object)this).transform.position;
			Network_rot = ((Component)(object)this).transform.rotation;
		}
	}

	public override void FrameUpdate()
	{
		base.FrameUpdate();
		if (!((NetworkBehaviour)this).isServer && !_pos.IsNaN())
		{
			((Component)(object)this).transform.position = _pos;
		}
		if (!((NetworkBehaviour)this).isServer && !_rot.IsNaN())
		{
			((Component)(object)this).transform.rotation = _rot;
		}
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		((NetworkBehaviour)this).SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteVector3(writer, _pos);
			NetworkWriterExtensions.WriteQuaternion(writer, _rot);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 1L) != 0L)
		{
			NetworkWriterExtensions.WriteVector3(writer, _pos);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 2L) != 0L)
		{
			NetworkWriterExtensions.WriteQuaternion(writer, _rot);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		((NetworkBehaviour)this).DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<Vector3>(ref _pos, (Action<Vector3, Vector3>)null, NetworkReaderExtensions.ReadVector3(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<Quaternion>(ref _rot, (Action<Quaternion, Quaternion>)null, NetworkReaderExtensions.ReadQuaternion(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 1L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<Vector3>(ref _pos, (Action<Vector3, Vector3>)null, NetworkReaderExtensions.ReadVector3(reader));
		}
		if ((num & 2L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<Quaternion>(ref _rot, (Action<Quaternion, Quaternion>)null, NetworkReaderExtensions.ReadQuaternion(reader));
		}
	}
}
