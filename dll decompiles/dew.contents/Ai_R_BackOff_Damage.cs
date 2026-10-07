using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Ai_R_BackOff_Damage : InstantDamageInstance
{
	public ScalingValue maxDamage;

	public DewAudioSource[] adjustedAudios;

	public AnimationCurve pitchCurve;

	public AnimationCurve volumeCurve;

	public FxCameraShake[] adjustedCameraShakes;

	public FxEntityShake[] adjustedEntityShakes;

	public AnimationCurve shakeCurve;

	public float stunDuration = 1f;

	[NonSerialized]
	[SyncVar]
	public float chargeAmount;

	[NonSerialized]
	[SyncVar]
	public float angle;

	[NonSerialized]
	[SyncVar]
	public float radius;

	[NonSerialized]
	private ScalingValue _baseDmgFactor;

	[NonSerialized]
	private Vector3 _baseStartFxScale;

	[NonSerialized]
	private Vector3 _baseStartFxPos;

	[NonSerialized]
	private float[] _basePitch;

	[NonSerialized]
	private float[] _baseVolume;

	[NonSerialized]
	private float[] _baseShakeAmplitude;

	[NonSerialized]
	private float[] _baseShakeIntensity;

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
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref chargeAmount, 128uL, (Action<float, float>)null);
		}
	}

	public float Networkangle
	{
		get
		{
			return angle;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref angle, 256uL, (Action<float, float>)null);
		}
	}

	public float Networkradius
	{
		get
		{
			return radius;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref radius, 512uL, (Action<float, float>)null);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		_baseDmgFactor = dmgFactor;
		_baseStartFxScale = startEffectNoStop.transform.localScale;
		_baseStartFxPos = startEffectNoStop.transform.localPosition;
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
		startEffectNoStop.transform.localScale = _baseStartFxScale;
		startEffectNoStop.transform.localPosition = _baseStartFxPos;
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
	}

	protected override void OnPrepare()
	{
		base.OnPrepare();
		dmgFactor = ScalingValue.Lerp(dmgFactor, maxDamage, chargeAmount);
		range.GeneratePolygonPoints_Arc(radius, angle);
		range.UpdateProxyCollider();
	}

	protected override void OnCreate()
	{
		startEffectNoStop.transform.localScale = Vector3.Scale(startEffectNoStop.transform.localScale, Vector3.Lerp(Vector3.one, new Vector3(2.7f, 1f, 3.2f), chargeAmount));
		startEffectNoStop.transform.localPosition += Vector3.Lerp(Vector3.zero, new Vector3(0f, 0f, -4.2f), chargeAmount);
		float num = pitchCurve.Evaluate(chargeAmount);
		float num2 = volumeCurve.Evaluate(chargeAmount);
		DewAudioSource[] array = adjustedAudios;
		foreach (DewAudioSource dewAudioSource in array)
		{
			if (!(dewAudioSource == null))
			{
				dewAudioSource.pitchMultiplier *= num;
				dewAudioSource.volumeMultiplier *= num2;
			}
		}
		float num3 = shakeCurve.Evaluate(chargeAmount);
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
		base.OnCreate();
	}

	protected override void OnBeforeDispatchDamage(ref DamageData dmg, Entity target)
	{
		base.OnBeforeDispatchDamage(ref dmg, target);
		if (chargeAmount > 0.5f)
		{
			dmg.SetAttr(DamageAttribute.IsCrit);
		}
	}

	protected override void OnHit(Entity entity)
	{
		base.OnHit(entity);
		if (stunDuration > 0f)
		{
			CreateBasicEffect(entity, new StunEffect(), stunDuration);
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
			NetworkWriterExtensions.WriteFloat(writer, angle);
			NetworkWriterExtensions.WriteFloat(writer, radius);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x80L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, chargeAmount);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x100L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, angle);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x200L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, radius);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref chargeAmount, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref angle, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref radius, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x80L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref chargeAmount, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
		if ((num & 0x100L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref angle, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
		if ((num & 0x200L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref radius, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
	}
}
