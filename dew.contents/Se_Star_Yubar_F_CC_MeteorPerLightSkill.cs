using System;
using Mirror;
using UnityEngine;

public class Se_Star_Yubar_F_CC_MeteorPerLightSkill : StarEffect
{
	public override Type heroType => typeof(Hero_Yubar);

	public override Type skillType => typeof(St_R_Cataclysm);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			victim.ActorEvent_OnAbilityInstanceBeforePrepare += new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
		}
	}

	private void ActorEventOnAbilityInstanceBeforePrepare(EventInfoAbilityInstance obj)
	{
		AbilityInstance instance = obj.instance;
		Se_R_Cataclysm c = instance as Se_R_Cataclysm;
		if (c != null)
		{
			Check(hero.Skill.Q);
			Check(hero.Skill.W);
			Check(hero.Skill.E);
			Check(hero.Skill.R);
			Check(hero.Skill.Identity);
		}
		void Check(SkillTrigger st)
		{
			if ((bool)(UnityEngine.Object)(object)st && !((UnityEngine.Object)(object)st == (UnityEngine.Object)(object)skill) && (st.tags & DescriptionTags.Light) != 0)
			{
				c.reuseCount++;
			}
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)victim != null)
		{
			victim.ActorEvent_OnAbilityInstanceBeforePrepare -= new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
		}
	}

	private void MirrorProcessed()
	{
	}
}
