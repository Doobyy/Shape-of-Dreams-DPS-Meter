using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class DewInterestManagement : InterestManagement
{
	private bool _isDirty;

	private void Start()
	{
		DewPlayer.onGamePlayerAdded += new Action<DewPlayer>(SetDirty);
		DewPlayer.onGamePlayerRemoved += new Action<DewPlayer>(SetDirty);
		DewPlayer.onSpectatorAdded += new Action<DewPlayer>(SetDirty);
		DewPlayer.onSpectatorRemoved += new Action<DewPlayer>(SetDirty);
		DewPlayer.onLobbyPlayerAdded += new Action<DewPlayer>(SetDirty);
		DewPlayer.onLobbyPlayerRemoved += new Action<DewPlayer>(SetDirty);
	}

	private void OnDestroy()
	{
		DewPlayer.onGamePlayerAdded -= new Action<DewPlayer>(SetDirty);
		DewPlayer.onGamePlayerRemoved -= new Action<DewPlayer>(SetDirty);
		DewPlayer.onSpectatorAdded -= new Action<DewPlayer>(SetDirty);
		DewPlayer.onSpectatorRemoved -= new Action<DewPlayer>(SetDirty);
		DewPlayer.onLobbyPlayerAdded -= new Action<DewPlayer>(SetDirty);
		DewPlayer.onLobbyPlayerRemoved -= new Action<DewPlayer>(SetDirty);
	}

	private void SetDirty(DewPlayer _)
	{
		_isDirty = true;
	}

	private void Update()
	{
		if (!_isDirty)
		{
			return;
		}
		_isDirty = false;
		foreach (KeyValuePair<uint, NetworkIdentity> item in NetworkServer.spawned)
		{
			NetworkServer.RebuildObservers(item.Value, false);
		}
	}

	public override bool OnCheckObserver(NetworkIdentity identity, NetworkConnectionToClient newObserver)
	{
		DewPlayer player = newObserver.GetPlayer();
		if ((UnityEngine.Object)(object)player == null)
		{
			return false;
		}
		PlayerState state = player.state;
		if (state == PlayerState.Spectating || state == PlayerState.Playing)
		{
			return true;
		}
		if (ManagerBase<NetworkLogicPackage>.instance.networkIdentities.Contains(identity))
		{
			return true;
		}
		if (((Component)(object)identity).TryGetComponent(out DewPlayer _))
		{
			return true;
		}
		return false;
	}

	public override void OnRebuildObservers(NetworkIdentity identity, HashSet<NetworkConnectionToClient> newObservers)
	{
		foreach (DewPlayer allHumanPlayer in DewPlayer.allHumanPlayers)
		{
			if (((InterestManagementBase)this).OnCheckObserver(identity, ((NetworkBehaviour)allHumanPlayer).connectionToClient))
			{
				newObservers.Add(allHumanPlayer);
			}
		}
	}

	public override void SetHostVisibility(NetworkIdentity identity, bool visible)
	{
	}
}
