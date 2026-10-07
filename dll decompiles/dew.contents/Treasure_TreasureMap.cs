using Mirror;

public class Treasure_TreasureMap : Treasure
{
	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			NetworkedManagerBase<QuestManager>.instance.StartQuest<Quest_TreasureMap>();
			Destroy();
		}
	}

	private void MirrorProcessed()
	{
	}
}
