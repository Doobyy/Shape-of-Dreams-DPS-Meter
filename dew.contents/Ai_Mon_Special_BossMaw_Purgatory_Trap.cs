using System;
using System.Collections;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Ai_Mon_Special_BossMaw_Purgatory_Trap : AbilityInstance
{
	public float duration;

	public float explosionDelay;

	public float range;

	public float rotationSpeed;

	public GameObject fxTrap;

	public GameObject fxTelegraph;

	[SyncVar]
	private Vector3 _desiredPosition;

	[SyncVar]
	private bool _canExplode;

	private Vector3 _cv;

	public Vector3 Network_desiredPosition
	{
		get
		{
			return _desiredPosition;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<Vector3>(value, ref _desiredPosition, 64uL, (Action<Vector3, Vector3>)null);
		}
	}

	public bool Network_canExplode
	{
		get
		{
			return _canExplode;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref _canExplode, 128uL, (Action<bool, bool>)null);
		}
	}

	protected override IEnumerator OnCreateSequenced()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			Network_desiredPosition = info.point;
			DestroyOnDeath(info.caster);
			FxPlayNetworked(fxTrap);
			yield return new SI.WaitForCondition(() => _canExplode);
			FxPlayNetworked(fxTelegraph);
			yield return new SI.WaitForSeconds(explosionDelay);
			CreateAbilityInstance<Ai_Mon_Special_BossMaw_Purgatory_Instance>(position, null, new CastInfo(info.caster, position));
			Destroy();
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!((NetworkBehaviour)this).isServer || _canExplode)
		{
			return;
		}
		if ((bool)SingletonBehaviour<Room_BossArena>.instance)
		{
			Vector3 vector = _desiredPosition - SingletonBehaviour<Room_BossArena>.instance.center;
			vector = Quaternion.Euler(0f, rotationSpeed * dt, 0f) * vector;
			Network_desiredPosition = SingletonBehaviour<Room_BossArena>.instance.center + vector;
		}
		if (Time.time - creationTime > duration)
		{
			Network_canExplode = true;
		}
		else if (!(Time.time - creationTime < 0.65f))
		{
			if (DewPhysics.OverlapCircleAllEntities(out var handle, position, range, tvDefaultHarmfulEffectTargets).Count > 0)
			{
				Network_canExplode = true;
			}
			handle.Return();
		}
	}

	protected override void ActiveFrameUpdate()
	{
		base.ActiveFrameUpdate();
		position = Dew.GetPositionOnGround(Vector3.SmoothDamp(position, _desiredPosition, ref _cv, 0.25f));
		rotation = Quaternion.Euler(0f, rotation.eulerAngles.y + Time.deltaTime * -30f, 0f);
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			FxStopNetworked(fxTrap);
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
			NetworkWriterExtensions.WriteVector3(writer, _desiredPosition);
			NetworkWriterExtensions.WriteBool(writer, _canExplode);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x40L) != 0L)
		{
			NetworkWriterExtensions.WriteVector3(writer, _desiredPosition);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x80L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, _canExplode);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<Vector3>(ref _desiredPosition, (Action<Vector3, Vector3>)null, NetworkReaderExtensions.ReadVector3(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref _canExplode, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x40L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<Vector3>(ref _desiredPosition, (Action<Vector3, Vector3>)null, NetworkReaderExtensions.ReadVector3(reader));
		}
		if ((num & 0x80L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref _canExplode, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
		}
	}
}
