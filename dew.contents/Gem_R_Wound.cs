using System;
using UnityEngine;

public class Gem_R_Wound : Gem
{
	protected override void OnDealDamage(EventInfoDamage info)
	{
		base.OnDealDamage(info);
		if ((UnityEngine.Object)(object)owner == null || (UnityEngine.Object)(object)info.actor == null || info.chain.DidReact(this) || info.victim.IsNullInactiveDeadOrKnockedOut() || !owner.CheckEnemyOrNeutral(info.victim))
		{
			return;
		}
		Se_Gem_R_Wound_Wounded se_Gem_R_Wound_Wounded = info.victim.Status.FindStatusEffect((Se_Gem_R_Wound_Wounded w) => (UnityEngine.Object)(object)w.info.caster == (UnityEngine.Object)(object)owner);
		if ((UnityEngine.Object)(object)se_Gem_R_Wound_Wounded != null)
		{
			se_Gem_R_Wound_Wounded.ResetTimer();
			return;
		}
		CreateStatusEffectWithSource(info.actor, info.victim, new CastInfo(owner), (Se_Gem_R_Wound_Wounded w) =>
		{
			w.chain = info.chain.New(this);
		}).ClientActorEvent_OnDestroyed += (Action<Actor>)((Actor _) =>
		{
			NotifyUse();
		});
		NotifyUse();
	}

	private void MirrorProcessed()
	{
	}
}
