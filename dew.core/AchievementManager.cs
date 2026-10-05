using System;
using System.Collections.Generic;
using Mirror;
using Steamworks;
using UnityEngine;

public class AchievementManager : ManagerBase<AchievementManager>
{
	public static LastGamePlayReward lastGamePlayReward;

	public List<DewAchievementItem> trackedAchievements = new List<DewAchievementItem>();

	public SafeAction<Type> LocalClientEvent_OnAchievementComplete;

	public SafeAction<int, string> LocalClientEvent_OnLimboDepthComplete;

	public bool disableTrackingAchievements;

	public bool isTrackingAchievements { get; private set; }

	private void Start()
	{
		if (disableTrackingAchievements)
		{
			return;
		}
		lastGamePlayReward = null;
		NetworkedManagerBase<GameResultManager>.instance.ClientEvent_OnGameConcluded += (Action<DewGameResult>)((DewGameResult obj) =>
		{
			if (isTrackingAchievements)
			{
				for (int num = trackedAchievements.Count - 1; num >= 0; num--)
				{
					trackedAchievements[num].FeedGameResult(obj);
				}
				StopTrackingAchievements();
				DeleteLocalContinueData();
			}
		});
		NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded += (Action<EventInfoLoadRoom>)((EventInfoLoadRoom _) =>
		{
			if (isTrackingAchievements)
			{
				SaveLocalContinueData();
				if (FlushAchievementProgress())
				{
					DewSave.SaveProfileMain();
				}
			}
		});
		NetworkedManagerBase<QuestManager>.instance.ClientEvent_OnArtifactAppraised += new Action<string, bool>(ClientEventOnArtifactAppraised);
		DewNetworkManager.instance.ClientEvent_OnSessionEnd += (Action)(() =>
		{
			if (isTrackingAchievements)
			{
				SaveLocalContinueData();
				StopTrackingAchievements();
				DewSave.SaveProfileMain();
			}
		});
		GameManager.CallOnReady(() =>
		{
			StartTrackingAchievements();
			DewPlayer.local.ClientEvent_OnHeroChanged += (Action<Hero, Hero>)((Hero _, Hero to) =>
			{
				if (isTrackingAchievements)
				{
					StopTrackingAchievements();
					StartTrackingAchievements();
				}
			});
		});
		NetworkedManagerBase<ClientEventManager>.instance.OnDismantled += new Action<Hero, NetworkBehaviour>(Dismantled);
		DewSteam.onGameOverlayShownChanged += new Action<bool>(OnGameOverlayShownChanged);
	}

	private void OnGameOverlayShownChanged(bool obj)
	{
		if (obj)
		{
			SetPlatformStats();
		}
	}

	private void OnApplicationFocus(bool hasFocus)
	{
		SetPlatformStats();
	}

	private void ClientEventOnArtifactAppraised(string artifactName, bool isNew)
	{
		if (lastGamePlayReward != null && isNew)
		{
			lastGamePlayReward.unlockedArtifacts.Add(artifactName);
		}
	}

	private void OnDestroy()
	{
		if (isTrackingAchievements)
		{
			StopTrackingAchievements();
		}
		DewSteam.onGameOverlayShownChanged -= new Action<bool>(OnGameOverlayShownChanged);
	}

	public void StartTrackingAchievements()
	{
		if (isTrackingAchievements)
		{
			return;
		}
		isTrackingAchievements = true;
		if (lastGamePlayReward == null)
		{
			lastGamePlayReward = new LastGamePlayReward();
		}
		if ((UnityEngine.Object)(object)DewPlayer.local == null || (UnityEngine.Object)(object)DewPlayer.local.hero == null)
		{
			return;
		}
		foreach (KeyValuePair<string, DewProfile.AchievementData> achievement in DewSave.profileMain.achievements)
		{
			if ((!achievement.Value.isCompleted || DewBuildProfile.current.HasFeature(BuildFeatureTag.Booth)) && Dew.IsAchievementIncludedInGame(achievement.Key))
			{
				DewAchievementItem item = (DewAchievementItem)Activator.CreateInstance(Dew.achievementsByName[achievement.Key]);
				trackedAchievements.Add(item);
			}
		}
		for (int num = trackedAchievements.Count - 1; num >= 0; num--)
		{
			DewAchievementItem dewAchievementItem = trackedAchievements[num];
			try
			{
				dewAchievementItem.OnStartLocalClient();
			}
			catch (Exception exception)
			{
				Debug.LogError("Exception occured while starting achievement: " + dewAchievementItem.name);
				Debug.LogException(exception, this);
				trackedAchievements.RemoveAt(num);
			}
		}
		DewPlayer.local.hero.Skill.ClientHeroEvent_OnSkillPickup += new Action<SkillTrigger>(ClientHeroEventOnSkillPickup);
		DewPlayer.local.hero.Skill.ClientHeroEvent_OnGemPickup += new Action<Gem>(ClientHeroEventOnGemPickup);
		CheckAndApplyLocalContinueData();
		Debug.Log($"Started tracking {trackedAchievements.Count} achievements");
		try
		{
			for (int num2 = trackedAchievements.Count - 1; num2 >= 0; num2--)
			{
				DewAchievementItem dewAchievementItem2 = trackedAchievements[num2];
				if (dewAchievementItem2.GetCurrentProgress() >= dewAchievementItem2.GetMaxProgress())
				{
					CompleteAchievement(dewAchievementItem2);
				}
			}
		}
		catch (Exception exception2)
		{
			Debug.LogException(exception2);
		}
	}

