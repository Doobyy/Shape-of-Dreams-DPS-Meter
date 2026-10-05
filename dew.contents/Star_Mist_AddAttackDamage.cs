using System;

public class Star_Mist_AddAttackDamage : DewHeroStarItemOld
{
	public static readonly int[] BonusAttackDamage = new int[3] { 3, 6, 12 };

	public override int maxLevel => 3;

	public override Type heroType => typeof(Hero_Mist);

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
		hero.Status.AddStatBonus(new StatBonus
		{
			attackDamageFlat = BonusAttackDamage.GetClamped(level - 1)
		});
	}
}
