using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MonsterAbilityPrewarmConfig", menuName = "Dew Monster Ability Prewarm Config")]
public class MonsterAbilityPrewarmConfig : ScriptableObject
{
	[Serializable]
	public class AbilityEntry
	{
		[HideInInspector]
		public string editorName;

		public AssetRef<GameObject> abilityPrefab;

		[Tooltip("True if this ability is spawned by the monster's basic attack trigger (ea.attackAbilityPreset). Auto-propose uses a higher multiplier for these because basic attacks fire many times over a fight.")]
		public bool isBasicAttack;

		[Min(0f)]
		public int countPerMonster;
	}

	[Serializable]
	public class MonsterEntry
	{
		[HideInInspector]
		public string editorName;

		public AssetRef<Monster> monsterPrefab;

		public List<AbilityEntry> abilities = new List<AbilityEntry>();
	}

	private static MonsterAbilityPrewarmConfig _instance;

	[SerializeField]
	public List<MonsterEntry> monsters = new List<MonsterEntry>();

	private Dictionary<string, MonsterEntry> _cacheByGuid;

	public static MonsterAbilityPrewarmConfig instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = Resources.Load<MonsterAbilityPrewarmConfig>("MonsterAbilityPrewarmConfig");
			}
			return _instance;
		}
	}

	private void OnEnable()
	{
		BuildCache();
	}

	private void OnValidate()
	{
		_cacheByGuid = null;
	}

	private void BuildCache()
	{
		_cacheByGuid = new Dictionary<string, MonsterEntry>();
		if (monsters == null)
		{
			return;
		}
		for (int i = 0; i < monsters.Count; i++)
		{
			MonsterEntry monsterEntry = monsters[i];
			if (monsterEntry != null && !string.IsNullOrEmpty(monsterEntry.monsterPrefab.guid))
			{
				_cacheByGuid[monsterEntry.monsterPrefab.guid] = monsterEntry;
			}
		}
	}

	public void AppendPrewarmCounts(Monster monsterPrefab, int basicAttackMonsterCount, int abilityMonsterCount, Dictionary<(uint assetId, uint owner), int> output)
	{
		if ((UnityEngine.Object)(object)monsterPrefab == null || output == null || (basicAttackMonsterCount <= 0 && abilityMonsterCount <= 0))
		{
			return;
		}
		if (_cacheByGuid == null)
		{
			BuildCache();
		}
		string guidOfAsset = DewResources.GetGuidOfAsset((UnityEngine.Object)(object)monsterPrefab);
		if (string.IsNullOrEmpty(guidOfAsset) || !_cacheByGuid.TryGetValue(guidOfAsset, out var value) || value.abilities == null)
		{
			return;
		}
		for (int i = 0; i < value.abilities.Count; i++)
		{
			AbilityEntry abilityEntry = value.abilities[i];
			if (abilityEntry == null || abilityEntry.countPerMonster <= 0)
			{
				continue;
			}
			int num = (abilityEntry.isBasicAttack ? basicAttackMonsterCount : abilityMonsterCount);
			if (num > 0)
			{
				uint networkAssetId = DewResources.GetNetworkAssetId(abilityEntry.abilityPrefab.guid);
				if (networkAssetId != 0)
				{
					int num2 = abilityEntry.countPerMonster * num;
					(uint, uint) key = (networkAssetId, 0u);
					output.TryGetValue(key, out var value2);
					output[key] = value2 + num2;
				}
			}
		}
	}
}
