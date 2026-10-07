using System;
using System.Collections.Generic;
using UnityEngine;

public class Star_Vesper_SanctuaryOfElHeal : DewHeroStarItemOld
{
	public static readonly float HealLostHealthRatio = 0.15f;

	public static readonly float SelfMultiplier = 2f;

	public override Type heroType => typeof(Hero_Vesper);

	public override Type affectedSkill => typeof(St_R_SanctuaryOfEl);

	public override bool ShouldInitInGame()
	{
		if (base.ShouldInitInGame())
		{
			return isServer;
		}
		return false;
	}

	public override void OnStartInGame()
	{
		base.OnStartInGame();
		hero.ActorEvent_OnAbilityInstanceBeforePrepare += new Action<EventInfoAbilityInstance>(EmpowerMainAbilityInstance);
	}

	private void EmpowerMainAbilityInstance(EventInfoAbilityInstance obj)
	{
		if (!(obj.instance is Ai_R_SanctuaryOfEl_Ground))
		{
			return;
		}
		List<Entity> appliedEntities = new List<Entity>();
		obj.instance.ActorEvent_OnAbilityInstanceCreated += (Action<EventInfoAbilityInstance>)((EventInfoAbilityInstance info) =>
		{
			AbilityInstance instance = info.instance;
			Se_R_SanctuaryOfEl_Buff buff = instance as Se_R_SanctuaryOfEl_Buff;
			if (buff != null && !appliedEntities.Contains(buff.victim))
			{
				appliedEntities.Add(buff.victim);
				info.instance.CreateStatusEffect(buff.victim, new CastInfo(buff.info.caster, buff.victim), (Se_GenericHealOverTime heal) =>
				{
					heal.ticks = 5;
					heal.tickInterval = 0.2f;
					float num = HealLostHealthRatio * buff.victim.Status.missingHealth;
					if ((UnityEngine.Object)(object)buff.victim == (UnityEngine.Object)(object)hero)
					{
						num *= SelfMultiplier;
					}
					heal.totalAmount = num;
				});
			}
		});
	}
}
