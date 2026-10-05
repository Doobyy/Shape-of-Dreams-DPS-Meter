public class Star_Global_Cleanse : DewStarItemOld
{
	public static readonly float[] DreamDustRefundMultiplier = new float[3] { 0.7f, 0.8f, 0.9f };

	public override int maxLevel => 3;

	public override bool ShouldInitInGame()
	{
		return isServer;
	}

	public override void OnStartInGame()
	{
		base.OnStartInGame();
		player.cleanseRefundMultiplier = DreamDustRefundMultiplier.GetClamped(level - 1);
	}
}
