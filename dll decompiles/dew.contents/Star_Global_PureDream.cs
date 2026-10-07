public class Star_Global_PureDream : DewStarItemOld
{
	public override int maxLevel => 1;

	public override bool ShouldInitInGame()
	{
		return isServer;
	}

	public override void OnStartInGame()
	{
		base.OnStartInGame();
	}
}
