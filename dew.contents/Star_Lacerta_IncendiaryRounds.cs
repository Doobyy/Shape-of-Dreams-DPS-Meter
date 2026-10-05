using System;

public class Star_Lacerta_IncendiaryRounds : DewHeroStarItemOld
{
	public static readonly int AddedShots = 1;

	public override Type heroType => typeof(Hero_Lacerta);

	public override Type affectedSkill => typeof(St_Q_IncendiaryRounds);

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
		if (obj.instance is Se_Q_IncendiaryRounds_EmpowerAttacks se_Q_IncendiaryRounds_EmpowerAttacks)
		{
			se_Q_IncendiaryRounds_EmpowerAttacks.numOfAttacks += AddedShots;
		}
	}
}
