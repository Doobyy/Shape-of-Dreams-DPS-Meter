using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;
using UnityEngine.Serialization;

public abstract class StatusEffect : AbilityInstance
{
	public class DoAbilityItem
	{
		public Func<AbilityTrigger, bool> condition;

		public Func<AbilityTrigger, Action> callback;

		public SafeAction stopAction;
	}

	[SyncVar]
	private Entity _victim;

	[SyncVar]
	public float? normalizedFillAmount;

	public bool isCleansable;

	[FormerlySerializedAs("isBeneficial")]
	public bool isBeneficialBuff;

	public bool isKilledByCrowdControlImmunity;

	public bool scaleDurationByTenacity;

	[SyncVar(hook = "OnShowIconChanged")]
	[FormerlySerializedAs("showIcon")]
	[SerializeField]
	private bool _showIcon;

	public Sprite icon;

	public Color topColor = Color.white;

	public Color bottomColor = Color.white;

	public int iconOrder;

	public bool hideOnWorldHealthBar;

	public GameObject startEffectVictim;

	public GameObject endEffectVictim;

	public Material[] addedMaterials;

	[SaveVar(SaveVarFlags.Default)]
	[SyncVar]
	private float? _maxDuration;

	[SaveVar(SaveVarFlags.Default)]
	[SyncVar(hook = "OnRemainingDurationChanged")]
	private float? _remainingDuration;

	private const float DurationSyncGranularity = 0.1f;

	private float _durationTickRemainder;

	private float _clientDurationElapsed;

	[CompilerGenerated]
	[SyncVar]
	private int? numberDisplay__BackingField;

	private const bool DoOrphanCheckOnlyWithSkillTriggers = true;

	private bool _doOrphanCheck;

	private AbilityTrigger _orphanCheckParentTrigger;

	private OnScreenTimerHandle _locallyShownTimer;

	private bool _isIconShown;

	public const BasicEffectMask HarmfulEffects = BasicEffectMask.Slow | BasicEffectMask.Cripple | BasicEffectMask.Root | BasicEffectMask.Silence | BasicEffectMask.Stun | BasicEffectMask.Blind | BasicEffectMask.ArmorReduction | BasicEffectMask.HealReduction;

	private List<BasicEffect> _basicEffects = new List<BasicEffect>();

	private List<StatBonus> _statBonuses = new List<StatBonus>();

	private List<DoAbilityItem> _doAbilityItems = new List<DoAbilityItem>();

	private Dictionary<AbilityTrigger, SafeAction> _doAbilityPendingCleanups = new Dictionary<AbilityTrigger, SafeAction>();

	private Action<int, AbilityTrigger> _cachedOnAbilityAdded;

	private Action<int, AbilityTrigger> _cachedOnAbilityRemoved;

	private Action<Actor> _cachedDoAbilityCleanup;

	protected NetworkBehaviourSyncVar ____victimNetId;

	public Action<bool, bool> _Mirror_SyncVarHookDelegate__showIcon;

	public Action<float?, float?> _Mirror_SyncVarHookDelegate__remainingDuration;

	public override bool isDestroyedOnRoomChange => false;

	[SaveVar(SaveVarFlags.Default)]
	public Entity victim
	{
		get
		{
			return Network_victim;
		}
		internal set
		{
			Network_victim = value;
		}
	}

	public bool showIcon
	{
		get
		{
			return _showIcon;
		}
		set
		{
			Network_showIcon = value;
		}
	}

	public float? maxDuration => _maxDuration;

	public float? remainingDuration
	{
		get
		{
			if (!_remainingDuration.HasValue)
			{
				return null;
			}
			if (((NetworkBehaviour)this).isServer)
			{
				return Mathf.Max(_remainingDuration.Value - _durationTickRemainder, 0f);
			}
			return Mathf.Max(_remainingDuration.Value - _clientDurationElapsed, 0f);
		}
	}

	public float? normalizedDuration
	{
		get
		{
			float? num = remainingDuration;
			if (!num.HasValue || !_maxDuration.HasValue)
			{
				return null;
			}
			return num.Value / _maxDuration.Value;
		}
	}

	public int? numberDisplay
	{
		[CompilerGenerated]
		get
		{
			return numberDisplay__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CnumberDisplay_003Ek__BackingField = value;
		}
	}

	public IReadOnlyList<BasicEffect> basicEffects => _basicEffects;

