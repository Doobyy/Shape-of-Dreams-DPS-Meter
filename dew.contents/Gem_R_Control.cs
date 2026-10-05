using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Gem_R_Control : Gem
{
	protected override void OnDealDamage(EventInfoDamage info)
	{
		base.OnDealDamage(info);
		if (!((NetworkBehaviour)this).isServer || info.chain.DidReact(this) || (Object)(object)owner == null || (Object)(object)info.victim == null || !owner.CheckEnemyOrNeutral(info.victim))
		{
			return;
		}
		Entity victim = info.victim;
		if (victim.Status.hasCrowdControlImmunity)
		{
			return;
		}
		List<StatusEffect> statusEffects = victim.Status.statusEffects;
		for (int i = 0; i < statusEffects.Count; i++)
		{
			if (statusEffects[i] is Se_Gem_R_Control_Debuff se_Gem_R_Control_Debuff && (Object)(object)se_Gem_R_Control_Debuff.parentActor == (Object)(object)this)
			{
				se_Gem_R_Control_Debuff.ResetTimer();
				NotifyUse();
				return;
			}
		}
		CreateStatusEffect<Se_Gem_R_Control_Debuff>(victim, new CastInfo(owner, victim));
		NotifyUse();
	}

	private void MirrorProcessed()
	{
	}
}
