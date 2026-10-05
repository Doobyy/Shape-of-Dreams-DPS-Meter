using System.Collections;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class Shrine_TheConsortOfNight_Rift : Shrine, ICustomInteractable
{
	public float startDelay;

	public GameObject fxStart;

	public GameObject fxLoop;

	public string nameRawText => DewLocalization.GetUIValue("Rift_ToSomewhereDeeper_Name");

	public string interactActionRawText => DewLocalization.GetUIValue("InGame_Interact_Rift_Enter");

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!isNewInstance)
		{
			if (((NetworkBehaviour)this).isServer)
			{
				FxPlayNetworked(fxLoop);
			}
			return;
		}
		interactableDelay += startDelay;
		if (((NetworkBehaviour)this).isServer)
		{
			((MonoBehaviour)(object)this).StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			yield return new WaitForSeconds(startDelay);
			FxPlayNetworked(fxStart);
			yield return new WaitForSeconds(interactableDelay - startDelay);
			FxPlayNetworked(fxLoop);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			FxStopNetworked(fxLoop);
		}
	}

	protected override bool OnUse(Entity entity)
	{
		TpcInteract(entity.owner);
		return true;
	}

	[TargetRpc]
	private void TpcInteract(NetworkConnectionToClient target)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendTargetRPCInternal((NetworkConnection)(object)target, "System.Void Shrine_TheConsortOfNight_Rift::TpcInteract(Mirror.NetworkConnectionToClient)", -1502673103, (NetworkWriter)(object)val, 0);
		NetworkWriterPool.Return(val);
	}

	[Command(requiresAuthority = false)]
	private void CmdTravelToNextZone(NetworkConnectionToClient target)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendCommandInternal("System.Void Shrine_TheConsortOfNight_Rift::CmdTravelToNextZone(Mirror.NetworkConnectionToClient)", -1211224798, (NetworkWriter)(object)val, 0, false);
		NetworkWriterPool.Return(val);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_TpcInteract__NetworkConnectionToClient(NetworkConnectionToClient target)
	{
		string text = "";
		text += DewLocalization.GetUIValue("InGame_Message_TheConsortOfNight_Rift");
		DewMessageSettings msg = new DewMessageSettings
		{
			buttons = (DewMessageSettings.ButtonType.Yes | DewMessageSettings.ButtonType.No),
			defaultButton = DewMessageSettings.ButtonType.No,
			destructiveConfirm = true,
			onClose = (DewMessageSettings.ButtonType b) =>
			{
				if (b == DewMessageSettings.ButtonType.Yes)
				{
					NetworkedManagerBase<ZoneManager>.instance.TravelWithValidationAndConfirmation(() =>
					{
						CmdTravelToNextZone(target);
					});
				}
			},
			validator = () => InGameUIManager.ValidateInGameActionMessage(),
			rawContent = text
		};
		ManagerBase<MessageManager>.instance.ShowMessage(msg);
	}

	protected static void InvokeUserCode_TpcInteract__NetworkConnectionToClient(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TpcInteract called on server.");
		}
		else
		{
			((Shrine_TheConsortOfNight_Rift)(object)obj).UserCode_TpcInteract__NetworkConnectionToClient((NetworkConnectionToClient)(object)NetworkClient.connection);
		}
	}

	protected void UserCode_CmdTravelToNextZone__NetworkConnectionToClient(NetworkConnectionToClient target)
	{
		DewPlayer player = target.GetPlayer();
		if (!((Object)(object)player == null) && !((Object)(object)NetworkedManagerBase<ZoneManager>.instance == null))
		{
			if (NetworkedManagerBase<ZoneManager>.instance.ShouldVoteOnTravel())
			{
				NetworkedManagerBase<ZoneManager>.instance.StartVoteNextZone(player);
			}
			else
			{
				NetworkedManagerBase<GameManager>.instance.LoadNextZone();
			}
		}
	}

	protected static void InvokeUserCode_CmdTravelToNextZone__NetworkConnectionToClient(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdTravelToNextZone called on client.");
		}
		else
		{
			((Shrine_TheConsortOfNight_Rift)(object)obj).UserCode_CmdTravelToNextZone__NetworkConnectionToClient(senderConnection);
		}
	}

	static Shrine_TheConsortOfNight_Rift()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected Obj, but got Unknown
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterCommand(typeof(Shrine_TheConsortOfNight_Rift), "System.Void Shrine_TheConsortOfNight_Rift::CmdTravelToNextZone(Mirror.NetworkConnectionToClient)", (RemoteCallDelegate)InvokeUserCode_CmdTravelToNextZone__NetworkConnectionToClient, false);
		RemoteProcedureCalls.RegisterRpc(typeof(Shrine_TheConsortOfNight_Rift), "System.Void Shrine_TheConsortOfNight_Rift::TpcInteract(Mirror.NetworkConnectionToClient)", (RemoteCallDelegate)InvokeUserCode_TpcInteract__NetworkConnectionToClient);
	}
}
