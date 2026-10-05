using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Ai_Q_CruelSun_Shockwave : InstantDamageInstance
{
	public GameObject fxColdEffect;

	public DewAudioSource[] audios;

	public FxCameraShake[] shakes;

	public AnimationCurve audioPitch;

	public AnimationCurve audioVolume;

	public Transform[] effectTransforms;

	public float stunDurationMin = 0.5f;

	public float stunDurationMax = 2f;

	public ScalingValue damageMin;

	public ScalingValue damageMax;

	[NonSerialized]
	[SyncVar]
	public bool doColdDamage;

	[SyncVar]
	internal float _length;

	[SyncVar]
	internal float _width;

	[SyncVar]
	internal float _chargeAmount;

	private Vector3 _baseRangeScale;

	private Vector3 _baseStartEffectNoStopScale;

	private Vector3[] _baseEffectTransformScales;

	private float[] _baseAudioPitch;

	private float[] _baseAudioVolume;

	private float[] _baseShakeAmplitude;

	private float _baseKnockupAmount;

	private ElementalType _baseElemental;

	private float _baseStunDurationMin;

	private float _baseStunDurationMax;

	private ScalingValue _baseDamageMin;

	private ScalingValue _baseDamageMax;

	private DewEffect.FxColorSnapshot _colorSnapshot;

	private bool _colorsAreCold;

	private FxPointLight[] _pointLights;

	private float[] _baseLightIntensity;

	public override bool reuseInRoom => true;

	public bool NetworkdoColdDamage
	{
		get
		{
			return doColdDamage;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref doColdDamage, 128uL, (Action<bool, bool>)null);
		}
	}

	public float Network_length
	{
		get
		{
			return _length;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref _length, 256uL, (Action<float, float>)null);
		}
	}

	public float Network_width
	{
		get
		{
			return _width;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref _width, 512uL, (Action<float, float>)null);
		}
	}

	public float Network_chargeAmount
	{
		get
		{
			return _chargeAmount;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref _chargeAmount, 1024uL, (Action<float, float>)null);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		_baseRangeScale = range.transform.localScale;
		_baseStartEffectNoStopScale = startEffectNoStop.transform.localScale;
		_baseEffectTransformScales = new Vector3[effectTransforms.Length];
		for (int i = 0; i < effectTransforms.Length; i++)
		{
			if (effectTransforms[i] != null)
			{
				_baseEffectTransformScales[i] = effectTransforms[i].localScale;
			}
		}
		_baseAudioPitch = new float[audios.Length];
		_baseAudioVolume = new float[audios.Length];
		for (int j = 0; j < audios.Length; j++)
		{
			if (audios[j] != null)
			{
				_baseAudioPitch[j] = audios[j].pitchMultiplier;
				_baseAudioVolume[j] = audios[j].volumeMultiplier;
			}
		}
		_baseShakeAmplitude = new float[shakes.Length];
		for (int k = 0; k < shakes.Length; k++)
		{
			if (shakes[k] != null)
			{
				_baseShakeAmplitude[k] = shakes[k].amplitude;
			}
		}
		_baseKnockupAmount = knockupAmount;
		_baseElemental = elemental;
		_baseStunDurationMin = stunDurationMin;
		_baseStunDurationMax = stunDurationMax;
		_baseDamageMin = damageMin;
		_baseDamageMax = damageMax;
		_colorSnapshot = DewEffect.CaptureColorsRecursively(((Component)(object)this).gameObject);
		_pointLights = ((Component)(object)this).GetComponentsInChildren<FxPointLight>();
		_baseLightIntensity = new float[_pointLights.Length];
		for (int l = 0; l < _pointLights.Length; l++)
		{
			if (_pointLights[l] != null)
			{
				_baseLightIntensity[l] = _pointLights[l].intensityMultiplier;
			}
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		range.transform.localScale = _baseRangeScale;
		startEffectNoStop.transform.localScale = _baseStartEffectNoStopScale;
		for (int i = 0; i < effectTransforms.Length; i++)
		{
			if (effectTransforms[i] != null)
			{
				effectTransforms[i].localScale = _baseEffectTransformScales[i];
			}
		}
		for (int j = 0; j < audios.Length; j++)
		{
			if (audios[j] != null)
			{
				audios[j].pitchMultiplier = _baseAudioPitch[j];
				audios[j].volumeMultiplier = _baseAudioVolume[j];
			}
		}
		for (int k = 0; k < shakes.Length; k++)
		{
			if (shakes[k] != null)
			{
				shakes[k].amplitude = _baseShakeAmplitude[k];
			}
		}
		for (int l = 0; l < _pointLights.Length; l++)
		{
			if (_pointLights[l] != null)
			{
				_pointLights[l].intensityMultiplier = _baseLightIntensity[l];
			}
		}
		knockupAmount = _baseKnockupAmount;
		elemental = _baseElemental;
		stunDurationMin = _baseStunDurationMin;
		stunDurationMax = _baseStunDurationMax;
		damageMin = _baseDamageMin;
		damageMax = _baseDamageMax;
		if (_colorsAreCold)
		{
			DewEffect.RestoreColorsRecursively(_colorSnapshot);
			_colorsAreCold = false;
		}
	}

	protected override void OnCreate()
	{
		if (doColdDamage)
		{
			DewEffect.ChangeColorRecursively(((Component)(object)this).gameObject, 0.55f, 0.7f, 0.7f);
			_colorsAreCold = true;
			FxPlay(fxColdEffect);
		}
		Vector3 b = new Vector3(_width, 1f, _length);
		if (((NetworkBehaviour)this).isServer)
		{
			range.transform.localScale = Vector3.Scale(range.transform.localScale, b);
			dmgFactor = ScalingValue.Lerp(damageMin, damageMax, _chargeAmount);
			knockupAmount *= Mathf.Lerp(0.2f, 1f, _chargeAmount);
			if (doColdDamage)
			{
				elemental = ElementalType.Cold;
			}
		}
		DewAudioSource[] array = audios;
		foreach (DewAudioSource dewAudioSource in array)
		{
			if (!(dewAudioSource == null))
			{
				dewAudioSource.pitchMultiplier *= audioPitch.Evaluate(_chargeAmount);
				dewAudioSource.volumeMultiplier *= audioVolume.Evaluate(_chargeAmount);
			}
		}
		FxCameraShake[] array2 = shakes;
		foreach (FxCameraShake fxCameraShake in array2)
		{
			if (!(fxCameraShake == null))
			{
				fxCameraShake.amplitude *= Mathf.Lerp(0.25f, 1f, _chargeAmount);
			}
		}
		for (int j = 0; j < effectTransforms.Length; j++)
		{
			Transform transform = effectTransforms[j];
			if (!(transform == null))
			{
				transform.localScale *= _width * 0.5f;
				transform.localPosition = Vector3.forward * _length * ((float)(j + 1) / (float)(effectTransforms.Length + 1));
			}
		}
		FxPointLight[] pointLights = _pointLights;
		foreach (FxPointLight fxPointLight in pointLights)
		{
			if (!(fxPointLight == null))
			{
				fxPointLight.intensityMultiplier *= _chargeAmount;
			}
		}
		startEffectNoStop.transform.localScale *= Mathf.Lerp(0.9f, 1.5f, _chargeAmount);
		base.OnCreate();
	}

	protected override void OnBeforeDispatchDamage(ref DamageData dmg, Entity target)
	{
		base.OnBeforeDispatchDamage(ref dmg, target);
		if (_chargeAmount > 0.99f)
		{
			dmg.SetAttr(DamageAttribute.IsCrit);
		}
	}

	protected override void OnHit(Entity entity)
	{
		base.OnHit(entity);
		CreateBasicEffect(entity, new StunEffect(), Mathf.Lerp(stunDurationMin, stunDurationMax, _chargeAmount), "SuppressionStun", DuplicateEffectBehavior.UsePrevious);
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteBool(writer, doColdDamage);
			NetworkWriterExtensions.WriteFloat(writer, _length);
			NetworkWriterExtensions.WriteFloat(writer, _width);
			NetworkWriterExtensions.WriteFloat(writer, _chargeAmount);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x80L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, doColdDamage);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x100L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, _length);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x200L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, _width);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x400L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, _chargeAmount);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref doColdDamage, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _length, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _width, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _chargeAmount, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x80L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref doColdDamage, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
		}
		if ((num & 0x100L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _length, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
		if ((num & 0x200L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _width, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
		if ((num & 0x400L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _chargeAmount, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
	}
}
