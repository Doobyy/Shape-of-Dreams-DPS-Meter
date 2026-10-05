public class Star_Global_DismantleDreamDustBonus : DewStarItemOld
{
	public static readonly float[] BonusDreamDustRatio = new float[4] { 0.07f, 0.12f, 0.18f, 0.25f };

	public override int maxLevel => 4;

	public override bool ShouldInitInGame()
	{
		return isServer;
	}

	public override void OnStartInGame()
	{
		base.OnStartInGame();
		player.dismantleDreamDustMultiplier = 1f + BonusDreamDustRatio.GetClamped(level - 1);
	}
}
