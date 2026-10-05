using System;
using Mirror;
using UnityEngine;

public class Se_Star_Cetus_D_ApUpByColdSkillCounts : StarEffect
{
	public StarScalingValue addedFlat;

	public StarScalingValue bonusPerSkill;

	private StatBonus _bonus;

	public override Type heroType => typeof(Hero_Cetus);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			_bonus = DoStatBonus();
			hero.Skill.ClientHeroEvent_OnSkillEquip += new Action<SkillTrigger>(Refresh);
			hero.Skill.ClientHeroEvent_OnSkillUnequip += new Action<SkillTrigger>(Refresh);
			Refresh(null);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (bool)(UnityEngine.Object)(object)hero)
		{
			hero.Skill.ClientHeroEvent_OnSkillEquip -= new Action<SkillTrigger>(Refresh);
			hero.Skill.ClientHeroEvent_OnSkillUnequip -= new Action<SkillTrigger>(Refresh);
		}
	}

	private void Refresh(SkillTrigger _)
	{
		float total = 0f;
		float perSkill = GetValue(bonusPerSkill);
		Check(hero.Skill.Q);
		Check(hero.Skill.W);
		Check(hero.Skill.E);
		Check(hero.Skill.R);
		Check(hero.Skill.Identity);
		_bonus.abilityPowerFlat = total + GetValue(addedFlat);
		void Check(SkillTrigger s)
		{
			if ((UnityEngine.Object)(object)s != null && s.tags.HasFlag(DescriptionTags.Cold))
			{
				total += perSkill;
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
