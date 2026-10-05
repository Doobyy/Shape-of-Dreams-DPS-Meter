using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class EntityStatus : EntityComponent, ICleanup
{
	public const float MinimumArmorValue = -300f;

	public const float MaximumArmorValue = 100000f;

	public SafeAction<StatusEffect> ClientEvent_OnShowStatusEffectIcon;

	[CompilerGenerated]
	[SyncVar]
	private bool isInConversation__BackingField;

	[SyncVar]
	private int _isHealthHiddenCounter;

	private float _healthRegenRemainder;

	private float _manaRegenRemainder;

	[SyncVar(hook = "OnLevelChanged")]
	[SerializeField]
	private int _level;

	public string manaTypeKey = "";

	[SyncVar(hook = "OnBaseStatsChanged")]
	public BaseStats baseStats = BaseStats.Default;

	[SyncVar(hook = "OnScalingStatsChanged")]
	public BonusStats scalingStats = BonusStats.Default;

	[CompilerGenerated]
	[SyncVar]
	private Vector2 specialFill__BackingField;

	[NonSerialized]
	[SyncVar]
	public BonusStats bonusStats = BonusStats.Default;

	[NonSerialized]
	[SyncVar]
	private float _currentHealth;

	[NonSerialized]
	[SyncVar]
	private float _currentMana;

	[CompilerGenerated]
	[SyncVar]
	private CounterBool isSectionTriggeringDisabled__BackingField;

	[CompilerGenerated]
	[SyncVar]
	private float mirageSkinInitAmount__BackingField;

	[NonSerialized]
	[SyncVar]
	private float _currentShield;

	[CompilerGenerated]
	[SyncVar(hook = "OnFinalStatsChanged")]
	private FinalStats finalStats__BackingField;

	public DataProcessorGroup<FinalStats> finalStatsProcessors = new DataProcessorGroup<FinalStats>();

	public SafeAction ClientEvent_OnFinalStatsChanged;

	public SafeAction ClientEvent_OnStatsCalculated;

	private readonly List<StatBonus> _grantedStatBonuses = new List<StatBonus>();

	private bool _areStatsDirty = true;

	private const BasicEffectMask SuppressedByCrowdControlImmunity = BasicEffectMask.Slow | BasicEffectMask.Cripple | BasicEffectMask.Root | BasicEffectMask.Silence | BasicEffectMask.Stun | BasicEffectMask.Blind;

	public readonly List<StatusEffect> statusEffects = new List<StatusEffect>();

	[CompilerGenerated]
	[SyncVar]
	private bool isUndetectableByNonAllies__BackingField;

	[CompilerGenerated]
	[SyncVar]
	private bool isInvisibilityRevealed__BackingField;

	internal List<BasicEffect> _basicEffects = new List<BasicEffect>();

	[CompilerGenerated]
	[SyncVar]
	private BasicEffectMask basicEffectMask__BackingField;

	[NonSerialized]
	[SyncVar]
	internal float _totalSlow;

	[NonSerialized]
	[SyncVar]
	internal float _totalSpeed;

	[NonSerialized]
	[SyncVar]
	internal float _totalHaste;

	[NonSerialized]
	[SyncVar]
	internal float _totalCripple;

	[NonSerialized]
	[SyncVar]
	internal float _totalFakeMaxHealth;

	[NonSerialized]
	[SyncVar]
	internal AbilityTrigger _overridenAttack;

	[NonSerialized]
	[SyncVar]
	internal float _totalArmorFromStatusEffects;

	private bool _isStatusInfoDirty = true;

	private float _lastShieldTick;

	[CompilerGenerated]
	[SyncVar]
	private bool hasCold__BackingField;

	[CompilerGenerated]
	[SyncVar]
	private int fireStack__BackingField;

	[CompilerGenerated]
	[SyncVar]
	private int lightStack__BackingField;

	[CompilerGenerated]
	[SyncVar]
	private int darkStack__BackingField;

	private Dictionary<BasicEffect, object> _registeredHandlers = new Dictionary<BasicEffect, object>();

	internal BasicEffect _lastDamagePreventer;

	protected NetworkBehaviourSyncVar ____overridenAttackNetId;

	public Action<int, int> _Mirror_SyncVarHookDelegate__level;

	public Action<BaseStats, BaseStats> _Mirror_SyncVarHookDelegate_baseStats;

	public Action<BonusStats, BonusStats> _Mirror_SyncVarHookDelegate_scalingStats;

	public Action<FinalStats, FinalStats> _Mirror_SyncVarHookDelegate__003CfinalStats_003Ek__BackingField;

	public bool isInConversation
	{
		[CompilerGenerated]
		get
		{
			return isInConversation__BackingField;
		}
		[CompilerGenerated]
		internal set
		{
			Network_003CisInConversation_003Ek__BackingField = value;
		}
	}

	public bool isHealthHidden => _isHealthHiddenCounter > 0;

	public float missingHealth => maxHealth - currentHealth;

	public float missingMana => maxMana - currentMana;

	public float normalizedHealth => currentHealth / maxHealth;

	public float normalizedMana => currentMana / maxMana;

	[SaveVar(SaveVarFlags.Default)]
	public int level
	{
		get
		{
			return _level;
		}
		set
		{
			if (!NetworkServer.active)
			{
				throw new Exception("Only server can change entity's level.");
			}
			Network_level = value;
		}
	}

	public Vector2 specialFill
	{
		[CompilerGenerated]
		get
		{
			return specialFill__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CspecialFill_003Ek__BackingField = value;
		}
	}

	[SaveVar(SaveVarFlags.ApplyAfterFrameDelay)]
	public float currentHealth
	{
		get
		{
			return _currentHealth;
		}
		internal set
		{
			Network_currentHealth = value;
		}
	}

	[SaveVar(SaveVarFlags.ApplyAfterFrameDelay)]
	public float currentMana
	{
		get
		{
			return _currentMana;
		}
		internal set
		{
			Network_currentMana = value;
		}
	}

	public bool isAlive
	{
		get
		{
			if (entity.isActive)
			{
				return (UnityEngine.Object)(object)this != null;
			}
			return false;
		}
	}

	public bool isDead
	{
		get
		{
			if (entity.isActive)
			{
				return (UnityEngine.Object)(object)this == null;
			}
			return true;
		}
	}

	bool ICleanup.canDestroy => true;

	public CounterBool isSectionTriggeringDisabled
	{
		[CompilerGenerated]
		get
		{
			return isSectionTriggeringDisabled__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			Network_003CisSectionTriggeringDisabled_003Ek__BackingField = value;
		}
	}

	public float currentShield => _currentShield;

	public float mirageSkinInitAmount
	{
		[CompilerGenerated]
		get
		{
			return mirageSkinInitAmount__BackingField;
		}
		[CompilerGenerated]
		internal set
		{
			Network_003CmirageSkinInitAmount_003Ek__BackingField = value;
		}
	}

	public bool hasMirageSkin => mirageSkinInitAmount > 0f;

	public FinalStats finalStats
	{
		[CompilerGenerated]
		get
		{
			return finalStats__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CfinalStats_003Ek__BackingField = value;
		}
	}

	public float attackDamage => finalStats.attackDamage;

	public float abilityPower => finalStats.abilityPower;

	public float maxHealth => finalStats.maxHealth;

	public float maxHealthWithoutBonus => finalStats.maxHealthWithoutBonus;

	public float maxMana => finalStats.maxMana;

	public float healthRegen => finalStats.healthRegen;

	public float manaRegen => finalStats.manaRegen;

	public float critAmp => finalStats.critAmp;

	public float critChance => finalStats.critChance;

	public float tenacity => finalStats.tenacity;

	public float abilityHaste => finalStats.abilityHaste;

	public float attackSpeedMultiplier => finalStats.attackSpeedMultiplier;

	public float movementSpeedMultiplier => finalStats.movementSpeedMultiplier;

	public float fireEffectAmp => finalStats.fireEffectAmp;

	public float coldEffectAmp => finalStats.coldEffectAmp;

	public float lightEffectAmp => finalStats.lightEffectAmp;

	public float darkEffectAmp => finalStats.darkEffectAmp;

	public float armor => finalStats.armor;

	public bool IsStatsDirty => AreStatsDirty();

	public float totalSlow => _totalSlow;

	public float totalSpeed => _totalSpeed;

	public float totalHaste => _totalHaste;

	public float totalCripple => _totalCripple;

	public float totalFakeMaxHealth => _totalFakeMaxHealth;

	public AbilityTrigger overridenAttack => Network_overridenAttack;

	public float totalArmorFromStatusEffects => _totalArmorFromStatusEffects;

	public bool hasSlow => (basicEffectMask & BasicEffectMask.Slow) != 0;

	public bool hasSpeed => (basicEffectMask & BasicEffectMask.Speed) != 0;

	public bool hasHaste => (basicEffectMask & BasicEffectMask.Haste) != 0;

	public bool hasCripple => (basicEffectMask & BasicEffectMask.Cripple) != 0;

	public bool hasRoot => (basicEffectMask & BasicEffectMask.Root) != 0;

	public bool hasSilence => (basicEffectMask & BasicEffectMask.Silence) != 0;

	public bool hasStun => (basicEffectMask & BasicEffectMask.Stun) != 0;

	public bool hasBlind => (basicEffectMask & BasicEffectMask.Blind) != 0;

	public bool hasAttackCritical => (basicEffectMask & BasicEffectMask.AttackCritical) != 0;

	public bool hasAttackOverride => (basicEffectMask & BasicEffectMask.AttackOverride) != 0;

	public bool hasInvulnerable => (basicEffectMask & BasicEffectMask.Invulnerable) != 0;

	public bool hasUnstoppable => (basicEffectMask & BasicEffectMask.Unstoppable) != 0;

	public bool hasProtected => (basicEffectMask & BasicEffectMask.Protected) != 0;

	public bool hasUncollidable => (basicEffectMask & BasicEffectMask.Uncollidable) != 0;

	public bool hasUntargetable => (basicEffectMask & BasicEffectMask.Untargetable) != 0;

	public bool hasArmorBoost => (basicEffectMask & BasicEffectMask.ArmorBoost) != 0;

	public bool hasArmorReduction => (basicEffectMask & BasicEffectMask.ArmorReduction) != 0;

	public bool hasDeathInterrupt => (basicEffectMask & BasicEffectMask.DeathInterrupt) != 0;

	public bool hasInvisible => (basicEffectMask & BasicEffectMask.Invisible) != 0;

	public bool hasReveal => (basicEffectMask & BasicEffectMask.Reveal) != 0;

	public bool hasStealth { get; internal set; }

	public bool isUndetectableByNonAllies
	{
		[CompilerGenerated]
		get
		{
			return isUndetectableByNonAllies__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			Network_003CisUndetectableByNonAllies_003Ek__BackingField = value;
		}
	}

	public bool isInvisibilityRevealed
	{
		[CompilerGenerated]
		get
		{
			return isInvisibilityRevealed__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			Network_003CisInvisibilityRevealed_003Ek__BackingField = value;
		}
	}

	public bool hasDamageImmunity
	{
		get
		{
			if (!hasInvulnerable)
			{
				return hasProtected;
			}
			return true;
		}
	}

	public bool hasCrowdControlImmunity
	{
		get
		{
			if (!hasUnstoppable)
			{
				return hasInvulnerable;
			}
			return true;
		}
	}

	public bool hasImmobility
	{
		get
		{
			if (!hasRoot)
			{
				return hasStun;
			}
			return true;
		}
	}

	public BasicEffectMask basicEffectMask
	{
		[CompilerGenerated]
		get
		{
			return basicEffectMask__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			Network_003CbasicEffectMask_003Ek__BackingField = value;
		}
	}

	public bool hasCold
	{
		[CompilerGenerated]
		get
		{
			return hasCold__BackingField;
		}
		[CompilerGenerated]
		internal set
		{
			Network_003ChasCold_003Ek__BackingField = value;
		}
	}

	public int fireStack
	{
		[CompilerGenerated]
		get
		{
			return fireStack__BackingField;
		}
		[CompilerGenerated]
		internal set
		{
			Network_003CfireStack_003Ek__BackingField = value;
		}
	}

	public int lightStack
	{
		[CompilerGenerated]
		get
		{
			return lightStack__BackingField;
		}
		[CompilerGenerated]
		internal set
		{
			Network_003ClightStack_003Ek__BackingField = value;
		}
	}

	public int darkStack
	{
		[CompilerGenerated]
		get
		{
			return darkStack__BackingField;
		}
		[CompilerGenerated]
		internal set
		{
			Network_003CdarkStack_003Ek__BackingField = value;
		}
	}

	public bool Network_003CisInConversation_003Ek__BackingField
	{
		get
		{
			return isInConversation__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref isInConversation__BackingField, 1uL, (Action<bool, bool>)null);
		}
	}

	public int Network_isHealthHiddenCounter
	{
		get
		{
			return _isHealthHiddenCounter;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<int>(value, ref _isHealthHiddenCounter, 2uL, (Action<int, int>)null);
		}
	}

	public int Network_level
	{
		get
		{
			return _level;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<int>(value, ref _level, 4uL, _Mirror_SyncVarHookDelegate__level);
		}
	}

	public BaseStats NetworkbaseStats
	{
		get
		{
			return baseStats;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<BaseStats>(value, ref baseStats, 8uL, _Mirror_SyncVarHookDelegate_baseStats);
		}
	}

	public BonusStats NetworkscalingStats
	{
		get
		{
			return scalingStats;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<BonusStats>(value, ref scalingStats, 16uL, _Mirror_SyncVarHookDelegate_scalingStats);
		}
	}

	public Vector2 Network_003CspecialFill_003Ek__BackingField
	{
		get
		{
			return specialFill__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<Vector2>(value, ref specialFill__BackingField, 32uL, (Action<Vector2, Vector2>)null);
		}
	}

	public BonusStats NetworkbonusStats
	{
		get
		{
			return bonusStats;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<BonusStats>(value, ref bonusStats, 64uL, (Action<BonusStats, BonusStats>)null);
		}
	}

	public float Network_currentHealth
	{
		get
		{
			return _currentHealth;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref _currentHealth, 128uL, (Action<float, float>)null);
		}
	}

	public float Network_currentMana
	{
		get
		{
			return _currentMana;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref _currentMana, 256uL, (Action<float, float>)null);
		}
	}

	public CounterBool Network_003CisSectionTriggeringDisabled_003Ek__BackingField
	{
		get
		{
			return isSectionTriggeringDisabled__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<CounterBool>(value, ref isSectionTriggeringDisabled__BackingField, 512uL, (Action<CounterBool, CounterBool>)null);
		}
	}

	public float Network_003CmirageSkinInitAmount_003Ek__BackingField
	{
		get
		{
			return mirageSkinInitAmount__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref mirageSkinInitAmount__BackingField, 1024uL, (Action<float, float>)null);
		}
	}

	public float Network_currentShield
	{
		get
		{
			return _currentShield;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref _currentShield, 2048uL, (Action<float, float>)null);
		}
	}

	public FinalStats Network_003CfinalStats_003Ek__BackingField
	{
		get
		{
			return finalStats__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<FinalStats>(value, ref finalStats__BackingField, 4096uL, _Mirror_SyncVarHookDelegate__003CfinalStats_003Ek__BackingField);
		}
	}

	public bool Network_003CisUndetectableByNonAllies_003Ek__BackingField
	{
		get
		{
			return isUndetectableByNonAllies__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref isUndetectableByNonAllies__BackingField, 8192uL, (Action<bool, bool>)null);
		}
	}

	public bool Network_003CisInvisibilityRevealed_003Ek__BackingField
	{
		get
		{
			return isInvisibilityRevealed__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref isInvisibilityRevealed__BackingField, 16384uL, (Action<bool, bool>)null);
		}
	}

	public BasicEffectMask Network_003CbasicEffectMask_003Ek__BackingField
	{
		get
		{
			return basicEffectMask__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<BasicEffectMask>(value, ref basicEffectMask__BackingField, 32768uL, (Action<BasicEffectMask, BasicEffectMask>)null);
		}
	}

	public float Network_totalSlow
	{
		get
		{
			return _totalSlow;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref _totalSlow, 65536uL, (Action<float, float>)null);
		}
	}

	public float Network_totalSpeed
	{
		get
		{
			return _totalSpeed;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref _totalSpeed, 131072uL, (Action<float, float>)null);
		}
	}

	public float Network_totalHaste
	{
		get
		{
			return _totalHaste;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref _totalHaste, 262144uL, (Action<float, float>)null);
		}
	}

	public float Network_totalCripple
	{
		get
		{
			return _totalCripple;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref _totalCripple, 524288uL, (Action<float, float>)null);
		}
	}

	public float Network_totalFakeMaxHealth
	{
		get
		{
			return _totalFakeMaxHealth;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref _totalFakeMaxHealth, 1048576uL, (Action<float, float>)null);
		}
	}

	public AbilityTrigger Network_overridenAttack
	{
		get
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			return ((NetworkBehaviour)this).GetSyncVarNetworkBehaviour<AbilityTrigger>(____overridenAttackNetId, ref _overridenAttack);
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter_NetworkBehaviour<AbilityTrigger>(value, ref _overridenAttack, 2097152uL, (Action<AbilityTrigger, AbilityTrigger>)null, ref ____overridenAttackNetId);
		}
	}

	public float Network_totalArmorFromStatusEffects
	{
		get
		{
			return _totalArmorFromStatusEffects;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref _totalArmorFromStatusEffects, 4194304uL, (Action<float, float>)null);
		}
	}

	public bool Network_003ChasCold_003Ek__BackingField
	{
		get
		{
			return hasCold__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref hasCold__BackingField, 8388608uL, (Action<bool, bool>)null);
		}
	}

	public int Network_003CfireStack_003Ek__BackingField
	{
		get
		{
			return fireStack__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<int>(value, ref fireStack__BackingField, 16777216uL, (Action<int, int>)null);
		}
	}

	public int Network_003ClightStack_003Ek__BackingField
	{
		get
		{
			return lightStack__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<int>(value, ref lightStack__BackingField, 33554432uL, (Action<int, int>)null);
		}
	}

	public int Network_003CdarkStack_003Ek__BackingField
	{
		get
		{
			return darkStack__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<int>(value, ref darkStack__BackingField, 67108864uL, (Action<int, int>)null);
		}
	}

	public override void ClearPooledEventsAndProcessors()
	{
		base.ClearPooledEventsAndProcessors();
		ClientEvent_OnShowStatusEffectIcon?.Clear();
		ClientEvent_OnFinalStatsChanged?.Clear();
		ClientEvent_OnStatsCalculated?.Clear();
		finalStatsProcessors?.Clear();
	}

	[Server]
	public void HideHealth()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntityStatus::HideHealth()' called when server was not active");
		}
		else
		{
			Network_isHealthHiddenCounter = _isHealthHiddenCounter + 1;
		}
	}

	[Server]
	public void ShowHealth()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntityStatus::ShowHealth()' called when server was not active");
		}
		else
		{
			Network_isHealthHiddenCounter = _isHealthHiddenCounter - 1;
		}
	}

	[Server]
	public void SetMana(float amount)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntityStatus::SetMana(System.Single)' called when server was not active");
		}
		else
		{
			Network_currentMana = Mathf.Clamp(amount, 0f, maxMana);
		}
	}

	[Server]
	public void SetHealth(float amount)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntityStatus::SetHealth(System.Single)' called when server was not active");
		}
		else
		{
			Network_currentHealth = Mathf.Clamp(amount, 0f, maxHealth);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		finalStatsProcessors.onChanged = MarkStatsDirty;
	}

	public override void LogicUpdate(float dt)
	{
		base.LogicUpdate(dt);
		if (!((NetworkBehaviour)this).isServer || entity.isSleeping)
		{
			return;
		}
		if (_isStatusInfoDirty)
		{
			CalculateStats();
		}
		if (isAlive)
		{
			if (healthRegen > float.Epsilon || healthRegen < -float.Epsilon)
			{
				_healthRegenRemainder += healthRegen * dt;
				float num = (float)Math.Truncate(_healthRegenRemainder);
				if (num != 0f)
				{
					_healthRegenRemainder -= num;
					float num2 = Mathf.Clamp(currentHealth + num, 0f, maxHealth);
					if (num2 != currentHealth)
					{
						currentHealth = num2;
					}
				}
				if ((currentHealth >= maxHealth && healthRegen > 0f) || (currentHealth <= 0f && healthRegen < 0f))
				{
					_healthRegenRemainder = 0f;
				}
			}
			if (manaRegen > float.Epsilon || manaRegen < -float.Epsilon)
			{
				_manaRegenRemainder += manaRegen * dt;
				float num3 = (float)Math.Truncate(_manaRegenRemainder);
				if (num3 != 0f)
				{
					_manaRegenRemainder -= num3;
					float num4 = Mathf.Clamp(currentMana + num3, 0f, maxMana);
					if (num4 != currentMana)
					{
						currentMana = num4;
					}
				}
				if ((currentMana >= maxMana && manaRegen > 0f) || (currentMana <= 0f && manaRegen < 0f))
				{
					_manaRegenRemainder = 0f;
				}
			}
		}
		for (int i = 0; i < _grantedStatBonuses.Count; i++)
		{
			if (_grantedStatBonuses[i]._isDirty)
			{
				CalculateStats();
				break;
			}
		}
		if (isAlive && currentHealth <= 0f)
		{
			entity.Kill();
		}
	}

	private void OnBaseStatsChanged(BaseStats _, BaseStats __)
	{
		if (((NetworkBehaviour)this).isServer)
		{
			CalculateStats();
		}
	}

	private void OnScalingStatsChanged(BonusStats _, BonusStats __)
	{
		if (((NetworkBehaviour)this).isServer)
		{
			CalculateStats();
		}
	}

	void ICleanup.OnCleanup()
	{
		if (statusEffects.Count == 0)
		{
			return;
		}
		List<StatusEffect> list = DewPool.GetList(out ListReturnHandle<StatusEffect> handle);
		try
		{
			list.AddRange(statusEffects);
			foreach (StatusEffect item in list)
			{
				if ((UnityEngine.Object)(object)item != null && item.isActive)
				{
					item.Destroy();
				}
			}
		}
		finally
		{
			handle.Return();
		}
	}

	private void OnLevelChanged(int oldLevel, int newLevel)
	{
		if (((NetworkBehaviour)this).isServer)
		{
			float num = maxHealth;
			CalculateStats();
			float num2 = maxHealth - num;
			if (num2 > 0f)
			{
				entity.Status.SetHealth(currentHealth + num2);
			}
			if (entity is Hero { ClientHeroEvent_OnLevelChanged: { } clientHeroEvent_OnLevelChanged })
			{
				clientHeroEvent_OnLevelChanged.Invoke(new EventInfoHeroLevelUp
				{
					oldLevel = oldLevel,
					newLevel = newLevel
				});
			}
		}
	}

	[Server]
	public void DisableSectionTriggering()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntityStatus::DisableSectionTriggering()' called when server was not active");
		}
		else
		{
			isSectionTriggeringDisabled += 1;
		}
	}

	[Server]
	public void EnableSectionTriggering()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntityStatus::EnableSectionTriggering()' called when server was not active");
		}
		else
		{
			isSectionTriggeringDisabled -= 1;
		}
	}

	[Server]
	internal void UpdateShieldAmount()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntityStatus::UpdateShieldAmount()' called when server was not active");
			return;
		}
		float num = 0f;
		for (int i = 0; i < _basicEffects.Count; i++)
		{
			if (_basicEffects[i] is ShieldEffect shieldEffect)
			{
				num += shieldEffect.amount;
			}
		}
		if (_currentShield != num)
		{
			Network_currentShield = num;
		}
	}

	private void OnFinalStatsChanged(FinalStats oldValue, FinalStats newValue)
	{
		ClientEvent_OnFinalStatsChanged?.Invoke();
	}

	[Server]
	public StatBonus AddStatBonus(StatBonus bonus)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'StatBonus EntityStatus::AddStatBonus(StatBonus)' called when server was not active");
			return null;
		}
		if (bonus == null)
		{
			throw new ArgumentNullException("bonus");
		}
		_grantedStatBonuses.Add(bonus);
		CalculateStats();
		return bonus;
	}

	[Server]
	public void RemoveStatBonus(StatBonus bonus)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntityStatus::RemoveStatBonus(StatBonus)' called when server was not active");
		}
		else if (_grantedStatBonuses.Remove(bonus))
		{
			CalculateStats();
		}
	}

	public void MarkStatsDirty()
	{
		_areStatsDirty = true;
	}

	private bool AreStatsDirty()
	{
		if (_areStatsDirty || _isStatusInfoDirty)
		{
			return true;
		}
		for (int i = 0; i < _grantedStatBonuses.Count; i++)
		{
			if (_grantedStatBonuses[i]._isDirty)
			{
				return true;
			}
		}
		return false;
	}

	[Server]
	public void CalculateStatsIfDirty()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntityStatus::CalculateStatsIfDirty()' called when server was not active");
		}
		else if (AreStatsDirty())
		{
			CalculateStats();
		}
	}

	[Server]
	public void CalculateStats()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntityStatus::CalculateStats()' called when server was not active");
			return;
		}
		BonusStats networkbonusStats = BonusStats.Default;
		for (int i = 0; i < _grantedStatBonuses.Count; i++)
		{
			StatBonus statBonus = _grantedStatBonuses[i];
			networkbonusStats += statBonus;
			statBonus._isDirty = false;
		}
		NetworkbonusStats = networkbonusStats;
		UpdateStatusEffectInfo();
		float num = currentHealth / maxHealth;
		float num2 = currentMana / maxMana;
		if (float.IsNaN(num))
		{
			num = 0f;
		}
		if (float.IsNaN(num2))
		{
			num2 = 0f;
		}
		FinalStats data = new FinalStats
		{
			attackDamage = baseStats.attackDamage,
			abilityPower = baseStats.abilityPower,
			maxHealth = baseStats.maxHealth,
			maxMana = baseStats.maxMana,
			healthRegen = baseStats.healthRegen,
			manaRegen = baseStats.manaRegen,
			attackSpeedMultiplier = 1f,
			critAmp = baseStats.critAmp,
			critChance = baseStats.critChance,
			tenacity = baseStats.tenacity,
			abilityHaste = baseStats.abilityHaste,
			movementSpeedMultiplier = 1f,
			fireEffectAmp = baseStats.fireEffectAmp,
			coldEffectAmp = baseStats.coldEffectAmp,
			lightEffectAmp = baseStats.lightEffectAmp,
			darkEffectAmp = baseStats.darkEffectAmp,
			armor = baseStats.armor
		};
		data.attackDamage += scalingStats.attackDamageFlat * (float)(_level - 1);
		data.attackDamage *= 1f + scalingStats.attackDamagePercentage / 100f * (float)(_level - 1);
		data.abilityPower += scalingStats.abilityPowerFlat * (float)(_level - 1);
		data.abilityPower *= 1f + scalingStats.abilityPowerPercentage / 100f * (float)(_level - 1);
		data.maxHealth += scalingStats.maxHealthFlat * (float)(_level - 1);
		data.maxHealth *= 1f + scalingStats.maxHealthPercentage / 100f * (float)(_level - 1);
		data.maxHealthWithoutBonus = data.maxHealth;
		data.maxMana += scalingStats.maxManaFlat * (float)(_level - 1);
		data.maxMana *= 1f + scalingStats.maxManaPercentage / 100f * (float)(_level - 1);
		data.healthRegen += scalingStats.healthRegenFlat * (float)(_level - 1);
		data.healthRegen *= 1f + scalingStats.healthRegenPercentage / 100f * (float)(_level - 1);
		data.manaRegen += scalingStats.manaRegenFlat * (float)(_level - 1);
		data.manaRegen *= 1f + scalingStats.manaRegenPercentage / 100f * (float)(_level - 1);
		data.attackSpeedMultiplier += scalingStats.attackSpeedPercentage * (float)(_level - 1) / 100f;
		data.critAmp += scalingStats.critAmpFlat * (float)(_level - 1);
		data.critAmp *= 1f + scalingStats.critAmpPercentage / 100f * (float)(_level - 1);
		data.critChance += scalingStats.critChanceFlat * (float)(_level - 1);
		data.critChance *= 1f + scalingStats.critChancePercentage / 100f * (float)(_level - 1);
		data.tenacity += scalingStats.tenacityFlat * (float)(_level - 1);
		data.tenacity *= 1f + scalingStats.tenacityPercentage / 100f * (float)(_level - 1);
		data.abilityHaste += scalingStats.abilityHasteFlat * (float)(_level - 1);
		data.abilityHaste *= 1f + scalingStats.abilityHastePercentage / 100f * (float)(_level - 1);
		data.movementSpeedMultiplier += scalingStats.movementSpeedPercentage * (float)(_level - 1) / 100f;
		data.fireEffectAmp += scalingStats.fireEffectAmpFlat * (float)(_level - 1);
		data.coldEffectAmp += scalingStats.coldEffectAmpFlat * (float)(_level - 1);
		data.lightEffectAmp += scalingStats.lightEffectAmpFlat * (float)(_level - 1);
		data.darkEffectAmp += scalingStats.darkEffectAmpFlat * (float)(_level - 1);
		data.armor += scalingStats.armorFlat * (float)(_level - 1);
		data.armor *= 1f + scalingStats.armorPercentage / 100f * (float)(_level - 1);
		data.attackDamage += bonusStats.attackDamageFlat;
		data.attackDamage *= 1f + bonusStats.attackDamagePercentage / 100f;
		data.abilityPower += bonusStats.abilityPowerFlat;
		data.abilityPower *= 1f + bonusStats.abilityPowerPercentage / 100f;
		data.maxHealth += bonusStats.maxHealthFlat;
		data.maxHealth *= 1f + bonusStats.maxHealthPercentage / 100f;
		data.maxMana += bonusStats.maxManaFlat;
		data.maxMana *= 1f + bonusStats.maxManaPercentage / 100f;
		data.healthRegen += bonusStats.healthRegenFlat;
		data.healthRegen *= 1f + bonusStats.healthRegenPercentage / 100f;
		data.manaRegen += bonusStats.manaRegenFlat;
		data.manaRegen *= 1f + bonusStats.manaRegenPercentage / 100f;
		data.attackSpeedMultiplier += bonusStats.attackSpeedPercentage / 100f;
		data.critAmp += bonusStats.critAmpFlat;
		data.critAmp *= 1f + bonusStats.critAmpPercentage / 100f;
		data.critChance += bonusStats.critChanceFlat;
		data.critChance *= 1f + bonusStats.critChancePercentage / 100f;
		data.abilityHaste += bonusStats.abilityHasteFlat;
		data.abilityHaste *= 1f + bonusStats.abilityHastePercentage / 100f;
		data.tenacity += bonusStats.tenacityFlat;
		data.tenacity *= 1f + bonusStats.tenacityPercentage / 100f;
		data.movementSpeedMultiplier += bonusStats.movementSpeedPercentage / 100f;
		data.fireEffectAmp += bonusStats.fireEffectAmpFlat;
		data.coldEffectAmp += bonusStats.coldEffectAmpFlat;
		data.lightEffectAmp += bonusStats.lightEffectAmpFlat;
		data.darkEffectAmp += bonusStats.darkEffectAmpFlat;
		data.armor += bonusStats.armorFlat;
		data.armor *= 1f + bonusStats.armorPercentage / 100f;
		if (hasRoot)
		{
			data.movementSpeedMultiplier = 0f;
		}
		else
		{
			data.movementSpeedMultiplier *= 1f + totalSpeed / 100f;
			data.movementSpeedMultiplier *= 1f - totalSlow / 100f;
			Dew.FilterNonOkayValues(ref data.movementSpeedMultiplier, 1f);
			data.movementSpeedMultiplier = Mathf.Max(data.movementSpeedMultiplier, 0f);
		}
		data.attackSpeedMultiplier += totalHaste / 100f;
		data.attackSpeedMultiplier *= 1f - totalCripple / 100f;
		Dew.FilterNonOkayValues(ref data.attackSpeedMultiplier, 1f);
		data.attackSpeedMultiplier = Mathf.Max(data.attackSpeedMultiplier, 0f);
		data.armor += totalArmorFromStatusEffects;
		data.abilityPower = Mathf.Max(data.abilityPower, 1f);
		data.attackDamage = Mathf.Max(data.attackDamage, 1f);
		data.maxHealth = Mathf.Max(data.maxHealth, 1f);
		data.healthRegen = Mathf.Max(data.healthRegen, 0f);
		finalStatsProcessors.Process(ref data);
		currentHealth = num * data.maxHealth;
		currentMana = num2 * data.maxMana;
		Network_003CfinalStats_003Ek__BackingField = data;
		_areStatsDirty = false;
		if (((NetworkBehaviour)this).isClient)
		{
			RpcInvokeStatsCalculated();
		}
	}

	[ClientRpc]
	private void RpcInvokeStatsCalculated()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendRPCInternal("System.Void EntityStatus::RpcInvokeStatsCalculated()", 1495528986, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	public float CalculateTotalBonusStat(float current, float bonusFlat, float bonusPercentage)
	{
		return bonusFlat + current * (1f - 1f / (1f + bonusPercentage * 0.01f));
	}

	public float GetBonusHealth()
	{
		return CalculateTotalBonusStat(maxHealth, bonusStats.maxHealthFlat, bonusStats.maxHealthPercentage);
	}

	public float GetBonusArmor()
	{
		return CalculateTotalBonusStat(armor, bonusStats.armorFlat, bonusStats.armorPercentage) + totalArmorFromStatusEffects;
	}

	public float GetBonusAttackDamage()
	{
		return CalculateTotalBonusStat(attackDamage, bonusStats.attackDamageFlat, bonusStats.attackDamagePercentage);
	}

	public float GetBonusAbilityPower()
	{
		return CalculateTotalBonusStat(abilityPower, bonusStats.abilityPowerFlat, bonusStats.abilityPowerPercentage);
	}

	[Server]
	internal void DirtyStatusInfo()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntityStatus::DirtyStatusInfo()' called when server was not active");
		}
		else
		{
			_isStatusInfoDirty = true;
		}
	}

	[Server]
	internal void AddBasicEffect(BasicEffect eff)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntityStatus::AddBasicEffect(BasicEffect)' called when server was not active");
			return;
		}
		eff.isAlive = true;
		if (entity.Control.isDisplacing && entity.Control.ongoingDisplacement.isCanceledByCC && !entity.Status.hasCrowdControlImmunity && (eff is RootEffect || eff is StunEffect))
		{
			entity.Control.CancelOngoingDisplacement();
		}
		AttackCriticalEffect ac = eff as AttackCriticalEffect;
		if (ac != null)
		{
			Action<EventInfoAttackFired> action = (EventInfoAttackFired info) =>
			{
				if (info.isCrit)
				{
					ac.onUse?.Invoke();
				}
			};
			entity.EntityEvent_OnAttackFired += action;
			_registeredHandlers.Add(ac, action);
		}
		else
		{
			AttackOverrideEffect ao = eff as AttackOverrideEffect;
			if (ao != null)
			{
				Action<EventInfoCast> action2 = (EventInfoCast _) =>
				{
					ao.onUse?.Invoke();
				};
				Action<EventInfoAbilityInstance> action3 = (EventInfoAbilityInstance info) =>
				{
					if (info.instance.skillLevel == -1)
					{
						info.instance.skillLevel = eff.parent.skillLevel;
					}
				};
				ao.trigger.TriggerEvent_OnCastComplete += action2;
				ao.trigger.ActorEvent_OnAbilityInstanceBeforePrepare += action3;
				_registeredHandlers.Add(ao, (action2, action3));
				entity.Ability._attackAbilityOverrides.Add(ao.trigger);
				ao.trigger.owner = entity;
				ao.trigger.parentActor = entity;
			}
			else
			{
				AttackEmpowerEffect emp = eff as AttackEmpowerEffect;
				if (emp != null)
				{
					float startTime = Time.time;
					List<Actor> banned = new List<Actor>();
					Action<EventInfoAttackEffect> action4 = (EventInfoAttackEffect info) =>
					{
						if (!banned.Contains(info.actor) && (info.actor is Entity || !(info.actor.creationTime < startTime)) && (emp.maxTriggerCount == int.MaxValue || emp._nextIndex < emp.maxTriggerCount))
						{
							emp.onAttackEffect?.Invoke(info, emp._nextIndex);
							emp._nextIndex++;
							if (emp.maxTriggerCount != int.MaxValue && emp._nextIndex >= emp.maxTriggerCount)
							{
								try
								{
									emp.onDepleted?.Invoke();
								}
								catch (Exception exception)
								{
									Debug.LogException(exception);
								}
							}
						}
					};
					Action<EventInfoAttackFired> action5 = (EventInfoAttackFired info) =>
					{
						if (emp.maxTriggerCount == int.MaxValue || emp._nextIndex < emp.maxTriggerCount)
						{
							eff.parent.LockDestroy();
							int index = emp._nextIndex;
							emp._nextIndex++;
							if (emp.maxTriggerCount != int.MaxValue && emp._nextIndex >= emp.maxTriggerCount)
							{
								try
								{
									emp.onDepleted?.Invoke();
								}
								catch (Exception exception)
								{
									Debug.LogException(exception);
								}
							}
							banned.Add(info.instance);
							info.instance.ActorEvent_OnAttackEffectTriggered += (Action<EventInfoAttackEffect>)((EventInfoAttackEffect attack) =>
							{
								emp.onAttackEffect?.Invoke(attack, index);
							});
							info.instance.ClientActorEvent_OnDestroyed += (Action<Actor>)((Actor _) =>
							{
								eff.parent.UnlockDestroy();
							});
						}
					};
					entity.EntityEvent_OnAttackEffectTriggered += action4;
					entity.EntityEvent_OnAttackFiredBeforePrepare += action5;
					Tuple<Action<EventInfoAttackEffect>, Action<EventInfoAttackFired>> value = new Tuple<Action<EventInfoAttackEffect>, Action<EventInfoAttackFired>>(action4, action5);
					_registeredHandlers.Add(emp, value);
				}
				else
				{
					ProtectedEffect pt = eff as ProtectedEffect;
					if (pt != null)
					{
						Action<EventInfoDamageNegatedByImmunity> action6 = (EventInfoDamageNegatedByImmunity info) =>
						{
							if (info.effect == pt)
							{
								pt.onDamageNegated?.Invoke(info);
							}
						};
						entity.EntityEvent_OnDamageNegatedByImmunity += action6;
						_registeredHandlers.Add(pt, action6);
					}
					else
					{
						InvulnerableEffect iv = eff as InvulnerableEffect;
						if (iv != null)
						{
							Action<EventInfoDamageNegatedByImmunity> action7 = (EventInfoDamageNegatedByImmunity info) =>
							{
								if (info.effect == iv)
								{
									iv.onDamageNegated?.Invoke(info);
								}
							};
							entity.EntityEvent_OnDamageNegatedByImmunity += action7;
							_registeredHandlers.Add(iv, action7);
						}
					}
				}
			}
		}
		_basicEffects.Add(eff);
		DirtyStatusInfo();
		CalculateStats();
	}

	[Server]
	internal void RemoveBasicEffect(BasicEffect eff)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntityStatus::RemoveBasicEffect(BasicEffect)' called when server was not active");
			return;
		}
		eff.isAlive = false;
		object value5;
		if (eff is AttackCriticalEffect key)
		{
			if (_registeredHandlers.TryGetValue(key, out var value))
			{
				entity.EntityEvent_OnAttackFired -= (Action<EventInfoAttackFired>)value;
				_registeredHandlers.Remove(key);
			}
		}
		else if (eff is AttackOverrideEffect attackOverrideEffect)
		{
			if (_registeredHandlers.TryGetValue(attackOverrideEffect, out var value2))
			{
				(Action<EventInfoCast>, Action<EventInfoAbilityInstance>) tuple = ((Action<EventInfoCast>, Action<EventInfoAbilityInstance>))value2;
				if (attackOverrideEffect.trigger != null)
				{
					attackOverrideEffect.trigger.TriggerEvent_OnCastComplete -= tuple.Item1;
					attackOverrideEffect.trigger.ActorEvent_OnAbilityInstanceBeforePrepare -= tuple.Item2;
				}
				_registeredHandlers.Remove(attackOverrideEffect);
				entity.Ability._attackAbilityOverrides.Remove(attackOverrideEffect.trigger);
				if (attackOverrideEffect.trigger != null)
				{
					attackOverrideEffect.trigger.owner = null;
				}
				if ((UnityEngine.Object)(object)attackOverrideEffect.trigger != null && ((NetworkBehaviour)attackOverrideEffect.trigger).isServer && attackOverrideEffect._shouldTriggerBeDisposed)
				{
					attackOverrideEffect.trigger.Destroy();
				}
			}
		}
		else if (eff is AttackEmpowerEffect key2)
		{
			if (_registeredHandlers.TryGetValue(key2, out var value3))
			{
				Tuple<Action<EventInfoAttackEffect>, Action<EventInfoAttackFired>> tuple2 = (Tuple<Action<EventInfoAttackEffect>, Action<EventInfoAttackFired>>)value3;
				entity.EntityEvent_OnAttackEffectTriggered -= tuple2.Item1;
				entity.EntityEvent_OnAttackFiredBeforePrepare -= tuple2.Item2;
				_registeredHandlers.Remove(key2);
			}
		}
		else if (eff is ProtectedEffect key3)
		{
			if (_registeredHandlers.TryGetValue(key3, out var value4))
			{
				entity.EntityEvent_OnDamageNegatedByImmunity -= (Action<EventInfoDamageNegatedByImmunity>)value4;
				_registeredHandlers.Remove(key3);
			}
		}
		else if (eff is InvulnerableEffect key4 && _registeredHandlers.TryGetValue(key4, out value5))
		{
			entity.EntityEvent_OnDamageNegatedByImmunity -= (Action<EventInfoDamageNegatedByImmunity>)value5;
			_registeredHandlers.Remove(key4);
		}
		_basicEffects.Remove(eff);
		DirtyStatusInfo();
	}

	[Server]
	private void UpdateStatusEffectInfo()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntityStatus::UpdateStatusEffectInfo()' called when server was not active");
		}
		else
		{
			if (!_isStatusInfoDirty)
			{
				return;
			}
			_isStatusInfoDirty = false;
			_lastDamagePreventer = null;
			BasicEffectMask basicEffectMask = (BasicEffectMask)0u;
			float num = 0f;
			float num2 = 0f;
			float num3 = 0f;
			float num4 = 0f;
			float num5 = 0f;
			AbilityTrigger abilityTrigger = null;
			float num6 = 0f;
			bool flag = false;
			for (int i = 0; i < _basicEffects.Count; i++)
			{
				BasicEffect basicEffect = _basicEffects[i];
				if (!(basicEffect is BasicEffectWithStrength { strength: not (>0f) }))
				{
					basicEffectMask |= basicEffect.mask;
				}
				if (basicEffect is BasicEffectWithStrength { strength: var num7 } basicEffectWithStrength2)
				{
					if (basicEffectWithStrength2.decay && basicEffectWithStrength2.parent.normalizedDuration.HasValue)
					{
						num7 *= basicEffectWithStrength2.parent.normalizedDuration.Value;
						_isStatusInfoDirty = true;
					}
					if (basicEffect is SlowEffect)
					{
						num = Mathf.Max(num, num7);
					}
					else if (basicEffect is SpeedEffect)
					{
						num2 += num7;
					}
					else if (basicEffect is HasteEffect)
					{
						num3 += num7;
					}
					else if (basicEffect is CrippleEffect)
					{
						num4 = Mathf.Max(_totalCripple, num7);
					}
					else if (basicEffect is ArmorBoostEffect)
					{
						num6 += num7;
					}
					else if (basicEffect is ArmorReductionEffect)
					{
						num6 -= num7;
					}
					else if (basicEffect is FakeMaxHealthEffect)
					{
						num5 += num7;
					}
				}
				else if (basicEffect is AttackOverrideEffect attackOverrideEffect)
				{
					abilityTrigger = attackOverrideEffect.trigger;
				}
				else if (basicEffect is ProtectedEffect)
				{
					if (_lastDamagePreventer == null || !(_lastDamagePreventer is InvulnerableEffect))
					{
						_lastDamagePreventer = basicEffect;
					}
				}
				else if (basicEffect is InvulnerableEffect)
				{
					_lastDamagePreventer = basicEffect;
				}
				else if (basicEffect is InvisibleEffect { ignoreReveal: not false })
				{
					flag = true;
				}
			}
			if ((basicEffectMask & (BasicEffectMask.Invulnerable | BasicEffectMask.Unstoppable)) != 0)
			{
				basicEffectMask &= ~(BasicEffectMask.Slow | BasicEffectMask.Cripple | BasicEffectMask.Root | BasicEffectMask.Silence | BasicEffectMask.Stun | BasicEffectMask.Blind);
				num = 0f;
				num4 = 0f;
			}
			num *= 1f - tenacity / 100f;
			num4 *= 1f - tenacity / 100f;
			num6 = Mathf.Clamp(num6, -300f, 100000f);
			bool flag2 = flag || ((basicEffectMask & BasicEffectMask.Invisible) != 0 && (basicEffectMask & BasicEffectMask.Reveal) == 0);
			bool flag3 = (basicEffectMask & BasicEffectMask.Invisible) != 0 && (basicEffectMask & BasicEffectMask.Reveal) != 0 && !flag2;
			if (this.basicEffectMask != basicEffectMask)
			{
				Network_003CbasicEffectMask_003Ek__BackingField = basicEffectMask;
			}
			if (_totalSlow != num)
			{
				Network_totalSlow = num;
			}
			if (_totalSpeed != num2)
			{
				Network_totalSpeed = num2;
			}
			if (_totalHaste != num3)
			{
				Network_totalHaste = num3;
			}
			if (_totalCripple != num4)
			{
				Network_totalCripple = num4;
			}
			if (_totalFakeMaxHealth != num5)
			{
				Network_totalFakeMaxHealth = num5;
			}
			if ((UnityEngine.Object)(object)Network_overridenAttack != (UnityEngine.Object)(object)abilityTrigger)
			{
				Network_overridenAttack = abilityTrigger;
			}
			if (_totalArmorFromStatusEffects != num6)
			{
				Network_totalArmorFromStatusEffects = num6;
			}
			if (isUndetectableByNonAllies != flag2)
			{
				Network_003CisUndetectableByNonAllies_003Ek__BackingField = flag2;
			}
			if (isInvisibilityRevealed != flag3)
			{
				Network_003CisInvisibilityRevealed_003Ek__BackingField = flag3;
			}
			UpdateShieldAmount();
		}
	}

	public bool HasStatusEffect<T>() where T : StatusEffect
	{
		return HasStatusEffect(typeof(T));
	}

	public bool HasStatusEffect(Type statusEffectType)
	{
		for (int i = 0; i < statusEffects.Count; i++)
		{
			StatusEffect o = statusEffects[i];
			if (statusEffectType.IsInstanceOfType(o))
			{
				return true;
			}
		}
		return false;
	}

	public T GetStatusEffect<T>() where T : StatusEffect
	{
		for (int i = 0; i < statusEffects.Count; i++)
		{
			if (statusEffects[i] is T result)
			{
				return result;
			}
		}
		return null;
	}

	public StatusEffect GetStatusEffect(Type type)
	{
		for (int i = 0; i < statusEffects.Count; i++)
		{
			StatusEffect statusEffect = statusEffects[i];
			if (type.IsInstanceOfType(statusEffect))
			{
				return statusEffect;
			}
		}
		return null;
	}

	public T FindStatusEffect<T>(Func<T, bool> predicate) where T : StatusEffect
	{
		for (int i = 0; i < statusEffects.Count; i++)
		{
			if (statusEffects[i] is T val && predicate(val))
			{
				return val;
			}
		}
		return null;
	}

	public StatusEffect FindStatusEffect(Type type, Func<StatusEffect, bool> predicate)
	{
		for (int i = 0; i < statusEffects.Count; i++)
		{
			StatusEffect statusEffect = statusEffects[i];
			if (type.IsInstanceOfType(statusEffect) && predicate(statusEffect))
			{
				return statusEffect;
			}
		}
		return null;
	}

	public IEnumerable<T> GetStatusEffects<T>() where T : StatusEffect
	{
		for (int i = statusEffects.Count - 1; i >= 0; i--)
		{
			if (statusEffects[i] is T val)
			{
				yield return val;
			}
		}
	}

	public IEnumerable<StatusEffect> GetStatusEffects(Type type)
	{
		for (int i = 0; i < statusEffects.Count; i++)
		{
			StatusEffect statusEffect = statusEffects[i];
			if (type.IsInstanceOfType(statusEffect))
			{
				yield return statusEffect;
			}
		}
	}

	public bool TryGetStatusEffect<T>(out T effect) where T : StatusEffect
	{
		for (int i = 0; i < statusEffects.Count; i++)
		{
			if (statusEffects[i] is T val)
			{
				effect = val;
				return true;
			}
		}
		effect = null;
		return false;
	}

	public bool TryGetStatusEffect(Type type, out StatusEffect effect)
	{
		for (int i = 0; i < statusEffects.Count; i++)
		{
			StatusEffect statusEffect = statusEffects[i];
			if (type.IsInstanceOfType(statusEffect))
			{
				effect = statusEffect;
				return true;
			}
		}
		effect = null;
		return false;
	}

	public bool HasElemental(ElementalType type)
	{
		return GetElementalStack(type) > 0;
	}

	public int GetElementalStack(ElementalType type)
	{
		switch (type)
		{
		case ElementalType.Fire:
			return fireStack;
		case ElementalType.Cold:
			if (!hasCold)
			{
				return 0;
			}
			return 1;
		case ElementalType.Light:
			return lightStack;
		case ElementalType.Dark:
			return darkStack;
		default:
			return 0;
		}
	}

	public float GetElementalAmp(ElementalType type)
	{
		return type switch
		{
			ElementalType.Fire => fireEffectAmp, 
			ElementalType.Cold => coldEffectAmp, 
			ElementalType.Light => lightEffectAmp, 
			ElementalType.Dark => darkEffectAmp, 
			_ => 0f, 
		};
	}

	public EntityStatus()
	{
		_Mirror_SyncVarHookDelegate__level = OnLevelChanged;
		_Mirror_SyncVarHookDelegate_baseStats = OnBaseStatsChanged;
		_Mirror_SyncVarHookDelegate_scalingStats = OnScalingStatsChanged;
		_Mirror_SyncVarHookDelegate__003CfinalStats_003Ek__BackingField = OnFinalStatsChanged;
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcInvokeStatsCalculated()
	{
		ClientEvent_OnStatsCalculated?.Invoke();
	}

	protected static void InvokeUserCode_RpcInvokeStatsCalculated(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcInvokeStatsCalculated called on server.");
		}
		else
		{
			((EntityStatus)(object)obj).UserCode_RpcInvokeStatsCalculated();
		}
	}

	static EntityStatus()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterRpc(typeof(EntityStatus), "System.Void EntityStatus::RpcInvokeStatsCalculated()", (RemoteCallDelegate)InvokeUserCode_RpcInvokeStatsCalculated);
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		((NetworkBehaviour)this).SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteBool(writer, isInConversation__BackingField);
			NetworkWriterExtensions.WriteInt(writer, _isHealthHiddenCounter);
			NetworkWriterExtensions.WriteInt(writer, _level);
			GeneratedNetworkCode._Write_BaseStats(writer, baseStats);
			GeneratedNetworkCode._Write_BonusStats(writer, scalingStats);
			NetworkWriterExtensions.WriteVector2(writer, specialFill__BackingField);
			GeneratedNetworkCode._Write_BonusStats(writer, bonusStats);
			NetworkWriterExtensions.WriteFloat(writer, _currentHealth);
			NetworkWriterExtensions.WriteFloat(writer, _currentMana);
			writer.WriteCounterBool(isSectionTriggeringDisabled__BackingField);
			NetworkWriterExtensions.WriteFloat(writer, mirageSkinInitAmount__BackingField);
			NetworkWriterExtensions.WriteFloat(writer, _currentShield);
			GeneratedNetworkCode._Write_FinalStats(writer, finalStats__BackingField);
			NetworkWriterExtensions.WriteBool(writer, isUndetectableByNonAllies__BackingField);
			NetworkWriterExtensions.WriteBool(writer, isInvisibilityRevealed__BackingField);
			GeneratedNetworkCode._Write_BasicEffectMask(writer, basicEffectMask__BackingField);
			NetworkWriterExtensions.WriteFloat(writer, _totalSlow);
			NetworkWriterExtensions.WriteFloat(writer, _totalSpeed);
			NetworkWriterExtensions.WriteFloat(writer, _totalHaste);
			NetworkWriterExtensions.WriteFloat(writer, _totalCripple);
			NetworkWriterExtensions.WriteFloat(writer, _totalFakeMaxHealth);
			NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)Network_overridenAttack);
			NetworkWriterExtensions.WriteFloat(writer, _totalArmorFromStatusEffects);
			NetworkWriterExtensions.WriteBool(writer, hasCold__BackingField);
			NetworkWriterExtensions.WriteInt(writer, fireStack__BackingField);
			NetworkWriterExtensions.WriteInt(writer, lightStack__BackingField);
			NetworkWriterExtensions.WriteInt(writer, darkStack__BackingField);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 1L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, isInConversation__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 2L) != 0L)
		{
			NetworkWriterExtensions.WriteInt(writer, _isHealthHiddenCounter);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 4L) != 0L)
		{
			NetworkWriterExtensions.WriteInt(writer, _level);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 8L) != 0L)
		{
			GeneratedNetworkCode._Write_BaseStats(writer, baseStats);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x10L) != 0L)
		{
			GeneratedNetworkCode._Write_BonusStats(writer, scalingStats);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x20L) != 0L)
		{
			NetworkWriterExtensions.WriteVector2(writer, specialFill__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x40L) != 0L)
		{
			GeneratedNetworkCode._Write_BonusStats(writer, bonusStats);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x80L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, _currentHealth);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x100L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, _currentMana);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x200L) != 0L)
		{
			writer.WriteCounterBool(isSectionTriggeringDisabled__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x400L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, mirageSkinInitAmount__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x800L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, _currentShield);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x1000L) != 0L)
		{
			GeneratedNetworkCode._Write_FinalStats(writer, finalStats__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x2000L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, isUndetectableByNonAllies__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x4000L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, isInvisibilityRevealed__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x8000L) != 0L)
		{
			GeneratedNetworkCode._Write_BasicEffectMask(writer, basicEffectMask__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x10000L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, _totalSlow);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x20000L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, _totalSpeed);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x40000L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, _totalHaste);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x80000L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, _totalCripple);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x100000L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, _totalFakeMaxHealth);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x200000L) != 0L)
		{
			NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)Network_overridenAttack);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x400000L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, _totalArmorFromStatusEffects);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x800000L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, hasCold__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x1000000L) != 0L)
		{
			NetworkWriterExtensions.WriteInt(writer, fireStack__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x2000000L) != 0L)
		{
			NetworkWriterExtensions.WriteInt(writer, lightStack__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x4000000L) != 0L)
		{
			NetworkWriterExtensions.WriteInt(writer, darkStack__BackingField);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		((NetworkBehaviour)this).DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref isInConversation__BackingField, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref _isHealthHiddenCounter, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref _level, _Mirror_SyncVarHookDelegate__level, NetworkReaderExtensions.ReadInt(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<BaseStats>(ref baseStats, _Mirror_SyncVarHookDelegate_baseStats, GeneratedNetworkCode._Read_BaseStats(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<BonusStats>(ref scalingStats, _Mirror_SyncVarHookDelegate_scalingStats, GeneratedNetworkCode._Read_BonusStats(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<Vector2>(ref specialFill__BackingField, (Action<Vector2, Vector2>)null, NetworkReaderExtensions.ReadVector2(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<BonusStats>(ref bonusStats, (Action<BonusStats, BonusStats>)null, GeneratedNetworkCode._Read_BonusStats(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _currentHealth, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _currentMana, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<CounterBool>(ref isSectionTriggeringDisabled__BackingField, (Action<CounterBool, CounterBool>)null, reader.ReadCounterBool());
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref mirageSkinInitAmount__BackingField, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _currentShield, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<FinalStats>(ref finalStats__BackingField, _Mirror_SyncVarHookDelegate__003CfinalStats_003Ek__BackingField, GeneratedNetworkCode._Read_FinalStats(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref isUndetectableByNonAllies__BackingField, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref isInvisibilityRevealed__BackingField, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<BasicEffectMask>(ref basicEffectMask__BackingField, (Action<BasicEffectMask, BasicEffectMask>)null, GeneratedNetworkCode._Read_BasicEffectMask(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _totalSlow, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _totalSpeed, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _totalHaste, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _totalCripple, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _totalFakeMaxHealth, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize_NetworkBehaviour<AbilityTrigger>(ref _overridenAttack, (Action<AbilityTrigger, AbilityTrigger>)null, reader, ref ____overridenAttackNetId);
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _totalArmorFromStatusEffects, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref hasCold__BackingField, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref fireStack__BackingField, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref lightStack__BackingField, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref darkStack__BackingField, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 1L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref isInConversation__BackingField, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
		}
		if ((num & 2L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref _isHealthHiddenCounter, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
		}
		if ((num & 4L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref _level, _Mirror_SyncVarHookDelegate__level, NetworkReaderExtensions.ReadInt(reader));
		}
		if ((num & 8L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<BaseStats>(ref baseStats, _Mirror_SyncVarHookDelegate_baseStats, GeneratedNetworkCode._Read_BaseStats(reader));
		}
		if ((num & 0x10L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<BonusStats>(ref scalingStats, _Mirror_SyncVarHookDelegate_scalingStats, GeneratedNetworkCode._Read_BonusStats(reader));
		}
		if ((num & 0x20L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<Vector2>(ref specialFill__BackingField, (Action<Vector2, Vector2>)null, NetworkReaderExtensions.ReadVector2(reader));
		}
		if ((num & 0x40L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<BonusStats>(ref bonusStats, (Action<BonusStats, BonusStats>)null, GeneratedNetworkCode._Read_BonusStats(reader));
		}
		if ((num & 0x80L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _currentHealth, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
		if ((num & 0x100L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _currentMana, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
		if ((num & 0x200L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<CounterBool>(ref isSectionTriggeringDisabled__BackingField, (Action<CounterBool, CounterBool>)null, reader.ReadCounterBool());
		}
		if ((num & 0x400L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref mirageSkinInitAmount__BackingField, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
		if ((num & 0x800L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _currentShield, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
		if ((num & 0x1000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<FinalStats>(ref finalStats__BackingField, _Mirror_SyncVarHookDelegate__003CfinalStats_003Ek__BackingField, GeneratedNetworkCode._Read_FinalStats(reader));
		}
		if ((num & 0x2000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref isUndetectableByNonAllies__BackingField, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
		}
		if ((num & 0x4000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref isInvisibilityRevealed__BackingField, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
		}
		if ((num & 0x8000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<BasicEffectMask>(ref basicEffectMask__BackingField, (Action<BasicEffectMask, BasicEffectMask>)null, GeneratedNetworkCode._Read_BasicEffectMask(reader));
		}
		if ((num & 0x10000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _totalSlow, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
		if ((num & 0x20000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _totalSpeed, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
		if ((num & 0x40000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _totalHaste, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
		if ((num & 0x80000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _totalCripple, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
		if ((num & 0x100000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _totalFakeMaxHealth, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
		if ((num & 0x200000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize_NetworkBehaviour<AbilityTrigger>(ref _overridenAttack, (Action<AbilityTrigger, AbilityTrigger>)null, reader, ref ____overridenAttackNetId);
		}
		if ((num & 0x400000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _totalArmorFromStatusEffects, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
		if ((num & 0x800000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref hasCold__BackingField, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
		}
		if ((num & 0x1000000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref fireStack__BackingField, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
		}
		if ((num & 0x2000000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref lightStack__BackingField, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
		}
		if ((num & 0x4000000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref darkStack__BackingField, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
		}
	}
}
