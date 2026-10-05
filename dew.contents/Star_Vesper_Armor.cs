using System;

public class Star_Vesper_Armor : DewHeroStarItemOld
{
	public static readonly int ArmorBonus = 10;

	public override Type heroType => typeof(Hero_Vesper);

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
		hero.CreateBasicEffect(hero, new ArmorBoostEffect
		{
			strength = ArmorBonus
		}, float.PositiveInfinity, "VesperStarArmor", DuplicateEffectBehavior.UsePrevious);
	}
}
