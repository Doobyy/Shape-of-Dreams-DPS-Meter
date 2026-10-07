using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

[LogicUpdatePriority(-200)]
public abstract class AbilityInstance : Actor
{
	public class WatchedCondition
	{
		internal Func<bool> _condition;

		internal Action _action;

		public bool isWatched { get; internal set; }

		public void StopWatching()
		{
			isWatched = false;
		}
	}

	private class OngoingSequence
	{
		public enum StateType
		{
			Active = 0,
			Done = 1,
			WaitForTime = 2,
			WaitForNextUpdate = WaitForTime,
			WaitForCondition = 3
		}

		public StateType state;

		public Stack<IEnumerator> enumerators;

		public float remainingWaitTime;

		public Func<bool> condition;
	}

	[SyncVar]
	private CastInfo _info;

	[SyncVar]
	internal int _skillLevel = -1;

	public GameObject startEffectNoStop;

	public GameObject startEffect;

	public GameObject endEffect;

	public int budgetCost;

	[NonSerialized]
	public ReactionChain chain;

	[SyncVar]
	private Gem _gem;

	protected AbilityTargetValidatorWrapper tvDefaultHarmfulEffectTargets;

	protected AbilityTargetValidatorWrapper tvDefaultUsefulEffectTargets;

	protected AbilityTargetValidatorWrapper tvDefaultAllExceptSelf;

	private List<OngoingSequence> _sequences = new List<OngoingSequence>();

	private static readonly Stack<OngoingSequence> _sequencePool = new Stack<OngoingSequence>();

	private static readonly Dictionary<Type, bool> _hasCreateSeqOverride = new Dictionary<Type, bool>();

	private List<WatchedCondition> _watchedConditions = new List<WatchedCondition>();

	private Action<EventInfoKill> _cachedDestroyWatchKill;

	private Action<Actor> _cachedDestroyWatchActor;

	private static readonly object _siDoImmediately = default(SI.DoImmediately);

	protected NetworkBehaviourSyncVar ____gemNetId;

	[SaveVar(SaveVarFlags.Default)]
	public CastInfo info
	{
		get
		{
			return _info;
		}
		set
		{
			Network_info = value;
		}
	}

	[SaveVar(SaveVarFlags.Default)]
	public int skillLevel
	{
		get
		{
			if (ScalingValue.levelOverride.HasValue)
			{
				return ScalingValue.levelOverride.Value;
			}
			if (NetworkServer.active)
			{
				UpdateLevelIfNecessary();
			}
			return _skillLevel;
		}
		set
		{
			Network_skillLevel = value;
		}
	}

	public Gem gem
	{
		get
		{
			if ((UnityEngine.Object)(object)Network_gem == null)
			{
				Network_gem = FindFirstOfType<Gem>();
			}
			return Network_gem;
		}
		set
		{
			Network_gem = value;
		}
	}

	public bool hasOngoingSequences => _sequences.Count > 0;

	public int effectiveLevel => skillLevel;

	public Entity statEntity => info.caster;

	public CastInfo Network_info
	{
		get
		{
			return _info;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<CastInfo>(value, ref _info, 8uL, (Action<CastInfo, CastInfo>)null);
		}
	}

	public int Network_skillLevel
	{
		get
		{
			return _skillLevel;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<int>(value, ref _skillLevel, 16uL, (Action<int, int>)null);
		}
	}

