using System;
using System.Collections.Generic;

public class Star_Yubar_StrongerCataclysm : DewHeroStarItemOld
{
	public override Type heroType => typeof(Hero_Yubar);

	public override Type affectedSkill => typeof(St_R_Cataclysm);

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
		hero.ActorEvent_OnAbilityInstanceBeforePrepare += new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
		foreach (KeyValuePair<int, AbilityTrigger> ability in hero.Ability.abilities)
		{
			if (ability.Value is St_R_Cataclysm st_R_Cataclysm)
			{
				st_R_Cataclysm.tags |= DescriptionTags.HardCC;
			}
		}
	}

	private void ActorEventOnAbilityInstanceBeforePrepare(EventInfoAbilityInstance obj)
	{
		if (obj.instance is Ai_R_Cataclysm_Meteor ai_R_Cataclysm_Meteor)
		{
			ai_R_Cataclysm_Meteor.spawnBurnInstance = true;
		}
	}
}
