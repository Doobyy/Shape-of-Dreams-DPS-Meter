using System;
using System.Collections;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Ai_Mon_Special_BossErebos_SpawnTormentor_Attacker_Instance : AbilityInstance
{
	public float castDelay;

	public float beamDuration;

	public float damageInterval;

	public AnimationCurve distanceCurve;

	public DewBeamRenderer beamRenderer;

	public float beamRadius;

	public float hitBoxLength;

	public GameObject hitEffect;

	public ScalingValue dmgFactor;

	public GameObject fxAtk;

	public GameObject fxTelegraph;

	private Entity[] _affected;

	private ArrayReturnHandle<Entity> _affectedHandle;

	private int _affectedCount;

	private float _lastDamageCheckTime;

	[SyncVar]
	private float _beamDuration;

	private bool _beamEnable;

	private float _beamStartTime;

	public float Network_beamDuration
	{
		get
		{
			return _beamDuration;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref _beamDuration, 64uL, (Action<float, float>)null);
		}
	}

	protected override void OnPrepare()
	{
		base.OnPrepare();
		Network_beamDuration = beamDuration;
	}

	protected override IEnumerator OnCreateSequenced()
	{
		_beamEnable = false;
		_affectedCount = 0;
		beamRenderer.enabled = false;
		FxPlay(fxTelegraph, Dew.GetPositionOnGround(((Component)(object)this).transform.position + info.forward * 5f), Quaternion.AngleAxis(info.angle, Vector3.up));
		yield return new SI.WaitForSeconds(castDelay);
		FxPlay(fxAtk, ((Component)(object)this).transform.position, null);
		UpdatePosition();
		beamRenderer.enabled = true;
		if (((NetworkBehaviour)this).isServer)
		{
			DestroyOnDeath(info.caster);
			_beamStartTime = creationTime + castDelay;
			_beamEnable = true;
			_affected = DewPool.GetArray(out _affectedHandle, 128);
			yield return new SI.WaitForSeconds(_beamDuration);
			Destroy();
		}
	}

	protected override void ActiveFrameUpdate()
	{
		base.ActiveFrameUpdate();
		UpdatePosition();
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!((NetworkBehaviour)this).isServer || !_beamEnable || Time.time - _lastDamageCheckTime < damageInterval)
		{
			return;
		}
		_lastDamageCheckTime = Time.time;
		float time = Mathf.Clamp01((Time.time - _beamStartTime) / _beamDuration);
		float num = distanceCurve.Evaluate(time);
		Vector3 vector = ((Component)(object)this).transform.position + info.forward * Mathf.Clamp(num - hitBoxLength, 0f, float.PositiveInfinity);
		Vector3 vector2 = vector + info.forward * num;
		ListReturnHandle<Entity> handle;
		foreach (Entity item in DewPhysics.SphereCastAllEntities(out handle, vector, beamRadius, vector2 - vector, (vector2 - vector).magnitude, tvDefaultHarmfulEffectTargets))
		{
			bool flag = false;
			for (int i = 0; i < _affectedCount; i++)
			{
				if ((UnityEngine.Object)(object)_affected[i] == (UnityEngine.Object)(object)item)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				FxPlayNewNetworked(hitEffect, item);
				Damage(dmgFactor).SetElemental(ElementalType.Light).SetOriginPosition(((Component)(object)this).transform.position).Dispatch(item);
				if (_affectedCount < _affected.Length)
				{
					_affected[_affectedCount] = item;
					_affectedCount++;
				}
			}
		}
		handle.Return();
	}

	private void UpdatePosition()
	{
		float time = Mathf.Clamp01((Time.time - _beamStartTime) / _beamDuration);
		Vector3 vector = ((Component)(object)this).transform.position;
		Vector3 vector2 = vector + info.forward * distanceCurve.Evaluate(time);
		vector2 = Dew.GetPositionOnGround(vector2);
		beamRenderer.SetPoints(vector, vector2);
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		beamRenderer.enabled = false;
		if (((NetworkBehaviour)this).isServer && _affected != null)
		{
			_affected = null;
			_affectedHandle.Return();
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
			NetworkWriterExtensions.WriteFloat(writer, _beamDuration);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x40L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, _beamDuration);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _beamDuration, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x40L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _beamDuration, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
	}
}
