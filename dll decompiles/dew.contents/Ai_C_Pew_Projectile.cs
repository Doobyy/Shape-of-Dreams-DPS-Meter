using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Ai_C_Pew_Projectile : StandardProjectile
{
	[NonSerialized]
	[SyncVar]
	internal float chargeAmount;

	public ScalingValue damageMin;

	public ScalingValue damageMax;

	public float critThreshold = 0.5f;

	public float procCoefficient = 1f;

	public bool doKnockback;

	public Knockback targetKnockback;

	public DewAudioSource[] adjustedSounds;

	public AnimationCurve soundPitch;

	public AnimationCurve soundVolume;

	public Transform[] scaleAdjustedTransforms;

	public AnimationCurve transformScale;

	public Light[] adjustedLights;

	public AnimationCurve lightIntensity;

	public FxCameraShake[] adjustedShakes;

	public AnimationCurve shakeAmplitude;

	private Vector3[] _baseTransformScales;

	private float[] _baseLightIntensities;

	private float[] _baseShakeAmplitudes;

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
		if (scaleAdjustedTransforms != null)
		{
			_baseTransformScales = new Vector3[scaleAdjustedTransforms.Length];
			for (int i = 0; i < scaleAdjustedTransforms.Length; i++)
			{
				if (scaleAdjustedTransforms[i] != null)
				{
					_baseTransformScales[i] = scaleAdjustedTransforms[i].localScale;
				}
			}
		}
		if (adjustedLights != null)
		{
			_baseLightIntensities = new float[adjustedLights.Length];
			for (int j = 0; j < adjustedLights.Length; j++)
			{
				if (adjustedLights[j] != null)
				{
					_baseLightIntensities[j] = adjustedLights[j].intensity;
				}
			}
		}
		if (adjustedShakes == null)
		{
			return;
		}
		_baseShakeAmplitudes = new float[adjustedShakes.Length];
		for (int k = 0; k < adjustedShakes.Length; k++)
		{
			if (adjustedShakes[k] != null)
			{
				_baseShakeAmplitudes[k] = adjustedShakes[k].amplitude;
			}
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		NetworkchargeAmount = 0f;
		if (scaleAdjustedTransforms != null && _baseTransformScales != null)
		{
			for (int i = 0; i < scaleAdjustedTransforms.Length; i++)
			{
				if (scaleAdjustedTransforms[i] != null)
				{
					scaleAdjustedTransforms[i].localScale = _baseTransformScales[i];
				}
			}
		}
		if (adjustedLights != null && _baseLightIntensities != null)
		{
			for (int j = 0; j < adjustedLights.Length; j++)
			{
				if (adjustedLights[j] != null)
				{
					adjustedLights[j].intensity = _baseLightIntensities[j];
				}
			}
		}
		if (adjustedShakes == null || _baseShakeAmplitudes == null)
		{
			return;
		}
		for (int k = 0; k < adjustedShakes.Length; k++)
		{
			if (adjustedShakes[k] != null)
			{
				adjustedShakes[k].amplitude = _baseShakeAmplitudes[k];
			}
		}
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
			}
		}
		float num = transformScale.Evaluate(chargeAmount);
		for (int j = 0; j < scaleAdjustedTransforms.Length; j++)
		{
			if (!(scaleAdjustedTransforms[j] == null))
			{
				scaleAdjustedTransforms[j].localScale = _baseTransformScales[j] * num;
			}
		}
		float num2 = lightIntensity.Evaluate(chargeAmount);
		for (int k = 0; k < adjustedLights.Length; k++)
		{
			if (!(adjustedLights[k] == null))
			{
				adjustedLights[k].intensity = _baseLightIntensities[k] * num2;
			}
		}
		float num3 = shakeAmplitude.Evaluate(chargeAmount);
		for (int l = 0; l < adjustedShakes.Length; l++)
		{
			if (!(adjustedShakes[l] == null))
			{
				adjustedShakes[l].amplitude = _baseShakeAmplitudes[l] * num3;
			}
		}
		base.OnCreate();
	}

	protected override void OnEntity(EntityHit hit)
	{
		base.OnEntity(hit);
		ScalingValue value = ScalingValue.Lerp(damageMin, damageMax, chargeAmount);
		DamageData damageData = Damage(value, procCoefficient).SetDirection(info.forward).SetElemental(ElementalType.Fire);
		if (chargeAmount > critThreshold)
		{
			damageData.SetAttr(DamageAttribute.IsCrit);
		}
		damageData.Dispatch(hit.entity);
		if (doKnockback)
		{
			targetKnockback.ApplyWithDirection(info.forward, hit.entity);
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
