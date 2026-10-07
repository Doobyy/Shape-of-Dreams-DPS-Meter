using System;
using Mirror;
using UnityEngine;

public class Se_Star_Bismuth_L_ContinuousSpeed : StarEffect
{
	public StarScalingValue maxSpeedAmount;

	public float attackTime = 5f;

	public float decayTime = 0.5f;

	public float gracePeriod = 0.25f;

	public float nonDuplicateBonusAmp = 0.3f;

	private SpeedEffect _speed;

	private float _lastMovingTime;

	private float _currentDecayedTime;

	public override Type heroType => typeof(Hero_Bismuth);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			_speed = DoSpeed(0f);
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!((NetworkBehaviour)this).isServer || _speed == null)
		{
			return;
		}
		bool num = hero.Control.isWalking || (hero.Control.isDisplacing && hero.Control.ongoingDisplacement.isFriendly);
		float num2 = GetValue(maxSpeedAmount);
		if (!((Hero_Bismuth)hero).HasSameTravelerMemory())
		{
			num2 *= 1f + nonDuplicateBonusAmp;
		}
		if (num)
		{
			_lastMovingTime = Time.time;
		}
		float a;
		if (num || Time.time - _lastMovingTime < gracePeriod)
		{
			_currentDecayedTime = 0f;
			normalizedFillAmount = 1f;
			a = Mathf.MoveTowards(_speed.strength, num2, num2 / attackTime * dt);
			showIcon = true;
		}
		else
		{
			_currentDecayedTime += dt;
			a = Mathf.MoveTowards(_speed.strength, 0f, num2 / decayTime * dt);
			normalizedFillAmount = Mathf.InverseLerp(decayTime, 0f, _currentDecayedTime);
			if (normalizedFillAmount <= 0f)
			{
				_currentDecayedTime = 0f;
				showIcon = false;
			}
		}
		if (!Mathf.Approximately(a, _speed.strength))
		{
			_speed.strength = a;
		}
	}

	private void MirrorProcessed()
	{
	}
}
