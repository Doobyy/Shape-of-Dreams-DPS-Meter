using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

[LogicUpdatePriority(-301)]
public class EntityAI : EntityComponent
{
	public CustomAIBehaviors customBehaviors;

	public static bool DisableAI = false;

	public const float DefaultDetectionRange = 15f;

	public const float BossDetectionRange = 25f;

	public const float AggroPropagationRange = 17f;

	public const float AggroPropagationDelayMin = 0.1f;

	public const float AggroPropagationDelayMax = 0.7f;

	public const float IdleAITickInterval = 1f;

	public const float CombatAITickInterval = 0.5f;

	public const float BossAITickInterval = 0.25f;

	public const float LoseTargetedEnemyTime = 5f;

	public const float RetargetingMinTime = 1f;

	public const float RetargetingMaxTime = 4f;

	public const float RetargetingDistanceFuzziness = 0.3f;

	public const int AITickCongestionThreshold = 5000;

	public const float WanderSpeedMultiplier = 0.6f;

	public const float WanderIntervalMin = 4f;

	public const float WanderIntervalMax = 15f;

	public static int PositionSampleCount = 4;

	public static int PositionSampleLagBehindFrames = 2;

	public static float PositionSampleInterval = 0.1f;

	private static int _numOfAi = 0;

	private static float _congestionSkipChance = 0f;

	private Func<Entity, bool> _targetEnemyValidator;

	private Func<Entity, bool> _allyValidator;

	public bool disableAI;

	public bool excludeFromAutoTargeting;

	[NonSerialized]
	public Func<float> predictionStrengthOverride;

	private bool _isAITicking;

	internal EntityAIContext _aiContext;

	private Vector3[] _positionSamples;

	private int _lastSampleIndex;

	private float _lastSampleTime;

	private float _nextAiUpdateTime;

	private float _lastAiUpdateTime;

	private float _nextWanderTime;

	public bool isAITicking
	{
		get
		{
			return _isAITicking;
		}
		set
		{
			if (_isAITicking != value)
			{
				_isAITicking = value;
				if (value)
				{
					_numOfAi++;
				}
				else
				{
					_numOfAi--;
				}
				if (_numOfAi > 5000)
				{
					_congestionSkipChance = (float)(_numOfAi - 5000) / (float)_numOfAi;
				}
				else
				{
					_congestionSkipChance = 0f;
				}
			}
		}
	}

	public float detectionRange { get; private set; }

	public EntityAIContext context => _aiContext;

	public Vector3 estimatedVelocity { get; private set; }

	public Vector3 estimatedVelocityUnclamped { get; private set; }

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void Init()
	{
		_numOfAi = 0;
		_congestionSkipChance = 0f;
	}

	public override void ClearPooledEventsAndProcessors()
	{
		base.ClearPooledEventsAndProcessors();
		predictionStrengthOverride = null;
	}

	private float GetAIInterval()
	{
		if (entity is Summon summon && summon.info.caster is Hero)
		{
			return 0.25f;
		}
		if (entity is Hero)
		{
			return 0.15f;
		}
		if (entity is Monster { type: var type } && (type == Monster.MonsterType.MiniBoss || type == Monster.MonsterType.Boss))
		{
			return 0.25f;
		}
		if (!((UnityEngine.Object)(object)_aiContext.targetEnemy == null))
		{
			return 0.5f;
		}
		return 1f;
	}

