using UnityEngine;

public class ActionCast : ActionBase
{
	public AbilityTrigger trigger;

	public int configIndex;

	public bool isPredictedOnCast;

	public CastInfo info;

	public Entity predictTarget;

	public bool skipRangeCheck;

	private EntityControl.BlockableAction _blockable;

	private Vector3? _lastReportedDestination;

	private float _lastReportedRequiredDistance;

	private float _lastAttackDestinationUpdateTime = float.NegativeInfinity;

	private float _nextAttackDestinationUpdateInterval;

	private float _lastPathStatusTime;

	private bool _lastPathReachable = true;

	private TriggerConfig _config => trigger.currentConfig;

	private bool _isMonsterChase
	{
		get
		{
			if (_entity is Monster)
			{
				return (Object)(object)trigger == (Object)(object)_entity.Ability.attackAbility;
			}
			return false;
		}
	}

	public override bool Tick()
	{
		//IL_02ba: Unknown result type (might be due to invalid IL or missing references)
		if (trigger.IsNullOrInactive() || (isPredictedOnCast && predictTarget.IsNullInactiveDeadOrKnockedOut()) || (configIndex >= 0 && trigger.currentConfigIndex != configIndex) || ((Object)(object)_entity.Ability.attackAbility != (Object)(object)trigger && !trigger.CanBeReserved()) || (info.target != null && info.target.IsNullInactiveDeadOrKnockedOut()))
		{
			return true;
		}
		if (isFirstTick)
		{
			if (trigger is AttackTrigger)
			{
				_blockable = EntityControl.BlockableAction.Attack;
			}
			else if (_entity is Hero && trigger.abilityIndex == 5)
			{
				_blockable = EntityControl.BlockableAction.Dodge;
			}
			else
			{
				_blockable = EntityControl.BlockableAction.Ability;
			}
			if (!_config.ignoreBlock && _entity.Control.IsActionBlocked(_blockable) == EntityControl.BlockStatus.BlockedCancelable)
			{
				_entity.Control.DisobeyBlock(_blockable);
			}
		}
		bool flag = trigger.CanBeCast();
		bool flag2 = !_config.ignoreBlock && _entity.Control.IsActionBlocked(_blockable) != EntityControl.BlockStatus.Allowed;
		if (trigger is AttackTrigger)
		{
			foreach (Channel ongoingChannel in _entity.Control.ongoingChannels)
			{
				if (ongoingChannel.isAttack)
				{
					flag2 = true;
				}
			}
		}
		if (!flag | flag2)
		{
			return false;
		}
		if ((Object)(object)predictTarget != null && !_config.targetValidator.Evaluate(_entity, predictTarget))
		{
			return true;
		}
		if (_config.castMethod.type == CastMethodType.Target && (bool)(Object)(object)predictTarget)
		{
			if (predictTarget.Status.hasUntargetable)
			{
				return true;
			}
			if (_entity.CheckEnemyOrNeutral(predictTarget) && predictTarget.Status.isUndetectableByNonAllies)
			{
				return true;
			}
		}
		bool flag3 = (((Object)(object)predictTarget != null) ? _config.CheckRange(_entity, predictTarget) : _config.CheckRange(info));
		if (trigger.ignoreRangeCheck)
		{
			flag3 = true;
		}
		if (skipRangeCheck)
		{
			flag3 = true;
		}
		if (((_entity is Monster && (Object)(object)predictTarget != null) & flag3) && (int)Dew.GetNavMeshPathStatusCached(_entity, predictTarget, _entity.agentPosition, predictTarget.agentPosition) != 0)
		{
			flag3 = false;
		}
		if (!flag3)
		{
			return false;
		}
		CastInfo castInfo = (((Object)(object)predictTarget != null) ? trigger.GetPredictedCastInfoToTarget(predictTarget) : info);
		if (_config.castMethod.type == CastMethodType.Point && _config.castMethod.pointData.isClamping)
		{
			float y = castInfo.point.y;
			Vector2 vector = _entity.agentPosition.ToXY() + Vector2.ClampMagnitude(castInfo.point.ToXY() - _entity.agentPosition.ToXY(), _config.castMethod.pointData.range);
			castInfo.point = new Vector3(vector.x, y, vector.y);
		}
		if (_config.castMethod.type == CastMethodType.Point)
		{
			Vector2 vector2 = castInfo.point.ToXY();
			if (vector2 == _entity.agentPosition.ToXY() || vector2 == _entity.position.ToXY())
			{
				castInfo.point = _entity.agentPosition + ((Component)(object)_entity).transform.forward * 0.001f;
			}
		}
		trigger.OnCastStart(trigger.currentConfigIndex, castInfo);
		return true;
	}

	public override Vector3? GetMoveDestination()
	{
		Vector3 vector = default;
		bool isMonsterChase = _isMonsterChase;
		if (isMonsterChase)
		{
			Monster monster = (Monster)_entity;
			if (Time.time - _lastAttackDestinationUpdateTime < _nextAttackDestinationUpdateInterval && _lastReportedDestination.HasValue)
			{
				if (Vector2.Distance(_lastReportedDestination.Value.ToXY(), _entity.agentPosition.ToXY()) < _lastReportedRequiredDistance)
				{
					return isPredictedOnCast ? predictTarget.GetAIAgentPosition(_entity) : _config.GetMoveToCastDestination(info);
				}
				return _lastReportedDestination;
			}
			_lastAttackDestinationUpdateTime = Time.time;
			_nextAttackDestinationUpdateInterval = Random.Range(0.5f, 1.5f);
			vector += Random.onUnitSphere.Flattened() * 4.5f * monster.chaseRandomness;
			if (isPredictedOnCast)
			{
				float num = Vector2.Distance(predictTarget.GetAIAgentPosition(_entity).ToXY(), _entity.agentPosition.ToXY());
				vector += predictTarget.AI.estimatedVelocity * (num / Mathf.Max(monster.Control.currentMaxAgentSpeed, 0.001f) * Random.value * monster.chasePredictiveness * 1.5f);
			}
		}
		Vector3 vector2 = (isPredictedOnCast ? (predictTarget.GetAIAgentPosition(_entity) + vector) : (_config.GetMoveToCastDestination(info) + vector));
		if (isMonsterChase)
		{
			vector2 = Dew.GetValidAgentDestination_LinearSweep(_entity.agentPosition, vector2);
			_lastReportedDestination = vector2;
		}
		return vector2;
	}

	public override float GetMoveDestinationRequiredDistance()
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Invalid comparison between Unknown and I4
		if (trigger.ignoreRangeCheck)
		{
			return float.PositiveInfinity;
		}
		float num = (isPredictedOnCast ? (_config.effectiveRange + predictTarget.Control.outerRadius) : _config.GetMoveToCastRequiredDistance(info));
		if (_entity is Monster && (Object)(object)predictTarget != null)
		{
			if (Time.time - _lastPathStatusTime > 0.5f)
			{
				_lastPathStatusTime = Time.time;
				_lastPathReachable = (int)Dew.GetNavMeshPathStatus(_entity.agentPosition, predictTarget.GetAIAgentPosition(_entity)) == 0;
			}
			if (!_lastPathReachable)
			{
				num = 0f;
			}
		}
		_lastReportedRequiredDistance = num;
		return num;
	}

	public override string ToString()
	{
		if (!isPredictedOnCast)
		{
			return string.Format("{0}({1})", "ActionCast", ((object)trigger).GetType());
		}
		return string.Format("{0}({1}, Predict:{2})", "ActionCast", ((object)trigger).GetType(), predictTarget.GetActorReadableName());
	}
}
