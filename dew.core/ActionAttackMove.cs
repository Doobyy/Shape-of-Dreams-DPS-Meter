using System.Collections.Generic;
using UnityEngine;

public class ActionAttackMove : ActionBase
{
	private const float AttackMoveCheckInterval = 0.2f;

	private const float AttackMoveAcquisitionBonusRange = 3f;

	private const float AttackMoveAcquisitionRangeMinimum = 6f;

	private static readonly IComparer<Entity> SortComparer = Comparer<Entity>.Create((Entity x, Entity y) => (Vector2.Distance(_sortPivot, x.agentPosition.ToXY()) - x.Control.outerRadius).CompareTo(Vector2.Distance(_sortPivot, y.agentPosition.ToXY()) - y.Control.outerRadius));

	private static Vector2 _sortPivot;

	public Vector3 destination;

	public bool useDistanceFromDestination;

	private float _lastAttackMoveCheckTime = float.NegativeInfinity;

	public override bool Tick()
	{
		if (isFirstTick)
		{
			if (_entity.Control.IsActionBlocked(EntityControl.BlockableAction.Move) == EntityControl.BlockStatus.BlockedCancelable)
			{
				_entity.Control.DisobeyBlock(EntityControl.BlockableAction.Move);
			}
			if (_entity.Control.IsActionBlocked(EntityControl.BlockableAction.Attack) == EntityControl.BlockStatus.BlockedCancelable)
			{
				_entity.Control.DisobeyBlock(EntityControl.BlockableAction.Attack);
			}
		}
		if (Vector2.Distance(destination.ToXY(), _entity.agentPosition.ToXY()) < 0.15f)
		{
			return true;
		}
		if (_entity.Status.hasStun)
		{
			return false;
		}
		if (Time.time - _lastAttackMoveCheckTime < 0.2f)
		{
			return false;
		}
		_lastAttackMoveCheckTime = Time.time;
		Entity entity = FindAttackMoveTarget(_entity, useDistanceFromDestination ? destination.ToXY() : _entity.agentPosition.ToXY());
		if ((Object)(object)entity != null)
		{
			_entity.Control.Attack(entity, doChase: true);
			return true;
		}
		return false;
	}

	public static Entity FindAttackMoveTarget(Entity ent, Vector3 sortPivot)
	{
		if ((Object)(object)ent.Ability.attackAbility == null)
		{
			return null;
		}
		float radius = Mathf.Max(ent.Ability.attackAbility.currentConfig.effectiveRange + 3f, 6f);
		_sortPivot = sortPivot;
		ListReturnHandle<Entity> handle;
		foreach (Entity item in DewPhysics.OverlapCircleAllEntities(out handle, ent.agentPosition, radius, ent.Ability.attackAbility.currentConfig.targetValidator, ent, new CollisionCheckSettings
		{
			sortComparer = SortComparer
		}))
		{
			if (!item.AI.excludeFromAutoTargeting)
			{
				handle.Return();
				return item;
			}
		}
		handle.Return();
		return null;
	}

	public override Vector3? GetMoveDestination()
	{
		return destination;
	}

	public override float GetMoveDestinationRequiredDistance()
	{
		return 0.15f;
	}

	public override bool ShouldCancelIfDisallowedToMove()
	{
		return true;
	}

	public override string ToString()
	{
		return "ActionAttackMove()";
	}
}
