using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Ai_Q_BigBorealChunk_Spawner : AbilityInstance
{
	[Serializable]
	public class ChargeEffectItem
	{
		public float time;

		public GameObject effect;
	}

	public ChargingChannelData channel;

	public Transform scaledChunkTransform;

	public Transform chunkCenter;

	public float damageAmpPerSecond = 0.25f;

	public AnimationCurve chunkVisualScaleCurve;

	public AnimationCurve explodeRadiusCurve;

	public float maxChannelTime = 4f;

	public float channelSpeedMultiplier = 1f;

	public ScalingValue armorAmount;

	public ChargeEffectItem[] chargeEffects;

	private ChargingChannel _channel;

	[SyncVar]
	private float _channelDuration;

	[NonSerialized]
	private float _baseMaxChannelTime;

	[NonSerialized]
	private float _baseChannelSpeedMultiplier;

	[NonSerialized]
	private float[] _baseChargeEffectTimes;

	[NonSerialized]
	private Vector3 _baseChunkScale;

	public override bool reuseInRoom => true;

	public float Network_channelDuration
	{
		get
		{
			return _channelDuration;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref _channelDuration, 64uL, (Action<float, float>)null);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		_baseMaxChannelTime = maxChannelTime;
		_baseChannelSpeedMultiplier = channelSpeedMultiplier;
		_baseChargeEffectTimes = new float[chargeEffects.Length];
		for (int i = 0; i < chargeEffects.Length; i++)
		{
			_baseChargeEffectTimes[i] = chargeEffects[i].time;
		}
		_baseChunkScale = scaledChunkTransform.localScale;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		maxChannelTime = _baseMaxChannelTime;
		channelSpeedMultiplier = _baseChannelSpeedMultiplier;
		for (int i = 0; i < chargeEffects.Length; i++)
		{
			chargeEffects[i].time = _baseChargeEffectTimes[i];
		}
		Network_channelDuration = 0f;
		scaledChunkTransform.localScale = _baseChunkScale;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		channel.chargeFullDuration = maxChannelTime / channelSpeedMultiplier;
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		_channel = channel.Get(this).SetInitialInfo(info).OnCast((ChargingChannel _) =>
		{
			Complete();
		})
			.OnCancel((ChargingChannel _) =>
			{
				Complete();
			})
			.OnComplete((ChargingChannel _) =>
			{
				Complete();
			})
			.Dispatch(info.caster, firstTrigger);
		CreateBasicEffect(info.caster, new UnstoppableEffect(), channel.completeDuration).DestroyOnDestroy(this);
		CreateBasicEffect(info.caster, new ArmorBoostEffect
		{
			strength = GetValue(armorAmount)
		}, channel.completeDuration).DestroyOnDestroy(this);
		for (int num = 0; num < chargeEffects.Length; num++)
		{
			if (chargeEffects[num].time < 0f)
			{
				chargeEffects[num].time = channel.chargeFullDuration * channelSpeedMultiplier - 0.01f;
			}
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer)
		{
			float channelDuration = _channelDuration;
			Network_channelDuration = Mathf.Min(_channel.elapsedTime * channelSpeedMultiplier, channel.chargeFullDuration * channelSpeedMultiplier);
			_channel.castMethod._radius = explodeRadiusCurve.Evaluate(_channelDuration);
			_channel.UpdateCastMethod();
			for (int i = 0; i < chargeEffects.Length; i++)
			{
				if (chargeEffects[i].time >= channelDuration && chargeEffects[i].time < _channelDuration)
				{
					FxPlayNetworked(chargeEffects[i].effect, info.caster);
				}
			}
		}
		scaledChunkTransform.localScale = Vector3.one * chunkVisualScaleCurve.Evaluate(_channelDuration);
	}

	private void Complete()
	{
		CreateAbilityInstance(info.caster.position, _channel.castInfo.rotation, new CastInfo(info.caster, _channel.castInfo.point), (Ai_Q_BigBorealChunk_Projectile p) =>
		{
			p.SetCustomStartPosition(chunkCenter.position);
			p.NetworkchunkVisualScale = chunkVisualScaleCurve.Evaluate(_channelDuration);
			p.NetworkchargeDuration = _channelDuration;
			p.NetworkexplodeRadius = explodeRadiusCurve.Evaluate(_channelDuration);
			p.NetworkdamageAmp = _channelDuration * damageAmpPerSecond;
		});
		if (_channelDuration > 1f)
		{
			info.caster.Control.StartDaze(0.2f);
		}
		info.caster.Control.Rotate(_channel.castInfo.rotation, immediately: true, 1f);
		Destroy();
	}

	public float GetCurrentAmp()
	{
		return _channelDuration * damageAmpPerSecond;
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteFloat(writer, _channelDuration);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x40L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, _channelDuration);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _channelDuration, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x40L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _channelDuration, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
	}
}
