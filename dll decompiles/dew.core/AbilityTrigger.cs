using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;
using UnityEngine.AI;

[LogicUpdatePriority(-300)]
public class AbilityTrigger : Actor
{
	public class ChangedConfigHandle
	{
		internal bool _isActive;

		internal Action<EventInfoAbilityInstance> _onUse;

		internal Action _onExpire;

		internal AbilityTrigger _trigger;

		public bool isActive
		{
			get
			{
				if (_isActive && (UnityEngine.Object)(object)_trigger != null)
				{
					return _trigger.currentConfigChangeHandle == this;
				}
				return false;
			}
		}

		public float remainingTime { get; internal set; }

		public float duration { get; internal set; }

		public int configIndex { get; internal set; }

		public int previousConfigIndex { get; internal set; }

		public bool setFillAmount { get; internal set; }

		public void Stop()
		{
			if (!isActive)
			{
				Debug.LogWarning("Tried to stop inactive ChangedConfigHandle.");
			}
			else
			{
				_trigger.RemoveCurrentConfigChange(isExpiration: false);
			}
		}
	}

	private struct ConfigSyncData
	{
		public float manaCost;

		public int maxCharges;

		public int addedCharges;

		public float cooldownTime;

		public float minimumDelay;
	}

	private struct CurrentAbilityInstanceData
	{
		public AbilityInstance instance;

		public int configIndex;

		public bool shouldLockCast;

		public bool shouldLockCooldown;

		public bool setFillAmount;
	}

	[Serializable]
	public struct PredictionSettings
	{
		public enum ModelType
		{
			Auto,
			None,
			Simple,
			SpeedAcceleration
		}

		public static readonly PredictionSettings Default = new PredictionSettings
		{
			type = ModelType.Auto
		};

		public ModelType type;

		public bool useCustomParameters;

		public float delay;

		public float initSpeed;

		public float targetSpeed;

		public float acceleration;

		public float frontDistance;

		private bool _shouldShowCustomParameters
		{
			get
			{
				if (type != ModelType.None)
				{
					return type != ModelType.Auto;
				}
				return false;
			}
		}

		private bool _shouldShowSimpleParameters
		{
			get
			{
				if (useCustomParameters)
				{
					if (type != ModelType.Simple)
					{
						return type == ModelType.SpeedAcceleration;
					}
					return true;
				}
				return false;
			}
		}

		private bool _shouldShowSpeedAccelerationParameters
		{
			get
			{
				if (useCustomParameters)
				{
					return type == ModelType.SpeedAcceleration;
				}
				return false;
			}
		}
	}

	public DataProcessorGroup<CooldownReductionSettings, Actor, AbilityTrigger> takenCooldownReductionProcessor = new DataProcessorGroup<CooldownReductionSettings, Actor, AbilityTrigger>();

	public DataProcessorGroup<CooldownReductionByRatioSettings, Actor, AbilityTrigger> takenCooldownReductionByRatioProcessor = new DataProcessorGroup<CooldownReductionByRatioSettings, Actor, AbilityTrigger>();

	public SafeAction ClientTriggerEvent_OnCurrentConfigCharged;

	public SafeAction ClientTriggerEvent_OnCurrentConfigCooldownReduced;

	public SafeAction<int, float> ClientTriggerEvent_OnCooldownReduced;

	public SafeAction<int, float> ClientTriggerEvent_OnCooldownReducedByRatio;

	[SyncVar(hook = "OnOwnerChanged")]
	private Entity _owner;

	public SafeAction<Entity, Entity> ClientEvent_OnOwnerChanged;

	[SyncVar(hook = "OnConfigChanged")]
	private int _currentConfigIndex;

	[CompilerGenerated]
	[SyncVar]
	private bool ignoreRangeCheck__BackingField;

	[CompilerGenerated]
	[SyncVar]
	private float fillAmount__BackingField;

	[NonSerialized]
	[SaveVar(SaveVarFlags.ApplyAfterFrameDelay)]
	public float[] currentUnscaledCooldownTimes;

	[NonSerialized]
	[SaveVar(SaveVarFlags.ApplyAfterFrameDelay)]
	public int[] currentCharges;

	[NonSerialized]
	[SaveVar(SaveVarFlags.ApplyAfterFrameDelay)]
	public float[] currentMinimumDelays;

	public SafeAction<EventInfoCast> TriggerEvent_OnCastStart;

	public SafeAction<EventInfoCast> TriggerEvent_OnCastComplete;

	public SafeAction<EventInfoCast> TriggerEvent_OnCastCompleteBeforePrepare;

	internal bool _isConfigDirty;

	internal readonly SyncList<bool> _configCastLocks = new SyncList<bool>();

	internal readonly SyncList<bool> _configCooldownLocks = new SyncList<bool>();

	private List<CurrentAbilityInstanceData> _currentInstances = new List<CurrentAbilityInstanceData>();

	private int[] _lockConfigCooldownCounters;

	public TriggerConfig[] configs = new TriggerConfig[1]
	{
		new TriggerConfig()
	};

	[CompilerGenerated]
	[SyncVar]
	private StatusEffect currentPassiveEffect__BackingField;

	[SyncVar]
	private bool _isCasting;

	protected AbilityTargetValidatorWrapper tvDefaultHarmfulEffectTargets;

	protected AbilityTargetValidatorWrapper tvDefaultUsefulEffectTargets;

	protected AbilityTargetValidatorWrapper tvDefaultAllExceptSelf;

	[CompilerGenerated]
	[SyncVar]
	private int abilityIndex__BackingField = -1;

	private ActorRef<Se_GenericEffectContainer> _unstoppable;

	private static bool[] _selfEffectLockBuffer;

	private const float PredictionAngleLimit = 75f;

	private const float SpeedAccelerationModel_TimeStep = 0.1f;

	private const float SpeedAccelerationModel_MaxFutureTime = 2f;

	private const float SpeedAccelerationModel_AccurateSearchRangeStart = -0.15f;

	private const float SpeedAccelerationModel_AccurateSearchRangeEnd = 0.15f;

	private const float SpeedAccelerationModel_AccurateSearchTimeStep = 0.025f;

	protected NetworkBehaviourSyncVar ____ownerNetId;

	protected NetworkBehaviourSyncVar ____003CcurrentPassiveEffect_003Ek__BackingFieldNetId;

	public Action<Entity, Entity> _Mirror_SyncVarHookDelegate__owner;

	public Action<int, int> _Mirror_SyncVarHookDelegate__currentConfigIndex;

	public override bool isDestroyedOnRoomChange => (UnityEngine.Object)(object)owner == null;

	public Entity owner
	{
		get
		{
			return Network_owner;
		}
		set
		{
			Network_owner = value;
		}
	}

	public int currentConfigIndex
	{
		get
		{
			return _currentConfigIndex;
		}
		set
		{
			if (value >= configs.Length || value < 0)
			{
				throw new Exception($"Tried to set an invalid config index({value}) on {this}");
			}
			Network_currentConfigIndex = value;
		}
	}

	public bool ignoreRangeCheck
	{
		[CompilerGenerated]
		get
		{
			return ignoreRangeCheck__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CignoreRangeCheck_003Ek__BackingField = value;
		}
	}

	public float fillAmount
	{
		[CompilerGenerated]
		get
		{
			return fillAmount__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CfillAmount_003Ek__BackingField = value;
		}
	}

	public TriggerConfig currentConfig => configs[currentConfigIndex];

	public float currentConfigMaxCooldownTime => GetMaxCooldownTime(currentConfigIndex);

	public float currentConfigUnscaledMaxCooldownTime => GetMaxCooldownTime(currentConfigIndex, scaled: false);

	public float currentConfigCooldownTime => currentUnscaledCooldownTimes[currentConfigIndex] * GetCooldownTimeMultiplier(currentConfigIndex);

	public float currentConfigUnscaledCooldownTime => currentUnscaledCooldownTimes[currentConfigIndex];

	public int currentConfigCurrentCharge => currentCharges[currentConfigIndex];

	public float currentConfigCurrentMinimumDelay => currentMinimumDelays[currentConfigIndex];

	public StatusEffect currentPassiveEffect
	{
		[CompilerGenerated]
		get
		{
			return Network_003CcurrentPassiveEffect_003Ek__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			Network_003CcurrentPassiveEffect_003Ek__BackingField = value;
		}
	}

	public ChangedConfigHandle currentConfigChangeHandle { get; private set; }

	public int abilityIndex
	{
		[CompilerGenerated]
		get
		{
			return abilityIndex__BackingField;
		}
		[CompilerGenerated]
		internal set
		{
			Network_003CabilityIndex_003Ek__BackingField = value;
		}
	}

