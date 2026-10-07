using System;
using Mirror;
using UnityEngine;

public class Se_Star_Husk_D_DamageOverTimeCritWithPenalty : StarEffect
{
	public float healReduction = 0.3f;

	public StarScalingValue damageRatio;

	public override Type heroType => typeof(Hero_Husk);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			hero.ActorEvent_OnDealDamage += new Action<EventInfoDamage>(ActorEventOnDealDamage);
			hero.takenHealProcessor.Add(Processor);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)hero != null)
		{
			hero.ActorEvent_OnDealDamage -= new Action<EventInfoDamage>(ActorEventOnDealDamage);
			hero.takenHealProcessor.Remove(Processor);
		}
	}

	private void Processor(ref HealData data, Actor actor, Entity target)
	{
		if (!data.IsAmountModifiedBy(this))
		{
			data.SetAmountModifiedBy(this);
			data.ApplyReduction(healReduction);
		}
	}

	private void ActorEventOnDealDamage(EventInfoDamage obj)
	{
		if ((obj.damage.attackEffectType == AttackEffectType.BasicAttackMain || obj.damage.attackEffectType == AttackEffectType.BasicAttackSub) && !obj.chain.DidReact(this) && !obj.victim.isDead && (obj.actor is Ai_Atk_HuskSword_Crit || obj.actor is Ai_D_ScarOfTheWind_DashAtk))
		{
			CreateStatusEffect(obj.victim, (Se_Star_Husk_D_DamageOverTimeCritWithPenalty_DoT se) =>
			{
				se.sourceDamage = obj.damage;
				se.totalDamage = (obj.damage.amount + obj.damage.discardedAmount) * GetValue(damageRatio);
			});
		}
	}

	private void MirrorProcessed()
	{
	}
}
