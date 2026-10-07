using UnityEngine;

public class AttackTrigger : AbilityTrigger
{
	public bool allowNonTargetedCast = true;

	private int _nextEveryFourAttackIndex;

	private float _randomValue;

	protected override void Awake()
	{
		base.Awake();
		_randomValue = Random.value;
	}

	public override float ProcessRange(float original)
	{
		if ((Object)(object)owner == null)
		{
			return original;
		}
		original += owner.Status.bonusStats.attackRangeFlat;
		original *= 1f + owner.Status.bonusStats.attackRangePercentage * 0.01f;
		return original;
	}

	public override float GetCooldownTimeMultiplier(int configIndex)
	{
		if ((Object)(object)owner == null)
		{
			return 1f;
		}
		return 1f / owner.Status.attackSpeedMultiplier;
	}

	public override float GetAnimationSpeed()
	{
		if ((Object)(object)owner == null)
		{
			return 1f;
		}
		return owner.Status.attackSpeedMultiplier;
	}

	public override float GetChannelDurationMultiplier()
	{
		if ((Object)(object)owner == null)
		{
			return 1f;
		}
		return 1f / owner.Status.attackSpeedMultiplier;
	}

	public override float GetPostDelayDurationMultiplier()
	{
		if ((Object)(object)owner == null)
		{
			return 1f;
		}
		return 1f / owner.Status.attackSpeedMultiplier;
	}

	public void UpdateConfigIndexForCrit()
	{
		if (configs.Length > 1 && (owner.Status.hasAttackCritical || _randomValue < owner.Status.critChance))
		{
			currentConfigIndex = 1;
		}
		else if (!owner.Status.hasAttackCritical)
		{
			currentConfigIndex = 0;
		}
	}

	public override void OnCastStart(int configIndex, CastInfo info)
	{
		UpdateConfigIndexForCrit();
		base.OnCastStart(currentConfigIndex, info);
	}

	public override AbilityInstance OnCastComplete(int configIndex, CastInfo info)
	{
		AbilityInstance abilityInstance = base.OnCastComplete(configIndex, info);
		CallAttackCompleteRoutines(abilityInstance, configIndex, info);
		return abilityInstance;
	}

	public void CallAttackCompleteRoutines(AbilityInstance newInstance, int configIndex, CastInfo info)
	{
		int num = Mathf.Min(owner.Status.bonusStats.everyFourAttackStartIndexFlat, 3);
		if (_nextEveryFourAttackIndex < num)
		{
			_nextEveryFourAttackIndex = num;
		}
		owner.EntityEvent_OnAttackFired?.Invoke(new EventInfoAttackFired
		{
			actor = this,
			instance = newInstance,
			info = info,
			isCrit = (configIndex == 1),
			isThisAttackFourthAttack = (_nextEveryFourAttackIndex == 3),
			isNextAttackFourthAttack = (_nextEveryFourAttackIndex == 2 || num >= 3),
			everyFourAttackNormalizedProgress = ((num >= 3) ? 1f : ((float)((_nextEveryFourAttackIndex + 1 - num) % (4 - num)) / (3f - (float)num)))
		});
		_nextEveryFourAttackIndex++;
		if (_nextEveryFourAttackIndex > 3)
		{
			_nextEveryFourAttackIndex = num;
		}
		_randomValue = Random.value;
	}

	public override void OnCastCompleteBeforePrepare(EventInfoCast cast)
	{
		base.OnCastCompleteBeforePrepare(cast);
		CallAttackCompleteBeforePrepareRoutines(cast);
	}

