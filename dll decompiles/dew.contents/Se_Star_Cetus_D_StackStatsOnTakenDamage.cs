using System;
using Mirror;
using UnityEngine;

public class Se_Star_Cetus_D_StackStatsOnTakenDamage : StarEffect
{
	public StarScalingValue maxBonusCount;

	public float stackDuration = 5f;

	public float bonusAmount = 1f;

	public float cooldown = 0.1f;

	private float _lastCheckTime;

	private float _expireTime;

	private int _currentStacks;

	private StatBonus _bonus;

	public override Type heroType => typeof(Hero_Cetus);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			victim.EntityEvent_OnTakeDamage += new Action<EventInfoDamage>(OnTakeDamage);
			_bonus = DoStatBonus();
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && !victim.IsNullOrInactive())
		{
			victim.EntityEvent_OnTakeDamage -= new Action<EventInfoDamage>(OnTakeDamage);
		}
	}

	private void OnTakeDamage(EventInfoDamage obj)
	{
		if (!(Time.time - _lastCheckTime < cooldown))
		{
			_lastCheckTime = Time.time;
			if (_currentStacks < GetValueInt(maxBonusCount))
			{
				_currentStacks++;
			}
			_expireTime = Time.time + stackDuration;
			showIcon = true;
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer)
		{
			if (_currentStacks > 0 && Time.time > _expireTime)
			{
				_currentStacks = 0;
				showIcon = false;
			}
			numberDisplay = _currentStacks;
			normalizedFillAmount = Mathf.InverseLerp(_expireTime, _lastCheckTime, Time.time);
			float num = (float)_currentStacks * bonusAmount;
			if (!Mathf.Approximately(_bonus.abilityPowerFlat, num))
			{
				_bonus.abilityHasteFlat = num;
				_bonus.abilityPowerFlat = num;
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
