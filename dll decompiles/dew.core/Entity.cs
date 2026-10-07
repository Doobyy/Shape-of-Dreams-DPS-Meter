using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

[LogicUpdatePriority(-400)]
[RequireComponent(typeof(EntityAI))]
[RequireComponent(typeof(EntityAnimation))]
[RequireComponent(typeof(EntityControl))]
[RequireComponent(typeof(EntityStatus))]
[RequireComponent(typeof(EntityVisual))]
[RequireComponent(typeof(EntityAbility))]
[RequireComponent(typeof(EntitySound))]
public class Entity : Actor
{
	public struct StaggerSettings
	{
		public static readonly StaggerSettings HeroDefault = new StaggerSettings
		{
			enabled = false
		};

		public static readonly StaggerSettings LesserMonsterDefault = new StaggerSettings
		{
			enabled = true,
			canStaggerWhileChanneling = true,
			highDamageStaggerThreshold = 0.3f,
			accumulatedDamageStaggerThreshold = 0.5f,
			knockbackDistance = 0.45f,
			knockbackDuration = 0.25f,
			knockbackEase = DewEase.EaseOutQuart,
			stunDuration = 0.5f,
			chance = 0.75f,
			chanceInChannel = 0.8f,
			staggerImmunityTime = 0f
		};

		public static readonly StaggerSettings NormalMonsterDefault = new StaggerSettings
		{
			enabled = true,
			canStaggerWhileChanneling = true,
			highDamageStaggerThreshold = 0.4f,
			accumulatedDamageStaggerThreshold = 0.7f,
			knockbackDistance = 0.45f,
			knockbackDuration = 0.25f,
			knockbackEase = DewEase.EaseOutQuart,
			stunDuration = 0.3f,
			chance = 0.75f,
			chanceInChannel = 0.5f,
			staggerImmunityTime = 0f
		};

		public static readonly StaggerSettings MiniBossMonsterDefault = new StaggerSettings
		{
			enabled = true,
			canStaggerWhileChanneling = false,
			highDamageStaggerThreshold = 0.4f,
			accumulatedDamageStaggerThreshold = 0.7f,
			knockbackDistance = 0.45f,
			knockbackDuration = 0.25f,
			knockbackEase = DewEase.EaseOutQuart,
			stunDuration = 0.3f,
			chance = 0.75f,
			chanceInChannel = 0f,
			staggerImmunityTime = 3f
		};

		public static readonly StaggerSettings BossMonsterDefault = new StaggerSettings
		{
			enabled = false
		};

		public static readonly StaggerSettings SummonDefault = new StaggerSettings
		{
			enabled = true,
			canStaggerWhileChanneling = true,
			highDamageStaggerThreshold = 0.3f,
			accumulatedDamageStaggerThreshold = 0.5f,
			knockbackDistance = 0.45f,
			knockbackDuration = 0.25f,
			knockbackEase = DewEase.EaseOutQuart,
			stunDuration = 0.3f,
			chance = 0.75f,
			chanceInChannel = 0.5f,
			staggerImmunityTime = 0f
		};

		public bool enabled;

		public bool canStaggerWhileChanneling;

		public float highDamageStaggerThreshold;

		public float accumulatedDamageStaggerThreshold;

		public float knockbackDistance;

		public float knockbackDuration;

		public DewEase knockbackEase;

		public float stunDuration;

		public float chance;

		public float chanceInChannel;

		public float staggerImmunityTime;
	}

	private EntityAI _AI;

	private EntityAnimation _Animation;

	private EntityAbility _Ability;

	private EntityControl _Control;

	private EntityStatus _Status;

	private EntityVisual _Visual;

	private EntitySound _Sound;

	[CompilerGenerated]
	[SyncVar(hook = "OnIsSleepingChanged")]
	private bool isSleeping__BackingField;

	public SafeAction<bool> ClientEntityEvent_OnIsSleepingChanged;

	internal float _accumulatedSleepTime;

	public SafeAction<EventInfoHeal> EntityEvent_OnTakeManaHeal;

	public SafeAction<EventInfoHeal> EntityEvent_OnTakeHeal;

	public SafeAction<EventInfoShield> EntityEvent_OnTakeShield;

	public SafeAction<EventInfoDamage> EntityEvent_OnTakeDamage;

	public SafeAction<EventInfoKill> EntityEvent_OnDeath;

