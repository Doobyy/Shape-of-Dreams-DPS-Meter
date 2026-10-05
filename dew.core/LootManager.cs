using System;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

public class LootManager : NetworkedManagerBase<LootManager>
{
	public PerRarityData<float> skillRarityChance;

	public PerRarityData<float> skillRarityChanceHigh;

	public PerRarityData<Formula> skillLevelMinByZoneIndex;

	public PerRarityData<Formula> skillLevelMaxByZoneIndex;

	public AnimationCurve skillLevelRandomCurve;

	public PerRarityData<float> gemRarityChance;

	public PerRarityData<float> gemRarityChanceHigh;

	public PerRarityData<Formula> gemQualityMinByZoneIndex;

	public PerRarityData<Formula> gemQualityMaxByZoneIndex;

	public AnimationCurve gemQualityRandomCurve;

	[NonSerialized]
	[SaveVar(SaveVarFlags.Default)]
	public List<string> poolGems = new List<string>();

	[NonSerialized]
	[SaveVar(SaveVarFlags.Default)]
	public List<string> poolSkills = new List<string>();

	[NonSerialized]
	[SaveVar(SaveVarFlags.Default)]
	public Dictionary<Rarity, List<string>> poolSkillsByRarity = new Dictionary<Rarity, List<string>>();

	[NonSerialized]
	[SaveVar(SaveVarFlags.Default)]
	public Dictionary<Rarity, List<string>> poolGemsByRarity = new Dictionary<Rarity, List<string>>();

	[NonSerialized]
	[SaveVar(SaveVarFlags.Default)]
	public Dictionary<DescriptionTags, List<string>> poolSkillsByTag = new Dictionary<DescriptionTags, List<string>>();

	[NonSerialized]
	[SaveVar(SaveVarFlags.Default)]
	public Dictionary<DescriptionTags, List<string>> poolGemsByTag = new Dictionary<DescriptionTags, List<string>>();

	public override void OnStartServer()
	{
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		base.OnStartServer();
		foreach (Rarity value in Enum.GetValues(typeof(Rarity)))
		{
			poolGemsByRarity.Add(value, new List<string>());
			poolSkillsByRarity.Add(value, new List<string>());
		}
		foreach (DescriptionTags value2 in Enum.GetValues(typeof(DescriptionTags)))
		{
			poolGemsByTag.Add(value2, new List<string>());
			poolSkillsByTag.Add(value2, new List<string>());
		}
		HashSet<string> hashSet = new HashSet<string>();
		Enumerator<string> enumerator3;
		foreach (DewPlayer allHumanPlayer in DewPlayer.allHumanPlayers)
		{
			enumerator3 = allHumanPlayer.unlockedGameItems.GetEnumerator();
			try
			{
				while (enumerator3.MoveNext())
				{
					string current = enumerator3.Current;
					hashSet.Add(current);
				}
			}
			finally
			{
				((IDisposable)enumerator3/*cast due to constrained. prefix*/).Dispose();
			}
		}
		enumerator3 = NetworkedManagerBase<GameSettingsManager>.instance.bannedGameItems.GetEnumerator();
		try
		{
			while (enumerator3.MoveNext())
			{
				string current2 = enumerator3.Current;
				hashSet.Remove(current2);
			}
		}
		finally
		{
			((IDisposable)enumerator3/*cast due to constrained. prefix*/).Dispose();
		}
		if (hashSet.Count == 0)
		{
			string[] localUnlockedGameItems = DewPlayer.GetLocalUnlockedGameItems();
			foreach (string item in localUnlockedGameItems)
			{
				hashSet.Add(item);
			}
		}
		AddToPool(hashSet.ToArray());
	}

