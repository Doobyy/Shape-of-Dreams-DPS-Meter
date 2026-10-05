using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "New Dew Game Experience Settings", menuName = "Dew Game Experience Settings")]
public class DewGameplayExperienceSettings : SerializedScriptableObject
{
	[Header("Experience")]
	public int maxHeroLevel = 20;

	public float expDropDeviation = 0.1f;

	public float expGlobalMultiplier = 1f;

	public PerMonsterTypeData<float> expMultiplier;

	public float droppedExpMultiplierPerPlayer = 1f;

	public float gainedSkillHastePerSkillLevel = 4f;

	[Header("Regen Orb")]
	public PerMonsterTypeData<float> regenOrbDropChanceOnMaxHealth;

	public PerMonsterTypeData<float> regenOrbDropChanceOnLowHealth;

	[Header("Gold")]
	public Formula globalGoldEconomyMultiplierByZoneIndex = "1 + 0.20*x";

	public PerMonsterTypeData<float> killGold;

	public Formula killGoldMultiplierByZoneIndex = "1 + 0.4*x";

	public float killGoldDeviation = 0.1f;

	public float generalServiceGoldPriceMultiplier = 1f;

	public Formula generalServiceGoldPriceMultiplierByZoneIndex = "1 + 0.25*x";

	[Header("Stardust")]
	public PerMonsterTypeData<float> stardustDeathDropChance;

	public Vector2Int stardustDeathDropAmount;

	public Vector2Int stardustBossSoulAmount;

	[Header("Max Population (Spawned)")]
	public float maxGlobalPopulation = 7f;

	public float maxGlobalPopulationMultiplierPerPlayer = 1.1f;

	public Formula maxGlobalPopulationMultiplierByAmbientLevel;

	[Header("Max Population (Section)")]
	public float maxSectionPopulation = 5f;

	public float maxSectionPopulationMultiplierPerPlayerInSection = 1.1f;

	public Formula maxSectionPopulationMultiplierByAmbientLevel;

	[Header("Monsters")]
	public Formula miniBossSpawnChanceByNonMiniBossCombatNodesVisited;

	public float monsterWavesMultiplierPerPlayer = 1f;

	public float monsterSpawnPopulationMultiplierPerPlayer = 1f;

	public Formula monsterSpawnPopulationMultiplierByTurnIndex = "1";

	[Tooltip("Scales the delay between individual (non-boss) monster spawns. x = extra players. < 1 makes monsters trickle in faster in co-op.")]
	public Formula monsterSpawnDelayMultiplierByExtraPlayers = "Clamp(1 - x * 0.3, 0.5, 1.0)";

	public float monsterBonusHealthPercentagePerPlayer;

	public float monsterBonusPowerPercentagePerPlayer = 20f;

	public float miniBossBonusHealthPercentagePerPlayer = 110f;

	public float miniBossBonusPowerPercentagePerPlayer = 17f;

	public float bossBonusHealthPercentagePerPlayer = 120f;

	public float bossBonusPowerPercentagePerPlayer = 17f;

	[Header("Hunters")]
	public AnimationCurve welcomingSpawnPopMultiplierByArea;

	[Header("Buy, Sell")]
	public float valueGlobalMultiplier = 1f;

	public PerRarityData<int> gemValue;

	public PerRarityData<int> skillValue;

	public float valueMultiplierPerSkillLevel;

	public float valueMultiplierPerGemQuality;

	public float sellValueMultiplier = 0.4f;

	[Header("Dismantle, Upgrades")]
	public Formula gemDismantleDreamDustByQuality;

	public Formula gemUpgradeDreamDustByQuality;

	public int gemAddedQualityOnUpgrade = 50;

	public Formula skillDismantleDreamDustByLevel;

	public Formula skillUpgradeDreamDustByLevel;

	public PerRarityData<float> gemDismantleDreamDustMultiplier;

	public PerRarityData<float> skillDismantleDreamDustMultiplier;

	[Header("Cleanses")]
	public Formula gemCleanseCostByQuality;

	public Formula skillCleanseCostByLevel;

	public int gemCleanseMinQuality;

	public int skillCleanseMinLevel;

	[Header("Combat Reward Chances By Zone Index")]
	public Formula combatRewardMemoryChance;

	public Formula combatRewardFantasyChance;

	[Header("Boss Rewards by Zone Index")]
	public Formula bossRewardsGoldMin;

	public Formula bossRewardsGoldMax;

	public Formula bossRewardsDreamDustMin;

	public Formula bossRewardsDreamDustMax;

	[Header("Cooldown Reductions")]
	public float cooldownFloorRatioByAbilityHaste;

	public float cooldownFloorRatioBySkillUpgrade;
}
