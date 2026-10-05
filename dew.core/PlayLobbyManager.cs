using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

public class PlayLobbyManager : ManagerBase<PlayLobbyManager>
{
	private bool _hasStarted;

	public GameObject fxRiftActivate;

	public GameObject fxRiftCharge;

	public GameObject fxRiftOpen;

	public GameObject fxRiftLoop;

	private List<Func<string>> _startGameConditionCheckers = new List<Func<string>>();

	public bool isEveryoneReady => numOfReadyPlayers == numOfReadyPlayersMax;

	public int numOfReadyPlayers { get; private set; }

	public int numOfReadyPlayersMax { get; private set; }

	public void AddStartGameCondition(Func<string> func)
	{
		_startGameConditionCheckers.Add(func);
	}

	public void RemoveStartGameCondition(Func<string> func)
	{
		_startGameConditionCheckers.Remove(func);
	}

	public bool CheckStartGameCondition(out string reason, bool showMessage)
	{
		reason = null;
		foreach (Func<string> startGameConditionChecker in _startGameConditionCheckers)
		{
			try
			{
				reason = startGameConditionChecker();
				if (reason != null)
				{
					if (showMessage)
					{
						ManagerBase<MessageManager>.instance.ShowMessage(new DewMessageSettings
						{
							buttons = DewMessageSettings.ButtonType.Ok,
							rawContent = reason,
							owner = this
						});
					}
					return false;
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
		return true;
	}

	private void Start()
	{
		if (DewBuildProfile.current.HasFeature(BuildFeatureTag.Booth))
		{
			DewSave.profileMain.didReadConstellationNotice = false;
		}
		DewResources.AddPreloadRule(this, (PreloadInterface preload) =>
		{
			preload.KeepEverything();
			foreach (Type allHero in Dew.allHeroes)
			{
				if (Dew.IsHeroIncludedInGame(allHero.Name))
				{
					preload.AddType(allHero.Name);
				}
			}
		});
		DewResources.UnloadUnused();
		NetworkedManagerBase<GameSettingsManager>.instance.ClientEvent_OnStateChanged += new Action(ClientEventOnStateChanged);
	}

	private void OnDestroy()
	{
		if ((UnityEngine.Object)(object)NetworkedManagerBase<GameSettingsManager>.instance != null)
		{
			NetworkedManagerBase<GameSettingsManager>.instance.ClientEvent_OnStateChanged -= new Action(ClientEventOnStateChanged);
		}
	}

	private void ClientEventOnStateChanged()
	{
		if (DewNetworkManager.startSettings.continueData == null)
		{
			if (NetworkedManagerBase<GameSettingsManager>.instance.state == GameState.Starting)
			{
				PlayRiftAnimations();
				LobbyUIManager.instance.SetState("Started");
			}
			else if (NetworkedManagerBase<GameSettingsManager>.instance.state == GameState.InGame)
			{
				DewEffect.Play(fxRiftOpen);
				DewEffect.Play(fxRiftLoop);
			}
		}
	}

	public override void LogicUpdate(float dt)
	{
		base.LogicUpdate(dt);
		numOfReadyPlayers = DewPlayer.gamePlayers.Count;
		numOfReadyPlayersMax = DewPlayer.gamePlayers.Count;
		foreach (DewPlayer lobbyPlayer in DewPlayer.lobbyPlayers)
		{
			if (!lobbyPlayer.isHostPlayer)
			{
				numOfReadyPlayersMax++;
				if (lobbyPlayer.isReady)
				{
					numOfReadyPlayers++;
				}
			}
		}
	}

	public void GoBack()
	{
		ManagerBase<MessageManager>.instance.ShowMessage(new DewMessageSettings
		{
			buttons = (DewMessageSettings.ButtonType.Yes | DewMessageSettings.ButtonType.No),
			defaultButton = DewMessageSettings.ButtonType.No,
			owner = this,
			rawContent = DewLocalization.GetUIValue("PlayLobby_QuitToMenuConfirm"),
			onClose = (DewMessageSettings.ButtonType b) =>
			{
				if (b == DewMessageSettings.ButtonType.Yes)
				{
					DewNetworkManager.instance.EndSession();
				}
			}
		});
	}

	public void SetReady(bool ready)
	{
		if (NetworkServer.active)
		{
			throw new InvalidOperationException();
		}
		DewPlayer.local.CmdSetIsReady(ready);
	}

	public void ToggleReady()
	{
		CheckDejavuCostAndCall(() =>
		{
			if (!NetworkServer.active)
			{
				DewPlayer.local.CmdSetIsReady(!DewPlayer.local.isReady);
			}
		});
	}

	private void PlayRiftAnimations()
	{
		StartCoroutine(Routine());
		IEnumerator Routine()
		{
			DewEffect.Play(fxRiftActivate);
			DewEffect.Play(fxRiftCharge);
			yield return new WaitForSeconds(1.45f);
			DewEffect.Stop(fxRiftCharge);
			DewEffect.Play(fxRiftOpen);
			DewEffect.Play(fxRiftLoop);
		}
	}

	public void JoinMidGame()
	{
		if (NetworkedManagerBase<GameSettingsManager>.instance.midJoinWaitType != MidJoinWaitType.None)
		{
			ManagerBase<MessageManager>.instance.ShowMessageLocalized("Lobby_MidJoin_Wait_" + NetworkedManagerBase<GameSettingsManager>.instance.midJoinWaitType);
			return;
		}
		CheckDejavuCostAndCall(() =>
		{
			if (NetworkedManagerBase<GameSettingsManager>.instance.state == GameState.InGame && !_hasStarted)
			{
				_hasStarted = true;
				LobbyUIManager.instance.SetState("Started");
				DewEffect.Play(fxRiftActivate);
				StartCoroutine(Routine());
			}
		});
		static IEnumerator Routine()
		{
			ManagerBase<TransitionManager>.instance.FadeOut(showTips: true);
			yield return new WaitForSecondsRealtime(1f);
			if (!NetworkClient.ready)
			{
				NetworkClient.Ready();
			}
			DewPlayer.local.CmdRequestMidJoin();
		}
	}

	private void CheckDejavuCostAndCall(Action callback)
	{
		if (string.IsNullOrEmpty(DewPlayer.local.selectedDejavuItem))
		{
			callback();
		}
		else if (NetworkedManagerBase<GameSettingsManager>.instance.bannedGameItems.Contains(DewPlayer.local.selectedDejavuItem))
		{
			ManagerBase<MessageManager>.instance.ShowMessage(new DewMessageSettings
			{
				owner = this,
				rawContent = DewLocalization.GetUIValue("Obliteration_Dejavu_" + (DewPlayer.local.selectedDejavuItem.StartsWith("St_") ? "Memory" : "Essence") + "Obliterated_StartGameConfirm"),
				buttons = (DewMessageSettings.ButtonType.Yes | DewMessageSettings.ButtonType.No),
				onClose = (DewMessageSettings.ButtonType b) =>
				{
					if (b == DewMessageSettings.ButtonType.Yes)
					{
						callback();
					}
				}
			});
		}
		else if (NetworkedManagerBase<GameSettingsManager>.instance.localPlayerDejavuCost > DewSave.profileMain.stardust)
		{
			ManagerBase<MessageManager>.instance.ShowMessage(new DewMessageSettings
			{
				owner = this,
				rawContent = DewLocalization.GetUIValue("Dejavu_InsufficientStardust_Message"),
				buttons = (DewMessageSettings.ButtonType.Yes | DewMessageSettings.ButtonType.No),
				onClose = (DewMessageSettings.ButtonType b) =>
				{
					if (b == DewMessageSettings.ButtonType.Yes)
					{
						DewPlayer.local.CmdSetDejavuItem(null);
						NetworkedManagerBase<GameSettingsManager>.instance.localPlayerDejavuCost = 0;
						callback();
					}
				}
			});
		}
		else
		{
			callback();
		}
	}

	public void StartGame()
	{
		if (!CheckStartGameCondition(out var _, showMessage: true))
		{
			return;
		}
		CheckDejavuCostAndCall(() =>
		{
			if (NetworkServer.active && NetworkedManagerBase<GameSettingsManager>.instance.state == GameState.InLobby && !_hasStarted && isEveryoneReady)
			{
				try
				{
					if (DewSave.profileContinue.continueData != null)
					{
						DewPersistence.GameData gameData = DewPersistence.FromJson<DewPersistence.GameData>(DewSave.profileContinue.continueData);
						if (gameData != null && gameData.Validate())
						{
							ManagerBase<MessageManager>.instance.ShowMessage(new DewMessageSettings
							{
								owner = this,
								validator = () => NetworkServer.active,
								buttons = (DewMessageSettings.ButtonType.Yes | DewMessageSettings.ButtonType.Cancel),
								destructiveConfirm = true,
								defaultButton = DewMessageSettings.ButtonType.Cancel,
								rawContent = DewLocalization.GetUIValue("Menu_Message_ContinueSave_ProgressWillBeLostOnStartNewGame") + "\n\n<color=#fffd7d>" + gameData.GetReadableDescription() + " (" + gameData.GetReadableGameType() + ")</color>",
								onClose = (DewMessageSettings.ButtonType b) =>
								{
									if (b == DewMessageSettings.ButtonType.Yes)
									{
										StartGame_Imp();
									}
								}
							});
							return;
						}
					}
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
				}
				StartGame_Imp();
			}
		});
	}

	private void StartGame_Imp()
	{
		string reason;
		if (!isEveryoneReady)
		{
			ManagerBase<MessageManager>.instance.ShowMessageLocalized("PlayLobby_PlayerNotReady");
		}
		else if (CheckStartGameCondition(out reason, showMessage: true))
		{
			((MonoBehaviour)(object)DewNetworkManager.instance).StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			_hasStarted = true;
			NetworkedManagerBase<GameSettingsManager>.instance.state = GameState.Starting;
			PreferredGameSettings localPreferredGameSettings = NetworkedManagerBase<GameSettingsManager>.instance.GetLocalPreferredGameSettings();
			localPreferredGameSettings.difficulty = NetworkedManagerBase<GameSettingsManager>.instance.difficulty;
			localPreferredGameSettings.lucidDreams = ((IEnumerable<string>)NetworkedManagerBase<GameSettingsManager>.instance.activeLucidDreams).ToList();
			localPreferredGameSettings.bannedGameItems = ((IEnumerable<string>)NetworkedManagerBase<GameSettingsManager>.instance.bannedGameItems).ToList();
			localPreferredGameSettings.customData = new Dictionary<string, string>((IDictionary<string, string>)NetworkedManagerBase<GameSettingsManager>.instance.customData);
			if (!NetworkServer.dontListen)
			{
				localPreferredGameSettings.enableVotes = NetworkedManagerBase<GameSettingsManager>.instance.enableVotes;
				localPreferredGameSettings.allowDejavu = NetworkedManagerBase<GameSettingsManager>.instance.allowDejavu;
				localPreferredGameSettings.allowMidJoins = NetworkedManagerBase<GameSettingsManager>.instance.allowMidJoins;
			}
			NetworkedManagerBase<GameSettingsManager>.instance.Lobby_UpdateCanJoinAndGameStarted(true);
			yield return new WaitForSecondsRealtime(2.1f);
			DewNetworkManager.instance.SetLoadingStatus(isLoading: true);
			yield return new WaitForSecondsRealtime(1f);
			yield return DewNetworkManager.instance.LoadSceneAsync("PlayGame");
			NetworkedManagerBase<GameSettingsManager>.instance.state = GameState.InGame;
		}
	}
}
