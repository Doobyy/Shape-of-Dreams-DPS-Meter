using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Ai_R_SanctuaryOfEl_Ground : AbilityInstance, IOtherPlayersTonedDownLimit
{
	[SyncVar]
	public float groundDuration = 10f;

	public float radius;

	public float checkInterval;

	public GameObject changePositionEffect;

	public float positionSmoothTime;

	public float damageDelay;

	public ScalingValue startDamage;

	public ScalingValue startShield;

	public GameObject damageHitEffect;

	public float aboutToExpireEffectDelay;

	public GameObject aboutToExpireEffect;

	public float shieldDuration = 4f;

	[NonSerialized]
	public float cooldownReductionRatioByExplosionAlly;

	[NonSerialized]
	public float cooldownReductionRatioByExplosionEnemy;

	private OnScreenTimerHandle _handle;

	private AbilityTrigger.ChangedConfigHandle _configHandle;

	private float _lastCheckTime = float.NegativeInfinity;

	[SyncVar]
	private Vector3 _desiredPosition;

	private Vector3 _cv;

	private AbilityTrigger _trigger;

	private float _baseGroundDuration;

	private ScalingValue _baseStartShield;

	private ParticleSystem[] _startEffectParticleSystems;

	private float[] _startEffectBaseSimulationSpeeds;

	public float fillAmount => 1f - (Time.time - creationTime) / groundDuration;

	public override bool reuseInRoom => true;

	public ReduceOtherPlayerEffectsStrength maxReduction => ReduceOtherPlayerEffectsStrength.VeryHigh;

	public float NetworkgroundDuration
	{
		get
		{
			return groundDuration;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref groundDuration, 64uL, (Action<float, float>)null);
		}
	}

	public Vector3 Network_desiredPosition
	{
		get
		{
			return _desiredPosition;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<Vector3>(value, ref _desiredPosition, 128uL, (Action<Vector3, Vector3>)null);
		}
	}

	protected override void Awake()
	{
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		base.Awake();
		_baseGroundDuration = groundDuration;
		_baseStartShield = startShield;
		_startEffectParticleSystems = ((startEffect != null) ? startEffect.GetComponentsInChildren<ParticleSystem>(includeInactive: true) : Array.Empty<ParticleSystem>());
		_startEffectBaseSimulationSpeeds = new float[_startEffectParticleSystems.Length];
		for (int i = 0; i < _startEffectParticleSystems.Length; i++)
		{
			float[] startEffectBaseSimulationSpeeds = _startEffectBaseSimulationSpeeds;
			int num = i;
			MainModule main = _startEffectParticleSystems[i].main;
			startEffectBaseSimulationSpeeds[num] = main.simulationSpeed;
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		NetworkgroundDuration = _baseGroundDuration;
		startShield = _baseStartShield;
		cooldownReductionRatioByExplosionAlly = 0f;
		cooldownReductionRatioByExplosionEnemy = 0f;
		_lastCheckTime = float.NegativeInfinity;
		_cv = default;
		Network_desiredPosition = position;
	}

	protected override IEnumerator OnCreateSequenced()
	{
		((Component)(object)this).transform.SetPositionAndRotation((info.point != default(Vector3)) ? info.point : info.caster.agentPosition, ManagerBase<CameraManager>.instance.entityCamAngleRotation);
		for (int i = 0; i < _startEffectParticleSystems.Length; i++)
		{
			MainModule main = _startEffectParticleSystems[i].main;
			main.simulationSpeed = _startEffectBaseSimulationSpeeds[i];
		}
		FxApplySpeedMultiplier(startEffect, 10f / groundDuration);
		if (((NetworkBehaviour)info.caster).isOwned)
		{
			_handle = ShowOnScreenTimerLocally(new OnScreenTimerHandle
			{
				fillAmountGetter = () => fillAmount
			});
		}
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		_trigger = firstTrigger;
		Network_desiredPosition = position;
		GiveShield(info.caster, GetValue(startShield), shieldDuration);
		List<Entity> list = DewPhysics.OverlapCircleAllEntities(out var handle, ((Component)(object)this).transform.position, radius, tvDefaultUsefulEffectTargets);
		for (int num = 0; num < list.Count; num++)
		{
			Entity entity = list[num];
			GiveShield(entity, GetValue(startShield), shieldDuration);
			FxPlayNewNetworked(damageHitEffect, entity);
		}
		if (cooldownReductionRatioByExplosionAlly > 0f && (UnityEngine.Object)(object)firstTrigger != null)
		{
			ApplyCooldownReductionByRatio(firstTrigger, cooldownReductionRatioByExplosionAlly * (float)list.Count);
		}
		handle.Return();
		yield return new SI.WaitForSeconds(damageDelay);
		List<Entity> list2 = DewPhysics.OverlapCircleAllEntities(out var handle2, ((Component)(object)this).transform.position, radius, tvDefaultHarmfulEffectTargets);
		for (int num2 = 0; num2 < list2.Count; num2++)
		{
			Entity entity2 = list2[num2];
			Damage(startDamage).SetElemental(ElementalType.Light).SetOriginPosition(((Component)(object)this).transform.position).Dispatch(entity2);
			FxPlayNewNetworked(damageHitEffect, entity2);
		}
		if (cooldownReductionRatioByExplosionEnemy > 0f && (UnityEngine.Object)(object)firstTrigger != null)
		{
			ApplyCooldownReductionByRatio(firstTrigger, cooldownReductionRatioByExplosionEnemy * (float)list2.Count);
		}
		handle2.Return();
		yield return new SI.WaitForSeconds(0.7f);
		_configHandle = firstTrigger.ChangeConfigTimedOnce(1, groundDuration, (EventInfoAbilityInstance obj) =>
		{
			if (isActive)
			{
				Network_desiredPosition = ((obj.instance.info.point != default(Vector3)) ? Dew.GetPositionOnGround(obj.instance.info.point) : info.caster.agentPosition);
				FxPlayNetworked(changePositionEffect);
			}
		});
		yield return new SI.WaitForSeconds(aboutToExpireEffectDelay - damageDelay - 0.7f);
		FxPlayNetworked(aboutToExpireEffect);
	}

	public override void FrameUpdate()
	{
		base.FrameUpdate();
		((Component)(object)this).transform.position = Vector3.SmoothDamp(((Component)(object)this).transform.position, _desiredPosition, ref _cv, positionSmoothTime);
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		if (_trigger.IsNullOrInactive() || (UnityEngine.Object)(object)_trigger.owner == null)
		{
			Destroy();
		}
		else if (Time.time - creationTime > groundDuration)
		{
			Destroy();
		}
		else
		{
			if (!(Time.time - _lastCheckTime > checkInterval))
			{
				return;
			}
			_lastCheckTime = Time.time;
			ListReturnHandle<Entity> handle;
			foreach (Entity item in DewPhysics.OverlapCircleAllEntities(out handle, ((Component)(object)this).transform.position, radius, tvDefaultUsefulEffectTargets))
			{
				Se_R_SanctuaryOfEl_Buff se_R_SanctuaryOfEl_Buff = item.Status.FindStatusEffect((Se_R_SanctuaryOfEl_Buff se) => (UnityEngine.Object)(object)se.parentActor == (UnityEngine.Object)(object)this);
				if ((UnityEngine.Object)(object)se_R_SanctuaryOfEl_Buff != null)
				{
					se_R_SanctuaryOfEl_Buff.ResetTimer();
				}
				else
				{
					CreateStatusEffect<Se_R_SanctuaryOfEl_Buff>(item);
				}
			}
			handle.Return();
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			_configHandle?.Stop();
			_configHandle = null;
		}
		if (_handle != null)
		{
			HideOnScreenTimerLocally(_handle);
			_handle = null;
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
			NetworkWriterExtensions.WriteFloat(writer, groundDuration);
			NetworkWriterExtensions.WriteVector3(writer, _desiredPosition);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x40L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, groundDuration);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x80L) != 0L)
		{
			NetworkWriterExtensions.WriteVector3(writer, _desiredPosition);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref groundDuration, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<Vector3>(ref _desiredPosition, (Action<Vector3, Vector3>)null, NetworkReaderExtensions.ReadVector3(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x40L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref groundDuration, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
		if ((num & 0x80L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<Vector3>(ref _desiredPosition, (Action<Vector3, Vector3>)null, NetworkReaderExtensions.ReadVector3(reader));
		}
	}
}