	public Gem Network_gem
	{
		get
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			return ((NetworkBehaviour)this).GetSyncVarNetworkBehaviour<Gem>(____gemNetId, ref _gem);
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter_NetworkBehaviour<Gem>(value, ref _gem, 32uL, (Action<Gem, Gem>)null, ref ____gemNetId);
		}
	}

	protected override void OnPrepare()
	{
		Entity caster = info.caster;
		if (tvDefaultHarmfulEffectTargets == null)
		{
			tvDefaultHarmfulEffectTargets = new AbilityTargetValidatorWrapper(caster, EntityRelation.Neutral | EntityRelation.Enemy);
			tvDefaultUsefulEffectTargets = new AbilityTargetValidatorWrapper(caster, EntityRelation.Self | EntityRelation.Ally);
			tvDefaultAllExceptSelf = new AbilityTargetValidatorWrapper(caster, EntityRelation.Neutral | EntityRelation.Enemy | EntityRelation.Ally);
		}
		else
		{
			tvDefaultHarmfulEffectTargets.self = caster;
			tvDefaultUsefulEffectTargets.self = caster;
			tvDefaultAllExceptSelf.self = caster;
		}
		base.OnPrepare();
		UpdateLevelIfNecessary();
	}

	[Server]
	private void UpdateLevelIfNecessary()
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'System.Void AbilityInstance::UpdateLevelIfNecessary()' called when server was not active");
		}
		else if (_skillLevel == -1)
		{
			if (parentActor is AbilityInstance { skillLevel: not -1 } abilityInstance)
			{
				Network_skillLevel = abilityInstance.skillLevel;
			}
			else if (parentActor is SkillTrigger { level: not -1 } skillTrigger)
			{
				Network_skillLevel = skillTrigger.level;
			}
			else if (parentActor is Gem { effectiveLevel: not -1 } gem)
			{
				Network_skillLevel = gem.effectiveLevel;
			}
			else if (parentActor is Summon { skillLevel: not -1 } summon)
			{
				Network_skillLevel = summon.skillLevel;
			}
			else if (parentActor is AbilityTrigger { owner: Summon { skillLevel: not -1 } owner })
			{
				Network_skillLevel = owner.skillLevel;
			}
		}
	}

	public override void OnStart()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			Entity caster = info.caster;
			if (tvDefaultHarmfulEffectTargets == null)
			{
				tvDefaultHarmfulEffectTargets = new AbilityTargetValidatorWrapper(caster, EntityRelation.Neutral | EntityRelation.Enemy);
				tvDefaultUsefulEffectTargets = new AbilityTargetValidatorWrapper(caster, EntityRelation.Self | EntityRelation.Ally);
				tvDefaultAllExceptSelf = new AbilityTargetValidatorWrapper(caster, EntityRelation.Neutral | EntityRelation.Enemy | EntityRelation.Ally);
			}
			else
			{
				tvDefaultHarmfulEffectTargets.self = caster;
				tvDefaultUsefulEffectTargets.self = caster;
				tvDefaultAllExceptSelf.self = caster;
			}
		}
		base.OnStart();
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		FxPlay(startEffect, info.caster);
		FxPlay(startEffectNoStop, info.caster);
		Type type = ((object)this).GetType();
		if (!_hasCreateSeqOverride.TryGetValue(type, out var value))
		{
			value = type.GetMethod("OnCreateSequenced", BindingFlags.Instance | BindingFlags.NonPublic).DeclaringType != typeof(AbilityInstance);
			_hasCreateSeqOverride[type] = value;
		}
		if (value)
		{
			try
			{
				StartSequence(OnCreateSequenced());
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogError("Exception occured on " + ((Component)(object)this).transform.GetScenePath() + "::OnCreateSequenced");
				UnityEngine.Debug.LogException(exception, (UnityEngine.Object)(object)this);
			}
		}
	}

	public override void LogicUpdate(float dt)
	{
		base.LogicUpdate(dt);
		if (!isActive)
		{
			return;
		}
		for (int num = _watchedConditions.Count - 1; num >= 0; num--)
		{
			WatchedCondition watchedCondition = _watchedConditions[num];
			if (!watchedCondition.isWatched)
			{
				_watchedConditions.RemoveAt(num);
			}
			else if (watchedCondition._condition())
			{
				watchedCondition._action?.Invoke();
				_watchedConditions.RemoveAt(num);
			}
		}
		if (!isActive)
		{
			return;
		}
		for (int num2 = _sequences.Count - 1; num2 >= 0; num2--)
		{
			OngoingSequence ongoingSequence = _sequences[num2];
			HandleSequence(ongoingSequence);
			if (ongoingSequence.state == OngoingSequence.StateType.Done)
			{
				_sequences.RemoveAt(num2);
				ReturnSequence(ongoingSequence);
			}
		}
	}

	private void HandleSequence(OngoingSequence seq)
	{
		IEnumerator enumerator;
		while (true)
		{
			if (seq.state == OngoingSequence.StateType.Done)
			{
				return;
			}
			if (seq.enumerators.Count <= 0)
			{
				seq.state = OngoingSequence.StateType.Done;
				return;
			}
			if (seq.state == OngoingSequence.StateType.WaitForCondition)
			{
				if (!((SI.WaitForCondition)seq.enumerators.Peek().Current)._condition())
				{
					return;
				}
				seq.state = OngoingSequence.StateType.Active;
			}
			if (seq.state == OngoingSequence.StateType.WaitForTime)
			{
				if (seq.remainingWaitTime > 0f)
				{
					seq.remainingWaitTime -= LogicUpdateManager.logicDeltaTime;
					if (seq.remainingWaitTime > 0f)
					{
						return;
					}
				}
				seq.state = OngoingSequence.StateType.Active;
			}
			if (seq.state == OngoingSequence.StateType.WaitForTime)
			{
				seq.state = OngoingSequence.StateType.Active;
			}
			enumerator = seq.enumerators.Peek();
			try
			{
				if (!enumerator.MoveNext())
				{
					seq.enumerators.Pop();
					continue;
				}
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogError("Exception occured while in sequence of " + GetActorReadableName());
				UnityEngine.Debug.LogException(exception, (UnityEngine.Object)(object)this);
				seq.state = OngoingSequence.StateType.Done;
				return;
			}
			if (!(enumerator.Current is SI.DoImmediately))
			{
				if (enumerator.Current is SI.WaitForSeconds waitForSeconds)
				{
					seq.state = OngoingSequence.StateType.WaitForTime;
					seq.remainingWaitTime = waitForSeconds._seconds;
					return;
				}
				object current = enumerator.Current;
				if (current is SI.WaitForCondition)
				{
					_ = (SI.WaitForCondition)current;
					seq.state = OngoingSequence.StateType.WaitForCondition;
					return;
				}
				if (enumerator.Current == null)
				{
					seq.state = OngoingSequence.StateType.WaitForTime;
					return;
				}
				if (!(enumerator.Current is IEnumerator item))
				{
					break;
				}
				seq.enumerators.Push(item);
			}
		}
		UnityEngine.Debug.LogError($"Sequence returned value out of range: {enumerator} returned {enumerator.Current}", (UnityEngine.Object)(object)this);
		seq.state = OngoingSequence.StateType.Done;
	}

	[Server]
	public WatchedCondition DestroyOnCondition(Func<bool> condition)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'AbilityInstance/WatchedCondition AbilityInstance::DestroyOnCondition(System.Func`1<System.Boolean>)' called when server was not active");
			return null;
		}
		WatchedCondition watchedCondition = new WatchedCondition
		{
			_condition = condition.Invoke,
			_action = () =>
			{
				if (isActive)
				{
					Destroy();
				}
			}
		};
		watchedCondition.isWatched = true;
		_watchedConditions.Add(watchedCondition);
		return watchedCondition;
	}

	[Server]
	public void DestroyOnDeath(Entity entity, bool includeKnockOuts = false)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'System.Void AbilityInstance::DestroyOnDeath(Entity,System.Boolean)' called when server was not active");
			return;
		}
		if ((UnityEngine.Object)(object)entity == null || entity.isDead || !entity.isActive || (includeKnockOuts && entity is Hero { isKnockedOut: not false }))
		{
			DestroyIfActive();
			return;
		}
		entity.EntityEvent_OnDeath += new Action<EventInfoKill>(DestroyWatchCallback);
		entity.ClientActorEvent_OnDestroyed += new Action<Actor>(DestroyWatchCallback);
		if (includeKnockOuts && entity is Hero hero2)
		{
			hero2.ClientHeroEvent_OnKnockedOut += new Action<EventInfoKill>(DestroyWatchCallback);
		}
		ClientActorEvent_OnDestroyed += (Action<Actor>)((Actor _) =>
		{
			if (!((UnityEngine.Object)(object)entity == null))
			{
				entity.EntityEvent_OnDeath -= _cachedDestroyWatchKill;
				entity.ClientActorEvent_OnDestroyed -= _cachedDestroyWatchActor;
				if (includeKnockOuts && entity is Hero hero3)
				{
					hero3.ClientHeroEvent_OnKnockedOut -= _cachedDestroyWatchKill;
				}
			}
		});
	}

	public void DestroyOnDestroy(Actor actor)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning($"DestroyOnDestroy on client (no-op): {((UnityEngine.Object)(object)this).name}\n{new StackTrace(1, fNeedFileInfo: false)}");
			return;
		}
		if (actor.IsNullOrInactive())
		{
			DestroyIfActive();
			return;
		}
		actor.ClientActorEvent_OnDestroyed += new Action<Actor>(DestroyWatchCallback);
		ClientActorEvent_OnDestroyed += (Action<Actor>)((Actor _) =>
		{
			if (!((UnityEngine.Object)(object)actor == null))
			{
				actor.ClientActorEvent_OnDestroyed -= _cachedDestroyWatchActor;
			}
		});
	}

	private void DestroyWatchCallback(Actor obj)
	{
		if (isActive)
		{
			Destroy();
		}
	}

	private void DestroyWatchCallback(EventInfoKill obj)
	{
		if (isActive)
		{
			Destroy();
		}
	}

	protected void StartSequence(IEnumerator sequence)
	{
		if (!isActive)
		{
			sequence.MoveNext();
			return;
		}
		OngoingSequence ongoingSequence = RentSequence();
		ongoingSequence.enumerators.Push(sequence);
		_sequences.Add(ongoingSequence);
		HandleSequence(ongoingSequence);
	}

	private static OngoingSequence RentSequence()
	{
		if (_sequencePool.Count > 0)
		{
			OngoingSequence ongoingSequence = _sequencePool.Pop();
			ongoingSequence.state = OngoingSequence.StateType.Active;
			ongoingSequence.remainingWaitTime = 0f;
			ongoingSequence.condition = null;
			return ongoingSequence;
		}
		return new OngoingSequence
		{
			state = OngoingSequence.StateType.Active,
			enumerators = new Stack<IEnumerator>()
		};
	}

	private static void ReturnSequence(OngoingSequence seq)
	{
		while (seq.enumerators.Count > 0)
		{
			if (seq.enumerators.Pop() is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}
		seq.condition = null;
		_sequencePool.Push(seq);
	}

	protected virtual IEnumerator OnCreateSequenced()
	{
		yield return _siDoImmediately;
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (endEffect != null)
		{
			FxPlay(endEffect, info.caster);
		}
		for (int i = 0; i < _watchedConditions.Count; i++)
		{
			_watchedConditions[i].isWatched = false;
		}
		if (startEffect != null)
		{
			((MonoBehaviour)(object)this).StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			yield return null;
			yield return null;
			FxStop(startEffect);
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		for (int num = _sequences.Count - 1; num >= 0; num--)
		{
			ReturnSequence(_sequences[num]);
		}
		_sequences.Clear();
		Network_skillLevel = -1;
	}

	[Server]
	public T CreateStatusEffect<T>(T prefab, Entity victim, Action<T> beforePrepare = null) where T : StatusEffect
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'T AbilityInstance::CreateStatusEffect(T,Entity,System.Action`1<T>)' called when server was not active");
			return null;
		}
		return CreateStatusEffect(prefab, victim, new CastInfo(info.caster), beforePrepare);
	}

	[Server]
	public T CreateStatusEffect<T>(Entity victim, Action<T> beforePrepare = null) where T : StatusEffect
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'T AbilityInstance::CreateStatusEffect(Entity,System.Action`1<T>)' called when server was not active");
			return null;
		}
		return CreateStatusEffect(victim, new CastInfo(info.caster), beforePrepare);
	}

	public override string GetActorReadableName()
	{
		return base.GetActorReadableName() + " " + ((skillLevel > 0) ? $" ({skillLevel})" : "");
	}

	public void StartChargingChannel(ChargingChannel channel)
	{
		channel.Dispatch(info.caster, firstTrigger);
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

	public T GetValue<T>(T[] val)
	{
		return val.GetClamped(effectiveLevel);
	}

	public float GetValue(StarScalingValue value)
	{
		float strength = FindFirstOfType<StarEffect>().strength;
		return value.GetValue(effectiveLevel, strength);
	}

	public int GetValueInt(StarScalingValue value)
	{
		float strength = FindFirstOfType<StarEffect>().strength;
		return Mathf.RoundToInt(value.GetValue(effectiveLevel, strength));
	}

	protected bool Adaptive_ShouldPlayEffects()
	{
		if (!(info.caster is Hero))
		{
			return true;
		}
		return ManagerBase<GraphicsManager>.instance.perfPressureStrength < UnityEngine.Random.value;
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			GeneratedNetworkCode._Write_CastInfo(writer, _info);
			NetworkWriterExtensions.WriteInt(writer, _skillLevel);
			NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)Network_gem);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 8L) != 0L)
		{
			GeneratedNetworkCode._Write_CastInfo(writer, _info);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x10L) != 0L)
		{
			NetworkWriterExtensions.WriteInt(writer, _skillLevel);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x20L) != 0L)
		{
			NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)Network_gem);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<CastInfo>(ref _info, (Action<CastInfo, CastInfo>)null, GeneratedNetworkCode._Read_CastInfo(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref _skillLevel, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize_NetworkBehaviour<Gem>(ref _gem, (Action<Gem, Gem>)null, reader, ref ____gemNetId);
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 8L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<CastInfo>(ref _info, (Action<CastInfo, CastInfo>)null, GeneratedNetworkCode._Read_CastInfo(reader));
		}
		if ((num & 0x10L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref _skillLevel, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
		}
		if ((num & 0x20L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize_NetworkBehaviour<Gem>(ref _gem, (Action<Gem, Gem>)null, reader, ref ____gemNetId);
		}
	}
}
