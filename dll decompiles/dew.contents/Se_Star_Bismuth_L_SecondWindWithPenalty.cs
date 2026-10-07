using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Se_Star_Bismuth_L_SecondWindWithPenalty : StarEffect
{
	public StarScalingValue invulTime;

	public override Type heroType => typeof(Hero_Bismuth);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		DoDeathInterrupt((EventInfoKill _) =>
		{
			victim.Status.SetHealth(victim.Status.maxHealth * 0.25f);
			if (!victim.Status.HasStatusEffect<Se_FatalHitProtection_Invulnerable>())
			{
				CreateStatusEffect(victim, (Se_FatalHitProtection_Invulnerable se) =>
				{
					se.duration = GetValue(invulTime);
				});
				List<SkillTrigger> list = new List<SkillTrigger>();
				if (!hero.Skill.Q.IsNullOrInactive())
				{
					list.Add(hero.Skill.Q);
				}
				if (!hero.Skill.W.IsNullOrInactive())
				{
					list.Add(hero.Skill.W);
				}
				if (!hero.Skill.E.IsNullOrInactive())
				{
					list.Add(hero.Skill.E);
				}
				if (!hero.Skill.R.IsNullOrInactive())
				{
					list.Add(hero.Skill.R);
				}
				if (!hero.Skill.Identity.IsNullOrInactive())
				{
					list.Add(hero.Skill.Identity);
				}
				if (list.Count > 0)
				{
					list[UnityEngine.Random.Range(0, list.Count)].Destroy();
				}
				Dew.CallDelayed(() =>
				{
					if (!this.IsNullOrInactive())
					{
						Destroy();
					}
				});
			}
		}, 100);
	}

	private void MirrorProcessed()
	{
	}
}
