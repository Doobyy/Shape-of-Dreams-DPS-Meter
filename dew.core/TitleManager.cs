using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class TitleManager : ManagerBase<TitleManager>, ISettingsChangedCallback
{
	private static bool IsFirstTitleScreenInThisSession;

	private static List<string> ProfileGUIDsWithMasteryRewardShown = new List<string>();

	public ParticleSystem riftParticleSystem;

	public GameObject didPlayTutorialObject;

	public GameObject didNotPlayTutorialObject;

	public UnityEvent onShowLangSelectionView;

	public View langSelectionView;

	public View splashView;

	[NonSerialized]
	public bool didConvertSave;

	private bool _isStartRoutineOver;

	public bool needTutorial
	{
		get
		{
			if (!DewSave.profileMain.didPlayTutorial)
			{
				return !DewSave.profileMain.gameplay.disableTutorial;
			}
			return false;
		}
	}

	[RuntimeInitializeOnLoadMethod]
	private static void Init()
	{
		IsFirstTitleScreenInThisSession = true;
		ProfileGUIDsWithMasteryRewardShown = new List<string>();
	}

	private void Start()
	{
		AchievementManager.lastGamePlayReward = null;
		DewResources.AddPreloadRule(this, (PreloadInterface preload) =>
		{
			foreach (Type allHero in Dew.allHeroes)
			{
				if (Dew.IsHeroIncludedInGame(allHero.Name))
				{
					preload.AddType(allHero.Name);
				}
			}
		});
		DewResources.UnloadUnused();
		OnSettingsChanged();
		if ((UnityEngine.Object)(object)riftParticleSystem != null)
		{
			riftParticleSystem.Simulate(2f);
			riftParticleSystem.Play();
		}
		StartCoroutine(Routine());
		IEnumerator Routine()
		{
			ManagerBase<TransitionManager>.instance.FadeIn();
			Time.timeScale = 1f;
			Resources.UnloadUnusedAssets();
			ManagerBase<UIManager>.instance.SetState("Waiting");
			yield return new WaitForSecondsRealtime(0.5f);
			if (ManagerBase<UGSManager>.instance.status == ServiceStatus.Error)
			{
				ManagerBase<UGSManager>.instance.TryInit();
			}
			if (ManagerBase<EOSManager>.instance.status == ServiceStatus.Error)
			{
				ManagerBase<EOSManager>.instance.TryInit();
			}
			if (DewSave.profileMainPath == null)
			{
				List<DewProfileItem> normalProfiles = DewSave.GetNormalProfiles();
				if (normalProfiles.Count == 1)
				{
					DewSave.LoadProfile(normalProfiles[0].path);
				}
				else if (normalProfiles.Count == 0)
				{
					onShowLangSelectionView.Invoke();
					yield return new WaitWhile(() => langSelectionView.isShowing);
				}
			}
			List<DewProfileItem> profiles = DewSave.GetProfiles();
			if (IsFirstTitleScreenInThisSession && profiles.FindIndex((DewProfileItem p) => p.state == DewProfileState.Convertible) != -1 && !DewSave.platformSettings.dontShowProfileMigration)
			{
				ManagerBase<MessageManager>.instance.ShowMessageLocalized("Title_Profile_Message_HasConvertibleProfile");
				didConvertSave = false;
				ManagerBase<UIManager>.instance.SetState("ProfileSelection");
				yield return new WaitWhile(() => ManagerBase<UIManager>.instance.state == "ProfileSelection");
				if (didConvertSave)
				{
					_isStartRoutineOver = true;
					yield break;
				}
				ManagerBase<MessageManager>.instance.ShowMessage(new DewMessageSettings
				{
					buttons = (DewMessageSettings.ButtonType.Yes | DewMessageSettings.ButtonType.No),
					rawContent = DewLocalization.GetUIValue("Title_Profile_Message_IgnoreConvertibleProfile"),
					defaultButton = DewMessageSettings.ButtonType.No,
					onClose = (DewMessageSettings.ButtonType b) =>
					{
						if (b == DewMessageSettings.ButtonType.Yes)
						{
							ManagerBase<MessageManager>.instance.ShowMessageLocalized("Title_Profile_Message_IgnoreConvertibleProfile_Confirmed");
							DewSave.platformSettings.dontShowProfileMigration = true;
						}
					}
				});
				yield return new WaitWhile(() => ManagerBase<MessageManager>.instance.isShowingMessage);
			}
			else if (DewSave.profileMain != null && DewSave.profileMainPath != null)
			{
				ManagerBase<UIManager>.instance.SetState("Title");
			}
			else
			{
				ManagerBase<UIManager>.instance.SetState("ProfileSelection");
			}
			yield return new WaitWhile(() => !ManagerBase<UIManager>.instance.IsState("Title"));
			if (IsFirstTitleScreenInThisSession)
			{
				ManagerBase<UIManager>.instance.SetState("Waiting");
				splashView.Show();
				yield return new WaitWhile(() => splashView.isShowing);
				ManagerBase<UIManager>.instance.SetState("Title");
			}
			_isStartRoutineOver = true;
		}
	}

	public override void LogicUpdate(float dt)
	{
		base.LogicUpdate(dt);
		if (DewSteam.isInitialized && DewLaunchOptions.connectLobby != null && ManagerBase<UIManager>.instance.IsState("Title") && DewSave.profileMainPath != null)
		{
			ManagerBase<TransitionManager>.instance.PlayGame(new DewNetworkStartSettings
			{
				networkMode = DewNetworkMode.MultiplayerJoinLobby,
				address = DewLaunchOptions.connectLobby
			});
			DewLaunchOptions.connectLobby = null;
			return;
		}
		if (DewSave.profileMainPath != null && _isStartRoutineOver && ManagerBase<UIManager>.instance.IsState("Title"))
		{
			CheckForOtherRoutine();
		}
		didPlayTutorialObject.SetActive(!needTutorial);
		didNotPlayTutorialObject.SetActive(needTutorial);
	}

	private void CheckForOtherRoutine()
	{
		if (!DewSave.profileMain.didRewardMastery && DewSave.profileMain.freeVersionPlayTimeMinutes > 10 && !ProfileGUIDsWithMasteryRewardShown.Contains(DewSave.profileMain.guid))
		{
			ProfileGUIDsWithMasteryRewardShown.Add(DewSave.profileMain.guid);
			ManagerBase<UIManager>.instance.SetState("MasteryReward");
		}
		else if (DewSave.profileMain.lastUnrewardedGameResult != null)
		{
			DewSave.ConsumeGameResult(DewSave.profileMain.lastUnrewardedGameResult, ref AchievementManager.lastGamePlayReward);
			DewSave.profileMain.lastUnrewardedGameResult = null;
		}
		else if (DewEula.needsToAgree)
		{
			ManagerBase<UIManager>.instance.SetState("EULA");
		}
	}

	private void OnDestroy()
	{
		IsFirstTitleScreenInThisSession = false;
	}

	public void JoinLobby(string lobbyId)
	{
		if (ManagerBase<UIManager>.instance.IsState("Title") || ManagerBase<UIManager>.instance.IsState("FindLobby"))
		{
			ManagerBase<TransitionManager>.instance.PlayGame(new DewNetworkStartSettings
			{
				networkMode = DewNetworkMode.MultiplayerJoinLobby,
				address = lobbyId
			});
		}
	}

	public void CheckTutorial(Action callback)
	{
		if (!needTutorial)
		{
			callback?.Invoke();
			return;
		}
		ManagerBase<MessageManager>.instance.ShowMessage(new DewMessageSettings
		{
			buttons = (DewMessageSettings.ButtonType.Yes | DewMessageSettings.ButtonType.No),
			defaultButton = DewMessageSettings.ButtonType.No,
			rawContent = DewLocalization.GetUIValue("Title_Message_TutorialWarning_Ask"),
			onClose = (DewMessageSettings.ButtonType res) =>
			{
				if (res == DewMessageSettings.ButtonType.Yes)
				{
					DewSave.profileMain.didPlayTutorial = true;
					ManagerBase<MessageManager>.instance.ShowMessage(new DewMessageSettings
					{
						buttons = DewMessageSettings.ButtonType.Ok,
						rawContent = DewLocalization.GetUIValue("Title_Message_TutorialWarning_Confirm"),
						onClose = (DewMessageSettings.ButtonType _) =>
						{
							callback?.Invoke();
						}
					});
				}
			}
		});
	}

	public void EnterSingleplayerShapeOfDreams()
	{
		ManagerBase<TransitionManager>.instance.PlayGame(new DewNetworkStartSettings());
	}

	public void EnterSingleplayerLimbo()
	{
		if (!GameMod_Limbo.IsLimboUnlocked())
		{
			ManagerBase<MessageManager>.instance.ShowMessage(new DewMessageSettings
			{
				owner = this,
				rawContent = GameMod_Limbo.GetInsufficientRequirementMessage(),
				buttons = DewMessageSettings.ButtonType.Ok
			});
		}
		else
		{
			ManagerBase<TransitionManager>.instance.PlayGame(new DewNetworkStartSettings
			{
				addedGameMods = new List<string> { "GameMod_Limbo" },
				customGameSettingsSaveKey = "limbo"
			});
		}
	}

	public void EnterContinueDreaming()
	{
		if (ManagerBase<TransitionManager>.instance.state == TransitionManager.StateType.Loading)
		{
			return;
		}
		DewPersistence.GameData gameData = DewPersistence.FromJson<DewPersistence.GameData>(DewSave.profileContinue.continueData);
		if (gameData == null)
		{
			return;
		}
		DewPersistence.PlayerData playerData = gameData.players.Find((DewPersistence.PlayerData p) => p.playerGuid == DewSave.profileMain.guid);
		if (playerData == null)
		{
			playerData = gameData.players[0];
		}
		if (playerData.isHeroKnockedOut)
		{
			playerData.isHeroKnockedOut = false;
			string key = "EntityStatus::currentHealth";
			if (DewPersistence.FromJson<float>(playerData.heroPersistenceData[key]) < 1f)
			{
				playerData.heroPersistenceData[key] = DewPersistence.ToJson(1f);
			}
		}
		CheckOldSave(() =>
		{
			CheckTutorial(() =>
			{
				if (gameData.isMultiplayer && !DewSteam.isInitialized)
				{
					ManagerBase<MessageManager>.instance.ShowMessage(new DewMessageSettings
					{
						owner = this,
						rawContent = string.Format(DewLocalization.GetUIValue("Title_ContinueDreaming_NoConnection"), DewLocalization.GetUIValue("Lobby_Type_" + gameData.lobbyType)),
						defaultButton = DewMessageSettings.ButtonType.Cancel,
						buttons = (DewMessageSettings.ButtonType.Yes | DewMessageSettings.ButtonType.Cancel),
						onClose = (DewMessageSettings.ButtonType b) =>
						{
							if (b == DewMessageSettings.ButtonType.Yes)
							{
								ManagerBase<TransitionManager>.instance.PlayGame(new DewNetworkStartSettings
								{
									networkMode = DewNetworkMode.Singleplayer,
									continueData = gameData
								});
							}
						}
					});
				}
				else if (DewBuildProfile.current.platform == PlatformType.STEAM && DewBuildProfile.current.useSteamLobbyAndRelay)
				{
					LobbyServiceEOS.CROSSPLAY = false;
					ManagerBase<TransitionManager>.instance.PlayGame(new DewNetworkStartSettings
					{
						networkMode = (gameData.isMultiplayer ? DewNetworkMode.MultiplayerHost : DewNetworkMode.Singleplayer),
						continueData = gameData,
						lobbyType = gameData.lobbyType
					});
				}
				else
				{
					LobbyServiceEOS.CROSSPLAY = DewSave.platformSettings.gameplay.enableCrossPlay;
					ManagerBase<TransitionManager>.instance.PlayGame(new DewNetworkStartSettings
					{
						networkMode = (gameData.isMultiplayer ? DewNetworkMode.MultiplayerHost : DewNetworkMode.Singleplayer),
						continueData = gameData,
						lobbyType = gameData.lobbyType
					});
				}
			});
		});
		void CheckOldSave(Action callback)
		{
			if (gameData.shouldShowVersionWarning)
			{
				ManagerBase<MessageManager>.instance.ShowMessage(new DewMessageSettings
				{
					owner = this,
					rawContent = DewLocalization.GetUIValue("Title_ContinueDreaming_OldVersionWarning"),
					buttons = (DewMessageSettings.ButtonType.Yes | DewMessageSettings.ButtonType.Cancel),
					onClose = (DewMessageSettings.ButtonType b) =>
					{
						if (b == DewMessageSettings.ButtonType.Yes)
						{
							callback?.Invoke();
						}
					}
				});
			}
			else
			{
				callback?.Invoke();
			}
		}
	}

	public void EnterTutorial()
	{
		ManagerBase<TransitionManager>.instance.LoadScene("PlayTutorial");
	}

	public void EnterCollectables()
	{
		ManagerBase<TransitionManager>.instance.LoadScene("Collectables");
	}

	public void ExitToDesktop()
	{
		DewSave.SaveProfileAll(immediate: true);
		DewSave.SavePlatformSettings();
		Application.Quit();
	}

	public void OnSettingsChanged()
	{
		if (!DewBuildProfile.current.HasFeature(BuildFeatureTag.Booth))
		{
			return;
		}
		DewSave.profileMain.stardust = DewBuildProfile.current.defaultStardustAmount;
		DewSave.profileMain.didPlayTutorial = false;
		PreferredGameSettings preferredGameSettings = new PreferredGameSettings();
		preferredGameSettings.Validate();
		DewSave.profileMain.preferredGameSettings["default"] = preferredGameSettings;
		DewSave.profileMain.preferredNametag = "";
		foreach (KeyValuePair<string, DewProfile.CosmeticsData> accessory in DewSave.profileMain.accessories)
		{
			Accessory byName = DewResources.GetByName<Accessory>(accessory.Key);
			if (!(byName == null) && !byName.generatedFromServer)
			{
				accessory.Value.isUnlocked = true;
				accessory.Value.isNew = true;
			}
		}
		foreach (KeyValuePair<string, DewProfileStats.HeroData> hero in DewSave.profileStats.heroes)
		{
			hero.Value.completedLimboDepth = UnityEngine.Random.Range(0, GameMod_Limbo.GetMaxDepths());
		}
		List<string> list = DewSave.profileStats.heroes.Keys.ToList();
		list.Shuffle();
		while (list.Count > 3)
		{
			list.RemoveAt(0);
		}
		foreach (string item in list)
		{
			DewSave.profileStats.heroes[item].completedLimboDepth = GameMod_Limbo.GetMaxDepths();
		}
		foreach (KeyValuePair<string, List<HeroLoadoutData>> heroLoadout in DewSave.profileMain.heroLoadouts)
		{
			for (int i = 0; i < heroLoadout.Value.Count; i++)
			{
				heroLoadout.Value[i] = new HeroLoadoutData();
			}
		}
		foreach (KeyValuePair<string, List<string>> heroEquippedAcc in DewSave.profileMain.heroEquippedAccs)
		{
			heroEquippedAcc.Value.Clear();
		}
		string[] array = DewSave.profileMain.heroSelectedSkins.Keys.ToArray();
		foreach (string text in array)
		{
			DewSave.profileMain.heroSelectedSkins[text] = Skin.GetDefaultSkin(text);
		}
		array = DewSave.profileMain.heroUnlockedStarSlots.Keys.ToArray();
		foreach (string key in array)
		{
			DewSave.profileMain.heroUnlockedStarSlots[key] = new DewProfile.HeroStarSlotUnlockData();
		}
		foreach (KeyValuePair<string, DewProfile.CosmeticsData> nametag in DewSave.profileMain.nametags)
		{
			nametag.Value.isUnlocked = false;
		}
		foreach (KeyValuePair<string, DewProfile.CosmeticsData> skin in DewSave.profileMain.skins)
		{
			skin.Value.isUnlocked = false;
		}
		foreach (KeyValuePair<string, DewProfileStats.HeroData> hero2 in DewSave.profileStats.heroes)
		{
			hero2.Value.masteryLevel = UnityEngine.Random.Range(25, 35);
			hero2.Value.currentMasteryPoints = Mathf.RoundToInt((float)Dew.GetRequiredMasteryPointsToLevelUp(hero2.Value.masteryLevel) * UnityEngine.Random.Range(0.1f, 0.8f));
			hero2.Value.totalMasteryPoints = UnityEngine.Random.Range(50000, 100000);
		}
		DewSave.profileMain.Validate();
		DewSave.profileStats.UpdateTotalData(0L);
	}

	public void ShowMods()
	{
		((MonoBehaviour)Dew.FindInterfaceOfType<IModManagerWindow>(includeInactive: true)).gameObject.SetActive(value: true);
	}
}
