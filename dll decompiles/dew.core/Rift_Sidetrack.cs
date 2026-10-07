using System.Collections.Generic;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

[DewResourceLink(ResourceLinkBy.Name)]
public class Rift_Sidetrack : Rift
{
	public new static Rift_Sidetrack softInstance;

	public Color mainColor;

	public float spawnChance = 0.01f;

	public bool oncePerLoop;

	public string[] allowedZones;

	public bool useWildcardPattern;

	public string wildcardPattern;

	public List<string> manualRoomNames = new List<string>();

	private bool _didUse;

	public new static Rift_Sidetrack instance => Dew.Helper_GetInstanceOfActor(ref softInstance);

	public bool isValid
	{
		get
		{
			if (!useWildcardPattern)
			{
				return manualRoomNames.Count > 0;
			}
			return !string.IsNullOrEmpty(wildcardPattern);
		}
	}

	public override bool ShouldBeSavedWithRoom()
	{
		return !_didUse;
	}

	private string GetZoneValidationMessage(string[] obj)
	{
		if (obj == null)
		{
			return null;
		}
		foreach (string text in obj)
		{
			if (!DewResources.database.nameToGuid.ContainsKey(text))
			{
				return "Zone of name '" + text + "' not found";
			}
		}
		return null;
	}

	protected override void Awake()
	{
		base.Awake();
		softInstance = this;
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if ((Object)(object)softInstance == (Object)(object)this)
		{
			softInstance = null;
		}
	}

	internal List<string> GetRoomNames()
	{
		if (!useWildcardPattern)
		{
			return manualRoomNames;
		}
		List<string> list = new List<string>();
		foreach (string sceneName in DewResources.database.sceneNames)
		{
			if (sceneName.EqualsWildcard(wildcardPattern))
			{
				list.Add(sceneName);
			}
		}
		return list;
	}

	protected override bool OnInteractRift(Hero hero)
	{
		TpcPromptConfirmation(hero.owner, NetworkedManagerBase<ZoneManager>.instance.isSidetracking);
		return false;
	}

	protected virtual string GetConfirmMessage()
	{
		return null;
	}

	[TargetRpc]
	private void TpcPromptConfirmation(NetworkConnectionToClient target, bool isReturning)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteBool((NetworkWriter)(object)val, isReturning);
		((NetworkBehaviour)this).SendTargetRPCInternal((NetworkConnection)(object)target, "System.Void Rift_Sidetrack::TpcPromptConfirmation(Mirror.NetworkConnectionToClient,System.Boolean)", 590471613, (NetworkWriter)(object)val, 0);
		NetworkWriterPool.Return(val);
	}

	[Command(requiresAuthority = false)]
	private void CmdConfirmMove(NetworkConnectionToClient sender = null)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendCommandInternal("System.Void Rift_Sidetrack::CmdConfirmMove(Mirror.NetworkConnectionToClient)", -1791713517, (NetworkWriter)(object)val, 0, false);
		NetworkWriterPool.Return(val);
	}

	public virtual void TravelImmediately()
	{
		_didUse = true;
		if (NetworkedManagerBase<ZoneManager>.instance.isSidetracking)
		{
			NetworkedManagerBase<ZoneManager>.instance.ReturnFromSidetracking();
			return;
		}
		List<string> roomNames = GetRoomNames();
		NetworkedManagerBase<ZoneManager>.instance.LoadSidetrackRoom(roomNames[Random.Range(0, roomNames.Count)]);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_TpcPromptConfirmation__NetworkConnectionToClient__Boolean(NetworkConnectionToClient target, bool isReturning)
	{
		if (NetworkedManagerBase<ZoneManager>.instance.isVoting)
		{
			InGameUIManager.instance.ShowCenterMessage(CenterMessageType.Error, "InGame_Message_AlreadyVotingForTravel");
			return;
		}
		string text = GetConfirmMessage();
		if (text == null)
		{
			text = DewLocalization.GetUIValue(isReturning ? "InGame_Message_ReturnToPreviousWorld" : "InGame_Message_TravelToOtherworld");
		}
		ManagerBase<MessageManager>.instance.ShowMessage(new DewMessageSettings
		{
			rawContent = text,
			buttons = (DewMessageSettings.ButtonType.Yes | DewMessageSettings.ButtonType.No),
			defaultButton = DewMessageSettings.ButtonType.No,
			onClose = (DewMessageSettings.ButtonType b) =>
			{
				if (b == DewMessageSettings.ButtonType.Yes)
				{
					NetworkedManagerBase<ZoneManager>.instance.TravelWithValidationAndConfirmation(() =>
					{
						CmdConfirmMove();
					}, ignoreSidetrack: true);
				}
			},
			validator = () => (Object)(object)this != null && CanInteract(DewPlayer.local.hero) && InGameUIManager.ValidateInGameActionMessage()
		});
	}

	protected static void InvokeUserCode_TpcPromptConfirmation__NetworkConnectionToClient__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TpcPromptConfirmation called on server.");
		}
		else
		{
			((Rift_Sidetrack)(object)obj).UserCode_TpcPromptConfirmation__NetworkConnectionToClient__Boolean((NetworkConnectionToClient)(object)NetworkClient.connection, NetworkReaderExtensions.ReadBool(reader));
		}
	}

	protected void UserCode_CmdConfirmMove__NetworkConnectionToClient(NetworkConnectionToClient sender)
	{
		DewPlayer player = sender.GetPlayer();
		if (!((Object)(object)player == null) && !NetworkedManagerBase<ZoneManager>.instance.isVoting)
		{
			FxPlayNetworked(fxActivate);
			if (NetworkedManagerBase<ZoneManager>.instance.ShouldVoteOnTravel())
			{
				NetworkedManagerBase<ZoneManager>.instance.StartVoteSidetrack(player, this);
			}
			else
			{
				TravelImmediately();
			}
		}
	}

	protected static void InvokeUserCode_CmdConfirmMove__NetworkConnectionToClient(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdConfirmMove called on client.");
		}
		else
		{
			((Rift_Sidetrack)(object)obj).UserCode_CmdConfirmMove__NetworkConnectionToClient(senderConnection);
		}
	}

	static Rift_Sidetrack()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected Obj, but got Unknown
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterCommand(typeof(Rift_Sidetrack), "System.Void Rift_Sidetrack::CmdConfirmMove(Mirror.NetworkConnectionToClient)", (RemoteCallDelegate)InvokeUserCode_CmdConfirmMove__NetworkConnectionToClient, false);
		RemoteProcedureCalls.RegisterRpc(typeof(Rift_Sidetrack), "System.Void Rift_Sidetrack::TpcPromptConfirmation(Mirror.NetworkConnectionToClient,System.Boolean)", (RemoteCallDelegate)InvokeUserCode_TpcPromptConfirmation__NetworkConnectionToClient__Boolean);
	}
}
