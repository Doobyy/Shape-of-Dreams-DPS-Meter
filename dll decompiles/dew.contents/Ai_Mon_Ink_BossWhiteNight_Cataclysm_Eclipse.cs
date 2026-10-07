using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Ai_Mon_Ink_BossWhiteNight_Cataclysm_Eclipse : AbilityInstance
{
	public GameObject fxHit;

	public GameObject fxTelegraph;

	public GameObject fxStart;

	public ScalingValue dmgPerSecond;

	public DewCollider range;

	public float startDelay;

	public float duration;

	public float checkInterval;

	public float hitInterval;

	public float maxSpeed;

	internal float _startTime;

	[SyncVar]
	private Vector3 _position;

	private Dictionary<Entity, float> _hitTimes = new Dictionary<Entity, float>();

	private float _lastCheckTime;

	private Vector3 _cv;

	private bool _isReady;

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

	protected override void OnPrepare()
	{
		base.OnPrepare();
		Network_position = position;
	}

	protected override IEnumerator OnCreateSequenced()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			DestroyOnDeath(info.caster);
			FxPlayNetworked(fxTelegraph);
			yield return new SI.WaitForSeconds(startDelay);
			FxPlayNetworked(fxStart);
			yield return new SI.WaitForCondition(() => _startTime - Time.time < 0.001f);
			_isReady = true;
			yield return new SI.WaitForSeconds(duration);
			FxStopNetworked(fxStart);
			Destroy();
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer && _isReady && !info.target.IsNullInactiveDeadOrKnockedOut())
		{
			Network_position = Vector3.SmoothDamp(_position, info.target.GetAIPosition(info.caster), ref _cv, 0.5f, maxSpeed, dt);
			Network_position = Dew.GetPositionOnGround(_position);
		}
		position = _position;
		if (!((NetworkBehaviour)this).isServer || !_isReady || Time.time - _lastCheckTime <= checkInterval)
		{
			return;
		}
		_lastCheckTime = Time.time;
		ListReturnHandle<Entity> handle;
		foreach (Entity entity in range.GetEntities(out handle, tvDefaultHarmfulEffectTargets))
		{
			if (!_hitTimes.TryGetValue(entity, out var value) || !(Time.time - value <= hitInterval))
			{
				_hitTimes[entity] = Time.time;
				FxPlayNewNetworked(fxHit, entity);
				CreateDamage(DamageData.SourceType.Default, dmgPerSecond).SetDirection(Vector3.down).SetElemental(ElementalType.Light).Dispatch(entity);
			}
		}
		handle.Return();
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			FxStopNetworked(fxStart);
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		_isReady = false;
		_hitTimes.Clear();
		_lastCheckTime = 0f;
		_cv = default;
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
