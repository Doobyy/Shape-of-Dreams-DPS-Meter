using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Se_R_BoneCrusher_Kidnapped : StatusEffect
{
	[NonSerialized]
	[SyncVar]
	public Vector3 offset;

	private Vector3 _ogPos;

	public new Se_R_BoneCrusher parentActor => base.parentActor as Se_R_BoneCrusher;

	public override bool reuseInRoom => true;

	public Vector3 Networkoffset
	{
		get
		{
			return offset;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<Vector3>(value, ref offset, 4096uL, (Action<Vector3, Vector3>)null);
		}
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		_ogPos = victim.agentPosition;
		if (((NetworkBehaviour)this).isServer)
		{
			DoStun();
			DestroyOnDestroy(parentActor);
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer)
		{
			if (victim.Status.hasUnstoppable)
			{
				Destroy();
			}
			victim.Control.Rotate(180f + parentActor.directionAngle, immediately: true);
		}
	}

	protected override void ActiveFrameUpdate()
	{
		base.ActiveFrameUpdate();
		if (victim.Control.isLocalMovementProcessor)
		{
			Vector3 b = parentActor.victim.agentPosition + Quaternion.Euler(0f, parentActor.directionAngle, 0f) * offset;
			victim.Control.SetAgentPosition(Vector3.Lerp(_ogPos, b, (Time.time - creationTime) * 5f));
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
			NetworkWriterExtensions.WriteVector3(writer, offset);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x1000L) != 0L)
		{
			NetworkWriterExtensions.WriteVector3(writer, offset);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<Vector3>(ref offset, (Action<Vector3, Vector3>)null, NetworkReaderExtensions.ReadVector3(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x1000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<Vector3>(ref offset, (Action<Vector3, Vector3>)null, NetworkReaderExtensions.ReadVector3(reader));
		}
	}
}
