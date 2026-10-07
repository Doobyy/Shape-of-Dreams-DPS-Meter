using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

[SelectionBase]
[DewResourceLink(ResourceLinkBy.Type)]
public class Actor : DewNetworkBehaviour, ICleanup, ICustomDestroyRoutine, IParentActorNetIdProvider
{
	private class Ad_ExcludedFromRoomSave
	{
	}

	private class RpcHandler
	{
		public Action<object> wrappedAction;

		public object originalAction;
	}

	public readonly struct AncestorEnumerable(Actor root) : IEnumerable<Actor>, IEnumerable
	{
		public struct Enumerator(Actor root) : IEnumerator<Actor>, IEnumerator, IDisposable
		{
			private Actor _cursor = root.parentActor;

			private Actor _current = null;

			private int _depth = 1;

			public Actor Current => _current;

			object IEnumerator.Current => _current;

			public bool MoveNext()
			{
				if ((UnityEngine.Object)(object)_cursor == null)
				{
					return false;
				}
				if (_depth >= 100)
				{
					throw new Exception("Ancestor depth reached upper limit, suspecting cyclic parenting and exiting enumeration.");
				}
				_current = _cursor;
				_cursor = _cursor.parentActor;
				_depth++;
				return true;
			}

			public void Reset()
			{
			}

			public void Dispose()
			{
			}
		}

		private readonly Actor _root = root;

		public Enumerator GetEnumerator()
		{
			return new Enumerator(_root);
		}

		IEnumerator<Actor> IEnumerable<Actor>.GetEnumerator()
		{
			return GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}

	private ParticleSystem[] _reuseParticleCache;

	private bool _reuseParticleCacheBaked;

	[CompilerGenerated]
	[SyncVar]
	private bool isNewInstance__BackingField = true;

	private float _creationTime;

	private int _destroyLock;

	private float _destroyLockUpdateTime;

	private uint _cachedNetId;

	private const int AncestorDepthLimit = 100;

	[SyncVar(hook = "OnParentActorChanged")]
	private Actor _parentActor;

	public readonly List<Actor> children = new List<Actor>();

	public readonly List<Actor> childrenInNetwork = new List<Actor>();

	private const float DestroyLockTimeout = 60f;

	[SyncVar(hook = "OnIsActiveChanged")]
	private bool _isActive;

	protected bool _didPrepare;

	internal bool _didCallCreate;

	internal bool _didCallDestroy;

	private IEffectComponent[] _cachedFxComponents;

	private string _originalName;

	private static readonly Stack<(float eAmp, int appliedStacks)> _pendingElemental;

	private static readonly Action<ElementalStatusEffect> _applyPendingElemental;

	public static Type[] KilledByUnstoppableEffects;

	public static Type[] ScaleDurationByTenacityEffects;

	private static readonly Stack<(BasicEffect eff, float duration, string id)> _pendingBasicEffects;

	private static readonly Action<Se_GenericEffectContainer> _applyPendingBasicEffect;

	private Dictionary<Type, List<RpcHandler>> _clientRpcHandlers = new Dictionary<Type, List<RpcHandler>>();

	private Dictionary<Type, List<RpcHandler>> _serverRpcHandlers = new Dictionary<Type, List<RpcHandler>>();

	private DewPlayer _serverRpcCaller;

	[SaveVar(SaveVarFlags.Default)]
	public readonly Dictionary<string, string> persistentData = new Dictionary<string, string>();

	[SaveVar(SaveVarFlags.Default)]
	public readonly SyncDictionary<string, string> persistentSyncedData = new SyncDictionary<string, string>();

	private HashSet<string> _persistentSyncedDataKnownKeys = new HashSet<string>();

	public SafeAction<string> ClientEvent_OnPersistentSyncedDataChanged = new SafeAction<string>();

	private Dictionary<Type, object> _dataByType = new Dictionary<Type, object>();

	private List<object> _data = new List<object>();

	public SafeAction<EventInfoAbilityInstance> ActorEvent_OnAbilityInstanceCreated;

	public SafeAction<EventInfoAbilityInstance> ActorEvent_OnAbilityInstanceBeforePrepare;

	public SafeAction<EventInfoDamage> ActorEvent_OnDealDamage;

	public SafeAction<EventInfoHeal> ActorEvent_OnDoHeal;

	public SafeAction<EventInfoHeal> ActorEvent_OnDoManaHeal;

	public SafeAction<EventInfoSpentMana> ActorEvent_OnSpendMana;

	public SafeAction<EventInfoKill> ActorEvent_OnKill;

	public SafeAction<EventInfoAttackHit> ActorEvent_OnAttackHit;

	public SafeAction<EventInfoAttackEffect> ActorEvent_OnAttackEffectTriggered;

	public SafeAction<EventInfoSummon> ActorEvent_OnSpawnSummon;

	public SafeAction<EventInfoApplyElemental> ActorEvent_OnApplyElemental;

	public SafeAction<EventInfoShield> ActorEvent_OnGiveShield;

	public SafeAction<Actor> ClientActorEvent_OnDestroyed;

	public SafeAction<Actor> ClientActorEvent_OnCreate;

	public DataProcessorGroup<DamageData, Actor, Entity> dealtDamageProcessor = new DataProcessorGroup<DamageData, Actor, Entity>();

	public DataProcessorGroup<HealData, Actor, Entity> dealtHealProcessor = new DataProcessorGroup<HealData, Actor, Entity>();

	public DataProcessorGroup<HealData, Actor, Entity> dealtManaHealProcessor = new DataProcessorGroup<HealData, Actor, Entity>();

	public DataProcessorGroup<HealData, Actor, Entity> dealtShieldProcessor = new DataProcessorGroup<HealData, Actor, Entity>();

	public DataProcessorGroup<CooldownReductionSettings, Actor, AbilityTrigger> dealtCooldownReductionProcessor = new DataProcessorGroup<CooldownReductionSettings, Actor, AbilityTrigger>();

	public DataProcessorGroup<CooldownReductionByRatioSettings, Actor, AbilityTrigger> dealtCooldownReductionByRatioProcessor = new DataProcessorGroup<CooldownReductionByRatioSettings, Actor, AbilityTrigger>();

	public DataProcessorGroup<VariantDef, Actor, Type> spawnedChildVarDefProcessor = new DataProcessorGroup<VariantDef, Actor, Type>();

	protected NetworkBehaviourSyncVar ____parentActorNetId;

	public Action<Actor, Actor> _Mirror_SyncVarHookDelegate__parentActor;

	public Action<bool, bool> _Mirror_SyncVarHookDelegate__isActive;

	public virtual bool isDestroyedOnRoomChange => true;

	public virtual bool reuseInRoom => false;

	public virtual bool reuseInRoomSkipPrewarmCap => false;

	public bool isNewInstance
	{
		[CompilerGenerated]
		get
		{
			return isNewInstance__BackingField;
		}
		[CompilerGenerated]
		internal set
		{
			Network_003CisNewInstance_003Ek__BackingField = value;
		}
	}

	public float creationTime => _creationTime;

	public uint persistentNetId
	{
		get
		{
			if (((NetworkBehaviour)this).netId == 0)
			{
				return _cachedNetId;
			}
			_cachedNetId = ((NetworkBehaviour)this).netId;
			return _cachedNetId;
		}
	}

	public bool isDestroyLocked => _destroyLock > 0;

	public Actor parentActor
	{
		get
		{
			return Network_parentActor;
		}
		set
		{
			if (Network_parentActor != null)
			{
				Network_parentActor.children.Remove(this);
				Network_parentActor.childrenInNetwork.Remove(this);
			}
			Network_parentActor = value;
			if (Network_parentActor != null && (bool)(UnityEngine.Object)(object)this)
			{
				if (isActive)
				{
					Network_parentActor.children.Add(this);
				}
				if (((NetworkBehaviour)this).isClient)
				{
					Network_parentActor.childrenInNetwork.Add(this);
				}
			}
		}
	}

	[SaveVar(SaveVarFlags.Default)]
	public Vector3 position
	{
		get
		{
			return ((Component)(object)this).transform.position;
		}
		set
		{
			((Component)(object)this).transform.position = value;
		}
	}

	[SaveVar(SaveVarFlags.Default)]
	public Quaternion rotation
	{
		get
		{
			return ((Component)(object)this).transform.rotation;
		}
		set
		{
			((Component)(object)this).transform.rotation = value;
		}
	}

	bool ICleanup.canDestroy
	{
		get
		{
			if (childrenInNetwork.Count > 0)
			{
				return false;
			}
			if (_destroyLock <= 0)
			{
				return true;
			}
			if (Time.time - _destroyLockUpdateTime <= 60f)
			{
				return false;
			}
			UnityEngine.Debug.LogWarning("'" + ((UnityEngine.Object)(object)this).name + "' destroy lock timed out! Ignoring the lock...");
			_destroyLock = 0;
			return true;
		}
	}

	public bool isActive => _isActive;

	public bool isDestroyed => !_isActive;

	public uint parentActorNetId
	{
		get
		{
			if ((bool)(UnityEngine.Object)(object)parentActor && (bool)(UnityEngine.Object)(object)((NetworkBehaviour)parentActor).netIdentity)
			{
				return ((NetworkBehaviour)parentActor).netId;
			}
			return 0u;
		}
	}

	public AncestorEnumerable ancestors => new AncestorEnumerable(this);

	public Entity firstEntity => FindFirstOfType<Entity>();

	public AbilityTrigger firstTrigger => FindFirstOfType<AbilityTrigger>();

	public bool Network_003CisNewInstance_003Ek__BackingField
	{
		get
		{
			return isNewInstance__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref isNewInstance__BackingField, 1uL, (Action<bool, bool>)null);
		}
	}

