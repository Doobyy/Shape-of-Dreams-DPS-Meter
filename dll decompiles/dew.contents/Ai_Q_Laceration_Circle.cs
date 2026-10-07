using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class Ai_Q_Laceration_Circle : AbilityInstance
{
	public DewAnimationClip animPrepare;

	public float prepareDuration;

	public DewCollider slashRange;

	public GameObject fxSlash;

	public GameObject fxSlashHit;

	public DewAnimationClip animSlash;

	public float slashDuration;

	public ScalingValue slashDamage;

	public float cooldownReductionPerHit = 0.15f;

	public float cooldownReductionMax = 0.6f;

	public float selfSlowAmount = 50f;

	public GameObject prepareEffect;

	[NonSerialized]
	public float castDurationReduction;

	[NonSerialized]
	public bool disableInvul;

	[NonSerialized]
	[SyncVar]
	public bool useChargingMode;

	[NonSerialized]
	public ChargingChannelData chargingData;

	[NonSerialized]
	public float addedInvulnerableTimeRatio;

	[NonSerialized]
	public float maxRangeMultiplier = 1f;

	public GameObject chargingOnlyPrepareEffect;

	[SyncVar]
	private float _syncedFxScale = 1f;

	private ChargingChannel _chargeChannel;

	private ActorRef<StatusEffect> _invul;

	private ActorRef<StatusEffect> _uncol;

	private bool _slashed;

	private float _basePrepareDuration;

	private Vector3 _baseSlashRangeScale;

	private Vector3 _baseFxSlashScale;

	private Vector3 _baseChargingPrepareScale;

	private GameObject _baseStartEffect;

	public override bool reuseInRoom => true;

	public bool NetworkuseChargingMode
	{
		get
		{
			return useChargingMode;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref useChargingMode, 64uL, (Action<bool, bool>)null);
		}
	}

	public float Network_syncedFxScale
	{
		get
		{
			return _syncedFxScale;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref _syncedFxScale, 128uL, (Action<float, float>)null);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		_basePrepareDuration = prepareDuration;
		_baseStartEffect = startEffect;
		_baseSlashRangeScale = ((slashRange != null) ? slashRange.transform.localScale : Vector3.one);
		_baseFxSlashScale = ((fxSlash != null) ? fxSlash.transform.localScale : Vector3.one);
		_baseChargingPrepareScale = ((chargingOnlyPrepareEffect != null) ? chargingOnlyPrepareEffect.transform.localScale : Vector3.one);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		prepareDuration = _basePrepareDuration;
		selfSlowAmount = 50f;
		disableInvul = false;
		castDurationReduction = 0f;
		NetworkuseChargingMode = false;
		startEffect = _baseStartEffect;
		chargingData = null;
		addedInvulnerableTimeRatio = 0f;
		maxRangeMultiplier = 1f;
		Network_syncedFxScale = 1f;
		_chargeChannel = null;
		_invul = null;
		_uncol = null;
		_slashed = false;
		if (slashRange != null)
		{
			slashRange.transform.localScale = _baseSlashRangeScale;
		}
		if (fxSlash != null)
		{
			fxSlash.transform.localScale = _baseFxSlashScale;
		}
		if (chargingOnlyPrepareEffect != null)
		{
			chargingOnlyPrepareEffect.transform.localScale = _baseChargingPrepareScale;
		}
	}

	protected override void OnCreate()
	{
		((Component)(object)this).transform.rotation = ManagerBase<CameraManager>.instance.entityCamAngleRotation;
		base.OnCreate();
		GameObject effect = (useChargingMode ? chargingOnlyPrepareEffect : prepareEffect);
		FxPlay(effect, info.caster);
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		if (useChargingMode)
		{
			StartChargingMode();
			return;
		}
		float num = prepareDuration * (1f - castDurationReduction);
		info.caster.Animation.PlayAbilityAnimation(animPrepare);
		info.caster.Control.StartChannel(new Channel
		{
			duration = num,
			blockedActions = (Channel.BlockedAction.Ability | Channel.BlockedAction.Attack),
			onCancel = () =>
			{
				info.caster.Animation.StopAbilityAnimation(animPrepare);
				Destroy();
			},
			onComplete = Slash
		});
		if (!disableInvul)
		{
			CreateBasicEffect(info.caster, new InvulnerableEffect(), num);
			CreateBasicEffect(info.caster, new UncollidableEffect(), num);
		}
		CreateBasicEffect(info.caster, new SpeedEffect
		{
			decay = true,
			strength = 100f
		}, num);
		info.caster.Control.Rotate(info.caster.rotation, immediately: true, num + 1f);
	}

	private void StartChargingMode()
	{
		float num = chargingData.chargeFullDuration * (1f - castDurationReduction);
		float duration = prepareDuration * (1f + addedInvulnerableTimeRatio);
		chargingData.chargeFullDuration = num;
		info.caster.Animation.PlayAbilityAnimation(animPrepare);
		if (!disableInvul)
		{
			_invul = CreateBasicEffect(info.caster, new InvulnerableEffect(), duration);
			_uncol = CreateBasicEffect(info.caster, new UncollidableEffect(), duration);
		}
		CreateBasicEffect(info.caster, new SpeedEffect
		{
			decay = true,
			strength = 100f
		}, num);
		info.caster.Control.Rotate(info.caster.rotation, immediately: true, num + 1f);
		_chargeChannel = chargingData.Get(this).OnComplete(DoChargedSlash).OnCast(DoChargedSlash)
			.OnCancel(AbortCharge)
			.Dispatch(info.caster, firstTrigger);
	}

	private void DoChargedSlash(ChargingChannel c)
	{
		if (!_slashed)
		{
			_slashed = true;
			EndInvulEarly();
			Slash();
		}
	}

	private void AbortCharge(ChargingChannel c)
	{
		EndInvulEarly();
		info.caster.Animation.StopAbilityAnimation(animPrepare);
		Destroy();
	}

	private void EndInvulEarly()
	{
		if (!_invul.IsNullOrInactive())
		{
			_invul.Get().Destroy();
		}
		if (!_uncol.IsNullOrInactive())
		{
			_uncol.Get().Destroy();
		}
		_invul = null;
		_uncol = null;
	}

	public override void FrameUpdate()
	{
		base.FrameUpdate();
		if (!((UnityEngine.Object)(object)info.caster == null))
		{
			((Component)(object)this).transform.position = info.caster.position;
			if (chargingOnlyPrepareEffect != null && !Mathf.Approximately(_syncedFxScale, 1f))
			{
				chargingOnlyPrepareEffect.transform.localScale = Vector3.one * _syncedFxScale;
			}
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer && _chargeChannel != null)
		{
			Network_syncedFxScale = Mathf.Lerp(1f, maxRangeMultiplier, _chargeChannel.chargeAmount);
		}
	}

	[ClientRpc]
	private void RpcSlashScale()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendRPCInternal("System.Void Ai_Q_Laceration_Circle::RpcSlashScale()", -2031538849, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	private void Slash()
	{
		RpcSlashScale();
		slashRange.transform.localScale *= _syncedFxScale;
		info.caster.Animation.PlayAbilityAnimation(animSlash);
		info.caster.Control.StartChannel(new Channel
		{
			duration = slashDuration,
			blockedActions = Channel.BlockedAction.Attack
		});
		FxPlayNetworked(fxSlash, info.caster);
		List<Entity> entities = slashRange.GetEntities(out var handle, tvDefaultHarmfulEffectTargets);
		foreach (Entity item in entities)
		{
			Damage(slashDamage).SetElemental(ElementalType.Dark).SetOriginPosition(position).DoAttackEffect(AttackEffectType.Others)
				.Dispatch(item);
			FxPlayNewNetworked(fxSlashHit, item);
		}
		if (entities.Count > 0 && (bool)(UnityEngine.Object)(object)firstTrigger)
		{
			float ratio = Mathf.Min(cooldownReductionPerHit * (float)entities.Count, cooldownReductionMax);
			ApplyCooldownReductionByRatio(firstTrigger, ratio);
		}
		handle.Return();
		CreateBasicEffect(info.caster, new SpeedEffect
		{
			decay = true,
			strength = 0f - selfSlowAmount
		}, 0.5f);
		Destroy();
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			FxStopNetworked(prepareEffect);
			FxStopNetworked(chargingOnlyPrepareEffect);
		}
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcSlashScale()
	{
		fxSlash.transform.localScale *= _syncedFxScale;
		FxStop(prepareEffect);
		FxStop(chargingOnlyPrepareEffect);
	}

	protected static void InvokeUserCode_RpcSlashScale(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSlashScale called on server.");
		}
		else
		{
			((Ai_Q_Laceration_Circle)(object)obj).UserCode_RpcSlashScale();
		}
	}

	static Ai_Q_Laceration_Circle()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterRpc(typeof(Ai_Q_Laceration_Circle), "System.Void Ai_Q_Laceration_Circle::RpcSlashScale()", (RemoteCallDelegate)InvokeUserCode_RpcSlashScale);
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteBool(writer, useChargingMode);
			NetworkWriterExtensions.WriteFloat(writer, _syncedFxScale);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x40L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, useChargingMode);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x80L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, _syncedFxScale);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref useChargingMode, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _syncedFxScale, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x40L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref useChargingMode, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
		}
		if ((num & 0x80L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _syncedFxScale, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
	}
}
