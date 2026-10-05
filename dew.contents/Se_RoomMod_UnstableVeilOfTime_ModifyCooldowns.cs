using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Se_RoomMod_UnstableVeilOfTime_ModifyCooldowns : StatusEffect
{
	public float goodCooldownMultiplier = 0.2f;

	public float badCooldownMultiplier = 5f;

	private List<SkillBonus> _appliedBonuses = new List<SkillBonus>();

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		List<AbilityTrigger> abilities = DewPool.GetList(out ListReturnHandle<AbilityTrigger> handle);
		if (victim is Hero hero)
		{
			AddIfExists(hero.Skill.Q);
			AddIfExists(hero.Skill.W);
			AddIfExists(hero.Skill.E);
			AddIfExists(hero.Skill.R);
		}
		else
		{
			foreach (KeyValuePair<int, AbilityTrigger> ability in victim.Ability.abilities)
			{
				if (ability.Key != 63)
				{
					AddIfExists(ability.Value);
				}
			}
		}
		if (abilities.Count == 0)
		{
			handle.Return();
			return;
		}
		int num = Random.Range(0, abilities.Count);
		for (int i = 0; i < abilities.Count; i++)
		{
			if (abilities[i] is SkillTrigger skillTrigger)
			{
				SkillBonus item = skillTrigger.AddSkillBonus(new SkillBonus
				{
					cooldownMultiplier = ((i == num) ? goodCooldownMultiplier : badCooldownMultiplier),
					ignoreReceiveCooldownReductionFlag = true
				});
				skillTrigger.specialOverlayColor = ((i == num) ? Color.white : new Color(0.5f, 0.1f, 0.1f));
				_appliedBonuses.Add(item);
			}
			else
			{
				abilities[i].configs[0].cooldownTime *= ((i == num) ? goodCooldownMultiplier : badCooldownMultiplier);
			}
		}
		handle.Return();
		void AddIfExists(AbilityTrigger trigger)
		{
			if (!((Object)(object)trigger == null))
			{
				abilities.Add(trigger);
			}
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		foreach (SkillBonus appliedBonuse in _appliedBonuses)
		{
			if ((Object)(object)appliedBonuse.parent != null)
			{
				appliedBonuse.parent.specialOverlayColor = Color.clear;
			}
			appliedBonuse.Stop();
		}
		_appliedBonuses.Clear();
	}

	private void MirrorProcessed()
	{
	}
}
