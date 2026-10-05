using System;
using DG.Tweening;
using Mirror;
using UnityEngine;

public class Mon_Ink_Archer : Monster, ISpawnableAsMiniBoss
{
	[NonSerialized]
	public Transform hatTransform;

	public Vector2 runDirRefreshTime;

	public Vector2 runDuration;

	public float maxDistance;

	private float _currentRunDuration;

	private float _runStartTime;

	private float _lastRunDirectionTime;

	private Vector3 _dir;

	private float _rotSpeed;

	private bool _isRunning;

	public override void OnModelLoaded()
	{
		base.OnModelLoaded();
		hatTransform = Visual.model.GetCustomMapping<Transform>("hatTransform");
	}

	protected override void AIUpdate(ref EntityAIContext context)
	{
		base.AIUpdate(ref context);
		if ((UnityEngine.Object)(object)context.targetEnemy == null)
		{
			return;
		}
		if (!AI.Helper_CanBeCast<At_Mon_Ink_Archer_Atk>() && !_isRunning)
		{
			StartRunning();
		}
		else if (Time.time - _runStartTime < _currentRunDuration)
		{
			if ((UnityEngine.Object)(object)context.targetEnemy == null || !context.targetEnemy.isActive || Vector2.Distance(context.targetEnemy.position.ToXY(), position.ToXY()) > maxDistance)
			{
				StopRunning();
			}
			else if (Time.time - _lastRunDirectionTime > UnityEngine.Random.Range(runDirRefreshTime.x, runDirRefreshTime.y))
			{
				_lastRunDirectionTime = Time.time;
				_dir = GetRunFromTargetDestination(context.targetEnemy);
				Control.MoveToDestination(_dir, immediately: false);
			}
		}
		else
		{
			_isRunning = false;
			if (AI.Helper_CanBeCast<At_Mon_Ink_Archer_Dodge>() && AI.Helper_IsTargetInRange<At_Mon_Ink_Archer_Dodge>())
			{
				AI.Helper_CastAbilityAuto<At_Mon_Ink_Archer_Dodge>();
			}
			else
			{
				AI.Helper_ChaseTarget();
			}
		}
	}

	protected override void OnDeath(EventInfoKill info)
	{
		base.OnDeath(info);
		ShortcutExtensions.DOKill((Component)hatTransform, false);
		hatTransform.localScale = Vector3.zero;
	}

	public void HideWeaponTemporarilyLocal()
	{
		ShortcutExtensions.DOKill((Component)hatTransform, false);
		TweenSettingsExtensions.SetId<Sequence>(TweenSettingsExtensions.Append(TweenSettingsExtensions.AppendInterval(TweenSettingsExtensions.Append(DOTween.Sequence(), (Tween)(object)ShortcutExtensions.DOScale(hatTransform, Vector3.zero, 0.1f)), 0.75f), (Tween)(object)ShortcutExtensions.DOScale(hatTransform, Vector3.one, 0.3f)), (object)hatTransform);
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
		float num = 10f;
		Vector3 normalized = (agentPosition - target.GetAIAgentPosition(this)).normalized;
		Vector3 vector = Quaternion.AngleAxis(UnityEngine.Random.Range(-20f, 20f), Vector3.up) * normalized;
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
			Status.AddStatBonus(new StatBonus
			{
				attackSpeedPercentage = 40f
			});
			Ability.GetAbility<At_Mon_Ink_Archer_Atk>().isMiniboss = true;
		}
	}

	private void MirrorProcessed()
	{
	}
}
