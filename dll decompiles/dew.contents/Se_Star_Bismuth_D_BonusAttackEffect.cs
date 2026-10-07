using System;
using Mirror;
using UnityEngine;

public class Se_Star_Bismuth_D_BonusAttackEffect : StarEffect
{
	public float healthPercentagePenalty;

	public StarScalingValue chance;

	private StatBonus _bonus;

	public override Type heroType => typeof(Hero_Bismuth);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			hero.EntityEvent_OnAttackHit += new Action<EventInfoAttackHit>(EntityEventOnAttackHit);
			UpdateStatBonus();
			hero.Skill.ClientHeroEvent_OnSkillEquip += new Action<SkillTrigger>(UpdateStatBonus);
			hero.Skill.ClientHeroEvent_OnSkillUnequip += new Action<SkillTrigger>(UpdateStatBonus);
		}
	}

	private void UpdateStatBonus(SkillTrigger _)
	{
		UpdateStatBonus();
	}

	private void UpdateStatBonus()
	{
		if (_bonus == null)
		{
			_bonus = DoStatBonus();
		}
		if (((Hero_Bismuth)hero).HasSameTravelerMemory())
		{
			_bonus.maxHealthPercentage = 0f - healthPercentagePenalty;
		}
		else
		{
			_bonus.maxHealthPercentage = 0f;
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)hero != null)
		{
			hero.EntityEvent_OnAttackHit -= new Action<EventInfoAttackHit>(EntityEventOnAttackHit);
			hero.Skill.ClientHeroEvent_OnSkillEquip -= new Action<SkillTrigger>(UpdateStatBonus);
			hero.Skill.ClientHeroEvent_OnSkillUnequip -= new Action<SkillTrigger>(UpdateStatBonus);
		}
	}

	private void EntityEventOnAttackHit(EventInfoAttackHit obj)
	{
		if (!(UnityEngine.Random.value > GetValue(chance)))
		{
			hero.CreateAbilityInstance<Ai_Star_Bismuth_D_BonusAttackEffect_Arrow>(hero.agentPosition, null, new CastInfo(hero, obj.victim));
		}
	}

	private void MirrorProcessed()
	{
	}
}
