public class Shrine_InvitationOfJonasMeeting : Shrine
{
	protected override bool OnUse(Entity entity)
	{
		if (NetworkedManagerBase<QuestManager>.instance.HasQuest<Quest_SecretMeeting>())
		{
			entity.owner.TpcShowCenterMessage(CenterMessageType.Error, "Quest_SecretMeeting_AlreadyHaveInvitation");
			return false;
		}
		NetworkedManagerBase<QuestManager>.instance.StartQuest<Quest_SecretMeeting>();
		Destroy();
		return true;
	}

	private void MirrorProcessed()
	{
	}
}
