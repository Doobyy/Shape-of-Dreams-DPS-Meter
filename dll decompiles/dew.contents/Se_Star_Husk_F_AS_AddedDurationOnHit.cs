using System;
using Mirror;
using UnityEngine;

public class Se_Star_Husk_F_AS_AddedDurationOnHit : StarEffect
{
	public float addedDuration = 0.2f;

	public float ultimateChargeGainRatio = 0.01f;

	public override Type heroType => typeof(Hero_Husk);

	public override Type skillType => typeof(St_R_AnnihilationStance);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			hero.ActorEvent_OnDealDamage += new Action<EventInfoDamage>(ActorEventOnDealDamage);
		}
	}

	private void ActorEventOnDealDamage(EventInfoDamage obj)
	{
		Actor actor = obj.actor;
		if ((!(actor is Ai_R_AnnihilationStance_Projectile) && !(actor is Ai_Atk_HuskSword) && !(actor is Ai_Atk_HuskSword_Crit) && !(actor is Ai_D_ScarOfTheWind_DashAtk)) || !hero.Status.TryGetStatusEffect<Se_R_AnnihilationStance>(out var effect))
		{
			return;
		}
		if (effect.remainingDuration.HasValue && effect.maxDuration.HasValue)
		{
			float num = effect.remainingDuration.Value + addedDuration;
			if (num > effect.maxDuration)
			{
				effect.SetTimer(num);
			}
			else
			{
				effect.SetTimer(effect.maxDuration.Value, num);
			}
		}
		foreach (AbilityTrigger value in hero.Ability.abilities.Values)
		{
			if (value is SkillTrigger { type: SkillType.Ultimate } skillTrigger)
			{
				ApplyCooldownReductionByRatio(skillTrigger, ultimateChargeGainRatio);
			}
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
