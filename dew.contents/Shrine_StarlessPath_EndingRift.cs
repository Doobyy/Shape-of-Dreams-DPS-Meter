using System.Linq;
using Mirror;
using UnityEngine;

public class Shrine_StarlessPath_EndingRift : Shrine, IShrineCustomAction, IShrineCustomName
{
	public override bool CanInteract(Entity entity)
	{
		if (base.CanInteract(entity))
		{
			return NetworkedManagerBase<ActorManager>.instance.allHeroes.All((Hero h) => !h.Status.isInConversation);
		}
		return false;
	}

	public override void OnInteract(Entity entity, bool alt)
	{
		base.OnInteract(entity, alt);
		if (!((NetworkBehaviour)entity).isOwned || !(entity is Hero))
		{
			return;
		}
		StarlessPath_BossPolarisManager bossPolarisManager = Dew.FindActorOfType<StarlessPath_BossPolarisManager>();
		if (bossPolarisManager.IsNullOrInactive() || bossPolarisManager.state != StarlessPath_BossPolarisManager.State.AfterFight)
		{
			return;
		}
		if (!StarlessPath_BossPolarisManager.AreAllHeroesNear(position, out var current, out var required))
		{
			InGameUIManager.instance.ShowCenterMessage(CenterMessageType.General, "InGame_Message_NeedAllPlayersPresence", new object[2] { current, required });
		}
		else if (!bossPolarisManager.didInterrupt)
		{
			bossPolarisManager.CmdInterruptMe();
		}
		else
		{
			if (bossPolarisManager.didStartEndJourney)
			{
				return;
			}
			ManagerBase<MessageManager>.instance.ShowMessage(new DewMessageSettings
			{
				buttons = (DewMessageSettings.ButtonType.Yes | DewMessageSettings.ButtonType.Cancel),
				defaultButton = DewMessageSettings.ButtonType.Cancel,
				owner = (Object)(object)this,
				rawContent = DewLocalization.GetUIValue("InGame_Message_EndYourJourney"),
				onClose = (DewMessageSettings.ButtonType b) =>
				{
					if (b == DewMessageSettings.ButtonType.Yes)
					{
						bossPolarisManager.CmdEndJourney();
					}
				}
			});
		}
	}

	protected override bool OnUse(Entity entity)
	{
		return false;
	}

	public string GetRawName()
	{
		return DewLocalization.GetUIValue("Rift_Name");
	}

	public string GetRawAction()
	{
		return DewLocalization.GetUIValue("InGame_Interact_Rift_Enter");
	}

	private void MirrorProcessed()
	{
	}
}
