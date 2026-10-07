using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class Gem : Actor, IInteractable, IItem, IExcludeFromPool
{
	public enum QualityType : byte
	{
		Chipped,
		Flawed,
		Regular,
		Flawless,
		Perfect,
		Otherworldly,
		Transcendent
	}

	public const float GemMergeExponent = 1f;

	public SafeAction ClientGemEvent_OnCooldownStarted;

	public SafeAction<float> ClientGemEvent_OnCooldownReduced;

	public SafeAction<float> ClientGemEvent_OnCooldownReducedByRatio;

	public SafeAction ClientGemEvent_OnCooldownReady;

	public SafeAction ClientGemEvent_OnFlash;

	public SafeAction<int, int> ClientGemEvent_OnQualityChanged;

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
	[SyncVar(hook = "OnOwnerChanged")]
	private Hero owner__BackingField;

	public SafeAction<Hero, Hero> ClientEvent_OnOwnerChanged;

	[CompilerGenerated]
	[SyncVar]
	private GemLocation location__BackingField;

	[CompilerGenerated]
	[SyncVar]
	private float fillAmount__BackingField;

	[CompilerGenerated]
	[SyncVar(hook = "OnSkillChanged")]
	private SkillTrigger skill__BackingField;

	[SyncVar(hook = "OnQualityChange")]
	private int _quality = -1;

	public Sprite icon;

	public Rarity rarity;

	public DescriptionTags tags;

	public bool isCooldownEnabled;

	public ScalingValue cooldownTime;

	public bool isRateLimited;

	public ScalingValue rateLimitTime;

	public ScalingValue rateLimitCount;

	public bool setNumberDisplay = true;

	public GameObject readyEffect;

	public bool enableStatBonus;

	public StatBonus statBonus;

	public bool excludeFromPool;

	protected AbilityTargetValidatorWrapper tvDefaultHarmfulEffectTargets;

	protected AbilityTargetValidatorWrapper tvDefaultUsefulEffectTargets;

	protected AbilityTargetValidatorWrapper tvDefaultAllExceptSelf;

	private Hero _equippedOwner;

	private SkillTrigger _equippedSkill;

	private GemWorldModel _worldModel;

	[SyncVar]
	private float _currentCooldown;

	[CompilerGenerated]
	[SyncVar]
	private int? numberDisplay__BackingField;

	private float _rateLimitRemainingUse;

	[CompilerGenerated]
	[SyncVar]
	private bool isNotReadyByRateLimit__BackingField;

	internal Action<float, float> onDismantleProgressChanged;

	[CompilerGenerated]
	[SyncVar(hook = "OnDismantleProgressChanged")]
	private float dismantleProgress__BackingField;

	private float _lastDismantleTapTime;

	private Hero _lastDismantler;

	[CompilerGenerated]
	[SyncVar]
	[SaveVar(SaveVarFlags.Default)]
	private int maxSellGold__BackingField = int.MaxValue;

	[CompilerGenerated]
	[SyncVar]
	[SaveVar(SaveVarFlags.Default)]
	private int? sellGoldOverride__BackingField;

	[CompilerGenerated]
	[SyncVar]
	[SaveVar(SaveVarFlags.Default)]
	private int? dismantleDreamDustOverride__BackingField;

	protected NetworkBehaviourSyncVar ____003ChandOwner_003Ek__BackingFieldNetId;

	protected NetworkBehaviourSyncVar ____003CtempOwner_003Ek__BackingFieldNetId;

	protected NetworkBehaviourSyncVar ____003Cowner_003Ek__BackingFieldNetId;

	protected NetworkBehaviourSyncVar ____003Cskill_003Ek__BackingFieldNetId;

	public Action<Hero, Hero> _Mirror_SyncVarHookDelegate__003ChandOwner_003Ek__BackingField;

	public Action<DewPlayer, DewPlayer> _Mirror_SyncVarHookDelegate__003CtempOwner_003Ek__BackingField;

	public Action<Hero, Hero> _Mirror_SyncVarHookDelegate__003Cowner_003Ek__BackingField;

	public Action<SkillTrigger, SkillTrigger> _Mirror_SyncVarHookDelegate__003Cskill_003Ek__BackingField;

	public Action<int, int> _Mirror_SyncVarHookDelegate__quality;

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

	public Hero owner
	{
		[CompilerGenerated]
		get
		{
			return Network_003Cowner_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003Cowner_003Ek__BackingField = value;
		}
	}

	public GemLocation location
	{
		[CompilerGenerated]
		get
		{
			return location__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003Clocation_003Ek__BackingField = value;
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

	Entity IItem.owner
	{
		get
		{
			return Network_003Cowner_003Ek__BackingField;
		}
		set
		{
			Network_003Cowner_003Ek__BackingField = value as Hero;
		}
	}

	public SkillTrigger skill
	{
		[CompilerGenerated]
		get
		{
			return Network_003Cskill_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003Cskill_003Ek__BackingField = value;
		}
	}

	[SaveVar(SaveVarFlags.Default)]
	public int quality
	{
		get
		{
			return ScalingValue.levelOverride ?? _quality;
		}
		set
		{
			Network_quality = value;
		}
	}

	public QualityType qualityType => GetQualityType(quality);

	public int effectiveLevel => Mathf.Clamp(quality, 1, int.MaxValue) + 1;

	bool IExcludeFromPool.excludeFromPool => excludeFromPool;

	Rarity IItem.rarity => rarity;

	public bool isValid
	{
		get
		{
			if ((UnityEngine.Object)(object)this != null && isActive)
			{
				return (UnityEngine.Object)(object)Network_003Cowner_003Ek__BackingField != null;
			}
			return false;
		}
	}

	public float maxCooldown
	{
		get
		{
			if (!isCooldownEnabled)
			{
				return 0f;
			}
			return GetValue(cooldownTime);
		}
	}

	public float currentCooldown => _currentCooldown;

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

	public bool isNotReadyByRateLimit
	{
		[CompilerGenerated]
		get
		{
			return isNotReadyByRateLimit__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			Network_003CisNotReadyByRateLimit_003Ek__BackingField = value;
		}
	}

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

	public override bool isDestroyedOnRoomChange
	{
		get
		{
			if ((UnityEngine.Object)(object)Network_003Cowner_003Ek__BackingField == null)
			{
				return (UnityEngine.Object)(object)Network_003ChandOwner_003Ek__BackingField == null;
			}
			return false;
		}
	}

	public virtual bool isDroppedOnOwnerDisconnect => false;

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

	public int? sellGoldOverride
	{
		[CompilerGenerated]
		get
		{
			return sellGoldOverride__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CsellGoldOverride_003Ek__BackingField = value;
		}
	}

	public int? dismantleDreamDustOverride
	{
		[CompilerGenerated]
		get
		{
			return dismantleDreamDustOverride__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CdismantleDreamDustOverride_003Ek__BackingField = value;
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
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref skipStartAnimation__BackingField, 8uL, (Action<bool, bool>)null);
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
			((NetworkBehaviour)this).GeneratedSyncVarSetter_NetworkBehaviour<Hero>(value, ref handOwner__BackingField, 16uL, _Mirror_SyncVarHookDelegate__003ChandOwner_003Ek__BackingField, ref ____003ChandOwner_003Ek__BackingFieldNetId);
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
			((NetworkBehaviour)this).GeneratedSyncVarSetter_NetworkBehaviour<DewPlayer>(value, ref tempOwner__BackingField, 32uL, _Mirror_SyncVarHookDelegate__003CtempOwner_003Ek__BackingField, ref ____003CtempOwner_003Ek__BackingFieldNetId);
		}
	}

	public Hero Network_003Cowner_003Ek__BackingField
	{
		get
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			return ((NetworkBehaviour)this).GetSyncVarNetworkBehaviour<Hero>(____003Cowner_003Ek__BackingFieldNetId, ref owner__BackingField);
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter_NetworkBehaviour<Hero>(value, ref owner__BackingField, 64uL, _Mirror_SyncVarHookDelegate__003Cowner_003Ek__BackingField, ref ____003Cowner_003Ek__BackingFieldNetId);
		}
	}

	public GemLocation Network_003Clocation_003Ek__BackingField
	{
		get
		{
			return location__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<GemLocation>(value, ref location__BackingField, 128uL, (Action<GemLocation, GemLocation>)null);
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
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref fillAmount__BackingField, 256uL, (Action<float, float>)null);
		}
	}

	public SkillTrigger Network_003Cskill_003Ek__BackingField
	{
		get
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			return ((NetworkBehaviour)this).GetSyncVarNetworkBehaviour<SkillTrigger>(____003Cskill_003Ek__BackingFieldNetId, ref skill__BackingField);
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter_NetworkBehaviour<SkillTrigger>(value, ref skill__BackingField, 512uL, _Mirror_SyncVarHookDelegate__003Cskill_003Ek__BackingField, ref ____003Cskill_003Ek__BackingFieldNetId);
		}
	}

	public int Network_quality
	{
		get
		{
			return _quality;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<int>(value, ref _quality, 1024uL, _Mirror_SyncVarHookDelegate__quality);
		}
	}

	public float Network_currentCooldown
	{
		get
		{
			return _currentCooldown;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref _currentCooldown, 2048uL, (Action<float, float>)null);
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
			((NetworkBehaviour)this).GeneratedSyncVarSetter<int?>(value, ref numberDisplay__BackingField, 4096uL, (Action<int?, int?>)null);
		}
	}

	public bool Network_003CisNotReadyByRateLimit_003Ek__BackingField
	{
		get
		{
			return isNotReadyByRateLimit__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref isNotReadyByRateLimit__BackingField, 8192uL, (Action<bool, bool>)null);
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
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref dismantleProgress__BackingField, 16384uL, _Mirror_SyncVarHookDelegate__003CdismantleProgress_003Ek__BackingField);
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
			((NetworkBehaviour)this).GeneratedSyncVarSetter<int>(value, ref maxSellGold__BackingField, 32768uL, (Action<int, int>)null);
		}
	}

	public int? Network_003CsellGoldOverride_003Ek__BackingField
	{
		get
		{
			return sellGoldOverride__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<int?>(value, ref sellGoldOverride__BackingField, 65536uL, (Action<int?, int?>)null);
		}
	}

	public int? Network_003CdismantleDreamDustOverride_003Ek__BackingField
	{
		get
		{
			return dismantleDreamDustOverride__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<int?>(value, ref dismantleDreamDustOverride__BackingField, 131072uL, (Action<int?, int?>)null);
		}
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

	protected override void OnPrepare()
	{
		base.OnPrepare();
		if (!isNewInstance)
		{
			Network_003CskipStartAnimation_003Ek__BackingField = true;
		}
	}

	private void OnHandOwnerChanged(Hero oldOwner, Hero newOwner)
	{
		ClientEvent_OnHandOwnerChanged?.Invoke(oldOwner, newOwner);
	}

	private void OnOwnerChanged(Hero oldOwner, Hero newOwner)
	{
		SetEquippedOwner(newOwner);
		ClientEvent_OnOwnerChanged?.Invoke(oldOwner, newOwner);
	}

	private void OnSkillChanged(SkillTrigger oldSkill, SkillTrigger newSkill)
	{
		SetEquippedSkill(newSkill);
	}

	private void SetEquippedOwner(Hero newOwner)
	{
		if (_equippedOwner != newOwner)
		{
			Hero equippedOwner = _equippedOwner;
			_equippedOwner = newOwner;
			if (equippedOwner != null)
			{
				OnUnequipGem(equippedOwner);
			}
			if ((UnityEngine.Object)(object)newOwner != null)
			{
				OnEquipGem(newOwner);
			}
		}
	}

	private void SetEquippedSkill(SkillTrigger newSkill)
	{
		if (_equippedSkill != newSkill)
		{
			SkillTrigger equippedSkill = _equippedSkill;
			_equippedSkill = newSkill;
			if (equippedSkill != null)
			{
				OnUnequipSkill(equippedSkill);
			}
			if ((UnityEngine.Object)(object)newSkill != null)
			{
				OnEquipSkill(newSkill);
			}
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		try
		{
			SetEquippedSkill(null);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception, (UnityEngine.Object)(object)this);
		}
		try
		{
			SetEquippedOwner(null);
		}
		catch (Exception exception2)
		{
			Debug.LogException(exception2, (UnityEngine.Object)(object)this);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		tvDefaultHarmfulEffectTargets = new AbilityTargetValidatorWrapper(null, EntityRelation.Neutral | EntityRelation.Enemy);
		tvDefaultUsefulEffectTargets = new AbilityTargetValidatorWrapper(null, EntityRelation.Self | EntityRelation.Ally);
		tvDefaultAllExceptSelf = new AbilityTargetValidatorWrapper(Network_003Cowner_003Ek__BackingField, EntityRelation.Neutral | EntityRelation.Enemy | EntityRelation.Ally);
	}

	public override void OnStartServer()
	{
		base.OnStartServer();
		if (quality < 0)
		{
			quality = NetworkedManagerBase<LootManager>.instance.SelectGemQuality(rarity);
		}
	}

	public override void OnStartClient()
	{
		base.OnStartClient();
		GameObject original = Resources.Load<GameObject>("WorldModels/Gem World Model");
		_worldModel = UnityEngine.Object.Instantiate(original, position, rotation, ((Component)(object)this).transform).GetComponent<GemWorldModel>();
	}

	public override void LogicUpdate(float dt)
	{
		base.LogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer && isCooldownEnabled && (UnityEngine.Object)(object)Network_003Cowner_003Ek__BackingField != null && _currentCooldown > 0f)
		{
			Network_currentCooldown = Mathf.MoveTowards(_currentCooldown, 0f, dt);
			if (_currentCooldown == 0f)
			{
				InvokeOnCooldownReady();
			}
		}
		if (((NetworkBehaviour)this).isServer)
		{
			DoDismantleLogicUpdate();
		}
		if (((NetworkBehaviour)this).isServer && isRateLimited)
		{
			int num = Mathf.RoundToInt(GetValue(rateLimitCount));
			float value = GetValue(rateLimitTime);
			_rateLimitRemainingUse = Mathf.MoveTowards(_rateLimitRemainingUse, num, dt * (float)num / value);
			Network_003CisNotReadyByRateLimit_003Ek__BackingField = _rateLimitRemainingUse < 1f;
			if (setNumberDisplay)
			{
				Network_003CnumberDisplay_003Ek__BackingField = Mathf.FloorToInt(_rateLimitRemainingUse);
			}
		}
	}

	public virtual void OnEquipGem(Hero newOwner)
	{
		if (((NetworkBehaviour)this).isServer)
		{
			Network_003CtempOwner_003Ek__BackingField = null;
			Network_003CskipStartAnimation_003Ek__BackingField = true;
		}
		((Component)(object)this).transform.localPosition = Vector3.zero;
		if (((NetworkBehaviour)this).isServer && enableStatBonus)
		{
			newOwner.Status.AddStatBonus(statBonus);
		}
		tvDefaultUsefulEffectTargets.self = newOwner;
		tvDefaultHarmfulEffectTargets.self = newOwner;
	}

	public virtual void OnUnequipGem(Hero oldOwner)
	{
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)oldOwner != null && enableStatBonus)
		{
			oldOwner.Status.RemoveStatBonus(statBonus);
		}
		tvDefaultUsefulEffectTargets.self = null;
		tvDefaultHarmfulEffectTargets.self = null;
	}

	public virtual void OnEquipSkill(SkillTrigger newSkill)
	{
		if (((NetworkBehaviour)this).isServer)
		{
			newSkill.TriggerEvent_OnCastComplete += new Action<EventInfoCast>(OnCastComplete);
			newSkill.TriggerEvent_OnCastCompleteBeforePrepare += new Action<EventInfoCast>(OnCastCompleteBeforePrepare);
			newSkill.ActorEvent_OnDealDamage += new Action<EventInfoDamage>(OnDealDamage);
			newSkill.ActorEvent_OnDoHeal += new Action<EventInfoHeal>(OnDoHeal);
		}
	}

	public virtual void OnUnequipSkill(SkillTrigger oldSkill)
	{
		if (((NetworkBehaviour)this).isServer && !((UnityEngine.Object)(object)oldSkill == null))
		{
			oldSkill.TriggerEvent_OnCastComplete -= new Action<EventInfoCast>(OnCastComplete);
			oldSkill.TriggerEvent_OnCastCompleteBeforePrepare -= new Action<EventInfoCast>(OnCastCompleteBeforePrepare);
			oldSkill.ActorEvent_OnDealDamage -= new Action<EventInfoDamage>(OnDealDamage);
			oldSkill.ActorEvent_OnDoHeal -= new Action<EventInfoHeal>(OnDoHeal);
		}
	}

	protected virtual void OnQualityChange(int oldQuality, int newQuality)
	{
		if (((NetworkBehaviour)this).isServer)
		{
			Network_003CmaxSellGold_003Ek__BackingField = int.MaxValue;
		}
		if ((UnityEngine.Object)(object)Network_003Cowner_003Ek__BackingField != null)
		{
			Network_003Cowner_003Ek__BackingField.Skill.ClientHeroEvent_OnGemQualityChanged?.Invoke(this, oldQuality, newQuality);
		}
		ClientGemEvent_OnQualityChanged?.Invoke(oldQuality, newQuality);
	}

	[ClientRpc]
	internal void RpcSetPositionAndRotation(Vector3 pos, Quaternion rot)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteVector3((NetworkWriter)(object)val, pos);
		NetworkWriterExtensions.WriteQuaternion((NetworkWriter)(object)val, rot);
		((NetworkBehaviour)this).SendRPCInternal("System.Void Gem::RpcSetPositionAndRotation(UnityEngine.Vector3,UnityEngine.Quaternion)", 1135075037, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	public float GetValue(ScalingValue val)
	{
		if ((UnityEngine.Object)(object)((NetworkBehaviour)this).netIdentity == null)
		{
			return val.GetValue(effectiveLevel, null);
		}
		return val.GetValue(effectiveLevel, Network_003Cowner_003Ek__BackingField);
	}

	protected virtual void OnDealDamage(EventInfoDamage info)
	{
	}

	protected virtual void OnDoHeal(EventInfoHeal obj)
	{
	}

	protected virtual void OnCastComplete(EventInfoCast info)
	{
	}

	protected virtual void OnCastCompleteBeforePrepare(EventInfoCast info)
	{
	}

	public bool CanInteract(Entity entity)
	{
		if ((UnityEngine.Object)(object)Network_003Cowner_003Ek__BackingField == null && (UnityEngine.Object)(object)Network_003ChandOwner_003Ek__BackingField == null && !_worldModel.isAnimating && Time.time - creationTime > 0.5f && !IsLockedFor(entity.owner))
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
		}
		else if (((NetworkBehaviour)this).isServer)
		{
			if (hero.Skill.TryGetEquippedGemOfSameType(((object)this).GetType(), out var _, out var gem))
			{
				hero.Skill.MergeGem(this, gem);
			}
			else
			{
				hero.Skill.HoldInHand(this);
			}
		}
	}

	public static int GetMergedQuality(int a, int b)
	{
		return Mathf.Clamp(Mathf.RoundToInt(Mathf.Pow(Mathf.Pow(a, 1f) + Mathf.Pow(b, 1f), 1f)), Mathf.Max(a, b) + 1, int.MaxValue);
	}

	[Server]
	public void StartCooldown()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Gem::StartCooldown()' called when server was not active");
			return;
		}
		if (!isCooldownEnabled)
		{
			throw new InvalidOperationException("Cooldown is disabled");
		}
		Network_currentCooldown = maxCooldown;
		InvokeOnCooldownStarted();
	}

	[Server]
	public void ApplyCooldownReduction(float amount)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Gem::ApplyCooldownReduction(System.Single)' called when server was not active");
			return;
		}
		InvokeOnCooldownReduced(amount);
		if (isCooldownEnabled)
		{
			Network_currentCooldown = Mathf.MoveTowards(_currentCooldown, 0f, amount);
			if (_currentCooldown == 0f)
			{
				InvokeOnCooldownReady();
			}
		}
		if (isRateLimited)
		{
			int num = Mathf.RoundToInt(GetValue(rateLimitCount));
			float value = GetValue(rateLimitTime);
			_rateLimitRemainingUse = Mathf.MoveTowards(_rateLimitRemainingUse, num, amount * (float)num / value);
			Network_003CisNotReadyByRateLimit_003Ek__BackingField = _rateLimitRemainingUse < 1f;
			if (setNumberDisplay)
			{
				Network_003CnumberDisplay_003Ek__BackingField = Mathf.FloorToInt(_rateLimitRemainingUse);
			}
		}
	}

	[Server]
	public void ApplyCooldownReductionByRatio(float ratio)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Gem::ApplyCooldownReductionByRatio(System.Single)' called when server was not active");
			return;
		}
		InvokeOnCooldownReducedByRatio(ratio);
		if (isCooldownEnabled)
		{
			Network_currentCooldown = Mathf.MoveTowards(_currentCooldown, 0f, GetValue(cooldownTime) * ratio);
			if (_currentCooldown == 0f)
			{
				InvokeOnCooldownReady();
			}
		}
		if (isRateLimited)
		{
			int num = Mathf.RoundToInt(GetValue(rateLimitCount));
			_rateLimitRemainingUse = Mathf.MoveTowards(_rateLimitRemainingUse, num, ratio * (float)num);
			Network_003CisNotReadyByRateLimit_003Ek__BackingField = _rateLimitRemainingUse < 1f;
			if (setNumberDisplay)
			{
				Network_003CnumberDisplay_003Ek__BackingField = Mathf.FloorToInt(_rateLimitRemainingUse);
			}
		}
	}

	[Server]
	public void ResetCooldown()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Gem::ResetCooldown()' called when server was not active");
			return;
		}
		if (!isCooldownEnabled && !isRateLimited)
		{
			throw new InvalidOperationException("Cooldown is disabled");
		}
		if (isCooldownEnabled && _currentCooldown > 0f)
		{
			Network_currentCooldown = 0f;
			InvokeOnCooldownReady();
		}
		if (!isRateLimited)
		{
			return;
		}
		int num = Mathf.RoundToInt(GetValue(rateLimitCount));
		if (_rateLimitRemainingUse < (float)num)
		{
			_rateLimitRemainingUse = num;
			Network_003CisNotReadyByRateLimit_003Ek__BackingField = _rateLimitRemainingUse < 1f;
			if (setNumberDisplay)
			{
				Network_003CnumberDisplay_003Ek__BackingField = Mathf.FloorToInt(_rateLimitRemainingUse);
			}
			InvokeOnCooldownReady();
		}
	}

	[Server]
	public void NotifyUse()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Gem::NotifyUse()' called when server was not active");
			return;
		}
		InvokeFlash();
		if (isRateLimited)
		{
			_rateLimitRemainingUse--;
			Network_003CisNotReadyByRateLimit_003Ek__BackingField = _rateLimitRemainingUse < 1f;
		}
	}

	[ClientRpc]
	private void InvokeOnCooldownReady()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendRPCInternal("System.Void Gem::InvokeOnCooldownReady()", -1566986505, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[ClientRpc]
	private void InvokeOnCooldownStarted()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendRPCInternal("System.Void Gem::InvokeOnCooldownStarted()", 622452117, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[ClientRpc]
	private void InvokeOnCooldownReducedByRatio(float ratio)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteFloat((NetworkWriter)(object)val, ratio);
		((NetworkBehaviour)this).SendRPCInternal("System.Void Gem::InvokeOnCooldownReducedByRatio(System.Single)", 1296628743, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[ClientRpc]
	private void InvokeOnCooldownReduced(float amount)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteFloat((NetworkWriter)(object)val, amount);
		((NetworkBehaviour)this).SendRPCInternal("System.Void Gem::InvokeOnCooldownReduced(System.Single)", 717694297, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[ClientRpc]
	private void InvokeFlash()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendRPCInternal("System.Void Gem::InvokeFlash()", -1601952754, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	public virtual bool IsReady()
	{
		if (!isCooldownEnabled || _currentCooldown <= 0f)
		{
			if (isRateLimited)
			{
				return !isNotReadyByRateLimit;
			}
			return true;
		}
		return false;
	}

	private void OnDismantleProgressChanged(float oldVal, float newVal)
	{
		onDismantleProgressChanged?.Invoke(oldVal, newVal);
		if (((NetworkBehaviour)this).isServer && newVal >= 1f && (UnityEngine.Object)(object)Network_003Cowner_003Ek__BackingField == null && (UnityEngine.Object)(object)Network_003ChandOwner_003Ek__BackingField == null && isActive)
		{
			DismantleGem();
		}
	}

	[Server]
	public void DismantleGem()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Gem::DismantleGem()' called when server was not active");
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
		if (dismantleDreamDustOverride.HasValue)
		{
			return dismantleDreamDustOverride.Value;
		}
		if ((UnityEngine.Object)(object)player == null)
		{
			player = _lastDismantler.owner;
		}
		float num = NetworkedManagerBase<GameManager>.instance.ges.gemDismantleDreamDustByQuality.Evaluate(quality);
		num *= NetworkedManagerBase<GameManager>.instance.ges.gemDismantleDreamDustMultiplier.Get(rarity);
		if ((UnityEngine.Object)(object)player != null)
		{
			num *= player.dismantleDreamDustMultiplier * player.dismantleGemDreamDustMultiplier;
		}
		num *= DewBuildProfile.current.dismantleDreamDustMultiplier;
		return Mathf.Max(1, Mathf.RoundToInt(num));
	}

	[Command(requiresAuthority = false)]
	private void CmdDoDismantleTap(NetworkConnectionToClient sender = null)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendCommandInternal("System.Void Gem::CmdDoDismantleTap(Mirror.NetworkConnectionToClient)", 1150960282, (NetworkWriter)(object)val, 0, false);
		NetworkWriterPool.Return(val);
	}

	private void DoDismantleLogicUpdate()
	{
		if ((dismantleProgress > 0f && Time.time - _lastDismantleTapTime > 1f) || (UnityEngine.Object)(object)Network_003Cowner_003Ek__BackingField != null || (UnityEngine.Object)(object)Network_003ChandOwner_003Ek__BackingField != null)
		{
			Network_003CdismantleProgress_003Ek__BackingField = 0f;
		}
	}

	public static QualityType GetQualityType(float quality)
	{
		if (quality < 50f)
		{
			return QualityType.Chipped;
		}
		if (quality < 100f)
		{
			return QualityType.Flawed;
		}
		if (quality < 200f)
		{
			return QualityType.Regular;
		}
		if (quality < 400f)
		{
			return QualityType.Flawless;
		}
		if (quality < 600f)
		{
			return QualityType.Perfect;
		}
		if (quality < 1000f)
		{
			return QualityType.Otherworldly;
		}
		return QualityType.Transcendent;
	}

	public int GetBuyGold()
	{
		return GetBuyGold(rarity, quality);
	}

	public int GetSellGold()
	{
		return sellGoldOverride ?? Mathf.Min(GetSellGold(rarity, quality), maxSellGold);
	}

	public static int GetBuyGold(Rarity rarity, int quality)
	{
		float amount = GetGoldValue(rarity, quality);
		amount = NetworkedManagerBase<GameManager>.instance.GetAdjustedGoldAmount_Cost(amount);
		return Mathf.Max(Mathf.RoundToInt(amount), 1);
	}

	public static int GetSellGold(Rarity rarity, int quality)
	{
		float num = GetGoldValue(rarity, quality);
		num *= NetworkedManagerBase<GameManager>.instance.ges.sellValueMultiplier;
		num = NetworkedManagerBase<GameManager>.instance.GetAdjustedGoldAmount_Income(num);
		return Mathf.Max(Mathf.RoundToInt(num), 1);
	}

	public static int GetGoldValue(Rarity rarity, int quality)
	{
		DewGameplayExperienceSettings ges = NetworkedManagerBase<GameManager>.instance.ges;
		float num = ges.gemValue.Get(rarity);
		num *= ges.valueGlobalMultiplier;
		num *= 1f + (ges.valueMultiplierPerGemQuality - 1f) * (float)quality;
		num = NetworkedManagerBase<GameManager>.instance.GetAdjustedGoldAmount(num);
		return Mathf.Max(Mathf.RoundToInt(num), 1);
	}

	public override bool ShouldBeSavedWithRoom()
	{
		return (UnityEngine.Object)(object)Network_003Cowner_003Ek__BackingField == null;
	}

	[Server]
	public T CreateAbilityInstanceWithSource<T>(Actor source, Vector3 position, Quaternion? rotation, CastInfo info, Action<T> beforePrepare = null) where T : AbilityInstance
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'T Gem::CreateAbilityInstanceWithSource(Actor,UnityEngine.Vector3,System.Nullable`1<UnityEngine.Quaternion>,CastInfo,System.Action`1<T>)' called when server was not active");
			return null;
		}
		return source.CreateAbilityInstance(position, rotation, info, (T t) =>
		{
			t.skillLevel = effectiveLevel;
			t.gem = this;
			beforePrepare?.Invoke(t);
		});
	}

	[Server]
	public T CreateStatusEffectWithSource<T>(Actor source, Entity victim, CastInfo info, Action<T> beforePrepare = null) where T : StatusEffect
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'T Gem::CreateStatusEffectWithSource(Actor,Entity,CastInfo,System.Action`1<T>)' called when server was not active");
			return null;
		}
		return source.CreateStatusEffect(victim, info, (T t) =>
		{
			t.skillLevel = effectiveLevel;
			t.gem = this;
			beforePrepare?.Invoke(t);
		});
	}

	public Se_GenericEffectContainer CreateBasicEffectWithSource(Actor source, Entity victim, BasicEffect eff, float duration, string id, DuplicateEffectBehavior onDuplicate = DuplicateEffectBehavior.ReplacePrevious)
	{
		return source.CreateBasicEffect(victim, eff, duration, id, onDuplicate);
	}

	public Gem()
	{
		_Mirror_SyncVarHookDelegate__003ChandOwner_003Ek__BackingField = OnHandOwnerChanged;
		_Mirror_SyncVarHookDelegate__003CtempOwner_003Ek__BackingField = OnTempOwnerChanged;
		_Mirror_SyncVarHookDelegate__003Cowner_003Ek__BackingField = OnOwnerChanged;
		_Mirror_SyncVarHookDelegate__003Cskill_003Ek__BackingField = OnSkillChanged;
		_Mirror_SyncVarHookDelegate__quality = OnQualityChange;
		_Mirror_SyncVarHookDelegate__003CdismantleProgress_003Ek__BackingField = OnDismantleProgressChanged;
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcSetPositionAndRotation__Vector3__Quaternion(Vector3 pos, Quaternion rot)
	{
		((Component)(object)this).transform.SetPositionAndRotation(pos, rot);
		Physics.SyncTransforms();
	}

	protected static void InvokeUserCode_RpcSetPositionAndRotation__Vector3__Quaternion(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetPositionAndRotation called on server.");
		}
		else
		{
			((Gem)(object)obj).UserCode_RpcSetPositionAndRotation__Vector3__Quaternion(NetworkReaderExtensions.ReadVector3(reader), NetworkReaderExtensions.ReadQuaternion(reader));
		}
	}

	protected void UserCode_InvokeOnCooldownReady()
	{
		ClientGemEvent_OnCooldownReady?.Invoke();
	}

	protected static void InvokeUserCode_InvokeOnCooldownReady(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC InvokeOnCooldownReady called on server.");
		}
		else
		{
			((Gem)(object)obj).UserCode_InvokeOnCooldownReady();
		}
	}

	protected void UserCode_InvokeOnCooldownStarted()
	{
		ClientGemEvent_OnCooldownStarted?.Invoke();
	}

	protected static void InvokeUserCode_InvokeOnCooldownStarted(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC InvokeOnCooldownStarted called on server.");
		}
		else
		{
			((Gem)(object)obj).UserCode_InvokeOnCooldownStarted();
		}
	}

	protected void UserCode_InvokeOnCooldownReducedByRatio__Single(float ratio)
	{
		ClientGemEvent_OnCooldownReducedByRatio?.Invoke(ratio);
	}

	protected static void InvokeUserCode_InvokeOnCooldownReducedByRatio__Single(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC InvokeOnCooldownReducedByRatio called on server.");
		}
		else
		{
			((Gem)(object)obj).UserCode_InvokeOnCooldownReducedByRatio__Single(NetworkReaderExtensions.ReadFloat(reader));
		}
	}

	protected void UserCode_InvokeOnCooldownReduced__Single(float amount)
	{
		ClientGemEvent_OnCooldownReduced?.Invoke(amount);
	}

	protected static void InvokeUserCode_InvokeOnCooldownReduced__Single(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC InvokeOnCooldownReduced called on server.");
		}
		else
		{
			((Gem)(object)obj).UserCode_InvokeOnCooldownReduced__Single(NetworkReaderExtensions.ReadFloat(reader));
		}
	}

	protected void UserCode_InvokeFlash()
	{
		ClientGemEvent_OnFlash?.Invoke();
	}

	protected static void InvokeUserCode_InvokeFlash(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC InvokeFlash called on server.");
		}
		else
		{
			((Gem)(object)obj).UserCode_InvokeFlash();
		}
	}

	protected void UserCode_CmdDoDismantleTap__NetworkConnectionToClient(NetworkConnectionToClient sender)
	{
		if (!((UnityEngine.Object)(object)Network_003Cowner_003Ek__BackingField != null) && !((UnityEngine.Object)(object)Network_003ChandOwner_003Ek__BackingField != null) && !(Time.time - _lastDismantleTapTime < 0.075f) && !((UnityEngine.Object)(object)sender.GetHero() == null))
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
			((Gem)(object)obj).UserCode_CmdDoDismantleTap__NetworkConnectionToClient(senderConnection);
		}
	}

	static Gem()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected Obj, but got Unknown
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Expected Obj, but got Unknown
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Expected Obj, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected Obj, but got Unknown
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected Obj, but got Unknown
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Expected Obj, but got Unknown
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterCommand(typeof(Gem), "System.Void Gem::CmdDoDismantleTap(Mirror.NetworkConnectionToClient)", (RemoteCallDelegate)InvokeUserCode_CmdDoDismantleTap__NetworkConnectionToClient, false);
		RemoteProcedureCalls.RegisterRpc(typeof(Gem), "System.Void Gem::RpcSetPositionAndRotation(UnityEngine.Vector3,UnityEngine.Quaternion)", (RemoteCallDelegate)InvokeUserCode_RpcSetPositionAndRotation__Vector3__Quaternion);
		RemoteProcedureCalls.RegisterRpc(typeof(Gem), "System.Void Gem::InvokeOnCooldownReady()", (RemoteCallDelegate)InvokeUserCode_InvokeOnCooldownReady);
		RemoteProcedureCalls.RegisterRpc(typeof(Gem), "System.Void Gem::InvokeOnCooldownStarted()", (RemoteCallDelegate)InvokeUserCode_InvokeOnCooldownStarted);
		RemoteProcedureCalls.RegisterRpc(typeof(Gem), "System.Void Gem::InvokeOnCooldownReducedByRatio(System.Single)", (RemoteCallDelegate)InvokeUserCode_InvokeOnCooldownReducedByRatio__Single);
		RemoteProcedureCalls.RegisterRpc(typeof(Gem), "System.Void Gem::InvokeOnCooldownReduced(System.Single)", (RemoteCallDelegate)InvokeUserCode_InvokeOnCooldownReduced__Single);
		RemoteProcedureCalls.RegisterRpc(typeof(Gem), "System.Void Gem::InvokeFlash()", (RemoteCallDelegate)InvokeUserCode_InvokeFlash);
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteBool(writer, skipStartAnimation__BackingField);
			NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)Network_003ChandOwner_003Ek__BackingField);
			NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)Network_003CtempOwner_003Ek__BackingField);
			NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)Network_003Cowner_003Ek__BackingField);
			GeneratedNetworkCode._Write_GemLocation(writer, location__BackingField);
			NetworkWriterExtensions.WriteFloat(writer, fillAmount__BackingField);
			NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)Network_003Cskill_003Ek__BackingField);
			NetworkWriterExtensions.WriteInt(writer, _quality);
			NetworkWriterExtensions.WriteFloat(writer, _currentCooldown);
			NetworkWriterExtensions.WriteIntNullable(writer, numberDisplay__BackingField);
			NetworkWriterExtensions.WriteBool(writer, isNotReadyByRateLimit__BackingField);
			NetworkWriterExtensions.WriteFloat(writer, dismantleProgress__BackingField);
			NetworkWriterExtensions.WriteInt(writer, maxSellGold__BackingField);
			NetworkWriterExtensions.WriteIntNullable(writer, sellGoldOverride__BackingField);
			NetworkWriterExtensions.WriteIntNullable(writer, dismantleDreamDustOverride__BackingField);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 8L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, skipStartAnimation__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x10L) != 0L)
		{
			NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)Network_003ChandOwner_003Ek__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x20L) != 0L)
		{
			NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)Network_003CtempOwner_003Ek__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x40L) != 0L)
		{
			NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)Network_003Cowner_003Ek__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x80L) != 0L)
		{
			GeneratedNetworkCode._Write_GemLocation(writer, location__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x100L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, fillAmount__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x200L) != 0L)
		{
			NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)Network_003Cskill_003Ek__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x400L) != 0L)
		{
			NetworkWriterExtensions.WriteInt(writer, _quality);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x800L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, _currentCooldown);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x1000L) != 0L)
		{
			NetworkWriterExtensions.WriteIntNullable(writer, numberDisplay__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x2000L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, isNotReadyByRateLimit__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x4000L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, dismantleProgress__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x8000L) != 0L)
		{
			NetworkWriterExtensions.WriteInt(writer, maxSellGold__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x10000L) != 0L)
		{
			NetworkWriterExtensions.WriteIntNullable(writer, sellGoldOverride__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x20000L) != 0L)
		{
			NetworkWriterExtensions.WriteIntNullable(writer, dismantleDreamDustOverride__BackingField);
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
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize_NetworkBehaviour<Hero>(ref owner__BackingField, _Mirror_SyncVarHookDelegate__003Cowner_003Ek__BackingField, reader, ref ____003Cowner_003Ek__BackingFieldNetId);
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<GemLocation>(ref location__BackingField, (Action<GemLocation, GemLocation>)null, GeneratedNetworkCode._Read_GemLocation(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref fillAmount__BackingField, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize_NetworkBehaviour<SkillTrigger>(ref skill__BackingField, _Mirror_SyncVarHookDelegate__003Cskill_003Ek__BackingField, reader, ref ____003Cskill_003Ek__BackingFieldNetId);
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref _quality, _Mirror_SyncVarHookDelegate__quality, NetworkReaderExtensions.ReadInt(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _currentCooldown, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int?>(ref numberDisplay__BackingField, (Action<int?, int?>)null, NetworkReaderExtensions.ReadIntNullable(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref isNotReadyByRateLimit__BackingField, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref dismantleProgress__BackingField, _Mirror_SyncVarHookDelegate__003CdismantleProgress_003Ek__BackingField, NetworkReaderExtensions.ReadFloat(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref maxSellGold__BackingField, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int?>(ref sellGoldOverride__BackingField, (Action<int?, int?>)null, NetworkReaderExtensions.ReadIntNullable(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int?>(ref dismantleDreamDustOverride__BackingField, (Action<int?, int?>)null, NetworkReaderExtensions.ReadIntNullable(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 8L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref skipStartAnimation__BackingField, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
		}
		if ((num & 0x10L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize_NetworkBehaviour<Hero>(ref handOwner__BackingField, _Mirror_SyncVarHookDelegate__003ChandOwner_003Ek__BackingField, reader, ref ____003ChandOwner_003Ek__BackingFieldNetId);
		}
		if ((num & 0x20L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize_NetworkBehaviour<DewPlayer>(ref tempOwner__BackingField, _Mirror_SyncVarHookDelegate__003CtempOwner_003Ek__BackingField, reader, ref ____003CtempOwner_003Ek__BackingFieldNetId);
		}
		if ((num & 0x40L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize_NetworkBehaviour<Hero>(ref owner__BackingField, _Mirror_SyncVarHookDelegate__003Cowner_003Ek__BackingField, reader, ref ____003Cowner_003Ek__BackingFieldNetId);
		}
		if ((num & 0x80L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<GemLocation>(ref location__BackingField, (Action<GemLocation, GemLocation>)null, GeneratedNetworkCode._Read_GemLocation(reader));
		}
		if ((num & 0x100L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref fillAmount__BackingField, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
		if ((num & 0x200L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize_NetworkBehaviour<SkillTrigger>(ref skill__BackingField, _Mirror_SyncVarHookDelegate__003Cskill_003Ek__BackingField, reader, ref ____003Cskill_003Ek__BackingFieldNetId);
		}
		if ((num & 0x400L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref _quality, _Mirror_SyncVarHookDelegate__quality, NetworkReaderExtensions.ReadInt(reader));
		}
		if ((num & 0x800L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _currentCooldown, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
		if ((num & 0x1000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int?>(ref numberDisplay__BackingField, (Action<int?, int?>)null, NetworkReaderExtensions.ReadIntNullable(reader));
		}
		if ((num & 0x2000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref isNotReadyByRateLimit__BackingField, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
		}
		if ((num & 0x4000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref dismantleProgress__BackingField, _Mirror_SyncVarHookDelegate__003CdismantleProgress_003Ek__BackingField, NetworkReaderExtensions.ReadFloat(reader));
		}
		if ((num & 0x8000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref maxSellGold__BackingField, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
		}
		if ((num & 0x10000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int?>(ref sellGoldOverride__BackingField, (Action<int?, int?>)null, NetworkReaderExtensions.ReadIntNullable(reader));
		}
		if ((num & 0x20000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int?>(ref dismantleDreamDustOverride__BackingField, (Action<int?, int?>)null, NetworkReaderExtensions.ReadIntNullable(reader));
		}
	}
}
