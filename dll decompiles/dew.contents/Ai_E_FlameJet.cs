using System;
using System.Collections;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Ai_E_FlameJet : AbilityInstance
{
	public int spawns = 8;

	public float spawnInterval = 0.375f;

	public float rotateSpeed = 40f;

	public float selfSlowAmount;

	public bool cancelable;

	public float uncancellableTime = 0.5f;

	[SyncVar]
	private Quaternion _syncedRotation;

	private ActorRef<StatusEffect> _eff;

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
		Network_syncedRotation = info.rotation;
		position = info.caster.position;
		rotation = _syncedRotation;
		base.OnCreate();
	}

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		_eff = CreateBasicEffect(info.caster, new SlowEffect
		{
			strength = selfSlowAmount
		}, (float)spawns * spawnInterval, "flamejet_selfslow");
		info.caster.Control.LockGamepadRotation();
		Channel channel = new Channel
		{
			duration = (float)spawns * spawnInterval,
			blockedActions = (Channel.BlockedAction)(6 | (cancelable ? 128 : 0)),
			uncancellableTime = uncancellableTime,
			onCancel = Destroy,
			onComplete = Destroy
		}.AddValidation(AbilitySelfValidator.Default);
		info.caster.Control.StartChannel(channel);
		for (int i = 0; i < spawns; i++)
		{
			if ((UnityEngine.Object)(object)firstTrigger != null)
			{
				firstTrigger.fillAmount = (float)(spawns - i) / (float)spawns;
			}
			CreateAbilityInstance<Ai_E_FlameJet_Projectile>(info.caster.position, Quaternion.identity, new CastInfo(info.caster, CastInfo.GetAngle(_syncedRotation)));
			yield return new SI.WaitForSeconds(spawnInterval);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			if (!_eff.IsNullOrInactive())
			{
				_eff.Get().Destroy();
			}
			if ((UnityEngine.Object)(object)firstTrigger != null)
			{
				firstTrigger.fillAmount = 0f;
			}
			if ((UnityEngine.Object)(object)info.caster != null)
			{
				info.caster.Animation.StopAbilityAnimation();
				info.caster.Control.UnlockGamepadRotation();
			}
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		Vector3 vector = default;
		bool flag = false;
		DewPlayer owner = info.caster.owner;
		if (owner.inputMode == InputMode.Gamepad && !owner.isGamepadExplicitAim)
		{
			if ((UnityEngine.Object)(object)owner.gamepadTargetEnemy != null)
			{
				flag = true;
				vector = owner.gamepadTargetEnemy.agentPosition;
			}
		}
		else
		{
			flag = true;
			vector = owner.cursorWorldPos;
		}
		if (flag)
		{
			Network_syncedRotation = Quaternion.RotateTowards(_syncedRotation, Quaternion.LookRotation(vector - info.caster.position), rotateSpeed * dt);
		}
		info.caster.Control.Rotate(_syncedRotation, immediately: false, 0.25f);
	}

	protected override void ActiveFrameUpdate()
	{
		base.ActiveFrameUpdate();
		position = info.caster.position;
		rotation = _syncedRotation;
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
