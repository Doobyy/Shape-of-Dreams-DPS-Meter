using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class Ai_R_StaticDischarge : AbilityInstance, ACH_THEYRE_JUST_BIG_CATS.ILightingActor
{
	public DewAnimationClip startAnim;

	public DewAnimationClip dischargeAnim;

	public ScalingValue armorAmount;

	public float armorLingerDuration = 1f;

	public ChargingChannelData channel;

	public ScalingValue minDamage;

	public ScalingValue maxDamage;

	public DewCollider range;

	public float slowAmount;

	public float slowDuration;

	public GameObject chargeEffect;

	public GameObject explodeEffect;

	public GameObject hitEffect;

	public float procCoefficient;

	public DewAudioSource[] adjustedAudios;

	public Transform[] adjustedTransforms;

	public AnimationCurve pitchMultiplier;

	public AnimationCurve volumeMultiplier;

	public AnimationCurve scaleMultiplier;

	public FxCameraShake shake;

	public AnimationCurve shakeMultiplier;

	public float endDaze;

	public bool doKnockback;

	public Knockback knockback;

	public Vector2 knockbackDist;

	public float knockbackDistFarThreshold;

	public AnimationCurve knockbackMultiplier;

	public FxPointLight explodeLight;

	public AnimationCurve intensityMultiplier;

	public GameObject fxArmorEffect;

	[SyncVar]
	private float _currentCharge;

	private ChargingChannel _channel;

	private OnScreenTimerHandle _handle;

	private float _originalRadius;

	private ActorRef<StatusEffect> _armorEffect;

	private Vector3 _baseRangeScale;

	private float _baseShakeAmplitude;

	private float _baseExplodeLightIntensityMul;

	private Vector3[] _baseAdjustedScales;

	private float[] _baseAudioPitchMul;

	private float[] _baseAudioVolumeMul;

	private int _generation;

	public override bool reuseInRoom => true;

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

	protected override void Awake()
	{
		base.Awake();
		if (range != null)
		{
			_baseRangeScale = range.transform.localScale;
		}
		if (shake != null)
		{
			_baseShakeAmplitude = shake.amplitude;
		}
		if (explodeLight != null)
		{
			_baseExplodeLightIntensityMul = explodeLight.intensityMultiplier;
		}
		if (adjustedTransforms != null)
		{
			_baseAdjustedScales = new Vector3[adjustedTransforms.Length];
			for (int i = 0; i < adjustedTransforms.Length; i++)
			{
				if (adjustedTransforms[i] != null)
				{
					_baseAdjustedScales[i] = adjustedTransforms[i].localScale;
				}
			}
		}
		if (adjustedAudios == null)
		{
			return;
		}
		_baseAudioPitchMul = new float[adjustedAudios.Length];
		_baseAudioVolumeMul = new float[adjustedAudios.Length];
		for (int j = 0; j < adjustedAudios.Length; j++)
		{
			if (adjustedAudios[j] != null)
			{
				_baseAudioPitchMul[j] = adjustedAudios[j].pitchMultiplier;
				_baseAudioVolumeMul[j] = adjustedAudios[j].volumeMultiplier;
			}
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		_generation++;
		Network_currentCharge = 0f;
		_originalRadius = 0f;
		if (range != null)
		{
			range.transform.localScale = _baseRangeScale;
		}
		if (shake != null)
		{
			shake.amplitude = _baseShakeAmplitude;
		}
		if (explodeLight != null)
		{
			explodeLight.intensityMultiplier = _baseExplodeLightIntensityMul;
		}
		if (adjustedTransforms != null && _baseAdjustedScales != null)
		{
			for (int i = 0; i < adjustedTransforms.Length; i++)
			{
				if (adjustedTransforms[i] != null)
				{
					adjustedTransforms[i].localScale = _baseAdjustedScales[i];
				}
			}
		}
		if (adjustedAudios == null || _baseAudioPitchMul == null)
		{
			return;
		}
		for (int j = 0; j < adjustedAudios.Length; j++)
		{
			if (adjustedAudios[j] != null)
			{
				adjustedAudios[j].pitchMultiplier = _baseAudioPitchMul[j];
				adjustedAudios[j].volumeMultiplier = _baseAudioVolumeMul[j];
			}
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
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		info.caster.Animation.PlayAbilityAnimation(startAnim);
		FxPlayNetworked(chargeEffect, info.caster);
		_channel = channel.Get(this).OnCancel(DoDischarge).OnCast(DoDischarge)
			.OnComplete(DoDischarge);
		_originalRadius = _channel.castMethod._radius;
		_channel.castMethod._radius *= scaleMultiplier.Evaluate(0f);
		_channel.Dispatch(info.caster, firstTrigger);
		Se_GenericEffectContainer se_GenericEffectContainer = CreateBasicEffect(info.caster, new ArmorBoostEffect
		{
			strength = GetValue(armorAmount)
		}, channel.completeDuration);
		_armorEffect = se_GenericEffectContainer;
		FxPlayNetworked(fxArmorEffect, info.caster);
		int gen = _generation;
		se_GenericEffectContainer.ClientActorEvent_OnDestroyed += (Action<Actor>)((Actor _) =>
		{
			if (!((UnityEngine.Object)(object)this == null) && gen == _generation)
			{
				FxStopNetworked(fxArmorEffect);
			}
		});
	}

	protected override void ActiveFrameUpdate()
	{
		base.ActiveFrameUpdate();
		if (!((UnityEngine.Object)(object)info.caster == null))
		{
			((Component)(object)this).transform.position = info.caster.position;
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer && _channel != null)
		{
			Network_currentCharge = _channel.chargeAmount;
			_channel.castMethod._radius = _originalRadius * scaleMultiplier.Evaluate(_currentCharge);
			_channel.UpdateCastMethod();
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (_handle != null)
		{
			HideOnScreenTimerLocally(_handle);
		}
		if (((NetworkBehaviour)this).isServer)
		{
			FxStopNetworked(chargeEffect);
			if (!_armorEffect.IsNullOrInactive())
			{
				_armorEffect.Get().SetTimer(armorLingerDuration);
			}
			_armorEffect = null;
		}
	}

	private void DoDischarge(ChargingChannel c)
	{
		Vector3 vector = info.caster.position;
		float num = c.chargeAmount;
		if ((double)num > 0.9)
		{
			num = 1f;
		}
		RpcPlayExplodeEffect(vector, num);
		range.transform.localScale *= scaleMultiplier.Evaluate(num);
		ScalingValue value = ScalingValue.Lerp(minDamage, maxDamage, num);
		List<Entity> entities = range.GetEntities(out var handle, tvDefaultHarmfulEffectTargets);
		float num2 = knockbackMultiplier.Evaluate(num);
		foreach (Entity item in entities)
		{
			FxPlayNewNetworked(hitEffect, item);
			Damage(value, procCoefficient).SetOriginPosition(vector).SetElemental(ElementalType.Light).Dispatch(item);
			if (slowAmount > 0f)
			{
				CreateBasicEffect(item, new SlowEffect
				{
					strength = slowAmount
				}, slowDuration, "staticdischarge_slow");
			}
			if (doKnockback)
			{
				float t = Mathf.Clamp01(Vector2.Distance(vector.ToXY(), item.agentPosition.ToXY()) / knockbackDistFarThreshold);
				knockback.distance = Mathf.Lerp(knockbackDist.x, knockbackDist.y, t);
				knockback.distance *= num2;
				knockback.ApplyWithOrigin(vector, item);
			}
		}
		handle.Return();
		info.caster.Animation.PlayAbilityAnimation(dischargeAnim);
		if (endDaze > 0f)
		{
			info.caster.Control.StartDaze(endDaze);
		}
		Destroy();
	}

	[ClientRpc]
	private void RpcPlayExplodeEffect(Vector3 pos, float strength)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteVector3((NetworkWriter)(object)val, pos);
		NetworkWriterExtensions.WriteFloat((NetworkWriter)(object)val, strength);
		((NetworkBehaviour)this).SendRPCInternal("System.Void Ai_R_StaticDischarge::RpcPlayExplodeEffect(UnityEngine.Vector3,System.Single)", -1594235088, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcPlayExplodeEffect__Vector3__Single(Vector3 pos, float strength)
	{
		if (shake != null)
		{
			shake.amplitude *= shakeMultiplier.Evaluate(strength);
		}
		float num = scaleMultiplier.Evaluate(strength);
		Transform[] array = adjustedTransforms;
		foreach (Transform transform in array)
		{
			if (!(transform == null))
			{
				transform.localScale *= num;
			}
		}
		float num2 = pitchMultiplier.Evaluate(strength);
		float num3 = volumeMultiplier.Evaluate(strength);
		DewAudioSource[] array2 = adjustedAudios;
		foreach (DewAudioSource dewAudioSource in array2)
		{
			if (!(dewAudioSource == null))
			{
				dewAudioSource.pitchMultiplier *= num2;
				dewAudioSource.volumeMultiplier *= num3;
			}
		}
		if (explodeLight != null)
		{
			explodeLight.intensityMultiplier *= intensityMultiplier.Evaluate(strength);
		}
		FxPlay(explodeEffect, pos, Quaternion.identity);
	}

	protected static void InvokeUserCode_RpcPlayExplodeEffect__Vector3__Single(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlayExplodeEffect called on server.");
		}
		else
		{
			((Ai_R_StaticDischarge)(object)obj).UserCode_RpcPlayExplodeEffect__Vector3__Single(NetworkReaderExtensions.ReadVector3(reader), NetworkReaderExtensions.ReadFloat(reader));
		}
	}

	static Ai_R_StaticDischarge()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterRpc(typeof(Ai_R_StaticDischarge), "System.Void Ai_R_StaticDischarge::RpcPlayExplodeEffect(UnityEngine.Vector3,System.Single)", (RemoteCallDelegate)InvokeUserCode_RpcPlayExplodeEffect__Vector3__Single);
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
