using System;
using System.Collections;
using Mirror;
using UnityEngine;

public class Mon_SnowMountain_LivingShards : Monster
{
	public RotateGameObject rot;

	public GameObject model;

	public float runDirRefreshTime = 1f;

	public Vector2 runDuration;

	public float maxDistance;

	private float _currentRunDuration;

	private float _runStartTime;

	private float _lastRunDirectionTime;

	private Vector3 _dir;

	private float _rotSpeed;

	private bool _isRunning;

	private float _baseRotSpeedY;

	protected override void Awake()
	{
		base.Awake();
		_baseRotSpeedY = rot.rot_speed_y;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			_rotSpeed = 15f;
			ActorEvent_OnAbilityInstanceCreated += new Action<EventInfoAbilityInstance>(OnAttackRoutine);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			rot.rot_speed_y = _baseRotSpeedY;
			ActorEvent_OnAbilityInstanceCreated -= new Action<EventInfoAbilityInstance>(OnAttackRoutine);
		}
	}

	private void LateUpdate()
	{
		model.transform.rotation = Quaternion.identity;
	}

	protected override void AIUpdate(ref EntityAIContext context)
	{
		base.AIUpdate(ref context);
		if ((UnityEngine.Object)(object)context.targetEnemy == null)
		{
			return;
		}
		if (!AI.Helper_CanBeCast<At_Mon_SnowMountain_LivingShards_Atk>() && !_isRunning)
		{
			StartRunning();
		}
		else if (Time.time - _runStartTime < _currentRunDuration)
		{
			if ((UnityEngine.Object)(object)context.targetEnemy == null || !context.targetEnemy.isActive || Vector2.Distance(context.targetEnemy.position.ToXY(), position.ToXY()) > maxDistance)
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

	private void OnAttackRoutine(EventInfoAbilityInstance info)
	{
		((MonoBehaviour)(object)this).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			if (((NetworkBehaviour)this).isServer && info.actor is At_Mon_SnowMountain_LivingShards_Atk)
			{
				RotateGameObject val = rot;
				val.rot_speed_y *= _rotSpeed;
				yield return new WaitForSeconds(0.8f);
				RotateGameObject val2 = rot;
				val2.rot_speed_y /= _rotSpeed;
			}
		}
	}

	public void StartRunning()
	{
		_isRunning = true;
		_runStartTime = Time.time;
		_currentRunDuration = UnityEngine.Random.Range(runDuration.x, runDuration.y);
	}

	public void StopRunning()
	{
		_isRunning = false;
		_runStartTime = float.NegativeInfinity;
		if ((UnityEngine.Object)(object)AI.context.targetEnemy != null)
		{
			Control.Attack(AI.context.targetEnemy, doChase: true);
		}
	}

	private Vector3 GetRunFromTargetDestination(Entity target)
	{
		if ((UnityEngine.Object)(object)target == null)
		{
			target = Dew.SelectRandomAliveHero(fallbackToDead: true, skipStealthed: true);
		}
		float num = 5f;
		Vector3 normalized = (agentPosition - target.GetAIAgentPosition(this)).normalized;
		Vector3 vector = Quaternion.AngleAxis(UnityEngine.Random.Range(-45f, 45f), Vector3.up) * normalized;
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
