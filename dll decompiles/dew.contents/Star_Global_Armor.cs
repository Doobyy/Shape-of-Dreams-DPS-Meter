public class Star_Global_Armor : DewStarItemOld
{
	public static readonly int[] ArmorBonus = new int[5] { 2, 4, 6, 9, 12 };

	public override int maxLevel => 5;

	public override bool ShouldInitInGame()
	{
		return isServer;
	}

	public override void OnStartInGame()
	{
		base.OnStartInGame();
		hero.CreateBasicEffect(hero, new ArmorBoostEffect
		{
			strength = ArmorBonus.GetClamped(level - 1)
		}, float.PositiveInfinity, "StarArmor");
	}
}
