using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

[RequireComponent(typeof(HeroSkill))]
public class Hero : Entity, IExcludeFromPool
{
	public enum HeroMainStatType
	{
		Strength,
		Intelligence,
		Agility
	}

	public enum HeroClassType
	{
		RangedAttacker,
		MeleeAttacker,
		RangedMage,
		MeleeMage,
		RangedTank,
		MeleeTank,
		RangedSupport,
		MeleeSupport,
		RangedSummoner,
		MeleeSummoner
	}

	public enum HeroDifficulty
	{
		VeryEasy,
		Easy,
		Medium,
		Hard,
		VeryHard
	}

	public const float MarkAsInCombatDuration = 2f;

	public const float InCombatCheckInterval = 0.5f;

	public SafeAction<EventInfoKill> ClientHeroEvent_OnKillOrAssist;

	public SafeAction<EventInfoSkillUse> ClientHeroEvent_OnSkillUse;

	public SafeAction<EventInfoKill> ClientHeroEvent_OnKnockedOut;

	public SafeAction<Hero> ClientHeroEvent_OnRevive;

	public SafeAction<EventInfoSkillAbilityInstance> HeroEvent_OnAbilityInstanceCreatedFromSkill;

	public SafeAction<EventInfoSkillAbilityInstance> HeroEvent_OnAbilityInstanceBeforePrepareFromSkill;

	public SafeAction<EventInfoHeroLevelUp> ClientHeroEvent_OnLevelChanged;

	public SafeAction<EventInfoDismantle> HeroEvent_OnDismantleItem;

	public DataProcessorGroup<int, Hero, Actor> dismantleProcessor = new DataProcessorGroup<int, Hero, Actor>();

	private HeroSkill _Skill;

	[CompilerGenerated]
	[SyncVar]
	private HeroLoadoutData loadout__BackingField;

	[SaveVar(SaveVarFlags.Default)]
	public readonly SyncList<string> accessories = new SyncList<string>();

	private KillTracker _assistTracker;

	[SyncVar]
	private int _exp;

	public int? maxLevelOverride;

	[SyncVar]
	private bool _isKnockedOut;

	[CompilerGenerated]
	[SyncVar(hook = "OnIsInCombatChanged")]
	private bool isInCombat__BackingField;

	public SafeAction<bool> ClientHeroEvent_OnIsInCombatChanged;

	private float _lastIsInCombatMarkTime;

	private float _nextCombatCheckTime;

	public Sprite icon;

	public Color mainColor;

	public HeroClassType classType;

	public HeroDifficulty difficulty;

	public GameObject decoConstellationPrefab;

	public bool disableDamageOverlay;

	public bool excludeFromPool;

	public float cDisplayBaseAngle;

	public HeroConstellationSettings cDestruction;

	public HeroConstellationSettings cLife;

	public HeroConstellationSettings cImagination;

	public HeroConstellationSettings cFlexible;

	[NonSerialized]
	public List<Summon> summons = new List<Summon>();

	[NonSerialized]
	public bool isWeaponHolstered;

	[CompilerGenerated]
	[SyncVar]
	[SaveVar(SaveVarFlags.Default)]
	private string skin__BackingField;

	private Vector3 _weaponOriginalScale;

	private Vector3 _holsteredWeaponOriginalScale;

	private Vector3 _weaponCv;

	private Vector3 _holsteredWeaponCv;

	private GameObject _levelUpEffect;

	private List<Renderer> _disabledFxRenderersOnWeapon;

	public Action<bool, bool> _Mirror_SyncVarHookDelegate__003CisInCombat_003Ek__BackingField;

	public override bool isDestroyedOnRoomChange
	{
		get
		{
			if (!((UnityEngine.Object)(object)owner == null))
			{
				return !owner.isHumanPlayer;
			}
			return true;
		}
	}

	public HeroSkill Skill => _Skill;

	public HeroLoadoutData loadout
	{
		[CompilerGenerated]
		get
		{
			return loadout__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003Cloadout_003Ek__BackingField = value;
		}
	}

