using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Mirror;
using Steamworks;
using UnityEngine;

public class DewNetworkAuthenticator : NetworkAuthenticator
{
	private readonly HashSet<NetworkConnection> _connectionsPendingDisconnect = new HashSet<NetworkConnection>();

	public override void OnStartServer()
	{
		NetworkServer.RegisterHandler<DewAuthRequestMessage>((Action<NetworkConnectionToClient, DewAuthRequestMessage>)OnAuthRequestMessage, false);
	}

	public override void OnStopServer()
	{
		NetworkServer.UnregisterHandler<DewAuthRequestMessage>();
	}

	public void OnAuthRequestMessage(NetworkConnectionToClient conn, DewAuthRequestMessage msg)
	{
		DewExceptionType error = DewExceptionType.UnknownError;
		try
		{
			if (_connectionsPendingDisconnect.Contains((NetworkConnection)(object)conn))
			{
				return;
			}
			if (ValidateAuth(msg, out error))
			{
				DewNetworkManager.instance._authMessagesFromClients[conn] = msg;
				((NetworkConnection)conn).Send<DewAuthResponseMessage>(default(DewAuthResponseMessage), 0);
				((NetworkAuthenticator)this).ServerAccept(conn);
				return;
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		_connectionsPendingDisconnect.Add((NetworkConnection)(object)conn);
		DewAuthResponseMessage dewAuthResponseMessage = new DewAuthResponseMessage
		{
			isError = true,
			errorType = error
		};
		((NetworkConnection)conn).Send<DewAuthResponseMessage>(dewAuthResponseMessage, 0);
		((NetworkConnection)conn).isAuthenticated = false;
		((MonoBehaviour)this).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			yield return new WaitForSecondsRealtime(1f);
			((NetworkAuthenticator)this).ServerReject(conn);
			yield return null;
			_connectionsPendingDisconnect.Remove((NetworkConnection)(object)conn);
		}
	}

	private bool ValidateAuth(DewAuthRequestMessage msg, out DewExceptionType error)
	{
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		error = DewExceptionType.UnknownError;
		if (string.IsNullOrEmpty(msg.profileGuid) || (ManagerBase<LobbyManager>.instance.isLobbyLeader && string.IsNullOrEmpty(msg.userId)) || string.IsNullOrEmpty(msg.profileName))
		{
			Debug.LogWarning("Rejecting player for malformed auth data");
			return false;
		}
		if (DewPlayer.allHumanPlayers.Any((DewPlayer h) => h.guid == msg.profileGuid))
		{
			Debug.LogWarning("Rejecting player for GUID collision: " + msg.profileGuid);
			error = DewExceptionType.DuplicatePlayerInGame;
			return false;
		}
		if (DewPlayer.allHumanPlayers.Any((DewPlayer h) => ((object)h.steamId/*cast due to constrained. prefix*/).ToString() == msg.userId))
		{
			Debug.LogWarning("Rejecting player for User ID collision: " + msg.profileGuid);
			error = DewExceptionType.DuplicatePlayerInGame;
			return false;
		}
		if (ManagerBase<LobbyManager>.instance.isLobbyLeader && ManagerBase<LobbyManager>.instance.service is LobbyServiceSteam lobbyServiceSteam)
		{
			CSteamID val = new CSteamID(ulong.Parse(msg.userId, CultureInfo.InvariantCulture));
			if (!lobbyServiceSteam.lobbyMembers.Contains(val))
			{
				Debug.LogWarning($"Rejecting player for user of given SteamID not found in lobby: {val}");
				return false;
			}
		}
		if (NetworkedManagerBase<GameSettingsManager>.instance.allowMidJoins == AllowMidJoinType.RejoinOnly && NetworkedManagerBase<GameSettingsManager>.instance.state == GameState.InGame && !NetworkedManagerBase<GameManager>.instance.playerRejoinData.ContainsKey(msg.profileGuid))
		{
			Debug.LogWarning("Rejecting player for rejoin-only limitation: " + msg.profileGuid);
			error = DewExceptionType.MidJoinOnlyRejoinAllowed;
			return false;
		}
		return true;
	}

	public override void OnStartClient()
	{
		NetworkClient.RegisterHandler<DewAuthResponseMessage>((Action<DewAuthResponseMessage>)OnAuthResponseMessage, false);
	}

	public override void OnStopClient()
	{
		NetworkClient.UnregisterHandler<DewAuthResponseMessage>();
	}

	public override void OnClientAuthenticate()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		NetworkClient.Send<DewAuthRequestMessage>(new DewAuthRequestMessage
		{
			userId = ((object)DewSteam.steamId/*cast due to constrained. prefix*/).ToString(),
			profileGuid = DewSave.profileMain.guid,
			profileName = DewSave.profileMain.name
		}, 0);
	}

	public void OnAuthResponseMessage(DewAuthResponseMessage msg)
	{
		if (msg.isError)
		{
			DewNetworkManager.instance.didRegisterError = true;
			DewSessionError.ShowError(new DewException(msg.errorType));
			((NetworkAuthenticator)this).ClientReject();
			DewNetworkManager.instance.EndSession();
		}
		else
		{
			((NetworkAuthenticator)this).ClientAccept();
		}
	}
}
