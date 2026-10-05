using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Ai_R_PrecisionShot_Projectile : StandardProjectile
{
	[NonSerialized]
	[SyncVar]
	internal float chargeAmount;

	public ScalingValue damageMin;

	public ScalingValue damageMax;

	public float critThreshold = 0.5f;

	public float procCoefficient = 1f;

	public float dmgAmpPerHit;

	public float stunDurationMin;

	public float stunDurationMax;

	public bool doKnockback;

	public Knockback targetKnockback;

	public DewAudioSource[] adjustedSounds;

	public AnimationCurve soundPitch;

	public AnimationCurve soundVolume;

	public DewAudioSource reverb;

	public AnimationCurve reverbVolume;

	public Transform[] scaleAdjustedTransforms;

	public AnimationCurve transformScale;

	public Light[] adjustedLights;

	public AnimationCurve lightIntensity;

	public FxCameraShake[] adjustedShakes;

	public AnimationCurve shakeAmplitude;

	[NonSerialized]
	public bool isBiggerShot;

	private Vector3[] _baseScales;

	private float[] _baseLightIntensities;

	private float[] _baseShakeAmplitudes;

	private float _baseCollisionRadius;

	private ScalingValue _baseDamageMin;

	private ScalingValue _baseDamageMax;

	private float _currentDmgAmp;

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
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref chargeAmount, 524288uL, (Action<float, float>)null);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		_baseScales = new Vector3[scaleAdjustedTransforms.Length];
		for (int i = 0; i < scaleAdjustedTransforms.Length; i++)
		{
			if (scaleAdjustedTransforms[i] != null)
			{
				_baseScales[i] = scaleAdjustedTransforms[i].localScale;
			}
		}
		_baseLightIntensities = new float[adjustedLights.Length];
		for (int j = 0; j < adjustedLights.Length; j++)
		{
			if (adjustedLights[j] != null)
			{
				_baseLightIntensities[j] = adjustedLights[j].intensity;
			}
		}
		_baseShakeAmplitudes = new float[adjustedShakes.Length];
		for (int k = 0; k < adjustedShakes.Length; k++)
		{
			if (adjustedShakes[k] != null)
			{
				_baseShakeAmplitudes[k] = adjustedShakes[k].amplitude;
			}
		}
		_baseCollisionRadius = collisionRadius;
		_baseDamageMin = damageMin;
		_baseDamageMax = damageMax;
	}

	protected override void OnCreate()
	{
		float pitchMultiplier = soundPitch.Evaluate(chargeAmount);
		float volumeMultiplier = soundVolume.Evaluate(chargeAmount);
		DewAudioSource[] array = adjustedSounds;
		foreach (DewAudioSource dewAudioSource in array)
		{
			if (!(dewAudioSource == null))
			{
				dewAudioSource.pitchMultiplier = pitchMultiplier;
				dewAudioSource.volumeMultiplier = volumeMultiplier;
				if (isBiggerShot)
				{
					dewAudioSource.pitchMultiplier *= 0.85f;
				}
			}
		}
		if (reverb != null)
		{
			reverb.volumeMultiplier = reverbVolume.Evaluate(chargeAmount);
		}
		float num = transformScale.Evaluate(chargeAmount);
		for (int j = 0; j < scaleAdjustedTransforms.Length; j++)
		{
			Transform transform = scaleAdjustedTransforms[j];
			if (!(transform == null))
			{
				transform.localScale = _baseScales[j] * num;
				if (isBiggerShot)
				{
					transform.localScale *= 1.3f;
				}
			}
		}
		float num2 = lightIntensity.Evaluate(chargeAmount);
		for (int k = 0; k < adjustedLights.Length; k++)
		{
			Light light = adjustedLights[k];
			if (!(light == null))
			{
				light.intensity = _baseLightIntensities[k] * num2;
			}
		}
		float num3 = shakeAmplitude.Evaluate(chargeAmount);
		for (int l = 0; l < adjustedShakes.Length; l++)
		{
			FxCameraShake fxCameraShake = adjustedShakes[l];
			if (!(fxCameraShake == null))
			{
				fxCameraShake.amplitude = _baseShakeAmplitudes[l] * num3;
				if (isBiggerShot)
				{
					fxCameraShake.amplitude *= 1.2f;
				}
			}
		}
		base.OnCreate();
	}

	protected override void OnEntity(EntityHit hit)
	{
		base.OnEntity(hit);
		ScalingValue value = ScalingValue.Lerp(damageMin, damageMax, chargeAmount);
		float num = Mathf.Lerp(stunDurationMin, stunDurationMax, chargeAmount);
		DamageData damageData = Damage(value, procCoefficient).SetDirection(info.forward);
		if (chargeAmount > critThreshold || _currentDmgAmp > 0.01f)
		{
			damageData.SetAttr(DamageAttribute.IsCrit);
		}
		damageData.ApplyAmplification(_currentDmgAmp);
		damageData.SetAttr(DamageAttribute.IgnoreArmor);
		damageData.Dispatch(hit.entity);
		if (doKnockback)
		{
			targetKnockback.ApplyWithDirection(info.forward, hit.entity);
		}
		if (num > 0.001f)
		{
			CreateBasicEffect(hit.entity, new StunEffect(), num, "precisionshot_stun");
		}
		_currentDmgAmp += dmgAmpPerHit;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		_currentDmgAmp = 0f;
		isBiggerShot = false;
		collisionRadius = _baseCollisionRadius;
		damageMin = _baseDamageMin;
		damageMax = _baseDamageMax;
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
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x80000L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, chargeAmount);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref chargeAmount, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x80000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref chargeAmount, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
	}
}
