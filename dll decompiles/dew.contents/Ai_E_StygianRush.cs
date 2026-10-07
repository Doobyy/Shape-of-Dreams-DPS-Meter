using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Ai_E_StygianRush : AbilityInstance
{
	public ChargingChannelData channel;

	public ScalingValue minDamage;

	public ScalingValue maxDamage;

	public float lengthMin;

	public float lengthMax;

	public float cooldownRefundRatioOnCancel = 0.7f;

	public float fullGraceThreshold;

	public bool doUnstoppable;

	public AnimationCurve shakeMultiplier;

	public AnimationCurve pitchMultiplier;

	public AnimationCurve volumeMultiplier;

	private ChargingChannel _channel;

	private ActorRef<StatusEffect> _unstoppable;

	[SyncVar]
	private float _chargeAmount;

	[SyncVar]
	private float _angle;

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

	public float Network_angle
	{
		get
		{
			return _angle;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref _angle, 128uL, (Action<float, float>)null);
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		Network_chargeAmount = 0f;
		Network_angle = 0f;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			_channel = channel.Get(this).SetInitialInfo(info).OnCast(DoRush)
				.OnCancel((ChargingChannel _) =>
				{
					OnCanceled();
				})
				.OnComplete((ChargingChannel _) =>
				{
					OnCanceled();
				})
				.Dispatch(info.caster, firstTrigger);
			if (doUnstoppable)
			{
				_unstoppable = CreateBasicEffect(info.caster, new UnstoppableEffect(), channel.completeDuration, "impactrush_unstoppable");
			}
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			if (!_unstoppable.IsNullOrInactive())
			{
				_unstoppable.Get().Destroy();
			}
			_unstoppable = null;
		}
	}

	private void OnCanceled()
	{
		if (isActive)
		{
			Destroy();
		}
		if ((UnityEngine.Object)(object)firstTrigger != null)
		{
			ApplyCooldownReductionByRatio(firstTrigger, cooldownRefundRatioOnCancel);
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer)
		{
			Network_chargeAmount = _channel.chargeAmount;
			Network_angle = _channel.castInfo.angle;
			_channel.castMethod._length = GetLength();
			_channel.UpdateCastMethod();
		}
	}

	private void DoRush(ChargingChannel obj)
	{
		float chargeAmount = obj.chargeAmount;
		float damage = Mathf.Lerp(GetValue(minDamage), GetValue(maxDamage), chargeAmount);
		float shakeMult = shakeMultiplier.Evaluate(chargeAmount);
		float pitchMult = pitchMultiplier.Evaluate(chargeAmount);
		float volumeMult = volumeMultiplier.Evaluate(chargeAmount);
		CreateAbilityInstance(info.caster.position, null, new CastInfo(info.caster, obj.castInfo.angle), (Ai_E_StygianRush_Rush b) =>
		{
			b.NetworkvolumeMultiplier = volumeMult;
			b.NetworkpitchMultiplier = pitchMult;
			b.Networkdamage = damage;
			b.NetworkchargeAmount = obj.chargeAmount;
			b.NetworkshakeMultiplier = shakeMult;
			if (b.chargeAmount > fullGraceThreshold)
			{
				b.NetworkchargeAmount = 1f;
			}
			b.NetworkmaxDistance = GetLength();
		});
		Destroy();
	}

	private float GetLength()
	{
		return Mathf.Lerp(lengthMin, lengthMax, _chargeAmount);
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
			NetworkWriterExtensions.WriteFloat(writer, _angle);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x40L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, _chargeAmount);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x80L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, _angle);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _chargeAmount, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _angle, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x40L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _chargeAmount, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
		if ((num & 0x80L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _angle, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
	}
}