	private void Dismantled(Hero hero, NetworkBehaviour item)
	{
		if (!((UnityEngine.Object)(object)DewPlayer.local == null) && !((UnityEngine.Object)(object)hero != (UnityEngine.Object)(object)DewPlayer.local.hero))
		{
			if (item is SkillTrigger && DewSave.profileMain.DiscoverSkill(((object)item).GetType().Name))
			{
				lastGamePlayReward?.discoveredSkills.Add(((object)item).GetType());
			}
			else if (item is Gem && DewSave.profileMain.DiscoverGem(((object)item).GetType().Name))
			{
				lastGamePlayReward?.discoveredGems.Add(((object)item).GetType());
			}
		}
	}

	private void ClientHeroEventOnGemPickup(Gem obj)
	{
		if (DewSave.profileMain.DiscoverGem(((object)obj).GetType().Name))
		{
			lastGamePlayReward?.discoveredGems.Add(((object)obj).GetType());
		}
	}

	private void ClientHeroEventOnSkillPickup(SkillTrigger obj)
	{
		if (DewSave.profileMain.DiscoverSkill(((object)obj).GetType().Name))
		{
			lastGamePlayReward?.discoveredSkills.Add(((object)obj).GetType());
		}
	}

	public void SetPlatformStats()
	{
		if (!isTrackingAchievements || !DewSteam.isInitialized)
		{
			return;
		}
		foreach (DewAchievementItem trackedAchievement in trackedAchievements)
		{
			SteamUserStats.SetStat("STAT_" + trackedAchievement.name, trackedAchievement.GetCurrentProgress());
		}
	}

	public bool FlushAchievementProgress()
	{
		bool result = false;
		foreach (DewAchievementItem trackedAchievement in trackedAchievements)
		{
			try
			{
				if (trackedAchievement.FlushProgressToProfile(midRun: true))
				{
					result = true;
				}
			}
			catch (Exception exception)
			{
				Debug.LogError("Exception occured while flushing achievement: " + trackedAchievement.name);
				Debug.LogException(exception, this);
			}
		}
		return result;
	}

	public void StopTrackingAchievements()
	{
		if (!isTrackingAchievements)
		{
			return;
		}
		isTrackingAchievements = false;
		foreach (DewAchievementItem trackedAchievement in trackedAchievements)
		{
			try
			{
				trackedAchievement.OnStopLocalClient();
			}
			catch (Exception exception)
			{
				Debug.LogError("Exception occured while stopping achievement: " + trackedAchievement.name);
				Debug.LogException(exception, this);
			}
		}
		if ((UnityEngine.Object)(object)DewPlayer.local != null && (UnityEngine.Object)(object)DewPlayer.local.hero != null)
		{
			DewPlayer.local.hero.Skill.ClientHeroEvent_OnSkillPickup -= new Action<SkillTrigger>(ClientHeroEventOnSkillPickup);
			DewPlayer.local.hero.Skill.ClientHeroEvent_OnGemPickup -= new Action<Gem>(ClientHeroEventOnGemPickup);
		}
		if (DewSteam.isInitialized)
		{
			SteamUserStats.StoreStats();
		}
		Debug.Log($"Stopped tracking {trackedAchievements.Count} achievements");
		trackedAchievements.Clear();
	}

