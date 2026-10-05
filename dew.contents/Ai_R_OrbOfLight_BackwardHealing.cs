using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Ai_R_OrbOfLight_BackwardHealing : StandardProjectile
{
	public ScalingValue healAmountPerEnemy;

	[NonSerialized]
	[SyncVar]
	public int hitEnemies;

	private Vector3 _baseFlyScale;

	private DewAudioSource[] _au;

	private float[] _basePitch;

	private float[] _baseVolume;

	public override bool reuseInRoom => true;

	public int NetworkhitEnemies
	{
		get
		{
			return hitEnemies;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<int>(value, ref hitEnemies, 524288uL, (Action<int, int>)null);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		_baseFlyScale = effectOnFly.transform.localScale;
		_au = effectOnFly.GetComponentsInChildren<DewAudioSource>(includeInactive: true);
		_basePitch = new float[_au.Length];
		_baseVolume = new float[_au.Length];
		for (int i = 0; i < _au.Length; i++)
		{
			_basePitch[i] = _au[i].pitchMultiplier;
			_baseVolume[i] = _au[i].volumeMultiplier;
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		NetworkhitEnemies = 0;
		effectOnFly.transform.localScale = _baseFlyScale;
		if (_au != null)
		{
			for (int i = 0; i < _au.Length; i++)
			{
				_au[i].pitchMultiplier = _basePitch[i];
				_au[i].volumeMultiplier = _baseVolume[i];
			}
		}
	}

	protected override void OnCreate()
	{
		effectOnFly.transform.localScale *= 0.6f + (float)Mathf.Clamp(hitEnemies, 0, 6) * 0.1f;
		ListReturnHandle<DewAudioSource> handle;
		foreach (DewAudioSource item in effectOnFly.GetComponentsInChildrenNonAlloc(out handle))
		{
			item.pitchMultiplier *= 0.5f + (float)Mathf.Clamp(hitEnemies, 0, 6) * 0.1f;
			item.volumeMultiplier *= 0.3f + (float)Mathf.Clamp(hitEnemies, 0, 5) * 0.14f;
		}
		handle.Return();
		base.OnCreate();
	}

	protected override void OnEntity(EntityHit hit)
	{
		base.OnEntity(hit);
		if (!((UnityEngine.Object)(object)hit.entity == (UnityEngine.Object)(object)info.caster) && hitEnemies > 0)
		{
			Heal(GetValue(healAmountPerEnemy) * (float)hitEnemies).Dispatch(hit.entity);
		}
	}

	protected override void OnComplete()
	{
		base.OnComplete();
		if (hitEnemies > 0)
		{
			Heal(GetValue(healAmountPerEnemy) * (float)hitEnemies).Dispatch(info.caster);
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
			NetworkWriterExtensions.WriteInt(writer, hitEnemies);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x80000L) != 0L)
		{
			NetworkWriterExtensions.WriteInt(writer, hitEnemies);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref hitEnemies, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x80000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref hitEnemies, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
		}
	}
}
