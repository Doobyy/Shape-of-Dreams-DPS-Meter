using System;

public class RoomMod_DreamTeller : SpecialEntityRoomModifier
{
	protected override Type GetEntityType()
	{
		return typeof(Mon_DreamTeller);
	}

	public override bool IsAvailableInGame()
	{
		return NetworkedManagerBase<QuestManager>.instance.currentArtifact != null;
	}

	private void MirrorProcessed()
	{
	}
}