	public void CompleteAchievement(DewAchievementItem item)
	{
		string text = item.name;
		if (!DewBuildProfile.current.HasFeature(BuildFeatureTag.Booth) && DewSave.profileMain.achievements[text].isCompleted)
		{
			Debug.LogWarning("Tried to complete already completed achievement: " + item.name);
			return;
		}
		if (!trackedAchievements.Contains(item))
		{
			Debug.LogWarning("Tried to complete non-tracked achievement: " + item.name);
			return;
		}
		Debug.Log("Achievement completed: " + text);
		int maxProgress = item.GetMaxProgress();
		DewProfile.AchievementData achievementData = DewSave.profileMain.achievements[text];
		achievementData.isNew = true;
		achievementData.isCompleted = true;
		achievementData.completeTimestamp = DateTime.UtcNow.ToTimestamp();
		achievementData.maxProgress = maxProgress;
		achievementData.currentProgress = maxProgress;
		achievementData.persistentVariables = null;
		if (DewSteam.isInitialized)
		{
			SteamUserStats.SetStat("STAT_" + item.GetType().Name, maxProgress);
			SteamUserStats.SetAchievement(item.GetType().Name);
			SteamUserStats.StoreStats();
		}
		foreach (Type item2 in Dew.GetUnlockedTargetsOfAchievement(item.GetType()))
		{
			if (item2.IsSubclassOf(typeof(Hero)))
			{
				if (DewSave.profileMain.heroes[item2.Name].status != UnlockStatus.Locked)
				{
					Debug.LogWarning("Tried to unlock already unlocked hero: " + item2.Name);
					continue;
				}
				Debug.Log("New hero unlocked: " + item2.Name);
				DewSave.profileMain.UnlockHero(item2.Name);
			}
			else if (item2.IsSubclassOf(typeof(SkillTrigger)))
			{
				if (DewSave.profileMain.skills[item2.Name].status != UnlockStatus.Locked)
				{
					Debug.LogWarning("Tried to unlock already unlocked skill: " + item2.Name);
					continue;
				}
				Debug.Log("New skill unlocked: " + item2.Name);
				DewSave.profileMain.UnlockSkill(item2.Name);
			}
			else if (item2.IsSubclassOf(typeof(Gem)))
			{
				if (DewSave.profileMain.gems[item2.Name].status != UnlockStatus.Locked)
				{
					Debug.LogWarning("Tried to unlock already unlocked gem: " + item2.Name);
					continue;
				}
				Debug.Log("New gem unlocked: " + item2.Name);
				DewSave.profileMain.UnlockGem(item2.Name);
			}
			else if (item2.IsSubclassOf(typeof(LucidDream)))
			{
				if (DewSave.profileMain.lucidDreams[item2.Name].status != UnlockStatus.Locked)
				{
					Debug.LogWarning("Tried to unlock already unlocked lucid dream: " + item2.Name);
					continue;
				}
				Debug.Log("New lucid dream unlocked: " + item2.Name);
				DewSave.profileMain.UnlockLucidDream(item2.Name);
			}
			else
			{
				Debug.LogWarning("Tried to unlock target of unknown type: " + item2.Name);
			}
		}
		try
		{
			item.OnStopLocalClient();
		}
		catch (Exception exception)
		{
			Debug.LogError("Exception occured while stopping achievement: " + item.name);
			Debug.LogException(exception, this);
		}
		DewSave.SaveProfileMain();
		trackedAchievements.Remove(item);
		try
		{
			LocalClientEvent_OnAchievementComplete?.Invoke(item.GetType());
		}
		catch (Exception exception2)
		{
			Debug.LogException(exception2, this);
		}
		if (lastGamePlayReward != null)
		{
			lastGamePlayReward.unlockedAchievements.Add(item.GetType());
		}
		DewPlayer.local.CmdRequestStardust(item.grantedStardust);
		NetworkedManagerBase<ChatManager>.instance.CmdSendAchievementMessage(item.name);
	}

	public void SaveLocalContinueData()
	{
		DewPersistence.AchievementsData item = DewPersistence.SerializeAchievementData();
		for (int i = 0; i < DewSave.profileContinue.achContinueData.Count; i++)
		{
			if (!(DewSave.profileContinue.achContinueData[i].runId != NetworkedManagerBase<GameManager>.instance.runId))
			{
				DewSave.profileContinue.achContinueData.RemoveAt(i);
				break;
			}
		}
		DewSave.profileContinue.achContinueData.Insert(0, item);
		while (DewSave.profileContinue.achContinueData.Count > 5)
		{
			DewSave.profileContinue.achContinueData.RemoveAt(DewSave.profileContinue.achContinueData.Count - 1);
		}
	}

	public void DeleteLocalContinueData()
	{
		for (int num = DewSave.profileContinue.achContinueData.Count - 1; num >= 0; num--)
		{
			if (DewSave.profileContinue.achContinueData[num].runId == NetworkedManagerBase<GameManager>.instance.runId)
			{
				DewSave.profileContinue.achContinueData.RemoveAt(num);
			}
		}
	}

	public void CheckAndApplyLocalContinueData()
	{
		foreach (DewPersistence.AchievementsData achContinueDatum in DewSave.profileContinue.achContinueData)
		{
			if (achContinueDatum.runId == NetworkedManagerBase<GameManager>.instance.runId)
			{
				Debug.Log("Local achievement continue data found. Applying...");
				DewPersistence.ApplyAchievementData(achContinueDatum);
				return;
			}
		}
		Debug.Log("No local achievement continue data found.");
	}
}
