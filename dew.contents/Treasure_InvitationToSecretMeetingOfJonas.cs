using Mirror;

public class Treasure_InvitationToSecretMeetingOfJonas : Treasure
{
	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			NetworkedManagerBase<QuestManager>.instance.StartQuest<Quest_SecretMeeting>();
			Destroy();
		}
	}

	public override bool CanBePurchased()
	{
		if (NetworkedManagerBase<QuestManager>.instance.HasQuest<Quest_SecretMeeting>())
		{
			player.TpcShowCenterMessage(CenterMessageType.Error, "Quest_SecretMeeting_AlreadyHaveInvitation");
			return false;
		}
		if (IsAnyCompassInGame())
		{
			player.TpcShowCenterMessage(CenterMessageType.Error, "Quest_SecretMeeting_AlreadyHaveCompass");
			return false;
		}
		return base.CanBePurchased();
	}

	public override bool ShouldBeIncludedInPool()
	{
		if (base.ShouldBeIncludedInPool() && player.hasPolarisEndingUnlocked && !NetworkedManagerBase<QuestManager>.instance.HasQuest<Quest_SecretMeeting>())
		{
			return !IsAnyCompassInGame();
		}
		return false;
	}

	private bool IsAnyCompassInGame()
	{
		foreach (Actor allActor in NetworkedManagerBase<ActorManager>.instance.allActors)
		{
			if (allActor is Gem_U_GuidingCompass_Charged || allActor is Gem_U_GuidingCompass_NotCharged)
			{
				return true;
			}
		}
		return false;
	}

	private void MirrorProcessed()
	{
	}
}
