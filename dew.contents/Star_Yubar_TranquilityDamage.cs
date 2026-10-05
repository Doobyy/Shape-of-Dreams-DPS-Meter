using System;

public class Star_Yubar_TranquilityDamage : DewHeroStarItemOld
{
	public override Type heroType => typeof(Hero_Yubar);

	public override Type affectedSkill => typeof(St_R_Tranquility);

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
		hero.ActorEvent_OnAbilityInstanceCreated += new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceCreated);
	}

	private void ActorEventOnAbilityInstanceCreated(EventInfoAbilityInstance obj)
	{
		if (obj.instance is Se_R_Tranquility)
		{
			obj.instance.CreateAbilityInstance<Ai_R_Tranquility_Damage>(hero.agentPosition, null, new CastInfo(hero));
		}
	}
}
