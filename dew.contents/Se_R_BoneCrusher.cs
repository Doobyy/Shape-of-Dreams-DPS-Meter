using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;
using UnityEngine.AI;

public class Se_R_BoneCrusher : StatusEffect
{
	public GameObject fxHit;

	public DewCollider range;

	public ChargingChannelData channel;

	public float initSpeed = 2f;

	public float targetSpeed = 20f;

	public float acceleration = 10f;

	public ScalingValue midChargeHitDamage;

	private List<Entity> _hitEntities = new List<Entity>();

	private ChargingChannel _channel;

	[NonSerialized]
	[SyncVar]
	public float directionAngle;

	private ActorRef<Se_R_BoneCrusher_UnstoppableAndShield> _shield;

	public override bool reuseInRoom => true;

	public float NetworkdirectionAngle
	{
		get
		{
			return directionAngle;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref directionAngle, 4096uL, (Action<float, float>)null);
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		_hitEntities.Clear();
		_channel = null;
		_shield = null;
		NetworkdirectionAngle = 0f;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			_shield = CreateStatusEffect<Se_R_BoneCrusher_UnstoppableAndShield>(info.caster);
			DoUnstoppable();
			_channel = channel.Get(this).SetInitialInfo(info).OnCancel((ChargingChannel _) =>
			{
				Collide();
			})
				.Dispatch(info.caster, firstTrigger);
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (_channel != null)
		{
			NetworkdirectionAngle = _channel.castInfo.angle;
		}
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		((Component)(object)this).transform.position = info.caster.position;
		((Component)(object)this).transform.rotation = info.caster.rotation;
		if (!_shield.IsNullOrInactive())
		{
			_shield.Get().ResetTimer();
		}
		info.caster.Control.Rotate(directionAngle, immediately: false);
		Vector3 vector = info.caster.agentPosition + Quaternion.Euler(0f, directionAngle, 0f) * Vector3.forward * 1f;
		NavMeshHit val = default;
		if (NavMesh.Raycast(info.caster.agentPosition, vector, ref val, -1))
		{
			Collide();
			return;
		}
		ListReturnHandle<Entity> handle;
		foreach (Entity e in range.GetEntities(out handle, tvDefaultHarmfulEffectTargets))
		{
			if (!_hitEntities.Contains(e))
			{
				_hitEntities.Add(e);
				Damage(midChargeHitDamage).SetDirection(Quaternion.Euler(0f, directionAngle, 0f)).SetOriginPosition(info.caster.position).Dispatch(e);
				FxPlayNewNetworked(fxHit, e);
			}
			if (e.Status.hasUnstoppable || e.Status.HasStatusEffect<Se_R_BoneCrusher_Kidnapped>())
			{
				continue;
			}
			CreateStatusEffect(e, (Se_R_BoneCrusher_Kidnapped se) =>
			{
				se.Networkoffset = Quaternion.Inverse(Quaternion.Euler(0f, directionAngle, 0f)) * (e.position - info.caster.position).normalized * UnityEngine.Random.Range(1.25f, 2f);
				if (se.offset.x < -1.3f)
				{
					se.offset.x = -1.3f;
				}
				if (se.offset.x > 1.3f)
				{
					se.offset.x = 1.3f;
				}
				if (se.offset.z < 0f)
				{
					se.offset.z *= -1f;
				}
				if (se.offset.z < 1f)
				{
					se.offset.z = 1f;
				}
			});
		}
		handle.Return();
	}

	private void Collide()
	{
		if (isActive)
		{
			CreateBasicEffect(info.caster, new SlowEffect
			{
				decay = true,
				strength = 80f
			}, 1f);
			CreateAbilityInstance<Ai_R_BoneCrusher_Explosion>(info.caster.agentPosition, Quaternion.Euler(0f, directionAngle, 0f), new CastInfo(victim));
			Destroy();
		}
	}

	protected override void ActiveFrameUpdate()
	{
		base.ActiveFrameUpdate();
		if (info.caster.Control.isLocalMovementProcessor)
		{
			float deltaTime = Time.deltaTime;
			float num = Mathf.Lerp(initSpeed, targetSpeed, (Time.time - creationTime) / (targetSpeed - initSpeed) * acceleration) * Mathf.Max(1f, info.caster.Status.movementSpeedMultiplier);
			info.caster.Control.SetAgentPosition(info.caster.agentPosition + Quaternion.Euler(0f, directionAngle, 0f) * Vector3.forward * (deltaTime * num));
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && _channel != null)
		{
			_channel.Cancel();
			_channel = null;
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
			NetworkWriterExtensions.WriteFloat(writer, directionAngle);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x1000L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, directionAngle);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref directionAngle, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x1000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref directionAngle, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
	}
}
