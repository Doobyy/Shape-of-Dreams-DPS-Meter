using Mirror;

public class Treasure_StrayMemory : Treasure
{
	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			NetworkedManagerBase<QuestManager>.instance.StartQuest<Quest_StrayMemory>();
			Destroy();
		}
	}

	private void MirrorProcessed()
	{
	}
}
