using System;
using UnityEngine;

[AchUnlockOnComplete(typeof(St_C_SparklingWaterGun))]
public class ACH_PEW_PEW_PEW : DewAchievementItem
{
	private const int RequiredCount = 200;

	[AchPersistentVar]
	private int _count;

	public override int GetMaxProgress()
	{
		return 200;
	}

	public override int GetCurrentProgress()
	{
		return _count;
	}

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnActorAdd += new Action<Actor>(OnActorAdd);
	}

	public override void OnStopLocalClient()
	{
		base.OnStopLocalClient();
		if ((UnityEngine.Object)(object)NetworkedManagerBase<ActorManager>.instance != null)
		{
			NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnActorAdd -= new Action<Actor>(OnActorAdd);
		}
	}

	private void OnActorAdd(Actor obj)
	{
		if (obj is StatusEffect { isBeneficialBuff: not false } statusEffect && !((UnityEngine.Object)(object)statusEffect.victim == (UnityEngine.Object)(object)hero) && !hero.CheckEnemyOrNeutral(statusEffect.victim) && statusEffect.IsDescendantOf(hero))
		{
			_count++;
			if (_count >= 200)
			{
				Complete();
			}
		}
	}
}
