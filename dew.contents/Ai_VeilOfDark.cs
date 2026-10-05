using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Ai_VeilOfDark : AbilityInstance
{
	[NonSerialized]
	[SyncVar]
	public float radius;

	public float collisionCheckInterval;

	public float duration;

	public GameObject fxInstance;

	public GameObject range;

	private float _currentTime;

	private float _elapsedTime;

	public override bool reuseInRoom => true;

	public float Networkradius
	{
		get
		{
			return radius;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref radius, 64uL, (Action<float, float>)null);
		}
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		_currentTime = 0f;
		_elapsedTime = Time.time + duration;
		range.transform.localScale = Vector3.one * radius;
		FxPlay(fxInstance, ((Component)(object)this).transform.position, null);
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		FxStop(fxInstance);
	}

	protected override void ActiveFrameUpdate()
	{
		base.ActiveFrameUpdate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		if (_elapsedTime <= Time.time)
		{
			Destroy();
		}
		if (Time.time - _currentTime < collisionCheckInterval)
		{
			return;
		}
		_currentTime = Time.time;
		ListReturnHandle<Entity> handle;
		foreach (Entity item in DewPhysics.OverlapCircleAllEntities(out handle, ((Component)(object)this).transform.position, radius))
		{
			if (item is Hero && item.owner.isHumanPlayer && !item.IsNullInactiveDeadOrKnockedOut())
			{
				if (item.Status.TryGetStatusEffect<Se_VeilOfDark>(out var effect))
				{
					effect.ResetTimer();
				}
				else
				{
					CreateStatusEffect<Se_VeilOfDark>(item);
				}
			}
		}
		handle.Return();
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteFloat(writer, radius);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x40L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, radius);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref radius, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x40L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref radius, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
	}
}
