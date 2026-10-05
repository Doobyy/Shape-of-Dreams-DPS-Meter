using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Ai_Mon_Special_BossLightElemental_Beam : AbilityInstance
{
	[NonSerialized]
	[SyncVar]
	public Vector2 angle;

	[NonSerialized]
	[SyncVar]
	public float duration;

	[NonSerialized]
	[SyncVar]
	public float damageMultiplier = 1f;

	public DewCollider range;

	public DewEase ease;

	public float frontDistance;

	public float damageCheckInterval;

	public float damageInterval;

	public float yOffset;

	public ScalingValue damage;

	public GameObject beamEffect;

	public GameObject beamHitEffect;

	public GameObject destinationEffect;

	public LineRenderer lineRenderer;

	public float beamInterval;

	private EaseFunction _easeFunc;

	private float _lastDamageTime;

	private bool _atTarget;

	private float _lastBeamInterval;

	private Dictionary<Entity, float> _hitTimes = new Dictionary<Entity, float>();

	private float _currentValue => _easeFunc(0f, 1f, Mathf.Clamp01((Time.time - creationTime) / duration));

	public Vector2 Networkangle
	{
		get
		{
			return angle;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<Vector2>(value, ref angle, 64uL, (Action<Vector2, Vector2>)null);
		}
	}

	public float Networkduration
	{
		get
		{
			return duration;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref duration, 128uL, (Action<float, float>)null);
		}
	}

	public float NetworkdamageMultiplier
	{
		get
		{
			return damageMultiplier;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref damageMultiplier, 256uL, (Action<float, float>)null);
		}
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		_easeFunc = EasingFunction.GetEasingFunction(ease);
		SolvePosition();
		FxPlay(beamEffect);
		FxPlay(destinationEffect);
		if (lineRenderer != null)
		{
			lineRenderer.enabled = true;
		}
		if (((NetworkBehaviour)this).isServer)
		{
			DestroyOnDeath(info.caster);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		FxStop(beamEffect);
		FxStop(destinationEffect);
		if (lineRenderer != null)
		{
			lineRenderer.enabled = false;
		}
	}

	protected override void ActiveFrameUpdate()
	{
		base.ActiveFrameUpdate();
		SolvePosition();
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		if (Time.time - creationTime > duration)
		{
			Destroy();
		}
		else
		{
			if (Time.time - _lastDamageTime < damageCheckInterval)
			{
				return;
			}
			_lastDamageTime = Time.time;
			ListReturnHandle<Entity> handle;
			foreach (Entity entity in range.GetEntities(out handle, tvDefaultHarmfulEffectTargets))
			{
				if (!_hitTimes.ContainsKey(entity) || !(Time.time - _hitTimes[entity] < damageInterval))
				{
					_hitTimes[entity] = Time.time;
					FxPlayNewNetworked(beamHitEffect, entity);
					Damage(damage).ApplyRawMultiplier(damageMultiplier).SetElemental(ElementalType.Light).SetOriginPosition(info.caster.position)
						.Dispatch(entity);
				}
			}
			handle.Return();
		}
	}

	private void SolvePosition()
	{
		float y = Mathf.Lerp(angle.x, angle.y, _currentValue);
		((Component)(object)this).transform.position = info.caster.position + Quaternion.Euler(0f, y, 0f) * Vector3.forward * frontDistance + Vector3.up * yOffset;
		((Component)(object)this).transform.rotation = Quaternion.Euler(0f, y, 0f);
		Vector3 vector = ((Component)(object)this).transform.position + ((Component)(object)this).transform.forward * 100f;
		RaycastHit val = default;
		if (Physics.Raycast(((Component)(object)this).transform.position, ((Component)(object)this).transform.forward, ref val, 100f, LayerMasks.Ground))
		{
			vector = val.point;
		}
		if (lineRenderer != null)
		{
			lineRenderer.SetPosition(0, ((Component)(object)this).transform.position);
			lineRenderer.SetPosition(1, vector);
		}
		destinationEffect.transform.position = vector;
		if (Time.time - _lastBeamInterval > beamInterval)
		{
			_atTarget = !_atTarget;
			beamEffect.transform.position = (_atTarget ? vector : ((Component)(object)this).transform.position);
			_lastBeamInterval = Time.time;
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
			NetworkWriterExtensions.WriteVector2(writer, angle);
			NetworkWriterExtensions.WriteFloat(writer, duration);
			NetworkWriterExtensions.WriteFloat(writer, damageMultiplier);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x40L) != 0L)
		{
			NetworkWriterExtensions.WriteVector2(writer, angle);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x80L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, duration);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x100L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, damageMultiplier);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<Vector2>(ref angle, (Action<Vector2, Vector2>)null, NetworkReaderExtensions.ReadVector2(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref duration, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref damageMultiplier, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x40L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<Vector2>(ref angle, (Action<Vector2, Vector2>)null, NetworkReaderExtensions.ReadVector2(reader));
		}
		if ((num & 0x80L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref duration, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
		if ((num & 0x100L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref damageMultiplier, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
	}
}
