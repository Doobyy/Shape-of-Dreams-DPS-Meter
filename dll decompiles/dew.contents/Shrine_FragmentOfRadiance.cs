public class Shrine_FragmentOfRadiance : Shrine
{
	protected override bool OnUse(Entity entity)
	{
		NetworkedManagerBase<QuestManager>.instance.StartQuest<Quest_FragmentOfRadiance>();
		return true;
	}

	private void MirrorProcessed()
	{
	}
}