	public override void OnStartServer()
	{
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		base.OnStartServer();
		customBehaviors = new CustomAIBehaviors();
		float aIInterval = GetAIInterval();
		_nextAiUpdateTime = Time.time + UnityEngine.Random.value * aIInterval * 0.5f;
		_lastAiUpdateTime = _nextAiUpdateTime - aIInterval * 0.5f;
		_targetEnemyValidator = (Entity e) =>
		{
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			if (e.Status.isUndetectableByNonAllies)
			{
				return false;
			}
			if (!entity.CanAggroTo(e))
			{
				return false;
			}
			return (entity.GetRelation(e) == EntityRelation.Enemy && e.isActive && e.Status.isAlive && !(e is Hero { isKnockedOut: not false }) && ((UnityEngine.Object)(object)_aiContext.targetEnemy == (UnityEngine.Object)(object)e || (int)Dew.GetNavMeshPathStatusCached(entity, e, entity.agentPosition, e.agentPosition) == 0)) ? true : false;
		};
		_allyValidator = (Entity e) => (entity.GetRelation(e) == EntityRelation.Ally && e.isActive && e.Status.isAlive) ? true : false;
		_positionSamples = ArrayPool<Vector3>.Shared.Rent(PositionSampleCount);
		for (int num = 0; num < PositionSampleCount; num++)
		{
			_positionSamples[num] = ((Component)(object)this).transform.position;
		}
		_lastSampleTime = Time.time;
		detectionRange = ((entity is Monster { type: Monster.MonsterType.Boss }) ? 25f : 15f);
		ListReturnHandle<Entity> handle;
		foreach (Entity item in DewPhysics.OverlapCircleAllEntities(out handle, ((Component)(object)this).transform.position, 17f, _allyValidator, new CollisionCheckSettings
		{
			sortComparer = CollisionCheckSettings.DistanceFromCenter
		}))
		{
			Entity targetEnemy = item.AI._aiContext.targetEnemy;
			if (!targetEnemy.IsNullInactiveDeadOrKnockedOut() && entity.GetRelation(targetEnemy) == EntityRelation.Enemy && (int)Dew.GetNavMeshPathStatus(entity.agentPosition, item.agentPosition) == 0)
			{
				if (entity.CanAggroTo(targetEnemy))
				{
					Aggro(targetEnemy);
				}
				break;
			}
		}
		handle.Return();
		entity.EntityEvent_OnTakeDamage += (Action<EventInfoDamage>)((EventInfoDamage dmg) =>
		{
			if (!((UnityEngine.Object)(object)_aiContext.targetEnemy != null))
			{
				Entity firstEntity = dmg.actor.firstEntity;
				if (!((UnityEngine.Object)(object)firstEntity == null) && entity.GetRelation(firstEntity) == EntityRelation.Enemy)
				{
					Aggro(firstEntity, doPropagation: true);
				}
			}
		});
	}

	public void CallAIUpdateImmediately()
	{
		float dt = Time.time - _lastAiUpdateTime;
		UpdateAIContext(dt);
		_aiContext._insideAIUpdate = true;
		entity.CallAIUpdate(ref _aiContext);
		_aiContext._insideAIUpdate = false;
	}

	public override void LogicUpdate(float dt)
	{
		base.LogicUpdate(dt);
		if (!((NetworkBehaviour)this).isServer || entity.isSleeping)
		{
			return;
		}
		isAITicking = entity.isActive && !disableAI && (UnityEngine.Object)(object)entity.owner.controllingEntity != (UnityEngine.Object)(object)entity;
		if (!DisableAI && isAITicking && Time.time > _nextAiUpdateTime && !entity.Visual.isSpawning)
		{
			if (entity.Control.IsActionBlocked(EntityControl.BlockableAction.Ability) == EntityControl.BlockStatus.Blocked)
			{
				return;
			}
			if (UnityEngine.Random.value < _congestionSkipChance)
			{
				_nextAiUpdateTime = Time.time + UnityEngine.Random.value * GetAIInterval();
				return;
			}
			CallAIUpdateImmediately();
			if ((entity is Monster || entity is IEnableWandering) && Time.time > _nextWanderTime && (UnityEngine.Object)(object)_aiContext.targetEnemy == null && entity.Control.queuedActions.Count == 0 && !(entity is IEnableWandering { shouldWanderAround: false }))
			{
				_nextWanderTime = Time.time + UnityEngine.Random.Range(4f, 15f);
				if (entity is Monster { campPosition: not null, campPosition: var campPosition } monster)
				{
					Vector3 positionOnGround = Dew.GetPositionOnGround(campPosition.Value + UnityEngine.Random.insideUnitSphere * 6f);
					positionOnGround = Dew.GetValidAgentDestination_LinearSweep(monster.campPosition.Value, positionOnGround);
					entity.Control.MoveToDestination(positionOnGround, immediately: false, 0.6f);
				}
				else if (entity.section != null)
				{
					_nextWanderTime = Time.time + UnityEngine.Random.Range(4f, 15f);
					Vector3 goodWanderPosition = entity.section.GetGoodWanderPosition(entity.agentPosition);
					entity.Control.MoveToDestination(goodWanderPosition, immediately: false, 0.6f);
				}
			}
			_lastAiUpdateTime = Time.time;
			_nextAiUpdateTime = Time.time + GetAIInterval();
		}
		if (!(Time.time - _lastSampleTime > PositionSampleInterval))
		{
			return;
		}
		if (_positionSamples.Length < PositionSampleCount)
		{
			_positionSamples = ArrayPool<Vector3>.Shared.Rent(PositionSampleCount);
			for (int i = 0; i < PositionSampleCount; i++)
			{
				_positionSamples[i] = ((Component)(object)this).transform.position;
			}
		}
		_lastSampleTime = Time.time;
		_lastSampleIndex = (_lastSampleIndex + 1) % PositionSampleCount;
		_positionSamples[_lastSampleIndex] = ((Component)(object)this).transform.position;
		int num = _lastSampleIndex + 1;
		if (num >= PositionSampleCount)
		{
			num = 0;
		}
		estimatedVelocity = (_positionSamples[(PositionSampleCount + _lastSampleIndex - PositionSampleLagBehindFrames) % PositionSampleCount] - _positionSamples[num]) / (PositionSampleInterval * (float)(PositionSampleCount - 1 - PositionSampleLagBehindFrames));
		estimatedVelocityUnclamped = estimatedVelocity;
		estimatedVelocity = Vector3.ClampMagnitude(estimatedVelocity, entity.Control.currentMaxAgentSpeed);
	}

