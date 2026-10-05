using System;
using System.Collections;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Ai_Mon_Forest_BossDemon_SpecialAtk : AbilityInstance
{
	public int spawnCount;

	public float spawnInterval;

	public float spawnDistance;

	public float duration;

	public float postDelay;

	public float rotStartSpeed;

	public float rotMaxSpeed;

	public GameObject fxCharged;

	public GameObject fxEnd;

	public GameObject fxTelegraph;

	public DewAnimationClip startAnim;

	public DewAnimationClip endAnim;

	[SyncVar]
	private Quaternion _syncedRotation;

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

	protected override void OnPrepare()
	{
		base.OnPrepare();
		Network_syncedRotation = info.caster.rotation;
	}

	protected override IEnumerator OnCreateSequenced()
	{
		info.caster.rotation = _syncedRotation;
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		DestroyOnDeath(info.caster);
		CreateBasicEffect(info.caster, new UnstoppableEffect(), float.PositiveInfinity).DestroyOnDestroy(this);
		FxPlayNetworked(fxCharged, info.caster);
		info.caster.Control.RotateTowards(info.target, immediately: false);
		info.caster.Animation.PlayAbilityAnimation(startAnim);
		float elapsedTime = 0f;
		info.caster.Control.StartChannel(new Channel
		{
			blockedActions = Channel.BlockedAction.Everything,
			duration = duration,
			onCancel = DestroyIfActive,
			onTick = (float dt) =>
			{
				elapsedTime += dt;
				float t = Mathf.Clamp01(elapsedTime / duration);
				float num = Mathf.Lerp(rotStartSpeed, rotMaxSpeed, t);
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
					Vector3 vector = AbilityTrigger.PredictPoint_Simple(info.caster, UnityEngine.Random.Range(0.5f, 1f), info.target, postDelay + dt);
					Network_syncedRotation = Quaternion.RotateTowards(_syncedRotation, Quaternion.LookRotation(vector - info.caster.agentPosition), num * dt);
				}
				info.caster.Control.Rotate(_syncedRotation, immediately: false);
			},
			onComplete = () =>
			{
				info.caster.Control.StartDaze(2f);
				((MonoBehaviour)(object)this).StartCoroutine(Routine());
			}
		});
		IEnumerator Routine()
		{
			FxPlayNetworked(fxTelegraph, info.caster.agentPosition, Quaternion.LookRotation(((Component)(object)info.caster).transform.forward));
			info.caster.Control.StopOverrideRotation();
			info.caster.Control.StartDaze(postDelay);
			yield return new WaitForSeconds(postDelay);
			FxStopNetworked(fxCharged);
			FxPlayNetworked(fxEnd, info.caster);
			info.caster.Animation.PlayAbilityAnimation(endAnim);
			for (int i = 0; i < spawnCount; i++)
			{
				Vector3 point = info.caster.agentPosition + ((Component)(object)info.caster).transform.forward * (spawnDistance * (float)i);
				CreateAbilityInstance<Ai_Mon_Forest_BossDemon_SpecialAtk_Instance>(point, ((Component)(object)info.caster).transform.rotation, new CastInfo(info.caster, point));
				yield return new WaitForSeconds(spawnInterval);
			}
			Destroy();
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			FxStopNetworked(fxCharged);
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
