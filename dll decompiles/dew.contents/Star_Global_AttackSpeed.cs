public class Star_Global_AttackSpeed : DewStarItemOld
{
	public static readonly float[] AttackSpeedBonus = new float[5] { 3f, 6f, 9f, 12f, 15f };

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
			attackSpeedPercentage = AttackSpeedBonus.GetClamped(level - 1)
		});
	}
}
