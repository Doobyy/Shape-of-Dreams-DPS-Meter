using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Ai_Q_BigBorealChunk_Projectile : StandardProjectile
{
	public DewAudioSource[] adjustedAudios;

	public AnimationCurve pitchCurve;

	public AnimationCurve volumeCurve;

	[NonSerialized]
	[SyncVar]
	public float chargeDuration;

	[NonSerialized]
	[SyncVar]
	public float explodeRadius;

	[NonSerialized]
	[SyncVar]
	public float damageAmp;

	[NonSerialized]
	[SyncVar]
	public float chunkVisualScale;

	[NonSerialized]
	private Vector3 _baseFlyScale;

	[NonSerialized]
	private float[] _basePitch;

	[NonSerialized]
	private float[] _baseVolume;

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
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref chargeDuration, 524288uL, (Action<float, float>)null);
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
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref explodeRadius, 1048576uL, (Action<float, float>)null);
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
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref damageAmp, 2097152uL, (Action<float, float>)null);
		}
	}

	public float NetworkchunkVisualScale
	{
		get
		{
			return chunkVisualScale;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref chunkVisualScale, 4194304uL, (Action<float, float>)null);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		_baseFlyScale = effectOnFly.transform.localScale;
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
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		effectOnFly.transform.localScale = _baseFlyScale;
		for (int i = 0; i < adjustedAudios.Length; i++)
		{
			if (!(adjustedAudios[i] == null))
			{
				adjustedAudios[i].pitchMultiplier = _basePitch[i];
				adjustedAudios[i].volumeMultiplier = _baseVolume[i];
			}
		}
	}

	protected override void OnEntity(EntityHit hit)
	{
		base.OnEntity(hit);
		CreateExplosion();
		Destroy();
	}

	protected override void OnComplete()
	{
		base.OnComplete();
		CreateExplosion();
	}

	private void CreateExplosion()
	{
		CreateAbilityInstance(position, null, new CastInfo(info.caster), (Ai_Q_BigBorealChunk_Explosion ai) =>
		{
			ai.NetworkexplodeRadius = explodeRadius;
			ai.dmgFactor *= 1f + damageAmp;
			ai.isCritDamage = chargeDuration > 0.6f;
			ai.NetworkchargeDuration = chargeDuration;
		});
	}

	protected override void OnCreate()
	{
		effectOnFly.transform.localScale *= chunkVisualScale;
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
		base.OnCreate();
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
			NetworkWriterExtensions.WriteFloat(writer, damageAmp);
			NetworkWriterExtensions.WriteFloat(writer, chunkVisualScale);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x80000L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, chargeDuration);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x100000L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, explodeRadius);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x200000L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, damageAmp);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x400000L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, chunkVisualScale);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref chargeDuration, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref explodeRadius, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref damageAmp, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref chunkVisualScale, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x80000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref chargeDuration, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
		if ((num & 0x100000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref explodeRadius, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
		if ((num & 0x200000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref damageAmp, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
		if ((num & 0x400000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref chunkVisualScale, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
	}
}
