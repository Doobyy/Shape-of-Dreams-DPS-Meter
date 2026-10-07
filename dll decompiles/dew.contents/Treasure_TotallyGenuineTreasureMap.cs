using Mirror;

public class Treasure_TotallyGenuineTreasureMap : Treasure
{
	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			NetworkedManagerBase<QuestManager>.instance.StartQuest<Quest_SuspiciousTreasureMap>();
			Destroy();
		}
	}

	private void MirrorProcessed()
	{
	}
}
