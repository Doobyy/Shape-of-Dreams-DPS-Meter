public class Star_Global_StartSkill : DewStarItemOld
{
	public static readonly int[] SkillLevel = new int[3] { 1, 2, 3 };

	public override int maxLevel => 3;

	public override bool ShouldInitInGame()
	{
		return isServer;
	}

	public override void OnStartInGame()
	{
		base.OnStartInGame();
	}
}
