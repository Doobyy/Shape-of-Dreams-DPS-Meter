using System;

public class Star_Mist_ParryAllDirections : DewHeroStarItemOld
{
	public override Type heroType => typeof(Hero_Mist);

	public override Type affectedSkill => typeof(St_R_Parry);

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
	}

	private void ActorEventOnAbilityInstanceBeforePrepare(EventInfoAbilityInstance obj)
	{
		if (obj.instance is Se_R_Parry_Start se_R_Parry_Start)
		{
			se_R_Parry_Start.allowAnyDirection = true;
		}
	}
}
