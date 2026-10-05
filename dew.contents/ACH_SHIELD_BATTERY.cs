using System;
using UnityEngine;

[AchUnlockOnComplete(typeof(Gem_R_Snow))]
public class ACH_SHIELD_BATTERY : DewAchievementItem
{
	private const int RequiredAmount = 20000;

	[AchPersistentVar]
	private float _negatedAmount;

	public override int GetMaxProgress()
	{
		return 20000;
	}

	public override int GetCurrentProgress()
	{
		return Mathf.FloorToInt(_negatedAmount);
	}

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		NetworkedManagerBase<ClientEventManager>.instance.OnDamageNegatedByShield += new Action<EventInfoDamageNegatedByShield>(EntityEventOnDamageNegatedByShield);
	}

	public override void OnStopLocalClient()
	{
		base.OnStopLocalClient();
		if ((UnityEngine.Object)(object)NetworkedManagerBase<ClientEventManager>.instance != null)
		{
			NetworkedManagerBase<ClientEventManager>.instance.OnDamageNegatedByShield -= new Action<EventInfoDamageNegatedByShield>(EntityEventOnDamageNegatedByShield);
		}
	}

	private void EntityEventOnDamageNegatedByShield(EventInfoDamageNegatedByShield obj)
	{
		if ((UnityEngine.Object)(object)obj.victim != (UnityEngine.Object)(object)hero || (UnityEngine.Object)(object)obj.actor == null)
		{
			return;
		}
		Entity firstEntity = obj.actor.firstEntity;
		if (!((UnityEngine.Object)(object)firstEntity == null) && hero.CheckEnemyOrNeutral(firstEntity))
		{
			_negatedAmount += obj.negatedAmount;
			if (_negatedAmount > 20000f)
			{
				Complete();
			}
		}
	}
}
