using Mirror;
using UnityEngine;

public class Mon_Sky_Baam : Monster, ISpawnableAsMiniBoss
{
	public float runDirRefreshTime = 1f;

	public Vector2 runDuration;

	public float maxDistance;

	public float teleportcChance;

	private float _currentRunDuration;

	private float _runStartTime;

	private float _lastRunDirectionTime;

	private Vector3 _dir;

	protected override void AIUpdate(ref EntityAIContext context)
	{
		base.AIUpdate(ref context);
		if ((Object)(object)context.targetEnemy == null)
		{
			return;
		}
		if (Time.time - _runStartTime < _currentRunDuration)
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
		else if (AI.Helper_CanBeCast<At_Mon_Sky_Baam_Teleport>() && AI.Helper_IsTargetInRange<At_Mon_Sky_Baam_Teleport>() && Random.value <= teleportcChance)
		{
			AI.Helper_CastAbilityAuto<At_Mon_Sky_Baam_Teleport>();
		}
		else
		{
			AI.Helper_ChaseTarget();
		}
	}

	public void StartRunning()
	{
		_runStartTime = Time.time;
		_currentRunDuration = Random.Range(runDuration.x, runDuration.y);
	}

	public void StopRunning()
	{
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

	public void OnBeforeSpawnAsMiniBoss()
	{
	}

	public void OnCreateAsMiniBoss()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			ISpawnableAsMiniBoss.GiveGenericMiniBossBonus(this);
			runDuration = Vector2.zero;
			At_Mon_Sky_Baam_Teleport ability = Ability.GetAbility<At_Mon_Sky_Baam_Teleport>();
			ability.configs[0].cooldownTime = 18f;
			ability.configs[0].maxCharges = 3;
			ability.configs[0].addedCharges = 3;
		}
	}

	private void MirrorProcessed()
	{
	}
}
