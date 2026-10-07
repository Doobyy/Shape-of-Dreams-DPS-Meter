using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Ai_Q_BigBorealChunk_Explosion : InstantDamageInstance
{
	public DewAudioSource[] adjustedAudios;

	public AnimationCurve pitchCurve;

	public AnimationCurve volumeCurve;

	public FxCameraShake[] adjustedCameraShakes;

	public FxEntityShake[] adjustedEntityShakes;

	public AnimationCurve shakeCurve;

	public float stunDuration = 1f;

	[NonSerialized]
	public bool isCritDamage;

	[NonSerialized]
	[SyncVar]
	public float chargeDuration;

	[NonSerialized]
	[SyncVar]
	public float explodeRadius;

	[NonSerialized]
	private ScalingValue _baseDmgFactor;

	[NonSerialized]
	private Vector3 _baseRangeScale;

	[NonSerialized]
	private Vector3 _baseStartFxScale;

	[NonSerialized]
	private float[] _basePitch;

	[NonSerialized]
	private float[] _baseVolume;

	[NonSerialized]
	private float[] _baseShakeAmplitude;

	[NonSerialized]
	private float[] _baseShakeIntensity;

	public override bool reuseInRoom => true;

	public float NetworkchargeDuration
	{
		get
		{
			return chargeDuration;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref chargeDuration, 128uL, (Action<float, float>)null);
		}
	}

	public float NetworkexplodeRadius
	{
		get
		{
			return explodeRadius;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref explodeRadius, 256uL, (Action<float, float>)null);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		_baseDmgFactor = dmgFactor;
		_baseRangeScale = range.transform.localScale;
		_baseStartFxScale = startEffectNoStop.transform.localScale;
		_basePitch = new float[adjustedAudios.Length];
		_baseVolume = new float[adjustedAudios.Length];
		for (int i = 0; i < adjustedAudios.Length; i++)
		{
			if (!(adjustedAudios[i] == null))
			{
				_basePitch[i] = adjustedAudios[i].pitchMultiplier;
				_baseVolume[i] = adjustedAudios[i].volumeMultiplier;
			}
		}
		_baseShakeAmplitude = new float[adjustedCameraShakes.Length];
		for (int j = 0; j < adjustedCameraShakes.Length; j++)
		{
			if (!(adjustedCameraShakes[j] == null))
			{
				_baseShakeAmplitude[j] = adjustedCameraShakes[j].amplitude;
			}
		}
		_baseShakeIntensity = new float[adjustedEntityShakes.Length];
		for (int k = 0; k < adjustedEntityShakes.Length; k++)
		{
			if (!(adjustedEntityShakes[k] == null))
			{
				_baseShakeIntensity[k] = adjustedEntityShakes[k].shakeIntensity;
			}
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		dmgFactor = _baseDmgFactor;
		range.transform.localScale = _baseRangeScale;
		startEffectNoStop.transform.localScale = _baseStartFxScale;
		for (int i = 0; i < adjustedAudios.Length; i++)
		{
			if (!(adjustedAudios[i] == null))
			{
				adjustedAudios[i].pitchMultiplier = _basePitch[i];
				adjustedAudios[i].volumeMultiplier = _baseVolume[i];
			}
		}
		for (int j = 0; j < adjustedCameraShakes.Length; j++)
		{
			if (!(adjustedCameraShakes[j] == null))
			{
				adjustedCameraShakes[j].amplitude = _baseShakeAmplitude[j];
			}
		}
		for (int k = 0; k < adjustedEntityShakes.Length; k++)
		{
			if (!(adjustedEntityShakes[k] == null))
			{
				adjustedEntityShakes[k].shakeIntensity = _baseShakeIntensity[k];
			}
		}
		isCritDamage = false;
	}

	protected override void OnCreate()
	{
		float num = pitchCurve.Evaluate(chargeDuration);
		float num2 = volumeCurve.Evaluate(chargeDuration);
		DewAudioSource[] array = adjustedAudios;
		foreach (DewAudioSource dewAudioSource in array)
		{
			if (!(dewAudioSource == null))
			{
				dewAudioSource.pitchMultiplier *= num;
				dewAudioSource.volumeMultiplier *= num2;
			}
		}
		float num3 = shakeCurve.Evaluate(chargeDuration);
		FxCameraShake[] array2 = adjustedCameraShakes;
		foreach (FxCameraShake fxCameraShake in array2)
		{
			if (!(fxCameraShake == null))
			{
				fxCameraShake.amplitude *= num3;
			}
		}
		FxEntityShake[] array3 = adjustedEntityShakes;
		foreach (FxEntityShake fxEntityShake in array3)
		{
			if (!(fxEntityShake == null))
			{
				fxEntityShake.shakeIntensity *= num3;
			}
		}
		range.transform.localScale *= explodeRadius;
		startEffectNoStop.transform.localScale *= explodeRadius;
		base.OnCreate();
	}

	protected override void OnBeforeDispatchDamage(ref DamageData dmg, Entity target)
	{
		base.OnBeforeDispatchDamage(ref dmg, target);
		if (isCritDamage)
		{
			dmg.SetAttr(DamageAttribute.IsCrit);
		}
	}

	protected override void OnHit(Entity entity)
	{
		base.OnHit(entity);
		CreateBasicEffect(entity, new StunEffect(), stunDuration);
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteFloat(writer, chargeDuration);
			NetworkWriterExtensions.WriteFloat(writer, explodeRadius);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x80L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, chargeDuration);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x100L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, explodeRadius);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref chargeDuration, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref explodeRadius, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x80L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref chargeDuration, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
		if ((num & 0x100L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref explodeRadius, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
	}
}
