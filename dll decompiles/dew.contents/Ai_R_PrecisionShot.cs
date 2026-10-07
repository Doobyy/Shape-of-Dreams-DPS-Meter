using System;
using System.Collections;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Ai_R_PrecisionShot : AbilityInstance
{
	public ChargingChannelData channel;

	public float lengthMin;

	public float lengthMax;

	public float cooldownRefundRatioOnCancel = 0.7f;

	public float fullGraceThreshold;

	public float cameraOffsetMultiplier = 1f;

	public int cameraZoomIndex;

	public float cameraRevertDelay = 0.1f;

	public float cameraRevertTime = 0.5f;

	public bool doUnstoppable;

	private ChargingChannel _channel;

	private CameraModifierOffset _offset;

	private CameraModifierZoom _zoom;

	private ActorRef<StatusEffect> _unstoppable;

	[SyncVar]
	private float _chargeAmount;

	[SyncVar]
	private float _angle;

	private float _baseChargeFullDuration;

	private float _baseWidth;

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

	protected override void Awake()
	{
		base.Awake();
		_baseChargeFullDuration = channel.chargeFullDuration;
		_baseWidth = channel.castMethod._width;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			_channel = channel.Get(this).SetInitialInfo(info).OnCast(OnCast)
				.OnCancel((ChargingChannel _) =>
				{
					ShootCanceled();
				})
				.OnComplete((ChargingChannel _) =>
				{
					ShootCanceled();
				})
				.Dispatch(info.caster, firstTrigger);
			if (doUnstoppable)
			{
				_unstoppable = CreateBasicEffect(info.caster, new UnstoppableEffect(), channel.completeDuration, "precision_unstoppable");
			}
		}
		if (((NetworkBehaviour)info.caster).isOwned)
		{
			_offset = new CameraModifierOffset
			{
				offset = Vector3.zero
			}.Apply();
			_zoom = new CameraModifierZoom
			{
				zoomIndex = cameraZoomIndex
			}.Apply();
		}
	}

	private void ShootCanceled()
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

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		CameraModifierOffset o;
		if (_offset != null)
		{
			o = _offset;
			_offset = null;
			ManagerBase<CameraManager>.instance.StartCoroutine(Routine());
		}
		CameraModifierZoom z;
		if (_zoom != null)
		{
			z = _zoom;
			_zoom = null;
			ManagerBase<CameraManager>.instance.StartCoroutine(Routine2());
		}
		if (((NetworkBehaviour)this).isServer)
		{
			if (!_unstoppable.IsNullOrInactive())
			{
				_unstoppable.Get().Destroy();
			}
			_unstoppable = null;
		}
		IEnumerator Routine()
		{
			yield return new WaitForSeconds(cameraRevertDelay);
			Vector3 startOffset = o.offset;
			for (float t = 0f; t < cameraRevertTime; t += Time.deltaTime)
			{
				o.offset = startOffset * ((cameraRevertTime - t) / cameraRevertTime);
				yield return null;
			}
			o.Remove();
		}
		IEnumerator Routine2()
		{
			yield return new WaitForSeconds(cameraRevertDelay);
			z.Remove();
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
		if (_offset != null && _angle != 0f)
		{
			_offset.offset = Quaternion.Euler(0f, _angle, 0f) * Vector3.forward * GetLength() * cameraOffsetMultiplier;
		}
	}

	private void OnCast(ChargingChannel obj)
	{
		CreateAbilityInstance(info.caster.position, Quaternion.identity, new CastInfo(info.caster, obj.castInfo.angle), (Ai_R_PrecisionShot_Projectile p) =>
		{
			p.NetworkchargeAmount = obj.chargeAmount;
			if (p.chargeAmount > fullGraceThreshold)
			{
				p.NetworkchargeAmount = 1f;
			}
			p.endDistance = GetLength();
		});
		Destroy();
	}

	private float GetLength()
	{
		return Mathf.Lerp(lengthMin, lengthMax, _chargeAmount);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		Network_chargeAmount = 0f;
		Network_angle = 0f;
		channel.chargeFullDuration = _baseChargeFullDuration;
		channel.castMethod._width = _baseWidth;
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