	public SafeAction<EventInfoSpentMana> EntityEvent_OnGetManaSpent;

	public SafeAction<EventInfoAttackFired> EntityEvent_OnAttackFired;

	public SafeAction<EventInfoAttackFired> EntityEvent_OnAttackFiredBeforePrepare;

	public SafeAction<EventInfoAttackHit> EntityEvent_OnAttackHit;

	public SafeAction<EventInfoAttackHit> EntityEvent_OnAttackTaken;

	public SafeAction<EventInfoAttackMissed> EntityEvent_OnAttackMissed;

	public SafeAction<EventInfoAttackMissed> EntityEvent_OnAttackDodged;

	public SafeAction<EventInfoDamageNegatedByImmunity> EntityEvent_OnDamageNegatedByImmunity;

	public SafeAction<EventInfoDamageNegatedByShield> EntityEvent_OnDamageNegatedByShield;

	public SafeAction<EventInfoStatusEffect> ClientEntityEvent_OnStatusEffectAdded;

	public SafeAction<EventInfoStatusEffect> ClientEntityEvent_OnStatusEffectRemoved;

	public SafeAction<EventInfoAttackEffect> EntityEvent_OnAttackEffectTriggered;

	public SafeAction<EventInfoCast> EntityEvent_OnCastStart;

	public SafeAction<EventInfoCast> EntityEvent_OnCastComplete;

	public SafeAction<EventInfoCast> EntityEvent_OnCastCompleteBeforePrepare;

	public DataProcessorGroup<DamageData, Actor, Entity> takenDamageProcessor = new DataProcessorGroup<DamageData, Actor, Entity>();

	public DataProcessorGroup<HealData, Actor, Entity> takenHealProcessor = new DataProcessorGroup<HealData, Actor, Entity>();

	public DataProcessorGroup<HealData, Actor, Entity> takenManaHealProcessor = new DataProcessorGroup<HealData, Actor, Entity>();

	public DataProcessorGroup<HealData, Actor, Entity> takenShieldProcessor = new DataProcessorGroup<HealData, Actor, Entity>();

	[SyncVar]
	private DewPlayer _owner;

	private float _lastAttackerTime;

	private Vector3 _lastKnownPositionForNonAllies;

	private Vector3 _lastKnownAgentPositionForNonAllies;

	internal StaggerSettings _staggerSettings;

	private float _accDamage;

	private float _lastStaggerTime;

	private Action<EventInfoDamage> _cachedHandleDamageForStagger;

	internal readonly List<RoomSection> _sections = new List<RoomSection>();

	internal DataProcessor<VariantDef, Actor, Type> _skinSkillProcessor;

	internal int _skinVisualVariantId;

	internal int _wasStuckCounter;

	internal int _stuckCheckVersion;

	protected NetworkBehaviourSyncVar ____ownerNetId;

	public Action<bool, bool> _Mirror_SyncVarHookDelegate__003CisSleeping_003Ek__BackingField;

	public EntityAI AI => _AI;

	public EntityAnimation Animation => _Animation;

	public EntityAbility Ability => _Ability;

	public EntityControl Control => _Control;

	public EntityStatus Status => _Status;

	public EntityVisual Visual => _Visual;

	public EntitySound Sound => _Sound;

	public bool isSleeping
	{
		[CompilerGenerated]
		get
		{
			return isSleeping__BackingField;
		}
		[CompilerGenerated]
		internal set
		{
			Network_003CisSleeping_003Ek__BackingField = value;
		}
	}

	[SaveVar(SaveVarFlags.Default)]
	public DewPlayer owner
	{
		get
		{
			DewPlayer network_owner = Network_owner;
			if (!((UnityEngine.Object)(object)network_owner == null))
			{
				return network_owner;
			}
			return defaultOwner;
		}
		set
		{
			Network_owner = value;
			if (!((UnityEngine.Object)(object)((NetworkBehaviour)this).netIdentity == null) && ((NetworkBehaviour)this).netIdentity.isServer)
			{
				((NetworkBehaviour)this).netIdentity.RemoveClientAuthority();
				if ((UnityEngine.Object)(object)value != null && value.isHumanPlayer)
				{
					((NetworkBehaviour)this).netIdentity.AssignClientAuthority(((NetworkBehaviour)value).connectionToClient);
				}
			}
		}
	}

	protected virtual DewPlayer defaultOwner => DewPlayer.environment;

	internal Actor _lastAttacker { get; private set; }

