using System;
using UnityEngine;

public class Gem_R_Rejuvenation : Gem
{
	public GameObject healEffect;

	public ScalingValue conversionRatio;

	protected override void OnCastCompleteBeforePrepare(EventInfoCast info)
	{
		base.OnCastCompleteBeforePrepare(info);
		if (!IsReady() || !info.trigger.configs[info.configIndex].canConsumeCastBonus)
		{
			return;
		}
		Se_R_Rejuvenation_Timer timer = CreateStatusEffect<Se_R_Rejuvenation_Timer>(owner, new CastInfo(owner));
		info.instance.ActorEvent_OnDealDamage += (Action<EventInfoDamage>)((EventInfoDamage damage) =>
		{
			if (!timer.IsNullOrInactive() && isValid && !damage.chain.DidReact(this) && owner.CheckEnemyOrNeutral(damage.victim))
			{
				Heal(damage.damage.amount * GetValue(conversionRatio)).SetAmountOrigin(damage.damage).Dispatch(owner, damage.chain.New(this));
				FxPlayNewNetworked(healEffect, owner);
				NotifyUse();
			}
		});
		StartCooldown();
	}

	private void MirrorProcessed()
	{
	}
}
