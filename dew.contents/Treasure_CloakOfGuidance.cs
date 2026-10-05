using Mirror;

public class Treasure_CloakOfGuidance : Treasure
{
	public int skippedTurns = 3;

	protected override void OnCreate()
	{
		base.OnCreate();
		NetworkedManagerBase<ChatManager>.instance.ShowMessageLocally(new ChatManager.Message
		{
			type = ChatManager.MessageType.Notice,
			content = "Treasure_CloakOfGuidance_HunterHasBeenDelayedBy",
			args = new string[2]
			{
				ChatManager.GetColoredDescribedPlayerName(player),
				skippedTurns.ToString("#,##0")
			}
		});
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		NetworkedManagerBase<ZoneManager>.instance.hunterSkippedTurns += skippedTurns;
		for (int i = 0; i < NetworkedManagerBase<ZoneManager>.instance.nodes.Count; i++)
		{
			if (NetworkedManagerBase<ZoneManager>.instance.hunterStatuses[i] == HunterStatus.AboutToBeTaken)
			{
				NetworkedManagerBase<ZoneManager>.instance.hunterStatuses[i] = HunterStatus.None;
			}
		}
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
