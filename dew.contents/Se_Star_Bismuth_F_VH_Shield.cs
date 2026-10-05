using System;
using Mirror;
using UnityEngine;

public class Se_Star_Bismuth_F_VH_Shield : StarEffect
{
	public float shieldDuration = 2.5f;

	public float shieldRatio = 0.5f;

	public float cooldownPenalty = 1f;

	public float nonDuplicateBonusAmp = 1f;

	public override Type heroType => typeof(Hero_Bismuth);

	public override Type skillType => typeof(St_QR_ValiantHeart);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			hero.ActorEvent_OnDealDamage += new Action<EventInfoDamage>(ActorEventOnDealDamage);
			DoSkillBonusAll(new SkillBonus
			{
				cooldownOffset = cooldownPenalty
			});
		}
	}

	private void ActorEventOnDealDamage(EventInfoDamage obj)
	{
		if (obj.actor is Ai_QR_ValiantHeart)
		{
			float num = (obj.damage.amount + obj.damage.discardedAmount) * shieldRatio;
			if (!((Hero_Bismuth)hero).HasSameTravelerMemory())
			{
				num *= 1f + nonDuplicateBonusAmp;
			}
			GiveShield(hero, num, shieldDuration);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)hero != null)
		{
			hero.ActorEvent_OnDealDamage -= new Action<EventInfoDamage>(ActorEventOnDealDamage);
		}
	}

	private void MirrorProcessed()
	{
	}
}
