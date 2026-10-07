using System;

public class Star_Lacerta_AttackSpeed : DewHeroStarItemOld
{
	public static readonly float[] AttackSpeedPercentage = new float[3] { 5f, 12f, 20f };

	public override int maxLevel => 3;

	public override Type heroType => typeof(Hero_Lacerta);

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
			attackSpeedPercentage = AttackSpeedPercentage.GetClamped(level - 1)
		});
	}
}
