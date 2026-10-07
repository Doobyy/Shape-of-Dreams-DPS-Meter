using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Ai_Gem_E_Fever_Explosion : InstantDamageInstance
{
	public Transform[] limitedScaledTransforms;

	public Transform[] scaledTransforms;

	public FxPointLight light;

	[NonSerialized]
	[SyncVar]
	public float explosionRadius = 1f;

	[NonSerialized]
	[SyncVar]
	public float damageAmp;

	private bool _origCaptured;

	private Vector3[] _origLimitedScales;

	private Vector3[] _origScaledScales;

	private float _origLightIntensity;

	private float _origLightRange;

	private FxCameraShake _cameraShake;

	private float _origShakeAmplitude;

	public override bool reuseInRoom => true;

	public float NetworkexplosionRadius
	{
		get
		{
			return explosionRadius;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref explosionRadius, 128uL, (Action<float, float>)null);
		}
	}

	public float NetworkdamageAmp
	{
		get
		{
			return damageAmp;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref damageAmp, 256uL, (Action<float, float>)null);
		}
	}

	protected override void OnCreate()
	{
		if (!_origCaptured)
		{
			_origLimitedScales = new Vector3[limitedScaledTransforms.Length];
			for (int i = 0; i < limitedScaledTransforms.Length; i++)
			{
				if (limitedScaledTransforms[i] != null)
				{
					_origLimitedScales[i] = limitedScaledTransforms[i].localScale;
				}
			}
			_origScaledScales = new Vector3[scaledTransforms.Length];
			for (int j = 0; j < scaledTransforms.Length; j++)
			{
				if (scaledTransforms[j] != null)
				{
					_origScaledScales[j] = scaledTransforms[j].localScale;
				}
			}
			if (light != null)
			{
				_origLightIntensity = light.intensityMultiplier;
				_origLightRange = light.rangeMultiplier;
			}
			_cameraShake = ((Component)(object)this).GetComponentInChildren<FxCameraShake>();
			if (_cameraShake != null)
			{
				_origShakeAmplitude = _cameraShake.amplitude;
			}
			_origCaptured = true;
		}
		float num = Mathf.Clamp(explosionRadius, 0f, 5f);
		for (int k = 0; k < limitedScaledTransforms.Length; k++)
		{
			if (!(limitedScaledTransforms[k] == null))
			{
				limitedScaledTransforms[k].localScale = _origLimitedScales[k] * num;
			}
		}
		for (int l = 0; l < scaledTransforms.Length; l++)
		{
			if (!(scaledTransforms[l] == null))
			{
				scaledTransforms[l].localScale = _origScaledScales[l] * explosionRadius;
			}
		}
		if (light != null)
		{
			light.intensityMultiplier = _origLightIntensity * explosionRadius;
			light.rangeMultiplier = _origLightRange * explosionRadius;
		}
		if (_cameraShake != null)
		{
			_cameraShake.amplitude = _origShakeAmplitude * explosionRadius * 1.1f;
		}
		DewAudioSource componentInChildren = ((Component)(object)this).GetComponentInChildren<DewAudioSource>();
		if (componentInChildren != null)
		{
			componentInChildren.volumeMultiplier = Mathf.Clamp(0.3f + explosionRadius * 0.1f, 0f, 1f);
			componentInChildren.pitchMultiplier = Mathf.Clamp(2f - explosionRadius * 0.2f, 0.75f, 1.7f);
		}
		base.OnCreate();
	}

	protected override void OnBeforeDispatchDamage(ref DamageData dmg, Entity target)
	{
		base.OnBeforeDispatchDamage(ref dmg, target);
		if (damageAmp > 0.25f)
		{
			dmg.SetAttr(DamageAttribute.IsCrit);
		}
		dmg.ApplyAmplification(damageAmp);
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteFloat(writer, explosionRadius);
			NetworkWriterExtensions.WriteFloat(writer, damageAmp);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x80L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, explosionRadius);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x100L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, damageAmp);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref explosionRadius, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref damageAmp, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x80L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref explosionRadius, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
		if ((num & 0x100L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref damageAmp, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
	}
}
