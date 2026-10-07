public class Shrine_StrayMemory_StartProp : Shrine
{
	protected override bool OnUse(Entity entity)
	{
		NetworkedManagerBase<QuestManager>.instance.StartQuest<Quest_StrayMemory>();
		return true;
	}

	private void MirrorProcessed()
	{
	}
}