	public Actor Network_parentActor
	{
		get
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			return ((NetworkBehaviour)this).GetSyncVarNetworkBehaviour<Actor>(____parentActorNetId, ref _parentActor);
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter_NetworkBehaviour<Actor>(value, ref _parentActor, 2uL, _Mirror_SyncVarHookDelegate__parentActor, ref ____parentActorNetId);
		}
	}

	public bool Network_isActive
	{
		get
		{
			return _isActive;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref _isActive, 4uL, _Mirror_SyncVarHookDelegate__isActive);
		}
	}

	public void ResetParticleSystemsForReuse()
	{
		if (!_reuseParticleCacheBaked)
		{
			_reuseParticleCacheBaked = true;
			_reuseParticleCache = DewEffect.BakeResetParticleSystems(((Component)(object)this).gameObject);
		}
		DewEffect.StopResetParticleSystems(_reuseParticleCache);
	}

	private void OnParentActorChanged(Actor prevActor, Actor newActor)
	{
		if (NetworkServer.active)
		{
			return;
		}
		if (prevActor != null)
		{
			prevActor.children.Remove(this);
			prevActor.childrenInNetwork.Remove(this);
		}
		if (newActor != null && (bool)(UnityEngine.Object)(object)this)
		{
			if (isActive)
			{
				Network_parentActor.children.Add(this);
			}
			if (((NetworkBehaviour)this).isClient)
			{
				Network_parentActor.childrenInNetwork.Add(this);
			}
		}
	}

	protected override void Awake()
	{
		base.Awake();
		if ((bool)((Component)(object)this).transform.parent)
		{
			((Component)(object)this).transform.SetParent(null, worldPositionStays: true);
		}
	}

	public override void OnStartServer()
	{
		base.OnStartServer();
		InvokeOnPrepareIfDidnt();
		Network_isActive = true;
	}

	public override void OnStart()
	{
		base.OnStart();
		if (!((UnityEngine.Object)(object)NetworkedManagerBase<ActorManager>.instance == null))
		{
			_creationTime = Time.time;
			UnityEngine.Object.DontDestroyOnLoad((UnityEngine.Object)(object)this);
		}
	}

	public override void OnStartClient()
	{
		base.OnStartClient();
		if (Network_parentActor != null && (bool)(UnityEngine.Object)(object)this)
		{
			if (isActive && !Network_parentActor.children.Contains(this))
			{
				Network_parentActor.children.Add(this);
			}
			if (!Network_parentActor.childrenInNetwork.Contains(this))
			{
				Network_parentActor.childrenInNetwork.Add(this);
			}
		}
		InvokeOnCreateIfDidnt();
		if (!isActive)
		{
			InvokeOnDestroyActorIfDidnt();
		}
	}

	public override void LogicUpdate(float dt)
	{
		base.LogicUpdate(dt);
		if (isActive)
		{
			try
			{
				ActiveLogicUpdate(dt);
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception, (UnityEngine.Object)(object)this);
			}
		}
		if (ActorManager.enableUsefulActorName)
		{
			((UnityEngine.Object)(object)this).name = GetActorReadableName();
		}
	}

	public override void FrameUpdate()
	{
		base.FrameUpdate();
		if (isActive)
		{
			try
			{
				ActiveFrameUpdate();
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception, (UnityEngine.Object)(object)this);
			}
		}
	}

	protected virtual void ActiveFrameUpdate()
	{
	}

	protected virtual void ActiveLogicUpdate(float dt)
	{
	}

	public virtual string GetActorReadableName()
	{
		return string.Format("[{0}{1}] {2}{3}{4}", persistentNetId, ((UnityEngine.Object)(object)parentActor != null) ? $" from {parentActor.persistentNetId}" : "", ((NetworkBehaviour)this).isClient ? "" : "~", isActive ? "" : "!", ((object)this).GetType());
	}

	public override string ToString()
	{
		if (!((UnityEngine.Object)(object)((NetworkBehaviour)this).netIdentity != null))
		{
			return ((UnityEngine.Object)(object)this).ToString();
		}
		return GetActorReadableName();
	}

	public void CustomDestroyRoutine()
	{
		throw new NotImplementedException();
	}

	void ICleanup.OnCleanup()
	{
		Network_isActive = false;
	}

	private void OnIsActiveChanged(bool oldVal, bool newVal)
	{
		if (!newVal)
		{
			InvokeOnCreateIfDidnt();
			InvokeOnDestroyActorIfDidnt();
		}
	}

	[Server]
	internal void PrepareAndSpawn()
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'System.Void Actor::PrepareAndSpawn()' called when server was not active");
			return;
		}
		InvokeOnPrepareIfDidnt();
		NetworkServer.Spawn(((Component)(object)this).gameObject, (NetworkConnection)(object)GetNetworkAuthorityConnection());
	}

	private void InvokeOnPrepareIfDidnt()
	{
		if (_didPrepare)
		{
			return;
		}
		_didPrepare = true;
		if ((UnityEngine.Object)(object)NetworkedManagerBase<ActorManager>.instance != null)
		{
			NetworkedManagerBase<ActorManager>.instance.onActorBeforePrepare?.Invoke(this);
		}
		try
		{
			OnPrepare();
		}
		catch (Exception exception)
		{
			UnityEngine.Debug.LogException(exception, (UnityEngine.Object)(object)this);
		}
	}

	protected virtual void OnPrepare()
	{
	}

	protected virtual void OnCreate()
	{
		try
		{
			InvokeOnCreate(this);
		}
		catch (Exception exception)
		{
			UnityEngine.Debug.LogException(exception);
		}
	}

	protected virtual void OnDestroyActor()
	{
		if (parentActor != null)
		{
			parentActor.children.Remove(this);
		}
		try
		{
			ClientActorEvent_OnDestroyed?.Invoke(this);
		}
		catch (Exception exception)
		{
			UnityEngine.Debug.LogException(exception, (UnityEngine.Object)(object)this);
		}
		if ((UnityEngine.Object)(object)NetworkedManagerBase<ActorManager>.instance != null)
		{
			NetworkedManagerBase<ActorManager>.instance.RemoveActor(this);
			NetworkedManagerBase<ActorManager>.instance.AddActorBeingDestroyed(this);
		}
	}

	public override void OnStopServer()
	{
		base.OnStopServer();
		if (parentActor != null)
		{
			parentActor.childrenInNetwork.Remove(this);
		}
	}

	public override void OnStopClient()
	{
		base.OnStopClient();
		if (parentActor != null)
		{
			parentActor.childrenInNetwork.Remove(this);
		}
	}

	protected virtual void OnEnable()
	{
	}

	protected virtual void OnDisable()
	{
		if ((UnityEngine.Object)(object)NetworkedManagerBase<ActorManager>.instance != null)
		{
			NetworkedManagerBase<ActorManager>.instance.RemoveActor(this);
			NetworkedManagerBase<ActorManager>.instance.RemoveActorBeingDestroyed(this);
		}
		_didCallDestroy = false;
		_didCallCreate = false;
		_didPrepare = false;
		_destroyLock = 0;
		DewPool.ClearEventsAndProcessors((Component)(object)this);
		parentActor = null;
	}

	public virtual void ClearPooledEventsAndProcessors()
	{
		ActorEvent_OnAbilityInstanceCreated?.Clear();
		ActorEvent_OnAbilityInstanceBeforePrepare?.Clear();
		ActorEvent_OnDealDamage?.Clear();
		ActorEvent_OnDoHeal?.Clear();
		ActorEvent_OnDoManaHeal?.Clear();
		ActorEvent_OnSpendMana?.Clear();
		ActorEvent_OnKill?.Clear();
		ActorEvent_OnAttackHit?.Clear();
		ActorEvent_OnAttackEffectTriggered?.Clear();
		ActorEvent_OnSpawnSummon?.Clear();
		ActorEvent_OnApplyElemental?.Clear();
		ActorEvent_OnGiveShield?.Clear();
		ClientActorEvent_OnDestroyed?.Clear();
		ClientActorEvent_OnCreate?.Clear();
		ClientEvent_OnPersistentSyncedDataChanged?.Clear();
		dealtDamageProcessor?.Clear();
		dealtHealProcessor?.Clear();
		dealtManaHealProcessor?.Clear();
		dealtShieldProcessor?.Clear();
		dealtCooldownReductionProcessor?.Clear();
		dealtCooldownReductionByRatioProcessor?.Clear();
		spawnedChildVarDefProcessor?.Clear();
	}

	private void InvokeOnCreateIfDidnt()
	{
		if (!_didCallCreate)
		{
			_didCallCreate = true;
			try
			{
				OnCreate();
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogError("Exception occured on " + ((Component)(object)this).transform.GetScenePath() + "::OnCreate");
				UnityEngine.Debug.LogException(exception, (UnityEngine.Object)(object)this);
			}
			NetworkedManagerBase<ActorManager>.instance.AddActor(this);
		}
	}

	internal void InvokeOnDestroyActorIfDidnt()
	{
		if (_didCallDestroy)
		{
			return;
		}
		_didCallDestroy = true;
		try
		{
			OnDestroyActor();
		}
		catch (Exception exception)
		{
			UnityEngine.Debug.LogError("Exception occured on " + ((Component)(object)this).transform.GetScenePath() + "::OnDestroyActor");
			UnityEngine.Debug.LogException(exception, (UnityEngine.Object)(object)this);
		}
	}

	public virtual bool ShouldBeSavedWithRoom()
	{
		return false;
	}

	void ICustomDestroyRoutine.CustomDestroyRoutine()
	{
		if (_cachedFxComponents == null)
		{
			_cachedFxComponents = ((Component)(object)this).GetComponentsInChildren<IEffectComponent>(true);
		}
		EffectAutoDestroy.Register(((Component)(object)this).gameObject, _cachedFxComponents);
	}

	protected virtual NetworkConnectionToClient GetNetworkAuthorityConnection()
	{
		return null;
	}

	[Server]
	public void InvalidatePoolReuseEverywhere()
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'System.Void Actor::InvalidatePoolReuseEverywhere()' called when server was not active");
			return;
		}
		InvalidateInstance();
		RpcInvalidatePoolReuse();
	}

	[ClientRpc]
	private void RpcInvalidatePoolReuse()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendRPCInternal("System.Void Actor::RpcInvalidatePoolReuse()", -413247510, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	public string GetOriginalName()
	{
		if (string.IsNullOrEmpty(_originalName))
		{
			_originalName = Dew.GetOriginalName(((UnityEngine.Object)(object)this).name);
		}
		return _originalName;
	}

	[Server]
	public void DoHeal(HealData heal, Entity target, ReactionChain chain = default(ReactionChain))
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'System.Void Actor::DoHeal(HealData,Entity,ReactionChain)' called when server was not active");
			return;
		}
		if (heal.originalAmount < 0f)
		{
			UnityEngine.Debug.LogWarning($"Heal by {GetActorReadableName()} original amount is {heal.originalAmount}");
			return;
		}
		ProcessDealtHeal(ref heal, target);
		target.ProcessReceivedHeal(ref heal, this);
		FinalHealData heal2 = new FinalHealData(heal, target.Status.missingHealth);
		if (!(heal2.amount + heal2.discardedAmount < 0.0001f))
		{
			if (heal2.amount + heal2.discardedAmount < 0f)
			{
				UnityEngine.Debug.LogWarning($"Heal by {GetActorReadableName()} final amount is {heal2.amount + heal2.discardedAmount}");
				return;
			}
			target.Status.currentHealth += heal2.amount;
			EventInfoHeal eventInfoHeal = new EventInfoHeal
			{
				actor = this,
				target = target,
				amount = heal2.amount,
				discardedAmount = heal2.discardedAmount,
				isCrit = heal2.isCrit,
				heal = heal2,
				chain = chain,
				canMerge = heal.canMerge
			};
			InvokeOnDoHeal(eventInfoHeal);
			target.EntityEvent_OnTakeHeal?.Invoke(eventInfoHeal);
		}
	}

	[Server]
	public void DoManaHeal(HealData heal, Entity target, ReactionChain chain = default(ReactionChain))
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'System.Void Actor::DoManaHeal(HealData,Entity,ReactionChain)' called when server was not active");
			return;
		}
		ProcessDealtManaHeal(ref heal, target);
		target.ProcessReceivedManaHeal(ref heal, this);
		FinalHealData heal2 = new FinalHealData(heal, target.Status.missingMana);
		if (!(heal2.amount < 0.01f))
		{
			target.Status.currentMana += heal2.amount;
			EventInfoHeal eventInfoHeal = new EventInfoHeal
			{
				actor = this,
				target = target,
				amount = heal2.amount,
				discardedAmount = heal2.discardedAmount,
				heal = heal2,
				isCrit = heal2.isCrit,
				chain = chain
			};
			InvokeOnDoManaHeal(eventInfoHeal);
			target.EntityEvent_OnTakeManaHeal?.Invoke(eventInfoHeal);
		}
	}

	public DamageData DefaultDamage(float amount, float procCoefficient = 1f)
	{
		return CreateDamage(DamageData.SourceType.Default, amount, procCoefficient);
	}

	public DamageData PhysicalDamage(float amount, float procCoefficient = 1f)
	{
		return CreateDamage(DamageData.SourceType.Physical, amount, procCoefficient);
	}

	public DamageData MagicDamage(float amount, float procCoefficient = 1f)
	{
		return CreateDamage(DamageData.SourceType.Magic, amount, procCoefficient);
	}

	public DamageData PureDamage(float amount, float procCoefficient = 1f)
	{
		return CreateDamage(DamageData.SourceType.Pure, amount, procCoefficient);
	}

	public DamageData CreateDamage(DamageData.SourceType type, float amount, float procCoefficient = 1f)
	{
		return new DamageData(type, amount, procCoefficient).SetActor(this);
	}

	public HealData Heal(float amount)
	{
		return new HealData(amount).SetActor(this);
	}

	[Server]
	public void DealDamage(DamageData damage, Entity target, ReactionChain chain = default(ReactionChain))
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'System.Void Actor::DealDamage(DamageData,Entity,ReactionChain)' called when server was not active");
		}
		else
		{
			if ((UnityEngine.Object)(object)target == null)
			{
				return;
			}
			if (target.IsNullInactiveDeadOrKnockedOut())
			{
				if (damage.attackEffectStrength > 0.001f)
				{
					TriggerAttackEffects(firstEntity, target, damage.attackEffectStrength, damage.attackEffectType, chain);
				}
				return;
			}
			if (damage.originalAmount < 0f)
			{
				UnityEngine.Debug.LogWarning($"Damage by {GetActorReadableName()} original amount is {damage.originalAmount}");
				return;
			}
			ProcessDealtDamage(ref damage, target);
			target.ProcessReceivedDamage(ref damage, this);
			float armor = target.Status.finalStats.armor;
			target.Status.CalculateStatsIfDirty();
			FinalDamageData finalDamageData = new FinalDamageData(damage, armor, target);
			if (finalDamageData.amount + finalDamageData.discardedAmount < 0f)
			{
				UnityEngine.Debug.LogWarning($"Damage by {GetActorReadableName()} final amount is {finalDamageData.amount + finalDamageData.discardedAmount}");
			}
			else
			{
				if (finalDamageData.amount + finalDamageData.discardedAmount <= 0.0001f)
				{
					return;
				}
				if (target is PropEntity { takeOneDamageOnHit: not false })
				{
					finalDamageData.amount = 1.0001f;
					finalDamageData.discardedAmount = 0f;
				}
				if (!damage.HasAttr(DamageAttribute.IgnoreDamageImmunity))
				{
					if (target.Status.hasDamageImmunity)
					{
						EventInfoDamageNegatedByImmunity arg = new EventInfoDamageNegatedByImmunity
						{
							actor = this,
							effect = target.Status._lastDamagePreventer,
							data = finalDamageData,
							victim = target
						};
						target.EntityEvent_OnDamageNegatedByImmunity?.Invoke(arg);
						return;
					}
					if (damage.isBlockedByImmunity)
					{
						EventInfoDamageNegatedByImmunity arg2 = new EventInfoDamageNegatedByImmunity
						{
							actor = this,
							effect = null,
							data = finalDamageData,
							victim = target
						};
						target.EntityEvent_OnDamageNegatedByImmunity?.Invoke(arg2);
						return;
					}
				}
				target.SetLastAttacker(this);
				if (finalDamageData.attackEffectStrength > 0.001f)
				{
					TriggerAttackEffects(firstEntity, target, finalDamageData.attackEffectStrength, finalDamageData.attackEffectType, chain);
				}
				float num = 0f;
				float num2 = finalDamageData.amount;
				if (!damage.HasAttr(DamageAttribute.IgnoreShield) && target.Status.currentShield > 0f)
				{
					List<BasicEffect> list = DewPool.GetList(out ListReturnHandle<BasicEffect> handle);
					foreach (BasicEffect basicEffect in target.Status._basicEffects)
					{
						list.Add(basicEffect);
					}
					for (int num3 = list.Count - 1; num3 >= 0; num3--)
					{
						if (list[num3].isAlive && list[num3] is ShieldEffect { amount: not (<0.0001f) } shieldEffect)
						{
							float num4 = num2 * damage.damageToShieldMultiplier;
							if (shieldEffect.amount > num4)
							{
								shieldEffect.amount -= num4;
								EventInfoDamageNegatedByShield arg3 = new EventInfoDamageNegatedByShield
								{
									actor = this,
									negatedAmount = num4,
									shield = shieldEffect,
									victim = target,
									damage = finalDamageData
								};
								num += num4;
								shieldEffect.onDamageNegated?.Invoke(arg3);
								target.EntityEvent_OnDamageNegatedByShield?.Invoke(arg3);
								num2 = 0f;
								break;
							}
							float amount = shieldEffect.amount;
							float num5 = shieldEffect.amount / damage.damageToShieldMultiplier;
							num2 -= num5;
							shieldEffect.amount = 0f;
							EventInfoDamageNegatedByShield arg4 = new EventInfoDamageNegatedByShield
							{
								actor = this,
								negatedAmount = amount,
								shield = shieldEffect,
								victim = target,
								damage = finalDamageData
							};
							num += amount;
							shieldEffect.onDamageNegated?.Invoke(arg4);
							target.EntityEvent_OnDamageNegatedByShield?.Invoke(arg4);
						}
					}
					handle.Return();
				}
				if (damage.HasAttr(DamageAttribute.DamageShieldOnly))
				{
					float num6 = finalDamageData.amount + finalDamageData.discardedAmount;
					finalDamageData.amount = num;
					finalDamageData.discardedAmount = num6 - num;
				}
				else
				{
					target.Status.currentHealth -= Mathf.Max(num2, 0f);
					finalDamageData.amount = num2 + num;
				}
				if (finalDamageData.elemental.HasValue)
				{
					if (finalDamageData.overrideElementalStacks.HasValue)
					{
						ApplyElemental(finalDamageData.elemental.Value, target, finalDamageData.overrideElementalStacks.Value);
					}
					else
					{
						switch (finalDamageData.elemental.Value)
						{
						case ElementalType.Fire:
						{
							Se_Elm_Fire effect;
							if (target.Status.fireStack == 0 || UnityEngine.Random.value < finalDamageData.procCoefficient)
							{
								ApplyElemental(ElementalType.Fire, target);
							}
							else if (target.Status.TryGetStatusEffect<Se_Elm_Fire>(out effect))
							{
								effect.ResetDecayTimer();
							}
							break;
						}
						case ElementalType.Cold:
						case ElementalType.Light:
						case ElementalType.Dark:
							ApplyElemental(finalDamageData.elemental.Value, target);
							break;
						}
					}
				}
				EventInfoDamage eventInfoDamage = new EventInfoDamage
				{
					actor = this,
					victim = target,
					damage = finalDamageData,
					chain = chain,
					negatedAmountByShield = num
				};
				try
				{
					InvokeOnDealDamage(eventInfoDamage);
				}
				catch (Exception exception)
				{
					UnityEngine.Debug.LogException(exception);
				}
				try
				{
					target.EntityEvent_OnTakeDamage?.Invoke(eventInfoDamage);
				}
				catch (Exception exception2)
				{
					UnityEngine.Debug.LogException(exception2);
				}
				if (target.Status.currentHealth <= 1E-05f)
				{
					target.Kill();
				}
			}
		}
	}

	[Server]
	public void DoBasicAttackHit(Entity from, Entity to, bool isCriticalHit, bool isMain, float damage, float attackEffect, ActionRef<DamageData> onBeforeDispatch = null)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'System.Void Actor::DoBasicAttackHit(Entity,Entity,System.Boolean,System.Boolean,System.Single,System.Single,ActionRef`1<DamageData>)' called when server was not active");
			return;
		}
		if (from.Status.hasBlind)
		{
			EventInfoAttackMissed arg = new EventInfoAttackMissed
			{
				actor = this,
				attacker = from,
				victim = to,
				isCrit = isCriticalHit
			};
			from.EntityEvent_OnAttackMissed?.Invoke(arg);
			to.EntityEvent_OnAttackDodged?.Invoke(arg);
			return;
		}
		DamageData item = new DamageData(DamageData.SourceType.Physical, new ScalingValue
		{
			adFactor = 1f
		}, from, 0, 1f);
		item.SetOriginPosition(from.position);
		if (isCriticalHit)
		{
			item.SetAttr(DamageAttribute.IsCrit);
			float num = from.Status.critChance;
			if (from.Status.hasAttackCritical)
			{
				num++;
			}
			if (num > 1f)
			{
				item.ApplyAmplification(DewMath.RandomRoundToInt(num));
			}
			else
			{
				item.ApplyAmplification(1f);
			}
			item.ApplyAmplification(from.Status.critAmp);
		}
		EventInfoAttackHit eventInfoAttackHit = new EventInfoAttackHit
		{
			actor = this,
			isCrit = isCriticalHit,
			attacker = from,
			victim = to,
			strength = damage
		};
		InvokeOnAttackHit(eventInfoAttackHit);
		from.EntityEvent_OnAttackHit?.Invoke(eventInfoAttackHit);
		to.EntityEvent_OnAttackTaken?.Invoke(eventInfoAttackHit);
		if (attackEffect > 0.001f)
		{
			item.DoAttackEffect((!isMain) ? AttackEffectType.BasicAttackSub : AttackEffectType.BasicAttackMain, attackEffect);
		}
		item.ApplyRawMultiplier(damage);
		onBeforeDispatch?.Invoke(ref item);
		DealDamage(item, to);
	}

	[Server]
	public void TriggerAttackEffects(Entity from, Entity target, float strength, AttackEffectType type, ReactionChain chain = default(ReactionChain))
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'System.Void Actor::TriggerAttackEffects(Entity,Entity,System.Single,AttackEffectType,ReactionChain)' called when server was not active");
			return;
		}
		if ((UnityEngine.Object)(object)from == null)
		{
			UnityEngine.Debug.LogError("Tried to TriggerAttackEffects with no from entity: " + GetActorReadableName(), (UnityEngine.Object)(object)this);
			return;
		}
		EventInfoAttackEffect eventInfoAttackEffect = new EventInfoAttackEffect
		{
			actor = this,
			attacker = from,
			victim = target,
			type = type,
			strength = strength,
			chain = chain
		};
		InvokeOnAttackEffectTriggered(eventInfoAttackEffect);
		from.EntityEvent_OnAttackEffectTriggered?.Invoke(eventInfoAttackEffect);
	}

	[Server]
	public void RefundMana(float amount, Entity target)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'System.Void Actor::RefundMana(System.Single,Entity)' called when server was not active");
		}
		else
		{
			target.Status.currentMana = Mathf.MoveTowards(target.currentMana, target.Status.maxMana, amount);
		}
	}

	[Server]
	public void SpendMana(float amount, Entity target)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'System.Void Actor::SpendMana(System.Single,Entity)' called when server was not active");
			return;
		}
		target.Status.currentMana = Mathf.MoveTowards(target.currentMana, 0f, amount);
		EventInfoSpentMana eventInfoSpentMana = new EventInfoSpentMana
		{
			actor = this,
			amount = amount,
			entity = target
		};
		InvokeOnSpendMana(eventInfoSpentMana);
		target.EntityEvent_OnGetManaSpent?.Invoke(eventInfoSpentMana);
	}

	[Server]
	public T CreateAbilityInstance<T>(T prefab, Vector3 position, Quaternion? rotation, CastInfo info, Action<T> beforePrepare = null) where T : AbilityInstance
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'T Actor::CreateAbilityInstance(T,UnityEngine.Vector3,System.Nullable`1<UnityEngine.Quaternion>,CastInfo,System.Action`1<T>)' called when server was not active");
			return null;
		}
		return Dew.CreateAbilityInstance(prefab, position, rotation, this, info, beforePrepare);
	}

	[Server]
	public T CreateAbilityInstance<T>(Vector3 position, Quaternion? rotation, CastInfo info, Action<T> beforePrepare = null) where T : AbilityInstance
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'T Actor::CreateAbilityInstance(UnityEngine.Vector3,System.Nullable`1<UnityEngine.Quaternion>,CastInfo,System.Action`1<T>)' called when server was not active");
			return null;
		}
		return CreateAbilityInstance(DewResources.GetByType<T>(DewResources.GetSuggestedResourceLoadSettings(this, typeof(T))), position, rotation, info, beforePrepare);
	}

	[Server]
	public AbilityInstance CreateAbilityInstance(Type type, Vector3 position, Quaternion? rotation, CastInfo info, Action<AbilityInstance> beforePrepare = null)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'AbilityInstance Actor::CreateAbilityInstance(System.Type,UnityEngine.Vector3,System.Nullable`1<UnityEngine.Quaternion>,CastInfo,System.Action`1<AbilityInstance>)' called when server was not active");
			return null;
		}
		return CreateAbilityInstance(DewResources.GetByType<AbilityInstance>(type, DewResources.GetSuggestedResourceLoadSettings(this, type)), position, rotation, info, beforePrepare);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[Server]
	public T CreatePickupInstance<T>(Vector3 position, Quaternion? rotation, CastInfo info, Action<T> beforePrepare = null) where T : PickupInstance
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'T Actor::CreatePickupInstance(UnityEngine.Vector3,System.Nullable`1<UnityEngine.Quaternion>,CastInfo,System.Action`1<T>)' called when server was not active");
			return null;
		}
		return CreateAbilityInstance(DewResources.GetByType<T>(DewResources.GetSuggestedResourceLoadSettings(this, typeof(T))), position, rotation, info, beforePrepare);
	}

	[Server]
	public T CreateStatusEffect<T>(T prefab, Entity victim, CastInfo info, Action<T> beforePrepare = null) where T : StatusEffect
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'T Actor::CreateStatusEffect(T,Entity,CastInfo,System.Action`1<T>)' called when server was not active");
			return null;
		}
		return Dew.CreateStatusEffect(prefab, victim, this, info, beforePrepare);
	}

	public T CreateStatusEffect<T>(Entity victim, CastInfo info, Action<T> beforePrepare = null) where T : StatusEffect
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning($"CreateStatusEffect<{typeof(T).Name}> on client (no-op): {((UnityEngine.Object)(object)this).name}\n{new StackTrace(1, fNeedFileInfo: false)}");
			return null;
		}
		return CreateStatusEffect(DewResources.GetByType<T>(DewResources.GetSuggestedResourceLoadSettings(this, typeof(T))), victim, info, beforePrepare);
	}

	[Server]
	public StatusEffect CreateStatusEffect(Type type, Entity victim, CastInfo info, Action<StatusEffect> beforePrepare = null)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'StatusEffect Actor::CreateStatusEffect(System.Type,Entity,CastInfo,System.Action`1<StatusEffect>)' called when server was not active");
			return null;
		}
		return CreateStatusEffect(DewResources.GetByType<StatusEffect>(type, DewResources.GetSuggestedResourceLoadSettings(this, type)), victim, info, beforePrepare);
	}

	public Se_GenericShield_OneShot GiveShield(Entity victim, float amount, float duration, bool isDecay = false, ReactionChain chain = default(ReactionChain))
	{
		return CreateStatusEffect(victim, new CastInfo(victim), (Se_GenericShield_OneShot se) =>
		{
			se.chain = chain;
			se.initAmount = amount;
			se.SetTimer(duration);
			se.isDecay = isDecay;
		});
	}

	public T CreateActor<T>(Vector3 pos, Quaternion? rot, Action<T> beforePrepare = null) where T : Actor
	{
		return Dew.CreateActor(pos, rot, this, beforePrepare);
	}

	public T CreateActor<T>(T prefab, Vector3 pos, Quaternion? rot, Action<T> beforePrepare = null) where T : Actor
	{
		return Dew.CreateActor(prefab, pos, rot, this, beforePrepare);
	}

	public T SpawnEntity<T>(Vector3 pos, Quaternion? rot, DewPlayer owner, int level, Action<T> beforeSpawn = null) where T : Entity
	{
		return Dew.SpawnEntity(pos, rot, this, owner, level, beforeSpawn);
	}

	public T SpawnEntity<T>(T entity, Vector3 pos, Quaternion? rot, DewPlayer owner, int level, Action<T> beforeSpawn = null) where T : Entity
	{
		return Dew.SpawnEntity(entity, pos, rot, this, owner, level, beforeSpawn);
	}

	[Server]
	public void Teleport(Entity entity, Vector3 position)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'System.Void Actor::Teleport(Entity,UnityEngine.Vector3)' called when server was not active");
		}
		else
		{
			entity.Control.Teleport(position);
		}
	}

	public OnScreenTimerHandle ShowOnScreenTimerLocally(OnScreenTimerHandle handle)
	{
		if (handle.rawText == null && handle.rawTextGetter == null && !DewLocalization.TryGetUIValue(((object)this).GetType().Name + "_Name", out handle.rawText))
		{
			StarEffect starEffect = FindFirstOfType<StarEffect>();
			if ((UnityEngine.Object)(object)starEffect != null)
			{
				handle.rawText = DewLocalization.GetStarName(((object)starEffect).GetType().Name);
			}
			if (handle.rawText == null)
			{
				AbilityTrigger abilityTrigger = firstTrigger;
				if ((UnityEngine.Object)(object)abilityTrigger != null)
				{
					handle.rawText = DewLocalization.GetSkillName(DewLocalization.GetSkillKey(((object)abilityTrigger).GetType()), 0);
				}
			}
		}
		try
		{
			NetworkedManagerBase<ClientEventManager>.instance.OnShowOnScreenTimer?.Invoke(handle);
		}
		catch (Exception exception)
		{
			UnityEngine.Debug.LogException(exception);
		}
		return handle;
	}

	public void HideOnScreenTimerLocally(OnScreenTimerHandle handle)
	{
		try
		{
			NetworkedManagerBase<ClientEventManager>.instance.OnHideOnScreenTimer?.Invoke(handle);
		}
		catch (Exception exception)
		{
			UnityEngine.Debug.LogException(exception);
		}
	}

	[Server]
	public ElementalStatusEffect ApplyElemental(ElementalType type, Entity to, int appliedStacks = 1)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'ElementalStatusEffect Actor::ApplyElemental(ElementalType,Entity,System.Int32)' called when server was not active");
			return null;
		}
		Type seType = DewElementals.GetSeType(type);
		Entity entity = this as Entity;
		if ((UnityEngine.Object)(object)entity == null)
		{
			entity = firstEntity;
		}
		if (entity is Summon)
		{
			Entity entity2 = entity;
			while ((UnityEngine.Object)(object)entity2 != null && entity2 is Summon)
			{
				entity2 = entity2.FindFirstAncestorOfType<Entity>();
			}
			if ((UnityEngine.Object)(object)entity2 != null)
			{
				entity = entity2;
			}
		}
		float num = (((UnityEngine.Object)(object)entity != null) ? entity.Status.GetElementalAmp(type) : 0f);
		if (to.Status.TryGetStatusEffect(seType, out var effect))
		{
			ElementalStatusEffect elementalStatusEffect = (ElementalStatusEffect)effect;
			if (appliedStacks > 0)
			{
				elementalStatusEffect.AddStack(appliedStacks);
				InvokeOnApplyElemental(new EventInfoApplyElemental
				{
					actor = this,
					type = type,
					victim = to,
					addedStack = appliedStacks
				});
			}
			else
			{
				elementalStatusEffect.ResetDecayTimer();
			}
			if (elementalStatusEffect.ampAmount < num)
			{
				elementalStatusEffect.NetworkampAmount = num;
				elementalStatusEffect.parentActor = (((UnityEngine.Object)(object)entity != null) ? entity : NetworkedManagerBase<ActorManager>.instance.serverActor);
				elementalStatusEffect.info = new CastInfo(entity);
			}
			return elementalStatusEffect;
		}
		ElementalStatusEffect byType = DewResources.GetByType<ElementalStatusEffect>(seType, DewResources.GetSuggestedResourceLoadSettings(this, seType));
		if ((UnityEngine.Object)(object)entity != null)
		{
			_pendingElemental.Push((num, appliedStacks));
			try
			{
				return entity.CreateStatusEffect(byType, to, new CastInfo(entity), _applyPendingElemental);
			}
			finally
			{
				_pendingElemental.Pop();
			}
		}
		_pendingElemental.Push((num, appliedStacks));
		ElementalStatusEffect result;
		try
		{
			result = NetworkedManagerBase<ActorManager>.instance.serverActor.CreateStatusEffect(byType, to, new CastInfo(entity), _applyPendingElemental);
		}
		finally
		{
			_pendingElemental.Pop();
		}
		InvokeOnApplyElemental(new EventInfoApplyElemental
		{
			actor = this,
			type = type,
			victim = to,
			addedStack = appliedStacks
		});
		return result;
	}

	private static void ApplyPendingElemental(ElementalStatusEffect s)
	{
		(float, int) tuple = _pendingElemental.Peek();
		(s.NetworkampAmount, _) = tuple;
		s.SetStack(tuple.Item2);
	}

	public Se_GenericEffectContainer CreateBasicEffect(Entity victim, BasicEffect eff, float duration, string id = null, DuplicateEffectBehavior onDuplicate = DuplicateEffectBehavior.ReplacePrevious)
	{
		if ((UnityEngine.Object)(object)victim == null)
		{
			UnityEngine.Debug.LogWarning("Tried to create " + eff.GetType().Name + " with null victim");
			return null;
		}
		if (id != null && onDuplicate != DuplicateEffectBehavior.DoNothing)
		{
			List<StatusEffect> list = DewPool.GetList(out ListReturnHandle<StatusEffect> handle);
			try
			{
				list.AddRange(victim.Status.statusEffects);
				foreach (StatusEffect item in list)
				{
					if (item.IsNullOrInactive())
					{
						victim.Status.statusEffects.Remove(item);
						UnityEngine.Debug.LogWarning("Destroyed status effect was still in " + victim.GetActorReadableName() + "'s status effect list (pruned)");
					}
					else if (item is Se_GenericEffectContainer se_GenericEffectContainer && se_GenericEffectContainer._id == id)
					{
						switch (onDuplicate)
						{
						case DuplicateEffectBehavior.ReplacePrevious:
							item.Destroy();
							break;
						case DuplicateEffectBehavior.UsePrevious:
							item.SetTimer(duration, duration);
							return se_GenericEffectContainer;
						}
					}
				}
			}
			finally
			{
				handle.Return();
			}
		}
		_pendingBasicEffects.Push((eff, duration, id));
		try
		{
			return CreateStatusEffect(victim, new CastInfo(firstEntity), _applyPendingBasicEffect);
		}
		finally
		{
			_pendingBasicEffects.Pop();
		}
	}

	private static void ApplyPendingBasicEffect(Se_GenericEffectContainer se)
	{
		(BasicEffect, float, string) tuple = _pendingBasicEffects.Peek();
		se.effect = tuple.Item1;
		se.duration = tuple.Item2;
		se.Network_id = tuple.Item3;
		se.isKilledByCrowdControlImmunity = KilledByUnstoppableEffects.Contains(tuple.Item1.GetType());
		se.scaleDurationByTenacity = ScaleDurationByTenacityEffects.Contains(tuple.Item1.GetType());
	}

	private int GetLevel()
	{
		Actor actor = this;
		while ((UnityEngine.Object)(object)actor != null)
		{
			if (actor is AbilityInstance abilityInstance)
			{
				return abilityInstance.effectiveLevel;
			}
			if (actor is SkillTrigger skillTrigger)
			{
				return skillTrigger.level;
			}
			if (actor is Entity entity)
			{
				return entity.level;
			}
			actor = parentActor;
		}
		return 1;
	}

	private DewPlayer GetPlayer()
	{
		Actor actor = this;
		while ((UnityEngine.Object)(object)actor != null)
		{
			if (actor is AbilityInstance abilityInstance && (UnityEngine.Object)(object)abilityInstance.info.caster != null)
			{
				return abilityInstance.info.caster.owner;
			}
			if (actor is SkillTrigger skillTrigger && (UnityEngine.Object)(object)skillTrigger.owner != null)
			{
				return skillTrigger.owner.owner;
			}
			if (actor is Entity entity)
			{
				return entity.owner;
			}
			actor = parentActor;
		}
		return null;
	}

	private CastInfo GetCastInfo()
	{
		CastInfo result = default;
		Actor actor = this;
		while ((UnityEngine.Object)(object)actor != null)
		{
			if (actor is AbilityInstance abilityInstance)
			{
				return abilityInstance.info;
			}
			if (actor is SkillTrigger skillTrigger)
			{
				result.caster = skillTrigger.owner;
			}
			actor = actor.parentActor;
		}
		return result;
	}

	public T SpawnSummon<T>(Vector3 position, Quaternion? rotation, Action<T> beforePrepare = null) where T : Summon
	{
		T val = Dew.SpawnEntity(position, rotation, this, GetPlayer(), 1, (T ent) =>
		{
			ent.info = GetCastInfo();
			ent.skillLevel = GetLevel();
			try
			{
				beforePrepare?.Invoke(ent);
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		});
		InvokeOnSpawnSummon(new EventInfoSummon
		{
			actor = this,
			summon = val
		});
		return val;
	}

	[Server]
	public void ApplyCooldownReduction(AbilityTrigger trigger, float amount, bool scaled = true, bool ignoreCanReceiveCooldown = false)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'System.Void Actor::ApplyCooldownReduction(AbilityTrigger,System.Single,System.Boolean,System.Boolean)' called when server was not active");
			return;
		}
		ApplyCooldownReduction(trigger, new CooldownReductionSettings
		{
			amount = amount,
			isUnscaled = !scaled,
			ignoreCanReceiveCooldown = ignoreCanReceiveCooldown,
			minThreshold = 0f
		});
	}

	[Server]
	public void ApplyCooldownReductionByRatio(AbilityTrigger trigger, float ratio, bool ignoreCanReceiveCooldown = false)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'System.Void Actor::ApplyCooldownReductionByRatio(AbilityTrigger,System.Single,System.Boolean)' called when server was not active");
			return;
		}
		ApplyCooldownReductionByRatio(trigger, new CooldownReductionByRatioSettings
		{
			ratio = ratio,
			ignoreCanReceiveCooldown = ignoreCanReceiveCooldown
		});
	}

	[Server]
	public void ResetCooldown(AbilityTrigger trigger, bool ignoreCanReceiveCooldown = false)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'System.Void Actor::ResetCooldown(AbilityTrigger,System.Boolean)' called when server was not active");
			return;
		}
		ApplyCooldownReductionByRatio(trigger, new CooldownReductionByRatioSettings
		{
			ratio = 1f,
			ignoreCanReceiveCooldown = ignoreCanReceiveCooldown
		});
	}

	[Server]
	public void ApplyCooldownReductionByRatio(AbilityTrigger trigger, CooldownReductionByRatioSettings settings)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'System.Void Actor::ApplyCooldownReductionByRatio(AbilityTrigger,CooldownReductionByRatioSettings)' called when server was not active");
			return;
		}
		ProcessDealtCooldownReductionByRatio(ref settings, trigger);
		trigger.ProcessReceivedCooldownReductionByRatio(ref settings, trigger);
		trigger.ApplyCooldownReductionByRatio_Imp(settings.ratio, settings.ignoreCanReceiveCooldown);
	}

	[Server]
	public void ApplyCooldownReduction(AbilityTrigger trigger, CooldownReductionSettings settings)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'System.Void Actor::ApplyCooldownReduction(AbilityTrigger,CooldownReductionSettings)' called when server was not active");
			return;
		}
		ProcessDealtCooldownReduction(ref settings, trigger);
		trigger.ProcessReceivedCooldownReduction(ref settings, trigger);
		trigger.ApplyCooldownReductionWithMinimum_Imp(settings.amount, settings.minThreshold, !settings.isUnscaled, settings.ignoreCanReceiveCooldown);
	}

	[Server]
	public Se_GenericStealth ApplyStealth(Entity target, float maxDuration = float.PositiveInfinity, Action<Se_GenericStealth> beforePrepare = null)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'Se_GenericStealth Actor::ApplyStealth(Entity,System.Single,System.Action`1<Se_GenericStealth>)' called when server was not active");
			return null;
		}
		if (target.Status.TryGetStatusEffect<Se_GenericStealth>(out var effect))
		{
			effect.Destroy();
		}
		Se_GenericStealth se_GenericStealth = CreateStatusEffect(target, new CastInfo(target), beforePrepare);
		if (!float.IsPositiveInfinity(maxDuration))
		{
			se_GenericStealth.SetTimer(maxDuration);
			se_GenericStealth.ShowOnScreenTimer();
		}
		return se_GenericStealth;
	}

	[Server]
	public Se_GenericRevealed Reveal(Entity target, float revealDuration, Action<Se_GenericRevealed> beforePrepare = null)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'Se_GenericRevealed Actor::Reveal(Entity,System.Single,System.Action`1<Se_GenericRevealed>)' called when server was not active");
			return null;
		}
		if (!target.Status.hasInvisible)
		{
			return null;
		}
		if (target.Status.TryGetStatusEffect<Se_GenericRevealed>(out var effect))
		{
			beforePrepare?.Invoke(effect);
			if (effect.remainingDuration.HasValue && effect.remainingDuration.Value < revealDuration)
			{
				effect.SetTimer(revealDuration);
			}
			return effect;
		}
		Se_GenericRevealed se_GenericRevealed = CreateStatusEffect(target, new CastInfo(target), beforePrepare);
		if (!float.IsPositiveInfinity(revealDuration))
		{
			se_GenericRevealed.SetTimer(revealDuration);
		}
		return se_GenericRevealed;
	}

	public void CustomRpc_RegisterClientMessageHandler<T>(Action<T> handler)
	{
		if (!_clientRpcHandlers.TryGetValue(typeof(T), out var value))
		{
			value = new List<RpcHandler>();
			_clientRpcHandlers[typeof(T)] = value;
		}
		value.Add(new RpcHandler
		{
			wrappedAction = (object obj) =>
			{
				if (obj is T obj2)
				{
					handler(obj2);
				}
			},
			originalAction = handler
		});
	}

	public void CustomRpc_ClearClientMessageHandlers()
	{
		_clientRpcHandlers.Clear();
	}

	public void CustomRpc_UnregisterClientMessageHandler<T>()
	{
		_clientRpcHandlers.Remove(typeof(T));
	}

	public void CustomRpc_UnregisterClientMessageHandler<T>(Action<T> handler)
	{
		if (_clientRpcHandlers.TryGetValue(typeof(T), out var value))
		{
			value.FilterInPlace((RpcHandler h) => !h.originalAction.Equals(handler));
		}
	}

	public void CustomRpc_RegisterServerMessageHandler<T>(Action<T> handler)
	{
		if (!_serverRpcHandlers.TryGetValue(typeof(T), out var value))
		{
			value = new List<RpcHandler>();
			_serverRpcHandlers[typeof(T)] = value;
		}
		value.Add(new RpcHandler
		{
			wrappedAction = (object obj) =>
			{
				if (obj is T obj2)
				{
					handler(obj2);
				}
			},
			originalAction = handler
		});
	}

	public void CustomRpc_RegisterServerMessageHandler<T>(string type, Action<T, DewPlayer> handler)
	{
		if (!_serverRpcHandlers.TryGetValue(typeof(T), out var value))
		{
			value = new List<RpcHandler>();
			_serverRpcHandlers[typeof(T)] = value;
		}
		value.Add(new RpcHandler
		{
			wrappedAction = (object obj) =>
			{
				if (!((UnityEngine.Object)(object)_serverRpcCaller == null) && obj is T arg)
				{
					handler(arg, _serverRpcCaller);
				}
			},
			originalAction = handler
		});
	}

	public void CustomRpc_ClearServerMessageHandlers()
	{
		_clientRpcHandlers.Clear();
	}

	public void CustomRpc_UnregisterServerMessageHandler<T>()
	{
		_clientRpcHandlers.Remove(typeof(T));
	}

	public void CustomRpc_UnregisterServerMessageHandler<T>(Action<T> handler)
	{
		if (_serverRpcHandlers.TryGetValue(typeof(T), out var value))
		{
			value.FilterInPlace((RpcHandler h) => !h.originalAction.Equals(handler));
		}
	}

	public void CustomRpc_UnregisterServerMessageHandler<T>(Action<T, DewPlayer> handler)
	{
		if (_serverRpcHandlers.TryGetValue(typeof(T), out var value))
		{
			value.FilterInPlace((RpcHandler h) => !h.originalAction.Equals(handler));
		}
	}

	public void CustomRpc_SendMessageToServer<T>(T msg)
	{
		if (!NetworkClient.active)
		{
			UnityEngine.Debug.LogWarning("Tried to call server RPC '" + typeof(T).Name + "' in non-networked context");
			return;
		}
		string msg2 = DewPersistence.ToJson(msg);
		CmdHandleRpc_Imp(typeof(T).Name, msg2);
	}

	public void CustomRpc_SendMessageToClient<T>(DewPlayer target, T msg)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("Tried to call client RPC '" + typeof(T).Name + "' in non-server context");
			return;
		}
		string parameter = DewPersistence.ToJson(msg);
		TpcHandleRpc_Imp(target, typeof(T).Name, parameter);
	}

	public void CustomRpc_SendMessageToAllClients<T>(T msg)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("Tried to call all clients RPC '" + typeof(T).Name + "' in non-server context");
			return;
		}
		string msg2 = DewPersistence.ToJson(msg);
		RpcHandleRpc_Imp(typeof(T).Name, msg2);
	}

	[Command(requiresAuthority = false)]
	private void CmdHandleRpc_Imp(string type, string msg, NetworkConnectionToClient conn = null)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteString((NetworkWriter)(object)val, type);
		NetworkWriterExtensions.WriteString((NetworkWriter)(object)val, msg);
		((NetworkBehaviour)this).SendCommandInternal("System.Void Actor::CmdHandleRpc_Imp(System.String,System.String,Mirror.NetworkConnectionToClient)", -403732121, (NetworkWriter)(object)val, 0, false);
		NetworkWriterPool.Return(val);
	}

	[ClientRpc]
	private void RpcHandleRpc_Imp(string type, string msg)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteString((NetworkWriter)(object)val, type);
		NetworkWriterExtensions.WriteString((NetworkWriter)(object)val, msg);
		((NetworkBehaviour)this).SendRPCInternal("System.Void Actor::RpcHandleRpc_Imp(System.String,System.String)", 938120551, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[TargetRpc]
	private void TpcHandleRpc_Imp(NetworkConnectionToClient target, string type, string parameter)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteString((NetworkWriter)(object)val, type);
		NetworkWriterExtensions.WriteString((NetworkWriter)(object)val, parameter);
		((NetworkBehaviour)this).SendTargetRPCInternal((NetworkConnection)(object)target, "System.Void Actor::TpcHandleRpc_Imp(Mirror.NetworkConnectionToClient,System.String,System.String)", -1953415030, (NetworkWriter)(object)val, 0);
		NetworkWriterPool.Return(val);
	}

	private void HandleRpc_Imp(string type, string serialized, DewPlayer caller)
	{
		_serverRpcCaller = caller;
		foreach (KeyValuePair<Type, List<RpcHandler>> item in ((UnityEngine.Object)(object)caller != null) ? _serverRpcHandlers : _clientRpcHandlers)
		{
			object obj;
			try
			{
				if (item.Key.Name != type)
				{
					continue;
				}
				obj = DewPersistence.FromJson(serialized, item.Key);
				goto IL_0058;
			}
			catch
			{
			}
			continue;
			IL_0058:
			foreach (RpcHandler item2 in item.Value)
			{
				try
				{
					item2.wrappedAction(obj);
				}
				catch (Exception exception)
				{
					UnityEngine.Debug.LogException(exception);
				}
			}
		}
	}

	public void OnCreate_ActorData()
	{
		((SyncIDictionary<string, string>)(object)persistentSyncedData).Callback += PersistentSyncedDataCallback;
		foreach (KeyValuePair<string, string> persistentSyncedDatum in persistentSyncedData)
		{
			_persistentSyncedDataKnownKeys.Add(persistentSyncedDatum.Key);
			PersistentSyncedDataCallback((Operation<string, string>)0, persistentSyncedDatum.Key, null);
		}
		void PersistentSyncedDataCallback(Operation<string, string> op, string key, string item)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Expected I4, but got Unknown
			//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
			switch ((int)op)
			{
			case 0:
			case 3:
				_persistentSyncedDataKnownKeys.Add(key);
				ClientEvent_OnPersistentSyncedDataChanged?.Invoke(key);
				break;
			case 1:
				foreach (string persistentSyncedDataKnownKey in _persistentSyncedDataKnownKeys)
				{
					ClientEvent_OnPersistentSyncedDataChanged?.Invoke(persistentSyncedDataKnownKey);
				}
				_persistentSyncedDataKnownKeys.Clear();
				break;
			case 2:
				_persistentSyncedDataKnownKeys.Remove(key);
				ClientEvent_OnPersistentSyncedDataChanged?.Invoke(key);
				break;
			default:
				throw new ArgumentOutOfRangeException("op", op, null);
			}
		}
	}

	public bool HasData<T>()
	{
		return _dataByType.ContainsKey(typeof(T));
	}

	public bool RemoveData<T>()
	{
		object item = default;
		if (!_dataByType.Remove(typeof(T), ref item))
		{
			return false;
		}
		_data.Remove(item);
		object obj = _data.Find((object o) => o.GetType() == typeof(T));
		if (obj != null)
		{
			_dataByType.Add(typeof(T), obj);
		}
		return true;
	}

	public bool RemoveData(object obj)
	{
		Type type = obj.GetType();
		if (!_dataByType.Remove(type))
		{
			return false;
		}
		_data.Remove(obj);
		object obj2 = _data.Find((object o) => o.GetType() == type);
		if (obj2 != null)
		{
			_dataByType.Add(type, obj2);
		}
		return true;
	}

	public void AddData<T>(T data)
	{
		if (!_dataByType.ContainsKey(typeof(T)))
		{
			_dataByType.Add(typeof(T), data);
		}
		_data.Add(data);
	}

	public bool TryGetData<T>(out T data)
	{
		bool flag = _dataByType.TryGetValue(typeof(T), out var value);
		if (flag)
		{
			data = (T)value;
			return flag;
		}
		data = default;
		return flag;
	}

	public T GetData<T>()
	{
		if (_dataByType.ContainsKey(typeof(T)))
		{
			return (T)_dataByType[typeof(T)];
		}
		return default;
	}

	public T FindData<T>(Predicate<T> predicate)
	{
		if (!_dataByType.TryGetValue(typeof(T), out var value))
		{
			return default;
		}
		if (predicate((T)value))
		{
			return (T)value;
		}
		object obj = _data.Find((object o) => o is T obj2 && predicate(obj2));
		if (obj == null)
		{
			return default;
		}
		return (T)obj;
	}

	public bool TryFindData<T>(Predicate<T> predicate, out T result)
	{
		if (!_dataByType.TryGetValue(typeof(T), out var value))
		{
			result = default;
			return false;
		}
		if (predicate((T)value))
		{
			result = (T)value;
			return true;
		}
		object obj = _data.Find((object o) => o is T obj2 && predicate(obj2));
		if (obj == null)
		{
			result = default;
			return false;
		}
		result = (T)obj;
		return true;
	}

	public void InvokeOnAbilityInstanceCreated(EventInfoAbilityInstance info)
	{
		Actor actor = this;
		while ((bool)(UnityEngine.Object)(object)actor)
		{
			actor.ActorEvent_OnAbilityInstanceCreated?.Invoke(info);
			actor = actor.parentActor;
		}
	}

	public void InvokeOnAbilityInstanceBeforePrepare(EventInfoAbilityInstance info)
	{
		Actor actor = this;
		while ((bool)(UnityEngine.Object)(object)actor)
		{
			actor.ActorEvent_OnAbilityInstanceBeforePrepare?.Invoke(info);
			actor = actor.parentActor;
		}
	}

	public void InvokeOnDealDamage(EventInfoDamage info)
	{
		Actor actor = this;
		while ((bool)(UnityEngine.Object)(object)actor)
		{
			actor.ActorEvent_OnDealDamage?.Invoke(info);
			actor = actor.parentActor;
		}
	}

	public void InvokeOnDoHeal(EventInfoHeal info)
	{
		Actor actor = this;
		while ((bool)(UnityEngine.Object)(object)actor)
		{
			actor.ActorEvent_OnDoHeal?.Invoke(info);
			actor = actor.parentActor;
		}
	}

	public void InvokeOnDoManaHeal(EventInfoHeal info)
	{
		Actor actor = this;
		while ((bool)(UnityEngine.Object)(object)actor)
		{
			actor.ActorEvent_OnDoManaHeal?.Invoke(info);
			actor = actor.parentActor;
		}
	}

	public void InvokeOnSpendMana(EventInfoSpentMana info)
	{
		Actor actor = this;
		while ((bool)(UnityEngine.Object)(object)actor)
		{
			actor.ActorEvent_OnSpendMana?.Invoke(info);
			actor = actor.parentActor;
		}
	}

	public void InvokeOnKill(EventInfoKill info)
	{
		Actor actor = this;
		while ((bool)(UnityEngine.Object)(object)actor)
		{
			actor.ActorEvent_OnKill?.Invoke(info);
			actor = actor.parentActor;
		}
	}

	public void InvokeOnAttackHit(EventInfoAttackHit info)
	{
		Actor actor = this;
		while ((bool)(UnityEngine.Object)(object)actor)
		{
			actor.ActorEvent_OnAttackHit?.Invoke(info);
			actor = actor.parentActor;
		}
	}

	public void InvokeOnAttackEffectTriggered(EventInfoAttackEffect info)
	{
		Actor actor = this;
		while ((bool)(UnityEngine.Object)(object)actor)
		{
			actor.ActorEvent_OnAttackEffectTriggered?.Invoke(info);
			actor = actor.parentActor;
		}
	}

	public void InvokeOnApplyElemental(EventInfoApplyElemental info)
	{
		Actor actor = this;
		while ((bool)(UnityEngine.Object)(object)actor)
		{
			actor.ActorEvent_OnApplyElemental?.Invoke(info);
			actor = actor.parentActor;
		}
	}

	public void InvokeOnGiveShield(EventInfoShield info)
	{
		Actor actor = this;
		while ((bool)(UnityEngine.Object)(object)actor)
		{
			actor.ActorEvent_OnGiveShield?.Invoke(info);
			actor = actor.parentActor;
		}
	}

	public void InvokeOnSpawnSummon(EventInfoSummon info)
	{
		Actor actor = this;
		while ((bool)(UnityEngine.Object)(object)actor)
		{
			actor.ActorEvent_OnSpawnSummon?.Invoke(info);
			actor = actor.parentActor;
		}
	}

	public void InvokeOnCreate(Actor info)
	{
		Actor actor = this;
		while ((bool)(UnityEngine.Object)(object)actor)
		{
			actor.ClientActorEvent_OnCreate?.Invoke(info);
			actor = actor.parentActor;
		}
	}

	public bool IsDescendantOf(Actor actor)
	{
		Actor actor2 = parentActor;
		int num = 1;
		while ((UnityEngine.Object)(object)actor2 != null)
		{
			if (num >= 100)
			{
				throw new Exception("Ancestor depth reached upper limit, suspecting cyclic parenting and exiting enumeration.");
			}
			if ((UnityEngine.Object)(object)actor2 == (UnityEngine.Object)(object)actor)
			{
				return true;
			}
			actor2 = actor2.parentActor;
			num++;
		}
		return false;
	}

	public bool IsDescendantOf<T>() where T : Actor
	{
		Actor actor = parentActor;
		int num = 1;
		while ((UnityEngine.Object)(object)actor != null)
		{
			if (num >= 100)
			{
				throw new Exception("Ancestor depth reached upper limit, suspecting cyclic parenting and exiting enumeration.");
			}
			if (actor is T)
			{
				return true;
			}
			actor = actor.parentActor;
			num++;
		}
		return false;
	}

	public T FindFirstAncestorOfType<T>() where T : Actor
	{
		Actor actor = parentActor;
		int num = 1;
		while ((UnityEngine.Object)(object)actor != null)
		{
			if (num >= 100)
			{
				throw new Exception("Ancestor depth reached upper limit, suspecting cyclic parenting and exiting enumeration.");
			}
			if (actor is T result)
			{
				return result;
			}
			actor = actor.parentActor;
			num++;
		}
		return null;
	}

	public T FindFirstOfType<T>() where T : Actor
	{
		Actor actor = this;
		int num = 1;
		while ((UnityEngine.Object)(object)actor != null)
		{
			if (num >= 100)
			{
				throw new Exception("Ancestor depth reached upper limit, suspecting cyclic parenting and exiting enumeration.");
			}
			if (actor is T result)
			{
				return result;
			}
			actor = actor.parentActor;
			num++;
		}
		return null;
	}

	public void Destroy()
	{
		if ((UnityEngine.Object)(object)this == null)
		{
			UnityEngine.Debug.LogWarning("Tried to destroy null actor");
		}
		else if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning($"Actor.Destroy() on client (no-op): {((UnityEngine.Object)(object)this).name}\n{new StackTrace(1, fNeedFileInfo: false)}");
		}
		else
		{
			Dew.Destroy(((Component)(object)this).gameObject);
		}
	}

	[Server]
	public void DestroyIfActive()
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'System.Void Actor::DestroyIfActive()' called when server was not active");
		}
		else if (!((UnityEngine.Object)(object)this == null) && isActive)
		{
			Dew.Destroy(((Component)(object)this).gameObject);
		}
	}

	[Server]
	public void LockDestroy()
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'System.Void Actor::LockDestroy()' called when server was not active");
			return;
		}
		checked
		{
			_destroyLock++;
			_destroyLockUpdateTime = Time.time;
		}
	}

	[Server]
	public void UnlockDestroy()
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'System.Void Actor::UnlockDestroy()' called when server was not active");
			return;
		}
		checked
		{
			_destroyLock--;
			_destroyLockUpdateTime = Time.time;
		}
	}

	[Server]
	public void LockDestroyFor(float seconds)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'System.Void Actor::LockDestroyFor(System.Single)' called when server was not active");
			return;
		}
		LockDestroy();
		ActorRef<Actor> selfRef = this;
		Dew.GetCoroutiner().StartCoroutine(Routine());
		IEnumerator Routine()
		{
			yield return new WaitForSeconds(seconds);
			Actor actor = selfRef.Get();
			if ((UnityEngine.Object)(object)actor != null)
			{
				actor.UnlockDestroy();
			}
		}
	}

	public bool IsExcludedFromRoomSave()
	{
		return HasData<Ad_ExcludedFromRoomSave>();
	}

	[Server]
	public void ExcludeFromRoomSave()
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'System.Void Actor::ExcludeFromRoomSave()' called when server was not active");
		}
		else if (!HasData<Ad_ExcludedFromRoomSave>())
		{
			AddData(new Ad_ExcludedFromRoomSave());
		}
	}

	[Server]
	public void ClearExcludeFromRoomSave()
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'System.Void Actor::ClearExcludeFromRoomSave()' called when server was not active");
		}
		else if (HasData<Ad_ExcludedFromRoomSave>())
		{
			RemoveData<Ad_ExcludedFromRoomSave>();
		}
	}

	private void ProcessDealtDamage(ref DamageData data, Entity target)
	{
		List<DataProcessorGroup<DamageData, Actor, Entity>> list = DewPool.GetList(out ListReturnHandle<DataProcessorGroup<DamageData, Actor, Entity>> handle);
		if (dealtDamageProcessor.count > 0)
		{
			list.Add(dealtDamageProcessor);
		}
		foreach (Actor ancestor in ancestors)
		{
			if (ancestor.dealtDamageProcessor.count > 0)
			{
				list.Add(ancestor.dealtDamageProcessor);
			}
		}
		DataProcessorGroup<DamageData, Actor, Entity>.ProcessMerged(list, ref data, this, target);
		handle.Return();
	}

	private void ProcessDealtHeal(ref HealData data, Entity target)
	{
		List<DataProcessorGroup<HealData, Actor, Entity>> list = DewPool.GetList(out ListReturnHandle<DataProcessorGroup<HealData, Actor, Entity>> handle);
		if (dealtHealProcessor.count > 0)
		{
			list.Add(dealtHealProcessor);
		}
		foreach (Actor ancestor in ancestors)
		{
			if (ancestor.dealtHealProcessor.count > 0)
			{
				list.Add(ancestor.dealtHealProcessor);
			}
		}
		DataProcessorGroup<HealData, Actor, Entity>.ProcessMerged(list, ref data, this, target);
		handle.Return();
	}

	private void ProcessDealtManaHeal(ref HealData data, Entity target)
	{
		List<DataProcessorGroup<HealData, Actor, Entity>> list = DewPool.GetList(out ListReturnHandle<DataProcessorGroup<HealData, Actor, Entity>> handle);
		if (dealtManaHealProcessor.count > 0)
		{
			list.Add(dealtManaHealProcessor);
		}
		foreach (Actor ancestor in ancestors)
		{
			if (ancestor.dealtManaHealProcessor.count > 0)
			{
				list.Add(ancestor.dealtManaHealProcessor);
			}
		}
		DataProcessorGroup<HealData, Actor, Entity>.ProcessMerged(list, ref data, this, target);
		handle.Return();
	}

	private void ProcessDealtShield(ref HealData data, Entity target)
	{
		List<DataProcessorGroup<HealData, Actor, Entity>> list = DewPool.GetList(out ListReturnHandle<DataProcessorGroup<HealData, Actor, Entity>> handle);
		if (dealtShieldProcessor.count > 0)
		{
			list.Add(dealtShieldProcessor);
		}
		foreach (Actor ancestor in ancestors)
		{
			if (ancestor.dealtShieldProcessor.count > 0)
			{
				list.Add(ancestor.dealtShieldProcessor);
			}
		}
		DataProcessorGroup<HealData, Actor, Entity>.ProcessMerged(list, ref data, this, target);
		handle.Return();
	}

	private void ProcessDealtCooldownReduction(ref CooldownReductionSettings data, AbilityTrigger target)
	{
		List<DataProcessorGroup<CooldownReductionSettings, Actor, AbilityTrigger>> list = DewPool.GetList(out ListReturnHandle<DataProcessorGroup<CooldownReductionSettings, Actor, AbilityTrigger>> handle);
		if (dealtCooldownReductionProcessor.count > 0)
		{
			list.Add(dealtCooldownReductionProcessor);
		}
		foreach (Actor ancestor in ancestors)
		{
			if (ancestor.dealtCooldownReductionProcessor.count > 0)
			{
				list.Add(ancestor.dealtCooldownReductionProcessor);
			}
		}
		DataProcessorGroup<CooldownReductionSettings, Actor, AbilityTrigger>.ProcessMerged(list, ref data, this, target);
		handle.Return();
	}

	private void ProcessDealtCooldownReductionByRatio(ref CooldownReductionByRatioSettings data, AbilityTrigger target)
	{
		List<DataProcessorGroup<CooldownReductionByRatioSettings, Actor, AbilityTrigger>> list = DewPool.GetList(out ListReturnHandle<DataProcessorGroup<CooldownReductionByRatioSettings, Actor, AbilityTrigger>> handle);
		if (dealtCooldownReductionByRatioProcessor.count > 0)
		{
			list.Add(dealtCooldownReductionByRatioProcessor);
		}
		foreach (Actor ancestor in ancestors)
		{
			if (ancestor.dealtCooldownReductionByRatioProcessor.count > 0)
			{
				list.Add(ancestor.dealtCooldownReductionByRatioProcessor);
			}
		}
		DataProcessorGroup<CooldownReductionByRatioSettings, Actor, AbilityTrigger>.ProcessMerged(list, ref data, this, target);
		handle.Return();
	}

	public float ProcessShieldAmount(float amount, Entity target)
	{
		HealData data = new HealData(amount);
		ProcessDealtShield(ref data, target);
		target.ProcessReceivedShield(ref data, this);
		return new FinalHealData(data, float.PositiveInfinity).amount;
	}

	public void ProcessSpawnedChildVarDefProcessor(ref VariantDef data, Type childType)
	{
		if (childType == null)
		{
			return;
		}
		List<DataProcessorGroup<VariantDef, Actor, Type>> list = DewPool.GetList(out ListReturnHandle<DataProcessorGroup<VariantDef, Actor, Type>> handle);
		if (spawnedChildVarDefProcessor.count > 0)
		{
			list.Add(spawnedChildVarDefProcessor);
		}
		foreach (Actor ancestor in ancestors)
		{
			if (ancestor.spawnedChildVarDefProcessor.count > 0)
			{
				list.Add(ancestor.spawnedChildVarDefProcessor);
			}
		}
		DataProcessorGroup<VariantDef, Actor, Type>.ProcessMerged(list, ref data, this, childType);
		handle.Return();
	}

	[Server]
	public KillTracker TrackKills(float gracePeriod, Action<EventInfoKill> callback)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'KillTracker Actor::TrackKills(System.Single,System.Action`1<EventInfoKill>)' called when server was not active");
			return null;
		}
		return new KillTracker(this, gracePeriod, callback);
	}

	public Actor()
	{
		((NetworkBehaviour)this).InitSyncObject((SyncObject)(object)persistentSyncedData);
		_Mirror_SyncVarHookDelegate__parentActor = OnParentActorChanged;
		_Mirror_SyncVarHookDelegate__isActive = OnIsActiveChanged;
	}

	static Actor()
	{
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Expected Obj, but got Unknown
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Expected Obj, but got Unknown
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Expected Obj, but got Unknown
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Expected Obj, but got Unknown
		_pendingElemental = new Stack<(float, int)>();
		_applyPendingElemental = ApplyPendingElemental;
		KilledByUnstoppableEffects = new Type[6]
		{
			typeof(SlowEffect),
			typeof(CrippleEffect),
			typeof(RootEffect),
			typeof(SilenceEffect),
			typeof(StunEffect),
			typeof(BlindEffect)
		};
		ScaleDurationByTenacityEffects = new Type[4]
		{
			typeof(RootEffect),
			typeof(SilenceEffect),
			typeof(StunEffect),
			typeof(BlindEffect)
		};
		_pendingBasicEffects = new Stack<(BasicEffect, float, string)>();
		_applyPendingBasicEffect = ApplyPendingBasicEffect;
		RemoteProcedureCalls.RegisterCommand(typeof(Actor), "System.Void Actor::CmdHandleRpc_Imp(System.String,System.String,Mirror.NetworkConnectionToClient)", (RemoteCallDelegate)InvokeUserCode_CmdHandleRpc_Imp__String__String__NetworkConnectionToClient, false);
		RemoteProcedureCalls.RegisterRpc(typeof(Actor), "System.Void Actor::RpcInvalidatePoolReuse()", (RemoteCallDelegate)InvokeUserCode_RpcInvalidatePoolReuse);
		RemoteProcedureCalls.RegisterRpc(typeof(Actor), "System.Void Actor::RpcHandleRpc_Imp(System.String,System.String)", (RemoteCallDelegate)InvokeUserCode_RpcHandleRpc_Imp__String__String);
		RemoteProcedureCalls.RegisterRpc(typeof(Actor), "System.Void Actor::TpcHandleRpc_Imp(Mirror.NetworkConnectionToClient,System.String,System.String)", (RemoteCallDelegate)InvokeUserCode_TpcHandleRpc_Imp__NetworkConnectionToClient__String__String);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcInvalidatePoolReuse()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			InvalidateInstance();
		}
	}

	protected static void InvokeUserCode_RpcInvalidatePoolReuse(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			UnityEngine.Debug.LogError("RPC RpcInvalidatePoolReuse called on server.");
		}
		else
		{
			((Actor)(object)obj).UserCode_RpcInvalidatePoolReuse();
		}
	}

	protected void UserCode_CmdHandleRpc_Imp__String__String__NetworkConnectionToClient(string type, string msg, NetworkConnectionToClient conn)
	{
		if (conn != null)
		{
			DewPlayer player = conn.GetPlayer();
			if (!((UnityEngine.Object)(object)player == null))
			{
				HandleRpc_Imp(type, msg, player);
			}
		}
	}

	protected static void InvokeUserCode_CmdHandleRpc_Imp__String__String__NetworkConnectionToClient(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogError("Command CmdHandleRpc_Imp called on client.");
		}
		else
		{
			((Actor)(object)obj).UserCode_CmdHandleRpc_Imp__String__String__NetworkConnectionToClient(NetworkReaderExtensions.ReadString(reader), NetworkReaderExtensions.ReadString(reader), senderConnection);
		}
	}

	protected void UserCode_RpcHandleRpc_Imp__String__String(string type, string msg)
	{
		HandleRpc_Imp(type, msg, null);
	}

	protected static void InvokeUserCode_RpcHandleRpc_Imp__String__String(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			UnityEngine.Debug.LogError("RPC RpcHandleRpc_Imp called on server.");
		}
		else
		{
			((Actor)(object)obj).UserCode_RpcHandleRpc_Imp__String__String(NetworkReaderExtensions.ReadString(reader), NetworkReaderExtensions.ReadString(reader));
		}
	}

	protected void UserCode_TpcHandleRpc_Imp__NetworkConnectionToClient__String__String(NetworkConnectionToClient target, string type, string parameter)
	{
		HandleRpc_Imp(type, parameter, null);
	}

	protected static void InvokeUserCode_TpcHandleRpc_Imp__NetworkConnectionToClient__String__String(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			UnityEngine.Debug.LogError("TargetRPC TpcHandleRpc_Imp called on server.");
		}
		else
		{
			((Actor)(object)obj).UserCode_TpcHandleRpc_Imp__NetworkConnectionToClient__String__String((NetworkConnectionToClient)(object)NetworkClient.connection, NetworkReaderExtensions.ReadString(reader), NetworkReaderExtensions.ReadString(reader));
		}
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		((NetworkBehaviour)this).SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteBool(writer, isNewInstance__BackingField);
			NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)Network_parentActor);
			NetworkWriterExtensions.WriteBool(writer, _isActive);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 1L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, isNewInstance__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 2L) != 0L)
		{
			NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)Network_parentActor);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 4L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, _isActive);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		((NetworkBehaviour)this).DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref isNewInstance__BackingField, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize_NetworkBehaviour<Actor>(ref _parentActor, _Mirror_SyncVarHookDelegate__parentActor, reader, ref ____parentActorNetId);
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref _isActive, _Mirror_SyncVarHookDelegate__isActive, NetworkReaderExtensions.ReadBool(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 1L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref isNewInstance__BackingField, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
		}
		if ((num & 2L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize_NetworkBehaviour<Actor>(ref _parentActor, _Mirror_SyncVarHookDelegate__parentActor, reader, ref ____parentActorNetId);
		}
		if ((num & 4L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref _isActive, _Mirror_SyncVarHookDelegate__isActive, NetworkReaderExtensions.ReadBool(reader));
		}
	}
}
