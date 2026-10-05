using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

public class Se_Curse_SoftSkin : CurseStatusEffect
{
	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			victim.takenDamageProcessor.Add(Processor);
		}
	}

	private void Processor(ref DamageData data, Actor actor, Entity target)
	{
		if (!((Object)(object)actor.firstEntity == null) && target.CheckEnemyOrNeutral(actor.firstEntity))
		{
			data.SetAttr(DamageAttribute.IgnoreArmor);
			data.SetAttr(DamageAttribute.IgnoreShield);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (Object)(object)victim != null)
		{
			victim.takenDamageProcessor.Remove(Processor);
		}
	}

	public override bool IsViable(Entity target)
	{
		if (!base.IsViable(target))
		{
			return false;
		}
		Hero h = target as Hero;
		if (h == null)
		{
			return false;
		}
		if (!(target.Status.armor > 20f) && !Check(HeroSkillLocation.Q) && !Check(HeroSkillLocation.W) && !Check(HeroSkillLocation.E) && !Check(HeroSkillLocation.R))
		{
			return h.Skill.gems.Any((KeyValuePair<GemLocation, Gem> pair) => pair.Value.tags.HasFlag(DescriptionTags.Shield));
		}
		return true;
		bool Check(HeroSkillLocation loc)
		{
			SkillTrigger skill = h.Skill.GetSkill(loc);
			if ((Object)(object)skill != null)
			{
				return skill.tags.HasFlag(DescriptionTags.Shield);
			}
			return false;
		}
	}

	private void MirrorProcessed()
	{
	}
}
