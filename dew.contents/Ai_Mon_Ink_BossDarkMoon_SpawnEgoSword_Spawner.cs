using System;
using System.Collections;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Ai_Mon_Ink_BossDarkMoon_SpawnEgoSword_Spawner : AbilityInstance
{
	public float chargeDuration;

	public float deviation;

	public float projectileSpeed;

	public GameObject fxTelegraph;

	[SyncVar(hook = "OnSyncedRotChanged")]
	private Quaternion _syncedRotation;

	private bool _isStartRotate;

	private Quaternion _startRotation;

	private Quaternion _targetRotation;

	private float _elapsedTime;

	private float _rotateDuration;

	public Action<Quaternion, Quaternion> _Mirror_SyncVarHookDelegate__syncedRotation;

	public Quaternion Network_syncedRotation
	{
		get
		{
			return _syncedRotation;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<Quaternion>(value, ref _syncedRotation, 64uL, _Mirror_SyncVarHookDelegate__syncedRotation);
		}
	}

	private void OnSyncedRotChanged(Quaternion _, Quaternion __)
	{
		rotation = _syncedRotation;
	}

	protected override IEnumerator OnCreateSequenced()
	{
		_elapsedTime = 0f;
		Network_syncedRotation = rotation;
		rotation = _syncedRotation;
		_startRotation = _syncedRotation;
		_rotateDuration = chargeDuration - chargeDuration * 0.4f;
		if (((NetworkBehaviour)this).isServer)
		{
			DestroyOnDeath(info.caster);
			Entity entity = info.target;
			if (entity.IsNullInactiveDeadOrKnockedOut())
			{
				entity = Dew.GetClosestAliveHero(position, fallbackToDead: true, info.caster);
			}
			Vector3 targetPoint = AbilityTrigger.PredictPoint_Simple(info.caster, 1f, entity, chargeDuration);
			targetPoint += UnityEngine.Random.insideUnitCircle.ToXZ() * UnityEngine.Random.Range(0f, deviation);
			targetPoint = Dew.GetPositionOnGround(targetPoint);
			_targetRotation = Quaternion.LookRotation(targetPoint - position);
			_isStartRotate = true;
			Network_syncedRotation = _targetRotation;
			Vector3 positionOnGround = Dew.GetPositionOnGround(position);
			float num = Vector3.Distance(targetPoint, positionOnGround) / projectileSpeed;
			float multiplier = 1f / (chargeDuration + num);
			FxApplySpeedMultiplierNetworked(fxTelegraph, multiplier);
			Quaternion rot = Quaternion.LookRotation((targetPoint - positionOnGround).normalized);
			FxPlayNewNetworked(fxTelegraph, targetPoint, null);
			yield return new SI.WaitForSeconds(_rotateDuration);
			_isStartRotate = false;
			yield return new SI.WaitForSeconds(chargeDuration - _rotateDuration);
			FxStopNetworked(startEffect);
			CreateAbilityInstance(targetPoint, rotation, new CastInfo(info.caster, targetPoint), (Ai_Mon_Ink_BossDarkMoon_SpawnEgoSword_Projectile b) =>
			{
				b.SetCustomStartPosition(position);
				b.range.transform.rotation = rot;
				b.initialSpeed = projectileSpeed;
			});
			Destroy();
		}
	}

	protected override void ActiveFrameUpdate()
	{
		base.ActiveFrameUpdate();
		if (((NetworkBehaviour)this).isServer && _isStartRotate)
		{
			_elapsedTime += Time.deltaTime;
			float t = _elapsedTime / _rotateDuration;
			Network_syncedRotation = Quaternion.Slerp(_startRotation, _targetRotation, t);
		}
	}

	public Ai_Mon_Ink_BossDarkMoon_SpawnEgoSword_Spawner()
	{
		_Mirror_SyncVarHookDelegate__syncedRotation = OnSyncedRotChanged;
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteQuaternion(writer, _syncedRotation);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x40L) != 0L)
		{
			NetworkWriterExtensions.WriteQuaternion(writer, _syncedRotation);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<Quaternion>(ref _syncedRotation, _Mirror_SyncVarHookDelegate__syncedRotation, NetworkReaderExtensions.ReadQuaternion(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x40L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<Quaternion>(ref _syncedRotation, _Mirror_SyncVarHookDelegate__syncedRotation, NetworkReaderExtensions.ReadQuaternion(reader));
		}
	}
}
