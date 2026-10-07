using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Ai_HunterArtillery_Big : InstantDamageInstance
{
	public float stunDuration = 1f;

	[NonSerialized]
	[SyncVar]
	public float speedMultiplier = 1f;

	private ParticleSystem[] _particleSystems;

	private float[] _baseSimulationSpeeds;

	private float _baseDamageDelay;

	public override bool reuseInRoom => true;

	public float NetworkspeedMultiplier
	{
		get
		{
			return speedMultiplier;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref speedMultiplier, 128uL, (Action<float, float>)null);
		}
	}

	protected override void Awake()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		base.Awake();
		_particleSystems = ((Component)(object)this).GetComponentsInChildren<ParticleSystem>(true);
		_baseSimulationSpeeds = new float[_particleSystems.Length];
		for (int i = 0; i < _particleSystems.Length; i++)
		{
			float[] baseSimulationSpeeds = _baseSimulationSpeeds;
			int num = i;
			MainModule main = _particleSystems[i].main;
			baseSimulationSpeeds[num] = main.simulationSpeed;
		}
		_baseDamageDelay = damageDelay;
	}

	protected override void OnCreate()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < _particleSystems.Length; i++)
		{
			MainModule main = _particleSystems[i].main;
			main.simulationSpeed = _baseSimulationSpeeds[i] * speedMultiplier;
		}
		if (((NetworkBehaviour)this).isServer)
		{
			damageDelay = _baseDamageDelay / speedMultiplier;
		}
		base.OnCreate();
	}

	protected override void OnDisable()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		base.OnDisable();
		for (int i = 0; i < _particleSystems.Length; i++)
		{
			MainModule main = _particleSystems[i].main;
			main.simulationSpeed = _baseSimulationSpeeds[i];
		}
		damageDelay = _baseDamageDelay;
		NetworkspeedMultiplier = 1f;
	}

	protected override void OnHit(Entity entity)
	{
		base.OnHit(entity);
		CreateBasicEffect(entity, new StunEffect(), stunDuration, "ArtilleryStun", DuplicateEffectBehavior.UsePrevious);
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteFloat(writer, speedMultiplier);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x80L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, speedMultiplier);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref speedMultiplier, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x80L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref speedMultiplier, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
	}
}
