using Mirror;

public class Treasure_StarSeekersJournal : Treasure
{
	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			NetworkedManagerBase<QuestManager>.instance.StartQuest<Quest_StarSeekersJournal>();
			Destroy();
		}
	}

	private void MirrorProcessed()
	{
	}
}
