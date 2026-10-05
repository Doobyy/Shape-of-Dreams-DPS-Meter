using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Se_MiniBoss_SpinningArrow : MiniBossEffect
{
	public float startDelay = 2f;

	public float angleStep = 13f;

	public float shootInterval = 0.2f;

	public bool disableOnDash;

	public float disableArrowsDuration = 0.5f;

	public GameObject fxAfterFirstShoot;

	public Transform angleTransform;

	private bool _didSetup;

	[SyncVar(hook = "OnAngleChanged")]
	private float _angle;

	private float _lastShootTime;

	private float _disableTime;

	public Action<float, float> _Mirror_SyncVarHookDelegate__angle;

	public override bool reuseInRoom => true;

	public float Network_angle
	{
		get
		{
			return _angle;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref _angle, 4096uL, _Mirror_SyncVarHookDelegate__angle);
		}
	}

	private void OnAngleChanged(float oldV, float newV)
	{
		angleTransform.rotation = Quaternion.Euler(0f, _angle, 0f);
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		OnAngleChanged(_angle, _angle);
		if (!((NetworkBehaviour)this).isServer)
		{
			victim.Control.ClientEvent_OnTeleport += new Action<Vector3, Vector3>(ClientEventOnTeleport);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (!((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)victim != null)
		{
			victim.Control.ClientEvent_OnTeleport -= new Action<Vector3, Vector3>(ClientEventOnTeleport);
		}
		FxStop(fxAfterFirstShoot);
	}

	private void ClientEventOnTeleport(Vector3 arg1, Vector3 arg2)
	{
		DisableArrows();
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!((NetworkBehaviour)this).isServer || Time.time - creationTime < startDelay)
		{
			return;
		}
		if (!_didSetup)
		{
			Network_angle = CastInfo.GetAngle(victim.position - Dew.GetClosestAliveHero(victim.position, fallbackToDead: true, victim).GetAIPosition(victim));
			_didSetup = true;
			FxPlayNetworked(fxAfterFirstShoot, victim);
		}
		if (!(Time.time - _lastShootTime < shootInterval))
		{
			Network_angle = _angle + angleStep;
			_lastShootTime = Time.time;
			if (victim.Visual.isSpawning || (disableOnDash && victim.Control.isDashing) || victim.Visual.isRendererOff)
			{
				DisableArrows();
			}
			else if (!(Time.time < _disableTime))
			{
				CreateAbilityInstance<Ai_MiniBoss_SpinningArrow_Arrow>(victim.position, null, new CastInfo(victim, _angle));
			}
		}
	}

	private void DisableArrows()
	{
		_disableTime = Time.time + disableArrowsDuration;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		_didSetup = false;
		Network_angle = 0f;
		_lastShootTime = 0f;
		_disableTime = 0f;
	}

	public Se_MiniBoss_SpinningArrow()
	{
		_Mirror_SyncVarHookDelegate__angle = OnAngleChanged;
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteFloat(writer, _angle);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x1000L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, _angle);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _angle, _Mirror_SyncVarHookDelegate__angle, NetworkReaderExtensions.ReadFloat(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x1000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _angle, _Mirror_SyncVarHookDelegate__angle, NetworkReaderExtensions.ReadFloat(reader));
		}
	}
}
