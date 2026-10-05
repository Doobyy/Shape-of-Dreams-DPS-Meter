using System;

public class Star_Yubar_SkillHaste : DewHeroStarItemOld
{
	public static readonly int[] SkillHasteBonus = new int[3] { 3, 7, 12 };

	public override int maxLevel => 3;

	public override Type heroType => typeof(Hero_Yubar);

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
			abilityHasteFlat = SkillHasteBonus.GetClamped(level - 1)
		});
	}
}