	[SaveVar(SaveVarFlags.ApplyAfterCreation)]
	public int exp
	{
		get
		{
			return _exp;
		}
		internal set
		{
			if (!((NetworkBehaviour)this).isServer)
			{
				throw new InvalidOperationException("Can't set Hero.exp on clients");
			}
			if (level >= maxLevel)
			{
				Network_exp = 0;
				return;
			}
			Network_exp = value;
			while (_exp >= maxExp && level < maxLevel)
			{
				Network_exp = _exp - maxExp;
				Status.level++;
			}
		}
	}

	public int maxExp
	{
		get
		{
			if (level < NetworkedManagerBase<GameManager>.instance.ges.maxHeroLevel)
			{
				return (int)((float)(50 + 10 * level) * Mathf.Pow(1.2f, level));
			}
			return 0;
		}
	}

	public int maxLevel
	{
		get
		{
			if (!maxLevelOverride.HasValue)
			{
				return NetworkedManagerBase<GameManager>.instance.ges.maxHeroLevel;
			}
			return maxLevelOverride.Value;
		}
	}

	public bool isKnockedOut
	{
		get
		{
			return _isKnockedOut;
		}
		internal set
		{
			Network_isKnockedOut = value;
		}
	}

	public bool isInCombat
	{
		[CompilerGenerated]
		get
		{
			return isInCombat__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			Network_003CisInCombat_003Ek__BackingField = value;
		}
	}

	bool IExcludeFromPool.excludeFromPool => excludeFromPool;

	public string skin
	{
		[CompilerGenerated]
		get
		{
			return skin__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003Cskin_003Ek__BackingField = value;
		}
	}

	public HeroLoadoutData Network_003Cloadout_003Ek__BackingField
	{
		get
		{
			return loadout__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<HeroLoadoutData>(value, ref loadout__BackingField, 32uL, (Action<HeroLoadoutData, HeroLoadoutData>)null);
		}
	}

	public int Network_exp
	{
		get
		{
			return _exp;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<int>(value, ref _exp, 64uL, (Action<int, int>)null);
		}
	}

