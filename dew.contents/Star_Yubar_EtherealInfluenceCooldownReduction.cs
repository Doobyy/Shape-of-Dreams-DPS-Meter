using System;

public class Star_Yubar_EtherealInfluenceCooldownReduction : DewHeroStarItemOld
{
	public static readonly float CooldownReductionRatioPerHit = 0.05f;

	public override Type heroType => typeof(Hero_Yubar);

	public override Type affectedSkill => typeof(St_Q_EtherealInfluence);

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
		hero.ActorEvent_OnDealDamage += new Action<EventInfoDamage>(ActorEventOnDealDamage);
	}

	private void ActorEventOnDealDamage(EventInfoDamage obj)
	{
		if (obj.actor is Ai_Q_EtherealInfluence)
		{
			_ = obj.actor.firstTrigger;
		}
	}
}
