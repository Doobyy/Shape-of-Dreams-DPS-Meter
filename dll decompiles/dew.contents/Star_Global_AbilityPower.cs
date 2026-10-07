public class Star_Global_AbilityPower : DewStarItemOld
{
	public static readonly int[] AbilityPowerBonus = new int[5] { 3, 6, 9, 12, 15 };

	public override int maxLevel => 5;

	public override bool ShouldInitInGame()
	{
		return isServer;
	}

	public override void OnStartInGame()
	{
		base.OnStartInGame();
		hero.Status.AddStatBonus(new StatBonus
		{
			abilityPowerFlat = AbilityPowerBonus.GetClamped(level - 1)
		});
	}
}
