using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;

public class DewProfile
{
	public class HeroStarSlotUnlockData
	{
		public List<int> addedDestruction = new List<int>();

		public List<int> addedLife = new List<int>();

		public List<int> addedImagination = new List<int>();

		public List<int> addedFlexible = new List<int>();

		public List<int> Get(StarType type)
		{
			return type switch
			{
				StarType.Life => addedLife, 
				StarType.Destruction => addedDestruction, 
				StarType.Imagination => addedImagination, 
				StarType.Flexible => addedFlexible, 
				_ => throw new ArgumentOutOfRangeException("type", type, null), 
			};
		}

		public void Set(StarType type, List<int> list)
		{
			switch (type)
			{
			case StarType.Life:
				addedLife = list;
				break;
			case StarType.Destruction:
				addedDestruction = list;
				break;
			case StarType.Imagination:
				addedImagination = list;
				break;
			case StarType.Flexible:
				addedFlexible = list;
				break;
			default:
				throw new ArgumentOutOfRangeException("type", type, null);
			}
		}
	}

	public class AchievementData
	{
		public bool isNew;

		public bool isCompleted;

		public int currentProgress;

		public int maxProgress;

		public long completeTimestamp;

		public Dictionary<string, string> persistentVariables;
	}

	public class DailyReverieData : ReverieDataBase
	{
		public long nextRefillTimestamp;

		public bool wasNeverFilled;
	}

	public class SpecialReverieData : ReverieDataBase
	{
		public long timeLimitTimestamp;
	}

	public class ReverieDataBase
	{
		public string type;

		public bool isComplete;

		public int currentProgress;

		public int maxProgress;

		public int grantedStardust;

		public string[] grantedItems;

		public Dictionary<string, string> persistentVariables;

		public bool IsEmpty()
		{
			return string.IsNullOrEmpty(type);
		}
	}

	public class UnlockData
	{
		public UnlockStatus status;

		public bool didReadMemory;

		public bool isNewHeroOrHeroSkill;

		public bool isAvailableInGame => status != UnlockStatus.Locked;
	}

	public class PurchaseData
	{
		public long timestamp;

		public string item;

		public int stardust;

		public PurchaseData(string item, int stardust)
		{
			this.item = item;
			this.stardust = stardust;
			timestamp = DateTime.UtcNow.ToTimestamp();
		}
	}

	public class CosmeticsData
	{
		public bool isUnlocked;

		public bool isNew;

		public long unlockDate;

		public string ownershipKey;
	}

	public class StarData
	{
		public int level;

		public StarData Clone()
		{
			return (StarData)MemberwiseClone();
		}

		public bool IsUnchanged(StarData other)
		{
			return level == other.level;
		}
	}

	public class ConcededGameData
	{
		public DewGameResult result;

		public string heroType;

		public long receivedTravelerMastery;
	}

	public class StoryNodeUnlockData
	{
		public bool isUnlocked;

		public int lastReadChunk = -1;

		public List<StoryChunkData> chunks;
	}

	public class StoryChunkData
	{
		public bool isUnlocked;

		public int lastPage;
	}

	public const int CurrentSaveVersion = 10;

	private static readonly HashSet<string> ProfileNameDisallowedTags = new HashSet<string>
	{
		"size", "voffset", "pos", "space", "cspace", "mspace", "indent", "line-indent", "line-height", "margin",
		"width", "align", "rotate", "font", "material", "style", "br", "nobr", "page", "noparse",
		"link"
	};

	public string guid;

	public string lastSteamId;

	public string name;

	public long creationDate = DateTime.UtcNow.ToTimestamp();

	public long totalPlayTimeMinutes;

	public string language = "";

	public int stardust;

	public int spentStardust;

	public string preferredNametag = "";

	public Dictionary<string, PreferredGameSettings> preferredGameSettings = new Dictionary<string, PreferredGameSettings>();

	public int saveVersion;

	public List<string> equippedEmotes = new List<string>();

	public int completedReveries;

	public SpecialReverieData specialReverie = new SpecialReverieData
	{
		type = null
	};

	public List<DailyReverieData> reverieSlots = new List<DailyReverieData>();

	public List<string> lastReverieTypes = new List<string>();

	public int remainingRerolls;

	public long nextRerollReplenishTimestamp;

	public bool didReadLoopNotice;

	public bool didReadConstellationNotice;

	public bool didReadPrivateDemoNotice;

	public bool didPlayTutorial;

	public List<string> experienceFlags = new List<string>();

	public bool didMeetDreamTeller;

	public long freeVersionPlayTimeMinutes;

	public bool didRewardMastery;

	public DewGameplaySettings_User gameplay = new DewGameplaySettings_User();

	public DewControlSettings_User controls = new DewControlSettings_User();

	public DewAudioSettings_User audio = new DewAudioSettings_User();

	public Dictionary<string, List<HeroLoadoutData>> heroLoadouts = new Dictionary<string, List<HeroLoadoutData>>();

	public Dictionary<string, List<string>> heroEquippedAccs = new Dictionary<string, List<string>>();

	public Dictionary<string, string> heroSelectedSkins = new Dictionary<string, string>();

	public Dictionary<string, HeroStarSlotUnlockData> heroUnlockedStarSlots = new Dictionary<string, HeroStarSlotUnlockData>();

	public Dictionary<string, StoryNodeUnlockData> storyNodes = new Dictionary<string, StoryNodeUnlockData>();

	public Dictionary<string, AchievementData> achievements = new Dictionary<string, AchievementData>();

	public Dictionary<string, StarData> stars = new Dictionary<string, StarData>();

	public Dictionary<string, StarData> newStars = new Dictionary<string, StarData>();

	public Dictionary<string, CosmeticsData> emotes = new Dictionary<string, CosmeticsData>();

	public Dictionary<string, CosmeticsData> accessories = new Dictionary<string, CosmeticsData>();

	public Dictionary<string, CosmeticsData> nametags = new Dictionary<string, CosmeticsData>();

	public Dictionary<string, CosmeticsData> skins = new Dictionary<string, CosmeticsData>();

	public Dictionary<string, UnlockData> heroes = new Dictionary<string, UnlockData>();

	public Dictionary<string, UnlockData> skills = new Dictionary<string, UnlockData>();

	public Dictionary<string, UnlockData> gems = new Dictionary<string, UnlockData>();

	public Dictionary<string, UnlockData> artifacts = new Dictionary<string, UnlockData>();

	public Dictionary<string, UnlockData> lucidDreams = new Dictionary<string, UnlockData>();

	public List<DewGameResult> favoriteGameResults = new List<DewGameResult>();

	public List<DewGameResult> lastGameResults = new List<DewGameResult>();

	public DewGameResult lastUnrewardedGameResult;

	public List<ConcededGameData> recentlyConcededGames = new List<ConcededGameData>();

	public Dictionary<string, long> dejavuCostReductionPeriodTimestamp = new Dictionary<string, long>();

	public List<string> doneTutorials = new List<string>();

	public List<string> seenGuides = new List<string>();

	public Dictionary<string, int> receivedLevelUpRewards = new Dictionary<string, int>();

	public int maxObliterationSlots;

	public List<PurchaseData> stardustPurchases = new List<PurchaseData>();

	public long lastUpsert;

	public void Initialize()
	{
		stardust = DewBuildProfile.current.defaultStardustAmount;
		saveVersion = 10;
		guid = Guid.NewGuid().ToString();
		gameplay.Initialize();
		maxObliterationSlots = Dew.GetDefaultObliterationSlots();
	}