	public Entity Network_victim
	{
		get
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			return ((NetworkBehaviour)this).GetSyncVarNetworkBehaviour<Entity>(____victimNetId, ref _victim);
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter_NetworkBehaviour<Entity>(value, ref _victim, 64uL, (Action<Entity, Entity>)null, ref ____victimNetId);
		}
	}

	public float? NetworknormalizedFillAmount
	{
		get
		{
			return normalizedFillAmount;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float?>(value, ref normalizedFillAmount, 128uL, (Action<float?, float?>)null);
		}
	}

	public bool Network_showIcon
	{
		get
		{
			return _showIcon;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref _showIcon, 256uL, _Mirror_SyncVarHookDelegate__showIcon);
		}
	}

	public float? Network_maxDuration
	{
		get
		{
			return _maxDuration;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float?>(value, ref _maxDuration, 512uL, (Action<float?, float?>)null);
		}
	}

	public float? Network_remainingDuration
	{
		get
		{
			return _remainingDuration;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float?>(value, ref _remainingDuration, 1024uL, _Mirror_SyncVarHookDelegate__remainingDuration);
		}
	}

	public int? Network_003CnumberDisplay_003Ek__BackingField
	{
		get
		{
			return numberDisplay__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<int?>(value, ref numberDisplay__BackingField, 2048uL, (Action<int?, int?>)null);
		}
	}

	private void OnRemainingDurationChanged(float? oldVal, float? newVal)
	{
		_clientDurationElapsed = 0f;
	}

	[Server]
	public StatBonus DoStatBonus()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'StatBonus StatusEffect::DoStatBonus()' called when server was not active");
			return null;
		}
		if (!isActive)
		{
			return null;
		}
		StatBonus statBonus = new StatBonus();
		_statBonuses.Add(statBonus);
		victim.Status.AddStatBonus(statBonus);
		return statBonus;
	}

	[Server]
	public StatBonus DoStatBonus(StatBonus bonus)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'StatBonus StatusEffect::DoStatBonus(StatBonus)' called when server was not active");
			return null;
		}
		if (!isActive)
		{
			return null;
		}
		if (bonus == null)
		{
			bonus = new StatBonus();
		}
		_statBonuses.Add(bonus);
		victim.Status.AddStatBonus(bonus);
		return bonus;
	}

	public override void OnStartServer()
	{
		base.OnStartServer();
		if (!victim.Status.statusEffects.Contains(this))
		{
			victim.Status.statusEffects.Add(this);
			victim.ClientEntityEvent_OnStatusEffectAdded?.Invoke(new EventInfoStatusEffect
			{
				victim = victim,
				effect = this
			});
		}
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		OnCreate_DoAbility();
		OnShowIconChanged(oldVal: false, _showIcon);
		if (isActive && startEffectVictim != null)
		{
			FxPlay(startEffectVictim, victim);
		}
		if (isActive && addedMaterials != null)
		{
			Material[] array = addedMaterials;
			foreach (Material material in array)
			{
				victim.Visual.AddSharedMaterialLocal(material);
			}
		}
		_doOrphanCheck = false;
		_orphanCheckParentTrigger = null;
		AbilityTrigger abilityTrigger = firstTrigger;
		if ((UnityEngine.Object)(object)abilityTrigger != null && abilityTrigger is SkillTrigger && (UnityEngine.Object)(object)gem == null)
		{
			_doOrphanCheck = true;
			_orphanCheckParentTrigger = abilityTrigger;
		}
		Entity entity = victim;
		if ((UnityEngine.Object)(object)entity != null && (UnityEngine.Object)(object)entity.Status != null && entity.Status.statusEffects != null && !entity.Status.statusEffects.Contains(this))
		{
			entity.Status.statusEffects.Add(this);
			entity.ClientEntityEvent_OnStatusEffectAdded?.Invoke(new EventInfoStatusEffect
			{
				victim = entity,
				effect = this
			});
		}
	}

	public override void FrameUpdate()
	{
		base.FrameUpdate();
		if (isActive && ((NetworkBehaviour)this).isServer)
		{
			if ((UnityEngine.Object)(object)victim == null || !victim.isActive || (isKilledByCrowdControlImmunity && victim.Status.hasCrowdControlImmunity))
			{
				NetworkedManagerBase<ClientEventManager>.instance.InvokeOnIgnoreCC(victim);
				Destroy();
			}
			else if (_remainingDuration.HasValue)
			{
				float num = Time.deltaTime;
				if (scaleDurationByTenacity)
				{
					num *= 1f + victim.Status.tenacity / 100f;
				}
				_durationTickRemainder += num;
				if (_remainingDuration.Value - _durationTickRemainder <= 0f)
				{
					Network_remainingDuration = 0f;
					_durationTickRemainder = 0f;
					StopTimer();
					Destroy();
				}
				else if (_durationTickRemainder >= 0.1f)
				{
					Network_remainingDuration = Mathf.MoveTowards(_remainingDuration.Value, 0f, _durationTickRemainder);
					_durationTickRemainder = 0f;
				}
			}
		}
		else if (isActive && _remainingDuration.HasValue)
		{
			float num2 = Time.deltaTime;
			if (scaleDurationByTenacity && (UnityEngine.Object)(object)victim != null)
			{
				num2 *= 1f + victim.Status.tenacity / 100f;
			}
			_clientDurationElapsed += num2;
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer && _doOrphanCheck && (_orphanCheckParentTrigger.IsNullOrInactive() || _orphanCheckParentTrigger.owner.IsNullOrInactive()))
		{
			Destroy();
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		Network_maxDuration = null;
		Network_remainingDuration = null;
		_durationTickRemainder = 0f;
		_clientDurationElapsed = 0f;
		NetworknormalizedFillAmount = null;
		Network_003CnumberDisplay_003Ek__BackingField = null;
	}

	protected override void OnDestroyActor()
	{
		try
		{
			base.OnDestroyActor();
		}
		catch (Exception exception)
		{
			Debug.LogException(exception, (UnityEngine.Object)(object)this);
		}
		try
		{
			OnDestroyActor_DoAbility();
		}
		catch (Exception exception2)
		{
			Debug.LogException(exception2, (UnityEngine.Object)(object)this);
		}
		if (_isIconShown)
		{
			_isIconShown = false;
		}
		if (_locallyShownTimer != null)
		{
			try
			{
				NetworkedManagerBase<ClientEventManager>.instance.OnHideOnScreenTimer?.Invoke(_locallyShownTimer);
			}
			catch (Exception exception3)
			{
				Debug.LogException(exception3);
			}
			_locallyShownTimer = null;
		}
		try
		{
			if (endEffectVictim != null)
			{
				FxPlay(endEffectVictim, victim);
			}
			if (startEffectVictim != null)
			{
				FxStop(startEffectVictim);
			}
		}
		catch (Exception exception4)
		{
			Debug.LogException(exception4, (UnityEngine.Object)(object)this);
		}
		if (!((UnityEngine.Object)(object)victim != null))
		{
			return;
		}
		if (((NetworkBehaviour)this).isServer)
		{
			foreach (BasicEffect basicEffect in _basicEffects)
			{
				try
				{
					victim.Status.RemoveBasicEffect(basicEffect);
				}
				catch (Exception exception5)
				{
					Debug.LogException(exception5, (UnityEngine.Object)(object)this);
				}
			}
			_basicEffects.Clear();
			foreach (StatBonus statBonuse in _statBonuses)
			{
				try
				{
					victim.Status.RemoveStatBonus(statBonuse);
				}
				catch (Exception exception6)
				{
					Debug.LogException(exception6, (UnityEngine.Object)(object)this);
				}
			}
			_statBonuses.Clear();
		}
		try
		{
			if (addedMaterials != null)
			{
				Material[] array = addedMaterials;
				foreach (Material material in array)
				{
					victim.Visual.RemoveMaterialLocal(material);
				}
			}
		}
		catch (Exception exception7)
		{
			Debug.LogException(exception7, (UnityEngine.Object)(object)this);
		}
		victim.Status.statusEffects.Remove(this);
		victim.ClientEntityEvent_OnStatusEffectRemoved?.Invoke(new EventInfoStatusEffect
		{
			victim = victim,
			effect = this
		});
	}

	[Server]
	public void SetTimer(float duration)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void StatusEffect::SetTimer(System.Single)' called when server was not active");
			return;
		}
		Network_maxDuration = duration;
		Network_remainingDuration = duration;
		_durationTickRemainder = 0f;
	}

	[Server]
	public void ShowOnScreenTimer(string customNameKey = null, Color color = default(Color), bool invertValue = false)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void StatusEffect::ShowOnScreenTimer(System.String,UnityEngine.Color,System.Boolean)' called when server was not active");
		}
		else if (!((UnityEngine.Object)(object)victim == null) && !((UnityEngine.Object)(object)victim.owner == null) && ((NetworkBehaviour)victim.owner).connectionToClient != null)
		{
			if (((NetworkBehaviour)victim).isOwned)
			{
				SetOnScreenTimerLocal(value: true, customNameKey, color, invertValue);
			}
			else
			{
				TpcSetOnScreenTimer(((NetworkBehaviour)victim.owner).connectionToClient, value: true, customNameKey, color, invertValue);
			}
		}
	}

	[Server]
	public void HideOnScreenTimer()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void StatusEffect::HideOnScreenTimer()' called when server was not active");
		}
		else if (!((UnityEngine.Object)(object)victim == null) && !((UnityEngine.Object)(object)victim.owner == null) && ((NetworkBehaviour)victim.owner).connectionToClient != null)
		{
			if (((NetworkBehaviour)victim).isOwned)
			{
				SetOnScreenTimerLocal(value: false, null, default, invertValue: false);
			}
			else
			{
				TpcSetOnScreenTimer(((NetworkBehaviour)victim.owner).connectionToClient, value: false, null, default, invertValue: false);
			}
		}
	}

	[TargetRpc]
	private void TpcSetOnScreenTimer(NetworkConnectionToClient target, bool value, string customNameKey, Color color, bool invertValue)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteBool((NetworkWriter)(object)val, value);
		NetworkWriterExtensions.WriteString((NetworkWriter)(object)val, customNameKey);
		NetworkWriterExtensions.WriteColor((NetworkWriter)(object)val, color);
		NetworkWriterExtensions.WriteBool((NetworkWriter)(object)val, invertValue);
		((NetworkBehaviour)this).SendTargetRPCInternal((NetworkConnection)(object)target, "System.Void StatusEffect::TpcSetOnScreenTimer(Mirror.NetworkConnectionToClient,System.Boolean,System.String,UnityEngine.Color,System.Boolean)", 1072093018, (NetworkWriter)(object)val, 0);
		NetworkWriterPool.Return(val);
	}

	private void SetOnScreenTimerLocal(bool value, string customNameKey, Color color, bool invertValue)
	{
		if (!isActive)
		{
			return;
		}
		if (_locallyShownTimer != null)
		{
			try
			{
				NetworkedManagerBase<ClientEventManager>.instance.OnHideOnScreenTimer?.Invoke(_locallyShownTimer);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
			_locallyShownTimer = null;
		}
		if (!value)
		{
			return;
		}
		string value2;
		if (!string.IsNullOrEmpty(customNameKey))
		{
			if (customNameKey.StartsWith("St_"))
			{
				value2 = DewLocalization.GetSkillName(DewLocalization.GetSkillKey(customNameKey), 0);
			}
			else if (customNameKey.StartsWith("Se_Star_"))
			{
				value2 = DewLocalization.GetStarName(customNameKey);
			}
			else if (customNameKey.StartsWith("Se_Curse_"))
			{
				value2 = DewLocalization.GetCurseName(DewLocalization.GetCurseKey(customNameKey));
			}
			else if (customNameKey.StartsWith("Gem_"))
			{
				value2 = DewLocalization.GetGemName(DewLocalization.GetGemKey(customNameKey));
			}
			else
			{
				value2 = ((!customNameKey.StartsWith("Treasure_")) ? DewLocalization.GetUIValue(customNameKey) : DewLocalization.GetTreasureName(DewLocalization.GetTreasureKey(customNameKey)));
			}
		}
		else if (!DewLocalization.TryGetUIValue(((object)this).GetType().Name + "_Name", out value2))
		{
			AbilityTrigger abilityTrigger = firstTrigger;
			if ((UnityEngine.Object)(object)abilityTrigger != null && abilityTrigger is SkillTrigger)
			{
				value2 = DewLocalization.GetSkillName(DewLocalization.GetSkillKey(((object)abilityTrigger).GetType()), 0);
			}
		}
		try
		{
			OnScreenTimerHandle arg = (_locallyShownTimer = new OnScreenTimerHandle
			{
				rawText = value2,
				fillAmountGetter = () =>
				{
					float num = 1f;
					if (normalizedDuration.HasValue)
					{
						num = normalizedDuration.Value;
					}
					else if (this is StackedStatusEffect { autoDecay: not false } stackedStatusEffect)
					{
						num = stackedStatusEffect.remainingDecayTime / stackedStatusEffect.decayTime;
					}
					if (invertValue)
					{
						num = 1f - num;
					}
					return num;
				},
				color = color
			});
			NetworkedManagerBase<ClientEventManager>.instance.OnShowOnScreenTimer?.Invoke(arg);
		}
		catch (Exception exception2)
		{
			Debug.LogException(exception2);
		}
	}

	[Server]
	public void SetTimer(float maxDuration, float duration)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void StatusEffect::SetTimer(System.Single,System.Single)' called when server was not active");
			return;
		}
		Network_maxDuration = maxDuration;
		Network_remainingDuration = duration;
		_durationTickRemainder = 0f;
	}

	[Server]
	public void ResetTimer()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void StatusEffect::ResetTimer()' called when server was not active");
			return;
		}
		Network_remainingDuration = _maxDuration;
		_durationTickRemainder = 0f;
	}

	[Server]
	public void StopTimer()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void StatusEffect::StopTimer()' called when server was not active");
			return;
		}
		if (!_maxDuration.HasValue)
		{
			Debug.LogWarning($"Tried to stop a timer when maxDuration was not set: {this}");
		}
		if (!_remainingDuration.HasValue)
		{
			Debug.LogWarning($"Tried to stop a timer when maxDuration was not set: {this}");
		}
		Network_maxDuration = null;
		Network_remainingDuration = null;
		_durationTickRemainder = 0f;
	}

	public override string GetActorReadableName()
	{
		return base.GetActorReadableName() + " " + (((UnityEngine.Object)(object)victim != null) ? $" on [{victim.persistentNetId}]" : "");
	}

	private void OnShowIconChanged(bool oldVal, bool newVal)
	{
		Entity entity = victim;
		if ((UnityEngine.Object)(object)entity == null)
		{
			return;
		}
		try
		{
			if (newVal && !_isIconShown)
			{
				_isIconShown = true;
				entity.Status.ClientEvent_OnShowStatusEffectIcon?.Invoke(this);
			}
			else if (!newVal && _isIconShown)
			{
				_isIconShown = false;
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	public virtual string GetCustomTooltipName()
	{
		return null;
	}

	public virtual string GetCustomTooltipDescription()
	{
		return null;
	}

	public void DoBasicEffect(BasicEffect eff)
	{
		if (!((UnityEngine.Object)(object)victim == null))
		{
			eff.victim = victim;
			eff.parent = this;
			if (isActive)
			{
				victim.Status.AddBasicEffect(eff);
				_basicEffects.Add(eff);
			}
		}
	}

	private T DoBasicEffect<T>() where T : BasicEffect, new()
	{
		T val = new T
		{
			victim = victim,
			parent = this
		};
		if (isActive)
		{
			victim.Status.AddBasicEffect(val);
			_basicEffects.Add(val);
		}
		return val;
	}

	private T DoBasicEffectNoRegister<T>() where T : BasicEffect, new()
	{
		return new T
		{
			victim = victim,
			parent = this
		};
	}

	private void RegisterBasicEffect<T>(T eff) where T : BasicEffect, new()
	{
		if (isActive)
		{
			victim.Status.AddBasicEffect(eff);
			_basicEffects.Add(eff);
		}
	}

	private T DoBasicEffectWithStrength<T>(float strength) where T : BasicEffectWithStrength, new()
	{
		T val = DoBasicEffectNoRegister<T>();
		val.strength = strength;
		RegisterBasicEffect(val);
		return val;
	}

	[Server]
	public SlowEffect DoSlow(float strength)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'SlowEffect StatusEffect::DoSlow(System.Single)' called when server was not active");
			return null;
		}
		return DoBasicEffectWithStrength<SlowEffect>(strength);
	}

	[Server]
	public SpeedEffect DoSpeed(float strength)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'SpeedEffect StatusEffect::DoSpeed(System.Single)' called when server was not active");
			return null;
		}
		return DoBasicEffectWithStrength<SpeedEffect>(strength);
	}

	[Server]
	public HasteEffect DoHaste(float strength)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'HasteEffect StatusEffect::DoHaste(System.Single)' called when server was not active");
			return null;
		}
		return DoBasicEffectWithStrength<HasteEffect>(strength);
	}

	[Server]
	public CrippleEffect DoCripple(float strength)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'CrippleEffect StatusEffect::DoCripple(System.Single)' called when server was not active");
			return null;
		}
		return DoBasicEffectWithStrength<CrippleEffect>(strength);
	}

	[Server]
	public RootEffect DoRoot()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'RootEffect StatusEffect::DoRoot()' called when server was not active");
			return null;
		}
		return DoBasicEffect<RootEffect>();
	}

	[Server]
	public SilenceEffect DoSilence()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'SilenceEffect StatusEffect::DoSilence()' called when server was not active");
			return null;
		}
		return DoBasicEffect<SilenceEffect>();
	}

	[Server]
	public StunEffect DoStun()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'StunEffect StatusEffect::DoStun()' called when server was not active");
			return null;
		}
		return DoBasicEffect<StunEffect>();
	}

	[Server]
	public BlindEffect DoBlind()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'BlindEffect StatusEffect::DoBlind()' called when server was not active");
			return null;
		}
		return DoBasicEffect<BlindEffect>();
	}

	[Server]
	public AttackCriticalEffect DoAttackCritical(Action onUse)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'AttackCriticalEffect StatusEffect::DoAttackCritical(System.Action)' called when server was not active");
			return null;
		}
		AttackCriticalEffect attackCriticalEffect = DoBasicEffectNoRegister<AttackCriticalEffect>();
		attackCriticalEffect.onUse = onUse;
		RegisterBasicEffect(attackCriticalEffect);
		return attackCriticalEffect;
	}

	[Server]
	public AttackOverrideEffect DoAttackOverride(AbilityTrigger trigger, Action onUse)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'AttackOverrideEffect StatusEffect::DoAttackOverride(AbilityTrigger,System.Action)' called when server was not active");
			return null;
		}
		AttackOverrideEffect attackOverrideEffect = DoBasicEffectNoRegister<AttackOverrideEffect>();
		attackOverrideEffect.trigger = trigger;
		attackOverrideEffect.onUse = onUse;
		attackOverrideEffect._shouldTriggerBeDisposed = false;
		RegisterBasicEffect(attackOverrideEffect);
		return attackOverrideEffect;
	}

	[Server]
	public AttackOverrideEffect DoAttackOverride<T>(Action onUse) where T : AbilityTrigger
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'AttackOverrideEffect StatusEffect::DoAttackOverride(System.Action)' called when server was not active");
			return null;
		}
		AttackOverrideEffect attackOverrideEffect = DoBasicEffectNoRegister<AttackOverrideEffect>();
		attackOverrideEffect.trigger = Dew.CreateAbilityTrigger<T>();
		attackOverrideEffect.onUse = onUse;
		attackOverrideEffect._shouldTriggerBeDisposed = true;
		RegisterBasicEffect(attackOverrideEffect);
		return attackOverrideEffect;
	}

	[Server]
	public AttackEmpowerEffect DoAttackEmpower(Action<EventInfoAttackEffect, int> onAttackEffect, int maxTriggerCount = int.MaxValue, Action onDepleted = null)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'AttackEmpowerEffect StatusEffect::DoAttackEmpower(System.Action`2<EventInfoAttackEffect,System.Int32>,System.Int32,System.Action)' called when server was not active");
			return null;
		}
		AttackEmpowerEffect attackEmpowerEffect = DoBasicEffectNoRegister<AttackEmpowerEffect>();
		attackEmpowerEffect.onAttackEffect = onAttackEffect;
		attackEmpowerEffect.onDepleted = onDepleted;
		attackEmpowerEffect.maxTriggerCount = maxTriggerCount;
		RegisterBasicEffect(attackEmpowerEffect);
		return attackEmpowerEffect;
	}

	[Server]
	public InvulnerableEffect DoInvulnerable(Action<EventInfoDamageNegatedByImmunity> onDamageNegated = null)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'InvulnerableEffect StatusEffect::DoInvulnerable(System.Action`1<EventInfoDamageNegatedByImmunity>)' called when server was not active");
			return null;
		}
		InvulnerableEffect invulnerableEffect = DoBasicEffectNoRegister<InvulnerableEffect>();
		invulnerableEffect.onDamageNegated = onDamageNegated;
		RegisterBasicEffect(invulnerableEffect);
		return invulnerableEffect;
	}

	[Server]
	public UnstoppableEffect DoUnstoppable()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'UnstoppableEffect StatusEffect::DoUnstoppable()' called when server was not active");
			return null;
		}
		return DoBasicEffect<UnstoppableEffect>();
	}

	[Server]
	public UntargetableEffect DoUntargetable()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'UntargetableEffect StatusEffect::DoUntargetable()' called when server was not active");
			return null;
		}
		return DoBasicEffect<UntargetableEffect>();
	}

	[Server]
	public InvisibleEffect DoInvisible(bool ignoreReveal = false)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'InvisibleEffect StatusEffect::DoInvisible(System.Boolean)' called when server was not active");
			return null;
		}
		InvisibleEffect invisibleEffect = DoBasicEffectNoRegister<InvisibleEffect>();
		invisibleEffect.ignoreReveal = ignoreReveal;
		RegisterBasicEffect(invisibleEffect);
		return invisibleEffect;
	}

	[Server]
	public RevealEffect DoReveal()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'RevealEffect StatusEffect::DoReveal()' called when server was not active");
			return null;
		}
		return DoBasicEffect<RevealEffect>();
	}

	[Server]
	public ProtectedEffect DoProtected(Action<EventInfoDamageNegatedByImmunity> onDamageNegated)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'ProtectedEffect StatusEffect::DoProtected(System.Action`1<EventInfoDamageNegatedByImmunity>)' called when server was not active");
			return null;
		}
		ProtectedEffect protectedEffect = DoBasicEffectNoRegister<ProtectedEffect>();
		protectedEffect.onDamageNegated = onDamageNegated;
		RegisterBasicEffect(protectedEffect);
		return protectedEffect;
	}

	[Server]
	public UncollidableEffect DoUncollidable()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'UncollidableEffect StatusEffect::DoUncollidable()' called when server was not active");
			return null;
		}
		return DoBasicEffect<UncollidableEffect>();
	}

	[Server]
	public ArmorBoostEffect DoArmorBoost(float strength)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'ArmorBoostEffect StatusEffect::DoArmorBoost(System.Single)' called when server was not active");
			return null;
		}
		return DoBasicEffectWithStrength<ArmorBoostEffect>(strength);
	}

	[Server]
	public ArmorReductionEffect DoArmorReduction(float strength)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'ArmorReductionEffect StatusEffect::DoArmorReduction(System.Single)' called when server was not active");
			return null;
		}
		return DoBasicEffectWithStrength<ArmorReductionEffect>(strength);
	}

	[Server]
	public DeathInterruptEffect DoDeathInterrupt(Action<EventInfoKill> onInterrupt, int priority)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'DeathInterruptEffect StatusEffect::DoDeathInterrupt(System.Action`1<EventInfoKill>,System.Int32)' called when server was not active");
			return null;
		}
		DeathInterruptEffect deathInterruptEffect = DoBasicEffectNoRegister<DeathInterruptEffect>();
		deathInterruptEffect.onInterrupt = onInterrupt;
		deathInterruptEffect.priority = priority;
		RegisterBasicEffect(deathInterruptEffect);
		return deathInterruptEffect;
	}

	[Server]
	public ShieldEffect DoShield(float amount, Action<EventInfoDamageNegatedByShield> onDamageNegated = null)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'ShieldEffect StatusEffect::DoShield(System.Single,System.Action`1<EventInfoDamageNegatedByShield>)' called when server was not active");
			return null;
		}
		float originalAmount = amount;
		amount = ProcessShieldAmount(amount, victim);
		ShieldEffect shieldEffect = DoBasicEffectNoRegister<ShieldEffect>();
		shieldEffect.amount = amount;
		shieldEffect.onDamageNegated += onDamageNegated;
		RegisterBasicEffect(shieldEffect);
		if (amount > 0.001f)
		{
			EventInfoShield arg = new EventInfoShield
			{
				originalAmount = originalAmount,
				finalAmount = amount,
				target = victim,
				statusEffect = this,
				chain = chain
			};
			InvokeOnGiveShield(arg);
			victim.EntityEvent_OnTakeShield?.Invoke(arg);
		}
		return shieldEffect;
	}

	[Server]
	public FakeMaxHealthEffect DoFakeMaxHealthEffect(float amount)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'FakeMaxHealthEffect StatusEffect::DoFakeMaxHealthEffect(System.Single)' called when server was not active");
			return null;
		}
		return DoBasicEffectWithStrength<FakeMaxHealthEffect>(amount);
	}

	[Server]
	public void StopAllBasicEffects()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void StatusEffect::StopAllBasicEffects()' called when server was not active");
			return;
		}
		foreach (BasicEffect basicEffect in _basicEffects)
		{
			victim.Status.RemoveBasicEffect(basicEffect);
		}
		_basicEffects.Clear();
	}

	[Server]
	public void StopBasicEffect(BasicEffect eff)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void StatusEffect::StopBasicEffect(BasicEffect)' called when server was not active");
			return;
		}
		if ((UnityEngine.Object)(object)victim != null)
		{
			victim.Status.RemoveBasicEffect(eff);
		}
		_basicEffects.Remove(eff);
	}

	public SafeAction DoAbility(Func<AbilityTrigger, bool> condition, Func<AbilityTrigger, Action> callback)
	{
		DoAbilityItem newItem = new DoAbilityItem
		{
			condition = condition,
			callback = callback,
			stopAction = new SafeAction()
		};
		_doAbilityItems.Add(newItem);
		newItem.stopAction += (Action)(() =>
		{
			_doAbilityItems.Remove(newItem);
		});
		if ((UnityEngine.Object)(object)victim == null)
		{
			return newItem.stopAction;
		}
		foreach (AbilityTrigger value in victim.Ability.abilities.Values)
		{
			newItem.stopAction += TryDoAbility(newItem, value);
		}
		return newItem.stopAction;
	}

	private Action TryDoAbility(DoAbilityItem item, AbilityTrigger at)
	{
		try
		{
			if (item.condition != null && !item.condition(at))
			{
				return null;
			}
			Action cleanup = item.callback(at);
			if (cleanup == null)
			{
				return null;
			}
			if (!_doAbilityPendingCleanups.ContainsKey(at))
			{
				_doAbilityPendingCleanups[at] = new SafeAction();
				at.ClientActorEvent_OnDestroyed += new Action<Actor>(DoAbilityCleanup);
			}
			SafeAction actionContainer = _doAbilityPendingCleanups[at];
			actionContainer += cleanup;
			return () =>
			{
				if (actionContainer.Contains(cleanup))
				{
					actionContainer -= cleanup;
					cleanup();
				}
			};
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			return null;
		}
	}

	public SafeAction DoSkill<T>(Func<T, Action> callback) where T : SkillTrigger
	{
		return DoAbility((AbilityTrigger at) => at is T, (AbilityTrigger at) => callback((T)at));
	}

	public SafeAction DoSkill<T>(Action<T> callback) where T : SkillTrigger
	{
		return DoAbility((AbilityTrigger at) => at is T, (AbilityTrigger at) =>
		{
			callback((T)at);
			return (Action)null;
		});
	}

	private void DoAbilityCleanup(Actor ac)
	{
		DoAbilityCleanup(ac as AbilityTrigger);
	}

	private void DoAbilityCleanup(AbilityTrigger at)
	{
		if (!((UnityEngine.Object)(object)at == null) && _doAbilityPendingCleanups.TryGetValue(at, out var value))
		{
			value.Invoke();
			at.ClientActorEvent_OnDestroyed -= _cachedDoAbilityCleanup;
			_doAbilityPendingCleanups.Remove(at);
		}
	}

	private void OnCreate_DoAbility()
	{
		if ((UnityEngine.Object)(object)victim != null)
		{
			victim.Ability.ClientEvent_OnAbilityAdded += new Action<int, AbilityTrigger>(ClientEventOnAbilityAdded);
			victim.Ability.ClientEvent_OnAbilityRemoved += new Action<int, AbilityTrigger>(ClientEventOnAbilityRemoved);
		}
	}

	private void ClientEventOnAbilityAdded(int arg1, AbilityTrigger arg2)
	{
		for (int i = 0; i < _doAbilityItems.Count; i++)
		{
			_doAbilityItems[i].stopAction += TryDoAbility(_doAbilityItems[i], arg2);
		}
	}

	private void ClientEventOnAbilityRemoved(int arg1, AbilityTrigger arg2)
	{
		DoAbilityCleanup(arg2);
	}

	private void OnDestroyActor_DoAbility()
	{
		if ((UnityEngine.Object)(object)victim != null)
		{
			victim.Ability.ClientEvent_OnAbilityAdded -= _cachedOnAbilityAdded;
			victim.Ability.ClientEvent_OnAbilityRemoved -= _cachedOnAbilityRemoved;
		}
		_doAbilityItems.Clear();
		List<AbilityTrigger> list = DewPool.GetList(out ListReturnHandle<AbilityTrigger> handle);
		foreach (KeyValuePair<AbilityTrigger, SafeAction> doAbilityPendingCleanup in _doAbilityPendingCleanups)
		{
			list.Add(doAbilityPendingCleanup.Key);
		}
		foreach (AbilityTrigger item in list)
		{
			DoAbilityCleanup(item);
		}
		handle.Return();
		_doAbilityPendingCleanups.Clear();
	}

	protected StatusEffect()
	{
		_Mirror_SyncVarHookDelegate__showIcon = OnShowIconChanged;
		_Mirror_SyncVarHookDelegate__remainingDuration = OnRemainingDurationChanged;
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_TpcSetOnScreenTimer__NetworkConnectionToClient__Boolean__String__Color__Boolean(NetworkConnectionToClient target, bool value, string customNameKey, Color color, bool invertValue)
	{
		SetOnScreenTimerLocal(value, customNameKey, color, invertValue);
	}

	protected static void InvokeUserCode_TpcSetOnScreenTimer__NetworkConnectionToClient__Boolean__String__Color__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TpcSetOnScreenTimer called on server.");
		}
		else
		{
			((StatusEffect)(object)obj).UserCode_TpcSetOnScreenTimer__NetworkConnectionToClient__Boolean__String__Color__Boolean((NetworkConnectionToClient)(object)NetworkClient.connection, NetworkReaderExtensions.ReadBool(reader), NetworkReaderExtensions.ReadString(reader), NetworkReaderExtensions.ReadColor(reader), NetworkReaderExtensions.ReadBool(reader));
		}
	}

	static StatusEffect()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterRpc(typeof(StatusEffect), "System.Void StatusEffect::TpcSetOnScreenTimer(Mirror.NetworkConnectionToClient,System.Boolean,System.String,UnityEngine.Color,System.Boolean)", (RemoteCallDelegate)InvokeUserCode_TpcSetOnScreenTimer__NetworkConnectionToClient__Boolean__String__Color__Boolean);
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)Network_victim);
			writer.WriteNullableFloat(normalizedFillAmount);
			NetworkWriterExtensions.WriteBool(writer, _showIcon);
			writer.WriteNullableFloat(_maxDuration);
			writer.WriteNullableFloat(_remainingDuration);
			NetworkWriterExtensions.WriteIntNullable(writer, numberDisplay__BackingField);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x40L) != 0L)
		{
			NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)Network_victim);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x80L) != 0L)
		{
			writer.WriteNullableFloat(normalizedFillAmount);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x100L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, _showIcon);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x200L) != 0L)
		{
			writer.WriteNullableFloat(_maxDuration);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x400L) != 0L)
		{
			writer.WriteNullableFloat(_remainingDuration);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x800L) != 0L)
		{
			NetworkWriterExtensions.WriteIntNullable(writer, numberDisplay__BackingField);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize_NetworkBehaviour<Entity>(ref _victim, (Action<Entity, Entity>)null, reader, ref ____victimNetId);
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float?>(ref normalizedFillAmount, (Action<float?, float?>)null, reader.ReadNullableFloat());
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref _showIcon, _Mirror_SyncVarHookDelegate__showIcon, NetworkReaderExtensions.ReadBool(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float?>(ref _maxDuration, (Action<float?, float?>)null, reader.ReadNullableFloat());
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float?>(ref _remainingDuration, _Mirror_SyncVarHookDelegate__remainingDuration, reader.ReadNullableFloat());
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int?>(ref numberDisplay__BackingField, (Action<int?, int?>)null, NetworkReaderExtensions.ReadIntNullable(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x40L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize_NetworkBehaviour<Entity>(ref _victim, (Action<Entity, Entity>)null, reader, ref ____victimNetId);
		}
		if ((num & 0x80L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float?>(ref normalizedFillAmount, (Action<float?, float?>)null, reader.ReadNullableFloat());
		}
		if ((num & 0x100L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref _showIcon, _Mirror_SyncVarHookDelegate__showIcon, NetworkReaderExtensions.ReadBool(reader));
		}
		if ((num & 0x200L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float?>(ref _maxDuration, (Action<float?, float?>)null, reader.ReadNullableFloat());
		}
		if ((num & 0x400L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float?>(ref _remainingDuration, _Mirror_SyncVarHookDelegate__remainingDuration, reader.ReadNullableFloat());
		}
		if ((num & 0x800L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int?>(ref numberDisplay__BackingField, (Action<int?, int?>)null, NetworkReaderExtensions.ReadIntNullable(reader));
		}
	}
}
