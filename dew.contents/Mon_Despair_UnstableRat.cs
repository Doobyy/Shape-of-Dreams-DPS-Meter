using Mirror;
using UnityEngine;

public class Mon_Despair_UnstableRat : Monster
{
	public float runDirRefreshTime = 1f;

	public Vector2 runDuration;

	public float maxDistance;

	private float _currentRunDuration;

	private float _runStartTime;

	private float _lastRunDirectionTime;

	private Vector3 _dir;

	private float _rotSpeed;

	private bool _isRunning;

	protected override void AIUpdate(ref EntityAIContext context)
	{
		base.AIUpdate(ref context);
		if ((Object)(object)context.targetEnemy == null)
		{
			return;
		}
		if (!AI.Helper_CanBeCast<At_Mon_Despair_UnstableRat_Atk>() && !_isRunning)
		{
			StartRunning();
		}
		else if (Time.time - _runStartTime < _currentRunDuration)
		{
			if ((Object)(object)context.targetEnemy == null || !context.targetEnemy.isActive || Vector2.Distance(context.targetEnemy.position.ToXY(), position.ToXY()) > maxDistance)
			{
				StopRunning();
			}
			else if (Time.time - _lastRunDirectionTime > runDirRefreshTime)
			{
				_lastRunDirectionTime = Time.time;
				_dir = GetRunFromTargetDestination(context.targetEnemy);
				Control.MoveToDestination(_dir, immediately: false);
			}
		}
		else
		{
			_isRunning = false;
			AI.Helper_ChaseTarget();
		}
	}

	protected override void OnDeath(EventInfoKill info)
	{
		base.OnDeath(info);
		if (((NetworkBehaviour)this).isServer)
		{
			CreateAbilityInstance<Ai_Mon_Despair_UnstableRat_Explosion>(Dew.GetPositionOnGround(position), null, new CastInfo(this));
		}
	}

	public void StartRunning()
	{
		_isRunning = true;
		_runStartTime = Time.time;
		_currentRunDuration = Random.Range(runDuration.x, runDuration.y);
	}

	public void StopRunning()
	{
		_isRunning = false;
		_runStartTime = float.NegativeInfinity;
		if ((Object)(object)AI.context.targetEnemy != null)
		{
			Control.Attack(AI.context.targetEnemy, doChase: true);
		}
	}

	private Vector3 GetRunFromTargetDestination(Entity target)
	{
		if ((Object)(object)target == null)
		{
			target = Dew.SelectRandomAliveHero(fallbackToDead: true, skipStealthed: true);
		}
		float num = 5f;
		Vector3 normalized = (agentPosition - target.GetAIAgentPosition(this)).normalized;
		Vector3 vector = Quaternion.AngleAxis(Random.Range(-45f, 45f), Vector3.up) * normalized;
		Vector3 vector2 = agentPosition + vector * num;
		Vector3 validAgentDestination_LinearSweep = Dew.GetValidAgentDestination_LinearSweep(agentPosition, vector2);
		if (Vector3.SqrMagnitude(vector2) > Vector3.SqrMagnitude(validAgentDestination_LinearSweep))
		{
			validAgentDestination_LinearSweep = Dew.GetValidAgentDestination_LinearSweep(agentPosition, Vector3.Reflect(-validAgentDestination_LinearSweep, Vector3.forward));
		}
		return validAgentDestination_LinearSweep;
	}

	private void MirrorProcessed()
	{
	}
}