	public void Validate()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0405: Unknown result type (might be due to invalid IL or missing references)
		//IL_041b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0420: Unknown result type (might be due to invalid IL or missing references)
		//IL_21ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_2204: Unknown result type (might be due to invalid IL or missing references)
		//IL_2316: Unknown result type (might be due to invalid IL or missing references)
		//IL_231b: Unknown result type (might be due to invalid IL or missing references)
		//IL_242d: Unknown result type (might be due to invalid IL or missing references)
		//IL_2432: Unknown result type (might be due to invalid IL or missing references)
		//IL_2544: Unknown result type (might be due to invalid IL or missing references)
		//IL_2549: Unknown result type (might be due to invalid IL or missing references)
		//IL_2939: Unknown result type (might be due to invalid IL or missing references)
		if (string.IsNullOrEmpty(guid) || (DewSteam.isInitialized && ((object)DewSteam.steamId/*cast due to constrained. prefix*/).ToString() != lastSteamId))
		{
			guid = Guid.NewGuid().ToString();
		}
		if (DewSteam.isInitialized)
		{
			lastSteamId = ((object)DewSteam.steamId/*cast due to constrained. prefix*/).ToString();
		}
		if (gameplay == null)
		{
			gameplay = new DewGameplaySettings_User();
		}
		if (controls == null)
		{
			controls = new DewControlSettings_User();
		}
		if (audio == null)
		{
			audio = new DewAudioSettings_User();
		}
		if (heroLoadouts == null)
		{
			heroLoadouts = new Dictionary<string, List<HeroLoadoutData>>();
		}
		if (heroEquippedAccs == null)
		{
			heroEquippedAccs = new Dictionary<string, List<string>>();
		}
		if (heroSelectedSkins == null)
		{
			heroSelectedSkins = new Dictionary<string, string>();
		}
		if (heroUnlockedStarSlots == null)
		{
			heroUnlockedStarSlots = new Dictionary<string, HeroStarSlotUnlockData>();
		}
		if (achievements == null)
		{
			achievements = new Dictionary<string, AchievementData>();
		}
		if (stars == null)
		{
			stars = new Dictionary<string, StarData>();
		}
		if (heroes == null)
		{
			heroes = new Dictionary<string, UnlockData>();
		}
		if (skills == null)
		{
			skills = new Dictionary<string, UnlockData>();
		}
		if (gems == null)
		{
			gems = new Dictionary<string, UnlockData>();
		}
		if (artifacts == null)
		{
			artifacts = new Dictionary<string, UnlockData>();
		}
		if (lucidDreams == null)
		{
			lucidDreams = new Dictionary<string, UnlockData>();
		}
		if (doneTutorials == null)
		{
			doneTutorials = new List<string>();
		}
		if (seenGuides == null)
		{
			seenGuides = new List<string>();
		}
		if (reverieSlots == null)
		{
			reverieSlots = new List<DailyReverieData>();
		}
		if (lastReverieTypes == null)
		{
			lastReverieTypes = new List<string>();
		}
		if (specialReverie == null)
		{
			specialReverie = new SpecialReverieData();
		}
		if (dejavuCostReductionPeriodTimestamp == null)
		{
			dejavuCostReductionPeriodTimestamp = new Dictionary<string, long>();
		}
		if (stardustPurchases == null)
		{
			stardustPurchases = new List<PurchaseData>();
		}
		if (lastGameResults == null)
		{
			lastGameResults = new List<DewGameResult>();
		}
		if (favoriteGameResults == null)
		{
			favoriteGameResults = new List<DewGameResult>();
		}
		if (recentlyConcededGames == null)
		{
			recentlyConcededGames = new List<ConcededGameData>();
		}
		if (receivedLevelUpRewards == null)
		{
			receivedLevelUpRewards = new Dictionary<string, int>();
		}
		if (preferredGameSettings == null)
		{
			preferredGameSettings = new Dictionary<string, PreferredGameSettings>();
		}
		if (storyNodes == null)
		{
			storyNodes = new Dictionary<string, StoryNodeUnlockData>();
		}
		if (!preferredGameSettings.ContainsKey("default"))
		{
			preferredGameSettings["default"] = new PreferredGameSettings();
		}
		string[] array = preferredGameSettings.Keys.ToArray();
		foreach (string key in array)
		{
			if (preferredGameSettings[key] == null)
			{
				preferredGameSettings[key] = new PreferredGameSettings();
			}
			preferredGameSettings[key].Validate();
		}
		DewSave.ValidateEnumValues(gameplay);
		DewSave.ValidateEnumValues(controls);
		DewSave.ValidateEnumValues(audio);
		if (saveVersion == 0)
		{
			saveVersion = 1;
		}
		if (saveVersion == 1)
		{
			saveVersion = 2;
			bool enableDirMoveKeys = controls.enableDirMoveKeys;
			controls = new DewControlSettings_User();
			controls.ApplyPreset(enableDirMoveKeys ? DewControlSettings_User.PresetType.WASD : DewControlSettings_User.PresetType.MOBA);
			GlobalLogicPackage.CallOnReady(() =>
			{
				ManagerBase<MessageManager>.instance.ShowMessageLocalized("Message_Warning_ControlSettingsReset");
			});
		}
		if (saveVersion == 2)
		{
			saveVersion = 3;
			FieldInfo[] fields = typeof(DewControlSettings_User).GetFields(BindingFlags.Instance | BindingFlags.Public);
			foreach (FieldInfo fieldInfo in fields)
			{
				if (!(fieldInfo.FieldType != typeof(DewBinding)))
				{
					DewBinding dewBinding = (DewBinding)fieldInfo.GetValue(controls);
					dewBinding.pcBinds = new List<PCBind>();
					if ((int)dewBinding.keyboard != 0)
					{
						dewBinding.pcBinds.Add(new PCBind
						{
							key = dewBinding.keyboard,
							modifiers = new List<Key>(dewBinding.keyModifiers)
						});
					}
					if (dewBinding.mouse != MouseButton.None)
					{
						dewBinding.pcBinds.Add(new PCBind
						{
							mouse = dewBinding.mouse,
							modifiers = new List<Key>(dewBinding.keyModifiers)
						});
					}
				}
			}
		}
		if (saveVersion == 3)
		{
			saveVersion = 4;
			controls = new DewControlSettings_User();
			favoriteGameResults.Clear();
			lastGameResults.Clear();
			artifacts.Clear();
			foreach (KeyValuePair<string, UnlockData> gem in gems)
			{
				gem.Value.didReadMemory = false;
			}
			foreach (KeyValuePair<string, UnlockData> skill in skills)
			{
				skill.Value.didReadMemory = false;
			}
			GlobalLogicPackage.CallOnReady(() =>
			{
				ManagerBase<MessageManager>.instance.ShowMessageLocalized("Message_Warning_ControlSettingsReset");
			});
		}
		if (saveVersion == 4)
		{
			saveVersion = 5;
			foreach (KeyValuePair<string, UnlockData> gem2 in gems)
			{
				gem2.Value.didReadMemory = false;
			}
			foreach (KeyValuePair<string, UnlockData> skill2 in skills)
			{
				skill2.Value.didReadMemory = false;
			}
			foreach (KeyValuePair<string, UnlockData> artifact in artifacts)
			{
				artifact.Value.didReadMemory = false;
			}
		}
		if (saveVersion == 5)
		{
			saveVersion = 6;
			array = skills.Keys.ToArray();
			UnlockData value = default;
			foreach (string k in array)
			{
				if (k.Contains("_L_") && !Dew.allSkills.Any((Type s) => s.Name == k))
				{
					string uniqueKey = k.Replace("_L_", "_U_");
					if (Dew.allSkills.Any((Type s) => s.Name == uniqueKey))
					{
						skills.Remove(k, ref value);
						skills.Add(uniqueKey, value);
					}
				}
			}
			array = gems.Keys.ToArray();
			UnlockData value2 = default;
			foreach (string k2 in array)
			{
				if (k2.Contains("_L_") && !Dew.allGems.Any((Type s) => s.Name == k2))
				{
					string uniqueKey2 = k2.Replace("_L_", "_U_");
					if (Dew.allGems.Any((Type s) => s.Name == uniqueKey2))
					{
						gems.Remove(k2, ref value2);
						gems.Add(uniqueKey2, value2);
					}
				}
			}
		}
		if (saveVersion == 6)
		{
			saveVersion = 7;
		}
		if (!string.IsNullOrEmpty(DewSave.profileMainPath))
		{
			DewSave.CreateSubProfilesIfNonExistent(DewSave.profileMainPath);
		}
		if (saveVersion == 7)
		{
			saveVersion = 8;
			freeVersionPlayTimeMinutes = totalPlayTimeMinutes;
		}
		if (saveVersion == 8)
		{
			saveVersion = 9;
			int num = 0;
			foreach (KeyValuePair<string, StarData> star in stars)
			{
				num += star.Value.level * 45;
			}
			stardust += Mathf.Max(spentStardust, num);
			spentStardust = 0;
			stars.Clear();
			foreach (KeyValuePair<string, UnlockData> gem3 in gems)
			{
				gem3.Value.didReadMemory = false;
			}
			foreach (KeyValuePair<string, UnlockData> skill3 in skills)
			{
				skill3.Value.didReadMemory = false;
			}
			for (int num2 = DewSave.profileMain.reverieSlots.Count - 1; num2 >= 0; num2--)
			{
				if (!string.IsNullOrEmpty(DewSave.profileMain.reverieSlots[num2].type))
				{
					DewSave.profileMain.reverieSlots[num2] = new DailyReverieData
					{
						type = null,
						currentProgress = 0,
						maxProgress = 0,
						grantedStardust = 0,
						persistentVariables = null,
						isComplete = false,
						nextRefillTimestamp = 0L,
						wasNeverFilled = false
					};
				}
			}
		}
		if (saveVersion == 9)
		{
			saveVersion = 10;
		}
		controls.Validate();
		if (!ValidateProfileName(name))
		{
			name = "Traveler";
		}
		if (language == "zh-HK")
		{
			language = "zh-TW";
		}
		if (!DewLocalization.buildData.dataByLanguage.ContainsKey(language))
		{
			language = DewLocalization.GetRecommendedSupportedLanguage();
		}
		array = achievements.Keys.ToArray();
		foreach (string text in array)
		{
			if (!Dew.achievementsByName.ContainsKey(text) || !Dew.IsAchievementIncludedInGame(text))
			{
				achievements.Remove(text);
			}
		}
		foreach (Type allAchievement in Dew.allAchievements)
		{
			if (!achievements.ContainsKey(allAchievement.Name) && Dew.IsAchievementIncludedInGame(allAchievement.Name))
			{
				DewAchievementItem dewAchievementItem = (DewAchievementItem)Activator.CreateInstance(allAchievement);
				achievements.Add(allAchievement.Name, new AchievementData
				{
					isCompleted = false,
					currentProgress = 0,
					maxProgress = dewAchievementItem.GetMaxProgress(),
					persistentVariables = new Dictionary<string, string>(),
					completeTimestamp = 0L,
					isNew = false
				});
			}
		}
		foreach (KeyValuePair<string, AchievementData> achievement in achievements)
		{
			if (achievement.Value.isCompleted && achievement.Value.completeTimestamp <= 0)
			{
				achievement.Value.completeTimestamp = DateTime.UtcNow.ToTimestamp();
			}
		}
		foreach (KeyValuePair<string, AchievementData> achievement2 in achievements)
		{
			if (achievement2.Value.isNew && !achievement2.Value.isCompleted)
			{
				achievement2.Value.isNew = false;
			}
		}
		HashSet<string> hashSet = new HashSet<string>();
		foreach (KeyValuePair<string, AchievementData> achievement3 in achievements)
		{
			if (achievement3.Value.isCompleted)
			{
				continue;
			}
			foreach (Type item in Dew.GetUnlockedTargetsOfAchievement(Dew.achievementsByName[achievement3.Key]))
			{
				if (typeof(Gem).IsAssignableFrom(item) || typeof(SkillTrigger).IsAssignableFrom(item) || typeof(LucidDream).IsAssignableFrom(item))
				{
					hashSet.Add(item.Name);
				}
				else if (typeof(Hero).IsAssignableFrom(item))
				{
					hashSet.Add(item.Name);
					HeroSkill component = ((Component)(object)DewResources.GetByType<Hero>(item, ResourceLoadSettings.Light)).GetComponent<HeroSkill>();
					SkillTrigger[] loadoutSkills = component.GetLoadoutSkills(HeroSkillLocation.Q);
					foreach (SkillTrigger skillTrigger in loadoutSkills)
					{
						hashSet.Add(((object)skillTrigger).GetType().Name);
					}
					loadoutSkills = component.GetLoadoutSkills(HeroSkillLocation.R);
					foreach (SkillTrigger skillTrigger2 in loadoutSkills)
					{
						hashSet.Add(((object)skillTrigger2).GetType().Name);
					}
					loadoutSkills = component.GetLoadoutSkills(HeroSkillLocation.Identity);
					foreach (SkillTrigger skillTrigger3 in loadoutSkills)
					{
						hashSet.Add(((object)skillTrigger3).GetType().Name);
					}
				}
			}
		}
		foreach (Type allHero in Dew.allHeroes)
		{
			if (!heroes.ContainsKey(allHero.Name))
			{
				heroes.Add(allHero.Name, new UnlockData());
			}
			if (!heroLoadouts.ContainsKey(allHero.Name))
			{
				heroLoadouts.Add(allHero.Name, new List<HeroLoadoutData>());
			}
			if (!heroUnlockedStarSlots.ContainsKey(allHero.Name))
			{
				heroUnlockedStarSlots.Add(allHero.Name, new HeroStarSlotUnlockData());
			}
			if (!heroEquippedAccs.ContainsKey(allHero.Name))
			{
				heroEquippedAccs.Add(allHero.Name, new List<string>());
			}
			if (!heroSelectedSkins.ContainsKey(allHero.Name))
			{
				heroSelectedSkins.Add(allHero.Name, Skin.GetDefaultSkin(allHero.Name));
			}
			if (!receivedLevelUpRewards.ContainsKey(allHero.Name))
			{
				receivedLevelUpRewards.Add(allHero.Name, 0);
			}
		}
		foreach (Type allSkill in Dew.allSkills)
		{
			if (!skills.ContainsKey(allSkill.Name))
			{
				skills.Add(allSkill.Name, new UnlockData());
			}
			if (!dejavuCostReductionPeriodTimestamp.ContainsKey(allSkill.Name))
			{
				dejavuCostReductionPeriodTimestamp.Add(allSkill.Name, 0L);
			}
		}
		foreach (Type allGem in Dew.allGems)
		{
			if (!gems.ContainsKey(allGem.Name))
			{
				gems.Add(allGem.Name, new UnlockData());
			}
			if (!dejavuCostReductionPeriodTimestamp.ContainsKey(allGem.Name))
			{
				dejavuCostReductionPeriodTimestamp.Add(allGem.Name, 0L);
			}
		}
		foreach (Type allArtifact in Dew.allArtifacts)
		{
			if (!artifacts.ContainsKey(allArtifact.Name))
			{
				artifacts.Add(allArtifact.Name, new UnlockData());
			}
		}
		foreach (Type allLucidDream in Dew.allLucidDreams)
		{
			if (!lucidDreams.ContainsKey(allLucidDream.Name))
			{
				lucidDreams.Add(allLucidDream.Name, new UnlockData());
			}
		}
		array = heroes.Keys.ToArray();
		foreach (string k3 in array)
		{
			if (Dew.allHeroes.All((Type s) => s.Name != k3))
			{
				heroes.Remove(k3);
			}
		}
		array = skills.Keys.ToArray();
		foreach (string k4 in array)
		{
			if (Dew.allSkills.All((Type s) => s.Name != k4))
			{
				skills.Remove(k4);
			}
		}
		array = gems.Keys.ToArray();
		foreach (string k5 in array)
		{
			if (Dew.allGems.All((Type s) => s.Name != k5))
			{
				gems.Remove(k5);
			}
		}
		array = artifacts.Keys.ToArray();
		foreach (string k6 in array)
		{
			if (Dew.allArtifacts.All((Type s) => s.Name != k6))
			{
				artifacts.Remove(k6);
			}
		}
		array = lucidDreams.Keys.ToArray();
		foreach (string k7 in array)
		{
			if (Dew.allLucidDreams.All((Type s) => s.Name != k7))
			{
				lucidDreams.Remove(k7);
			}
		}
		foreach (KeyValuePair<string, PreferredGameSettings> preferredGameSetting in preferredGameSettings)
		{
			if (preferredGameSetting.Key == "default" && preferredGameSetting.Value.difficulty == "diffLimbo")
			{
				preferredGameSetting.Value.difficulty = "diffNormal";
			}
		}
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		int num6 = 0;
		int num7 = 0;
		array = heroes.Keys.ToArray();
		foreach (string text2 in array)
		{
			if (Dew.IsHeroIncludedInGame(text2))
			{
				if (hashSet.Contains(text2))
				{
					LockHero(text2);
					continue;
				}
				UnlockHero(text2);
				num3++;
			}
		}
		array = skills.Keys.ToArray();
		foreach (string text3 in array)
		{
			if (Dew.IsSkillIncludedInGame(text3))
			{
				if (hashSet.Contains(text3))
				{
					LockSkill(text3);
					continue;
				}
				UnlockSkill(text3);
				num4++;
			}
		}
		array = gems.Keys.ToArray();
		foreach (string text4 in array)
		{
			if (hashSet.Contains(text4))
			{
				LockGem(text4);
				continue;
			}
			UnlockGem(text4);
			num5++;
		}
		array = artifacts.Keys.ToArray();
		foreach (string key2 in array)
		{
			UnlockData unlockData = artifacts[key2];
			if (unlockData.status == UnlockStatus.Locked)
			{
				unlockData.status = UnlockStatus.NotDiscovered;
			}
			if (unlockData.status == UnlockStatus.Complete)
			{
				num6++;
			}
		}
		array = lucidDreams.Keys.ToArray();
		foreach (string text5 in array)
		{
			if (hashSet.Contains(text5))
			{
				LockLucidDream(text5);
				continue;
			}
			UnlockLucidDream(text5);
			num7++;
		}
		int num8 = 0;
		foreach (KeyValuePair<string, AchievementData> achievement4 in achievements)
		{
			DewAchievementItem dewAchievementItem2 = (DewAchievementItem)Activator.CreateInstance(Dew.achievementsByName[achievement4.Key]);
			achievement4.Value.maxProgress = dewAchievementItem2.GetMaxProgress();
			if (achievement4.Value.isCompleted)
			{
				num8++;
				achievement4.Value.currentProgress = achievement4.Value.maxProgress;
				achievement4.Value.persistentVariables = null;
			}
			else if (achievement4.Value.persistentVariables == null)
			{
				achievement4.Value.persistentVariables = new Dictionary<string, string>();
			}
		}
		int refundedStardust = 0;
		foreach (Type allStarType in Dew.allStarTypes)
		{
			newStars.TryAdd(allStarType.Name, new StarData());
		}
		array = newStars.Keys.ToArray();
		foreach (string k8 in array)
		{
			StarEffect se = DewResources.GetByShortTypeName(k8, ResourceLoadSettings.Light) as StarEffect;
			if ((UnityEngine.Object)(object)se == null)
			{
				newStars.Remove(k8);
				Refund((PurchaseData purchaseData) => purchaseData.item.StartsWith(k8 + "|"));
				continue;
			}
			StarData starData = newStars[k8];
			if (starData.level < 0)
			{
				starData.level = 0;
			}
			else if (starData.level > se.maxStarLevel)
			{
				starData.level = se.maxStarLevel;
				Refund((PurchaseData purchaseData) => purchaseData.item.StartsWith(k8 + "|") && int.Parse(purchaseData.item.Split("|", StringSplitOptions.None)[1], CultureInfo.InvariantCulture) > se.maxStarLevel);
			}
		}
		while (DewSave.profileMain.reverieSlots.Count > 3)
		{
			DewSave.profileMain.reverieSlots.RemoveAt(DewSave.profileMain.reverieSlots.Count - 1);
		}
		while (DewSave.profileMain.reverieSlots.Count < 3)
		{
			DewSave.profileMain.reverieSlots.Add(new DailyReverieData
			{
				type = null,
				currentProgress = 0,
				maxProgress = 0,
				grantedStardust = 0,
				persistentVariables = null,
				isComplete = false,
				nextRefillTimestamp = 0L,
				wasNeverFilled = true
			});
		}
		for (int num9 = DewSave.profileMain.reverieSlots.Count - 1; num9 >= 0; num9--)
		{
			DailyReverieData dailyReverieData = DewSave.profileMain.reverieSlots[num9];
			if (!string.IsNullOrEmpty(dailyReverieData.type) && !Dew.reveriesByName.ContainsKey(dailyReverieData.type))
			{
				DewSave.profileMain.reverieSlots[num9] = new DailyReverieData
				{
					type = null,
					currentProgress = 0,
					maxProgress = 0,
					grantedStardust = 0,
					persistentVariables = null,
					isComplete = false,
					nextRefillTimestamp = 0L,
					wasNeverFilled = false
				};
			}
		}
		foreach (DailyReverieData reverieSlot in DewSave.profileMain.reverieSlots)
		{
			if (reverieSlot.maxProgress <= 0)
			{
				reverieSlot.maxProgress = 1;
			}
			if (reverieSlot.isComplete)
			{
				reverieSlot.currentProgress = reverieSlot.maxProgress;
			}
		}
		Emote[] array2 = DewResources.FindAllByNameSubstring<Emote>("Emote_", ResourceLoadSettings.Light).ToArray();
		Emote[] array3 = array2;
		foreach (Emote emote in array3)
		{
			if (!emotes.ContainsKey(emote.name) && !emote.generatedFromServer)
			{
				emotes.Add(emote.name, new CosmeticsData
				{
					isNew = false,
					isUnlocked = false,
					unlockDate = 0L,
					ownershipKey = null
				});
			}
		}
		Accessory[] array4 = DewResources.FindAllByNameSubstring<Accessory>("Acc_", ResourceLoadSettings.Light).ToArray();
		Accessory[] array5 = array4;
		foreach (Accessory accessory in array5)
		{
			if (!accessories.ContainsKey(accessory.name) && !accessory.generatedFromServer)
			{
				accessories.Add(accessory.name, new CosmeticsData
				{
					isNew = false,
					isUnlocked = false,
					unlockDate = 0L,
					ownershipKey = null
				});
			}
		}
		Nametag[] array6 = DewResources.FindAllByNameSubstring<Nametag>("Nametag_", ResourceLoadSettings.Light).ToArray();
		Nametag[] array7 = array6;
		foreach (Nametag nametag in array7)
		{
			if (!nametags.ContainsKey(nametag.name) && !nametag.generatedFromServer)
			{
				nametags.Add(nametag.name, new CosmeticsData
				{
					isNew = false,
					isUnlocked = false,
					unlockDate = 0L,
					ownershipKey = null
				});
			}
		}
		Skin[] array8 = DewResources.FindAllByNameSubstring<Skin>("Skin_", ResourceLoadSettings.Light).ToArray();
		Skin[] array9 = array8;
		foreach (Skin skin in array9)
		{
			if (!skins.ContainsKey(skin.name) && !skin.generatedFromServer)
			{
				skins.Add(skin.name, new CosmeticsData
				{
					isNew = false,
					isUnlocked = false,
					unlockDate = 0L,
					ownershipKey = null
				});
			}
		}
		TravelerStoryNodeDef[] array10 = DewResources.FindAllByNameSubstring<TravelerStoryNodeDef>("TravelerStory_", default(ResourceLoadSettings)).ToArray();
		HashSet<string> hashSet2 = new HashSet<string>();
		TravelerStoryNodeDef[] array11 = array10;
		foreach (TravelerStoryNodeDef travelerStoryNodeDef in array11)
		{
			if (!((UnityEngine.Object)(object)travelerStoryNodeDef == null) && !string.IsNullOrEmpty(((UnityEngine.Object)(object)travelerStoryNodeDef).name))
			{
				hashSet2.Add(((UnityEngine.Object)(object)travelerStoryNodeDef).name);
				if (!storyNodes.ContainsKey(((UnityEngine.Object)(object)travelerStoryNodeDef).name))
				{
					storyNodes.Add(((UnityEngine.Object)(object)travelerStoryNodeDef).name, new StoryNodeUnlockData());
				}
			}
		}
		array = storyNodes.Keys.ToArray();
		foreach (string text6 in array)
		{
			if (!hashSet2.Contains(text6))
			{
				storyNodes.Remove(text6);
			}
		}
		array11 = array10;
		foreach (TravelerStoryNodeDef travelerStoryNodeDef2 in array11)
		{
			if (((UnityEngine.Object)(object)travelerStoryNodeDef2).name.Contains("_Start"))
			{
				storyNodes[((UnityEngine.Object)(object)travelerStoryNodeDef2).name].isUnlocked = true;
			}
		}
		array11 = array10;
		foreach (TravelerStoryNodeDef travelerStoryNodeDef3 in array11)
		{
			StoryNodeUnlockData storyNodeUnlockData = storyNodes[((UnityEngine.Object)(object)travelerStoryNodeDef3).name];
			if (travelerStoryNodeDef3.nodeType != TravelerStoryNodeDef.NodeType.Episode)
			{
				storyNodeUnlockData.chunks = null;
				continue;
			}
			if (storyNodeUnlockData.chunks == null)
			{
				storyNodeUnlockData.chunks = new List<StoryChunkData>();
			}
			while (storyNodeUnlockData.chunks.Count > travelerStoryNodeDef3.contentChunkCount)
			{
				storyNodeUnlockData.chunks.RemoveAt(storyNodeUnlockData.chunks.Count - 1);
			}
			while (storyNodeUnlockData.chunks.Count < travelerStoryNodeDef3.contentChunkCount)
			{
				storyNodeUnlockData.chunks.Add(new StoryChunkData());
			}
			for (int num10 = 0; num10 < storyNodeUnlockData.chunks.Count; num10++)
			{
				if (storyNodeUnlockData.chunks[num10] == null)
				{
					storyNodeUnlockData.chunks[num10] = new StoryChunkData();
				}
			}
		}
		array = emotes.Keys.ToArray();
		foreach (string k9 in array)
		{
			if (string.IsNullOrEmpty(emotes[k9].ownershipKey) && !array2.Any((Emote nt) => nt.name == k9))
			{
				emotes.Remove(k9);
				Refund((PurchaseData purchaseData) => purchaseData.item == k9);
			}
		}
		array = accessories.Keys.ToArray();
		foreach (string k10 in array)
		{
			if (string.IsNullOrEmpty(accessories[k10].ownershipKey) && !array4.Any((Accessory nt) => nt.name == k10))
			{
				accessories.Remove(k10);
				Refund((PurchaseData purchaseData) => purchaseData.item == k10);
			}
		}
		array = nametags.Keys.ToArray();
		foreach (string k11 in array)
		{
			if (string.IsNullOrEmpty(nametags[k11].ownershipKey) && !array6.Any((Nametag nt) => nt.name == k11))
			{
				nametags.Remove(k11);
				Refund((PurchaseData purchaseData) => purchaseData.item == k11);
			}
		}
		array = skins.Keys.ToArray();
		foreach (string k12 in array)
		{
			if (string.IsNullOrEmpty(skins[k12].ownershipKey) && !array8.Any((Skin sk) => sk.name == k12))
			{
				skins.Remove(k12);
				Refund((PurchaseData purchaseData) => purchaseData.item == k12);
			}
		}
		array = Emote.DefaultUnlocks;
		foreach (string text7 in array)
		{
			if (emotes.TryGetValue(text7, out var value3) && !value3.isUnlocked)
			{
				UnlockEmote(text7, null);
				emotes[text7].isNew = false;
			}
		}
		array9 = array8;
		foreach (Skin skin2 in array9)
		{
			if (skins.TryGetValue(skin2.name, out var value4) && !value4.isUnlocked && skin2.name.EndsWith("_Default"))
			{
				UnlockSkin(skin2.name, null);
				skins[skin2.name].isNew = false;
			}
		}
		if (equippedEmotes.Count == 0)
		{
			equippedEmotes.AddRange(Emote.DefaultUnlocks);
		}
		while (equippedEmotes.Count > 9)
		{
			equippedEmotes.RemoveAt(equippedEmotes.Count - 1);
		}
		while (equippedEmotes.Count < 9)
		{
			equippedEmotes.Add(null);
		}
		array = heroEquippedAccs.Keys.ToArray();
		foreach (string key3 in array)
		{
			if (heroEquippedAccs[key3] == null)
			{
				heroEquippedAccs[key3] = new List<string>();
			}
			for (int num11 = heroEquippedAccs[key3].Count - 1; num11 >= 0; num11--)
			{
				if (!DewResources.database.nameToGuid.ContainsKey(heroEquippedAccs[key3][num11]))
				{
					heroEquippedAccs[key3].RemoveAt(num11);
				}
			}
		}
		array = heroSelectedSkins.Keys.ToArray();
		foreach (string text8 in array)
		{
			if (string.IsNullOrEmpty(heroSelectedSkins[text8]) || !DewResources.database.nameToGuid.ContainsKey(heroSelectedSkins[text8]))
			{
				heroSelectedSkins[text8] = Skin.GetDefaultSkin(text8);
			}
		}
		array = emotes.Keys.ToArray();
		foreach (string text9 in array)
		{
			if (!Dew.IsEmoteIncludedInGame(text9))
			{
				continue;
			}
			Emote byName = DewResources.GetByName<Emote>(text9, ResourceLoadSettings.Light);
			if (byName == null || !byName.generatedFromServer)
			{
				continue;
			}
			if (string.IsNullOrEmpty(emotes[text9].ownershipKey))
			{
				emotes.Remove(text9);
				continue;
			}
			DecryptedItemData decryptedItemData = DewItem.GetDecryptedItemData(emotes[text9].ownershipKey);
			if (decryptedItemData == null || decryptedItemData.item != text9)
			{
				emotes.Remove(text9);
			}
			else if (DewSteam.isInitialized && (DewSteam.steamId.m_SteamID.ToString() != decryptedItemData.owner || decryptedItemData.IsExpired() || decryptedItemData.IsDLCNotOwned()))
			{
				emotes.Remove(text9);
			}
		}
		array = accessories.Keys.ToArray();
		foreach (string text10 in array)
		{
			if (!Dew.IsAccessoryIncludedInGame(text10))
			{
				continue;
			}
			Accessory byName2 = DewResources.GetByName<Accessory>(text10, ResourceLoadSettings.Light);
			if (byName2 == null || !byName2.generatedFromServer)
			{
				continue;
			}
			if (string.IsNullOrEmpty(accessories[text10].ownershipKey))
			{
				accessories.Remove(text10);
				continue;
			}
			DecryptedItemData decryptedItemData2 = DewItem.GetDecryptedItemData(accessories[text10].ownershipKey);
			if (decryptedItemData2 == null || decryptedItemData2.item != text10)
			{
				accessories.Remove(text10);
			}
			else if (DewSteam.isInitialized && (DewSteam.steamId.m_SteamID.ToString() != decryptedItemData2.owner || decryptedItemData2.IsExpired() || decryptedItemData2.IsDLCNotOwned()))
			{
				accessories.Remove(text10);
			}
		}
		array = nametags.Keys.ToArray();
		foreach (string text11 in array)
		{
			if (!Dew.IsNametagIncludedInGame(text11))
			{
				continue;
			}
			Nametag byName3 = DewResources.GetByName<Nametag>(text11, ResourceLoadSettings.Light);
			if (byName3 == null || !byName3.generatedFromServer)
			{
				continue;
			}
			if (string.IsNullOrEmpty(nametags[text11].ownershipKey))
			{
				nametags.Remove(text11);
				continue;
			}
			DecryptedItemData decryptedItemData3 = DewItem.GetDecryptedItemData(nametags[text11].ownershipKey);
			if (decryptedItemData3 == null || decryptedItemData3.item != text11)
			{
				nametags.Remove(text11);
			}
			else if (DewSteam.isInitialized && (DewSteam.steamId.m_SteamID.ToString() != decryptedItemData3.owner || decryptedItemData3.IsExpired() || decryptedItemData3.IsDLCNotOwned()))
			{
				nametags.Remove(text11);
			}
		}
		array = skins.Keys.ToArray();
		foreach (string text12 in array)
		{
			if (!Dew.IsSkinIncludedInGame(text12))
			{
				continue;
			}
			Skin byName4 = DewResources.GetByName<Skin>(text12, ResourceLoadSettings.Light);
			if (byName4 == null || !byName4.generatedFromServer)
			{
				continue;
			}
			if (string.IsNullOrEmpty(skins[text12].ownershipKey))
			{
				skins.Remove(text12);
				continue;
			}
			DecryptedItemData decryptedItemData4 = DewItem.GetDecryptedItemData(skins[text12].ownershipKey);
			if (decryptedItemData4 == null || decryptedItemData4.item != text12)
			{
				skins.Remove(text12);
			}
			else if (DewSteam.isInitialized && (DewSteam.steamId.m_SteamID.ToString() != decryptedItemData4.owner || decryptedItemData4.IsExpired() || decryptedItemData4.IsDLCNotOwned()))
			{
				skins.Remove(text12);
			}
		}
		array = heroUnlockedStarSlots.Keys.ToArray();
		foreach (string k13 in array)
		{
			if (heroUnlockedStarSlots[k13] == null)
			{
				heroUnlockedStarSlots[k13] = new HeroStarSlotUnlockData();
			}
			Hero h;
			HeroStarSlotUnlockData d;
			if (Dew.IsHeroIncludedInGame(k13))
			{
				h = DewResources.GetByShortTypeName(k13, ResourceLoadSettings.Light) as Hero;
				if (!((UnityEngine.Object)(object)h == null))
				{
					d = heroUnlockedStarSlots[k13];
					ValidateType(StarType.Destruction);
					ValidateType(StarType.Life);
					ValidateType(StarType.Imagination);
					ValidateType(StarType.Flexible);
				}
			}
			void ValidateType(StarType type)
			{
				List<int> list = d.Get(type);
				if (list == null)
				{
					list = new List<int>();
					d.Set(type, list);
				}
				HeroConstellationSettings constellationSettings = h.GetConstellationSettings(type);
				List<int> list2 = new List<int>();
				for (int j = constellationSettings.defaultCount; j < constellationSettings.maxCount; j++)
				{
					list2.Add(j);
				}
				for (int num12 = list.Count - 1; num12 >= 0; num12--)
				{
					int slotIndex = list[num12];
					if (!list2.Contains(slotIndex))
					{
						list.RemoveAt(num12);
						Refund((PurchaseData purchaseData) => purchaseData.item == $"{k13}|{type}|{slotIndex}");
					}
				}
			}
		}
		array = heroLoadouts.Keys.ToArray();
		foreach (string text13 in array)
		{
			if (!Dew.IsHeroIncludedInGame(text13) || (UnityEngine.Object)(object)(DewResources.GetByShortTypeName(text13, ResourceLoadSettings.Light) as Hero) == null)
			{
				continue;
			}
			if (heroLoadouts[text13] == null)
			{
				heroLoadouts[text13] = new List<HeroLoadoutData>();
			}
			while (heroLoadouts[text13].Count > 5)
			{
				heroLoadouts[text13].RemoveAt(heroLoadouts[text13].Count - 1);
			}
			while (heroLoadouts[text13].Count < 5)
			{
				heroLoadouts[text13].Add(new HeroLoadoutData());
			}
			foreach (HeroLoadoutData item2 in heroLoadouts[text13])
			{
				HeroLoadoutData l = item2;
				l.Validate_Imp(text13, isRepair: true, checkStarLevels: false, heroUnlockedStarSlots[text13]);
				Process(StarType.Destruction);
				Process(StarType.Imagination);
				Process(StarType.Life);
				Process(StarType.Flexible);
				void Process(StarType type)
				{
					List<LoadoutStarItem> starList = l.GetStarList(type);
					for (int j = 0; j < starList.Count; j++)
					{
						if (!string.IsNullOrEmpty(starList[j].name) && (!DewSave.profileMain.newStars.TryGetValue(starList[j].name, out var value5) || value5.level < 1))
						{
							LoadoutStarItem value6 = starList[j];
							value6.name = null;
							starList[j] = value6;
						}
					}
				}
			}
		}
		if (maxObliterationSlots < Dew.GetDefaultObliterationSlots())
		{
			maxObliterationSlots = Dew.GetDefaultObliterationSlots();
		}
		if (refundedStardust > 0)
		{
			GlobalLogicPackage.CallOnReady(() =>
			{
				ManagerBase<MessageManager>.instance.ShowMessage(new DewMessageSettings
				{
					buttons = DewMessageSettings.ButtonType.Ok,
					rawContent = string.Format(DewLocalization.GetUIValue("Message_Warning_RemovedContentRefunded"), refundedStardust.ToString("#,##0")),
					defaultButton = DewMessageSettings.ButtonType.Ok
				});
			});
		}
		if (DewBuildProfile.current.HasFeature(BuildFeatureTag.Booth))
		{
			maxObliterationSlots = 4;
		}
		DewSave.AddMissingServerGeneratedItemsToProfile(this);
		Debug.Log($"Profile '{name}' validated: Achievements({num8}/{achievements.Count}), Heroes({num3}/{Dew.allHeroes.Count}), Skills({num4}/{Dew.allSkills.Count}), Gems({num5}/{Dew.allGems.Count}), Artifacts({num6}/{Dew.allArtifacts.Count}), LucidDreams({num7}/{Dew.allLucidDreams.Count})");
		if (Application.isPlaying && DateTime.UtcNow - lastUpsert.ToDateTime() > TimeSpan.FromHours(4.0) && !DewBuildProfile.current.HasFeature(BuildFeatureTag.Booth))
		{
			DewSave.UpsertProfile();
		}
		void Refund(Func<PurchaseData, bool> condition)
		{
			for (int num12 = stardustPurchases.Count - 1; num12 >= 0; num12--)
			{
				try
				{
					PurchaseData purchaseData = stardustPurchases[num12];
					if (condition(purchaseData))
					{
						stardustPurchases.RemoveAt(num12);
						refundedStardust += purchaseData.stardust;
						stardust += purchaseData.stardust;
						spentStardust -= purchaseData.stardust;
					}
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
				}
			}
		}
	}

	public void UnlockHero(string s)
	{
		if (!heroes.TryGetValue(s, out var value) || value.status != UnlockStatus.Locked)
		{
			return;
		}
		value.status = UnlockStatus.Complete;
		value.didReadMemory = false;
		value.isNewHeroOrHeroSkill = true;
		HeroSkill component = ((Component)(object)DewResources.GetByShortTypeName<Hero>(s, ResourceLoadSettings.Light)).GetComponent<HeroSkill>();
		List<string> list = new List<string>();
		SkillTrigger[] loadoutSkills = component.GetLoadoutSkills(HeroSkillLocation.Q);
		foreach (SkillTrigger skillTrigger in loadoutSkills)
		{
			list.Add(((object)skillTrigger).GetType().Name);
		}
		loadoutSkills = component.GetLoadoutSkills(HeroSkillLocation.R);
		foreach (SkillTrigger skillTrigger2 in loadoutSkills)
		{
			list.Add(((object)skillTrigger2).GetType().Name);
		}
		loadoutSkills = component.GetLoadoutSkills(HeroSkillLocation.Identity);
		foreach (SkillTrigger skillTrigger3 in loadoutSkills)
		{
			list.Add(((object)skillTrigger3).GetType().Name);
		}
		foreach (string item in list)
		{
			if (skills[item].status == UnlockStatus.Locked)
			{
				Type requiredAchievementOfTarget = Dew.GetRequiredAchievementOfTarget(item);
				if (!(requiredAchievementOfTarget != null) || achievements[requiredAchievementOfTarget.Name].isCompleted)
				{
					UnlockSkill(item);
				}
			}
		}
	}

	public void LockHero(string s)
	{
		if (heroes.TryGetValue(s, out var value) && value.status != UnlockStatus.Locked)
		{
			value.status = UnlockStatus.Locked;
			value.didReadMemory = false;
			value.isNewHeroOrHeroSkill = false;
		}
	}

	public void UnlockSkill(string s)
	{
		if (!skills.TryGetValue(s, out var value))
		{
			return;
		}
		bool flag = Dew.allHeroSkills.FirstOrDefault((Type t) => t.Name == s) != null;
		if (value.status == UnlockStatus.Locked)
		{
			if (!flag && Dew.GetRequiredAchievementOfTarget(s) == null)
			{
				value.status = UnlockStatus.NotDiscovered;
				value.isNewHeroOrHeroSkill = false;
			}
			else
			{
				value.status = UnlockStatus.Complete;
				value.isNewHeroOrHeroSkill = flag;
			}
			value.didReadMemory = false;
		}
	}

	public void LockSkill(string s)
	{
		if (skills.TryGetValue(s, out var value) && value.status != UnlockStatus.Locked)
		{
			value.status = UnlockStatus.Locked;
			value.didReadMemory = false;
			value.isNewHeroOrHeroSkill = false;
		}
	}

	public void UnlockGem(string g)
	{
		if (gems.TryGetValue(g, out var value) && value.status == UnlockStatus.Locked)
		{
			if (Dew.GetRequiredAchievementOfTarget(g) == null)
			{
				value.status = UnlockStatus.NotDiscovered;
			}
			else
			{
				value.status = UnlockStatus.Complete;
			}
			value.didReadMemory = false;
			value.isNewHeroOrHeroSkill = false;
		}
	}

	public void LockGem(string g)
	{
		if (gems.TryGetValue(g, out var value) && value.status != UnlockStatus.Locked)
		{
			value.status = UnlockStatus.Locked;
			value.didReadMemory = false;
			value.isNewHeroOrHeroSkill = false;
		}
	}

	public bool DiscoverGem(string g)
	{
		if (!gems.TryGetValue(g, out var value))
		{
			return false;
		}
		if (value.status != UnlockStatus.NotDiscovered)
		{
			return false;
		}
		value.status = UnlockStatus.Complete;
		value.didReadMemory = false;
		value.isNewHeroOrHeroSkill = false;
		return true;
	}

	public bool DiscoverSkill(string s)
	{
		if (!skills.TryGetValue(s, out var value))
		{
			return false;
		}
		if (value.status != UnlockStatus.NotDiscovered)
		{
			return false;
		}
		value.status = UnlockStatus.Complete;
		value.didReadMemory = false;
		value.isNewHeroOrHeroSkill = false;
		return true;
	}

	public void DiscoverArtifact(string g)
	{
		if (artifacts.TryGetValue(g, out var value) && value.status == UnlockStatus.NotDiscovered)
		{
			value.status = UnlockStatus.Complete;
			value.didReadMemory = false;
			value.isNewHeroOrHeroSkill = false;
		}
	}

	public void UnlockLucidDream(string l)
	{
		if (lucidDreams.TryGetValue(l, out var value) && value.status == UnlockStatus.Locked)
		{
			value.status = UnlockStatus.Complete;
			value.didReadMemory = false;
			value.isNewHeroOrHeroSkill = false;
		}
	}

	public void LockLucidDream(string l)
	{
		if (lucidDreams.TryGetValue(l, out var value) && value.status != UnlockStatus.Locked)
		{
			value.status = UnlockStatus.Locked;
			value.didReadMemory = false;
			value.isNewHeroOrHeroSkill = false;
		}
	}

	public void UnlockEmote(string emoteName, string ownershipKey)
	{
		if (!emotes.TryGetValue(emoteName, out var value))
		{
			if (string.IsNullOrEmpty(ownershipKey))
			{
				return;
			}
			value = new CosmeticsData();
			emotes[emoteName] = value;
		}
		if (value.isUnlocked)
		{
			if (!string.IsNullOrEmpty(ownershipKey))
			{
				value.ownershipKey = ownershipKey;
			}
		}
		else
		{
			value.unlockDate = DateTime.UtcNow.ToTimestamp();
			value.isUnlocked = true;
			value.isNew = true;
			value.ownershipKey = ownershipKey;
		}
	}

	public void LockEmote(string emoteName)
	{
		CosmeticsData cosmeticsData = emotes[emoteName];
		if (cosmeticsData.isUnlocked)
		{
			cosmeticsData.isUnlocked = false;
			cosmeticsData.isNew = false;
			cosmeticsData.unlockDate = 0L;
			cosmeticsData.ownershipKey = null;
		}
	}

	public void UnlockAccessory(string accName, string ownershipKey)
	{
		if (!accessories.TryGetValue(accName, out var value))
		{
			if (string.IsNullOrEmpty(ownershipKey))
			{
				return;
			}
			value = new CosmeticsData();
			accessories[accName] = value;
		}
		if (value.isUnlocked)
		{
			if (!string.IsNullOrEmpty(ownershipKey))
			{
				value.ownershipKey = ownershipKey;
			}
		}
		else
		{
			value.unlockDate = DateTime.UtcNow.ToTimestamp();
			value.isUnlocked = true;
			value.isNew = true;
			value.ownershipKey = ownershipKey;
		}
	}

	public void LockAccessory(string accName)
	{
		CosmeticsData cosmeticsData = accessories[accName];
		if (cosmeticsData.isUnlocked)
		{
			cosmeticsData.isUnlocked = false;
			cosmeticsData.isNew = false;
			cosmeticsData.unlockDate = 0L;
			cosmeticsData.ownershipKey = null;
		}
	}

	public void UnlockNametag(string ntName, string ownershipKey)
	{
		if (!nametags.TryGetValue(ntName, out var value))
		{
			if (string.IsNullOrEmpty(ownershipKey))
			{
				return;
			}
			value = new CosmeticsData();
			nametags[ntName] = value;
		}
		if (value.isUnlocked)
		{
			if (!string.IsNullOrEmpty(ownershipKey))
			{
				value.ownershipKey = ownershipKey;
			}
		}
		else
		{
			value.unlockDate = DateTime.UtcNow.ToTimestamp();
			value.isUnlocked = true;
			value.isNew = true;
			value.ownershipKey = ownershipKey;
		}
	}

	public void LockNametag(string ntName)
	{
		CosmeticsData cosmeticsData = nametags[ntName];
		if (cosmeticsData.isUnlocked)
		{
			cosmeticsData.isUnlocked = false;
			cosmeticsData.isNew = false;
			cosmeticsData.unlockDate = 0L;
			cosmeticsData.ownershipKey = null;
		}
	}

	public void UnlockSkin(string ntName, string ownershipKey)
	{
		if (!skins.TryGetValue(ntName, out var value))
		{
			if (string.IsNullOrEmpty(ownershipKey))
			{
				return;
			}
			value = new CosmeticsData();
			skins[ntName] = value;
		}
		if (value.isUnlocked)
		{
			if (!string.IsNullOrEmpty(ownershipKey))
			{
				value.ownershipKey = ownershipKey;
			}
		}
		else
		{
			value.unlockDate = DateTime.UtcNow.ToTimestamp();
			value.isUnlocked = true;
			value.isNew = true;
			value.ownershipKey = ownershipKey;
		}
	}

	public void LockSkin(string ntName)
	{
		CosmeticsData cosmeticsData = skins[ntName];
		if (cosmeticsData.isUnlocked)
		{
			cosmeticsData.isUnlocked = false;
			cosmeticsData.isNew = false;
			cosmeticsData.unlockDate = 0L;
			cosmeticsData.ownershipKey = null;
		}
	}

	public bool TryGetItem(string key, out CosmeticsData data)
	{
		data = null;
		if (string.IsNullOrEmpty(key))
		{
			return false;
		}
		if (key.StartsWith("Emote_"))
		{
			return emotes.TryGetValue(key, out data);
		}
		if (key.StartsWith("Acc_"))
		{
			return accessories.TryGetValue(key, out data);
		}
		if (key.StartsWith("Nametag_"))
		{
			return nametags.TryGetValue(key, out data);
		}
		if (key.StartsWith("Skin_"))
		{
			return skins.TryGetValue(key, out data);
		}
		Debug.Log("Unknown item type: " + key);
		return false;
	}

	public void UnlockServerGeneratedItem(DecryptedItemData item)
	{
		if (item != null)
		{
			if (item.item.StartsWith("Emote_"))
			{
				DewSave.profileMain.UnlockEmote(item.item, item.ownershipKey);
			}
			else if (item.item.StartsWith("Acc_"))
			{
				DewSave.profileMain.UnlockAccessory(item.item, item.ownershipKey);
			}
			else if (item.item.StartsWith("Nametag_"))
			{
				DewSave.profileMain.UnlockNametag(item.item, item.ownershipKey);
			}
			else if (item.item.StartsWith("Skin_"))
			{
				DewSave.profileMain.UnlockSkin(item.item, item.ownershipKey);
			}
			else
			{
				Debug.Log("Unknown item type: " + item.item);
			}
		}
	}

	public void LockServerGeneratedItem(DecryptedItemData item)
	{
		if (item != null)
		{
			if (item.item.StartsWith("Emote_"))
			{
				DewSave.profileMain.LockEmote(item.item);
			}
			else if (item.item.StartsWith("Acc_"))
			{
				DewSave.profileMain.LockAccessory(item.item);
			}
			else if (item.item.StartsWith("Nametag_"))
			{
				DewSave.profileMain.LockNametag(item.item);
			}
			else if (item.item.StartsWith("Skin_"))
			{
				DewSave.profileMain.LockSkin(item.item);
			}
			else
			{
				Debug.Log("Unknown item type: " + item.item);
			}
		}
	}

	public int GetLockedGemCount()
	{
		return gems.Count((KeyValuePair<string, UnlockData> g) => g.Value.status == UnlockStatus.Locked);
	}

	public int GetUnlockedGemCount()
	{
		return gems.Count((KeyValuePair<string, UnlockData> g) => g.Value.status != UnlockStatus.Locked);
	}

	public int GetLockedSkillCount()
	{
		return skills.Count((KeyValuePair<string, UnlockData> g) => g.Value.status == UnlockStatus.Locked);
	}

	public int GetUnlockedSkillCount()
	{
		return skills.Count((KeyValuePair<string, UnlockData> g) => g.Value.status != UnlockStatus.Locked);
	}

	public int GetLockedLucidDreamsCount()
	{
		return lucidDreams.Count((KeyValuePair<string, UnlockData> g) => g.Value.status == UnlockStatus.Locked);
	}

	public int GetUnlockedLucidDreamsCount()
	{
		return lucidDreams.Count((KeyValuePair<string, UnlockData> g) => g.Value.status != UnlockStatus.Locked);
	}

	public static bool ValidateProfileName(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			return false;
		}
		if (name.Length > 30)
		{
			return false;
		}
		if (name.Length < 1)
		{
			return false;
		}
		int num = 0;
		while (num < name.Length)
		{
			if (name[num] != '<')
			{
				num++;
				continue;
			}
			int i = num + 1;
			if (i < name.Length && name[i] == '/')
			{
				i++;
			}
			int num2 = i;
			for (; i < name.Length && (char.IsLetter(name[i]) || name[i] == '-'); i++)
			{
			}
			if (i > num2 && i < name.Length && (name[i] == '>' || name[i] == '=' || char.IsWhiteSpace(name[i])) && ProfileNameDisallowedTags.Contains(name.Substring(num2, i - num2).ToLowerInvariant()))
			{
				return false;
			}
			num++;
		}
		return true;
	}

	public int RefundPurchase(Func<PurchaseData, bool> condition)
	{
		int num = 0;
		for (int num2 = stardustPurchases.Count - 1; num2 >= 0; num2--)
		{
			PurchaseData purchaseData = stardustPurchases[num2];
			if (condition(purchaseData))
			{
				stardustPurchases.RemoveAt(num2);
				num += purchaseData.stardust;
				stardust += purchaseData.stardust;
				spentStardust -= purchaseData.stardust;
			}
		}
		return num;
	}

	public PreferredGameSettings GetPreferredGameSettings(string key = null)
	{
		if (string.IsNullOrEmpty(key))
		{
			key = "default";
		}
		if (!preferredGameSettings.TryGetValue(key, out var value))
		{
			value = new PreferredGameSettings();
			value.Validate();
			preferredGameSettings[key] = value;
		}
		return value;
	}
}
