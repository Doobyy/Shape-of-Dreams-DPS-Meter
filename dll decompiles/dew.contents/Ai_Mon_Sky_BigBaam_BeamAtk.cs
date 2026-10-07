using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Ai_Mon_Sky_BigBaam_BeamAtk : AbilityInstance
{
	public AbilitySelfValidator channelValidator;

	public ScalingValue damage;

	public float beamDuration;

	public float damageInterval;

	public AnimationCurve distanceCurve;

	public DewBeamRenderer beamRenderer;

	public Vector3 startOffset;

	public float beamRadius;

	public float hitBoxLength;

	public GameObject hitEffect;

	public float endDaze;

	private Entity[] _affected;

	private ArrayReturnHandle<Entity> _affectedHandle;

	private int _affectedCount;

	private float _lastDamageCheckTime;

	[SyncVar]
	private float _beamDuration;

	private float _initialBeamDuration;

	private float _initialEndDaze;

	private LevelScaling _initialDamageLeveling;

	public override bool reuseInRoom => true;

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

	protected override void Awake()
	{
		base.Awake();
		_initialBeamDuration = beamDuration;
		_initialEndDaze = endDaze;
		_initialDamageLeveling = damage.leveling;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		_affectedCount = 0;
		_lastDamageCheckTime = 0f;
		beamDuration = _initialBeamDuration;
		endDaze = _initialEndDaze;
		damage.leveling = _initialDamageLeveling;
	}

	protected override void OnPrepare()
	{
		base.OnPrepare();
		Network_beamDuration = beamDuration;
		if (info.caster is Hero)
		{
			Network_beamDuration = _beamDuration / (Mathf.Clamp(info.caster.Status.attackSpeedMultiplier, 1f, 3f) * 0.8f);
		}
		else
		{
			Network_beamDuration = _beamDuration * info.caster.Status.attackSpeedMultiplier;
		}
		if (info.caster is Hero)
		{
			damage.leveling = LevelScaling.SkillDefault;
		}
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		UpdatePosition();
		beamRenderer.enabled = true;
		if (((NetworkBehaviour)this).isServer)
		{
			_affected = DewPool.GetArray(out _affectedHandle, 128);
			info.caster.Control.StartChannel(new Channel
			{
				blockedActions = Channel.BlockedAction.Everything,
				duration = _beamDuration,
				onComplete = Destroy,
				onCancel = Destroy
			}.AddValidation(channelValidator));
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
		if (!((NetworkBehaviour)this).isServer || Time.time - _lastDamageCheckTime < damageInterval)
		{
			return;
		}
		_lastDamageCheckTime = Time.time;
		float time = Mathf.Clamp01((Time.time - creationTime) / _beamDuration);
		float num = distanceCurve.Evaluate(time);
		Vector3 vector = info.caster.position + info.caster.rotation * startOffset + info.forward * Mathf.Clamp(num - hitBoxLength, 0f, float.PositiveInfinity);
		Vector3 vector2 = info.caster.position + info.caster.rotation * startOffset + info.forward * num;
		Debug.DrawLine(vector, vector2, Color.red, 0.5f);
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
				Damage(damage).SetElemental(ElementalType.Light).SetOriginPosition(info.caster.position).Dispatch(item);
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
		float time = Mathf.Clamp01((Time.time - creationTime) / _beamDuration);
		Vector3 vector = ((!(info.caster is Hero)) ? (info.caster.position + info.caster.rotation * startOffset) : info.caster.Visual.GetBonePosition((HumanBodyBones)10));
		Vector3 vector2 = vector + info.forward * distanceCurve.Evaluate(time);
		vector2 = Dew.GetPositionOnGround(vector2);
		beamRenderer.SetPoints(vector, vector2);
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		beamRenderer.enabled = false;
		if (((NetworkBehaviour)this).isServer)
		{
			if (_affected != null)
			{
				_affected = null;
				_affectedHandle.Return();
			}
			if (info.caster.isActive)
			{
				info.caster.Control.StartDaze(endDaze);
			}
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
