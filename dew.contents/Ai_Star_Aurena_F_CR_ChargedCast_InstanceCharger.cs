using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Ai_Star_Aurena_F_CR_ChargedCast_InstanceCharger : AbilityInstance
{
	public GameObject fxChargeEnd;

	public ChargingChannelData channelData;

	public float maxRangeMultiplier = 0.25f;

	public float maxEffectMultiplier = 1f;

	[SyncVar]
	private float _currentCharge;

	private ChargingChannel _channel;

	private float _originalRadius;

	private OnScreenTimerHandle _handle;

	public float maxChargeTime => channelData.chargeFullDuration;

	public float Network_currentCharge
	{
		get
		{
			return _currentCharge;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref _currentCharge, 64uL, (Action<float, float>)null);
		}
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)info.caster).isOwned)
		{
			_handle = ShowOnScreenTimerLocally(new OnScreenTimerHandle
			{
				fillAmountGetter = () => _currentCharge
			});
		}
		if (((NetworkBehaviour)this).isServer)
		{
			_channel = channelData.Get(this).OnCancel(DoCharge).OnCast(DoCharge)
				.OnComplete(DoCharge);
			_originalRadius = channelData.castMethod._radius;
			_channel.Dispatch(info.caster, firstTrigger);
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer || _channel != null)
		{
			Network_currentCharge = _channel.chargeAmount;
			_channel.castMethod._radius = _originalRadius * Mathf.Lerp(1f, 1f + maxRangeMultiplier, _currentCharge);
			_channel.UpdateCastMethod();
		}
	}

	protected override void ActiveFrameUpdate()
	{
		base.ActiveFrameUpdate();
		if (((NetworkBehaviour)this).isServer)
		{
			position = info.caster.position;
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (_handle != null)
		{
			HideOnScreenTimerLocally(_handle);
			_handle = null;
		}
	}

	private void DoCharge(ChargingChannel c)
	{
		float num = c.chargeAmount;
		if (num > 0.9f)
		{
			num = 1f;
		}
		float rangeMultiplier = Mathf.Lerp(1f, 1f + maxRangeMultiplier, num);
		float effectMultiplier = Mathf.Lerp(1f, 1f + maxEffectMultiplier, num);
		FxPlayNetworked(fxChargeEnd, info.caster);
		CreateAbilityInstance(info.caster.position, null, new CastInfo(info.caster, info.caster.position), (Ai_R_ChainReaction ai) =>
		{
			ai.NetworkchargedRangeMultiplier = rangeMultiplier;
			ai.chargedEffectMultiplier = effectMultiplier;
		});
		Destroy();
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteFloat(writer, _currentCharge);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x40L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, _currentCharge);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _currentCharge, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x40L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _currentCharge, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
	}
}
