using System;

public class Star_Lacerta_AttackRange : DewHeroStarItemOld
{
	public static readonly int RangeBonus = 1;

	public override Type heroType => typeof(Hero_Lacerta);

	public override Type affectedSkill => typeof(At_Atk_LacertaRifle);

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
		hero.Ability.attackAbility.configs[0].castMethod._range += RangeBonus;
		hero.Ability.attackAbility.configs[1].castMethod._range += RangeBonus;
		hero.Ability.attackAbility.SyncCastMethodChanges(0);
		hero.Ability.attackAbility.SyncCastMethodChanges(1);
	}
}
