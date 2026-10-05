using System;
using System.Collections;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Ai_E_ShadowVolley : AbilityInstance
{
	public ChargingChannelData channel;

	public ScalingValue baseShootCount;

	public ScalingValue baseDuration;

	private ChargingChannel _channel;

	private int _totalShootCount;

	private int _shotCount;

	private float _lastShootTime;

	private OnScreenTimerHandle _timer;

	[SyncVar]
	private float _duration = 10f;

	public override bool reuseInRoom => true;

	public float Network_duration
	{
		get
		{
			return _duration;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref _duration, 64uL, (Action<float, float>)null);
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		_shotCount = 0;
		_totalShootCount = 0;
		Network_duration = 10f;
		_lastShootTime = 0f;
	}

	protected override IEnumerator OnCreateSequenced()
	{
		if (((NetworkBehaviour)info.caster.owner).isLocalPlayer)
		{
			_timer = ShowOnScreenTimerLocally(new OnScreenTimerHandle
			{
				fillAmountGetter = () => (Time.time - creationTime) / _duration
			});
		}
		if (((NetworkBehaviour)this).isServer)
		{
			yield return null;
			_channel = channel.Get(this).SetInitialInfo(info).OnCancel((ChargingChannel _) =>
			{
				DestroyIfActive();
			})
				.Dispatch(info.caster, firstTrigger);
			_totalShootCount = Mathf.RoundToInt(GetValue(baseShootCount) * (1f + info.caster.Status.critChance));
			Network_duration = GetValue(baseDuration) / Mathf.Max(1f, info.caster.Status.attackSpeedMultiplier);
			_lastShootTime = Time.time - _duration / (float)_totalShootCount;
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!((NetworkBehaviour)this).isServer || _totalShootCount == 0)
		{
			return;
		}
		if (_shotCount >= _totalShootCount)
		{
			Destroy();
			return;
		}
		float num = _duration / (float)_totalShootCount;
		if (Time.time - _lastShootTime > num)
		{
			_lastShootTime += num;
			CreateAbilityInstance<Ai_E_ShadowVolley_Projectile>(info.caster.agentPosition, null, new CastInfo(info.caster, _channel.castInfo.angle));
			_shotCount++;
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (_timer != null)
		{
			HideOnScreenTimerLocally(_timer);
			_timer = null;
		}
		if (((NetworkBehaviour)this).isServer && _channel != null && _channel.isActive)
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
			NetworkWriterExtensions.WriteFloat(writer, _duration);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x40L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, _duration);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _duration, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x40L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _duration, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
	}
}
