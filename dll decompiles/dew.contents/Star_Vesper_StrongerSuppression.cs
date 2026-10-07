using System;

public class Star_Vesper_StrongerSuppression : DewHeroStarItemOld
{
	public override Type heroType => typeof(Hero_Vesper);

	public override Type affectedSkill => typeof(St_Q_Suppression_Old);

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
		if (obj.instance is Se_Q_Suppression_Old se_Q_Suppression_Old)
		{
			se_Q_Suppression_Old.enableAreaStun = true;
		}
	}
}
