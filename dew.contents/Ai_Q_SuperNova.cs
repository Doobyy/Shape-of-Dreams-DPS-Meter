using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Ai_Q_SuperNova : AbilityInstance
{
	public float maxChannelTime;

	public float chargeFullTime;

	public float selfSlowAmount;

	public bool blockAttacksAndAbility;

	public bool continuousRotateOverride;

	public float chargeFullGraceThreshold;

	public DewAnimationClip chargeAnimation;

	public DewAnimationClip endAnimation;

	public GameObject endEffectOnCaster;

	public ScalingValue damage;

	public ScalingValue maxDamageMultiplier;

	public DewCollider range;

	public AnimationCurve scaleCurve;

	public AnimationCurve explodePitchCurve;

	public AnimationCurve explodeVolumeCurve;

	public AnimationCurve shakeMagnitudeCurve;

	public Transform[] scaledTransforms;

	public GameObject fullyChargedEffect;

	public GameObject explodeEffect;

	public GameObject critExplodeEffect;

	public GameObject explodeHitEffect;

	public FxCameraShake shake;

	private float _originalShake;

	[NonSerialized]
	[SyncVar]
	public float scaleMultiplier = 1f;

	[NonSerialized]
	public bool disableRecastAutoExplode;

	[SyncVar]
	private float _currentCharge;

	private St_Q_SuperNova _trigger;

	private Vector3[] _originalScales;

	private DewAudioSource[] _explodeAudioSources;

	private Channel _channel;

	private OnScreenTimerHandle _handle;

	private AbilityTrigger.ChangedConfigHandle _configHandle;

	private ActorRef<StatusEffect> _selfSlow;

	private ScalingValue _baseDamage;

	private Vector3[] _baseScaledLocalScales;

	private float _baseShakeAmplitude;

	public override bool reuseInRoom => true;

	public float NetworkscaleMultiplier
	{
		get
		{
			return scaleMultiplier;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref scaleMultiplier, 64uL, (Action<float, float>)null);
		}
	}

	public float Network_currentCharge
	{
		get
		{
			return _currentCharge;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref _currentCharge, 128uL, (Action<float, float>)null);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		_baseDamage = damage;
		_baseScaledLocalScales = new Vector3[scaledTransforms.Length];
		for (int i = 0; i < scaledTransforms.Length; i++)
		{
			_baseScaledLocalScales[i] = ((scaledTransforms[i] != null) ? scaledTransforms[i].localScale : Vector3.one);
		}
		if (shake != null)
		{
			_baseShakeAmplitude = shake.amplitude;
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		damage = _baseDamage;
		for (int i = 0; i < scaledTransforms.Length; i++)
		{
			if (scaledTransforms[i] != null)
			{
				scaledTransforms[i].localScale = _baseScaledLocalScales[i];
			}
		}
		if (shake != null)
		{
			shake.amplitude = _baseShakeAmplitude;
		}
		NetworkscaleMultiplier = 1f;
		disableRecastAutoExplode = false;
		Network_currentCharge = 0f;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		_trigger = (St_Q_SuperNova)firstTrigger;
		((Component)(object)this).transform.position = info.point;
		_originalScales = new Vector3[scaledTransforms.Length];
		_explodeAudioSources = ((explodeEffect != null) ? explodeEffect.GetComponentsInChildren<DewAudioSource>() : new DewAudioSource[0]);
		for (int i = 0; i < scaledTransforms.Length; i++)
		{
			if (!(scaledTransforms[i] == null))
			{
				_originalScales[i] = scaledTransforms[i].localScale * scaleMultiplier;
			}
		}
		if (shake != null)
		{
			_originalShake = shake.amplitude;
		}
		if (((NetworkBehaviour)info.caster).isOwned)
		{
			_handle = ShowOnScreenTimerLocally(new OnScreenTimerHandle
			{
				fillAmountGetter = () => _currentCharge
			});
		}
		if (info.caster is Hero hero && !hero.Skill.Movement.IsNullOrInactive())
		{
			hero.Skill.Movement.SetChargeAll(999);
		}
		if (((NetworkBehaviour)this).isServer && !disableRecastAutoExplode)
		{
			_configHandle = _trigger.ChangeConfigTimedOnce(1, maxChannelTime, OnUse, OnExpire, setFillAmount: false);
			if (selfSlowAmount > 0f)
			{
				_selfSlow = CreateBasicEffect(info.caster, new SlowEffect
				{
					strength = selfSlowAmount
				}, maxChannelTime, "supernova_selfslow");
			}
			if (blockAttacksAndAbility)
			{
				info.caster.Control.IncrementBlockCounters(Channel.BlockedAction.Ability | Channel.BlockedAction.Attack);
			}
		}
	}

	private void OnExpire()
	{
		Finish();
	}

	private void OnUse(EventInfoAbilityInstance obj)
	{
		if (info.caster is Hero hero && !hero.Skill.Movement.IsNullOrInactive())
		{
			hero.Skill.Movement.SetChargeAll(999);
		}
		Finish();
	}

	private void Finish()
	{
		if (!isActive)
		{
			return;
		}
		FxStopNetworked(fullyChargedEffect);
		FxPlayNetworked(explodeEffect);
		if (_currentCharge > chargeFullGraceThreshold)
		{
			Network_currentCharge = 1f;
		}
		if (_currentCharge > 0.99f)
		{
			FxPlayNetworked(critExplodeEffect);
		}
		ListReturnHandle<Entity> handle;
		foreach (Entity entity in range.GetEntities(out handle, tvDefaultHarmfulEffectTargets))
		{
			DamageData damageData = Damage(damage).ApplyRawMultiplier(Mathf.Lerp(1f, GetValue(maxDamageMultiplier), _currentCharge)).SetElemental(ElementalType.Light).SetOriginPosition(((Component)(object)this).transform.position);
			if (_currentCharge > 0.99f)
			{
				damageData.SetAttr(DamageAttribute.IsCrit);
			}
			damageData.Dispatch(entity);
			FxPlayNewNetworked(explodeHitEffect, entity);
		}
		handle.Return();
		if ((UnityEngine.Object)(object)info.caster != null && info.caster.isActive)
		{
			info.caster.Animation.StopAbilityAnimation(chargeAnimation);
			if (endAnimation != null)
			{
				info.caster.Animation.PlayAbilityAnimation(endAnimation);
			}
			FxPlayNetworked(endEffectOnCaster, info.caster);
		}
		Destroy();
	}

	protected override void OnDestroyActor()
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		base.OnDestroyActor();
		if (_handle != null)
		{
			HideOnScreenTimerLocally(_handle);
		}
		if (((NetworkBehaviour)this).isServer)
		{
			_configHandle?.Stop();
			_configHandle = null;
			if (!disableRecastAutoExplode && blockAttacksAndAbility)
			{
				info.caster.Control.DecrementBlockCounters(Channel.BlockedAction.Ability | Channel.BlockedAction.Attack);
			}
			if ((UnityEngine.Object)(object)_trigger != null)
			{
				_trigger.fillAmount = 0f;
			}
			if (!_selfSlow.IsNullOrInactive())
			{
				_selfSlow.Get().Destroy();
			}
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)_trigger != null)
		{
			float num = dt / chargeFullTime;
			num *= 1f + info.caster.Status.finalStats.abilityHaste * 0.01f;
			float num2 = Mathf.MoveTowards(_currentCharge, 1f, num);
			if (_currentCharge < 0.999f && num2 > 0.999f)
			{
				FxPlayNetworked(fullyChargedEffect);
			}
			Network_currentCharge = num2;
			_trigger.fillAmount = _currentCharge;
		}
		if (((NetworkBehaviour)this).isServer && disableRecastAutoExplode && _currentCharge > 0.99f)
		{
			Finish();
		}
		for (int i = 0; i < scaledTransforms.Length; i++)
		{
			Transform transform = scaledTransforms[i];
			if (!(transform == null))
			{
				transform.localScale = _originalScales[i] * scaleCurve.Evaluate(_currentCharge);
			}
		}
		DewAudioSource[] explodeAudioSources = _explodeAudioSources;
		foreach (DewAudioSource dewAudioSource in explodeAudioSources)
		{
			if (!(dewAudioSource == null))
			{
				dewAudioSource.pitchMultiplier = explodePitchCurve.Evaluate(_currentCharge);
				dewAudioSource.volumeMultiplier = explodeVolumeCurve.Evaluate(_currentCharge);
			}
		}
		if (shake != null)
		{
			shake.amplitude = shakeMagnitudeCurve.Evaluate(_currentCharge) * _originalShake;
		}
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		ListReturnHandle<Entity> handle;
		foreach (Entity entity in range.GetEntities(out handle, tvDefaultHarmfulEffectTargets))
		{
			if (!entity.Status.hasUnstoppable)
			{
				if (entity.Status.TryGetStatusEffect<Se_Q_SuperNova_Slow>(out var effect))
				{
					effect.ResetTimer();
				}
				else
				{
					CreateStatusEffect<Se_Q_SuperNova_Slow>(entity);
				}
			}
		}
		handle.Return();
		if (continuousRotateOverride && (UnityEngine.Object)(object)info.caster != null && info.caster.isActive)
		{
			info.caster.Control.RotateTowards(info.point, immediately: false, 0.5f);
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
			NetworkWriterExtensions.WriteFloat(writer, scaleMultiplier);
			NetworkWriterExtensions.WriteFloat(writer, _currentCharge);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x40L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, scaleMultiplier);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x80L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, _currentCharge);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref scaleMultiplier, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _currentCharge, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x40L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref scaleMultiplier, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
		if ((num & 0x80L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _currentCharge, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
	}
}
