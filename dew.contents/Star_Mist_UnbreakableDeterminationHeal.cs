using System;

public class Star_Mist_UnbreakableDeterminationHeal : DewHeroStarItemOld
{
	public static readonly float LostHealthRatio = 0.25f;

	public override Type heroType => typeof(Hero_Mist);

	public override Type affectedSkill => typeof(St_R_UnbreakableDetermination);

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
		hero.ActorEvent_OnAbilityInstanceCreated += new Action<EventInfoAbilityInstance>(CreateHealEffect);
	}

	private void CreateHealEffect(EventInfoAbilityInstance obj)
	{
		if (obj.instance is Se_R_UnbreakableDetermination)
		{
			obj.instance.CreateStatusEffect(hero, new CastInfo(hero, hero), (Se_GenericHealOverTime heal) =>
			{
				heal.tickInterval = 0.1f;
				heal.ticks = 6;
				heal.totalAmount = LostHealthRatio * hero.Status.missingHealth;
			});
		}
	}
}
