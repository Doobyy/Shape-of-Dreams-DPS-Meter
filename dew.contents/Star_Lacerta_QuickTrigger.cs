using System;
using System.Collections.Generic;

public class Star_Lacerta_QuickTrigger : DewHeroStarItemOld
{
	public override Type heroType => typeof(Hero_Lacerta);

	public override Type affectedSkill => typeof(St_R_QuickTrigger);

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
		foreach (KeyValuePair<int, AbilityTrigger> ability in hero.Ability.abilities)
		{
			if (ability.Value is St_R_QuickTrigger)
			{
				ability.Value.configs[0].addedCharges = 999;
			}
		}
	}
}
