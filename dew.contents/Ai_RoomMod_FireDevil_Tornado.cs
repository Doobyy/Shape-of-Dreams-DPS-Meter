using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Ai_RoomMod_FireDevil_Tornado : TickDamageInstance
{
	[NonSerialized]
	[SyncVar]
	public Vector3 endPos;

	private Vector3 _startPos;

	public override bool reuseInRoom => true;

	public Vector3 NetworkendPos
	{
		get
		{
			return endPos;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<Vector3>(value, ref endPos, 128uL, (Action<Vector3, Vector3>)null);
		}
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		_startPos = ((Component)(object)this).transform.position;
	}

	protected override void ActiveFrameUpdate()
	{
		base.ActiveFrameUpdate();
		Vector3 vector = Vector3.Lerp(_startPos, endPos, (Time.time - creationTime) / duration);
		vector = Dew.GetPositionOnGround(vector);
		vector.y = Mathf.MoveTowards(((Component)(object)this).transform.position.y, vector.y, 3f * Time.deltaTime);
		((Component)(object)this).transform.position = vector;
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteVector3(writer, endPos);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x80L) != 0L)
		{
			NetworkWriterExtensions.WriteVector3(writer, endPos);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<Vector3>(ref endPos, (Action<Vector3, Vector3>)null, NetworkReaderExtensions.ReadVector3(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x80L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<Vector3>(ref endPos, (Action<Vector3, Vector3>)null, NetworkReaderExtensions.ReadVector3(reader));
		}
	}
}