	public void CallAttackCompleteBeforePrepareRoutines(EventInfoCast cast)
	{
		if (cast.instance is MeleeAttackInstance meleeAttackInstance)
		{
			meleeAttackInstance.isCrit = cast.configIndex == 1;
		}
		if (cast.instance is AttackProjectile attackProjectile)
		{
			attackProjectile.isCrit = cast.configIndex == 1;
		}
		int num = Mathf.Min(owner.Status.bonusStats.everyFourAttackStartIndexFlat, 3);
		if (_nextEveryFourAttackIndex < num)
		{
			_nextEveryFourAttackIndex = num;
		}
		owner.EntityEvent_OnAttackFiredBeforePrepare?.Invoke(new EventInfoAttackFired
		{
			actor = this,
			instance = cast.instance,
			info = cast.info,
			isCrit = (cast.configIndex == 1),
			isThisAttackFourthAttack = (_nextEveryFourAttackIndex == 3),
			isNextAttackFourthAttack = (_nextEveryFourAttackIndex == 2 || owner.Status.bonusStats.everyFourAttackStartIndexFlat >= 3),
			everyFourAttackNormalizedProgress = ((num >= 3) ? 1f : ((float)((_nextEveryFourAttackIndex + 1 - num) % (4 - num)) / (3f - (float)num)))
		});
	}

	public override void OnCastCompleteSetMinimumDelay(int configIndex, CastInfo info)
	{
		SetMinimumDelayAll(configs[configIndex].minimumDelay);
	}

	public override void OnCastCompleteSetCooldownTime(int configIndex, CastInfo info)
	{
		SetCooldownTimeAll(GetMaxCooldownTime(configIndex, scaled: false) - configs[configIndex].channel.duration, scaled: false);
	}

	public override void OnCastCompleteSetCharge(int configIndex, CastInfo info)
	{
		SetChargeAll(currentCharges[configIndex] - 1);
	}

	protected override void OnCastCancelSetCooldownTime(int configIndex, CastInfo info)
	{
		SetCooldownTimeAll(0f, scaled: false);
	}

	protected override void OnStartChannel(int configIndex, CastInfo info)
	{
		TriggerConfig triggerConfig = configs[configIndex];
		Channel channel = ((triggerConfig.castMethod.type != CastMethodType.Target || info.target == null) ? triggerConfig.channel.CreateChannel(() =>
		{
			OnCastComplete(configIndex, info);
		}, () =>
		{
			OnCastCancel(configIndex, info);
		}, triggerConfig.selfValidator) : triggerConfig.channel.CreateChannel(() =>
		{
			OnCastComplete(configIndex, info);
		}, () =>
		{
			OnCastCancel(configIndex, info);
		}, triggerConfig.selfValidator, info.target, triggerConfig.targetValidator));
		channel.duration *= GetChannelDurationMultiplier();
		if ((Object)(object)owner.owner != null && owner.owner.isHumanPlayer)
		{
			channel.blockedActions &= ~Channel.BlockedAction.Attack;
			channel.blockedActions &= ~Channel.BlockedAction.Move;
			channel.blockedActions |= Channel.BlockedAction.Cancelable;
			owner.Control.SetAttackMovSpdDisadvantage(channel.duration);
		}
		channel.isAttack = true;
		channel.AddValidation(() => !this.IsNullOrInactive() && !owner.IsNullInactiveDeadOrKnockedOut());
		owner.Control.StartChannel(channel);
	}

	protected override void OnRotateForward(int configIndex, CastInfo info)
	{
		TriggerConfig triggerConfig = configs[configIndex];
		if (triggerConfig.castMethod.type == CastMethodType.Target && info.target == null)
		{
			owner.Control.Rotate(info.rotation, immediately: false, triggerConfig.overrideRotation ? triggerConfig.overrideRotationDuration : (-1f));
			return;
		}
		switch (triggerConfig.castMethod.type)
		{
		case CastMethodType.Cone:
		case CastMethodType.Arrow:
			owner.Control.Rotate(info.rotation, immediately: false, triggerConfig.overrideRotation ? triggerConfig.overrideRotationDuration : (-1f));
			break;
		case CastMethodType.Target:
			owner.Control.RotateTowards(info.target, immediately: false, triggerConfig.overrideRotation ? triggerConfig.overrideRotationDuration : (-1f));
			break;
		case CastMethodType.Point:
			owner.Control.RotateTowards(info.point, immediately: false, triggerConfig.overrideRotation ? triggerConfig.overrideRotationDuration : (-1f));
			break;
		case CastMethodType.None:
			break;
		}
	}

	private void MirrorProcessed()
	{
	}
}
