using System;
using Mirror;

public class Treasure_Clairvoyance : Treasure
{
	protected override void OnCreate()
	{
		base.OnCreate();
		NetworkedManagerBase<ChatManager>.instance.ShowMessageLocally(new ChatManager.Message
		{
			type = ChatManager.MessageType.Notice,
			content = "Treasure_Clairvoyance_WorldHasBeenReaveledBy",
			args = new string[1] { ChatManager.GetColoredDescribedPlayerName(player) }
		});
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		for (int i = 0; i < NetworkedManagerBase<ZoneManager>.instance.nodes.Count; i++)
		{
			WorldNodeData worldNodeData = NetworkedManagerBase<ZoneManager>.instance.nodes[i];
			if (worldNodeData.status != WorldNodeStatus.HasVisited)
			{
				worldNodeData.status = WorldNodeStatus.RevealedFull;
				NetworkedManagerBase<ZoneManager>.instance.nodes[i] = worldNodeData;
			}
		}
	}

	private bool HasNonRevealedArea()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		Enumerator<WorldNodeData> enumerator = NetworkedManagerBase<ZoneManager>.instance.nodes.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				WorldNodeData current = enumerator.Current;
				if (current.status != WorldNodeStatus.RevealedFull && current.status != WorldNodeStatus.HasVisited)
				{
					return true;
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
		return false;
	}

	public override bool ShouldBeIncludedInPool()
	{
		return HasNonRevealedArea();
	}

	public override bool CanBePurchased()
	{
		if (!HasNonRevealedArea())
		{
			player.TpcShowCenterMessage(CenterMessageType.Error, "Treasure_Clairvoyance_WorldHasAlreadyBeenRevealed");
			return false;
		}
		return true;
	}

	private void MirrorProcessed()
	{
	}
}
