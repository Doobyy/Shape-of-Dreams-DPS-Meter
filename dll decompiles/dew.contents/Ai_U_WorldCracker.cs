using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;
using UnityEngine.Serialization;

public class Ai_U_WorldCracker : AbilityInstance
{
	public DewAnimationClip beamAnim;

	public float frontDistance;

	public float damageCheckInterval;

	public float damageInterval;

	public float yOffset;

	public ScalingValue damagePerTick;

	public float uncancellableTime;

	public float cameraOffset;

	public int cameraZoomIndex;

	public float cameraRevertDelay = 0.1f;

	public float cameraRevertTime = 0.5f;

	public float maxDistance;

	public float radius = 0.8f;

	public GameObject beamEffect;

	public GameObject beamHitEffect;

	public GameObject destinationEffect;

	public LineRenderer lineRenderer;

	public float beamInterval;

	public GameObject periodicEffect;

	public float periodicEffectInterval;

	private float _lastPeriodicEffectInterval;

	private CameraModifierOffset _offset;

	private CameraModifierZoom _zoom;

	[FormerlySerializedAs("beamRotationSpeed")]
	public float angleSpeed;

	public float procCoefficient;

	private float _lastDamageTime;

	private bool _atTarget;

	private float _lastBeamInterval;

	[SyncVar]
	private float _angle;

	private Dictionary<Entity, float> _hitTimes = new Dictionary<Entity, float>();

	public override bool reuseInRoom => true;

	public float Network_angle
	{
		get
		{
			return _angle;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref _angle, 64uL, (Action<float, float>)null);
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		_hitTimes.Clear();
		_atTarget = false;
		Network_angle = 0f;
		_lastDamageTime = 0f;
		_lastBeamInterval = 0f;
		_lastPeriodicEffectInterval = 0f;
	}

	protected override void OnPrepare()
	{
		base.OnPrepare();
		Network_angle = info.angle;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		SolvePosition();
		FxPlay(beamEffect);
		FxPlay(destinationEffect);
		if (lineRenderer != null)
		{
			lineRenderer.enabled = true;
		}
		if (((NetworkBehaviour)info.caster).isOwned)
		{
			_offset = new CameraModifierOffset
			{
				offset = Vector3.zero
			}.Apply();
			_zoom = new CameraModifierZoom
			{
				zoomIndex = cameraZoomIndex
			}.Apply();
		}
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		info.caster.EntityEvent_OnCastComplete += new Action<EventInfoCast>(PreventYubarMovementSkill);
		info.caster.Animation.PlayAbilityAnimation(beamAnim);
		DestroyOnDeath(info.caster);
		info.caster.Control.StartChannel(new Channel
		{
			blockedActions = Channel.BlockedAction.EverythingCancelable,
			duration = 3600f,
			onCancel = () =>
			{
				if (isActive)
				{
					Destroy();
				}
			},
			onComplete = () =>
			{
				if (isActive)
				{
					Destroy();
				}
			},
			uncancellableTime = uncancellableTime
		}.AddValidation(AbilitySelfValidator.Default));
	}

	private void PreventYubarMovementSkill(EventInfoCast obj)
	{
		if (obj.instance is Se_M_Flicker)
		{
			Destroy();
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
		CameraModifierOffset o;
		if (_offset != null)
		{
			o = _offset;
			_offset = null;
			ManagerBase<CameraManager>.instance.StartCoroutine(Routine());
		}
		CameraModifierZoom z;
		if (_zoom != null)
		{
			z = _zoom;
			_zoom = null;
			ManagerBase<CameraManager>.instance.StartCoroutine(Routine2());
		}
		if (((NetworkBehaviour)this).isServer && !((UnityEngine.Object)(object)info.caster == null))
		{
			info.caster.EntityEvent_OnCastComplete -= new Action<EventInfoCast>(PreventYubarMovementSkill);
			info.caster.Animation.StopAbilityAnimation(beamAnim);
		}
		IEnumerator Routine()
		{
			yield return new WaitForSeconds(cameraRevertDelay);
			Vector3 startOffset = o.offset;
			for (float t = 0f; t < cameraRevertTime; t += Time.deltaTime)
			{
				o.offset = startOffset * ((cameraRevertTime - t) / cameraRevertTime);
				yield return null;
			}
			o.Remove();
		}
		IEnumerator Routine2()
		{
			yield return new WaitForSeconds(cameraRevertDelay);
			z.Remove();
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
		if (_offset != null && _angle != 0f)
		{
			_offset.offset = Quaternion.Euler(0f, _angle, 0f) * Vector3.forward * cameraOffset;
		}
		if (Time.time - _lastPeriodicEffectInterval > periodicEffectInterval)
		{
			_lastPeriodicEffectInterval = Time.time;
			FxPlay(periodicEffect);
		}
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		if ((UnityEngine.Object)(object)info.caster == null || (UnityEngine.Object)(object)info.caster.owner == null)
		{
			Destroy();
			return;
		}
		float target = Vector3.SignedAngle(Vector3.forward, info.caster.owner.cursorWorldPos - info.caster.position, Vector3.up);
		Network_angle = Mathf.MoveTowardsAngle(_angle, target, angleSpeed * dt);
		info.caster.Control.Rotate(_angle, immediately: true, 0.25f);
		if (Time.time - _lastDamageTime < damageCheckInterval)
		{
			return;
		}
		_lastDamageTime = Time.time;
		float num = maxDistance;
		RaycastHit val = default;
		if (Physics.Raycast(((Component)(object)this).transform.position, ((Component)(object)this).transform.forward, ref val, maxDistance, LayerMasks.Ground | LayerMasks.IncludeInNavigation))
		{
			num = val.distance + 2f;
		}
		ListReturnHandle<Entity> handle;
		foreach (Entity item in DewPhysics.SphereCastAllEntities(out handle, ((Component)(object)this).transform.position, radius, ((Component)(object)this).transform.forward, num, tvDefaultHarmfulEffectTargets))
		{
			if (!(Vector2.Distance(info.caster.agentPosition.ToXY(), item.agentPosition.ToXY()) > num) && (!_hitTimes.ContainsKey(item) || !(Time.time - _hitTimes[item] < damageInterval)))
			{
				_hitTimes[item] = Time.time;
				FxPlayNewNetworked(beamHitEffect, item);
				Damage(damagePerTick, procCoefficient).SetElemental(ElementalType.Light).SetOriginPosition(info.caster.position).Dispatch(item);
			}
		}
		handle.Return();
	}

	private void SolvePosition()
	{
		((Component)(object)this).transform.position = info.caster.position + Quaternion.Euler(0f, _angle, 0f) * Vector3.forward * frontDistance + Vector3.up * yOffset;
		((Component)(object)this).transform.rotation = Quaternion.Euler(0f, _angle, 0f);
		Vector3 vector = ((Component)(object)this).transform.position + ((Component)(object)this).transform.forward * 100f;
		RaycastHit val = default;
		if (Physics.Raycast(((Component)(object)this).transform.position, ((Component)(object)this).transform.forward, ref val, 100f, LayerMasks.Ground | LayerMasks.IncludeInNavigation))
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
			NetworkWriterExtensions.WriteFloat(writer, _angle);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x40L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, _angle);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _angle, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x40L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _angle, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
	}
}