	public float currentHealth => Status.currentHealth;

	public float currentMana => Status.currentMana;

	public float maxHealth => Status.maxHealth;

	public float maxHealthWithoutBonus => Status.maxHealthWithoutBonus;

	public float maxMana => Status.maxMana;

	public float normalizedHealth => Status.normalizedHealth;

	public float normalizedMana => Status.normalizedMana;

	public bool isAlive => Status.isAlive;

	public bool isDead => Status.isDead;

	public int level => Status.level;

	public Vector3 agentPosition => Control.agentPosition;

	public RoomSection section
	{
		get
		{
			for (int num = _sections.Count - 1; num >= 0; num--)
			{
				if ((bool)_sections[num])
				{
					return _sections[num];
				}
			}
			return null;
		}
	}

	public RoomSection lastSection { get; internal set; }

	public bool Network_003CisSleeping_003Ek__BackingField
	{
		get
		{
			return isSleeping__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref isSleeping__BackingField, 8uL, _Mirror_SyncVarHookDelegate__003CisSleeping_003Ek__BackingField);
		}
	}

	public DewPlayer Network_owner
	{
		get
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			return ((NetworkBehaviour)this).GetSyncVarNetworkBehaviour<DewPlayer>(____ownerNetId, ref _owner);
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter_NetworkBehaviour<DewPlayer>(value, ref _owner, 16uL, (Action<DewPlayer, DewPlayer>)null, ref ____ownerNetId);
		}
	}

	private void OnIsSleepingChanged(bool oldVal, bool newVal)
	{
		try
		{
			ClientEntityEvent_OnIsSleepingChanged?.Invoke(newVal);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		try
		{
			if (newVal)
			{
				NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnAwakeEntityRemove?.Invoke(this);
			}
			else
			{
				NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnAwakeEntityAdd?.Invoke(this);
			}
		}
		catch (Exception exception2)
		{
			Debug.LogException(exception2);
		}
	}

	public override void ClearPooledEventsAndProcessors()
	{
		base.ClearPooledEventsAndProcessors();
		ClientEntityEvent_OnIsSleepingChanged?.Clear();
		EntityEvent_OnTakeManaHeal?.Clear();
		EntityEvent_OnTakeHeal?.Clear();
		EntityEvent_OnTakeShield?.Clear();
		EntityEvent_OnTakeDamage?.Clear();
		EntityEvent_OnDeath?.Clear();
		EntityEvent_OnGetManaSpent?.Clear();
		EntityEvent_OnAttackFired?.Clear();
		EntityEvent_OnAttackFiredBeforePrepare?.Clear();
		EntityEvent_OnAttackHit?.Clear();
		EntityEvent_OnAttackTaken?.Clear();
		EntityEvent_OnAttackMissed?.Clear();
		EntityEvent_OnAttackDodged?.Clear();
		EntityEvent_OnDamageNegatedByImmunity?.Clear();
		EntityEvent_OnDamageNegatedByShield?.Clear();
		ClientEntityEvent_OnStatusEffectAdded?.Clear();
		ClientEntityEvent_OnStatusEffectRemoved?.Clear();
		EntityEvent_OnAttackEffectTriggered?.Clear();
		EntityEvent_OnCastStart?.Clear();
		EntityEvent_OnCastComplete?.Clear();
		EntityEvent_OnCastCompleteBeforePrepare?.Clear();
		takenDamageProcessor?.Clear();
		takenHealProcessor?.Clear();
		takenManaHealProcessor?.Clear();
		takenShieldProcessor?.Clear();
	}

	public Vector3 GetAIPosition(Entity prober)
	{
		if ((bool)(UnityEngine.Object)(object)prober && prober.CheckEnemyOrNeutral(this) && Status.isUndetectableByNonAllies)
		{
			return _lastKnownPositionForNonAllies;
		}
		return position;
	}

	public Vector3 GetAIAgentPosition(Entity prober)
	{
		if ((bool)(UnityEngine.Object)(object)prober && prober.CheckEnemyOrNeutral(this) && Status.isUndetectableByNonAllies)
		{
			return _lastKnownAgentPositionForNonAllies;
		}
		return agentPosition;
	}

	protected override void Awake()
	{
		base.Awake();
		_lastKnownPositionForNonAllies = position;
		_lastKnownAgentPositionForNonAllies = position;
		AssignComponents();
		spawnedChildVarDefProcessor.Add(delegate(ref VariantDef varDef, Actor _, Type spawnedType)
		{
			if (!varDef.Contains(DewResources.vOtherPlayersTonedDown) && (bool)(UnityEngine.Object)(object)owner && owner.isHumanPlayer)
			{
				if (FxSelectiveVisibility.forceFail)
				{
					varDef = varDef.Add(DewResources.vOtherPlayersTonedDown);
				}
				else if (spawnedType.IsSubclassOf(typeof(AbilityInstance)))
				{
					DewPlayer local = DewPlayer.local;
					if ((bool)(UnityEngine.Object)(object)ManagerBase<CameraManager>.instance.focusedEntity && (bool)(UnityEngine.Object)(object)ManagerBase<CameraManager>.instance.focusedEntity.owner && ManagerBase<CameraManager>.instance.focusedEntity.owner.isHumanPlayer)
					{
						local = ManagerBase<CameraManager>.instance.focusedEntity.owner;
					}
					if ((bool)(UnityEngine.Object)(object)local && !((UnityEngine.Object)(object)local == (UnityEngine.Object)(object)owner))
					{
						varDef = varDef.Add(DewResources.vOtherPlayersTonedDown);
					}
				}
			}
		});
		spawnedChildVarDefProcessor.Add(delegate(ref VariantDef varDef, Actor _, Type spawnedType)
		{
			if (_skinSkillProcessor != null)
			{
				_skinSkillProcessor(ref varDef, _, spawnedType);
			}
		});
	}

	protected override void OnPrepare()
	{
		base.OnPrepare();
		if (!isNewInstance)
		{
			Visual.NetworkskipSpawning = true;
		}
		Status.CalculateStats();
		Status.currentHealth = maxHealth;
		Status.currentMana = maxMana;
	}

	public override void OnStartServer()
	{
		base.OnStartServer();
		if ((UnityEngine.Object)(object)owner == null)
		{
			owner = DewPlayer.creep;
		}
		_staggerSettings = GetStaggerSettings();
		EntityEvent_OnTakeDamage += new Action<EventInfoDamage>(HandleDamageForStagger);
	}

	public override void OnStopServer()
	{
		base.OnStopServer();
		EntityEvent_OnTakeDamage -= _cachedHandleDamageForStagger;
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if ((bool)(UnityEngine.Object)(object)Control && (bool)Control.proxyCollider)
		{
			DewPhysics.RemoveProxy(Control.proxyCollider);
		}
		if (_sections.Count == 1)
		{
			RoomSection roomSection = _sections[0];
			if ((bool)roomSection)
			{
				roomSection.HandleEntityExit(this);
			}
		}
		else if (_sections.Count > 1)
		{
			ListReturnHandle<RoomSection> handle;
			foreach (RoomSection item in _sections.ToListNonAlloc(out handle))
			{
				if ((bool)item)
				{
					item.HandleEntityExit(this);
				}
			}
			handle.Return();
		}
		_sections.Clear();
		if (!isSleeping)
		{
			try
			{
				NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnAwakeEntityRemove?.Invoke(this);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)_lastAttacker != null)
		{
			_lastAttacker.UnlockDestroy();
			_lastAttacker = null;
		}
		if ((UnityEngine.Object)(object)Control != null)
		{
			DewPhysics.RemoveProxy(Control.proxyCollider);
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!Status.isUndetectableByNonAllies)
		{
			_lastKnownPositionForNonAllies = position;
			_lastKnownAgentPositionForNonAllies = agentPosition;
		}
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)_lastAttacker != null && Time.time - _lastAttackerTime > 30f)
		{
			_lastAttacker.UnlockDestroy();
			_lastAttacker = null;
		}
	}

	protected virtual void OnDrawGizmos()
	{
		if (Application.IsPlaying((UnityEngine.Object)(object)this))
		{
			if (!isActive)
			{
				Gizmos.color = Color.gray;
				Gizmos.DrawCube(position, Vector3.one * 0.5f);
				return;
			}
			Gizmos.color = Color.yellow;
			Gizmos.DrawCube(position, Vector3.one * 0.5f);
			Gizmos.color = Color.cyan;
			Gizmos.DrawCube(agentPosition, Vector3.one * 0.35f);
		}
	}

	[Server]
	internal void SetLastAttacker(Actor actor)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Entity::SetLastAttacker(Actor)' called when server was not active");
		}
		else
		{
			if ((UnityEngine.Object)(object)actor == null || !((NetworkBehaviour)actor).isServer || !isActive)
			{
				return;
			}
			_lastAttackerTime = Time.time;
			if (!((UnityEngine.Object)(object)_lastAttacker == (UnityEngine.Object)(object)actor))
			{
				if ((UnityEngine.Object)(object)_lastAttacker != null)
				{
					_lastAttacker.UnlockDestroy();
				}
				_lastAttacker = actor;
				actor.LockDestroy();
			}
		}
	}

	private void HandleDamageForStagger(EventInfoDamage info)
	{
		if (Control.isAirborne || !_staggerSettings.enabled || (!_staggerSettings.canStaggerWhileChanneling && Control.ongoingChannels.Count > 0))
		{
			return;
		}
		float num = ((Control.ongoingChannels.Count > 0) ? _staggerSettings.chanceInChannel : _staggerSettings.chance);
		if (info.damage.amount > maxHealth * _staggerSettings.highDamageStaggerThreshold)
		{
			if (!(Time.time - _lastStaggerTime < _staggerSettings.staggerImmunityTime) && !(UnityEngine.Random.value > num))
			{
				Stagger(info.damage.direction);
			}
			return;
		}
		_accDamage += info.damage.amount / maxHealth;
		if (_accDamage > _staggerSettings.accumulatedDamageStaggerThreshold && !(Time.time - _lastStaggerTime < _staggerSettings.staggerImmunityTime) && !(UnityEngine.Random.value > num))
		{
			Stagger(info.damage.direction);
		}
	}

	[Server]
	public void Stagger(Vector3? dir)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Entity::Stagger(System.Nullable`1<UnityEngine.Vector3>)' called when server was not active");
		}
		else if (!Status.hasCrowdControlImmunity)
		{
			_accDamage = 0f;
			_lastStaggerTime = Time.time;
			CreateBasicEffect(this, new StunEffect(), _staggerSettings.stunDuration, "Stagger");
			Animation.PlayStaggerAnimation();
			if (!dir.HasValue)
			{
				Control.Rotate(Quaternion.Euler(0f, UnityEngine.Random.Range(-30, 30), 0f) * ((Component)(object)this).transform.forward, immediately: true);
				return;
			}
			dir = dir.Value.Flattened().normalized;
			dir = Quaternion.Euler(0f, UnityEngine.Random.Range(-30, 30), 0f) * dir.Value;
			Control.Rotate(-dir.Value, immediately: true);
			Control.StartDisplacement(new DispByDestination
			{
				destination = position + dir.Value * _staggerSettings.knockbackDistance,
				duration = _staggerSettings.knockbackDuration,
				ease = _staggerSettings.knockbackEase,
				isFriendly = false,
				onCancel = null,
				onFinish = null,
				rotateForward = false,
				canGoOverTerrain = false,
				isCanceledByCC = false
			});
		}
	}

	protected virtual StaggerSettings GetStaggerSettings()
	{
		return new StaggerSettings
		{
			enabled = false
		};
	}

	protected virtual void AIUpdate(ref EntityAIContext context)
	{
	}

	internal void CallAIUpdate(ref EntityAIContext context)
	{
		if (AI.customBehaviors.Execute(ref context))
		{
			return;
		}
		if ((UnityEngine.Object)(object)owner == (UnityEngine.Object)(object)DewPlayer.local && (UnityEngine.Object)(object)context.targetEnemy != null)
		{
			Debug.DrawLine(position, context.targetEnemy.position, Color.red, 0.1f);
		}
		try
		{
			AIUpdate(ref context);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception, (UnityEngine.Object)(object)this);
		}
	}

	public void ProcessReceivedDamage(ref DamageData data, Actor actor)
	{
		takenDamageProcessor.Process(ref data, actor, this);
	}

	public void ProcessReceivedHeal(ref HealData data, Actor actor)
	{
		takenHealProcessor.Process(ref data, actor, this);
	}

	public void ProcessReceivedManaHeal(ref HealData data, Actor actor)
	{
		takenManaHealProcessor.Process(ref data, actor, this);
	}

	public void ProcessReceivedShield(ref HealData data, Actor actor)
	{
		takenShieldProcessor.Process(ref data, actor, this);
	}

	private bool AssignComponents()
	{
		_AI = ((Component)(object)this).GetComponent<EntityAI>();
		_Ability = ((Component)(object)this).GetComponent<EntityAbility>();
		_Control = ((Component)(object)this).GetComponent<EntityControl>();
		_Status = ((Component)(object)this).GetComponent<EntityStatus>();
		_Animation = ((Component)(object)this).GetComponent<EntityAnimation>();
		_Visual = ((Component)(object)this).GetComponent<EntityVisual>();
		_Sound = ((Component)(object)this).GetComponent<EntitySound>();
		_AI.entity = this;
		_Ability.entity = this;
		_Control.entity = this;
		_Status.entity = this;
		_Animation.entity = this;
		_Visual.entity = this;
		_Sound.entity = this;
		return true;
	}

	public bool CheckEnemyOrNeutral(Entity target)
	{
		EntityRelation relation = GetRelation(target);
		if (relation != EntityRelation.Enemy)
		{
			return relation == EntityRelation.Neutral;
		}
		return true;
	}

	public EntityRelation GetRelation(Entity other)
	{
		if ((UnityEngine.Object)(object)other == null)
		{
			return EntityRelation.Neutral;
		}
		if ((UnityEngine.Object)(object)this == (UnityEngine.Object)(object)other)
		{
			return EntityRelation.Self;
		}
		return GetTeamRelation(other.owner) switch
		{
			TeamRelation.Own => EntityRelation.Ally, 
			TeamRelation.Enemy => EntityRelation.Enemy, 
			TeamRelation.Ally => EntityRelation.Ally, 
			_ => EntityRelation.Neutral, 
		};
	}

	public TeamRelation GetTeamRelation(DewPlayer other)
	{
		DewPlayer dewPlayer = owner;
		if ((UnityEngine.Object)(object)dewPlayer == null)
		{
			return TeamRelation.Neutral;
		}
		return dewPlayer.GetTeamRelation(other);
	}

	[Server]
	public void Kill()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Entity::Kill()' called when server was not active");
		}
		else
		{
			Kill(ignoreDeathInterrupts: false);
		}
	}

	[Server]
	public void KillNoInterrupt()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Entity::KillNoInterrupt()' called when server was not active");
		}
		else
		{
			Kill(ignoreDeathInterrupts: true);
		}
	}

	[Server]
	private void Kill(bool ignoreDeathInterrupts)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Entity::Kill(System.Boolean)' called when server was not active");
		}
		else
		{
			if (!isActive)
			{
				return;
			}
			EventInfoKill eventInfoKill = new EventInfoKill
			{
				actor = _lastAttacker,
				victim = this
			};
			Status.currentHealth = 0f;
			if ((UnityEngine.Object)(object)eventInfoKill.actor == null)
			{
				eventInfoKill.actor = this;
			}
			if (Status.hasDeathInterrupt && !ignoreDeathInterrupts)
			{
				List<DeathInterruptEffect> list = DewPool.GetList(out ListReturnHandle<DeathInterruptEffect> handle);
				foreach (BasicEffect basicEffect in Status._basicEffects)
				{
					if (basicEffect is DeathInterruptEffect item)
					{
						list.Add(item);
					}
				}
				list.Sort((DeathInterruptEffect x, DeathInterruptEffect y) => x.priority.CompareTo(y.priority));
				foreach (DeathInterruptEffect item2 in list)
				{
					try
					{
						item2.onInterrupt?.Invoke(eventInfoKill);
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
					}
					if (currentHealth > 0f)
					{
						handle.Return();
						return;
					}
				}
				handle.Return();
			}
			EntityEvent_OnDeath?.Invoke(eventInfoKill);
			if ((UnityEngine.Object)(object)eventInfoKill.actor != null)
			{
				eventInfoKill.actor.InvokeOnKill(eventInfoKill);
			}
			NotifyEntityDeathToClients(eventInfoKill);
			Destroy();
		}
	}

	protected virtual void OnDeath(EventInfoKill info)
	{
	}

	[ClientRpc]
	internal void NotifyEntityDeathToClients(EventInfoKill info)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_EventInfoKill((NetworkWriter)(object)val, info);
		((NetworkBehaviour)this).SendRPCInternal("System.Void Entity::NotifyEntityDeathToClients(EventInfoKill)", 1937835807, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	public override string GetActorReadableName()
	{
		return $"{base.GetActorReadableName()} of {((UnityEngine.Object)(object)owner).name} ({Status.level})";
	}

	[ClientRpc]
	internal void RpcInvokeInteract(NetworkIdentity target, bool alt)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteNetworkIdentity((NetworkWriter)(object)val, target);
		NetworkWriterExtensions.WriteBool((NetworkWriter)(object)val, alt);
		((NetworkBehaviour)this).SendRPCInternal("System.Void Entity::RpcInvokeInteract(Mirror.NetworkIdentity,System.Boolean)", -1989805683, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[Server]
	public void WakeUp()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Entity::WakeUp()' called when server was not active");
		}
		else if (isSleeping)
		{
			Network_003CisSleeping_003Ek__BackingField = false;
			_accumulatedSleepTime = 0f;
		}
	}

	[Server]
	public void Sleep()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Entity::Sleep()' called when server was not active");
		}
		else if (!isSleeping)
		{
			Network_003CisSleeping_003Ek__BackingField = true;
			if ((UnityEngine.Object)(object)AI != null)
			{
				AI.DropAggro();
			}
		}
	}

	public override bool ShouldBeSavedWithRoom()
	{
		return true;
	}

	protected override NetworkConnectionToClient GetNetworkAuthorityConnection()
	{
		return owner;
	}

	public virtual bool CanAggroTo(Entity target)
	{
		return true;
	}

	public virtual void LoadEntityModelLocal()
	{
		Visual.LoadModelDefaultLocal();
	}

	public virtual void OnModelLoaded()
	{
	}

	public virtual bool CanSleep()
	{
		return true;
	}

	public Entity()
	{
		_Mirror_SyncVarHookDelegate__003CisSleeping_003Ek__BackingField = OnIsSleepingChanged;
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_NotifyEntityDeathToClients__EventInfoKill(EventInfoKill info)
	{
		OnDeath(info);
	}

	protected static void InvokeUserCode_NotifyEntityDeathToClients__EventInfoKill(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC NotifyEntityDeathToClients called on server.");
		}
		else
		{
			((Entity)(object)obj).UserCode_NotifyEntityDeathToClients__EventInfoKill(GeneratedNetworkCode._Read_EventInfoKill(reader));
		}
	}

	protected void UserCode_RpcInvokeInteract__NetworkIdentity__Boolean(NetworkIdentity target, bool alt)
	{
		IInteractable component = ((Component)(object)target).GetComponent<IInteractable>();
		if (component != null)
		{
			try
			{
				component.OnInteract(this, alt);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception, component as UnityEngine.Object);
			}
			ManagerBase<ControlManager>.instance.UpdateInteractableFocus(alsoCheckNearby: true);
		}
	}

	protected static void InvokeUserCode_RpcInvokeInteract__NetworkIdentity__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcInvokeInteract called on server.");
		}
		else
		{
			((Entity)(object)obj).UserCode_RpcInvokeInteract__NetworkIdentity__Boolean(NetworkReaderExtensions.ReadNetworkIdentity(reader), NetworkReaderExtensions.ReadBool(reader));
		}
	}

	static Entity()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterRpc(typeof(Entity), "System.Void Entity::NotifyEntityDeathToClients(EventInfoKill)", (RemoteCallDelegate)InvokeUserCode_NotifyEntityDeathToClients__EventInfoKill);
		RemoteProcedureCalls.RegisterRpc(typeof(Entity), "System.Void Entity::RpcInvokeInteract(Mirror.NetworkIdentity,System.Boolean)", (RemoteCallDelegate)InvokeUserCode_RpcInvokeInteract__NetworkIdentity__Boolean);
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteBool(writer, isSleeping__BackingField);
			NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)Network_owner);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 8L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, isSleeping__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x10L) != 0L)
		{
			NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)Network_owner);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref isSleeping__BackingField, _Mirror_SyncVarHookDelegate__003CisSleeping_003Ek__BackingField, NetworkReaderExtensions.ReadBool(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize_NetworkBehaviour<DewPlayer>(ref _owner, (Action<DewPlayer, DewPlayer>)null, reader, ref ____ownerNetId);
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 8L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref isSleeping__BackingField, _Mirror_SyncVarHookDelegate__003CisSleeping_003Ek__BackingField, NetworkReaderExtensions.ReadBool(reader));
		}
		if ((num & 0x10L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize_NetworkBehaviour<DewPlayer>(ref _owner, (Action<DewPlayer, DewPlayer>)null, reader, ref ____ownerNetId);
		}
	}
}
