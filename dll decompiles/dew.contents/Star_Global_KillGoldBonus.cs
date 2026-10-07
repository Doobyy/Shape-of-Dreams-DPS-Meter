public class Star_Global_KillGoldBonus : DewStarItemOld
{
	public static readonly float[] BonusRatio = new float[5] { 0.03f, 0.06f, 0.09f, 0.12f, 0.15f };

	public override int maxLevel => 5;

	public override bool ShouldInitInGame()
	{
		return isServer;
	}

	public override void OnStartInGame()
	{
		base.OnStartInGame();
		player.monsterKillGoldMultiplier = 1f + BonusRatio.GetClamped(level - 1);
	}
}
