public static class CostExtensions
{
	public static AffordType CanAfford(this Cost? cost, Entity entity, int playerStardust = -1)
	{
		return cost?.CanAfford(entity, playerStardust) ?? AffordType.Yes;
	}

	public static AffordType CanAfford(this Cost? cost, DewPlayer player, int playerStardust = -1)
	{
		return cost?.CanAfford(player, playerStardust) ?? AffordType.Yes;
	}
}
