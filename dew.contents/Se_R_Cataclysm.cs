using System;
using Mirror;
using UnityEngine;

public class Se_R_Cataclysm : StatusEffect
{
	public ScalingValue shieldAmount;

	public float duration;

	public ScalingValue bonusApPercent;

	public int reuseCount = 3;

	public bool doUnstoppable;

	[NonSerialized]
	public bool increaseAdInstead;

	[NonSerialized]
	public float increaseStatMultiplier = 1f;

	private int _currentUseCount;

	private AbilityTrigger.ChangedConfigHandle _handle;

	private StatBonus _bonus;

	private int _baseReuseCount;

	private float _baseDuration;

	public override bool reuseInRoom => true;

	protected override void Awake()
	{
		base.Awake();
		_baseReuseCount = reuseCount;
		_baseDuration = duration;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		reuseCount = _baseReuseCount;
		duration = _baseDuration;
		increaseAdInstead = false;
		_currentUseCount = 0;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			_handle = firstTrigger.ChangeConfigTimed(1, duration, OnUse, Destroy, setFillAmount: false);
			GiveShield(victim, GetValue(shieldAmount), duration);
			SetTimer(duration);
			ShowOnScreenTimer();
			if ((bool)(UnityEngine.Object)(object)firstTrigger)
			{
				firstTrigger.fillAmount = 1f;
			}
			if (doUnstoppable)
			{
				DoUnstoppable();
			}
			if (increaseAdInstead)
			{
				_bonus = new StatBonus
				{
					attackDamagePercentage = GetValue(bonusApPercent) * increaseStatMultiplier
				};
			}
			else
			{
				_bonus = new StatBonus
				{
					abilityPowerPercentage = GetValue(bonusApPercent) * increaseStatMultiplier
				};
			}
			victim.Status.AddStatBonus(_bonus);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			if (_handle.isActive)
			{
				_handle.Stop();
			}
			if ((UnityEngine.Object)(object)victim != null && _bonus != null)
			{
				victim.Status.RemoveStatBonus(_bonus);
			}
			if ((bool)(UnityEngine.Object)(object)firstTrigger)
			{
				firstTrigger.fillAmount = 0f;
			}
		}
	}

	private void OnUse(EventInfoAbilityInstance obj)
	{
		CreateAbilityInstance<Ai_R_Cataclysm_Meteor>(obj.instance.info.point, null, new CastInfo(info.caster));
		_currentUseCount++;
		if ((bool)(UnityEngine.Object)(object)firstTrigger)
		{
			firstTrigger.fillAmount = 1f - (float)_currentUseCount / (float)reuseCount;
		}
		if (_currentUseCount >= reuseCount && _handle.isActive)
		{
			_handle.Stop();
		}
	}

	private void MirrorProcessed()
	{
	}
}
