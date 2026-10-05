using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Ai_Q_CruelSun : AbilityInstance
{
	public ChargingChannelData channel;

	public float fullChargeLengthMult;

	public float fullChargeWidthMult;

	public float fullGraceThreshold;

	[NonSerialized]
	public bool disableArmor;

	[NonSerialized]
	public bool enableMovement;

	[NonSerialized]
	[SyncVar]
	public bool doColdDamage;

	[NonSerialized]
	public float stunDurationOffset;

	[NonSerialized]
	public float buffLingerDuration;

	private ChargingChannel _channel;

	private float _initLength;

	private float _initWidth;

	private float _baseChargeFullDuration;

	private DewEffect.FxColorSnapshot _colorSnapshot;

	private bool _colorsAreCold;

	public override bool reuseInRoom => true;

	public bool NetworkdoColdDamage
	{
		get
		{
			return doColdDamage;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref doColdDamage, 64uL, (Action<bool, bool>)null);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		_baseChargeFullDuration = channel.chargeFullDuration;
		_colorSnapshot = DewEffect.CaptureColorsRecursively(((Component)(object)this).gameObject);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (_colorsAreCold)
		{
			DewEffect.RestoreColorsRecursively(_colorSnapshot);
			_colorsAreCold = false;
		}
		stunDurationOffset = 0f;
		NetworkdoColdDamage = false;
		disableArmor = false;
		enableMovement = false;
		buffLingerDuration = 0f;
	}

	protected override void OnCreate()
	{
		if (doColdDamage)
		{
			DewEffect.ChangeColorRecursively(((Component)(object)this).gameObject, 0.55f, 0.7f, 0.7f);
			_colorsAreCold = true;
		}
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			if (info.caster.Status.TryGetStatusEffect<Se_Q_CruelSun_Armored>(out var effect))
			{
				effect.Destroy();
			}
			channel.chargeFullDuration = _baseChargeFullDuration / Mathf.Max(1f, info.caster.Status.attackSpeedMultiplier);
			channel.canMove = enableMovement;
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
			_initLength = channel.castMethod._length;
			_initWidth = channel.castMethod._width;
			CreateStatusEffect(info.caster, new CastInfo(info.caster), (Se_Q_CruelSun_Armored se) =>
			{
				se.NetworkdoColdDamage = doColdDamage;
				se.disableArmor = disableArmor;
			});
			ResetCooldown(info.caster.Ability.attackAbility);
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer)
		{
			_channel.castMethod._length = _initLength * Mathf.Lerp(1f, fullChargeLengthMult, _channel.chargeAmount);
			_channel.castMethod._width = _initWidth * Mathf.Lerp(1f, fullChargeWidthMult, _channel.chargeAmount);
			_channel.UpdateCastMethod();
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)info.caster != null && info.caster.Status.TryGetStatusEffect<Se_Q_CruelSun_Armored>(out var effect))
		{
			if (buffLingerDuration > 0f)
			{
				effect.SetTimer(buffLingerDuration);
			}
			else
			{
				effect.Destroy();
			}
		}
	}

	private void Complete()
	{
		CreateAbilityInstance(info.caster.position, _channel.castInfo.rotation, new CastInfo(info.caster, _channel.castInfo.angle), (Ai_Q_CruelSun_Shockwave p) =>
		{
			float num = _channel.chargeAmount;
			if (num > fullGraceThreshold)
			{
				num = 1f;
			}
			p.Network_length = _initLength * Mathf.Lerp(1f, fullChargeLengthMult, num);
			p.Network_width = _initWidth * Mathf.Lerp(1f, fullChargeWidthMult, num);
			p.Network_chargeAmount = num;
			p.NetworkdoColdDamage = doColdDamage;
			p.stunDurationMin += stunDurationOffset;
			p.stunDurationMax += stunDurationOffset;
		});
		info.caster.Control.Rotate(_channel.castInfo.rotation, immediately: true, 1f);
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
			NetworkWriterExtensions.WriteBool(writer, doColdDamage);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x40L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, doColdDamage);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref doColdDamage, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x40L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref doColdDamage, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
		}
	}
}
