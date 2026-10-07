using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

[DewResourceLink(ResourceLinkBy.Type)]
public class PlayGameManager : GameManager
{
	public new static PlayGameManager instance => NetworkedManagerBase<GameManager>.instance as PlayGameManager;

	public override void OnStartClient()
	{
		base.OnStartClient();
		GameManager.CallOnReady(() =>
		{
			NetworkedManagerBase<GameSettingsManager>.instance.GetLocalPreferredGameSettings().hero = ((object)DewPlayer.local.hero).GetType().Name;
			if (((NetworkBehaviour)this).isServer && DewNetworkManager.startSettings.continueData == null)
			{
				NetworkedManagerBase<QuestManager>.instance.StartQuest<Quest_ShapeOfDreams>();
			}
		});
		if (DewBuildProfile.current.HasFeature(BuildFeatureTag.Booth))
		{
			GameManager.CallOnReady(() =>
			{
				Dew.GetControlPresetWindow().Show(showCancel: true);
			});
		}
		((MonoBehaviour)(object)this).StartCoroutine(IncrementPlaytime());
		IEnumerator IncrementPlaytime()
		{
			while (NetworkClient.active)
			{
				yield return new WaitForSeconds(60f);
				if (!isGameTimePaused)
				{
					DewSave.profileMain.totalPlayTimeMinutes++;
				}
			}
		}
	}

	public void SpawnHero(DewPlayer player, Hero hero, HeroLoadoutData loadout, string skin, List<string> accs)
	{
		if ((UnityEngine.Object)(object)player.hero != null)
		{
			Dew.Destroy(((Component)(object)player.hero).gameObject);
		}
		Vector3 position = (((UnityEngine.Object)(object)NetworkedManagerBase<ZoneManager>.instance.currentRoom == null) ? Vector3.zero : NetworkedManagerBase<ZoneManager>.instance.currentRoom.GetHeroSpawnPosition());
		Hero hero2 = Dew.SpawnHero(hero, position, Quaternion.identity, player, 1, loadout, skin, (Hero h) =>
		{
			h.accessories.AddRange((IEnumerable<string>)accs);
		});
		if (DewBuildProfile.current.bonusMemoryHaste > 0.1f)
		{
			hero2.Status.AddStatBonus(new StatBonus
			{
				abilityHasteFlat = DewBuildProfile.current.bonusMemoryHaste
			});
		}
		player.hero = hero2;
		player.controllingEntity = hero2;
	}

	public override void OnLateStartServer()
	{
		base.OnLateStartServer();
		((MonoBehaviour)(object)this).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			yield return Dew.WaitForClientsReadyRoutine();
			if (DewNetworkManager.startSettings.continueData != null)
			{
				Debug.Log("Loading game from continue data");
				DewPlayer[] array = DewPlayer.lobbyPlayers.ToArray();
				foreach (DewPlayer obj in array)
				{
					obj.joinedMidGame = false;
					obj.state = PlayerState.Playing;
				}
				DewPersistence.ApplyGameData(DewNetworkManager.startSettings.continueData, () =>
				{
					Debug.Log("Load game finished");
					DewNetworkManager.instance.SetLoadingStatus(isLoading: false);
				});
			}
			else
			{
				DewPlayer[] array = DewPlayer.lobbyPlayers.ToArray();
				foreach (DewPlayer dewPlayer in array)
				{
					dewPlayer.joinedMidGame = false;
					SpawnHero(dewPlayer, DewResources.GetByShortTypeName<Hero>(dewPlayer.selectedHeroType, default(ResourceLoadSettings)), dewPlayer.selectedLoadout, dewPlayer.selectedSkin, ((IEnumerable<string>)dewPlayer.selectedAccessories).ToList());
					dewPlayer.state = PlayerState.Playing;
				}
				yield return new WaitForSecondsRealtime(0.1f);
				elapsedGameTime = 0f;
				LoadNextZone();
				if (DewBuildProfile.current.buildType != BuildType.DemoLite)
				{
					NetworkedManagerBase<ZoneManager>.instance.CallOnReadyAfterTransition(() =>
					{
						foreach (DewPlayer gamePlayer in DewPlayer.gamePlayers)
						{
							DoDejavuSpawn(gamePlayer);
						}
					});
				}
			}
		}
	}

	public void DoDejavuSpawn(DewPlayer h)
	{
		try
		{
			if (!NetworkedManagerBase<GameSettingsManager>.instance.allowDejavu)
			{
				return;
			}
			if ((UnityEngine.Object)(object)h.hero == null || string.IsNullOrEmpty(h.selectedDejavuItem))
			{
				h.TpcNotifyDejavuUse();
			}
			else
			{
				if (NetworkedManagerBase<GameSettingsManager>.instance.bannedGameItems.Contains(h.selectedDejavuItem))
				{
					return;
				}
				Vector3 goodRewardPosition = Dew.GetGoodRewardPosition(h.hero.agentPosition);
				if (h.selectedDejavuItem.StartsWith("St_"))
				{
					if (!Dew.IsSkillIncludedInGame(h.selectedDejavuItem))
					{
						return;
					}
					SkillTrigger byShortTypeName = DewResources.GetByShortTypeName<SkillTrigger>(h.selectedDejavuItem, default(ResourceLoadSettings));
					if (byShortTypeName.excludeFromPool)
					{
						return;
					}
					Rarity rarity = byShortTypeName.rarity;
					if (rarity == Rarity.Character || rarity == Rarity.Identity)
					{
						return;
					}
					Dew.CreateSkillTrigger(byShortTypeName, goodRewardPosition, 1, h);
				}
				else
				{
					if (!h.selectedDejavuItem.StartsWith("Gem_") || !Dew.IsGemIncludedInGame(h.selectedDejavuItem))
					{
						return;
					}
					Gem byShortTypeName2 = DewResources.GetByShortTypeName<Gem>(h.selectedDejavuItem, default(ResourceLoadSettings));
					if (byShortTypeName2.excludeFromPool)
					{
						return;
					}
					Dew.CreateGem(byShortTypeName2, goodRewardPosition, 100, h);
				}
				h.TpcNotifyDejavuUse();
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	public override void LoadNextZone()
	{
		base.LoadNextZone();
		if (NetworkedManagerBase<ZoneManager>.instance.currentZone != null && (DewBuildProfile.current.buildType == BuildType.DemoLite || DewBuildProfile.current.HasFeature(BuildFeatureTag.Booth)))
		{
			int num = DewBuildProfile.current.content.zoneCountByTier.Sum();
			if (NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex >= num - 1)
			{
				ConcludeUnknownFate();
				return;
			}
		}
		NetworkedManagerBase<ZoneManager>.instance.LoadNextZoneByContentSettings();
	}

	protected override DewDifficultySettings GetDifficulty()
	{
		return DewResources.GetByName<DewDifficultySettings>(NetworkedManagerBase<GameSettingsManager>.instance.difficulty);
	}

	public override bool IsContinueSaveSupported()
	{
		return !DewBuildProfile.current.HasFeature(BuildFeatureTag.Booth);
	}

	private void MirrorProcessed()
	{
	}
}
