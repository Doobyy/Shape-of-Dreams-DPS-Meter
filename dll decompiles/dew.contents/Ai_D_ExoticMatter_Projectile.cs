using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Ai_D_ExoticMatter_Projectile : StandardProjectile
{
	[NonSerialized]
	[SyncVar]
	public int explosionStack;

	public ScalingValue damagePerStack;

	public GameObject explosionEffect;

	public GameObject explosionHitEffect;

	public DewCollider range;

	public float procCoefficient;

	public AnimationCurve scaleCurve;

	public AnimationCurve explodePitchCurve;

	public AnimationCurve explodeVolumeCurve;

	public float scaleAmpPerUpgrade = 0.07f;

	public float scaleAmpMax = 1f;

	public Transform[] scaledTransforms;

	public DewAudioSource[] adjustedAudioSources;

	private Vector3[] _baseScaledTransformScales;

	public float currentScaleAmp => Mathf.Min((float)(skillLevel - 1) * scaleAmpPerUpgrade, scaleAmpMax);

	public override bool reuseInRoom => true;

	public int NetworkexplosionStack
	{
		get
		{
			return explosionStack;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<int>(value, ref explosionStack, 524288uL, (Action<int, int>)null);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		if (scaledTransforms == null)
		{
			return;
		}
		_baseScaledTransformScales = new Vector3[scaledTransforms.Length];
		for (int i = 0; i < scaledTransforms.Length; i++)
		{
			if (scaledTransforms[i] != null)
			{
				_baseScaledTransformScales[i] = scaledTransforms[i].localScale;
			}
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (_baseScaledTransformScales == null)
		{
			return;
		}
		for (int i = 0; i < scaledTransforms.Length; i++)
		{
			if (scaledTransforms[i] != null)
			{
				scaledTransforms[i].localScale = _baseScaledTransformScales[i];
			}
		}
	}

	protected override void OnCreate()
	{
		float time = (float)explosionStack / (float)FindFirstOfType<Se_D_ExoticMatter>().maxStack;
		Transform[] array = scaledTransforms;
		foreach (Transform transform in array)
		{
			if (!(transform == null))
			{
				transform.localScale *= scaleCurve.Evaluate(time) * (1f + currentScaleAmp);
			}
		}
		DewAudioSource[] array2 = adjustedAudioSources;
		foreach (DewAudioSource dewAudioSource in array2)
		{
			if (!(dewAudioSource == null))
			{
				dewAudioSource.pitchMultiplier = explodePitchCurve.Evaluate(time);
				dewAudioSource.volumeMultiplier = explodeVolumeCurve.Evaluate(time);
			}
		}
		base.OnCreate();
	}

	protected override void OnComplete()
	{
		base.OnComplete();
		FxPlayNewNetworked(explosionEffect, targetPosition, Quaternion.identity);
		((Component)(object)this).transform.position = targetPosition;
		ListReturnHandle<Entity> handle;
		foreach (Entity entity in range.GetEntities(out handle, tvDefaultHarmfulEffectTargets))
		{
			Damage(damagePerStack, procCoefficient).ApplyRawMultiplier(explosionStack).SetElemental(ElementalType.Light).SetOriginPosition(info.caster.position)
				.Dispatch(entity);
			FxPlayNewNetworked(explosionHitEffect, entity);
		}
		handle.Return();
		Destroy();
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteInt(writer, explosionStack);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x80000L) != 0L)
		{
			NetworkWriterExtensions.WriteInt(writer, explosionStack);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref explosionStack, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x80000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref explosionStack, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
		}
	}
}
