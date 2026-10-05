using System;

public class Star_Lacerta_FireAmp : DewHeroStarItemOld
{
	public static readonly float[] BonusFireAmp = new float[3] { 0.1f, 0.2f, 0.4f };

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
			fireEffectAmpFlat = BonusFireAmp.GetClamped(level - 1)
		});
	}
}