	public override void OnStopServer()
	{
		base.OnStopServer();
		_aiContext = default;
		if (isAITicking)
		{
			isAITicking = false;
		}
		if (_positionSamples != null)
		{
			ArrayPool<Vector3>.Shared.Return(_positionSamples, false);
		}
	}

	[Server]
	public void DropAggro()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntityAI::DropAggro()' called when server was not active");
		}
		else if (!((UnityEngine.Object)(object)_aiContext.targetEnemy == null))
		{
			if ((UnityEngine.Object)(object)entity.Control.attackTarget == (UnityEngine.Object)(object)_aiContext.targetEnemy)
			{
				entity.Control.attackTarget = null;
			}
			_aiContext.targetEnemy = null;
			_aiContext._targetEnemyLoseElapsedTime = 0f;
		}
	}

	public void Aggro(Entity target, bool doPropagation = false)
	{
		if (!base.entity.CheckEnemyOrNeutral(target))
		{
			Debug.LogWarning($"Cannot aggro to non-enemy entity: {base.entity} => {target}");
		}
		else
		{
			if (!base.entity.CanAggroTo(target))
			{
				return;
			}
			if ((UnityEngine.Object)(object)_aiContext.targetEnemy == null)
			{
				_nextAiUpdateTime = Time.time - 0.01f;
			}
			_aiContext.targetEnemy = target;
			_aiContext._targetEnemyStartTime = Time.time;
			_aiContext._targetEnemyLoseElapsedTime = 0f;
			_aiContext._retargetingTime = UnityEngine.Random.Range(1f, 4f);
			if (!doPropagation)
			{
				return;
			}
			List<Entity> list = DewPhysics.OverlapCircleAllEntities(out var handle, ((Component)(object)this).transform.position, 17f, _allyValidator, new CollisionCheckSettings
			{
				sortComparer = CollisionCheckSettings.DistanceFromCenter
			});
			foreach (Entity item in list)
			{
				item.AI.ReceiveAggroPropagation(target);
			}
			if (base.entity.section != null)
			{
				foreach (ActorRef<Entity> entity2 in base.entity.section.entities)
				{
					Entity entity = entity2.Get();
					if (!entity.IsNullOrInactive() && !list.Contains(entity) && base.entity.GetRelation(entity) == EntityRelation.Ally)
					{
						entity.AI.ReceiveAggroPropagation(target);
					}
				}
			}
			handle.Return();
		}
	}

	internal void ReceiveAggroPropagation(Entity target)
	{
		if (entity.GetRelation(target) == EntityRelation.Enemy)
		{
			((MonoBehaviour)(object)this).StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			yield return new WaitForSeconds(UnityEngine.Random.Range(0.1f, 0.7f));
			if (!((UnityEngine.Object)(object)_aiContext.targetEnemy != null) && entity.isActive)
			{
				Aggro(target);
			}
		}
	}

	[Server]
	private void UpdateAIContext(float dt)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntityAI::UpdateAIContext(System.Single)' called when server was not active");
			return;
		}
		_aiContext.deltaTime = dt;
		if ((UnityEngine.Object)(object)_aiContext.targetEnemy != null)
		{
			if (!_aiContext.targetEnemy.isActive || _aiContext.targetEnemy.Status.isDead || _aiContext.targetEnemy is Hero { isKnockedOut: not false } || _aiContext._targetEnemyLoseElapsedTime > 5f || !entity.CanAggroTo(_aiContext.targetEnemy))
			{
				_aiContext.targetEnemy = null;
				if (_aiContext.targetEnemyLastKnownPosition.HasValue && (UnityEngine.Object)(object)entity.Ability.attackAbility != null)
				{
					entity.Control.MoveToDestination(_aiContext.targetEnemyLastKnownPosition.Value, immediately: false);
				}
			}
			else
			{
				if (_aiContext.targetEnemy is Hero hero2)
				{
					hero2.MarkAsInCombat();
				}
				float num = ((_aiContext.targetEnemy.Status.hasInvulnerable || _aiContext.targetEnemy.Status.hasUntargetable || _aiContext.targetEnemy.Status.isUndetectableByNonAllies) ? 1.5f : 1f);
				if (_aiContext.targetEnemyElapsedTime * num > _aiContext._retargetingTime)
				{
					List<Entity> list = DewPhysics.OverlapCircleAllEntities(out var handle, ((Component)(object)this).transform.position, detectionRange, _targetEnemyValidator);
					for (int num2 = list.Count - 1; num2 >= 0; num2--)
					{
						if (list[num2].Status.isUndetectableByNonAllies)
						{
							list.RemoveAt(num2);
						}
					}
					if (list.Count > 0)
					{
						if (entity.IsAnyBoss() && UnityEngine.Random.value < 0.4f)
						{
							Aggro(list[UnityEngine.Random.Range(0, list.Count)]);
						}
						else
						{
							Entity target = Dew.SelectBestWithScore((IList<Entity>)list, (Func<Entity, int, float>)((Entity e, int _) =>
							{
								float num3 = 0f - Vector2.Distance(entity.agentPosition.ToXY(), e.agentPosition.ToXY());
								if (e.Status.hasUntargetable)
								{
									num3 -= 100f;
								}
								return num3;
							}), 0.3f, (DewRandom)null);
							Aggro(target);
						}
					}
					else if (!_aiContext.targetEnemy.Status.isUndetectableByNonAllies)
					{
						if (Vector2.Distance(_aiContext.targetEnemy.position.ToXY(), ((Component)(object)this).transform.position.ToXY()) - _aiContext.targetEnemy.Control.outerRadius <= detectionRange)
						{
							Aggro(_aiContext.targetEnemy);
						}
					}
					else
					{
						if ((UnityEngine.Object)(object)entity.Control.attackTarget == (UnityEngine.Object)(object)_aiContext.targetEnemy)
						{
							entity.Control.attackTarget = null;
						}
						_aiContext.targetEnemy = null;
					}
					handle.Return();
				}
			}
		}
		if ((UnityEngine.Object)(object)_aiContext.targetEnemy == null)
		{
			List<Entity> list2 = DewPhysics.OverlapCircleAllEntities(out var handle2, ((Component)(object)this).transform.position, detectionRange, _targetEnemyValidator, new CollisionCheckSettings
			{
				sortComparer = CollisionCheckSettings.DistanceFromCenter
			});
			if (list2.Count > 0)
			{
				Aggro(list2[0], doPropagation: true);
			}
			else if (entity is BossMonster)
			{
				Hero closestAliveHero = Dew.GetClosestAliveHero(entity.agentPosition, fallbackToDead: false, entity);
				if ((UnityEngine.Object)(object)closestAliveHero != null && !closestAliveHero.Status.isUndetectableByNonAllies)
				{
					Aggro(closestAliveHero, doPropagation: true);
				}
			}
			handle2.Return();
		}
		if ((UnityEngine.Object)(object)_aiContext.targetEnemy != null)
		{
			_aiContext.targetEnemyLastKnownPosition = _aiContext.targetEnemy.position;
			if (Vector2.Distance(_aiContext.targetEnemy.position.ToXY(), ((Component)(object)this).transform.position.ToXY()) - _aiContext.targetEnemy.Control.outerRadius > detectionRange || !_targetEnemyValidator(_aiContext.targetEnemy))
			{
				_aiContext._targetEnemyLoseElapsedTime += dt;
			}
		}
	}

	private void CheckInsideAIUpdate()
	{
		if (!_aiContext._insideAIUpdate)
		{
			throw new InvalidOperationException("AI helper functions can only be called inside AIUpdate");
		}
	}

	public T Helper_GetAbility<T>() where T : AbilityTrigger
	{
		CheckInsideAIUpdate();
		T val = ((!(entity.Ability.attackAbility is T val2)) ? entity.Ability.GetAbility<T>() : val2);
		if ((UnityEngine.Object)(object)val == null)
		{
			Debug.LogError(string.Format("{0}: '{1}' does not have '{2}'", "Helper_GetAbility", entity, typeof(T)));
		}
		return val;
	}

	public void Helper_ChaseTarget()
	{
		CheckInsideAIUpdate();
		if ((UnityEngine.Object)(object)_aiContext.targetEnemy == null)
		{
			Debug.LogWarning(string.Format("{0}: '{1}' does not have a target right now", "Helper_ChaseTarget", entity));
		}
		else
		{
			entity.Control.Attack(_aiContext.targetEnemy, doChase: true);
		}
	}

	public void Helper_CastAbility<T>(CastInfo info) where T : AbilityTrigger
	{
		CheckInsideAIUpdate();
		T val = Helper_GetAbility<T>();
		if ((UnityEngine.Object)(object)val == null)
		{
			Debug.LogError(string.Format("{0}: '{1}' does not have '{2}'", "Helper_CastAbility", entity, typeof(T)));
		}
		else if (!val.CanBeCast())
		{
			Debug.LogWarning(string.Format("{0}: '{1}' cannot cast '{2}' right now", "Helper_CastAbility", entity, typeof(T)));
		}
		else
		{
			entity.Control.Cast(val, info);
		}
	}

	public bool Helper_TryGetCastInfoAuto<T>(out CastInfo info) where T : AbilityTrigger
	{
		CheckInsideAIUpdate();
		T val = Helper_GetAbility<T>();
		if ((UnityEngine.Object)(object)val == null)
		{
			Debug.LogError(string.Format("{0}: '{1}' does not have '{2}'", "Helper_CastAbility", entity, typeof(T)));
			info = default;
			return false;
		}
		return Helper_TryGetCastInfoAuto(val, out info);
	}

	public bool Helper_TryGetCastInfoAuto(AbilityTrigger abil, out CastInfo info)
	{
		CheckInsideAIUpdate();
		if ((UnityEngine.Object)(object)abil == null)
		{
			info = default;
			return false;
		}
		if (abil.currentConfig.castMethod.type == CastMethodType.None)
		{
			info = new CastInfo(base.entity);
			return true;
		}
		Entity entity = null;
		if ((UnityEngine.Object)(object)_aiContext.targetEnemy != null && abil.currentConfig.targetValidator.Evaluate(base.entity, _aiContext.targetEnemy))
		{
			entity = _aiContext.targetEnemy;
		}
		else
		{
			ListReturnHandle<Entity> handle;
			foreach (Entity item in DewPhysics.OverlapCircleAllEntities(out handle, ((Component)(object)this).transform.position, detectionRange, new CollisionCheckSettings
			{
				sortComparer = CollisionCheckSettings.DistanceFromCenter
			}))
			{
				if (abil.currentConfig.targetValidator.Evaluate(base.entity, item))
				{
					entity = item;
					break;
				}
			}
			handle.Return();
		}
		if ((UnityEngine.Object)(object)entity == null)
		{
			info = default;
			return false;
		}
		info = abil.GetPredictedCastInfoToTarget(entity);
		return true;
	}

	public void Helper_CastAbilityAuto<T>() where T : AbilityTrigger
	{
		CheckInsideAIUpdate();
		T val = Helper_GetAbility<T>();
		CastInfo info;
		if ((UnityEngine.Object)(object)val == null)
		{
			Debug.LogError(string.Format("{0}: '{1}' does not have '{2}'", "Helper_CastAbilityAuto", entity, typeof(T)));
		}
		else if (!val.CanBeCast())
		{
			Debug.LogWarning(string.Format("{0}: '{1}' cannot cast '{2}' right now", "Helper_CastAbilityAuto", entity, typeof(T)));
		}
		else if (!Helper_TryGetCastInfoAuto(val, out info))
		{
			Debug.Log("Helper_CastAbilityAuto: Helper_TryGetCastInfoAuto failed");
		}
		else
		{
			entity.Control.Cast(val, info);
		}
	}

	public bool Helper_CastAbilityAuto(AbilityTrigger abil)
	{
		CheckInsideAIUpdate();
		if ((UnityEngine.Object)(object)abil == null)
		{
			return false;
		}
		if (!abil.CanBeCast())
		{
			return false;
		}
		if (!Helper_TryGetCastInfoAuto(abil, out var info))
		{
			return false;
		}
		entity.Control.Cast(abil, info);
		return true;
	}

	public bool Helper_CanBeCast<T>() where T : AbilityTrigger
	{
		CheckInsideAIUpdate();
		T val = Helper_GetAbility<T>();
		if ((UnityEngine.Object)(object)val == null)
		{
			return false;
		}
		if (val.CanBeCast())
		{
			if (!val.currentConfig.ignoreBlock)
			{
				return entity.Control.IsActionBlocked((!(val is AttackTrigger)) ? EntityControl.BlockableAction.Ability : EntityControl.BlockableAction.Attack) != EntityControl.BlockStatus.Blocked;
			}
			return true;
		}
		return false;
	}

	public bool Helper_CanBeCast(AbilityTrigger abil)
	{
		CheckInsideAIUpdate();
		if ((UnityEngine.Object)(object)abil == null)
		{
			return false;
		}
		if (abil.CanBeCast())
		{
			if (!abil.currentConfig.ignoreBlock)
			{
				return entity.Control.IsActionBlocked((!(abil is AttackTrigger)) ? EntityControl.BlockableAction.Ability : EntityControl.BlockableAction.Attack) != EntityControl.BlockStatus.Blocked;
			}
			return true;
		}
		return false;
	}

	public bool Helper_IsTargetInRange<T>() where T : AbilityTrigger
	{
		CheckInsideAIUpdate();
		T val = Helper_GetAbility<T>();
		if ((UnityEngine.Object)(object)val == null)
		{
			Debug.LogError(string.Format("{0}: '{1}' does not have '{2}'", "Helper_IsTargetInRange", entity, typeof(T)));
			return false;
		}
		return Helper_IsTargetInRange(val);
	}

	public bool Helper_IsTargetInRange(AbilityTrigger abil)
	{
		CheckInsideAIUpdate();
		if ((UnityEngine.Object)(object)abil == null)
		{
			return false;
		}
		if ((UnityEngine.Object)(object)_aiContext.targetEnemy == null)
		{
			return false;
		}
		return abil.IsTargetInRange(_aiContext.targetEnemy, isAI: true);
	}

	public bool Helper_IsTargetInRangeOfAttack()
	{
		CheckInsideAIUpdate();
		AbilityTrigger attackAbility = entity.Ability.attackAbility;
		if ((UnityEngine.Object)(object)attackAbility == null)
		{
			Debug.LogError(string.Format("{0}: '{1}' does not have attack ability", "Helper_IsTargetInRangeOfAttack", entity));
			return false;
		}
		if ((UnityEngine.Object)(object)_aiContext.targetEnemy == null)
		{
			return false;
		}
		float effectiveRange = attackAbility.currentConfig.effectiveRange;
		return Vector3.Distance(entity.position, _aiContext.targetEnemy.GetAIPosition(entity)) - _aiContext.targetEnemy.Control.outerRadius < effectiveRange;
	}

	private void MirrorProcessed()
	{
	}
}
