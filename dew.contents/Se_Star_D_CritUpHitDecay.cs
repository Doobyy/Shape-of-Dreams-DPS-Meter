using System;
using Mirror;
using UnityEngine;

public class Se_Star_D_CritUpHitDecay : StarEffect
{
	public StarScalingValue critChanceAmp;

	public int decayCritPerHit;

	public float gracePeriod = 0.55f;

	private StatBonus _bonus;

	private float _lastCheckTime;

	[SaveVar(SaveVarFlags.Default)]
	private int _currentDecayCritAmount;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			_bonus = DoStatBonus();
			UpdateBonus();
			victim.EntityEvent_OnTakeDamage += new Action<EventInfoDamage>(OnTakeDamage);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)victim != null)
		{
			victim.EntityEvent_OnTakeDamage -= new Action<EventInfoDamage>(OnTakeDamage);
		}
	}

	private void OnTakeDamage(EventInfoDamage damage)
	{
		if (!(Time.time - _lastCheckTime < gracePeriod))
		{
			Entity entity = damage.actor.firstEntity;
			if (!entity.IsNullInactiveDeadOrKnockedOut() && victim.CheckEnemyOrNeutral(entity) && !(damage.actor is Se_Elm_Fire))
			{
				_currentDecayCritAmount += decayCritPerHit;
				int max = Mathf.RoundToInt(GetValue(critChanceAmp) * 100f);
				_currentDecayCritAmount = Mathf.Clamp(_currentDecayCritAmount, 0, max);
				UpdateBonus();
				_lastCheckTime = Time.time;
			}
		}
	}

	private void UpdateBonus()
	{
		_bonus.critChanceFlat = GetValue(critChanceAmp) - (float)_currentDecayCritAmount * 0.01f;
	}

	private void MirrorProcessed()
	{
	}
}
