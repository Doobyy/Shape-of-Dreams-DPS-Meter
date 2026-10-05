using System;
using Mirror;
using UnityEngine;

public class Se_Star_Yubar_D_APPerLightSkill : StarEffect
{
	public StarScalingValue bonusPerSkill;

	private StatBonus _bonus;

	public override Type heroType => typeof(Hero_Yubar);

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

	private void Refresh(SkillTrigger _)
	{
		float total = 0f;
		float perSkill = GetValue(bonusPerSkill);
		Check(hero.Skill.Q);
		Check(hero.Skill.W);
		Check(hero.Skill.E);
		Check(hero.Skill.R);
		Check(hero.Skill.Identity);
		_bonus.abilityPowerFlat = total;
		void Check(SkillTrigger s)
		{
			if ((UnityEngine.Object)(object)s != null && s.tags.HasFlag(DescriptionTags.Light))
			{
				total += perSkill;
			}
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			hero.Skill.ClientHeroEvent_OnSkillEquip -= new Action<SkillTrigger>(Refresh);
			hero.Skill.ClientHeroEvent_OnSkillUnequip -= new Action<SkillTrigger>(Refresh);
		}
	}

	private void MirrorProcessed()
	{
	}
}