	private void AddToPool(string[] list)
	{
		Array values = Enum.GetValues(typeof(DescriptionTags));
		for (int i = 0; i < list.Length; i++)
		{
			if (poolSkills.Contains(list[i]) || poolGems.Contains(list[i]))
			{
				continue;
			}
			UnityEngine.Object byShortTypeName = DewResources.GetByShortTypeName(list[i], new ResourceLoadSettings
			{
				loadLight = true
			});
			if (byShortTypeName == null)
			{
				continue;
			}
			if (byShortTypeName is SkillTrigger skillTrigger && Dew.IsSkillIncludedInGame(((object)skillTrigger).GetType().Name) && !skillTrigger.isCharacterSkill && !skillTrigger.excludeFromPool)
			{
				poolSkills.Add(list[i]);
				poolSkillsByRarity[skillTrigger.rarity].Add(list[i]);
				if (skillTrigger.tags == DescriptionTags.None)
				{
					poolSkillsByTag[skillTrigger.tags].Add(list[i]);
				}
				else
				{
					foreach (DescriptionTags item in values)
					{
						if (item != DescriptionTags.None && skillTrigger.tags.HasFlag(item))
						{
							poolSkillsByTag[item].Add(list[i]);
						}
					}
				}
			}
			if (!(byShortTypeName is Gem gem) || !Dew.IsGemIncludedInGame(((object)gem).GetType().Name) || gem.excludeFromPool)
			{
				continue;
			}
			poolGems.Add(list[i]);
			poolGemsByRarity[gem.rarity].Add(list[i]);
			if (gem.tags == DescriptionTags.None)
			{
				poolGemsByTag[gem.tags].Add(list[i]);
				continue;
			}
			foreach (DescriptionTags item2 in values)
			{
				if (item2 != DescriptionTags.None && gem.tags.HasFlag(item2))
				{
					poolGemsByTag[item2].Add(list[i]);
				}
			}
		}
		if (!NetworkedManagerBase<ZoneManager>.instance.isInAnyTransition && ManagerBase<TransitionManager>.instance.state != TransitionManager.StateType.Loading)
		{
			DewResources.UnloadUnused();
		}
	}

	public static Rarity SelectRarity(PerRarityData<float> chances)
	{
		float value = UnityEngine.Random.value;
		Rarity result = Rarity.Common;
		if (value < chances.legendary)
		{
			result = Rarity.Legendary;
		}
		else if (value < chances.legendary + chances.epic)
		{
			result = Rarity.Epic;
		}
		else if (value < chances.legendary + chances.epic + chances.rare)
		{
			result = Rarity.Rare;
		}
		return result;
	}

	public Rarity SelectGemRarity(bool isHigh = false)
	{
		return SelectRarity(isHigh ? gemRarityChanceHigh : gemRarityChance);
	}

	public Rarity SelectSkillRarity(bool isHigh = false)
	{
		return SelectRarity(isHigh ? skillRarityChanceHigh : skillRarityChance);
	}

	public int SelectSkillLevel(Rarity rarity)
	{
		float a = skillLevelMinByZoneIndex.Get(rarity).Evaluate(NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex);
		float b = skillLevelMaxByZoneIndex.Get(rarity).Evaluate(NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex);
		return Mathf.Clamp(Mathf.RoundToInt(Mathf.Lerp(a, b, skillLevelRandomCurve.Evaluate(UnityEngine.Random.value))), 1, 100);
	}

	public void SelectSkillAndLevel(Rarity? rarity, out SkillTrigger skill, out int level)
	{
		if (!rarity.HasValue)
		{
			rarity = SelectSkillRarity();
		}
		List<string> list = poolSkillsByRarity[rarity.Value];
		skill = DewResources.GetByShortTypeName<SkillTrigger>(list[UnityEngine.Random.Range(0, list.Count)], default(ResourceLoadSettings));
		level = SelectSkillLevel(rarity.Value);
	}

	public int SelectGemQuality(Rarity rarity)
	{
		float a = gemQualityMinByZoneIndex.Get(rarity).Evaluate(NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex);
		float b = gemQualityMaxByZoneIndex.Get(rarity).Evaluate(NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex);
		return Mathf.Clamp(Mathf.RoundToInt(Mathf.Lerp(a, b, gemQualityRandomCurve.Evaluate(UnityEngine.Random.value)) / 10f) * 10, 10, 2000);
	}

	public void SelectGemAndQuality(Rarity? rarity, out Gem gem, out int quality)
	{
		if (!rarity.HasValue)
		{
			rarity = SelectGemRarity();
		}
		List<string> list = poolGemsByRarity[rarity.Value];
		gem = DewResources.GetByShortTypeName<Gem>(list[UnityEngine.Random.Range(0, list.Count)], default(ResourceLoadSettings));
		quality = SelectGemQuality(rarity.Value);
	}

	private void MirrorProcessed()
	{
	}
}
