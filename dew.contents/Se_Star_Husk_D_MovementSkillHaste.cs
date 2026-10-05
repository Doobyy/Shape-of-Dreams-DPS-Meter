using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Se_Star_Husk_D_MovementSkillHaste : StarEffect
{
	public StarScalingValue cooldownReduction;

	private Dictionary<SkillTrigger, SkillBonus> _bonuses = new Dictionary<SkillTrigger, SkillBonus>();

	public override Type heroType => typeof(Hero_Husk);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			hero.Skill.ClientHeroEvent_OnSkillEquip += new Action<SkillTrigger>(ClientHeroEventOnSkillEquip);
			hero.Skill.ClientHeroEvent_OnSkillUnequip += new Action<SkillTrigger>(ClientHeroEventOnSkillUnequip);
			ClientHeroEventOnSkillEquip(hero.Skill.Q);
			ClientHeroEventOnSkillEquip(hero.Skill.W);
			ClientHeroEventOnSkillEquip(hero.Skill.E);
			ClientHeroEventOnSkillEquip(hero.Skill.R);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		if ((UnityEngine.Object)(object)hero != null)
		{
			hero.Skill.ClientHeroEvent_OnSkillEquip -= new Action<SkillTrigger>(ClientHeroEventOnSkillEquip);
			hero.Skill.ClientHeroEvent_OnSkillUnequip -= new Action<SkillTrigger>(ClientHeroEventOnSkillUnequip);
		}
		foreach (SkillBonus value in _bonuses.Values)
		{
			value.Stop();
		}
		_bonuses.Clear();
	}

	private void ClientHeroEventOnSkillEquip(SkillTrigger obj)
	{
		if (!((UnityEngine.Object)(object)obj == null))
		{
			HeroSkillLocation heroSkillLocation = obj.skillType;
			if ((heroSkillLocation == HeroSkillLocation.Q || heroSkillLocation == HeroSkillLocation.W || heroSkillLocation == HeroSkillLocation.E || heroSkillLocation == HeroSkillLocation.R) && (obj.configs[0].selfValidator.isMovementAbility || (obj.tags & DescriptionTags.Mobility) != 0))
			{
				SkillBonus value = obj.AddSkillBonus(new SkillBonus
				{
					cooldownMultiplier = 1f - GetValue(cooldownReduction)
				});
				_bonuses[obj] = value;
			}
		}
	}

	private void ClientHeroEventOnSkillUnequip(SkillTrigger obj)
	{
		if (!((UnityEngine.Object)(object)obj == null) && _bonuses.TryGetValue(obj, out var value))
		{
			value.Stop();
			_bonuses.Remove(obj);
		}
	}

	private void MirrorProcessed()
	{
	}
}
