using Mirror;
using UnityEngine;

public class LobbyUIManager : UIManager
{
	public GameObject lobbyEffect;

	public new static LobbyUIManager instance => ManagerBase<UIManager>.instance as LobbyUIManager;

	public new static LobbyUIManager softInstance => ManagerBase<UIManager>.softInstance as LobbyUIManager;

	private void Start()
	{
		UpdateUIStatus();
		DewEffect.Play(lobbyEffect);
		ManagerBase<GlobalUIManager>.instance.AddBackHandler(this, 1, () =>
		{
			if (DewInput.currentMode != InputMode.Gamepad)
			{
				return false;
			}
			if (state != "Lobby")
			{
				return false;
			}
			ManagerBase<PlayLobbyManager>.instance.GoBack();
			return true;
		});
	}

	public override void LogicUpdate(float dt)
	{
		base.LogicUpdate(dt);
		UpdateUIStatus();
	}

	private void UpdateUIStatus()
	{
		int num;
		if ((NetworkServer.active || NetworkClient.active) && !NetworkClient.isConnecting && !((Object)(object)DewPlayer.local == null))
		{
			num = ((!DewPlayer.local.isEveryInfoSet) ? 1 : 0);
			if (num == 0)
			{
				goto IL_0052;
			}
		}
		else
		{
			num = 1;
		}
		if (state != "Loading")
		{
			SetState("Loading");
		}
		goto IL_0052;
		IL_0052:
		if (num == 0 && state == "Loading")
		{
			SetState("Lobby");
		}
		if ((Object)(object)DewPlayer.local != null && DewPlayer.local.isWaitingForContinueMidJoin && state != "WaitingForContinueMidJoin")
		{
			SetState("WaitingForContinueMidJoin");
		}
	}

	protected override void OnStateChanged(string oldState, string newState)
	{
		base.OnStateChanged(oldState, newState);
		if (newState == "Constellations" && oldState != "Constellations")
		{
			DewEffect.Stop(lobbyEffect);
		}
		else if (newState != "Constellations" && oldState == "Constellations")
		{
			DewEffect.Play(lobbyEffect);
		}
	}

	public void OpenCharacterSelection()
	{
		if (!((Object)(object)DewPlayer.local == null) && !DewPlayer.local.isReady)
		{
			SetState("Character");
		}
	}
}
