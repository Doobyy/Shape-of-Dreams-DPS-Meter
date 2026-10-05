using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Ai_C_Pew : AbilityInstance
{
	public ChargingChannelData channel;

	public float widthMin;

	public float widthMax;

	public float fullGraceThreshold;

	public float refundMax = 0.8f;

	private ChargingChannel _channel;

	[SyncVar]
	private float _chargeAmount;

	public override bool reuseInRoom => true;

	public float Network_chargeAmount
	{
		get
		{
			return _chargeAmount;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref _chargeAmount, 64uL, (Action<float, float>)null);
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		Network_chargeAmount = 0f;
		_channel = null;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			_channel = channel.Get(this).SetInitialInfo(info).OnCast(OnCast)
				.OnCancel(OnCast)
				.OnComplete(OnCast)
				.Dispatch(info.caster, firstTrigger);
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer)
		{
			Network_chargeAmount = _channel.chargeAmount;
			_channel.castMethod._width = GetWidth();
			_channel.UpdateCastMethod();
		}
	}

	private void OnCast(ChargingChannel obj)
	{
		CreateAbilityInstance(info.caster.position, Quaternion.identity, new CastInfo(info.caster, obj.castInfo.angle), (Ai_C_Pew_Projectile p) =>
		{
			p.NetworkchargeAmount = obj.chargeAmount;
			if (p.chargeAmount > fullGraceThreshold)
			{
				p.NetworkchargeAmount = 1f;
			}
			p.collisionRadius = GetWidth() / 2f;
		});
		if ((UnityEngine.Object)(object)firstTrigger != null)
		{
			float chargeAmount = obj.chargeAmount;
			ApplyCooldownReductionByRatio(firstTrigger, Mathf.Clamp(1f - chargeAmount, 0f, refundMax));
		}
		Destroy();
	}

	private float GetWidth()
	{
		return Mathf.Lerp(widthMin, widthMax, _chargeAmount);
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteFloat(writer, _chargeAmount);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x40L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, _chargeAmount);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _chargeAmount, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x40L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _chargeAmount, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
	}
}
