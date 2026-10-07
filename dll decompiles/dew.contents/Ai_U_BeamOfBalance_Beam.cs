using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Ai_U_BeamOfBalance_Beam : AbilityInstance
{
	private struct HitRecord
	{
		public float nextHitTime;

		public float lastSeenTime;
	}

	public GameObject fxHit;

	public ScalingValue enemyDamagePerSecond;

	public ScalingValue allyHealPerSecond;

	public DewCollider range;

	public float checkInterval;

	public float hitInterval;

	public float maxSpeed;

	[NonSerialized]
	public Vector3 targetPosition;

	[NonSerialized]
	public float hitEndTime = float.PositiveInfinity;

	[SyncVar]
	private Vector3 _position;

	private Dictionary<Entity, HitRecord> _hitRecords = new Dictionary<Entity, HitRecord>();

	private float _lastCheckTime;

	private Vector3 _cv;

	public override bool reuseInRoom => true;

	public Vector3 Network_position
	{
		get
		{
			return _position;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<Vector3>(value, ref _position, 64uL, (Action<Vector3, Vector3>)null);
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		_hitRecords.Clear();
		_lastCheckTime = 0f;
		_cv = Vector3.zero;
		hitEndTime = float.PositiveInfinity;
	}

	protected override void OnPrepare()
	{
		base.OnPrepare();
		Network_position = position;
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer)
		{
			Network_position = Vector3.SmoothDamp(_position, targetPosition, ref _cv, 0.5f, maxSpeed, dt);
			Network_position = Dew.GetPositionOnGround(_position);
		}
		position = _position;
		if (!((NetworkBehaviour)this).isServer || Time.time - _lastCheckTime <= checkInterval)
		{
			return;
		}
		_lastCheckTime = Time.time;
		float time = Time.time;
		ListReturnHandle<Entity> handle;
		foreach (Entity entity in range.GetEntities(out handle))
		{
			if (_hitRecords.TryGetValue(entity, out var value) && time - value.lastSeenTime <= hitInterval)
			{
				value.lastSeenTime = time;
				if (time >= value.nextHitTime && time < hitEndTime)
				{
					value.nextHitTime += hitInterval;
					Hit(entity);
				}
				_hitRecords[entity] = value;
			}
			else if (time < hitEndTime)
			{
				_hitRecords[entity] = new HitRecord
				{
					nextHitTime = time + hitInterval,
					lastSeenTime = time
				};
				Hit(entity);
			}
		}
		handle.Return();
	}

	private void Hit(Entity e)
	{
		FxPlayNewNetworked(fxHit, e);
		if (info.caster.CheckEnemyOrNeutral(e))
		{
			Damage(enemyDamagePerSecond).SetDirection(Vector3.down).ApplyStrength(hitInterval).Dispatch(e);
		}
		else
		{
			Heal(allyHealPerSecond).ApplyRawMultiplier(hitInterval).Dispatch(e);
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
			NetworkWriterExtensions.WriteVector3(writer, _position);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x40L) != 0L)
		{
			NetworkWriterExtensions.WriteVector3(writer, _position);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<Vector3>(ref _position, (Action<Vector3, Vector3>)null, NetworkReaderExtensions.ReadVector3(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x40L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<Vector3>(ref _position, (Action<Vector3, Vector3>)null, NetworkReaderExtensions.ReadVector3(reader));
		}
	}
}
