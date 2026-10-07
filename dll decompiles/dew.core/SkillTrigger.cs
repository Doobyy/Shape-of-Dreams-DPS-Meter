using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class SkillTrigger : AbilityTrigger, IInteractable, IItem, IExcludeFromPool
{
	public static bool DisableHeroTypeCheckOnIdentityMemories;

	public const float Ult_GlobalChargeMultiplier = 0.85f;

	public const float Ult_OnKillLesser = 0.12750001f;

	public const float Ult_OnKillNormal = 0.63750005f;

	public const float Ult_OnKillMiniBoss = 12.75f;

	public const float Ult_OnKillBoss = 42.5f;

	public const float Ult_PerDealtNonElementalDamage = 0.2125f;

	public const float Ult_PerDealtElementalDamage = 0.0425f;

	public const float Ult_PerDealtAttack = 0.2125f;

	public const float Ult_PerDealtDamageLesserEnemyHealthRatio = 0.12750001f;

	public const float Ult_PerDealtDamageNormalEnemyHealthRatio = 0.25500003f;

	public const float Ult_PerDealtDamageMiniBossEnemyHealthRatio = 12.75f;

	public const float Ult_PerDealtDamageBossEnemyHealthRatio = 25.5f;

	public const float Ult_PerTakenNonElementalDamage = 0.2125f;

	public const float Ult_PerTakenElementalDamage = 0.0425f;

	public const float Ult_PerTakenDamageMyHealthRatio = 25.5f;

	[CompilerGenerated]
	[SyncVar]
	private bool skipStartAnimation__BackingField;

	[CompilerGenerated]
	[SyncVar(hook = "OnHandOwnerChanged")]
	private Hero handOwner__BackingField;

	public SafeAction<Hero, Hero> ClientEvent_OnHandOwnerChanged;

	[CompilerGenerated]
	[SyncVar(hook = "OnTempOwnerChanged")]
	private DewPlayer tempOwner__BackingField;

	public SafeAction<DewPlayer, DewPlayer> ClientEvent_OnTempOwnerChanged;

	[CompilerGenerated]
	[SyncVar]
	private HeroSkillLocation skillType__BackingField;

	[CompilerGenerated]
	[SyncVar]
	private string characterSkillOwner__BackingField;

	public SafeAction<int, int> ClientSkillEvent_OnLevelChange;

	[SyncVar(hook = "OnLevelChange")]
	private int _level;

	public Rarity rarity;

	[SyncVar]
	public SkillType type;

	[SyncVar]
	public DescriptionTags tags;

	public bool isLevelUpEnabled = true;

	public bool useCustomSkillHastePerLevel;

	public float customSkillHastePerLevel = 10f;

	public GameObject startEffect;

	public GameObject endEffect;

	public bool excludeFromPool;

	[NonSerialized]
	public StatBonus statBonus = new StatBonus();

	private SkillWorldModel _worldModel;

	private SkillBonus _skillHastePerLevelBonus;

	[SyncVar]
	private float _skillHasteGrantedByLevelMultiplier = 1f;

	[CompilerGenerated]
	[SyncVar]
	private Color specialOverlayColor__BackingField = Color.clear;

	[CompilerGenerated]
	[SyncVar]
	private bool isAnimatingEmbraceNewIdentity__BackingField;

	public const float DismantleTapMinInterval = 0.075f;

	public const float DismantleDecayStartTime = 1f;

	public const float DismantleTapStrength = 0.4f;

	public SafeAction<float, float> ClientSkillEvent_OnDismantleProgressChanged;

	[CompilerGenerated]
	[SyncVar(hook = "OnDismantleProgressChanged")]
	private float dismantleProgress__BackingField;

	private float _lastDismantleTapTime;

	internal Hero _lastDismantler;

	[CompilerGenerated]
	[SyncVar]
	private int maxSellGold__BackingField = int.MaxValue;

	private List<SkillBonus> _grantedSkillBonus;

	internal bool _isSkillBonusDirty;

	[CompilerGenerated]
	[SyncVar]
	private float sbCooldownMultiplier__BackingField = 1f;

	[CompilerGenerated]
	[SyncVar]
	private float sbCooldownMultiplierIgnoreReceiveCooldownReductionFlag__BackingField = 1f;

	[CompilerGenerated]
	[SyncVar]
	private float sbCooldownOffset__BackingField;

	[CompilerGenerated]
	[SyncVar]
	private float sbCooldownOffsetIgnoreReceiveCooldownReductionFlag__BackingField;

	private int? _mainConfigOriginalCharge;

	protected NetworkBehaviourSyncVar ____003ChandOwner_003Ek__BackingFieldNetId;

	protected NetworkBehaviourSyncVar ____003CtempOwner_003Ek__BackingFieldNetId;

	public Action<Hero, Hero> _Mirror_SyncVarHookDelegate__003ChandOwner_003Ek__BackingField;

	public Action<DewPlayer, DewPlayer> _Mirror_SyncVarHookDelegate__003CtempOwner_003Ek__BackingField;

	public Action<int, int> _Mirror_SyncVarHookDelegate__level;

	public Action<float, float> _Mirror_SyncVarHookDelegate__003CdismantleProgress_003Ek__BackingField;

	int IInteractable.priority
	{
		get
		{
			if (!((UnityEngine.Object)(object)Network_003CtempOwner_003Ek__BackingField != null) || !((UnityEngine.Object)(object)Network_003CtempOwner_003Ek__BackingField != (UnityEngine.Object)(object)DewPlayer.local))
			{
				return 0;
			}
			return 50;
		}
	}

	public ItemWorldModel worldModel => _worldModel;

	public bool skipStartAnimation
	{
		[CompilerGenerated]
		get
		{
			return skipStartAnimation__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CskipStartAnimation_003Ek__BackingField = value;
		}
	}

	public Hero handOwner
	{
		[CompilerGenerated]
		get
		{
			return Network_003ChandOwner_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003ChandOwner_003Ek__BackingField = value;
		}
	}

	public DewPlayer tempOwner
	{
		[CompilerGenerated]
		get
		{
			return Network_003CtempOwner_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CtempOwner_003Ek__BackingField = value;
		}
	}

	public bool isLocked => IsLockedFor(DewPlayer.local);

	public HeroSkillLocation skillType
	{
		[CompilerGenerated]
		get
		{
			return skillType__BackingField;
		}
		[CompilerGenerated]
		internal set
		{
			Network_003CskillType_003Ek__BackingField = value;
		}
	}

	[SaveVar(SaveVarFlags.Default)]
	public string characterSkillOwner
	{
		[CompilerGenerated]
		get
		{
			return characterSkillOwner__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CcharacterSkillOwner_003Ek__BackingField = value;
		}
	}

	[SaveVar(SaveVarFlags.Default)]
	public int level
	{
		get
		{
			return ScalingValue.levelOverride ?? _level;
		}
		set
		{
			Network_level = value;
		}
	}

	public float skillHastePerLevel
	{
		get
		{
			if (useCustomSkillHastePerLevel)
			{
				return customSkillHastePerLevel;
			}
			if ((bool)(UnityEngine.Object)(object)NetworkedManagerBase<GameManager>.instance)
			{
				return NetworkedManagerBase<GameManager>.instance.GetGainedSkillHastePerSkillLevel(this);
			}
			return 6f;
		}
	}

	bool IExcludeFromPool.excludeFromPool => excludeFromPool;

	public bool isCharacterSkill => rarity == Rarity.Character;

	public Color specialOverlayColor
	{
		[CompilerGenerated]
		get
		{
			return specialOverlayColor__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CspecialOverlayColor_003Ek__BackingField = value;
		}
	}

	Rarity IItem.rarity => rarity;

	public new Hero owner => base.owner as Hero;

	public Transform interactPivot
	{
		get
		{
			if (!(_worldModel != null))
			{
				return ((Component)(object)this).transform;
			}
			return _worldModel.iconQuad.transform;
		}
	}

	public bool canInteractWithMouse => true;

	public float focusDistance => 3f;

	public bool isAnimatingEmbraceNewIdentity
	{
		[CompilerGenerated]
		get
		{
			return isAnimatingEmbraceNewIdentity__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CisAnimatingEmbraceNewIdentity_003Ek__BackingField = value;
		}
	}

	public float dismantleProgress
	{
		[CompilerGenerated]
		get
		{
			return dismantleProgress__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			Network_003CdismantleProgress_003Ek__BackingField = value;
		}
	}

	public int maxSellGold
	{
		[CompilerGenerated]
		get
		{
			return maxSellGold__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CmaxSellGold_003Ek__BackingField = value;
		}
	}

	public int effectiveLevel => level;

	public Entity statEntity => owner;

	public float sbCooldownMultiplier
	{
		[CompilerGenerated]
		get
		{
			return sbCooldownMultiplier__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			Network_003CsbCooldownMultiplier_003Ek__BackingField = value;
		}
	}

	public float sbCooldownMultiplierIgnoreReceiveCooldownReductionFlag
	{
		[CompilerGenerated]
		get
		{
			return sbCooldownMultiplierIgnoreReceiveCooldownReductionFlag__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			Network_003CsbCooldownMultiplierIgnoreReceiveCooldownReductionFlag_003Ek__BackingField = value;
		}
	}

	public float sbCooldownOffset
	{
		[CompilerGenerated]
		get
		{
			return sbCooldownOffset__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			Network_003CsbCooldownOffset_003Ek__BackingField = value;
		}
	}

	public float sbCooldownOffsetIgnoreReceiveCooldownReductionFlag
	{
		[CompilerGenerated]
		get
		{
			return sbCooldownOffsetIgnoreReceiveCooldownReductionFlag__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			Network_003CsbCooldownOffsetIgnoreReceiveCooldownReductionFlag_003Ek__BackingField = value;
		}
	}

	public int mainConfigOriginalCharge
	{
		get
		{
			if (!_mainConfigOriginalCharge.HasValue)
			{
				_mainConfigOriginalCharge = configs[0].maxCharges;
			}
			return _mainConfigOriginalCharge.Value;
		}
	}

	public bool Network_003CskipStartAnimation_003Ek__BackingField
	{
		get
		{
			return skipStartAnimation__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref skipStartAnimation__BackingField, 1024uL, (Action<bool, bool>)null);
		}
	}

	public Hero Network_003ChandOwner_003Ek__BackingField
	{
		get
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			return ((NetworkBehaviour)this).GetSyncVarNetworkBehaviour<Hero>(____003ChandOwner_003Ek__BackingFieldNetId, ref handOwner__BackingField);
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter_NetworkBehaviour<Hero>(value, ref handOwner__BackingField, 2048uL, _Mirror_SyncVarHookDelegate__003ChandOwner_003Ek__BackingField, ref ____003ChandOwner_003Ek__BackingFieldNetId);
		}
	}

	public DewPlayer Network_003CtempOwner_003Ek__BackingField
	{
		get
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			return ((NetworkBehaviour)this).GetSyncVarNetworkBehaviour<DewPlayer>(____003CtempOwner_003Ek__BackingFieldNetId, ref tempOwner__BackingField);
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter_NetworkBehaviour<DewPlayer>(value, ref tempOwner__BackingField, 4096uL, _Mirror_SyncVarHookDelegate__003CtempOwner_003Ek__BackingField, ref ____003CtempOwner_003Ek__BackingFieldNetId);
		}
	}

	public HeroSkillLocation Network_003CskillType_003Ek__BackingField
	{
		get
		{
			return skillType__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<HeroSkillLocation>(value, ref skillType__BackingField, 8192uL, (Action<HeroSkillLocation, HeroSkillLocation>)null);
		}
	}

	public string Network_003CcharacterSkillOwner_003Ek__BackingField
	{
		get
		{
			return characterSkillOwner__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<string>(value, ref characterSkillOwner__BackingField, 16384uL, (Action<string, string>)null);
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
			((NetworkBehaviour)this).GeneratedSyncVarSetter<int>(value, ref _level, 32768uL, _Mirror_SyncVarHookDelegate__level);
		}
	}

	public SkillType Networktype
	{
		get
		{
			return type;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<SkillType>(value, ref type, 65536uL, (Action<SkillType, SkillType>)null);
		}
	}

	public DescriptionTags Networktags
	{
		get
		{
			return tags;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<DescriptionTags>(value, ref tags, 131072uL, (Action<DescriptionTags, DescriptionTags>)null);
		}
	}

	public float Network_skillHasteGrantedByLevelMultiplier
	{
		get
		{
			return _skillHasteGrantedByLevelMultiplier;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref _skillHasteGrantedByLevelMultiplier, 262144uL, (Action<float, float>)null);
		}
	}

	public Color Network_003CspecialOverlayColor_003Ek__BackingField
	{
		get
		{
			return specialOverlayColor__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<Color>(value, ref specialOverlayColor__BackingField, 524288uL, (Action<Color, Color>)null);
		}
	}

	public bool Network_003CisAnimatingEmbraceNewIdentity_003Ek__BackingField
	{
		get
		{
			return isAnimatingEmbraceNewIdentity__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref isAnimatingEmbraceNewIdentity__BackingField, 1048576uL, (Action<bool, bool>)null);
		}
	}

	public float Network_003CdismantleProgress_003Ek__BackingField
	{
		get
		{
			return dismantleProgress__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref dismantleProgress__BackingField, 2097152uL, _Mirror_SyncVarHookDelegate__003CdismantleProgress_003Ek__BackingField);
		}
	}

	public int Network_003CmaxSellGold_003Ek__BackingField
	{
		get
		{
			return maxSellGold__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<int>(value, ref maxSellGold__BackingField, 4194304uL, (Action<int, int>)null);
		}
	}

	public float Network_003CsbCooldownMultiplier_003Ek__BackingField
	{
		get
		{
			return sbCooldownMultiplier__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref sbCooldownMultiplier__BackingField, 8388608uL, (Action<float, float>)null);
		}
	}

	public float Network_003CsbCooldownMultiplierIgnoreReceiveCooldownReductionFlag_003Ek__BackingField
	{
		get
		{
			return sbCooldownMultiplierIgnoreReceiveCooldownReductionFlag__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref sbCooldownMultiplierIgnoreReceiveCooldownReductionFlag__BackingField, 16777216uL, (Action<float, float>)null);
		}
	}

	public float Network_003CsbCooldownOffset_003Ek__BackingField
	{
		get
		{
			return sbCooldownOffset__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref sbCooldownOffset__BackingField, 33554432uL, (Action<float, float>)null);
		}
	}

	public float Network_003CsbCooldownOffsetIgnoreReceiveCooldownReductionFlag_003Ek__BackingField
	{
		get
		{
			return sbCooldownOffsetIgnoreReceiveCooldownReductionFlag__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref sbCooldownOffsetIgnoreReceiveCooldownReductionFlag__BackingField, 67108864uL, (Action<float, float>)null);
		}
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void OnInit()
	{
		DisableHeroTypeCheckOnIdentityMemories = false;
	}

	private void OnTempOwnerChanged(DewPlayer _, DewPlayer __)
	{
		ClientEvent_OnTempOwnerChanged?.Invoke(_, __);
	}

	public bool IsLockedFor(DewPlayer player)
	{
		if ((UnityEngine.Object)(object)Network_003CtempOwner_003Ek__BackingField != null && (UnityEngine.Object)(object)Network_003CtempOwner_003Ek__BackingField != (UnityEngine.Object)(object)player)
		{
			return !Network_003CtempOwner_003Ek__BackingField.hero.IsNullInactiveDeadOrKnockedOut();
		}
		return false;
	}

	private void OnHandOwnerChanged(Hero oldOwner, Hero newOwner)
	{
		ClientEvent_OnHandOwnerChanged?.Invoke(oldOwner, newOwner);
	}

	protected override void Awake()
	{
		base.Awake();
		ActorEvent_OnAbilityInstanceCreated += (Action<EventInfoAbilityInstance>)((EventInfoAbilityInstance info) =>
		{
			if (!((UnityEngine.Object)(object)owner == null))
			{
				HeroSkillLocation heroSkillLocation;
				if ((UnityEngine.Object)(object)owner.Skill.Q == (UnityEngine.Object)(object)this)
				{
					heroSkillLocation = HeroSkillLocation.Q;
				}
				else if ((UnityEngine.Object)(object)owner.Skill.W == (UnityEngine.Object)(object)this)
				{
					heroSkillLocation = HeroSkillLocation.W;
				}
				else if ((UnityEngine.Object)(object)owner.Skill.E == (UnityEngine.Object)(object)this)
				{
					heroSkillLocation = HeroSkillLocation.E;
				}
				else if ((UnityEngine.Object)(object)owner.Skill.R == (UnityEngine.Object)(object)this)
				{
					heroSkillLocation = HeroSkillLocation.R;
				}
				else if ((UnityEngine.Object)(object)owner.Skill.Identity == (UnityEngine.Object)(object)this)
				{
					heroSkillLocation = HeroSkillLocation.Identity;
				}
				else
				{
					if (!((UnityEngine.Object)(object)owner.Skill.Movement == (UnityEngine.Object)(object)this))
					{
						return;
					}
					heroSkillLocation = HeroSkillLocation.Movement;
				}
				owner.HeroEvent_OnAbilityInstanceCreatedFromSkill?.Invoke(new EventInfoSkillAbilityInstance
				{
					type = heroSkillLocation,
					instance = info.instance,
					skill = this
				});
			}
		});
		ActorEvent_OnAbilityInstanceBeforePrepare += (Action<EventInfoAbilityInstance>)((EventInfoAbilityInstance info) =>
		{
			if (!((UnityEngine.Object)(object)owner == null))
			{
				HeroSkillLocation heroSkillLocation;
				if ((UnityEngine.Object)(object)owner.Skill.Q == (UnityEngine.Object)(object)this)
				{
					heroSkillLocation = HeroSkillLocation.Q;
				}
				else if ((UnityEngine.Object)(object)owner.Skill.W == (UnityEngine.Object)(object)this)
				{
					heroSkillLocation = HeroSkillLocation.W;
				}
				else if ((UnityEngine.Object)(object)owner.Skill.E == (UnityEngine.Object)(object)this)
				{
					heroSkillLocation = HeroSkillLocation.E;
				}
				else if ((UnityEngine.Object)(object)owner.Skill.R == (UnityEngine.Object)(object)this)
				{
					heroSkillLocation = HeroSkillLocation.R;
				}
				else if ((UnityEngine.Object)(object)owner.Skill.Identity == (UnityEngine.Object)(object)this)
				{
					heroSkillLocation = HeroSkillLocation.Identity;
				}
				else
				{
					if (!((UnityEngine.Object)(object)owner.Skill.Movement == (UnityEngine.Object)(object)this))
					{
						return;
					}
					heroSkillLocation = HeroSkillLocation.Movement;
				}
				owner.HeroEvent_OnAbilityInstanceBeforePrepareFromSkill?.Invoke(new EventInfoSkillAbilityInstance
				{
					type = heroSkillLocation,
					instance = info.instance,
					skill = this
				});
			}
		});
	}

	protected override void OnPrepare()
	{
		base.OnPrepare();
		if (!isNewInstance)
		{
			Network_003CskipStartAnimation_003Ek__BackingField = true;
		}
	}

	public override void OnStartServer()
	{
		base.OnStartServer();
		_skillHastePerLevelBonus = new SkillBonus();
		UpdateSkillHastePerLevelBonus();
		AddSkillBonus(_skillHastePerLevelBonus);
	}

	public override void OnStartClient()
	{
		base.OnStartClient();
		GameObject original = Resources.Load<GameObject>("WorldModels/Skill World Model");
		_worldModel = UnityEngine.Object.Instantiate(original, position, rotation, ((Component)(object)this).transform).GetComponent<SkillWorldModel>();
	}

	private void UpdateSkillHastePerLevelBonus()
	{
		_skillHastePerLevelBonus.cooldownMultiplier = GetCooldownMultiplierOfSkillHastePerLevelBonus(level);
	}

	private float GetCooldownMultiplierOfSkillHastePerLevelBonus(int lvl)
	{
		float num = (((UnityEngine.Object)(object)NetworkedManagerBase<GameManager>.instance) ? NetworkedManagerBase<GameManager>.instance.ges.cooldownFloorRatioBySkillUpgrade : 0f);
		return num + (1f - num) / (1f + skillHastePerLevel * (float)Mathf.Clamp(lvl - 1, 0, int.MaxValue) * 0.01f);
	}

	public override float GetCooldownTimeMultiplier(int configIndex)
	{
		if (type == SkillType.Ultimate && configIndex == 0)
		{
			return 100f / configs[configIndex].cooldownTime;
		}
		float cooldownTimeMultiplier = base.GetCooldownTimeMultiplier(configIndex);
		if (configs[configIndex].canReceiveCooldownReduction)
		{
			return cooldownTimeMultiplier * sbCooldownMultiplier;
		}
		return cooldownTimeMultiplier * sbCooldownMultiplierIgnoreReceiveCooldownReductionFlag;
	}

	public override float GetCooldownTimeOffset(int configIndex)
	{
		float cooldownTimeOffset = base.GetCooldownTimeOffset(configIndex);
		if (configs[configIndex].canReceiveCooldownReduction)
		{
			return cooldownTimeOffset + sbCooldownOffset;
		}
		return cooldownTimeOffset + sbCooldownOffsetIgnoreReceiveCooldownReductionFlag;
	}

	public override void LogicUpdate(float dt)
	{
		base.LogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer)
		{
			DoDismantleLogicUpdate();
			DoSkillBonusLogicUpdate();
		}
	}

	public override AbilityInstance OnCastComplete(int configIndex, CastInfo info)
	{
		AbilityInstance result = base.OnCastComplete(configIndex, info);
		HeroSkillLocation heroSkillLocation;
		if ((UnityEngine.Object)(object)owner.Skill.GetSkill(HeroSkillLocation.Q) == (UnityEngine.Object)(object)this)
		{
			heroSkillLocation = HeroSkillLocation.Q;
		}
		else if ((UnityEngine.Object)(object)owner.Skill.GetSkill(HeroSkillLocation.W) == (UnityEngine.Object)(object)this)
		{
			heroSkillLocation = HeroSkillLocation.W;
		}
		else if ((UnityEngine.Object)(object)owner.Skill.GetSkill(HeroSkillLocation.E) == (UnityEngine.Object)(object)this)
		{
			heroSkillLocation = HeroSkillLocation.E;
		}
		else if ((UnityEngine.Object)(object)owner.Skill.GetSkill(HeroSkillLocation.R) == (UnityEngine.Object)(object)this)
		{
			heroSkillLocation = HeroSkillLocation.R;
		}
		else if ((UnityEngine.Object)(object)owner.Skill.GetSkill(HeroSkillLocation.Identity) == (UnityEngine.Object)(object)this)
		{
			heroSkillLocation = HeroSkillLocation.Identity;
		}
		else
		{
			if (!((UnityEngine.Object)(object)owner.Skill.GetSkill(HeroSkillLocation.Movement) == (UnityEngine.Object)(object)this))
			{
				return result;
			}
			heroSkillLocation = HeroSkillLocation.Movement;
		}
		InvokeOnSkillUse(new EventInfoSkillUse
		{
			skill = this,
			type = heroSkillLocation
		});
		return result;
	}

	[ClientRpc]
	private void InvokeOnSkillUse(EventInfoSkillUse info)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_EventInfoSkillUse((NetworkWriter)(object)val, info);
		((NetworkBehaviour)this).SendRPCInternal("System.Void SkillTrigger::InvokeOnSkillUse(EventInfoSkillUse)", 2077614875, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	protected virtual void OnLevelChange(int oldLevel, int newLevel)
	{
		if (newLevel < 1)
		{
			return;
		}
		if (((NetworkBehaviour)this).isServer)
		{
			Network_003CmaxSellGold_003Ek__BackingField = int.MaxValue;
			UpdatePassiveEffectIfNeccessary();
			if (_skillHastePerLevelBonus != null)
			{
				UpdateSkillHastePerLevelBonus();
			}
			UpdateSkillBonus();
		}
		ClientSkillEvent_OnLevelChange?.Invoke(oldLevel, newLevel);
		if ((UnityEngine.Object)(object)owner != null)
		{
			owner.Skill.ClientHeroEvent_OnSkillLevelChanged?.Invoke(this, oldLevel, newLevel);
		}
	}

	protected override void OnEquip(Entity newOwner)
	{
		base.OnEquip(newOwner);
		if (((NetworkBehaviour)this).isServer)
		{
			Network_003CtempOwner_003Ek__BackingField = null;
			Network_003CskipStartAnimation_003Ek__BackingField = true;
			newOwner.Status.AddStatBonus(statBonus);
			if (isCharacterSkill)
			{
				Network_003CcharacterSkillOwner_003Ek__BackingField = newOwner.owner.guid;
			}
			newOwner.ActorEvent_OnDealDamage += new Action<EventInfoDamage>(ActorEventOnDealDamage);
			newOwner.EntityEvent_OnTakeDamage += new Action<EventInfoDamage>(EntityEventOnTakeDamage);
			newOwner.ActorEvent_OnKill += new Action<EventInfoKill>(ActorEventOnKill);
			newOwner.EntityEvent_OnAttackHit += new Action<EventInfoAttackHit>(EntityEventOnAttackHit);
		}
		FxPlay(startEffect, newOwner);
	}

	protected override void OnUnequip(Entity formerOwner)
	{
		base.OnUnequip(formerOwner);
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)formerOwner != null)
		{
			formerOwner.Status.RemoveStatBonus(statBonus);
			formerOwner.ActorEvent_OnDealDamage -= new Action<EventInfoDamage>(ActorEventOnDealDamage);
			formerOwner.EntityEvent_OnTakeDamage -= new Action<EventInfoDamage>(EntityEventOnTakeDamage);
			formerOwner.ActorEvent_OnKill -= new Action<EventInfoKill>(ActorEventOnKill);
			formerOwner.EntityEvent_OnAttackHit -= new Action<EventInfoAttackHit>(EntityEventOnAttackHit);
		}
		FxPlay(endEffect, formerOwner);
		FxStop(startEffect);
	}

	private void EntityEventOnAttackHit(EventInfoAttackHit obj)
	{
		if (obj.victim is Monster && type == SkillType.Ultimate && !obj.victim.Status.hasDamageImmunity)
		{
			ChargeUltimate(0.2125f * obj.strength);
		}
	}

	private void ActorEventOnKill(EventInfoKill obj)
	{
		if (type == SkillType.Ultimate && obj.victim is Monster monster)
		{
			switch (monster.type)
			{
			case Monster.MonsterType.Lesser:
				ChargeUltimate(0.12750001f);
				break;
			case Monster.MonsterType.Normal:
				ChargeUltimate(0.63750005f);
				break;
			case Monster.MonsterType.MiniBoss:
				ChargeUltimate(12.75f);
				break;
			case Monster.MonsterType.Boss:
				ChargeUltimate(42.5f);
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}
	}

	private void ActorEventOnDealDamage(EventInfoDamage obj)
	{
		if (type == SkillType.Ultimate && obj.victim is Monster monster && !obj.victim.Status.hasDamageImmunity)
		{
			float num = ((obj.actor is ElementalStatusEffect) ? 0.0425f : 0.2125f);
			float num2 = obj.damage.amount / monster.maxHealth;
			ChargeUltimate(monster.type switch
			{
				Monster.MonsterType.Lesser => num + num2 * 0.12750001f, 
				Monster.MonsterType.Normal => num + num2 * 0.25500003f, 
				Monster.MonsterType.MiniBoss => num + num2 * 12.75f, 
				Monster.MonsterType.Boss => num + num2 * 25.5f, 
				_ => throw new ArgumentOutOfRangeException(), 
			});
		}
	}

	private void EntityEventOnTakeDamage(EventInfoDamage obj)
	{
		if (type == SkillType.Ultimate)
		{
			float num = ((obj.actor is ElementalStatusEffect) ? 0.0425f : 0.2125f);
			num += 25.5f * obj.damage.amount / owner.maxHealthWithoutBonus;
			ChargeUltimate(num);
		}
	}

	[Server]
	private void ChargeUltimate(float percentage)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void SkillTrigger::ChargeUltimate(System.Single)' called when server was not active");
		}
		else
		{
			if (this.IsNullOrInactive() || type != SkillType.Ultimate)
			{
				return;
			}
			percentage /= sbCooldownMultiplier;
			percentage /= AbilityTrigger.GetCooldownTimeMultiplierByAbilityHaste(owner.Status.abilityHaste);
			percentage *= 100f / (configs[0].cooldownTime + sbCooldownOffset);
			int num = 0;
			if (!configs[num].canReceiveCooldownReduction || _configCooldownLocks[num])
			{
				return;
			}
			float num2 = percentage * (configs[num].cooldownTime + sbCooldownOffset) / 100f;
			while (num2 > 0f && currentCharges[num] < configs[num].maxCharges)
			{
				if (currentCharges[num] == 0)
				{
					currentUnscaledCooldownTimes[num] -= num2;
					if (currentUnscaledCooldownTimes[num] < 0f)
					{
						currentUnscaledCooldownTimes[num] = 0f;
					}
					break;
				}
				currentUnscaledCooldownTimes[num] -= num2;
				num2 = 0f;
				if (currentUnscaledCooldownTimes[num] < 0f)
				{
					num2 = 0f - currentUnscaledCooldownTimes[num];
					currentCharges[num] = Mathf.Min(currentCharges[num] + configs[num].addedCharges, configs[num].maxCharges);
					if (num == currentConfigIndex)
					{
						InvokeCurrentConfigCharged();
					}
					if (currentCharges[num] >= configs[num].maxCharges)
					{
						currentUnscaledCooldownTimes[num] = 0f;
						break;
					}
					currentUnscaledCooldownTimes[num] = GetMaxCooldownTime(num, scaled: false);
				}
			}
			RpcSetCooldownTime(num, currentUnscaledCooldownTimes[num]);
			RpcSetCharge(num, currentCharges[num]);
		}
	}

	public override string GetActorReadableName()
	{
		return $"{base.GetActorReadableName()} ({level})";
	}

	[Server]
	public void RpcSetPositionAndRotation(Vector3 pos, Quaternion rot)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void SkillTrigger::RpcSetPositionAndRotation(UnityEngine.Vector3,UnityEngine.Quaternion)' called when server was not active");
			return;
		}
		((Component)(object)this).transform.SetPositionAndRotation(pos, rot);
		Physics.SyncTransforms();
		RpcSetPositionAndRotation_Imp(pos, rot);
	}

	[ClientRpc]
	private void RpcSetPositionAndRotation_Imp(Vector3 pos, Quaternion rot)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteVector3((NetworkWriter)(object)val, pos);
		NetworkWriterExtensions.WriteQuaternion((NetworkWriter)(object)val, rot);
		((NetworkBehaviour)this).SendRPCInternal("System.Void SkillTrigger::RpcSetPositionAndRotation_Imp(UnityEngine.Vector3,UnityEngine.Quaternion)", 1362066538, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	public bool CanInteract(Entity entity)
	{
		if (!isAnimatingEmbraceNewIdentity && (UnityEngine.Object)(object)owner == null && (UnityEngine.Object)(object)Network_003ChandOwner_003Ek__BackingField == null && !_worldModel.isAnimating && Time.time - creationTime > 0.5f && !IsLockedFor(entity.owner))
		{
			return !InGameUIManager.instance.isDoingEnding;
		}
		return false;
	}

	void IInteractable.OnInteract(Entity entity, bool alt)
	{
		if (!(entity is Hero hero) || IsLockedFor(entity.owner))
		{
			return;
		}
		if (alt)
		{
			if (((NetworkBehaviour)hero).isOwned && !ManagerBase<ControlManager>.instance.isDismantleDisabled && (ManagerBase<ControlManager>.instance.dismantleConstraint == null || ManagerBase<ControlManager>.instance.dismantleConstraint((UnityEngine.Object)(object)this)))
			{
				CmdDoDismantleTap();
			}
			return;
		}
		if (isCharacterSkill && !string.IsNullOrEmpty(characterSkillOwner) && characterSkillOwner != hero.owner.guid)
		{
			if (((NetworkBehaviour)entity).isOwned)
			{
				InGameUIManager.instance.ShowCenterMessage(CenterMessageType.Error, "InGame_Message_CanOnlyEquipYourOwnCharacterSkill");
			}
			return;
		}
		if (rarity == Rarity.Identity)
		{
			Type loadoutHero = GetLoadoutHero();
			if (!DisableHeroTypeCheckOnIdentityMemories && loadoutHero != null && ((object)entity).GetType() != loadoutHero)
			{
				if (((NetworkBehaviour)entity).isOwned)
				{
					InGameUIManager.instance.ShowCenterMessage(CenterMessageType.Error, "InGame_Message_CanOnlyEquipIdentitySkillOfSameTraveler");
				}
			}
			else
			{
				if (!((NetworkBehaviour)entity).isOwned)
				{
					return;
				}
				string rawContent = (hero.Skill.Identity.IsNullOrInactive() ? DewLocalization.GetUIValue("InGame_Message_ConfirmEmbraceNewIdentity_NoDestroy") : string.Format(DewLocalization.GetUIValue("InGame_Message_ConfirmEmbraceNewIdentity"), hero.Skill.Identity.GetFormattedSkillTitle(), GetFormattedSkillTitle()));
				ManagerBase<MessageManager>.instance.ShowMessage(new DewMessageSettings
				{
					owner = (UnityEngine.Object)(object)this,
					rawContent = rawContent,
					buttons = (DewMessageSettings.ButtonType.Yes | DewMessageSettings.ButtonType.No),
					defaultButton = DewMessageSettings.ButtonType.No,
					destructiveConfirm = true,
					onClose = (DewMessageSettings.ButtonType b) =>
					{
						if (b == DewMessageSettings.ButtonType.Yes)
						{
							CmdEquipIdentitySkill();
						}
					},
					validator = () => !this.IsNullOrInactive() && (UnityEngine.Object)(object)owner == null && !isAnimatingEmbraceNewIdentity && InGameUIManager.ValidateInGameActionMessage()
				});
			}
			return;
		}
		HeroSkillLocation? heroSkillLocation = null;
		if (!hero.Skill.TryGetSkill(skillType, out var _))
		{
			heroSkillLocation = skillType;
		}
		else if ((UnityEngine.Object)(object)hero.Skill.Q == null)
		{
			heroSkillLocation = HeroSkillLocation.Q;
		}
		else if ((UnityEngine.Object)(object)hero.Skill.W == null)
		{
			heroSkillLocation = HeroSkillLocation.W;
		}
		else if ((UnityEngine.Object)(object)hero.Skill.E == null)
		{
			heroSkillLocation = HeroSkillLocation.E;
		}
		else if ((UnityEngine.Object)(object)hero.Skill.R == null)
		{
			heroSkillLocation = HeroSkillLocation.R;
		}
		if (heroSkillLocation.HasValue)
		{
			if (((NetworkBehaviour)this).isServer)
			{
				hero.Skill.EquipSkill(heroSkillLocation.Value, this);
				hero.Skill.RpcInvokeOnSkillPickup(this);
			}
			if (((NetworkBehaviour)entity).isOwned)
			{
				ManagerBase<FeedbackManager>.instance.PlayFeedbackEffect("UI_EditSkill_SkillEquip");
			}
		}
		else if (((NetworkBehaviour)this).isServer)
		{
			hero.Skill.HoldInHand(this);
		}
	}

	private void OnDismantleProgressChanged(float oldVal, float newVal)
	{
		ClientSkillEvent_OnDismantleProgressChanged?.Invoke(oldVal, newVal);
		if (((NetworkBehaviour)this).isServer && newVal >= 1f && (UnityEngine.Object)(object)owner == null && (UnityEngine.Object)(object)Network_003ChandOwner_003Ek__BackingField == null && isActive)
		{
			DismantleSkill();
		}
	}

	[Server]
	public void DismantleSkill()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void SkillTrigger::DismantleSkill()' called when server was not active");
		}
		else if (!((UnityEngine.Object)(object)_lastDismantler == null) && !((UnityEngine.Object)(object)_lastDismantler.owner == null))
		{
			int data = GetDismantleAmount(_lastDismantler.owner);
			_lastDismantler.dismantleProcessor.Process(ref data, _lastDismantler, this);
			_lastDismantler.HeroEvent_OnDismantleItem?.Invoke(new EventInfoDismantle
			{
				target = this,
				dismantler = _lastDismantler,
				dismantleAmount = data
			});
			NetworkedManagerBase<PickupManager>.instance.DropDreamDust(isGivenByOtherPlayer: false, data, position, _lastDismantler);
			NetworkedManagerBase<ClientEventManager>.instance.InvokeOnDismantled(_lastDismantler, (NetworkBehaviour)(object)this);
			Destroy();
		}
	}

	public int GetDismantleAmount(DewPlayer player)
	{
		if ((UnityEngine.Object)(object)player == null)
		{
			player = _lastDismantler.owner;
		}
		float num = NetworkedManagerBase<GameManager>.instance.ges.skillDismantleDreamDustByLevel.Evaluate(level);
		num *= NetworkedManagerBase<GameManager>.instance.ges.skillDismantleDreamDustMultiplier.Get(rarity);
		if ((UnityEngine.Object)(object)player != null)
		{
			num *= player.dismantleDreamDustMultiplier * player.dismantleSkillDreamDustMultiplier;
		}
		num *= DewBuildProfile.current.dismantleDreamDustMultiplier;
		return Mathf.Max(1, Mathf.RoundToInt(num));
	}

	[Command(requiresAuthority = false)]
	private void CmdDoDismantleTap(NetworkConnectionToClient sender = null)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendCommandInternal("System.Void SkillTrigger::CmdDoDismantleTap(Mirror.NetworkConnectionToClient)", -275473734, (NetworkWriter)(object)val, 0, false);
		NetworkWriterPool.Return(val);
	}

	[Command(requiresAuthority = false)]
	private void CmdEquipIdentitySkill(NetworkConnectionToClient sender = null)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendCommandInternal("System.Void SkillTrigger::CmdEquipIdentitySkill(Mirror.NetworkConnectionToClient)", -289398032, (NetworkWriter)(object)val, 0, false);
		NetworkWriterPool.Return(val);
	}

	private void DoDismantleLogicUpdate()
	{
		if (!(dismantleProgress <= 0f) && (Time.time - _lastDismantleTapTime > 1f || (UnityEngine.Object)(object)owner != null || (UnityEngine.Object)(object)Network_003ChandOwner_003Ek__BackingField != null))
		{
			Network_003CdismantleProgress_003Ek__BackingField = 0f;
		}
	}

	public float GetCooldownTimeOnLevel(int lvl)
	{
		if (!configs[0].canReceiveCooldownReduction)
		{
			return Mathf.Max(0f, GetMaxCooldownTime(0, scaled: false));
		}
		Hero hero = null;
		if ((UnityEngine.Object)(object)((NetworkBehaviour)this).netIdentity != null && (UnityEngine.Object)(object)owner != null)
		{
			hero = owner;
		}
		else if ((bool)(UnityEngine.Object)(object)DewPlayer.local)
		{
			hero = DewPlayer.local.hero;
		}
		float num = (((UnityEngine.Object)(object)hero != null) ? AbilityTrigger.GetCooldownTimeMultiplierByAbilityHaste(hero.Status.abilityHaste) : 1f);
		float cooldownMultiplierOfSkillHastePerLevelBonus = GetCooldownMultiplierOfSkillHastePerLevelBonus(lvl);
		float num2 = (configs[0].cooldownTime + sbCooldownOffset) * num * cooldownMultiplierOfSkillHastePerLevelBonus * sbCooldownMultiplier;
		num2 /= _skillHasteGrantedByLevelMultiplier;
		return Mathf.Max(0f, num2);
	}

	public int GetBuyGold()
	{
		return GetBuyGold(rarity, level);
	}

	public int GetSellGold()
	{
		return Mathf.Min(GetSellGold(rarity, level), maxSellGold);
	}

	public static int GetBuyGold(Rarity rarity, int level)
	{
		float amount = GetGoldValue(rarity, level);
		amount = NetworkedManagerBase<GameManager>.instance.GetAdjustedGoldAmount_Cost(amount);
		return Mathf.Max(Mathf.RoundToInt(amount), 1);
	}

	public static int GetSellGold(Rarity rarity, int level)
	{
		float num = GetGoldValue(rarity, level);
		num *= NetworkedManagerBase<GameManager>.instance.ges.sellValueMultiplier;
		num = NetworkedManagerBase<GameManager>.instance.GetAdjustedGoldAmount_Income(num);
		return Mathf.Max(Mathf.RoundToInt(num), 1);
	}

	public static int GetGoldValue(Rarity rarity, int level)
	{
		DewGameplayExperienceSettings ges = NetworkedManagerBase<GameManager>.instance.ges;
		float num = ges.skillValue.Get(rarity);
		num *= ges.valueGlobalMultiplier;
		num *= 1f + (ges.valueMultiplierPerSkillLevel - 1f) * (float)(level - 1);
		num = NetworkedManagerBase<GameManager>.instance.GetAdjustedGoldAmount(num);
		return Mathf.Max(Mathf.RoundToInt(num), 1);
	}

	protected override bool ShouldTickCooldown(int configIndex)
	{
		if (type != SkillType.Ultimate || configIndex != 0)
		{
			return base.ShouldTickCooldown(configIndex);
		}
		return false;
	}

	public override bool ShouldBeSavedWithRoom()
	{
		return (UnityEngine.Object)(object)owner == null;
	}

	public string GetFormattedSkillTitle()
	{
		string rarityColorHex = Dew.GetRarityColorHex(rarity);
		string skillName = DewLocalization.GetSkillName(DewLocalization.GetSkillKey(((object)this).GetType()), 0);
		return "<color=" + rarityColorHex + ">" + string.Format(DewLocalization.GetSkillLevelTemplate(level), skillName) + "</color>";
	}

	public Type GetLoadoutHero()
	{
		foreach (Hero item in DewResources.FindAllByType<Hero>(ResourceLoadSettings.Light))
		{
			HeroSkill component = ((Component)(object)item).GetComponent<HeroSkill>();
			foreach (AssetRef<SkillTrigger> item2 in component.loadoutQ.Concat(component.loadoutR).Concat(component.loadoutMovement).Concat(component.loadoutTrait))
			{
				if (Dew.IsSkillIncludedInGame(item2.typeName) && item2.type == ((object)this).GetType())
				{
					return ((object)item).GetType();
				}
			}
		}
		return null;
	}

	public DamageData DefaultDamage(ScalingValue value, float procCoefficient = 1f)
	{
		return CreateDamage(DamageData.SourceType.Default, value, procCoefficient);
	}

	public DamageData PhysicalDamage(ScalingValue value, float procCoefficient = 1f)
	{
		return CreateDamage(DamageData.SourceType.Physical, value, procCoefficient);
	}

	public DamageData MagicDamage(ScalingValue value, float procCoefficient = 1f)
	{
		return CreateDamage(DamageData.SourceType.Magic, value, procCoefficient);
	}

	public DamageData PhysicalMagicDamage(ScalingValue value, float procCoefficient = 1f)
	{
		return CreateDamage(DamageData.SourceType.Physical | DamageData.SourceType.Magic, value, procCoefficient);
	}

	public DamageData PureDamage(ScalingValue value, float procCoefficient = 1f)
	{
		return CreateDamage(DamageData.SourceType.Pure, value, procCoefficient);
	}

	public DamageData Damage(ScalingValue value, float procCoefficient = 1f)
	{
		DamageData.SourceType sourceType = DamageData.SourceType.Default;
		if (value.adFactor > 0f)
		{
			sourceType |= DamageData.SourceType.Physical;
		}
		else if (value.apFactor > 0f)
		{
			sourceType |= DamageData.SourceType.Magic;
		}
		return CreateDamage(sourceType, value, procCoefficient);
	}

	public DamageData CreateDamage(DamageData.SourceType type, ScalingValue value, float procCoefficient = 1f)
	{
		return new DamageData(type, value, statEntity, effectiveLevel, procCoefficient).SetActor(this);
	}

	public HealData Heal(ScalingValue amount)
	{
		return new HealData(GetValue(amount)).SetActor(this);
	}

	public float GetValue(ScalingValue val)
	{
		return val.GetValue(effectiveLevel, statEntity);
	}

	[Server]
	public SkillBonus AddSkillBonus(SkillBonus bonus)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'SkillBonus SkillTrigger::AddSkillBonus(SkillBonus)' called when server was not active");
			return null;
		}
		if (_grantedSkillBonus == null)
		{
			_grantedSkillBonus = new List<SkillBonus>();
		}
		if ((UnityEngine.Object)(object)bonus.parent != null)
		{
			Debug.LogWarning($"SkillBonus for {this} already has a parent");
			return bonus;
		}
		bonus.parent = this;
		_grantedSkillBonus.Add(bonus);
		UpdateSkillBonus();
		return bonus;
	}

	[Server]
	public void RemoveSkillBonus(SkillBonus bonus)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void SkillTrigger::RemoveSkillBonus(SkillBonus)' called when server was not active");
			return;
		}
		bonus.parent = null;
		_grantedSkillBonus.Remove(bonus);
		UpdateSkillBonus();
	}

	[Server]
	public void UpdateSkillBonus()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void SkillTrigger::UpdateSkillBonus()' called when server was not active");
			return;
		}
		Network_003CsbCooldownMultiplier_003Ek__BackingField = 1f;
		Network_003CsbCooldownMultiplierIgnoreReceiveCooldownReductionFlag_003Ek__BackingField = 1f;
		Network_003CsbCooldownOffset_003Ek__BackingField = 0f;
		Network_003CsbCooldownOffsetIgnoreReceiveCooldownReductionFlag_003Ek__BackingField = 0f;
		_isSkillBonusDirty = false;
		if (_grantedSkillBonus == null)
		{
			return;
		}
		int num = mainConfigOriginalCharge;
		for (int i = 0; i < _grantedSkillBonus.Count; i++)
		{
			sbCooldownMultiplier *= _grantedSkillBonus[i].cooldownMultiplier;
			sbCooldownOffset += _grantedSkillBonus[i].cooldownOffset;
			if (_grantedSkillBonus[i].ignoreReceiveCooldownReductionFlag)
			{
				sbCooldownMultiplierIgnoreReceiveCooldownReductionFlag *= _grantedSkillBonus[i].cooldownMultiplier;
				sbCooldownOffsetIgnoreReceiveCooldownReductionFlag += _grantedSkillBonus[i].cooldownOffset;
			}
			num += _grantedSkillBonus[i].addedCharge;
		}
		if (num != configs[0].maxCharges)
		{
			if (num > configs[0].maxCharges && currentConfigUnscaledCooldownTime == 0f)
			{
				SetCooldownTime(0, GetMaxCooldownTime(0, scaled: false), scaled: false);
			}
			configs[0].maxCharges = num;
		}
		Network_skillHasteGrantedByLevelMultiplier = ((_skillHastePerLevelBonus != null) ? _skillHastePerLevelBonus.cooldownMultiplier : 1f);
	}

	private void DoSkillBonusLogicUpdate()
	{
		if (_isSkillBonusDirty)
		{
			UpdateSkillBonus();
		}
	}

	public SkillTrigger()
	{
		_Mirror_SyncVarHookDelegate__003ChandOwner_003Ek__BackingField = OnHandOwnerChanged;
		_Mirror_SyncVarHookDelegate__003CtempOwner_003Ek__BackingField = OnTempOwnerChanged;
		_Mirror_SyncVarHookDelegate__level = OnLevelChange;
		_Mirror_SyncVarHookDelegate__003CdismantleProgress_003Ek__BackingField = OnDismantleProgressChanged;
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_InvokeOnSkillUse__EventInfoSkillUse(EventInfoSkillUse info)
	{
		if (!((UnityEngine.Object)(object)owner == null))
		{
			owner.ClientHeroEvent_OnSkillUse?.Invoke(info);
		}
	}

	protected static void InvokeUserCode_InvokeOnSkillUse__EventInfoSkillUse(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC InvokeOnSkillUse called on server.");
		}
		else
		{
			((SkillTrigger)(object)obj).UserCode_InvokeOnSkillUse__EventInfoSkillUse(GeneratedNetworkCode._Read_EventInfoSkillUse(reader));
		}
	}

	protected void UserCode_RpcSetPositionAndRotation_Imp__Vector3__Quaternion(Vector3 pos, Quaternion rot)
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			((Component)(object)this).transform.SetPositionAndRotation(pos, rot);
		}
		Physics.SyncTransforms();
	}

	protected static void InvokeUserCode_RpcSetPositionAndRotation_Imp__Vector3__Quaternion(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetPositionAndRotation_Imp called on server.");
		}
		else
		{
			((SkillTrigger)(object)obj).UserCode_RpcSetPositionAndRotation_Imp__Vector3__Quaternion(NetworkReaderExtensions.ReadVector3(reader), NetworkReaderExtensions.ReadQuaternion(reader));
		}
	}

	protected void UserCode_CmdDoDismantleTap__NetworkConnectionToClient(NetworkConnectionToClient sender)
	{
		if (!((UnityEngine.Object)(object)owner != null) && !((UnityEngine.Object)(object)Network_003ChandOwner_003Ek__BackingField != null) && !(Time.time - _lastDismantleTapTime < 0.075f) && !((UnityEngine.Object)(object)sender.GetHero() == null))
		{
			_lastDismantleTapTime = Time.time;
			dismantleProgress += 0.4f;
			_lastDismantler = sender.GetHero();
		}
	}

	protected static void InvokeUserCode_CmdDoDismantleTap__NetworkConnectionToClient(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdDoDismantleTap called on client.");
		}
		else
		{
			((SkillTrigger)(object)obj).UserCode_CmdDoDismantleTap__NetworkConnectionToClient(senderConnection);
		}
	}

	protected void UserCode_CmdEquipIdentitySkill__NetworkConnectionToClient(NetworkConnectionToClient sender)
	{
		if ((UnityEngine.Object)(object)owner != null || (UnityEngine.Object)(object)Network_003ChandOwner_003Ek__BackingField != null)
		{
			return;
		}
		DewPlayer player = sender.GetPlayer();
		if (!(UnityEngine.Object)(object)player || IsLockedFor(player))
		{
			return;
		}
		Hero hero = sender.GetHero();
		if (!hero.IsNullInactiveDeadOrKnockedOut())
		{
			Network_003CisAnimatingEmbraceNewIdentity_003Ek__BackingField = true;
			hero.CreateAbilityInstance(position, null, new CastInfo(hero), (Ai_EmbraceNewIdentity ai) =>
			{
				ai.NetworktargetSkill = this;
			});
		}
	}

	protected static void InvokeUserCode_CmdEquipIdentitySkill__NetworkConnectionToClient(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdEquipIdentitySkill called on client.");
		}
		else
		{
			((SkillTrigger)(object)obj).UserCode_CmdEquipIdentitySkill__NetworkConnectionToClient(senderConnection);
		}
	}

	static SkillTrigger()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected Obj, but got Unknown
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Expected Obj, but got Unknown
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Expected Obj, but got Unknown
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterCommand(typeof(SkillTrigger), "System.Void SkillTrigger::CmdDoDismantleTap(Mirror.NetworkConnectionToClient)", (RemoteCallDelegate)InvokeUserCode_CmdDoDismantleTap__NetworkConnectionToClient, false);
		RemoteProcedureCalls.RegisterCommand(typeof(SkillTrigger), "System.Void SkillTrigger::CmdEquipIdentitySkill(Mirror.NetworkConnectionToClient)", (RemoteCallDelegate)InvokeUserCode_CmdEquipIdentitySkill__NetworkConnectionToClient, false);
		RemoteProcedureCalls.RegisterRpc(typeof(SkillTrigger), "System.Void SkillTrigger::InvokeOnSkillUse(EventInfoSkillUse)", (RemoteCallDelegate)InvokeUserCode_InvokeOnSkillUse__EventInfoSkillUse);
		RemoteProcedureCalls.RegisterRpc(typeof(SkillTrigger), "System.Void SkillTrigger::RpcSetPositionAndRotation_Imp(UnityEngine.Vector3,UnityEngine.Quaternion)", (RemoteCallDelegate)InvokeUserCode_RpcSetPositionAndRotation_Imp__Vector3__Quaternion);
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteBool(writer, skipStartAnimation__BackingField);
			NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)Network_003ChandOwner_003Ek__BackingField);
			NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)Network_003CtempOwner_003Ek__BackingField);
			GeneratedNetworkCode._Write_HeroSkillLocation(writer, skillType__BackingField);
			NetworkWriterExtensions.WriteString(writer, characterSkillOwner__BackingField);
			NetworkWriterExtensions.WriteInt(writer, _level);
			GeneratedNetworkCode._Write_SkillType(writer, type);
			GeneratedNetworkCode._Write_DescriptionTags(writer, tags);
			NetworkWriterExtensions.WriteFloat(writer, _skillHasteGrantedByLevelMultiplier);
			NetworkWriterExtensions.WriteColor(writer, specialOverlayColor__BackingField);
			NetworkWriterExtensions.WriteBool(writer, isAnimatingEmbraceNewIdentity__BackingField);
			NetworkWriterExtensions.WriteFloat(writer, dismantleProgress__BackingField);
			NetworkWriterExtensions.WriteInt(writer, maxSellGold__BackingField);
			NetworkWriterExtensions.WriteFloat(writer, sbCooldownMultiplier__BackingField);
			NetworkWriterExtensions.WriteFloat(writer, sbCooldownMultiplierIgnoreReceiveCooldownReductionFlag__BackingField);
			NetworkWriterExtensions.WriteFloat(writer, sbCooldownOffset__BackingField);
			NetworkWriterExtensions.WriteFloat(writer, sbCooldownOffsetIgnoreReceiveCooldownReductionFlag__BackingField);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x400L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, skipStartAnimation__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x800L) != 0L)
		{
			NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)Network_003ChandOwner_003Ek__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x1000L) != 0L)
		{
			NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)Network_003CtempOwner_003Ek__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x2000L) != 0L)
		{
			GeneratedNetworkCode._Write_HeroSkillLocation(writer, skillType__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x4000L) != 0L)
		{
			NetworkWriterExtensions.WriteString(writer, characterSkillOwner__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x8000L) != 0L)
		{
			NetworkWriterExtensions.WriteInt(writer, _level);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x10000L) != 0L)
		{
			GeneratedNetworkCode._Write_SkillType(writer, type);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x20000L) != 0L)
		{
			GeneratedNetworkCode._Write_DescriptionTags(writer, tags);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x40000L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, _skillHasteGrantedByLevelMultiplier);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x80000L) != 0L)
		{
			NetworkWriterExtensions.WriteColor(writer, specialOverlayColor__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x100000L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, isAnimatingEmbraceNewIdentity__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x200000L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, dismantleProgress__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x400000L) != 0L)
		{
			NetworkWriterExtensions.WriteInt(writer, maxSellGold__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x800000L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, sbCooldownMultiplier__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x1000000L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, sbCooldownMultiplierIgnoreReceiveCooldownReductionFlag__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x2000000L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, sbCooldownOffset__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x4000000L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, sbCooldownOffsetIgnoreReceiveCooldownReductionFlag__BackingField);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref skipStartAnimation__BackingField, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize_NetworkBehaviour<Hero>(ref handOwner__BackingField, _Mirror_SyncVarHookDelegate__003ChandOwner_003Ek__BackingField, reader, ref ____003ChandOwner_003Ek__BackingFieldNetId);
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize_NetworkBehaviour<DewPlayer>(ref tempOwner__BackingField, _Mirror_SyncVarHookDelegate__003CtempOwner_003Ek__BackingField, reader, ref ____003CtempOwner_003Ek__BackingFieldNetId);
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<HeroSkillLocation>(ref skillType__BackingField, (Action<HeroSkillLocation, HeroSkillLocation>)null, GeneratedNetworkCode._Read_HeroSkillLocation(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<string>(ref characterSkillOwner__BackingField, (Action<string, string>)null, NetworkReaderExtensions.ReadString(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref _level, _Mirror_SyncVarHookDelegate__level, NetworkReaderExtensions.ReadInt(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<SkillType>(ref type, (Action<SkillType, SkillType>)null, GeneratedNetworkCode._Read_SkillType(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<DescriptionTags>(ref tags, (Action<DescriptionTags, DescriptionTags>)null, GeneratedNetworkCode._Read_DescriptionTags(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _skillHasteGrantedByLevelMultiplier, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<Color>(ref specialOverlayColor__BackingField, (Action<Color, Color>)null, NetworkReaderExtensions.ReadColor(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref isAnimatingEmbraceNewIdentity__BackingField, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref dismantleProgress__BackingField, _Mirror_SyncVarHookDelegate__003CdismantleProgress_003Ek__BackingField, NetworkReaderExtensions.ReadFloat(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref maxSellGold__BackingField, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref sbCooldownMultiplier__BackingField, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref sbCooldownMultiplierIgnoreReceiveCooldownReductionFlag__BackingField, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref sbCooldownOffset__BackingField, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref sbCooldownOffsetIgnoreReceiveCooldownReductionFlag__BackingField, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x400L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref skipStartAnimation__BackingField, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
		}
		if ((num & 0x800L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize_NetworkBehaviour<Hero>(ref handOwner__BackingField, _Mirror_SyncVarHookDelegate__003ChandOwner_003Ek__BackingField, reader, ref ____003ChandOwner_003Ek__BackingFieldNetId);
		}
		if ((num & 0x1000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize_NetworkBehaviour<DewPlayer>(ref tempOwner__BackingField, _Mirror_SyncVarHookDelegate__003CtempOwner_003Ek__BackingField, reader, ref ____003CtempOwner_003Ek__BackingFieldNetId);
		}
		if ((num & 0x2000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<HeroSkillLocation>(ref skillType__BackingField, (Action<HeroSkillLocation, HeroSkillLocation>)null, GeneratedNetworkCode._Read_HeroSkillLocation(reader));
		}
		if ((num & 0x4000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<string>(ref characterSkillOwner__BackingField, (Action<string, string>)null, NetworkReaderExtensions.ReadString(reader));
		}
		if ((num & 0x8000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref _level, _Mirror_SyncVarHookDelegate__level, NetworkReaderExtensions.ReadInt(reader));
		}
		if ((num & 0x10000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<SkillType>(ref type, (Action<SkillType, SkillType>)null, GeneratedNetworkCode._Read_SkillType(reader));
		}
		if ((num & 0x20000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<DescriptionTags>(ref tags, (Action<DescriptionTags, DescriptionTags>)null, GeneratedNetworkCode._Read_DescriptionTags(reader));
		}
		if ((num & 0x40000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _skillHasteGrantedByLevelMultiplier, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
		if ((num & 0x80000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<Color>(ref specialOverlayColor__BackingField, (Action<Color, Color>)null, NetworkReaderExtensions.ReadColor(reader));
		}
		if ((num & 0x100000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref isAnimatingEmbraceNewIdentity__BackingField, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
		}
		if ((num & 0x200000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref dismantleProgress__BackingField, _Mirror_SyncVarHookDelegate__003CdismantleProgress_003Ek__BackingField, NetworkReaderExtensions.ReadFloat(reader));
		}
		if ((num & 0x400000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref maxSellGold__BackingField, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
		}
		if ((num & 0x800000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref sbCooldownMultiplier__BackingField, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
		if ((num & 0x1000000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref sbCooldownMultiplierIgnoreReceiveCooldownReductionFlag__BackingField, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
		if ((num & 0x2000000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref sbCooldownOffset__BackingField, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
		if ((num & 0x4000000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref sbCooldownOffsetIgnoreReceiveCooldownReductionFlag__BackingField, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
	}
}