	public bool Network_isKnockedOut
	{
		get
		{
			return _isKnockedOut;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref _isKnockedOut, 128uL, (Action<bool, bool>)null);
		}
	}

	public bool Network_003CisInCombat_003Ek__BackingField
	{
		get
		{
			return isInCombat__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref isInCombat__BackingField, 256uL, _Mirror_SyncVarHookDelegate__003CisInCombat_003Ek__BackingField);
		}
	}

	public string Network_003Cskin_003Ek__BackingField
	{
		get
		{
			return skin__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<string>(value, ref skin__BackingField, 512uL, (Action<string, string>)null);
		}
	}

	private void OnIsInCombatChanged(bool oldVal, bool newVal)
	{
		try
		{
			ClientHeroEvent_OnIsInCombatChanged?.Invoke(newVal);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	public HeroConstellationSettings GetConstellationSettings(StarType type)
	{
		return type switch
		{
			StarType.Life => cLife, 
			StarType.Destruction => cDestruction, 
			StarType.Imagination => cImagination, 
			StarType.Flexible => cFlexible, 
			_ => throw new ArgumentOutOfRangeException("type", type, null), 
		};
	}

	protected override void Awake()
	{
		base.Awake();
		AssignComponents();
		_levelUpEffect = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Effects/HeroLevelUp"), ((Component)(object)this).transform);
		_levelUpEffect.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
	}

	public override void OnModelLoaded()
	{
		base.OnModelLoaded();
		if (Visual.model.weapon != null)
		{
			_weaponOriginalScale = Visual.model.weapon.localScale;
		}
		if (Visual.model.holsteredWeapon != null)
		{
			_holsteredWeaponOriginalScale = Visual.model.holsteredWeapon.localScale;
		}
	}

	protected override void AIUpdate(ref EntityAIContext context)
	{
		base.AIUpdate(ref context);
		if (!Control.isDisplacing && ((Behaviour)(object)Control._agent).enabled)
		{
			if (UnityEngine.Random.value < 0.15f)
			{
				Vector3 end = agentPosition + UnityEngine.Random.insideUnitCircle.ToXZ().normalized * UnityEngine.Random.Range(2f, 7f);
				end = Dew.GetValidAgentDestination_LinearSweep(agentPosition, end);
				Control.MoveToDestination(end, immediately: true);
			}
			else
			{
				Hero hero = null;
				float num = float.PositiveInfinity;
				Hero hero2 = null;
				float num2 = float.PositiveInfinity;
				foreach (Hero allHero in NetworkedManagerBase<ActorManager>.instance.allHeroes)
				{
					if ((UnityEngine.Object)(object)allHero == (UnityEngine.Object)(object)this)
					{
						continue;
					}
					float num3 = Vector3.Distance(allHero.agentPosition, agentPosition);
					if (num3 < num2)
					{
						hero2 = allHero;
						num2 = num3;
					}
					if (!allHero.AI.isAITicking && allHero.owner.isHumanPlayer)
					{
						float num4 = Vector3.Distance(allHero.agentPosition, agentPosition);
						if (!(num4 > num))
						{
							hero = allHero;
							num = num4;
						}
					}
				}
				if ((bool)(UnityEngine.Object)(object)hero)
				{
					Vector3 end2 = hero.agentPosition + hero.Control.agentVelocity * 1.5f;
					end2 = Dew.GetValidAgentDestination_LinearSweep(hero.agentPosition, end2);
					if (UnityEngine.Random.value < 0.4f && (bool)(UnityEngine.Object)(object)hero && (num > UnityEngine.Random.Range(5f, 7f) || (Control._desiredAgentDestination.HasValue && Vector3.Distance(end2, Control._desiredAgentDestination.Value) > 4f)))
					{
						end2 += UnityEngine.Random.insideUnitCircle.ToXZ().normalized * UnityEngine.Random.Range(3f, 5f);
						end2 = Dew.GetValidAgentDestination_LinearSweep(hero.agentPosition, end2);
						Control.MoveToDestination(end2, immediately: true);
						if (UnityEngine.Random.value < 0.2f && (bool)(UnityEngine.Object)(object)Skill.Movement)
						{
							Control.Cast(Skill.Movement, new CastInfo(this, end2));
						}
					}
					else if (UnityEngine.Random.value < 0.4f && (bool)(UnityEngine.Object)(object)hero2 && num2 < 3f && !Control._desiredAgentDestination.HasValue)
					{
						Vector3 destination = hero2.agentPosition + (agentPosition - hero2.agentPosition).normalized * UnityEngine.Random.Range(3f, 5f);
						Control.MoveToDestination(destination, immediately: true);
					}
				}
			}
		}
		if ((UnityEngine.Object)(object)context.targetEnemy != null && !TrySkill(Skill.Q) && !TrySkill(Skill.W) && !TrySkill(Skill.E) && !TrySkill(Skill.R))
		{
			AI.Helper_ChaseTarget();
		}
		bool TrySkill(SkillTrigger skill)
		{
			if ((UnityEngine.Object)(object)skill == null)
			{
				return false;
			}
			if (UnityEngine.Random.value < 0.5f)
			{
				return false;
			}
			if (!skill.CanBeCast())
			{
				return false;
			}
			if (!skill.currentConfig.ignoreBlock && Control.IsActionBlocked(EntityControl.BlockableAction.Ability) != EntityControl.BlockStatus.Allowed)
			{
				return false;
			}
			return AI.Helper_CastAbilityAuto(skill);
		}
	}

	public override void FrameUpdate()
	{
		base.FrameUpdate();
		if (Visual.model == null)
		{
			return;
		}
		bool flag = (Animation.abilityAnimStatus.isPlaying && Animation.abilityAnimStatus.currentClip.hideWeaponOnHeroes) || isWeaponHolstered;
		Transform weapon = Visual.model.weapon;
		if (weapon != null)
		{
			weapon.localScale = Vector3.SmoothDamp(weapon.localScale, flag ? Vector3.zero : _weaponOriginalScale, ref _weaponCv, 0.05f);
			weapon.gameObject.SetActive(weapon.localScale.x > 0.01f);
			if (flag && _disabledFxRenderersOnWeapon == null)
			{
				_disabledFxRenderersOnWeapon = new List<Renderer>();
				foreach (FxAttachToEntity attachedEffect in Visual._attachedEffects)
				{
					if (attachedEffect.position != FxAttachToEntity.PositionType.Weapon && attachedEffect.position != FxAttachToEntity.PositionType.Muzzle && attachedEffect.position != FxAttachToEntity.PositionType.RightHand)
					{
						continue;
					}
					ListReturnHandle<Renderer> handle;
					foreach (Renderer item in ((Component)attachedEffect).GetComponentsInChildrenNonAlloc(out handle))
					{
						if (!(item == null) && !item.enabled)
						{
							return;
						}
					}
					handle.Return();
				}
			}
			else if (!flag && _disabledFxRenderersOnWeapon != null)
			{
				foreach (Renderer item2 in _disabledFxRenderersOnWeapon)
				{
					if (!(item2 == null))
					{
						item2.enabled = true;
					}
				}
				_disabledFxRenderersOnWeapon = null;
			}
		}
		Transform holsteredWeapon = Visual.model.holsteredWeapon;
		if (holsteredWeapon != null)
		{
			holsteredWeapon.localScale = Vector3.SmoothDamp(holsteredWeapon.localScale, isWeaponHolstered ? _holsteredWeaponOriginalScale : Vector3.zero, ref _holsteredWeaponCv, 0.05f);
			holsteredWeapon.gameObject.SetActive(holsteredWeapon.localScale.x > 0.01f);
		}
	}

	public override void OnStart()
	{
		base.OnStart();
		if (loadout == null)
		{
			Network_003Cloadout_003Ek__BackingField = new HeroLoadoutData();
		}
	}

	public override void OnStartServer()
	{
		base.OnStartServer();
		ClientHeroEvent_OnLevelChanged += new Action<EventInfoHeroLevelUp>(OnHeroLevelUp);
		_assistTracker = TrackKills(30f, (EventInfoKill obj) =>
		{
			RpcInvokeOnKillOrAssist(obj);
		});
		EntityEvent_OnTakeDamage += (Action<EventInfoDamage>)((EventInfoDamage dmg) =>
		{
			Entity entity = dmg.actor.firstEntity;
			if (!((UnityEngine.Object)(object)entity == null) && entity.GetRelation(this) == EntityRelation.Enemy)
			{
				MarkAsInCombat();
			}
		});
		ActorEvent_OnDealDamage += (Action<EventInfoDamage>)((EventInfoDamage dmg) =>
		{
			if (GetRelation(dmg.victim) == EntityRelation.Enemy)
			{
				MarkAsInCombat();
			}
		});
		takenDamageProcessor.Add(delegate(ref DamageData data, Actor actor, Entity target)
		{
			if (!((UnityEngine.Object)(object)owner == null) && owner.isPlayingCutscene)
			{
				data.BlockWithImmunity();
			}
		});
	}

	public override void OnLateStartServer()
	{
		base.OnLateStartServer();
		ApplyHeroStatusEffects();
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer && Time.time > _nextCombatCheckTime)
		{
			_nextCombatCheckTime = Time.time + 0.5f;
			bool flag = Time.time - _lastIsInCombatMarkTime < 2f || (section != null && section.monsters.isCombatActive);
			if (flag != isInCombat)
			{
				Network_003CisInCombat_003Ek__BackingField = flag;
			}
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && _assistTracker != null)
		{
			_assistTracker.Stop();
			_assistTracker = null;
		}
	}

	[ClientRpc]
	private void RpcInvokeOnKillOrAssist(EventInfoKill obj)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_EventInfoKill((NetworkWriter)(object)val, obj);
		((NetworkBehaviour)this).SendRPCInternal("System.Void Hero::RpcInvokeOnKillOrAssist(EventInfoKill)", 162120419, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	public override void OnStartAuthority()
	{
		((NetworkBehaviour)this).OnStartAuthority();
		NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnLocalHeroAdd?.Invoke(this);
	}

	public override void OnStopAuthority()
	{
		((NetworkBehaviour)this).OnStopAuthority();
		if ((UnityEngine.Object)(object)NetworkedManagerBase<ActorManager>.instance != null)
		{
			NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnLocalHeroRemove?.Invoke(this);
		}
	}

	private void ApplyHeroStatusEffects()
	{
		CreateStatusEffect<Se_HeroDeathInterrupt>(this, new CastInfo(this));
		CreateStatusEffect<Se_HeroOneShotProtection>(this, new CastInfo(this));
		CreateStatusEffect<Se_HeroInCombatIcon>(this, new CastInfo(this));
	}

	protected override StaggerSettings GetStaggerSettings()
	{
		return StaggerSettings.HeroDefault;
	}

	private bool AssignComponents()
	{
		_Skill = ((Component)(object)this).GetComponent<HeroSkill>();
		_Skill.entity = this;
		return true;
	}

	[Server]
	public void ReceiveExperience(int amount)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Hero::ReceiveExperience(System.Int32)' called when server was not active");
		}
		else
		{
			exp += amount;
		}
	}

	private void OnHeroLevelUp(EventInfoHeroLevelUp obj)
	{
		FxPlayNewNetworked(_levelUpEffect, this);
		float num = Mathf.Clamp(0.2f - (float)obj.oldLevel * 0.0075f, 0.05f, 1f);
		Heal(Status.missingHealth * num).Dispatch(this);
		InGameUIManager.instance.ShowWorldPopMessage(new WorldMessageSetting
		{
			color = new Color(1f, 0.65f, 0.96f),
			rawText = DewLocalization.GetUIValue("InGame_Message_LevelUpPopUp"),
			worldPosGetter = () => this.IsNullInactiveDeadOrKnockedOut() ? Vector3.zero : Visual.GetCenterPosition()
		});
	}

	[Server]
	public void MarkAsInCombat()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Hero::MarkAsInCombat()' called when server was not active");
			return;
		}
		_lastIsInCombatMarkTime = Time.time;
		if (!isInCombat)
		{
			Network_003CisInCombat_003Ek__BackingField = true;
		}
	}

	public override void LoadEntityModelLocal()
	{
		if (string.IsNullOrEmpty(skin) || !owner.IsAllowedToUseItem(skin))
		{
			base.LoadEntityModelLocal();
			return;
		}
		Skin byName = DewResources.GetByName<Skin>(skin);
		if (byName == null || !byName.IsValidFor(((object)this).GetType().Name))
		{
			base.LoadEntityModelLocal();
		}
		else
		{
			Visual.LoadModelLocal(byName.GetComponent<EntityModel>());
		}
	}

	public bool IsMeleeHero()
	{
		return Dew.IsMeleeHero(classType);
	}

	public bool IsRangedHero()
	{
		return Dew.IsRangedHero(classType);
	}

	[Command]
	public void CmdTeleportToWaypoint(Room_Waypoint waypoint)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteNetworkBehaviour((NetworkWriter)(object)val, (NetworkBehaviour)(object)waypoint);
		((NetworkBehaviour)this).SendCommandInternal("System.Void Hero::CmdTeleportToWaypoint(Room_Waypoint)", 1444585225, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	public Hero()
	{
		((NetworkBehaviour)this).InitSyncObject((SyncObject)(object)accessories);
		_Mirror_SyncVarHookDelegate__003CisInCombat_003Ek__BackingField = OnIsInCombatChanged;
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcInvokeOnKillOrAssist__EventInfoKill(EventInfoKill obj)
	{
		try
		{
			ClientHeroEvent_OnKillOrAssist?.Invoke(obj);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception, (UnityEngine.Object)(object)this);
		}
	}

	protected static void InvokeUserCode_RpcInvokeOnKillOrAssist__EventInfoKill(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcInvokeOnKillOrAssist called on server.");
		}
		else
		{
			((Hero)(object)obj).UserCode_RpcInvokeOnKillOrAssist__EventInfoKill(GeneratedNetworkCode._Read_EventInfoKill(reader));
		}
	}

	protected void UserCode_CmdTeleportToWaypoint__Room_Waypoint(Room_Waypoint waypoint)
	{
		if (!((UnityEngine.Object)(object)waypoint == null) && waypoint.isUnlocked)
		{
			if (isInCombat)
			{
				owner.TpcShowCenterMessage(CenterMessageType.Error, "InGame_Message_TeleportUnavailableInCombat");
			}
			else if (!Status.HasStatusEffect<Se_WaypointTeleport>())
			{
				CreateStatusEffect<Se_WaypointTeleport>(this, new CastInfo(this, ((Component)(object)waypoint).transform.position));
			}
		}
	}

	protected static void InvokeUserCode_CmdTeleportToWaypoint__Room_Waypoint(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdTeleportToWaypoint called on client.");
		}
		else
		{
			((Hero)(object)obj).UserCode_CmdTeleportToWaypoint__Room_Waypoint(NetworkReaderExtensions.ReadNetworkBehaviour<Room_Waypoint>(reader));
		}
	}

	static Hero()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected Obj, but got Unknown
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterCommand(typeof(Hero), "System.Void Hero::CmdTeleportToWaypoint(Room_Waypoint)", (RemoteCallDelegate)InvokeUserCode_CmdTeleportToWaypoint__Room_Waypoint, true);
		RemoteProcedureCalls.RegisterRpc(typeof(Hero), "System.Void Hero::RpcInvokeOnKillOrAssist(EventInfoKill)", (RemoteCallDelegate)InvokeUserCode_RpcInvokeOnKillOrAssist__EventInfoKill);
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			GeneratedNetworkCode._Write_HeroLoadoutData(writer, loadout__BackingField);
			NetworkWriterExtensions.WriteInt(writer, _exp);
			NetworkWriterExtensions.WriteBool(writer, _isKnockedOut);
			NetworkWriterExtensions.WriteBool(writer, isInCombat__BackingField);
			NetworkWriterExtensions.WriteString(writer, skin__BackingField);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x20L) != 0L)
		{
			GeneratedNetworkCode._Write_HeroLoadoutData(writer, loadout__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x40L) != 0L)
		{
			NetworkWriterExtensions.WriteInt(writer, _exp);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x80L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, _isKnockedOut);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x100L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, isInCombat__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x200L) != 0L)
		{
			NetworkWriterExtensions.WriteString(writer, skin__BackingField);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<HeroLoadoutData>(ref loadout__BackingField, (Action<HeroLoadoutData, HeroLoadoutData>)null, GeneratedNetworkCode._Read_HeroLoadoutData(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref _exp, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref _isKnockedOut, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref isInCombat__BackingField, _Mirror_SyncVarHookDelegate__003CisInCombat_003Ek__BackingField, NetworkReaderExtensions.ReadBool(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<string>(ref skin__BackingField, (Action<string, string>)null, NetworkReaderExtensions.ReadString(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x20L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<HeroLoadoutData>(ref loadout__BackingField, (Action<HeroLoadoutData, HeroLoadoutData>)null, GeneratedNetworkCode._Read_HeroLoadoutData(reader));
		}
		if ((num & 0x40L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref _exp, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
		}
		if ((num & 0x80L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref _isKnockedOut, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
		}
		if ((num & 0x100L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref isInCombat__BackingField, _Mirror_SyncVarHookDelegate__003CisInCombat_003Ek__BackingField, NetworkReaderExtensions.ReadBool(reader));
		}
		if ((num & 0x200L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<string>(ref skin__BackingField, (Action<string, string>)null, NetworkReaderExtensions.ReadString(reader));
		}
	}
}
