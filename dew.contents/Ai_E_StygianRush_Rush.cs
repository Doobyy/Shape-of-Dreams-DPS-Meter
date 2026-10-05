using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Ai_E_StygianRush_Rush : AbilityInstance
{
	[NonSerialized]
	[SyncVar]
	internal float chargeAmount;

	[NonSerialized]
	[SyncVar]
	internal float maxDistance;

	[NonSerialized]
	[SyncVar]
	internal float damage;

	[NonSerialized]
	[SyncVar]
	internal float shakeMultiplier;

	[NonSerialized]
	[SyncVar]
	internal float pitchMultiplier;

	[NonSerialized]
	[SyncVar]
	internal float volumeMultiplier;

	public DewCollider range;

	public float dashSpeed;

	public float stunDuration;

	public GameObject fxImpact;

	public GameObject fxHitImpact;

	public GameObject fxHitSub;

	public FxCameraShake[] shakes;

	public DewAudioSource[] adjustedAudios;

	public Transform[] adjustedTransforms;

	public ScalingValue shieldAmountPerHit;

	public float shieldDuration = 3f;

	public float shieldBossAmp = 2f;

	public Knockback knockback;

	private Entity _ent;

	private Vector3 _currentPos;

	private Vector3 _previousPos;

	private Vector3[] _baseTransformScales;

	private float[] _baseAudioPitch;

	private float[] _baseAudioVolume;

	private float[] _baseShakeAmplitude;

	private Vector3 _baseFxHitImpactScale;

	private float _baseKnockbackDistance;

	private float _baseKnockbackDuration;

	public override bool reuseInRoom => true;

	public float NetworkchargeAmount
	{
		get
		{
			return chargeAmount;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref chargeAmount, 64uL, (Action<float, float>)null);
		}
	}

	public float NetworkmaxDistance
	{
		get
		{
			return maxDistance;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref maxDistance, 128uL, (Action<float, float>)null);
		}
	}

	public float Networkdamage
	{
		get
		{
			return damage;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref damage, 256uL, (Action<float, float>)null);
		}
	}

	public float NetworkshakeMultiplier
	{
		get
		{
			return shakeMultiplier;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref shakeMultiplier, 512uL, (Action<float, float>)null);
		}
	}

	public float NetworkpitchMultiplier
	{
		get
		{
			return pitchMultiplier;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref pitchMultiplier, 1024uL, (Action<float, float>)null);
		}
	}

	public float NetworkvolumeMultiplier
	{
		get
		{
			return volumeMultiplier;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref volumeMultiplier, 2048uL, (Action<float, float>)null);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		if (adjustedTransforms != null)
		{
			_baseTransformScales = new Vector3[adjustedTransforms.Length];
			for (int i = 0; i < adjustedTransforms.Length; i++)
			{
				if (adjustedTransforms[i] != null)
				{
					_baseTransformScales[i] = adjustedTransforms[i].localScale;
				}
			}
		}
		if (adjustedAudios != null)
		{
			_baseAudioPitch = new float[adjustedAudios.Length];
			_baseAudioVolume = new float[adjustedAudios.Length];
			for (int j = 0; j < adjustedAudios.Length; j++)
			{
				if (adjustedAudios[j] != null)
				{
					_baseAudioPitch[j] = adjustedAudios[j].pitchMultiplier;
					_baseAudioVolume[j] = adjustedAudios[j].volumeMultiplier;
				}
			}
		}
		if (shakes != null)
		{
			_baseShakeAmplitude = new float[shakes.Length];
			for (int k = 0; k < shakes.Length; k++)
			{
				if (shakes[k] != null)
				{
					_baseShakeAmplitude[k] = shakes[k].amplitude;
				}
			}
		}
		if (fxHitImpact != null)
		{
			_baseFxHitImpactScale = fxHitImpact.transform.localScale;
		}
		if (knockback != null)
		{
			_baseKnockbackDistance = knockback.distance;
			_baseKnockbackDuration = knockback.duration;
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		_ent = null;
		NetworkchargeAmount = 0f;
		NetworkmaxDistance = 0f;
		Networkdamage = 0f;
		NetworkshakeMultiplier = 0f;
		NetworkpitchMultiplier = 0f;
		NetworkvolumeMultiplier = 0f;
		if (adjustedTransforms != null && _baseTransformScales != null)
		{
			for (int i = 0; i < adjustedTransforms.Length; i++)
			{
				if (adjustedTransforms[i] != null)
				{
					adjustedTransforms[i].localScale = _baseTransformScales[i];
				}
			}
		}
		if (adjustedAudios != null && _baseAudioPitch != null)
		{
			for (int j = 0; j < adjustedAudios.Length; j++)
			{
				if (adjustedAudios[j] != null)
				{
					adjustedAudios[j].pitchMultiplier = _baseAudioPitch[j];
					adjustedAudios[j].volumeMultiplier = _baseAudioVolume[j];
				}
			}
		}
		if (shakes != null && _baseShakeAmplitude != null)
		{
			for (int k = 0; k < shakes.Length; k++)
			{
				if (shakes[k] != null)
				{
					shakes[k].amplitude = _baseShakeAmplitude[k];
				}
			}
		}
		if (fxHitImpact != null)
		{
			fxHitImpact.transform.localScale = _baseFxHitImpactScale;
		}
		if (knockback != null)
		{
			knockback.distance = _baseKnockbackDistance;
			knockback.duration = _baseKnockbackDuration;
		}
	}

	protected override void OnCreate()
	{
		Transform[] array = adjustedTransforms;
		foreach (Transform transform in array)
		{
			if (!(transform == null))
			{
				transform.localScale *= Mathf.Lerp(0.5f, 1f, chargeAmount);
			}
		}
		((Component)(object)this).transform.position = info.caster.position;
		((Component)(object)this).transform.rotation = info.rotation;
		base.OnCreate();
	}

	protected override IEnumerator OnCreateSequenced()
	{
		DewAudioSource[] array = adjustedAudios;
		foreach (DewAudioSource dewAudioSource in array)
		{
			if (!(dewAudioSource == null))
			{
				dewAudioSource.pitchMultiplier *= Mathf.Max(0.5f, pitchMultiplier);
				dewAudioSource.volumeMultiplier *= Mathf.Max(0.5f, volumeMultiplier);
			}
		}
		if (((NetworkBehaviour)this).isServer)
		{
			_previousPos = info.caster.agentPosition;
			Vector3 end = info.caster.agentPosition + info.forward * (maxDistance + 0.5f);
			end = Dew.GetValidAgentDestination_LinearSweep(info.caster.agentPosition, end);
			float duration = Vector3.Distance(info.caster.agentPosition, end) / (dashSpeed * Mathf.Lerp(0.4f, 0.7f, chargeAmount));
			info.caster.Control.StartDisplacement(new DispByDestination
			{
				affectedByMovementSpeed = false,
				canGoOverTerrain = false,
				destination = end,
				duration = duration,
				ease = DewEase.Linear,
				isCanceledByCC = false,
				isFriendly = true,
				rotateForward = true,
				onCancel = OnRushComplete,
				onFinish = OnRushComplete
			});
			DoCollisionCheck();
		}
		yield break;
	}

	private void OnRushComplete()
	{
		FxCameraShake[] array = shakes;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].amplitude *= Mathf.Max(0.3f, shakeMultiplier);
		}
		((Component)(object)this).transform.position = info.caster.position;
		((Component)(object)this).transform.rotation = info.rotation;
		List<Entity> entities = range.GetEntities(out var handle, tvDefaultHarmfulEffectTargets, new CollisionCheckSettings
		{
			sortComparer = CollisionCheckSettings.DistanceFromCenter
		});
		Entity entity = null;
		if (_ent.IsNullInactiveDeadOrKnockedOut())
		{
			entity = _ent;
		}
		else
		{
			foreach (Entity item in entities)
			{
				if (!item.IsNullInactiveDeadOrKnockedOut())
				{
					entity = item;
					break;
				}
			}
		}
		if ((UnityEngine.Object)(object)entity == null)
		{
			Destroy();
			return;
		}
		fxHitImpact.transform.localScale *= Mathf.Lerp(0.4f, 0.7f, chargeAmount);
		FxPlayNetworked(fxImpact, ((Component)(object)this).transform.position, ((Component)(object)this).transform.rotation * fxImpact.transform.localRotation);
		FxPlayNetworked(fxHitImpact, entity);
		knockback.distance *= Mathf.Lerp(0.5f, 2f, chargeAmount);
		knockback.duration *= Mathf.Lerp(1f, 1.5f, chargeAmount);
		Hit(entity);
		float num = 0f;
		for (int j = 0; j < entities.Count; j++)
		{
			num = ((!entities[j].IsAnyBoss()) ? (num + GetValue(shieldAmountPerHit)) : (num + GetValue(shieldAmountPerHit) * (1f + shieldBossAmp)));
			if (!((UnityEngine.Object)(object)entity == (UnityEngine.Object)(object)entities[j]))
			{
				Hit(entities[j]);
			}
		}
		handle.Return();
		GiveShield(info.caster, num, shieldDuration);
		Destroy();
		void Hit(Entity e)
		{
			if (!e.IsNullInactiveDeadOrKnockedOut())
			{
				CreateBasicEffect(e, new StunEffect(), stunDuration);
				knockback.ApplyWithDirection(info.forward, e);
				FxPlayNewNetworked(fxHitSub, e);
				PhysicalDamage(damage).SetElemental(ElementalType.Dark).SetDirection(info.forward).SetOriginPosition(info.caster.agentPosition)
					.DoAttackEffect(AttackEffectType.Others)
					.Dispatch(e);
			}
		}
	}

	protected override void ActiveFrameUpdate()
	{
		base.ActiveFrameUpdate();
		((Component)(object)this).transform.position = info.caster.position;
		((Component)(object)this).transform.rotation = info.rotation;
		if (((NetworkBehaviour)this).isServer)
		{
			DoCollisionCheck();
		}
	}

	private void DoCollisionCheck()
	{
		if ((UnityEngine.Object)(object)_ent != null)
		{
			return;
		}
		_currentPos = info.caster.agentPosition;
		float num = Vector3.Distance(_previousPos, _currentPos);
		ListReturnHandle<Entity> handle;
		foreach (Entity item in DewPhysics.SphereCastAllEntities(out handle, _previousPos + info.forward * 0.5f, 0.75f, info.forward, num + 0.5f, tvDefaultHarmfulEffectTargets, new CollisionCheckSettings
		{
			includeUncollidable = false,
			sortComparer = CollisionCheckSettings.DistanceFromCenter
		}))
		{
			if (!item.IsNullInactiveDeadOrKnockedOut())
			{
				_ent = item;
				break;
			}
		}
		_previousPos = info.caster.position;
		handle.Return();
		if (!((UnityEngine.Object)(object)_ent == null))
		{
			info.caster.Control.CancelOngoingDisplacement();
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
			NetworkWriterExtensions.WriteFloat(writer, chargeAmount);
			NetworkWriterExtensions.WriteFloat(writer, maxDistance);
			NetworkWriterExtensions.WriteFloat(writer, damage);
			NetworkWriterExtensions.WriteFloat(writer, shakeMultiplier);
			NetworkWriterExtensions.WriteFloat(writer, pitchMultiplier);
			NetworkWriterExtensions.WriteFloat(writer, volumeMultiplier);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x40L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, chargeAmount);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x80L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, maxDistance);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x100L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, damage);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x200L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, shakeMultiplier);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x400L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, pitchMultiplier);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x800L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, volumeMultiplier);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref chargeAmount, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref maxDistance, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref damage, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref shakeMultiplier, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref pitchMultiplier, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref volumeMultiplier, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x40L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref chargeAmount, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
		if ((num & 0x80L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref maxDistance, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
		if ((num & 0x100L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref damage, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
		if ((num & 0x200L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref shakeMultiplier, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
		if ((num & 0x400L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref pitchMultiplier, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
		if ((num & 0x800L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref volumeMultiplier, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
	}
}
