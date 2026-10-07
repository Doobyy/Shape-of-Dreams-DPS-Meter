using System;
using System.Collections;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Ai_Mon_Ink_BossDarkMoon_SpawnEgoSword : AbilityInstance
{
	internal float _spawnTime = float.PositiveInfinity;

	public float firstSpawnTime;

	public float coolDownTime;

	public float castDuration;

	public GameObject fxSword;

	[SyncVar(hook = "OnSyncedPosChanged")]
	private Vector3 _syncedPos;

	[SyncVar(hook = "OnSyncedRotChanged")]
	private Quaternion _syncedRot;

	public Action<Vector3, Vector3> _Mirror_SyncVarHookDelegate__syncedPos;

	public Action<Quaternion, Quaternion> _Mirror_SyncVarHookDelegate__syncedRot;

	public Vector3 Network_syncedPos
	{
		get
		{
			return _syncedPos;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<Vector3>(value, ref _syncedPos, 64uL, _Mirror_SyncVarHookDelegate__syncedPos);
		}
	}

	public Quaternion Network_syncedRot
	{
		get
		{
			return _syncedRot;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<Quaternion>(value, ref _syncedRot, 128uL, _Mirror_SyncVarHookDelegate__syncedRot);
		}
	}

	private void OnSyncedPosChanged(Vector3 _, Vector3 __)
	{
		position = _syncedPos;
	}

	private void OnSyncedRotChanged(Quaternion _, Quaternion __)
	{
		rotation = _syncedRot;
	}

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		DestroyOnDeath(info.caster);
		_spawnTime = float.PositiveInfinity;
		Network_syncedPos = position;
		Network_syncedRot = rotation;
		FxPlayNetworked(fxSword, info.caster);
		while (true)
		{
			yield return new SI.WaitForCondition(() => _spawnTime - Time.time < 0.001f);
			Entity entity = info.target;
			if (entity.IsNullInactiveDeadOrKnockedOut())
			{
				entity = Dew.GetClosestAliveHero(info.caster.position, fallbackToDead: true, info.caster);
			}
			FxStopNetworked(fxSword);
			CreateAbilityInstance<Ai_Mon_Ink_BossDarkMoon_SpawnEgoSword_Spawner>(_syncedPos, rotation, new CastInfo(info.caster, entity));
			yield return new SI.WaitForSeconds(coolDownTime - castDuration);
			FxPlayNetworked(fxSword);
			_spawnTime = float.PositiveInfinity;
			yield return new SI.WaitForSeconds(castDuration);
		}
	}

	protected override void ActiveFrameUpdate()
	{
		base.ActiveFrameUpdate();
		if (((NetworkBehaviour)this).isServer)
		{
			Network_syncedPos = position;
			Network_syncedRot = rotation;
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			FxStopNetworked(fxSword);
		}
	}

	public Ai_Mon_Ink_BossDarkMoon_SpawnEgoSword()
	{
		_Mirror_SyncVarHookDelegate__syncedPos = OnSyncedPosChanged;
		_Mirror_SyncVarHookDelegate__syncedRot = OnSyncedRotChanged;
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteVector3(writer, _syncedPos);
			NetworkWriterExtensions.WriteQuaternion(writer, _syncedRot);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x40L) != 0L)
		{
			NetworkWriterExtensions.WriteVector3(writer, _syncedPos);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x80L) != 0L)
		{
			NetworkWriterExtensions.WriteQuaternion(writer, _syncedRot);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<Vector3>(ref _syncedPos, _Mirror_SyncVarHookDelegate__syncedPos, NetworkReaderExtensions.ReadVector3(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<Quaternion>(ref _syncedRot, _Mirror_SyncVarHookDelegate__syncedRot, NetworkReaderExtensions.ReadQuaternion(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x40L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<Vector3>(ref _syncedPos, _Mirror_SyncVarHookDelegate__syncedPos, NetworkReaderExtensions.ReadVector3(reader));
		}
		if ((num & 0x80L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<Quaternion>(ref _syncedRot, _Mirror_SyncVarHookDelegate__syncedRot, NetworkReaderExtensions.ReadQuaternion(reader));
		}
	}
}
