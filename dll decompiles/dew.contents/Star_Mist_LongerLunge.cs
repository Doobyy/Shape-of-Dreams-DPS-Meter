using System;
using System.Collections.Generic;

public class Star_Mist_LongerLunge : DewHeroStarItemOld
{
	public static readonly float LungeRangeMultiplier = 1.33f;

	public override Type heroType => typeof(Hero_Mist);

	public override Type affectedSkill => typeof(St_Q_Lunge);

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
			if (ability.Value is St_Q_Lunge st_Q_Lunge)
			{
				st_Q_Lunge.configs[0].castMethod._range *= LungeRangeMultiplier;
				st_Q_Lunge.SyncCastMethodChanges(0);
			}
		}
	}
}
