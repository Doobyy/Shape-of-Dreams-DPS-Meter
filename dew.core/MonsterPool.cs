using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Monster Pool", menuName = "Monster Pool")]
public class MonsterPool : ScriptableObject
{
	[Serializable]
	public class SpawnRuleEntry
	{
		public AssetRef<Monster> monster;

		public float chance = 1f;

		public int minCount = 1;

		public int maxCount = 1;

		public List<MonsterSpawnCondition> conditions;

		public bool useAnd = true;

		public bool EvaluateCondition()
		{
			if (conditions == null || conditions.Count <= 0)
			{
				return true;
			}
			if (useAnd)
			{
				foreach (MonsterSpawnCondition condition in conditions)
				{
					if (!condition.EvaluateCondition())
					{
						return false;
					}
				}
				return true;
			}
			foreach (MonsterSpawnCondition condition2 in conditions)
			{
				if (condition2.EvaluateCondition())
				{
					return true;
				}
			}
			return false;
		}

		public SpawnRuleEntry Clone()
		{
			SpawnRuleEntry spawnRuleEntry = (SpawnRuleEntry)MemberwiseClone();
			if (spawnRuleEntry.conditions != null)
			{
				spawnRuleEntry.conditions = new List<MonsterSpawnCondition>(spawnRuleEntry.conditions);
			}
			return spawnRuleEntry;
		}
	}

	private const int SkipWithoutSelectDisableRNGThreshold = 10;

	private const int SkipWithoutSelectLimit = 16;

	public bool scrambleOrder = true;

	public List<SpawnRuleEntry> entries = new List<SpawnRuleEntry>();

	public List<SpawnRuleEntry> GetFilteredEntries()
	{
		List<SpawnRuleEntry> list = new List<SpawnRuleEntry>();
		foreach (SpawnRuleEntry entry in entries)
		{
			if (Dew.IsMonsterIncludedInGame(DewResources.database.guidToType[entry.monster.guid].Name))
			{
				list.Add(entry);
			}
		}
		return list;
	}

	public IEnumerator<Monster> GetMonsters(int sectionIndex, DewRandom random = null)
	{
		if (random == null)
		{
			random = DewRandom.instance;
		}
		int entryIndex = 0;
		int skipCountWithoutSelect = 0;
		List<SpawnRuleEntry> sampledEntries = GetFilteredEntries();
		if (scrambleOrder)
		{
			sampledEntries.Shuffle(random);
		}
		while (true)
		{
			SpawnRuleEntry currentEntry = sampledEntries[entryIndex];
			int count = random.Range(currentEntry.minCount, currentEntry.maxCount + 1);
			if (((skipCountWithoutSelect >= 10) ? float.NegativeInfinity : random.Value()) > currentEntry.chance || !currentEntry.EvaluateCondition() || count <= 0)
			{
				if (skipCountWithoutSelect <= 16)
				{
					skipCountWithoutSelect++;
					IncrementEntryIndex();
					continue;
				}
				Debug.LogWarning("Spawn rule '" + name + "' skipped too much without any spawn. Wrong configuration? Ignoring requirements...");
			}
			for (int i = 0; i < count; i++)
			{
				skipCountWithoutSelect = 0;
				yield return currentEntry.monster;
			}
			IncrementEntryIndex();
		}
		void IncrementEntryIndex()
		{
			entryIndex++;
			if (entryIndex == sampledEntries.Count)
			{
				entryIndex = 0;
				if (scrambleOrder)
				{
					sampledEntries = GetFilteredEntries();
					sampledEntries.Shuffle(random);
				}
			}
		}
	}
}
