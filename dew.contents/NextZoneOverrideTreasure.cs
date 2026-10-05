using Mirror;
using UnityEngine;

public abstract class NextZoneOverrideTreasure : Treasure
{
	protected abstract string GetZoneName();

	protected override void OnCreate()
	{
		base.OnCreate();
		Zone byName = DewResources.GetByName<Zone>(GetZoneName());
		NetworkedManagerBase<ChatManager>.instance.ShowMessageLocally(new ChatManager.Message
		{
			type = ChatManager.MessageType.Notice,
			content = "NextZoneOverrideTreasure_Announcement",
			args = new string[2]
			{
				ChatManager.GetColoredDescribedPlayerName(player),
				"<color=" + Dew.GetHex(Color.Lerp(byName.mainColor, Color.white, 0.5f)) + ">" + DewLocalization.GetUIValue(GetZoneName() + "_Name") + "</color>"
			}
		});
		if (((NetworkBehaviour)this).isServer)
		{
			NetworkedManagerBase<ZoneManager>.instance.nextZoneOverride = byName;
			Destroy();
		}
	}

	public override bool ShouldBeIncludedInPool()
	{
		if (!base.ShouldBeIncludedInPool())
		{
			return false;
		}
		if (NetworkedManagerBase<ZoneManager>.instance.nextZoneOverride != null)
		{
			return false;
		}
		string zoneName = GetZoneName();
		if (!Dew.IsZoneIncludedInGame(zoneName))
		{
			return false;
		}
		Zone byName = DewResources.GetByName<Zone>(zoneName);
		if (byName == null)
		{
			return false;
		}
		int currentTier = NetworkedManagerBase<ZoneManager>.instance.currentTier;
		int count = NetworkedManagerBase<ZoneManager>.instance.remainingZonesOfCurrentTier.Count;
		if (count != 0 || byName.zoneTier != (currentTier + 1) % DewBuildProfile.current.content.zoneCountByTier.Count)
		{
			if (count > 0)
			{
				return byName.zoneTier == currentTier;
			}
			return false;
		}
		return true;
	}

	public override bool CanBePurchased()
	{
		if (!base.CanBePurchased())
		{
			return false;
		}
		if (NetworkedManagerBase<ZoneManager>.instance.nextZoneOverride != null)
		{
			player.TpcShowCenterMessage(CenterMessageType.Error, "NextZoneOverrideTreasure_AlreadyOverriden", new string[1] { "ui." + NetworkedManagerBase<ZoneManager>.instance.nextZoneOverride.asset.name + "_Name" });
			return false;
		}
		return true;
	}

	private void MirrorProcessed()
	{
	}
}
