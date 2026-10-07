using System;
using System.Collections;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Ai_Mon_LavaLand_BossInfernus_BreathFire : AbilityInstance
{
	public int spawns = 8;

	public float spawnInterval = 0.375f;

	public Vector2 rotateSpeedRange = new Vector2(20f, 120f);

	public float endDaze = 2f;

	[SyncVar]
	private Quaternion _syncedRotation;

	private float _normalizedElapsedTime;

	public override bool reuseInRoom => true;

	public Quaternion Network_syncedRotation
	{
		get
		{
			return _syncedRotation;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<Quaternion>(value, ref _syncedRotation, 64uL, (Action<Quaternion, Quaternion>)null);
		}
	}

	protected override void OnCreate()
	{
		Network_syncedRotation = info.caster.rotation;
		position = info.caster.position;
		rotation = _syncedRotation;
		base.OnCreate();
	}

	protected override IEnumerator OnCreateSequenced()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			BossMonster.RevealStealthedBeforeSpecialAttack();
			CreateBasicEffect(info.caster, new UnstoppableEffect(), float.PositiveInfinity).DestroyOnDestroy(this);
			Channel channel = new Channel
			{
				duration = (float)spawns * spawnInterval,
				blockedActions = Channel.BlockedAction.Everything,
				onCancel = Destroy,
				onComplete = Destroy
			};
			info.caster.Control.StartChannel(channel);
			for (int i = 0; i < spawns; i++)
			{
				_normalizedElapsedTime = (float)i / (float)spawns;
				CreateAbilityInstance<Ai_Mon_LavaLand_BossInfernus_BreathFire_Projectile>(info.caster.position, Quaternion.identity, new CastInfo(info.caster, CastInfo.GetAngle(_syncedRotation)));
				yield return new SI.WaitForSeconds(spawnInterval);
			}
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && !info.caster.IsNullInactiveDeadOrKnockedOut())
		{
			info.caster.Animation.StopAbilityAnimation();
			info.caster.Control.StartDaze(endDaze);
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		if (info.target.IsNullInactiveDeadOrKnockedOut())
		{
			Hero closestAliveHero = Dew.GetClosestAliveHero(info.caster.position, fallbackToDead: false, info.caster);
			if ((UnityEngine.Object)(object)closestAliveHero != null)
			{
				info = new CastInfo(info.caster, closestAliveHero);
			}
		}
		if (!info.target.IsNullInactiveDeadOrKnockedOut())
		{
			float num = Mathf.Lerp(rotateSpeedRange.x, rotateSpeedRange.y, _normalizedElapsedTime);
			Network_syncedRotation = Quaternion.RotateTowards(_syncedRotation, Quaternion.LookRotation(info.target.GetAIAgentPosition(info.caster) - info.caster.position), num * dt);
		}
		info.caster.Control.Rotate(_syncedRotation, immediately: false);
	}

	protected override void ActiveFrameUpdate()
	{
		base.ActiveFrameUpdate();
		position = info.caster.position;
		rotation = _syncedRotation;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		_normalizedElapsedTime = 0f;
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteQuaternion(writer, _syncedRotation);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x40L) != 0L)
		{
			NetworkWriterExtensions.WriteQuaternion(writer, _syncedRotation);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<Quaternion>(ref _syncedRotation, (Action<Quaternion, Quaternion>)null, NetworkReaderExtensions.ReadQuaternion(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x40L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<Quaternion>(ref _syncedRotation, (Action<Quaternion, Quaternion>)null, NetworkReaderExtensions.ReadQuaternion(reader));
		}
	}
}
