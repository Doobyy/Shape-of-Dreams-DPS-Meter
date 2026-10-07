using System;

public class Star_Mist_LightningFleche : DewHeroStarItemOld
{
	public override Type heroType => typeof(Hero_Mist);

	public override Type affectedSkill => typeof(St_Q_Fleche);

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
		if (obj.instance is Ai_Q_Fleche_Dash ai_Q_Fleche_Dash)
		{
			ai_Q_Fleche_Dash.NetworkempoweredWithLightning = true;
		}
	}
}
