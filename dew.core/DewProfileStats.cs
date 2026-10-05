using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class DewProfileStats
{
	public class HeroData
	{
		public int masteryLevel;

		public int completedLimboDepth;

		public long currentMasteryPoints;

		public long totalMasteryPoints;

		public bool didUnlockPolarisEnding;

		public long pureWhiteDreams;

		public long pureWhiteDreamsNightmare;

		public long unknownFates;

		public long unknownFatesNightmare;

		public long starlessPaths;

		public long starlessPathsNightmare;

		public long wins;

		public long winsNightmare;

		public long loses;

		public long playCount;

		public long levelUps;

		public long kills;

		public long hunterKills;

		public long heroicBossKills;

		public long miniBossKills;

		public double playTimeMinutes;

		public long deaths;

		public long visitedLocations;

		public long visitedHunterLocations;

		public long visitedWorlds;

		public long upgradeCount;

		public long dismantleCount;

		public long buyCount;

		public long sellCount;

		public double spentGold;

		public double spentDreamDust;

		public long chaosCount;

		public double damageDealt;

		public double damageTaken;

		public double healToSelf;

		public double healToOthers;

		public double earnedGold;

		public double earnedDreamDust;

		public long maxVisitedWorlds;

		public double maxElapsedGameTimeSeconds;

		public double maxTotalDamage;

		public double maxSingleTargetDamage;

		public double maxEarnedGold;

		public double maxEarnedDreamDust;

		public double maxEarnedStardust;

		public HeroData Clone()
		{
			return (HeroData)MemberwiseClone();
		}

		public void AddMasteryPoints(long points)
		{
			totalMasteryPoints += points;
			currentMasteryPoints += points;
			while (true)
			{
				long requiredMasteryPointsToLevelUp = Dew.GetRequiredMasteryPointsToLevelUp(masteryLevel);
				if (currentMasteryPoints >= requiredMasteryPointsToLevelUp)
				{
					masteryLevel++;
					currentMasteryPoints -= requiredMasteryPointsToLevelUp;
					continue;
				}
				break;
			}
		}

		public void RemoveMasteryPoints(long points)
		{
			totalMasteryPoints -= points;
			currentMasteryPoints -= points;
			while (currentMasteryPoints < 0)
			{
				masteryLevel--;
				if (masteryLevel == -1)
				{
					masteryLevel = 0;
					currentMasteryPoints = 0L;
					break;
				}
				currentMasteryPoints += Dew.GetRequiredMasteryPointsToLevelUp(masteryLevel);
			}
			if (totalMasteryPoints < 0)
			{
				totalMasteryPoints = 0L;
			}
			if (currentMasteryPoints < 0)
			{
				currentMasteryPoints = 0L;
			}
		}
	}

	public class ItemData
	{
		public long wins;

		public long loses;

		public long playCount;

		public long upgradeCount;

		public long dismantleCount;

		public long buyCount;

		public long sellCount;
	}

	public class MonsterData
	{
		public long kills;

		public long nightmareKills;

		public long deaths;
	}

	public class ZoneData
	{
		public long kills;

		public long hunterKills;

		public long heroicBossKills;

		public long miniBossKills;

		public long deaths;

		public double playTimeMinutes;

		public long visited;

		public long visitedLocations;

		public long visitedHunterLocations;
	}

	public HeroData total;

	public Dictionary<string, HeroData> heroes = new Dictionary<string, HeroData>();

	public Dictionary<string, MonsterData> monsters = new Dictionary<string, MonsterData>();

	public Dictionary<string, ZoneData> zones = new Dictionary<string, ZoneData>();

	public Dictionary<string, ItemData> skills = new Dictionary<string, ItemData>();

	public Dictionary<string, ItemData> gems = new Dictionary<string, ItemData>();

	public List<string> recoveredLossPoints = new List<string>();

	private static FieldInfo[] _fields;

	private static FieldInfo[] _heroDataFields;

	private static FieldInfo[] _itemDataFields;

	private static FieldInfo[] _monsterDataFields;

	private static FieldInfo[] _zoneDataFields;

	private static FieldInfo[] HeroDataFields => _heroDataFields ?? (_heroDataFields = typeof(HeroData).GetFields(BindingFlags.Instance | BindingFlags.Public));

	private static FieldInfo[] ItemDataFields => _itemDataFields ?? (_itemDataFields = typeof(ItemData).GetFields(BindingFlags.Instance | BindingFlags.Public));

	private static FieldInfo[] MonsterDataFields => _monsterDataFields ?? (_monsterDataFields = typeof(MonsterData).GetFields(BindingFlags.Instance | BindingFlags.Public));

	private static FieldInfo[] ZoneDataFields => _zoneDataFields ?? (_zoneDataFields = typeof(ZoneData).GetFields(BindingFlags.Instance | BindingFlags.Public));

	public void Validate()
	{
		if (heroes == null)
		{
			heroes = new Dictionary<string, HeroData>();
		}
		if (monsters == null)
		{
			monsters = new Dictionary<string, MonsterData>();
		}
		if (zones == null)
		{
			zones = new Dictionary<string, ZoneData>();
		}
		if (skills == null)
		{
			skills = new Dictionary<string, ItemData>();
		}
		if (gems == null)
		{
			gems = new Dictionary<string, ItemData>();
		}
		foreach (Type allHero in Dew.allHeroes)
		{
			if (!heroes.ContainsKey(allHero.Name))
			{
				heroes.Add(allHero.Name, new HeroData());
			}
			else if (heroes[allHero.Name] == null)
			{
				heroes[allHero.Name] = new HeroData();
			}
		}
		foreach (Type allMonster in Dew.allMonsters)
		{
			if (!monsters.ContainsKey(allMonster.Name))
			{
				monsters.Add(allMonster.Name, new MonsterData());
			}
			else if (monsters[allMonster.Name] == null)
			{
				monsters[allMonster.Name] = new MonsterData();
			}
		}
		foreach (string includedZone in DewBuildProfile.current.content.includedZones)
		{
			if (!zones.ContainsKey(includedZone))
			{
				zones.Add(includedZone, new ZoneData());
			}
			else if (zones[includedZone] == null)
			{
				zones[includedZone] = new ZoneData();
			}
		}
		foreach (string availableSkill in DewBuildProfile.current.content.availableSkills)
		{
			if (!skills.ContainsKey(availableSkill))
			{
				skills.Add(availableSkill, new ItemData());
			}
			else if (skills[availableSkill] == null)
			{
				skills[availableSkill] = new ItemData();
			}
		}
		foreach (string availableGem in DewBuildProfile.current.content.availableGems)
		{
			if (!gems.ContainsKey(availableGem))
			{
				gems.Add(availableGem, new ItemData());
			}
			else if (gems[availableGem] == null)
			{
				gems[availableGem] = new ItemData();
			}
		}
		foreach (KeyValuePair<string, HeroData> hero in heroes)
		{
			if (hero.Value.masteryLevel < 0)
			{
				hero.Value.masteryLevel = 0;
			}
			if (hero.Value.totalMasteryPoints < 0)
			{
				hero.Value.totalMasteryPoints = 0L;
			}
			if (hero.Value.currentMasteryPoints < 0)
			{
				hero.Value.currentMasteryPoints = 0L;
			}
			hero.Value.winsNightmare = hero.Value.pureWhiteDreamsNightmare + hero.Value.unknownFatesNightmare + hero.Value.starlessPathsNightmare;
		}
		DewProfile profileMain = DewSave.profileMain;
		if (profileMain?.storyNodes != null)
		{
			foreach (TravelerStoryNodeDef item in DewResources.FindAllByNameSubstring<TravelerStoryNodeDef>("TravelerStory_", default(ResourceLoadSettings)))
			{
				if (!((UnityEngine.Object)(object)item == null) && item.unlocksPolarisEnding && profileMain.storyNodes.TryGetValue(((UnityEngine.Object)(object)item).name, out var value) && value.isUnlocked && heroes.TryGetValue(item.heroType, out var value2))
				{
					value2.didUnlockPolarisEnding = true;
				}
			}
		}
		UpdateTotalData(0L);
	}

	public void AddMasteryPoints(string heroType, long points)
	{
		if (points > 0 && heroes.TryGetValue(heroType, out var value))
		{
			value.AddMasteryPoints(points);
			UpdateTotalData(0L);
		}
	}

	public void RemoveMasteryPoints(string heroType, long points)
	{
		if (points > 0 && heroes.TryGetValue(heroType, out var value))
		{
			value.RemoveMasteryPoints(points);
			UpdateTotalData(0L);
		}
	}

	public void UpdateTotalData(long minPlayTime = 0L)
	{
		if (total != null)
		{
			minPlayTime = Math.Max(minPlayTime, (long)total.playTimeMinutes);
		}
		total = new HeroData();
		if (_fields == null)
		{
			_fields = typeof(HeroData).GetFields(BindingFlags.Instance | BindingFlags.Public);
		}
		Dictionary<FieldInfo, double> dictionary = DewPool.GetDictionary(out DictionaryReturnHandle<FieldInfo, double> handle);
		FieldInfo[] fields = _fields;
		foreach (FieldInfo key in fields)
		{
			dictionary[key] = 0.0;
		}
		foreach (KeyValuePair<string, HeroData> hero in heroes)
		{
			HeroData value = hero.Value;
			fields = _fields;
			foreach (FieldInfo fieldInfo in fields)
			{
				bool flag = fieldInfo.Name.StartsWith("max") || fieldInfo.Name == "completedLimboDepth";
				double num;
				if (fieldInfo.FieldType == typeof(double))
				{
					num = (double)fieldInfo.GetValue(value);
				}
				else if (fieldInfo.FieldType == typeof(int))
				{
					num = (int)fieldInfo.GetValue(value);
				}
				else if (fieldInfo.FieldType == typeof(long))
				{
					num = (long)fieldInfo.GetValue(value);
				}
				else
				{
					if (!(fieldInfo.FieldType == typeof(bool)))
					{
						continue;
					}
					num = (((bool)fieldInfo.GetValue(value)) ? 1 : 0);
				}
				if (flag)
				{
					dictionary[fieldInfo] = Math.Max(dictionary[fieldInfo], num);
				}
				else
				{
					dictionary[fieldInfo] += num;
				}
			}
		}
		fields = _fields;
		foreach (FieldInfo fieldInfo2 in fields)
		{
			if (fieldInfo2.FieldType == typeof(double))
			{
				fieldInfo2.SetValue(total, dictionary[fieldInfo2]);
			}
			else if (fieldInfo2.FieldType == typeof(int))
			{
				fieldInfo2.SetValue(total, (int)Math.Round(dictionary[fieldInfo2]));
			}
			else if (fieldInfo2.FieldType == typeof(long))
			{
				fieldInfo2.SetValue(total, (long)Math.Round(dictionary[fieldInfo2]));
			}
			else if (fieldInfo2.FieldType == typeof(bool))
			{
				fieldInfo2.SetValue(total, dictionary[fieldInfo2] > 0.0);
			}
		}
		handle.Return();
		total.playTimeMinutes = Math.Max(total.playTimeMinutes, minPlayTime);
	}

	public bool TryGetItemData(Type type, out ItemData data)
	{
		data = GetItemData(type);
		return data != null;
	}

	public ItemData GetItemData(Type type)
	{
		if (type.IsSubclassOf(typeof(SkillTrigger)))
		{
			return CollectionExtensions.GetValueOrDefault<string, ItemData>((IReadOnlyDictionary<string, ItemData>)skills, type.Name);
		}
		if (type.IsSubclassOf(typeof(Gem)))
		{
			return CollectionExtensions.GetValueOrDefault<string, ItemData>((IReadOnlyDictionary<string, ItemData>)gems, type.Name);
		}
		return null;
	}

	public static DewProfileStats GetRecoveryDelta(DewProfileStats beforeLoss, DewProfileStats afterLoss)
	{
		DewProfileStats dewProfileStats = new DewProfileStats();
		dewProfileStats.Validate();
		ProcessDictionary<HeroData>(dewProfileStats.heroes, beforeLoss.heroes, afterLoss.heroes, HeroDataFields);
		ProcessDictionary<MonsterData>(dewProfileStats.monsters, beforeLoss.monsters, afterLoss.monsters, MonsterDataFields);
		ProcessDictionary<ZoneData>(dewProfileStats.zones, beforeLoss.zones, afterLoss.zones, ZoneDataFields);
		ProcessDictionary<ItemData>(dewProfileStats.skills, beforeLoss.skills, afterLoss.skills, ItemDataFields);
		ProcessDictionary<ItemData>(dewProfileStats.gems, beforeLoss.gems, afterLoss.gems, ItemDataFields);
		dewProfileStats.total = null;
		return dewProfileStats;
		static void CalculateDeltaForObject<T>(T deltaObj, T beforeObj, T afterObj, FieldInfo[] fields)
		{
			if (beforeObj != null && afterObj != null)
			{
				foreach (FieldInfo fieldInfo in fields)
				{
					if (!(fieldInfo.Name == "totalMasteryPoints") && !(fieldInfo.Name == "currentMasteryPoints"))
					{
						if (fieldInfo.Name.StartsWith("max"))
						{
							fieldInfo.SetValue(deltaObj, fieldInfo.GetValue(beforeObj));
						}
						else if (fieldInfo.FieldType == typeof(double))
						{
							double num = (double)fieldInfo.GetValue(beforeObj);
							double num2 = (double)fieldInfo.GetValue(afterObj);
							fieldInfo.SetValue(deltaObj, Math.Max(0.0, num - num2));
						}
						else if (fieldInfo.FieldType == typeof(long))
						{
							long num3 = (long)fieldInfo.GetValue(beforeObj);
							long num4 = (long)fieldInfo.GetValue(afterObj);
							fieldInfo.SetValue(deltaObj, Math.Max(0L, num3 - num4));
						}
						else if (fieldInfo.FieldType == typeof(int))
						{
							int num5 = (int)fieldInfo.GetValue(beforeObj);
							int num6 = (int)fieldInfo.GetValue(afterObj);
							fieldInfo.SetValue(deltaObj, Math.Max(0, num5 - num6));
						}
					}
				}
			}
		}
		static void ProcessDictionary<TData>(Dictionary<string, TData> deltaDict, Dictionary<string, TData> beforeDict, Dictionary<string, TData> afterDict, FieldInfo[] fields) where TData : new()
		{
			foreach (string key in beforeDict.Keys)
			{
				if (!deltaDict.ContainsKey(key))
				{
					deltaDict[key] = new TData();
				}
				if (!afterDict.TryGetValue(key, out var value))
				{
					value = new TData();
				}
				CalculateDeltaForObject<TData>(deltaDict[key], beforeDict[key], value, fields);
			}
		}
	}

	public void ApplyRecoveryDelta(DewProfileStats delta)
	{
		ProcessDictionary<HeroData>(heroes, delta.heroes, HeroDataFields);
		ProcessDictionary<MonsterData>(monsters, delta.monsters, MonsterDataFields);
		ProcessDictionary<ZoneData>(zones, delta.zones, ZoneDataFields);
		ProcessDictionary<ItemData>(skills, delta.skills, ItemDataFields);
		ProcessDictionary<ItemData>(gems, delta.gems, ItemDataFields);
		UpdateTotalData(0L);
		static void ApplyDeltaToObject<T>(T currentObj, T deltaObj, FieldInfo[] fields)
		{
			if (currentObj != null && deltaObj != null)
			{
				foreach (FieldInfo fieldInfo in fields)
				{
					if (!(fieldInfo.Name == "totalMasteryPoints") && !(fieldInfo.Name == "currentMasteryPoints"))
					{
						if (fieldInfo.Name.StartsWith("max"))
						{
							if (fieldInfo.FieldType == typeof(double))
							{
								fieldInfo.SetValue(currentObj, Math.Max((double)fieldInfo.GetValue(currentObj), (double)fieldInfo.GetValue(deltaObj)));
							}
							else if (fieldInfo.FieldType == typeof(long))
							{
								fieldInfo.SetValue(currentObj, Math.Max((long)fieldInfo.GetValue(currentObj), (long)fieldInfo.GetValue(deltaObj)));
							}
							else if (fieldInfo.FieldType == typeof(int))
							{
								fieldInfo.SetValue(currentObj, Math.Max((int)fieldInfo.GetValue(currentObj), (int)fieldInfo.GetValue(deltaObj)));
							}
						}
						else if (fieldInfo.FieldType == typeof(double))
						{
							fieldInfo.SetValue(currentObj, (double)fieldInfo.GetValue(currentObj) + (double)fieldInfo.GetValue(deltaObj));
						}
						else if (fieldInfo.FieldType == typeof(long))
						{
							fieldInfo.SetValue(currentObj, (long)fieldInfo.GetValue(currentObj) + (long)fieldInfo.GetValue(deltaObj));
						}
						else if (fieldInfo.FieldType == typeof(int))
						{
							fieldInfo.SetValue(currentObj, (int)fieldInfo.GetValue(currentObj) + (int)fieldInfo.GetValue(deltaObj));
						}
					}
				}
			}
		}
		static void ProcessDictionary<TData>(Dictionary<string, TData> currentDict, Dictionary<string, TData> deltaDict, FieldInfo[] fields) where TData : new()
		{
			if (deltaDict == null)
			{
				return;
			}
			foreach (string key in deltaDict.Keys)
			{
				if (!currentDict.ContainsKey(key))
				{
					currentDict[key] = new TData();
				}
				ApplyDeltaToObject<TData>(currentDict[key], deltaDict[key], fields);
			}
		}
	}
}
