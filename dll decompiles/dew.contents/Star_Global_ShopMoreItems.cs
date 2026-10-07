public class Star_Global_ShopMoreItems : DewStarItemOld
{
	public override int maxLevel => 1;

	public override bool ShouldInitInGame()
	{
		return isServer;
	}

	public override void OnStartInGame()
	{
		base.OnStartInGame();
		player.shopAddedItems = 1;
	}
}
