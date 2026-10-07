using System;
using System.Collections.Generic;

public class Star_Yubar_StrongerSuperNova : DewHeroStarItemOld
{
	public static readonly float StunDuration = 1f;

	public static readonly float RadiusMultiplier = 1.2f;

	public override Type heroType => typeof(Hero_Yubar);

	public override Type affectedSkill => typeof(St_Q_SuperNova);

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
		hero.ActorEvent_OnDealDamage += new Action<EventInfoDamage>(ActorEventOnDealDamage);
		foreach (KeyValuePair<int, AbilityTrigger> ability in hero.Ability.abilities)
		{
			if (ability.Value is St_Q_SuperNova st_Q_SuperNova)
			{
				st_Q_SuperNova.tags |= DescriptionTags.HardCC;
			}
		}
	}

	private void ActorEventOnAbilityInstanceBeforePrepare(EventInfoAbilityInstance obj)
	{
		if (obj.instance is Ai_Q_SuperNova ai_Q_SuperNova)
		{
			ai_Q_SuperNova.NetworkscaleMultiplier = RadiusMultiplier;
		}
	}

	private void ActorEventOnDealDamage(EventInfoDamage obj)
	{
		if (obj.actor is Ai_Q_SuperNova)
		{
			obj.actor.CreateBasicEffect(obj.victim, new StunEffect(), StunDuration, "SuperNovaStun", DuplicateEffectBehavior.UsePrevious);
		}
	}
}
