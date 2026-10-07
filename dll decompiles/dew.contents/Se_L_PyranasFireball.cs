using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Se_L_PyranasFireball : StatusEffect
{
	public GameObject fxHit;

	public DewCollider range;

	public ScalingValue damage;

	public ScalingValue armorAmount;

	public float procCoefficient = 0.5f;

	public float hitInterval = 0.5f;

	public float checkInterval = 0.1f;

	public float recastDelay = 0.75f;

	public float baseDuration = 3f;

	public ScalingValue durationIncreasePerHit;

	public ScalingValue maxDurationIncrease;

	public float acceleration = 10f;

	public float maxSpeed = 10f;

	public DewAudioSource[] adjustedAudios;

	public AnimationCurve volumeCurve;

	public AnimationCurve pitchCurve;

	private float _lastCheckTime;

	private float _increasedDuration;

	private Dictionary<Entity, float> _hitTimes = new Dictionary<Entity, float>();

	[SyncVar]
	private Vector2 _currVel;

	private AbilityLockHandle _handle;

	private float[] _baseAudioPitchMul;

	private float[] _baseAudioVolumeMul;

	public override bool reuseInRoom => true;

	public Vector2 Network_currVel
	{
		get
		{
			return _currVel;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<Vector2>(value, ref _currVel, 4096uL, (Action<Vector2, Vector2>)null);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		_baseAudioPitchMul = new float[adjustedAudios.Length];
		_baseAudioVolumeMul = new float[adjustedAudios.Length];
		for (int i = 0; i < adjustedAudios.Length; i++)
		{
			if (!(adjustedAudios[i] == null))
			{
				_baseAudioPitchMul[i] = adjustedAudios[i].pitchMultiplier;
				_baseAudioVolumeMul[i] = adjustedAudios[i].volumeMultiplier;
			}
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		_lastCheckTime = 0f;
		_increasedDuration = 0f;
		Network_currVel = Vector2.zero;
		_hitTimes.Clear();
		_handle = null;
		for (int i = 0; i < adjustedAudios.Length; i++)
		{
			if (!(adjustedAudios[i] == null))
			{
				adjustedAudios[i].pitchMultiplier = _baseAudioPitchMul[i];
				adjustedAudios[i].volumeMultiplier = _baseAudioVolumeMul[i];
			}
		}
	}

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		DestroyOnDeath(victim, includeKnockOuts: true);
		_handle = victim.Ability.GetNewAbilityLockHandle();
		_handle.LockAllAbilitiesCast();
		DoUnstoppable();
		DoArmorBoost(GetValue(armorAmount));
		SetTimer(baseDuration);
		ShowOnScreenTimer();
		victim.Control.IncrementBlockCounters(Channel.BlockedAction.Everything);
		victim.takenDamageProcessor.Add(Processor);
		if (victim.Status.TryGetStatusEffect<Se_Elm_Fire>(out var effect))
		{
			effect.Destroy();
		}
		yield return new SI.WaitForSeconds(recastDelay);
		if ((UnityEngine.Object)(object)firstTrigger != null)
		{
			firstTrigger.ChangeConfigTimedOnce(1, 3600f, (EventInfoAbilityInstance _) =>
			{
				DestroyIfActive();
			}, null, setFillAmount: false);
		}
	}

	private void Processor(ref DamageData data, Actor actor, Entity target)
	{
		if (data.elemental == ElementalType.Fire)
		{
			data.BlockWithImmunity();
		}
	}

	protected override void ActiveFrameUpdate()
	{
		base.ActiveFrameUpdate();
		if (victim.Control.isLocalMovementProcessor)
		{
			float movementSpeedMultiplier = victim.Status.movementSpeedMultiplier;
			Vector2 target = (DewPlayer.local.cursorWorldPos - victim.position).ToXY().normalized * (maxSpeed * movementSpeedMultiplier);
			Network_currVel = Vector2.MoveTowards(_currVel, target, acceleration * movementSpeedMultiplier * Time.deltaTime);
			Vector3 agentPosition = info.caster.agentPosition;
			Vector3 end = agentPosition + _currVel.ToXZ() * Time.deltaTime;
			end = Dew.GetValidAgentDestination_LinearSweep(info.caster.agentPosition, end);
			info.caster.Control.SetAgentPosition(end);
			if (Time.deltaTime > 0.0001f)
			{
				Network_currVel = (end - agentPosition).ToXY() / Time.deltaTime;
			}
		}
		position = info.caster.agentPosition;
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		float magnitude = _currVel.magnitude;
		float target = pitchCurve.Evaluate(magnitude);
		float target2 = volumeCurve.Evaluate(magnitude);
		DewAudioSource[] array = adjustedAudios;
		foreach (DewAudioSource dewAudioSource in array)
		{
			if (!(dewAudioSource == null))
			{
				dewAudioSource.pitchMultiplier = Mathf.MoveTowards(dewAudioSource.pitchMultiplier, target, 2f * dt);
				dewAudioSource.volumeMultiplier = Mathf.MoveTowards(dewAudioSource.volumeMultiplier, target2, 2f * dt);
			}
		}
		if (!((NetworkBehaviour)this).isServer || Time.time - _lastCheckTime < checkInterval)
		{
			return;
		}
		_lastCheckTime = Time.time;
		ListReturnHandle<Entity> handle;
		foreach (Entity entity in range.GetEntities(out handle, tvDefaultHarmfulEffectTargets))
		{
			if (_hitTimes.TryGetValue(entity, out var value) && Time.time - value < hitInterval)
			{
				continue;
			}
			_hitTimes[entity] = Time.time;
			FxPlayNewNetworked(fxHit, entity);
			Damage(damage, procCoefficient).SetElemental(ElementalType.Fire).SetOriginPosition(victim.position).Dispatch(entity);
			float value2 = GetValue(maxDurationIncrease);
			if (_increasedDuration < value2)
			{
				float num = Mathf.Min(GetValue(durationIncreasePerHit), value2 - _increasedDuration);
				if (remainingDuration.Value + num < maxDuration.Value)
				{
					SetTimer(maxDuration.Value, remainingDuration.Value + num);
				}
				else
				{
					SetTimer(remainingDuration.Value + num);
				}
				_increasedDuration += num;
			}
		}
		handle.Return();
	}

	protected override void OnDestroyActor()
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			if ((UnityEngine.Object)(object)firstTrigger != null)
			{
				firstTrigger.UndoChangeConfig();
			}
			if (_handle != null)
			{
				_handle.Stop();
				_handle = null;
			}
			if ((UnityEngine.Object)(object)victim != null)
			{
				victim.Control.DecrementBlockCounters(Channel.BlockedAction.Everything);
				victim.Control.Stop();
				victim.Control.SetAgentDestination(victim.agentPosition + _currVel.ToXZ().normalized * 4f, important: true);
				victim.takenDamageProcessor.Remove(Processor);
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
			NetworkWriterExtensions.WriteVector2(writer, _currVel);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x1000L) != 0L)
		{
			NetworkWriterExtensions.WriteVector2(writer, _currVel);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<Vector2>(ref _currVel, (Action<Vector2, Vector2>)null, NetworkReaderExtensions.ReadVector2(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x1000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<Vector2>(ref _currVel, (Action<Vector2, Vector2>)null, NetworkReaderExtensions.ReadVector2(reader));
		}
	}
}
