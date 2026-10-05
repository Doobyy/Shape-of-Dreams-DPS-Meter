using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Summon : Entity
{
	private enum SummonCommandState
	{
		None,
		Move,
		Attack
	}

	private const float HealthScaling_RequiredUpgradesPerZone = 3f;

	public float maxDuration = 10f;

	public ScalingValue armor = "0";

	[SyncVar]
	private float _addedDuration;

	[SyncVar]
	private CastInfo _info;

	[SyncVar]
	internal int _skillLevel = -1;

	protected AbilityTargetValidatorWrapper tvDefaultHarmfulEffectTargets;

	protected AbilityTargetValidatorWrapper tvDefaultUsefulEffectTargets;

	protected AbilityTargetValidatorWrapper tvDefaultAllExceptSelf;

	private StatBonus _statSyncBonus;

	private bool _doOrphanCheck;

	private AbilityTrigger _orphanCheckParentTrigger;

	private float _lastStuckCheckTime;

	private float _nextSoftTauntTime;

	private List<Summon> _parentsSummons;

	private SummonCommandState _scState;

	private Entity _scTarget;

	private Vector3 _scDestination;

	private float _scExpireTime;

	public override bool isDestroyedOnRoomChange => (UnityEngine.Object)(object)hero == null;

	public float normalizedRemainingDuration => Mathf.Clamp01(1f - (Time.time - creationTime - _addedDuration) / maxDuration);

	public float summonHealth
	{
		get
		{
			EntityStatus entityStatus = Status;
			if ((UnityEngine.Object)(object)entityStatus == null)
			{
				entityStatus = ((Component)(object)this).GetComponent<EntityStatus>();
			}
			if ((UnityEngine.Object)(object)NetworkedManagerBase<GameManager>.softInstance == null)
			{
				return entityStatus.baseStats.maxHealth * GameManager.GetUniversalDamageScalingMultiplier_Imp(0f);
			}
			return entityStatus.baseStats.maxHealth * NetworkedManagerBase<GameManager>.instance.GetRegularMonsterDamageMultiplierByScaling((float)(skillLevel - 1) / 3f);
		}
	}

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

	public int skillLevel
	{
		get
		{
			return ScalingValue.levelOverride ?? _skillLevel;
		}
		set
		{
			Network_skillLevel = value;
		}
	}

	public Hero hero { get; private set; }

	public int effectiveLevel => skillLevel;

	public Entity statEntity => this;

	public float Network_addedDuration
	{
		get
		{
			return _addedDuration;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref _addedDuration, 32uL, (Action<float, float>)null);
		}
	}

	public CastInfo Network_info
	{
		get
		{
			return _info;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<CastInfo>(value, ref _info, 64uL, (Action<CastInfo, CastInfo>)null);
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
			((NetworkBehaviour)this).GeneratedSyncVarSetter<int>(value, ref _skillLevel, 128uL, (Action<int, int>)null);
		}
	}

	protected override void OnPrepare()
	{
		tvDefaultHarmfulEffectTargets = new AbilityTargetValidatorWrapper(info.caster, EntityRelation.Neutral | EntityRelation.Enemy);
		tvDefaultUsefulEffectTargets = new AbilityTargetValidatorWrapper(info.caster, EntityRelation.Self | EntityRelation.Ally);
		tvDefaultAllExceptSelf = new AbilityTargetValidatorWrapper(info.caster, EntityRelation.Neutral | EntityRelation.Enemy | EntityRelation.Ally);
		base.OnPrepare();
		if (skillLevel == -1)
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
		}
		if ((UnityEngine.Object)(object)info.caster != null)
		{
			info.caster.Status.ClientEvent_OnFinalStatsChanged += new Action(SyncStats);
		}
		_statSyncBonus = Status.AddStatBonus(new StatBonus());
		SyncStats();
	}

	[Server]
	private void SyncStats()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Summon::SyncStats()' called when server was not active");
		}
		else if (!((UnityEngine.Object)(object)info.caster == null))
		{
			_statSyncBonus.attackDamageFlat = info.caster.Status.attackDamage;
			_statSyncBonus.abilityPowerFlat = info.caster.Status.abilityPower;
			_statSyncBonus.maxHealthFlat = summonHealth - Status.baseStats.maxHealth;
		}
	}

	protected override void OnCreate()
	{
		hero = FindFirstOfType<Hero>();
		base.OnCreate();
		OnCreate_Command();
		if (((NetworkBehaviour)this).isServer)
		{
			_orphanCheckParentTrigger = firstTrigger;
			_doOrphanCheck = (UnityEngine.Object)(object)_orphanCheckParentTrigger != null;
			SyncStats();
			List<Entity> list = DewPhysics.OverlapCircleAllEntities(out var handle, position, 5f, tvDefaultHarmfulEffectTargets, new CollisionCheckSettings
			{
				sortComparer = CollisionCheckSettings.DistanceFromCenter
			});
			if (list.Count > 0)
			{
				AI.Aggro(list[0]);
			}
			handle.Return();
			CreateBasicEffect(this, new ArmorBoostEffect
			{
				strength = GetValue(armor)
			}, float.PositiveInfinity);
			if ((UnityEngine.Object)(object)info.caster != null)
			{
				info.caster.EntityEvent_OnAttackHit += new Action<EventInfoAttackHit>(EntityEventOnAttackHit);
			}
		}
	}

	private void EntityEventOnAttackHit(EventInfoAttackHit obj)
	{
		if ((UnityEngine.Object)(object)AI.context.targetEnemy == null && !obj.victim.IsNullInactiveDeadOrKnockedOut())
		{
			AI.Aggro(obj.victim);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (_parentsSummons != null)
		{
			_parentsSummons.Remove(this);
		}
		if (((NetworkBehaviour)this).isServer)
		{
			if ((UnityEngine.Object)(object)info.caster != null)
			{
				info.caster.EntityEvent_OnAttackHit -= new Action<EventInfoAttackHit>(EntityEventOnAttackHit);
			}
			if ((UnityEngine.Object)(object)info.caster != null)
			{
				info.caster.Status.ClientEvent_OnFinalStatsChanged -= new Action(SyncStats);
			}
		}
	}

	public override void OnStart()
	{
		Actor actor = parentActor;
		while ((UnityEngine.Object)(object)actor != null)
		{
			if (actor is Hero hero)
			{
				_parentsSummons = hero.summons;
				_parentsSummons.Add(this);
				break;
			}
			actor = actor.parentActor;
		}
		if (!((NetworkBehaviour)this).isServer)
		{
			tvDefaultHarmfulEffectTargets = new AbilityTargetValidatorWrapper(info.caster, EntityRelation.Neutral | EntityRelation.Enemy);
			tvDefaultUsefulEffectTargets = new AbilityTargetValidatorWrapper(info.caster, EntityRelation.Self | EntityRelation.Ally);
			tvDefaultAllExceptSelf = new AbilityTargetValidatorWrapper(info.caster, EntityRelation.Neutral | EntityRelation.Enemy | EntityRelation.Ally);
		}
		base.OnStart();
	}

	protected override StaggerSettings GetStaggerSettings()
	{
		return StaggerSettings.SummonDefault;
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		base.ActiveLogicUpdate(dt);
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		if (_doOrphanCheck && (_orphanCheckParentTrigger.IsNullOrInactive() || _orphanCheckParentTrigger.owner.IsNullOrInactive()))
		{
			Kill();
			return;
		}
		if (normalizedRemainingDuration <= 0.0001f && !NetworkedManagerBase<ZoneManager>.instance.isInAnyTransition)
		{
			Kill();
			return;
		}
		if (hero != null && hero.IsNullInactiveDeadOrKnockedOut())
		{
			Kill();
			return;
		}
		if ((UnityEngine.Object)(object)hero != null && ((Behaviour)(object)Control._agent).enabled && !Control.isDisplacing && Time.time - _lastStuckCheckTime > 1f)
		{
			_lastStuckCheckTime = Time.time;
			if ((int)Dew.GetNavMeshPathStatus(hero.agentPosition, agentPosition) != 0)
			{
				Vector3 v = agentPosition;
				Vector3 validAgentDestination_Closest = Dew.GetValidAgentDestination_Closest(hero.agentPosition, v);
				Dew.FilterNonOkayValues(ref v);
				Control.StartDisplacement(new DispByDestination
				{
					destination = validAgentDestination_Closest,
					duration = 0.2f,
					ease = DewEase.EaseOutQuad,
					canGoOverTerrain = true,
					isFriendly = true,
					rotateForward = false,
					rotateSmoothly = false,
					affectedByMovementSpeed = false,
					isCanceledByCC = false
				});
			}
		}
		if (!((UnityEngine.Object)(object)hero != null) || !hero.isInCombat || !(Time.time > _nextSoftTauntTime))
		{
			return;
		}
		_nextSoftTauntTime = Time.time + UnityEngine.Random.Range(0.5f, 1.5f);
		ListReturnHandle<Entity> handle;
		foreach (Entity item in DewPhysics.OverlapCircleAllEntities(out handle, agentPosition, 4f, tvDefaultHarmfulEffectTargets))
		{
			if (UnityEngine.Random.value < 0.5f && (UnityEngine.Object)(object)item.AI.context.targetEnemy == (UnityEngine.Object)(object)hero)
			{
				item.AI.Aggro(this);
			}
		}
		handle.Return();
	}

	protected override void AIUpdate(ref EntityAIContext context)
	{
		base.AIUpdate(ref context);
		if (!((UnityEngine.Object)(object)info.caster != null))
		{
			return;
		}
		float num = Vector2.Distance(info.caster.agentPosition.ToXY(), agentPosition.ToXY());
		if ((UnityEngine.Object)(object)context.targetEnemy == null)
		{
			if (num > 5f || info.caster.Control.isWalking)
			{
				FollowCaster();
			}
			else if (Control._desiredAgentDestination.HasValue && Vector2.Distance(Control._desiredAgentDestination.Value.ToXY(), agentPosition.ToXY()) < 2f)
			{
				Control.ClearMovement();
			}
		}
		else
		{
			AI.Helper_ChaseTarget();
		}
	}

	public override bool CanAggroTo(Entity target)
	{
		if (!base.CanAggroTo(target))
		{
			return false;
		}
		if (info.caster.IsNullInactiveDeadOrKnockedOut())
		{
			return true;
		}
		EntityRelation relation = info.caster.GetRelation(target);
		float num = 2.5f;
		float num2 = 14f;
		if (relation == EntityRelation.Neutral)
		{
			num = 4f;
			num2 = 10f;
		}
		return Vector2.Distance((info.caster.Control.isWalking ? (info.caster.agentPosition + info.caster.rotation * (Vector3.forward * num)) : info.caster.agentPosition).ToXY(), target.agentPosition.ToXY()) < num2;
	}

	private void FollowCaster()
	{
		Vector3 vector = AbilityTrigger.PredictPoint_Simple(this, 1f, info.caster, 1f);
		Control.MoveToDestination(vector, immediately: false, Mathf.Lerp(0.3f, 1f, Vector2.Distance(vector.ToXY(), agentPosition.ToXY()) / 8f));
	}

	[Server]
	public void AddDuration(float duration, bool clampToMaxDuration = true)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Summon::AddDuration(System.Single,System.Boolean)' called when server was not active");
		}
		else if (!(duration <= 0f) && !float.IsPositiveInfinity(maxDuration))
		{
			if (clampToMaxDuration)
			{
				float num = Time.time - creationTime - _addedDuration;
				float b = Mathf.Max(0f, maxDuration - num);
				duration = Mathf.Min(duration, b);
			}
			Network_addedDuration = _addedDuration + duration;
		}
	}

	[Server]
	public void ReduceDuration(float duration, bool clampToMaxDuration = true)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Summon::ReduceDuration(System.Single,System.Boolean)' called when server was not active");
		}
		else if (!(duration <= 0f) && !float.IsPositiveInfinity(maxDuration))
		{
			Network_addedDuration = _addedDuration - duration;
		}
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

	private void OnCreate_Command()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		AI.customBehaviors.Add(delegate(ref EntityAIContext context)
		{
			if (_scState != SummonCommandState.None && Time.time > _scExpireTime)
			{
				if (_scState == SummonCommandState.Move)
				{
					Control.ClearMovement();
				}
				_scState = SummonCommandState.None;
			}
			if (_scState == SummonCommandState.Attack && _scTarget.IsNullInactiveDeadOrKnockedOut())
			{
				_scState = SummonCommandState.None;
			}
			if (_scState == SummonCommandState.Move)
			{
				if (Vector3.Distance(agentPosition, _scDestination) < 2f)
				{
					_scState = SummonCommandState.None;
					Control.ClearMovement();
				}
				Control.MoveToDestination(_scDestination, immediately: false);
				return true;
			}
			if (_scState == SummonCommandState.Attack)
			{
				if ((UnityEngine.Object)(object)context.targetEnemy != (UnityEngine.Object)(object)_scTarget)
				{
					AI.Aggro(_scTarget);
				}
				return false;
			}
			return false;
		}, 100);
	}

	[Server]
	public void SummonCommand_MoveToDestination(Vector3 destination)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Summon::SummonCommand_MoveToDestination(UnityEngine.Vector3)' called when server was not active");
			return;
		}
		destination = Dew.GetValidAgentDestination_Closest(agentPosition, destination);
		_scState = SummonCommandState.Move;
		_scExpireTime = Time.time + 2f + Vector3.Distance(agentPosition, destination) / Control.baseAgentSpeed * 1.25f;
		_scDestination = destination;
		AI.CallAIUpdateImmediately();
	}

	[Server]
	public void SummonCommand_AttackTarget(Entity target)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Summon::SummonCommand_AttackTarget(Entity)' called when server was not active");
		}
		else if (!target.IsNullInactiveDeadOrKnockedOut())
		{
			_scState = SummonCommandState.Attack;
			_scExpireTime = Time.time + 4f + Vector3.Distance(agentPosition, target.agentPosition) / Control.baseAgentSpeed * 1.25f;
			_scTarget = target;
			AI.CallAIUpdateImmediately();
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
			NetworkWriterExtensions.WriteFloat(writer, _addedDuration);
			GeneratedNetworkCode._Write_CastInfo(writer, _info);
			NetworkWriterExtensions.WriteInt(writer, _skillLevel);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x20L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, _addedDuration);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x40L) != 0L)
		{
			GeneratedNetworkCode._Write_CastInfo(writer, _info);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x80L) != 0L)
		{
			NetworkWriterExtensions.WriteInt(writer, _skillLevel);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _addedDuration, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<CastInfo>(ref _info, (Action<CastInfo, CastInfo>)null, GeneratedNetworkCode._Read_CastInfo(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref _skillLevel, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x20L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _addedDuration, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
		if ((num & 0x40L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<CastInfo>(ref _info, (Action<CastInfo, CastInfo>)null, GeneratedNetworkCode._Read_CastInfo(reader));
		}
		if ((num & 0x80L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref _skillLevel, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
		}
	}
}