	public Entity Network_owner
	{
		get
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			return ((NetworkBehaviour)this).GetSyncVarNetworkBehaviour<Entity>(____ownerNetId, ref _owner);
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter_NetworkBehaviour<Entity>(value, ref _owner, 8uL, _Mirror_SyncVarHookDelegate__owner, ref ____ownerNetId);
		}
	}

	public int Network_currentConfigIndex
	{
		get
		{
			return _currentConfigIndex;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<int>(value, ref _currentConfigIndex, 16uL, _Mirror_SyncVarHookDelegate__currentConfigIndex);
		}
	}

	public bool Network_003CignoreRangeCheck_003Ek__BackingField
	{
		get
		{
			return ignoreRangeCheck__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref ignoreRangeCheck__BackingField, 32uL, (Action<bool, bool>)null);
		}
	}

	public float Network_003CfillAmount_003Ek__BackingField
	{
		get
		{
			return fillAmount__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref fillAmount__BackingField, 64uL, (Action<float, float>)null);
		}
	}

	public StatusEffect Network_003CcurrentPassiveEffect_003Ek__BackingField
	{
		get
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			return ((NetworkBehaviour)this).GetSyncVarNetworkBehaviour<StatusEffect>(____003CcurrentPassiveEffect_003Ek__BackingFieldNetId, ref currentPassiveEffect__BackingField);
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter_NetworkBehaviour<StatusEffect>(value, ref currentPassiveEffect__BackingField, 128uL, (Action<StatusEffect, StatusEffect>)null, ref ____003CcurrentPassiveEffect_003Ek__BackingFieldNetId);
		}
	}

	public bool Network_isCasting
	{
		get
		{
			return _isCasting;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref _isCasting, 256uL, (Action<bool, bool>)null);
		}
	}

	public int Network_003CabilityIndex_003Ek__BackingField
	{
		get
		{
			return abilityIndex__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<int>(value, ref abilityIndex__BackingField, 512uL, (Action<int, int>)null);
		}
	}

	public override void ClearPooledEventsAndProcessors()
	{
		base.ClearPooledEventsAndProcessors();
		ClientTriggerEvent_OnCurrentConfigCharged?.Clear();
		ClientTriggerEvent_OnCurrentConfigCooldownReduced?.Clear();
		ClientTriggerEvent_OnCooldownReduced?.Clear();
		ClientTriggerEvent_OnCooldownReducedByRatio?.Clear();
		ClientEvent_OnOwnerChanged?.Clear();
		TriggerEvent_OnCastStart?.Clear();
		TriggerEvent_OnCastComplete?.Clear();
		TriggerEvent_OnCastCompleteBeforePrepare?.Clear();
		takenCooldownReductionProcessor?.Clear();
		takenCooldownReductionByRatioProcessor?.Clear();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		_unstoppable = null;
	}

	protected override void OnPrepare()
	{
		base.OnPrepare();
		tvDefaultHarmfulEffectTargets = new AbilityTargetValidatorWrapper(null, EntityRelation.Neutral | EntityRelation.Enemy);
		tvDefaultUsefulEffectTargets = new AbilityTargetValidatorWrapper(null, EntityRelation.Self | EntityRelation.Ally);
		tvDefaultAllExceptSelf = new AbilityTargetValidatorWrapper(null, EntityRelation.Neutral | EntityRelation.Enemy | EntityRelation.Ally);
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer || isNewInstance)
		{
			return;
		}
		Dew.CallDelayed(() =>
		{
			if (!this.IsNullOrInactive() && ((NetworkBehaviour)this).isServer)
			{
				RpcSyncAfterFrameDelayStates();
			}
		}, 2);
	}

	public static float GetCooldownTimeMultiplierByAbilityHaste(float abilityHaste)
	{
		return Mathf.Lerp(((UnityEngine.Object)(object)NetworkedManagerBase<GameManager>.instance) ? NetworkedManagerBase<GameManager>.instance.ges.cooldownFloorRatioByAbilityHaste : 0f, 1f, 1f / (1f + abilityHaste * 0.01f));
	}

	public virtual float GetCooldownTimeMultiplier(int configIndex)
	{
		if ((UnityEngine.Object)(object)owner == null)
		{
			return 1f;
		}
		if (configs[configIndex].canReceiveCooldownReduction)
		{
			return GetCooldownTimeMultiplierByAbilityHaste(owner.Status.abilityHaste);
		}
		return 1f;
	}

	public float GetMaxCooldownTime(int configIndex, bool scaled = true)
	{
		float num = configs[configIndex].cooldownTime + GetCooldownTimeOffset(currentConfigIndex);
		if (scaled)
		{
			num *= GetCooldownTimeMultiplier(currentConfigIndex);
		}
		return Mathf.Max(0f, num);
	}

	public virtual float ProcessRange(float original)
	{
		return original;
	}

	public virtual float GetCooldownTimeOffset(int configIndex)
	{
		return 0f;
	}

	public virtual float GetAnimationSpeed()
	{
		return 1f;
	}

	public virtual float GetChannelDurationMultiplier()
	{
		return 1f;
	}

	public virtual float GetPostDelayDurationMultiplier()
	{
		return 1f;
	}

	[Server]
	public void UndoChangeConfig()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void AbilityTrigger::UndoChangeConfig()' called when server was not active");
		}
		else
		{
			RemoveCurrentConfigChange(isExpiration: false);
		}
	}

	[Server]
	private void RemoveCurrentConfigChange(bool isExpiration)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void AbilityTrigger::RemoveCurrentConfigChange(System.Boolean)' called when server was not active");
			return;
		}
		if (currentConfigChangeHandle == null || !currentConfigChangeHandle.isActive)
		{
			currentConfigChangeHandle = null;
			Debug.LogWarning("Tried to expire inactive ChangedConfigHandle.", (UnityEngine.Object)(object)this);
			return;
		}
		ChangedConfigHandle changedConfigHandle = currentConfigChangeHandle;
		currentConfigChangeHandle = null;
		currentConfigIndex = changedConfigHandle.previousConfigIndex;
		if (changedConfigHandle.setFillAmount)
		{
			Network_003CfillAmount_003Ek__BackingField = 0f;
		}
		if (isExpiration)
		{
			changedConfigHandle._onExpire?.Invoke();
		}
	}

	[Server]
	public ChangedConfigHandle ChangeConfigTimed(int index, float duration, Action<EventInfoAbilityInstance> onUse = null, Action onExpire = null, bool setFillAmount = true)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'AbilityTrigger/ChangedConfigHandle AbilityTrigger::ChangeConfigTimed(System.Int32,System.Single,System.Action`1<EventInfoAbilityInstance>,System.Action,System.Boolean)' called when server was not active");
			return null;
		}
		if (currentConfigChangeHandle != null)
		{
			RemoveCurrentConfigChange(isExpiration: true);
		}
		ChangedConfigHandle changedConfigHandle = new ChangedConfigHandle();
		changedConfigHandle._isActive = true;
		changedConfigHandle.remainingTime = duration;
		changedConfigHandle.duration = duration;
		changedConfigHandle._onUse = onUse;
		changedConfigHandle._onExpire = onExpire;
		changedConfigHandle._trigger = this;
		changedConfigHandle.previousConfigIndex = currentConfigIndex;
		changedConfigHandle.configIndex = index;
		changedConfigHandle.setFillAmount = setFillAmount;
		currentConfigIndex = index;
		currentConfigChangeHandle = changedConfigHandle;
		return changedConfigHandle;
	}

	[Server]
	public ChangedConfigHandle ChangeConfigTimedOnce(int index, float duration, Action<EventInfoAbilityInstance> onUse = null, Action onExpire = null, bool setFillAmount = true)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'AbilityTrigger/ChangedConfigHandle AbilityTrigger::ChangeConfigTimedOnce(System.Int32,System.Single,System.Action`1<EventInfoAbilityInstance>,System.Action,System.Boolean)' called when server was not active");
			return null;
		}
		ChangedConfigHandle handle = ChangeConfigTimed(index, duration, onUse, onExpire, setFillAmount);
		ChangedConfigHandle changedConfigHandle = handle;
		changedConfigHandle._onUse = (Action<EventInfoAbilityInstance>)Delegate.Combine(changedConfigHandle._onUse, (Action<EventInfoAbilityInstance>)((EventInfoAbilityInstance _) =>
		{
			if (handle.isActive)
			{
				RemoveCurrentConfigChange(isExpiration: false);
			}
		}));
		return handle;
	}

	[Server]
	internal void ApplyCooldownReductionWithMinimum_Imp(float amount, float minThreshold, bool scaled = true, bool ignoreCanReceiveCooldown = false)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void AbilityTrigger::ApplyCooldownReductionWithMinimum_Imp(System.Single,System.Single,System.Boolean,System.Boolean)' called when server was not active");
			return;
		}
		for (int i = 0; i < configs.Length; i++)
		{
			if (!configs[i].canReceiveCooldownReduction && !ignoreCanReceiveCooldown)
			{
				continue;
			}
			InvokeCooldownReduced(i, amount);
			float num = minThreshold;
			float num2 = amount;
			if (scaled)
			{
				float cooldownTimeMultiplier = GetCooldownTimeMultiplier(i);
				num /= cooldownTimeMultiplier;
				num2 /= cooldownTimeMultiplier;
			}
			if ((currentCharges[i] == 0 && currentUnscaledCooldownTimes[i] <= num) || currentCharges[i] >= configs[i].maxCharges)
			{
				continue;
			}
			while (num2 > 0f && currentCharges[i] < configs[i].maxCharges)
			{
				if (currentCharges[i] >= configs[i].maxCharges - 1)
				{
					currentUnscaledCooldownTimes[i] -= num2;
					if (currentUnscaledCooldownTimes[i] < num)
					{
						currentUnscaledCooldownTimes[i] = num;
					}
					break;
				}
				currentUnscaledCooldownTimes[i] -= num2;
				num2 = 0f;
				if (currentUnscaledCooldownTimes[i] < 0f)
				{
					num2 = 0f - currentUnscaledCooldownTimes[i];
					currentCharges[i] = Mathf.Min(currentCharges[i] + configs[i].addedCharges, configs[i].maxCharges);
					if (i == currentConfigIndex)
					{
						InvokeCurrentConfigCharged();
					}
					if (currentCharges[i] >= configs[i].maxCharges)
					{
						currentUnscaledCooldownTimes[i] = 0f;
						break;
					}
					currentUnscaledCooldownTimes[i] = GetMaxCooldownTime(i, scaled: false);
				}
			}
			if (i == currentConfigIndex)
			{
				InvokeCurrentConfigCooldownReduced();
			}
			RpcSetCooldownTime(i, currentUnscaledCooldownTimes[i]);
			RpcSetCharge(i, currentCharges[i]);
		}
	}

	[Server]
	internal void ApplyCooldownReductionByRatio_Imp(float ratio, bool ignoreCanReceiveCooldown = false)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void AbilityTrigger::ApplyCooldownReductionByRatio_Imp(System.Single,System.Boolean)' called when server was not active");
			return;
		}
		for (int i = 0; i < configs.Length; i++)
		{
			if (!configs[i].canReceiveCooldownReduction && !ignoreCanReceiveCooldown)
			{
				continue;
			}
			InvokeCooldownReducedByRatio(i, ratio);
			if (currentCharges[i] >= configs[i].maxCharges)
			{
				continue;
			}
			float num = GetMaxCooldownTime(i, scaled: false) * ratio;
			while (num > 0f && currentCharges[i] < configs[i].maxCharges)
			{
				currentUnscaledCooldownTimes[i] -= num;
				num = 0f;
				if (currentUnscaledCooldownTimes[i] < 0f)
				{
					num = 0f - currentUnscaledCooldownTimes[i];
					currentCharges[i] = Mathf.Min(currentCharges[i] + configs[i].addedCharges, configs[i].maxCharges);
					if (i == currentConfigIndex)
					{
						InvokeCurrentConfigCharged();
					}
					if (currentCharges[i] >= configs[i].maxCharges)
					{
						currentUnscaledCooldownTimes[i] = 0f;
						break;
					}
					currentUnscaledCooldownTimes[i] = GetMaxCooldownTime(i, scaled: false);
				}
			}
			if (i == currentConfigIndex)
			{
				InvokeCurrentConfigCooldownReduced();
			}
			RpcSetCooldownTime(i, currentUnscaledCooldownTimes[i]);
			RpcSetCharge(i, currentCharges[i]);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		TriggerConfig[] array = configs;
		for (int i = 0; i < array.Length; i++)
		{
			array[i]._parent = this;
		}
		_lockConfigCooldownCounters = new int[configs.Length];
	}

	public virtual void OnCastStart(int configIndex, CastInfo info)
	{
		Network_isCasting = true;
		if (!configs[configIndex].isActive)
		{
			throw new Exception($"Tried to cast a passive ability: {this}");
		}
		bool flag = owner is Hero && abilityIndex == 5 && Ai_GenericDodge.ShouldSkipAnimation(owner);
		TriggerConfig triggerConfig = configs[configIndex];
		info.animSelectValue = UnityEngine.Random.value;
		if (triggerConfig.startAnim != null && !flag)
		{
			owner.Animation.PlayAbilityAnimation(triggerConfig.startAnim, GetAnimationSpeed(), info.animSelectValue);
		}
		if (triggerConfig.castVoice != null)
		{
			owner.Sound.Say(triggerConfig.castVoice, interruptPrevious: true);
		}
		if (triggerConfig.effectOnCast != null)
		{
			DewEffect.PlayCastEffectNetworked(((NetworkBehaviour)this).netIdentity, triggerConfig.effectOnCast, info, triggerConfig.castMethod.type, triggerConfig.channel.duration * GetChannelDurationMultiplier());
		}
		if (triggerConfig.faceForward && (!flag || !owner.Control.overridenDesiredAngle.HasValue))
		{
			OnRotateForward(configIndex, info);
		}
		OnStartChannel(configIndex, info);
		EventInfoCast arg = new EventInfoCast
		{
			configIndex = configIndex,
			info = info,
			instance = null,
			trigger = this
		};
		owner.EntityEvent_OnCastStart?.Invoke(arg);
		TriggerEvent_OnCastStart?.Invoke(arg);
	}

	protected virtual void OnStartChannel(int configIndex, CastInfo info)
	{
		TriggerConfig triggerConfig = configs[configIndex];
		Channel channel = ((triggerConfig.castMethod.type != CastMethodType.Target) ? triggerConfig.channel.CreateChannel(() =>
		{
			OnCastComplete(configIndex, info);
		}, () =>
		{
			OnCastCancel(configIndex, info);
		}, triggerConfig.selfValidator) : triggerConfig.channel.CreateChannel(() =>
		{
			OnCastComplete(configIndex, info);
		}, () =>
		{
			OnCastCancel(configIndex, info);
		}, triggerConfig.selfValidator, info.target, triggerConfig.targetValidator));
		channel.duration *= GetChannelDurationMultiplier();
		if (triggerConfig.unstoppableWhileCasting)
		{
			_unstoppable = CreateBasicEffect(owner, new UnstoppableEffect(), channel.duration);
		}
		channel.AddValidation(() => !this.IsNullOrInactive() && !owner.IsNullInactiveDeadOrKnockedOut());
		owner.Control.StartChannel(channel);
	}

	protected virtual void OnRotateForward(int configIndex, CastInfo info)
	{
		TriggerConfig triggerConfig = configs[configIndex];
		switch (triggerConfig.castMethod.type)
		{
		case CastMethodType.Cone:
		case CastMethodType.Arrow:
			owner.Control.StopOverrideRotation();
			owner.Control.Rotate(info.rotation, immediately: false, triggerConfig.overrideRotation ? triggerConfig.overrideRotationDuration : (-1f));
			break;
		case CastMethodType.Target:
		{
			owner.Control.StopOverrideRotation();
			Entity target = info.target;
			if (!target.IsNullInactiveDeadOrKnockedOut() && (UnityEngine.Object)(object)target != (UnityEngine.Object)(object)info.caster)
			{
				owner.Control.RotateTowards(target, immediately: false, triggerConfig.overrideRotation ? triggerConfig.overrideRotationDuration : (-1f));
			}
			break;
		}
		case CastMethodType.Point:
			owner.Control.StopOverrideRotation();
			owner.Control.RotateTowards(info.point, immediately: false, triggerConfig.overrideRotation ? triggerConfig.overrideRotationDuration : (-1f));
			break;
		case CastMethodType.None:
			break;
		}
	}

	public override void OnSerialize(NetworkWriter writer, bool initialState)
	{
		((NetworkBehaviour)this).OnSerialize(writer, initialState);
		if (initialState)
		{
			for (int i = 0; i < configs.Length; i++)
			{
				NetworkWriterExtensions.WriteFloat(writer, currentUnscaledCooldownTimes[i]);
				NetworkWriterExtensions.WriteInt(writer, currentCharges[i]);
				NetworkWriterExtensions.WriteFloat(writer, currentMinimumDelays[i]);
				NetworkWriterExtensions.WriteFloat(writer, configs[i].cooldownTime);
				NetworkWriterExtensions.WriteFloat(writer, configs[i].manaCost);
				NetworkWriterExtensions.WriteInt(writer, configs[i].maxCharges);
				NetworkWriterExtensions.WriteFloat(writer, configs[i].minimumDelay);
				writer.Write<CastMethodData>(configs[i].castMethod);
			}
		}
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		((NetworkBehaviour)this).OnDeserialize(reader, initialState);
		if (initialState)
		{
			currentUnscaledCooldownTimes = new float[configs.Length];
			currentCharges = new int[configs.Length];
			currentMinimumDelays = new float[configs.Length];
			for (int i = 0; i < configs.Length; i++)
			{
				currentUnscaledCooldownTimes[i] = NetworkReaderExtensions.ReadFloat(reader);
				currentCharges[i] = NetworkReaderExtensions.ReadInt(reader);
				currentMinimumDelays[i] = NetworkReaderExtensions.ReadFloat(reader);
				configs[i].cooldownTime = NetworkReaderExtensions.ReadFloat(reader);
				configs[i].manaCost = NetworkReaderExtensions.ReadFloat(reader);
				configs[i].maxCharges = NetworkReaderExtensions.ReadInt(reader);
				configs[i].minimumDelay = NetworkReaderExtensions.ReadFloat(reader);
				configs[i].castMethod = reader.Read<CastMethodData>();
			}
		}
	}

	private void RpcSyncAfterFrameDelayStates()
	{
		RpcSyncAfterFrameDelayStates_Imp(currentUnscaledCooldownTimes, currentCharges, currentMinimumDelays);
	}

	[ClientRpc]
	private void RpcSyncAfterFrameDelayStates_Imp(float[] a, int[] b, float[] c)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_System_002ESingle_005B_005D((NetworkWriter)(object)val, a);
		GeneratedNetworkCode._Write_System_002EInt32_005B_005D((NetworkWriter)(object)val, b);
		GeneratedNetworkCode._Write_System_002ESingle_005B_005D((NetworkWriter)(object)val, c);
		((NetworkBehaviour)this).SendRPCInternal("System.Void AbilityTrigger::RpcSyncAfterFrameDelayStates_Imp(System.Single[],System.Int32[],System.Single[])", 1198612675, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	protected virtual void OnCastCancel(int configIndex, CastInfo info)
	{
		Network_isCasting = false;
		OnCastCancelSetCooldownTime(configIndex, info);
		TriggerConfig triggerConfig = configs[configIndex];
		if (triggerConfig.effectOnCast != null)
		{
			FxStopNetworked(triggerConfig.effectOnCast);
		}
		if (triggerConfig.startAnim != null && (UnityEngine.Object)(object)owner != null)
		{
			owner.Animation.StopAbilityAnimation(triggerConfig.startAnim);
		}
		if (!_unstoppable.IsNullOrInactive())
		{
			_unstoppable.Get().Destroy();
		}
		_unstoppable = null;
	}

	protected virtual void OnCastCancelSetCooldownTime(int configIndex, CastInfo info)
	{
		if (currentUnscaledCooldownTimes[configIndex] < 1f)
		{
			SetCooldownTime(configIndex, 1f, scaled: false);
		}
	}

	protected virtual Vector3 GetInstanceSpawnPosition(int configIndex, CastInfo info)
	{
		return ((Component)(object)owner).transform.position;
	}

	protected virtual Quaternion? GetInstanceSpawnRotation(int configIndex, CastInfo info)
	{
		CastMethodType type = configs[configIndex].castMethod.type;
		Quaternion? quaternion = null;
		return (type != CastMethodType.Arrow && type != CastMethodType.Cone) ? new Quaternion?(owner.Control.desiredRotation) : new Quaternion?(info.rotation);
	}

	public virtual AbilityInstance OnCastComplete(int configIndex, CastInfo info)
	{
		Network_isCasting = false;
		if (!configs[configIndex].isActive)
		{
			throw new Exception($"Tried to cast a passive ability: {this}");
		}
		OnCastCompleteSpendMana(configIndex, info);
		OnCastCompleteSetCooldownTime(configIndex, info);
		OnCastCompleteSetCharge(configIndex, info);
		OnCastCompleteSetMinimumDelay(configIndex, info);
		TriggerConfig triggerConfig = configs[configIndex];
		if (triggerConfig.castMethod.type == CastMethodType.Point)
		{
			float y = info.point.y;
			Vector2 vector = owner.agentPosition.ToXY() + Vector2.ClampMagnitude(info.point.ToXY() - owner.agentPosition.ToXY(), triggerConfig.castMethod.pointData.range);
			info.point = new Vector3(vector.x, y, vector.y);
		}
		if (!_unstoppable.IsNullOrInactive())
		{
			_unstoppable.Get().Destroy();
		}
		_unstoppable = null;
		if (triggerConfig.endAnim != null)
		{
			owner.Animation.PlayAbilityAnimation(triggerConfig.endAnim, GetAnimationSpeed(), info.animSelectValue);
		}
		if (triggerConfig.effectOnCast != null)
		{
			FxStopNetworked(triggerConfig.effectOnCast);
		}
		Type type = triggerConfig.spawnedInstanceRef.type;
		AbilityInstance abilityInstance;
		if (type == null)
		{
			abilityInstance = CreateAbilityInstance(GetInstanceSpawnPosition(configIndex, info), GetInstanceSpawnRotation(configIndex, info), info, (MockAbilityInstance ai) =>
			{
				EventInfoCast eventInfoCast2 = new EventInfoCast
				{
					configIndex = configIndex,
					info = info,
					trigger = this,
					instance = ai
				};
				OnCastCompleteBeforePrepare(eventInfoCast2);
				try
				{
					TriggerEvent_OnCastCompleteBeforePrepare?.Invoke(eventInfoCast2);
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
				}
				try
				{
					owner.EntityEvent_OnCastCompleteBeforePrepare?.Invoke(eventInfoCast2);
				}
				catch (Exception exception2)
				{
					Debug.LogException(exception2);
				}
			});
		}
		else if (typeof(StatusEffect).IsAssignableFrom(type))
		{
			Entity entity = ((triggerConfig.castMethod.type != CastMethodType.Target || triggerConfig.victim != TriggerConfig.StatusEffectVictimType.Target) ? info.caster : info.target);
			if (triggerConfig.destroyExistingEffect && entity.Status.TryGetStatusEffect(type, out var effect))
			{
				effect.Destroy();
			}
			abilityInstance = CreateStatusEffect(type, entity, info, (StatusEffect ai) =>
			{
				EventInfoCast eventInfoCast2 = new EventInfoCast
				{
					configIndex = configIndex,
					info = info,
					trigger = this,
					instance = ai
				};
				OnCastCompleteBeforePrepare(eventInfoCast2);
				try
				{
					TriggerEvent_OnCastCompleteBeforePrepare?.Invoke(eventInfoCast2);
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
				}
				try
				{
					owner.EntityEvent_OnCastCompleteBeforePrepare?.Invoke(eventInfoCast2);
				}
				catch (Exception exception2)
				{
					Debug.LogException(exception2);
				}
			});
		}
		else
		{
			abilityInstance = CreateAbilityInstance(type, GetInstanceSpawnPosition(configIndex, info), GetInstanceSpawnRotation(configIndex, info), info, (AbilityInstance ai) =>
			{
				EventInfoCast eventInfoCast2 = new EventInfoCast
				{
					configIndex = configIndex,
					info = info,
					trigger = this,
					instance = ai
				};
				OnCastCompleteBeforePrepare(eventInfoCast2);
				try
				{
					TriggerEvent_OnCastCompleteBeforePrepare?.Invoke(eventInfoCast2);
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
				}
				try
				{
					owner.EntityEvent_OnCastCompleteBeforePrepare?.Invoke(eventInfoCast2);
				}
				catch (Exception exception2)
				{
					Debug.LogException(exception2);
				}
			});
		}
		_currentInstances.Add(new CurrentAbilityInstanceData
		{
			instance = abilityInstance,
			configIndex = configIndex,
			shouldLockCast = triggerConfig.lockCastUntilKilled,
			shouldLockCooldown = triggerConfig.lockCooldownUntilKilled,
			setFillAmount = (triggerConfig.IsStatusEffectNonTargeted() && triggerConfig.setFillAmount)
		});
		UpdateCastAndCooldownLocks();
		if (currentConfigChangeHandle != null && currentConfigChangeHandle.isActive && currentConfigChangeHandle.configIndex == configIndex)
		{
			currentConfigChangeHandle._onUse?.Invoke(new EventInfoAbilityInstance
			{
				actor = this,
				instance = abilityInstance
			});
		}
		float num = triggerConfig.postDelay * GetPostDelayDurationMultiplier();
		if (num > 0f)
		{
			owner.Control.StartDaze(num);
		}
		EventInfoCast eventInfoCast = new EventInfoCast
		{
			configIndex = configIndex,
			info = info,
			trigger = this,
			instance = abilityInstance
		};
		Entity entity2 = owner;
		TriggerEvent_OnCastComplete?.Invoke(eventInfoCast);
		entity2.EntityEvent_OnCastComplete?.Invoke(eventInfoCast);
		NetworkedManagerBase<ClientEventManager>.instance.InvokeOnCastComplete(eventInfoCast);
		if (triggerConfig.moveTowardsCastDirection)
		{
			Vector3? vector2 = null;
			switch (triggerConfig.castMethod.type)
			{
			case CastMethodType.Cone:
			case CastMethodType.Arrow:
				vector2 = info.forward;
				break;
			case CastMethodType.Target:
				vector2 = (info.target.agentPosition - entity2.agentPosition).Flattened().normalized;
				break;
			case CastMethodType.Point:
				vector2 = (info.point - entity2.agentPosition).Flattened().normalized;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			case CastMethodType.None:
				break;
			}
			if (vector2.HasValue && vector2.Value.sqrMagnitude > 0.01f)
			{
				Vector3 agentPosition = entity2.agentPosition;
				Vector3? vector3 = vector2 * 20f;
				Vector3? vector4 = agentPosition + vector3;
				NavMeshHit val = default;
				if (NavMesh.Raycast(entity2.agentPosition, vector4.Value, ref val, -1))
				{
					vector4 = val.position;
				}
				entity2.Control.MoveToDestination(vector4.Value, immediately: true);
			}
		}
		return abilityInstance;
	}

	public virtual void OnCastCompleteSpendMana(int configIndex, CastInfo info)
	{
		if (configs[configIndex].manaCost > 0f)
		{
			SpendMana(configs[configIndex].manaCost, owner);
		}
	}

	public virtual void OnCastCompleteSetCooldownTime(int configIndex, CastInfo info)
	{
		if (currentUnscaledCooldownTimes[configIndex] <= 0f)
		{
			SetCooldownTime(configIndex, GetMaxCooldownTime(configIndex, scaled: false), scaled: false);
		}
	}

	public virtual void OnCastCompleteSetCharge(int configIndex, CastInfo info)
	{
		SetCharge(configIndex, currentCharges[configIndex] - 1);
	}

	public virtual void OnCastCompleteSetMinimumDelay(int configIndex, CastInfo info)
	{
		SetMinimumDelay(configIndex, configs[configIndex].minimumDelay);
	}

	public virtual void OnCastCompleteBeforePrepare(EventInfoCast cast)
	{
	}

	protected virtual bool ShouldTickCooldown(int configIndex)
	{
		return !_configCooldownLocks[configIndex];
	}

	public override void LogicUpdate(float dt)
	{
		base.LogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer && currentConfigChangeHandle != null)
		{
			currentConfigChangeHandle.remainingTime = Mathf.MoveTowards(currentConfigChangeHandle.remainingTime, 0f, dt);
			if (currentConfigChangeHandle.setFillAmount)
			{
				Network_003CfillAmount_003Ek__BackingField = currentConfigChangeHandle.remainingTime / currentConfigChangeHandle.duration;
			}
			if (currentConfigChangeHandle.remainingTime <= 0f)
			{
				RemoveCurrentConfigChange(isExpiration: true);
			}
		}
		if (((NetworkBehaviour)this).isServer)
		{
			for (int num = _currentInstances.Count - 1; num >= 0; num--)
			{
				if ((UnityEngine.Object)(object)_currentInstances[num].instance == null || !_currentInstances[num].instance.isActive)
				{
					if (_currentInstances[num].setFillAmount)
					{
						Network_003CfillAmount_003Ek__BackingField = 0f;
					}
					_currentInstances.RemoveAt(num);
					UpdateCastAndCooldownLocks();
				}
				else if (_currentInstances[num].setFillAmount && _currentInstances[num].instance is StatusEffect statusEffect)
				{
					Network_003CfillAmount_003Ek__BackingField = statusEffect.normalizedDuration ?? 0f;
				}
			}
		}
		if ((UnityEngine.Object)(object)owner != null)
		{
			for (int i = 0; i < configs.Length; i++)
			{
				if (ShouldTickCooldown(i))
				{
					float maxDelta = dt / GetCooldownTimeMultiplier(i);
					currentUnscaledCooldownTimes[i] = Mathf.MoveTowards(currentUnscaledCooldownTimes[i], 0f, maxDelta);
				}
				currentMinimumDelays[i] = Mathf.MoveTowards(currentMinimumDelays[i], 0f, dt);
				if (!((NetworkBehaviour)this).isServer)
				{
					continue;
				}
				if (currentUnscaledCooldownTimes[i] == 0f && currentCharges[i] < configs[i].maxCharges)
				{
					if (currentCharges[i] < configs[i].maxCharges - 1)
					{
						SetCooldownTime(i, GetMaxCooldownTime(i, scaled: false), scaled: false);
					}
					SetCharge(i, currentCharges[i] + configs[i].addedCharges);
					if (i == currentConfigIndex)
					{
						InvokeCurrentConfigCharged();
					}
				}
				if (currentCharges[i] > configs[i].maxCharges)
				{
					SetCharge(i, configs[i].maxCharges);
				}
				if (currentCharges[i] >= configs[i].maxCharges && currentUnscaledCooldownTimes[i] > 0f)
				{
					SetCooldownTime(i, 0f, scaled: false);
				}
			}
		}
		if (((NetworkBehaviour)this).isServer && _isConfigDirty)
		{
			_isConfigDirty = false;
			ConfigSyncData[] array = new ConfigSyncData[configs.Length];
			for (int j = 0; j < configs.Length; j++)
			{
				array[j].cooldownTime = configs[j].cooldownTime;
				array[j].manaCost = configs[j].manaCost;
				array[j].addedCharges = configs[j].addedCharges;
				array[j].maxCharges = configs[j].maxCharges;
				array[j].minimumDelay = configs[j].minimumDelay;
			}
			RpcSyncConfigs(array);
		}
	}

	public override void OnStartServer()
	{
		base.OnStartServer();
		currentUnscaledCooldownTimes = new float[configs.Length];
		currentCharges = new int[configs.Length];
		currentMinimumDelays = new float[configs.Length];
		for (int i = 0; i < configs.Length; i++)
		{
			_configCastLocks.Add(false);
		}
		for (int j = 0; j < configs.Length; j++)
		{
			_configCooldownLocks.Add(false);
		}
		for (int k = 0; k < configs.Length; k++)
		{
			if (configs[k].startCharges < 0)
			{
				currentCharges[k] = configs[k].maxCharges;
			}
			else
			{
				currentCharges[k] = Mathf.Clamp(configs[k].startCharges, 0, configs[k].maxCharges);
			}
			if (currentCharges[k] < configs[k].maxCharges)
			{
				currentUnscaledCooldownTimes[k] = GetMaxCooldownTime(k, scaled: false);
			}
		}
	}

	public override void OnStopServer()
	{
		base.OnStopServer();
		if (currentConfigChangeHandle != null && currentConfigChangeHandle.isActive)
		{
			RemoveCurrentConfigChange(isExpiration: true);
		}
	}

	protected virtual void OnEquip(Entity newOwner)
	{
		if (((NetworkBehaviour)this).isServer)
		{
			UpdatePassiveEffectIfNeccessary();
			tvDefaultAllExceptSelf.self = newOwner;
			tvDefaultHarmfulEffectTargets.self = newOwner;
			tvDefaultUsefulEffectTargets.self = newOwner;
		}
	}

	protected virtual void OnUnequip(Entity formerOwner)
	{
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)Network_003CcurrentPassiveEffect_003Ek__BackingField != null && Network_003CcurrentPassiveEffect_003Ek__BackingField.isActive)
		{
			Network_003CcurrentPassiveEffect_003Ek__BackingField.Destroy();
			Network_003CcurrentPassiveEffect_003Ek__BackingField = null;
		}
	}

	protected virtual void OnConfigChanged(int oldIndex, int newIndex)
	{
		if (((NetworkBehaviour)this).isServer)
		{
			UpdatePassiveEffectIfNeccessary();
		}
	}

	[Server]
	internal void UpdatePassiveEffectIfNeccessary()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void AbilityTrigger::UpdatePassiveEffectIfNeccessary()' called when server was not active");
		}
		else if (currentConfig.isActive || (UnityEngine.Object)(object)currentConfig.appliedStatusEffect == null)
		{
			if ((UnityEngine.Object)(object)Network_003CcurrentPassiveEffect_003Ek__BackingField != null)
			{
				Network_003CcurrentPassiveEffect_003Ek__BackingField.DestroyIfActive();
				Network_003CcurrentPassiveEffect_003Ek__BackingField = null;
			}
		}
		else if (Network_003CcurrentPassiveEffect_003Ek__BackingField.IsNullOrInactive() || !(((object)currentConfig.appliedStatusEffect).GetType() == ((object)Network_003CcurrentPassiveEffect_003Ek__BackingField).GetType()) || !((UnityEngine.Object)(object)Network_003CcurrentPassiveEffect_003Ek__BackingField.victim == (UnityEngine.Object)(object)owner) || (this is SkillTrigger skillTrigger && Network_003CcurrentPassiveEffect_003Ek__BackingField.skillLevel != skillTrigger.level))
		{
			if ((UnityEngine.Object)(object)Network_003CcurrentPassiveEffect_003Ek__BackingField != null)
			{
				Network_003CcurrentPassiveEffect_003Ek__BackingField.DestroyIfActive();
				Network_003CcurrentPassiveEffect_003Ek__BackingField = null;
			}
			Network_003CcurrentPassiveEffect_003Ek__BackingField = CreateStatusEffect(((object)currentConfig.appliedStatusEffect).GetType(), owner, new CastInfo(owner));
		}
	}

	[Server]
	public void SetCooldownTime(int configIndex, float time, bool scaled = true)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void AbilityTrigger::SetCooldownTime(System.Int32,System.Single,System.Boolean)' called when server was not active");
			return;
		}
		if (scaled)
		{
			time /= GetCooldownTimeMultiplier(configIndex);
		}
		currentUnscaledCooldownTimes[configIndex] = time;
		RpcSetCooldownTime(configIndex, time);
	}

	[ClientRpc]
	internal void RpcSetCooldownTime(int configIndex, float time)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteInt((NetworkWriter)(object)val, configIndex);
		NetworkWriterExtensions.WriteFloat((NetworkWriter)(object)val, time);
		((NetworkBehaviour)this).SendRPCInternal("System.Void AbilityTrigger::RpcSetCooldownTime(System.Int32,System.Single)", -840561602, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[Server]
	public void SetCooldownTimeAll(float time, bool scaled = true)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void AbilityTrigger::SetCooldownTimeAll(System.Single,System.Boolean)' called when server was not active");
			return;
		}
		for (int i = 0; i < configs.Length; i++)
		{
			currentUnscaledCooldownTimes[i] = (scaled ? (time / GetCooldownTimeMultiplier(i)) : time);
		}
		RpcSetCooldownTimeAll(time);
	}

	[ClientRpc]
	private void RpcSetCooldownTimeAll(float time)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteFloat((NetworkWriter)(object)val, time);
		((NetworkBehaviour)this).SendRPCInternal("System.Void AbilityTrigger::RpcSetCooldownTimeAll(System.Single)", 984725360, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[Server]
	public void SetCharge(int configIndex, int charge)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void AbilityTrigger::SetCharge(System.Int32,System.Int32)' called when server was not active");
			return;
		}
		charge = Mathf.Clamp(charge, 0, configs[configIndex].maxCharges);
		currentCharges[configIndex] = charge;
		if (currentCharges[configIndex] < configs[configIndex].maxCharges && currentUnscaledCooldownTimes[configIndex] <= 0f)
		{
			SetCooldownTime(configIndex, GetMaxCooldownTime(configIndex, scaled: false), scaled: false);
		}
		RpcSetCharge(configIndex, charge);
	}

	[ClientRpc]
	internal void RpcSetCharge(int configIndex, int charge)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteInt((NetworkWriter)(object)val, configIndex);
		NetworkWriterExtensions.WriteInt((NetworkWriter)(object)val, charge);
		((NetworkBehaviour)this).SendRPCInternal("System.Void AbilityTrigger::RpcSetCharge(System.Int32,System.Int32)", 802458050, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[Server]
	public void SetChargeAll(int charge)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void AbilityTrigger::SetChargeAll(System.Int32)' called when server was not active");
			return;
		}
		for (int i = 0; i < configs.Length; i++)
		{
			SetCharge(i, charge);
		}
	}

	[Server]
	public void SetMinimumDelay(int configIndex, float delay)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void AbilityTrigger::SetMinimumDelay(System.Int32,System.Single)' called when server was not active");
			return;
		}
		currentMinimumDelays[configIndex] = delay;
		RpcSetMinimumDelay(configIndex, delay);
	}

	[ClientRpc]
	private void RpcSetMinimumDelay(int configIndex, float delay)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteInt((NetworkWriter)(object)val, configIndex);
		NetworkWriterExtensions.WriteFloat((NetworkWriter)(object)val, delay);
		((NetworkBehaviour)this).SendRPCInternal("System.Void AbilityTrigger::RpcSetMinimumDelay(System.Int32,System.Single)", -712970245, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[Server]
	public void SetMinimumDelayAll(float delay)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void AbilityTrigger::SetMinimumDelayAll(System.Single)' called when server was not active");
			return;
		}
		for (int i = 0; i < configs.Length; i++)
		{
			currentMinimumDelays[i] = delay;
		}
		RpcSetMinimumDelayAll(delay);
	}

	[ClientRpc]
	private void RpcSetMinimumDelayAll(float delay)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteFloat((NetworkWriter)(object)val, delay);
		((NetworkBehaviour)this).SendRPCInternal("System.Void AbilityTrigger::RpcSetMinimumDelayAll(System.Single)", 482718573, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[ClientRpc]
	private void RpcSyncConfigs(ConfigSyncData[] data)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_AbilityTrigger_002FConfigSyncData_005B_005D((NetworkWriter)(object)val, data);
		((NetworkBehaviour)this).SendRPCInternal("System.Void AbilityTrigger::RpcSyncConfigs(AbilityTrigger/ConfigSyncData[])", -180556732, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	public virtual bool CanBeCast()
	{
		if (!_configCastLocks[currentConfigIndex] && currentConfigCurrentCharge > 0 && currentConfigCurrentMinimumDelay == 0f && currentConfig.selfValidator.Evaluate(owner) && owner.currentMana >= currentConfig.manaCost && !_isCasting)
		{
			return !owner.Ability.IsAbilityCastLocked(abilityIndex);
		}
		return false;
	}

	public virtual bool CanBeReserved()
	{
		if (!_configCastLocks[currentConfigIndex] && (UnityEngine.Object)(object)owner != null && currentConfigCurrentCharge > 0 && currentConfigCurrentMinimumDelay == 0f && owner.currentMana >= currentConfig.manaCost)
		{
			return !owner.Ability.IsAbilityCastLocked(abilityIndex);
		}
		return false;
	}

	private void OnOwnerChanged(Entity oldOwner, Entity newOwner)
	{
		try
		{
			if ((UnityEngine.Object)(object)oldOwner != null)
			{
				OnUnequip(oldOwner);
			}
			if ((UnityEngine.Object)(object)newOwner != null)
			{
				OnEquip(newOwner);
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		ClientEvent_OnOwnerChanged?.Invoke(oldOwner, newOwner);
	}

	public override string GetActorReadableName()
	{
		return string.Format("[{0}] {1}{2}{3}{4}", persistentNetId, ((NetworkBehaviour)this).isClient ? "" : "~", isActive ? "" : "!", ((object)this).GetType(), ((UnityEngine.Object)(object)owner != null) ? $" on [{owner.persistentNetId}]" : "");
	}

	[ClientRpc]
	internal void InvokeCurrentConfigCharged()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendRPCInternal("System.Void AbilityTrigger::InvokeCurrentConfigCharged()", 632735808, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[ClientRpc]
	private void InvokeCurrentConfigCooldownReduced()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendRPCInternal("System.Void AbilityTrigger::InvokeCurrentConfigCooldownReduced()", -305176029, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[ClientRpc]
	private void InvokeCooldownReduced(int configIndex, float amount)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteInt((NetworkWriter)(object)val, configIndex);
		NetworkWriterExtensions.WriteFloat((NetworkWriter)(object)val, amount);
		((NetworkBehaviour)this).SendRPCInternal("System.Void AbilityTrigger::InvokeCooldownReduced(System.Int32,System.Single)", 840126176, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[ClientRpc]
	private void InvokeCooldownReducedByRatio(int configIndex, float amount)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteInt((NetworkWriter)(object)val, configIndex);
		NetworkWriterExtensions.WriteFloat((NetworkWriter)(object)val, amount);
		((NetworkBehaviour)this).SendRPCInternal("System.Void AbilityTrigger::InvokeCooldownReducedByRatio(System.Int32,System.Single)", 1447345474, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	public bool IsTargetInRange(Entity target, bool isAI = false)
	{
		if ((UnityEngine.Object)(object)owner == null)
		{
			return false;
		}
		float effectiveRange = currentConfig.effectiveRange;
		Vector3 v = (isAI ? target.GetAIAgentPosition(owner) : target.agentPosition);
		return Vector2.Distance(owner.agentPosition.ToXY(), v.ToXY()) - target.Control.outerRadius < effectiveRange;
	}

	[Server]
	private void UpdateCastAndCooldownLocks()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void AbilityTrigger::UpdateCastAndCooldownLocks()' called when server was not active");
			return;
		}
		for (int i = 0; i < configs.Length; i++)
		{
			_selfEffectLockBuffer[i] = false;
		}
		for (int j = 0; j < _currentInstances.Count; j++)
		{
			if (_currentInstances[j].shouldLockCast)
			{
				_selfEffectLockBuffer[_currentInstances[j].configIndex] = true;
			}
		}
		for (int k = 0; k < configs.Length; k++)
		{
			_configCastLocks[k] = _selfEffectLockBuffer[k];
		}
		for (int l = 0; l < configs.Length; l++)
		{
			_selfEffectLockBuffer[l] = _lockConfigCooldownCounters[l] > 0;
		}
		for (int m = 0; m < _currentInstances.Count; m++)
		{
			if (_currentInstances[m].shouldLockCooldown)
			{
				_selfEffectLockBuffer[_currentInstances[m].configIndex] = true;
			}
		}
		for (int n = 0; n < configs.Length; n++)
		{
			_configCooldownLocks[n] = _selfEffectLockBuffer[n];
		}
	}

	public bool IsConfigLocked(int configIndex)
	{
		return _configCastLocks[configIndex];
	}

	public void SyncCastMethodChanges(int configIndex)
	{
		RpcSetCastMethod(configIndex, configs[configIndex].castMethod);
	}

	[ClientRpc]
	private void RpcSetCastMethod(int configIndex, CastMethodData method)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteInt((NetworkWriter)(object)val, configIndex);
		((NetworkWriter)(object)val).WriteCastMethodData(method);
		((NetworkBehaviour)this).SendRPCInternal("System.Void AbilityTrigger::RpcSetCastMethod(System.Int32,CastMethodData)", -948141829, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[Server]
	public void LockCooldown(int configIndex)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void AbilityTrigger::LockCooldown(System.Int32)' called when server was not active");
			return;
		}
		_lockConfigCooldownCounters[configIndex]++;
		UpdateCastAndCooldownLocks();
	}

	[Server]
	public void UnlockCooldown(int configIndex)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void AbilityTrigger::UnlockCooldown(System.Int32)' called when server was not active");
			return;
		}
		_lockConfigCooldownCounters[configIndex]--;
		UpdateCastAndCooldownLocks();
	}

	public void ProcessReceivedCooldownReduction(ref CooldownReductionSettings data, Actor actor)
	{
		takenCooldownReductionProcessor.Process(ref data, actor, this);
	}

	public void ProcessReceivedCooldownReductionByRatio(ref CooldownReductionByRatioSettings data, Actor actor)
	{
		takenCooldownReductionByRatioProcessor.Process(ref data, actor, this);
	}

	public CastInfo GetPredictedCastInfoToTarget(Entity target)
	{
		if (owner is Monster { isHunter: not false } monster && !monster.IsAnyBoss())
		{
			return GetPredictedCastInfoToTarget(target, UnityEngine.Random.value);
		}
		return GetPredictedCastInfoToTarget(target, owner.AI.predictionStrengthOverride?.Invoke() ?? NetworkedManagerBase<GameManager>.instance.GetPredictionStrength());
	}

	public CastInfo GetPredictedCastInfoToTarget(Entity target, float strength)
	{
		PredictionSettings prediction = currentConfig.predictionSettings;
		if (prediction.type == PredictionSettings.ModelType.None)
		{
			return GetCastInfoToTarget(target);
		}
		CastMethodData castMethod = currentConfig.castMethod;
		CastMethodType type = castMethod.type;
		if (type == CastMethodType.None || type == CastMethodType.Target)
		{
			return GetCastInfoToTarget(target);
		}
		PopulatePredictionAutoParameters(ref prediction);
		Vector3 aIPosition = target.GetAIPosition(owner);
		float angle = CastInfo.GetAngle(aIPosition - owner.position);
		switch (prediction.type)
		{
		case PredictionSettings.ModelType.Simple:
			switch (castMethod.type)
			{
			case CastMethodType.Cone:
			case CastMethodType.Arrow:
			{
				float num2 = PredictAngle_Simple(owner, strength, target, owner.position, prediction.delay);
				if (Mathf.Abs(angle - num2) > 75f)
				{
					num2 = angle;
				}
				return new CastInfo(owner, num2);
			}
			case CastMethodType.Point:
			{
				Vector3 vector2 = PredictPointClamped_Simple(owner, strength, target, owner.position, currentConfig.effectiveRange, prediction.delay);
				if (Mathf.Abs(angle - CastInfo.GetAngle(vector2 - owner.position)) > 75f)
				{
					vector2 = aIPosition;
				}
				return new CastInfo(owner, vector2);
			}
			default:
				throw new ArgumentOutOfRangeException();
			}
		case PredictionSettings.ModelType.SpeedAcceleration:
			switch (castMethod.type)
			{
			case CastMethodType.Cone:
			case CastMethodType.Arrow:
			{
				float num = PredictAngle_SpeedAcceleration(owner, strength, target, owner.position, prediction.delay, prediction.frontDistance, prediction.initSpeed, prediction.targetSpeed, prediction.acceleration);
				if (Mathf.Abs(angle - num) > 75f)
				{
					num = angle;
				}
				return new CastInfo(owner, num);
			}
			case CastMethodType.Point:
			{
				Vector3 vector = PredictPointClamped_SpeedAcceleration(owner, strength, target, owner.position, currentConfig.effectiveRange, prediction.delay, prediction.frontDistance, prediction.initSpeed, prediction.targetSpeed, prediction.acceleration);
				if (Mathf.Abs(angle - CastInfo.GetAngle(vector - owner.position)) > 75f)
				{
					vector = aIPosition;
				}
				return new CastInfo(owner, vector);
			}
			default:
				throw new ArgumentOutOfRangeException();
			}
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public CastInfo GetCastInfoToTarget(Entity target)
	{
		switch (currentConfig.castMethod.type)
		{
		case CastMethodType.None:
			return new CastInfo(owner);
		case CastMethodType.Cone:
		case CastMethodType.Arrow:
			return new CastInfo(owner, CastInfo.GetAngle(target.GetAIPosition(owner) - owner.position));
		case CastMethodType.Target:
			return new CastInfo(owner, target);
		case CastMethodType.Point:
			return new CastInfo(owner, target.GetAIPosition(owner));
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	private void PopulatePredictionAutoParameters(ref PredictionSettings prediction)
	{
		if (prediction.type == PredictionSettings.ModelType.Auto)
		{
			prediction.useCustomParameters = false;
			prediction.type = ((currentConfig.spawnedInstance is StandardProjectile) ? PredictionSettings.ModelType.SpeedAcceleration : PredictionSettings.ModelType.Simple);
		}
		if (!prediction.useCustomParameters)
		{
			if (currentConfig.spawnedInstance is StandardProjectile standardProjectile)
			{
				prediction.delay = currentConfig.channel.duration;
				prediction.frontDistance = standardProjectile.startInFrontDistance;
				prediction.initSpeed = standardProjectile._initialSpeed;
				prediction.targetSpeed = standardProjectile._targetSpeed;
				prediction.acceleration = standardProjectile._acceleration;
			}
			else if (currentConfig.spawnedInstance is InstantDamageInstance instantDamageInstance)
			{
				prediction.delay = currentConfig.channel.duration + instantDamageInstance.damageDelay;
			}
			else
			{
				prediction.delay = currentConfig.channel.duration;
			}
		}
	}

	public static Vector3 PredictPoint_Simple(Entity self, float strength, Entity target, float delay)
	{
		return Dew.GetPositionOnGround(target.GetAIPosition(self) + target.AI.estimatedVelocity * (delay * strength));
	}

	public static Vector3 PredictPointClamped_Simple(Entity self, float strength, Entity target, Vector3 startPos, float range, float delay)
	{
		Vector3 vector = PredictPoint_Simple(self, strength, target, delay) - startPos;
		vector = Vector3.ClampMagnitude(vector, range);
		return startPos + vector;
	}

	public static float PredictAngle_Simple(Entity self, float strength, Entity target, Vector3 startPos, float delay)
	{
		return CastInfo.GetAngle(PredictPoint_Simple(self, strength, target, delay) - startPos);
	}

	public static float PredictTime_SpeedAcceleration(Entity self, Entity target, Vector3 startPos, float delay, float frontDistance, float initSpeed, float targetSpeed, float acceleration)
	{
		Vector2 posTarget = target.GetAIPosition(self).ToXY();
		Vector2 velTarget = target.AI.estimatedVelocity.ToXY();
		float thresholdPt = ((acceleration == 0f) ? float.PositiveInfinity : ((targetSpeed - initSpeed) / acceleration));
		float num = float.PositiveInfinity;
		float num2 = -1f;
		for (float num3 = delay; num3 < 2f + delay; num3 += 0.1f)
		{
			float num4 = GetDiff(num3);
			if (num4 < num)
			{
				num = num4;
				num2 = num3;
			}
		}
		float num5 = num2 + -0.15f;
		float num6 = num2 + 0.15f;
		for (float num7 = num5; num7 < num6; num7 += 0.025f)
		{
			float num8 = GetDiff(num7);
			if (num8 < num)
			{
				num = num8;
				num2 = num7;
			}
		}
		return num2;
		float GetDiff(float t)
		{
			float num9 = Pow2(velTarget.x * t + posTarget.x - startPos.x) + Pow2(velTarget.y * t + posTarget.y - startPos.y);
			float num10 = frontDistance;
			float num11 = t - delay;
			num10 = ((!(num11 <= thresholdPt)) ? (num10 + Pow2(targetSpeed * num11 + ((0f - targetSpeed) * targetSpeed - initSpeed * initSpeed + 2f * initSpeed * targetSpeed) / (2f * acceleration))) : (num10 + Pow2(initSpeed * num11 + 0.5f * acceleration * num11 * num11)));
			return Mathf.Abs(num10 - num9);
		}
	}

	public static Vector3 PredictPoint_SpeedAcceleration(Entity self, float strength, Entity target, Vector3 startPos, float delay, float frontDistance, float initSpeed, float targetSpeed, float acceleration)
	{
		float delay2 = PredictTime_SpeedAcceleration(self, target, startPos, delay, frontDistance, initSpeed, targetSpeed, acceleration);
		return PredictPoint_Simple(self, strength, target, delay2);
	}

	public static Vector3 PredictPointClamped_SpeedAcceleration(Entity self, float strength, Entity target, Vector3 startPos, float range, float delay, float frontDistance, float initSpeed, float targetSpeed, float acceleration)
	{
		float delay2 = PredictTime_SpeedAcceleration(self, target, startPos, delay, frontDistance, initSpeed, targetSpeed, acceleration);
		return PredictPointClamped_Simple(self, strength, target, startPos, range, delay2);
	}

	public static float PredictAngle_SpeedAcceleration(Entity self, float strength, Entity target, Vector3 startPos, float delay, float frontDistance, float initSpeed, float targetSpeed, float acceleration)
	{
		float delay2 = PredictTime_SpeedAcceleration(self, target, startPos, delay, frontDistance, initSpeed, targetSpeed, acceleration);
		return PredictAngle_Simple(self, strength, target, startPos, delay2);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static float Pow2(float a)
	{
		return a * a;
	}

	public AbilityTrigger()
	{
		((NetworkBehaviour)this).InitSyncObject((SyncObject)(object)_configCastLocks);
		((NetworkBehaviour)this).InitSyncObject((SyncObject)(object)_configCooldownLocks);
		_Mirror_SyncVarHookDelegate__owner = OnOwnerChanged;
		_Mirror_SyncVarHookDelegate__currentConfigIndex = OnConfigChanged;
	}

	static AbilityTrigger()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Expected Obj, but got Unknown
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Expected Obj, but got Unknown
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Expected Obj, but got Unknown
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Expected Obj, but got Unknown
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Expected Obj, but got Unknown
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Expected Obj, but got Unknown
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Expected Obj, but got Unknown
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Expected Obj, but got Unknown
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Expected Obj, but got Unknown
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Expected Obj, but got Unknown
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Expected Obj, but got Unknown
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Expected Obj, but got Unknown
		_selfEffectLockBuffer = new bool[32];
		RemoteProcedureCalls.RegisterRpc(typeof(AbilityTrigger), "System.Void AbilityTrigger::RpcSyncAfterFrameDelayStates_Imp(System.Single[],System.Int32[],System.Single[])", (RemoteCallDelegate)InvokeUserCode_RpcSyncAfterFrameDelayStates_Imp__Single_005B_005D__Int32_005B_005D__Single_005B_005D);
		RemoteProcedureCalls.RegisterRpc(typeof(AbilityTrigger), "System.Void AbilityTrigger::RpcSetCooldownTime(System.Int32,System.Single)", (RemoteCallDelegate)InvokeUserCode_RpcSetCooldownTime__Int32__Single);
		RemoteProcedureCalls.RegisterRpc(typeof(AbilityTrigger), "System.Void AbilityTrigger::RpcSetCooldownTimeAll(System.Single)", (RemoteCallDelegate)InvokeUserCode_RpcSetCooldownTimeAll__Single);
		RemoteProcedureCalls.RegisterRpc(typeof(AbilityTrigger), "System.Void AbilityTrigger::RpcSetCharge(System.Int32,System.Int32)", (RemoteCallDelegate)InvokeUserCode_RpcSetCharge__Int32__Int32);
		RemoteProcedureCalls.RegisterRpc(typeof(AbilityTrigger), "System.Void AbilityTrigger::RpcSetMinimumDelay(System.Int32,System.Single)", (RemoteCallDelegate)InvokeUserCode_RpcSetMinimumDelay__Int32__Single);
		RemoteProcedureCalls.RegisterRpc(typeof(AbilityTrigger), "System.Void AbilityTrigger::RpcSetMinimumDelayAll(System.Single)", (RemoteCallDelegate)InvokeUserCode_RpcSetMinimumDelayAll__Single);
		RemoteProcedureCalls.RegisterRpc(typeof(AbilityTrigger), "System.Void AbilityTrigger::RpcSyncConfigs(AbilityTrigger/ConfigSyncData[])", (RemoteCallDelegate)InvokeUserCode_RpcSyncConfigs__ConfigSyncData_005B_005D);
		RemoteProcedureCalls.RegisterRpc(typeof(AbilityTrigger), "System.Void AbilityTrigger::InvokeCurrentConfigCharged()", (RemoteCallDelegate)InvokeUserCode_InvokeCurrentConfigCharged);
		RemoteProcedureCalls.RegisterRpc(typeof(AbilityTrigger), "System.Void AbilityTrigger::InvokeCurrentConfigCooldownReduced()", (RemoteCallDelegate)InvokeUserCode_InvokeCurrentConfigCooldownReduced);
		RemoteProcedureCalls.RegisterRpc(typeof(AbilityTrigger), "System.Void AbilityTrigger::InvokeCooldownReduced(System.Int32,System.Single)", (RemoteCallDelegate)InvokeUserCode_InvokeCooldownReduced__Int32__Single);
		RemoteProcedureCalls.RegisterRpc(typeof(AbilityTrigger), "System.Void AbilityTrigger::InvokeCooldownReducedByRatio(System.Int32,System.Single)", (RemoteCallDelegate)InvokeUserCode_InvokeCooldownReducedByRatio__Int32__Single);
		RemoteProcedureCalls.RegisterRpc(typeof(AbilityTrigger), "System.Void AbilityTrigger::RpcSetCastMethod(System.Int32,CastMethodData)", (RemoteCallDelegate)InvokeUserCode_RpcSetCastMethod__Int32__CastMethodData);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcSyncAfterFrameDelayStates_Imp__Single_005B_005D__Int32_005B_005D__Single_005B_005D(float[] a, int[] b, float[] c)
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			currentUnscaledCooldownTimes = a;
			currentCharges = b;
			currentMinimumDelays = c;
		}
	}

	protected static void InvokeUserCode_RpcSyncAfterFrameDelayStates_Imp__Single_005B_005D__Int32_005B_005D__Single_005B_005D(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSyncAfterFrameDelayStates_Imp called on server.");
		}
		else
		{
			((AbilityTrigger)(object)obj).UserCode_RpcSyncAfterFrameDelayStates_Imp__Single_005B_005D__Int32_005B_005D__Single_005B_005D(GeneratedNetworkCode._Read_System_002ESingle_005B_005D(reader), GeneratedNetworkCode._Read_System_002EInt32_005B_005D(reader), GeneratedNetworkCode._Read_System_002ESingle_005B_005D(reader));
		}
	}

	protected void UserCode_RpcSetCooldownTime__Int32__Single(int configIndex, float time)
	{
		if (!NetworkServer.active)
		{
			currentUnscaledCooldownTimes[configIndex] = time;
		}
	}

	protected static void InvokeUserCode_RpcSetCooldownTime__Int32__Single(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetCooldownTime called on server.");
		}
		else
		{
			((AbilityTrigger)(object)obj).UserCode_RpcSetCooldownTime__Int32__Single(NetworkReaderExtensions.ReadInt(reader), NetworkReaderExtensions.ReadFloat(reader));
		}
	}

	protected void UserCode_RpcSetCooldownTimeAll__Single(float time)
	{
		if (!NetworkServer.active)
		{
			for (int i = 0; i < configs.Length; i++)
			{
				currentUnscaledCooldownTimes[i] = time;
			}
		}
	}

	protected static void InvokeUserCode_RpcSetCooldownTimeAll__Single(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetCooldownTimeAll called on server.");
		}
		else
		{
			((AbilityTrigger)(object)obj).UserCode_RpcSetCooldownTimeAll__Single(NetworkReaderExtensions.ReadFloat(reader));
		}
	}

	protected void UserCode_RpcSetCharge__Int32__Int32(int configIndex, int charge)
	{
		if (!NetworkServer.active)
		{
			currentCharges[configIndex] = charge;
		}
	}

	protected static void InvokeUserCode_RpcSetCharge__Int32__Int32(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetCharge called on server.");
		}
		else
		{
			((AbilityTrigger)(object)obj).UserCode_RpcSetCharge__Int32__Int32(NetworkReaderExtensions.ReadInt(reader), NetworkReaderExtensions.ReadInt(reader));
		}
	}

	protected void UserCode_RpcSetMinimumDelay__Int32__Single(int configIndex, float delay)
	{
		if (!NetworkServer.active)
		{
			currentMinimumDelays[configIndex] = delay;
		}
	}

	protected static void InvokeUserCode_RpcSetMinimumDelay__Int32__Single(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetMinimumDelay called on server.");
		}
		else
		{
			((AbilityTrigger)(object)obj).UserCode_RpcSetMinimumDelay__Int32__Single(NetworkReaderExtensions.ReadInt(reader), NetworkReaderExtensions.ReadFloat(reader));
		}
	}

	protected void UserCode_RpcSetMinimumDelayAll__Single(float delay)
	{
		if (!NetworkServer.active)
		{
			for (int i = 0; i < configs.Length; i++)
			{
				currentMinimumDelays[i] = delay;
			}
		}
	}

	protected static void InvokeUserCode_RpcSetMinimumDelayAll__Single(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetMinimumDelayAll called on server.");
		}
		else
		{
			((AbilityTrigger)(object)obj).UserCode_RpcSetMinimumDelayAll__Single(NetworkReaderExtensions.ReadFloat(reader));
		}
	}

	protected void UserCode_RpcSyncConfigs__ConfigSyncData_005B_005D(ConfigSyncData[] data)
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			if (data.Length != configs.Length)
			{
				throw new Exception($"{this} configuration sync failed; data length ({data.Length}) different from expected local config length ({configs.Length}), number of configs should never be edited on runtime");
			}
			for (int i = 0; i < data.Length; i++)
			{
				configs[i]._cooldownTime = data[i].cooldownTime;
				configs[i]._manaCost = data[i].manaCost;
				configs[i]._addedCharges = data[i].addedCharges;
				configs[i]._maxCharges = data[i].maxCharges;
				configs[i]._minimumDelay = data[i].minimumDelay;
			}
		}
	}

	protected static void InvokeUserCode_RpcSyncConfigs__ConfigSyncData_005B_005D(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSyncConfigs called on server.");
		}
		else
		{
			((AbilityTrigger)(object)obj).UserCode_RpcSyncConfigs__ConfigSyncData_005B_005D(GeneratedNetworkCode._Read_AbilityTrigger_002FConfigSyncData_005B_005D(reader));
		}
	}

	protected void UserCode_InvokeCurrentConfigCharged()
	{
		ClientTriggerEvent_OnCurrentConfigCharged?.Invoke();
	}

	protected static void InvokeUserCode_InvokeCurrentConfigCharged(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC InvokeCurrentConfigCharged called on server.");
		}
		else
		{
			((AbilityTrigger)(object)obj).UserCode_InvokeCurrentConfigCharged();
		}
	}

	protected void UserCode_InvokeCurrentConfigCooldownReduced()
	{
		ClientTriggerEvent_OnCurrentConfigCooldownReduced?.Invoke();
	}

	protected static void InvokeUserCode_InvokeCurrentConfigCooldownReduced(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC InvokeCurrentConfigCooldownReduced called on server.");
		}
		else
		{
			((AbilityTrigger)(object)obj).UserCode_InvokeCurrentConfigCooldownReduced();
		}
	}

	protected void UserCode_InvokeCooldownReduced__Int32__Single(int configIndex, float amount)
	{
		ClientTriggerEvent_OnCooldownReduced?.Invoke(configIndex, amount);
	}

	protected static void InvokeUserCode_InvokeCooldownReduced__Int32__Single(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC InvokeCooldownReduced called on server.");
		}
		else
		{
			((AbilityTrigger)(object)obj).UserCode_InvokeCooldownReduced__Int32__Single(NetworkReaderExtensions.ReadInt(reader), NetworkReaderExtensions.ReadFloat(reader));
		}
	}

	protected void UserCode_InvokeCooldownReducedByRatio__Int32__Single(int configIndex, float amount)
	{
		ClientTriggerEvent_OnCooldownReducedByRatio?.Invoke(configIndex, amount);
	}

	protected static void InvokeUserCode_InvokeCooldownReducedByRatio__Int32__Single(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC InvokeCooldownReducedByRatio called on server.");
		}
		else
		{
			((AbilityTrigger)(object)obj).UserCode_InvokeCooldownReducedByRatio__Int32__Single(NetworkReaderExtensions.ReadInt(reader), NetworkReaderExtensions.ReadFloat(reader));
		}
	}

	protected void UserCode_RpcSetCastMethod__Int32__CastMethodData(int configIndex, CastMethodData method)
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			configs[configIndex].castMethod = method;
		}
	}

	protected static void InvokeUserCode_RpcSetCastMethod__Int32__CastMethodData(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetCastMethod called on server.");
		}
		else
		{
			((AbilityTrigger)(object)obj).UserCode_RpcSetCastMethod__Int32__CastMethodData(NetworkReaderExtensions.ReadInt(reader), reader.ReadWriteCastMethodData());
		}
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)Network_owner);
			NetworkWriterExtensions.WriteInt(writer, _currentConfigIndex);
			NetworkWriterExtensions.WriteBool(writer, ignoreRangeCheck__BackingField);
			NetworkWriterExtensions.WriteFloat(writer, fillAmount__BackingField);
			NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)Network_003CcurrentPassiveEffect_003Ek__BackingField);
			NetworkWriterExtensions.WriteBool(writer, _isCasting);
			NetworkWriterExtensions.WriteInt(writer, abilityIndex__BackingField);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 8L) != 0L)
		{
			NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)Network_owner);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x10L) != 0L)
		{
			NetworkWriterExtensions.WriteInt(writer, _currentConfigIndex);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x20L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, ignoreRangeCheck__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x40L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, fillAmount__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x80L) != 0L)
		{
			NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)Network_003CcurrentPassiveEffect_003Ek__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x100L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, _isCasting);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x200L) != 0L)
		{
			NetworkWriterExtensions.WriteInt(writer, abilityIndex__BackingField);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize_NetworkBehaviour<Entity>(ref _owner, _Mirror_SyncVarHookDelegate__owner, reader, ref ____ownerNetId);
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref _currentConfigIndex, _Mirror_SyncVarHookDelegate__currentConfigIndex, NetworkReaderExtensions.ReadInt(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref ignoreRangeCheck__BackingField, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref fillAmount__BackingField, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize_NetworkBehaviour<StatusEffect>(ref currentPassiveEffect__BackingField, (Action<StatusEffect, StatusEffect>)null, reader, ref ____003CcurrentPassiveEffect_003Ek__BackingFieldNetId);
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref _isCasting, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref abilityIndex__BackingField, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 8L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize_NetworkBehaviour<Entity>(ref _owner, _Mirror_SyncVarHookDelegate__owner, reader, ref ____ownerNetId);
		}
		if ((num & 0x10L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref _currentConfigIndex, _Mirror_SyncVarHookDelegate__currentConfigIndex, NetworkReaderExtensions.ReadInt(reader));
		}
		if ((num & 0x20L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref ignoreRangeCheck__BackingField, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
		}
		if ((num & 0x40L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref fillAmount__BackingField, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
		if ((num & 0x80L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize_NetworkBehaviour<StatusEffect>(ref currentPassiveEffect__BackingField, (Action<StatusEffect, StatusEffect>)null, reader, ref ____003CcurrentPassiveEffect_003Ek__BackingFieldNetId);
		}
		if ((num & 0x100L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref _isCasting, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
		}
		if ((num & 0x200L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref abilityIndex__BackingField, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
		}
	}
}
