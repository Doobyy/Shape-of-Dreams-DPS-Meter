using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Dew Difficulty Settings", menuName = "Dew Difficulty Settings")]
[DewResourceLink(ResourceLinkBy.Name)]
public class DewDifficultySettings : ScriptableObject
{
	public Color difficultyColor;

	public Sprite icon;

	public float iconScale = 1f;

	public float maxPopulationMultiplier;

	public float regenOrbChanceMultiplier;

	public AnimationCurve predictionStrengthCurve;

	public float healRawMultiplier;

	public float scoreMultiplier;

	public float specialSkillChanceMultiplier = 1f;

	public int bossKillBonusStardust;

	[Tooltip("Number of zones per loop whose bosses use their harder ability pool (index 1)")]
	public int hardVariantBossZones;

	public bool enableBleedOuts = true;

	public Vector2Int lostSoulDistance;

	public float beneficialNodeMultiplier = 1f;

	public float harmfulNodeMultiplier = 1f;

	public float hunterSpreadChance;

	public float enemyHealthPercentage;

	public float enemyPowerPercentage;

	public float enemyMovementSpeedPercentage;

	public float enemyAttackSpeedPercentage;

	public float enemyAbilityHasteFlat;

	[Space(16f)]
	public float miniBossArmor;

	public float heroicBossArmor;

	public float healthFactorBeforeLoop = 1f;

	public float healthFactorAfterLoop = 1f;

	public float damageFactorBeforeLoop = 1f;

	public float damageFactorAfterLoop = 1f;

	public int positionSampleCount;

	public int positionSampleLagBehindFrames;

	public float positionSampleInterval;

	internal void ApplyDifficultyModifiers(Entity entity)
	{
		if (entity is IMonsterNoScaling)
		{
			return;
		}
		if (entity is Monster monster)
		{
			StatBonus statBonus = new StatBonus
			{
				maxHealthPercentage = enemyHealthPercentage,
				attackDamagePercentage = enemyPowerPercentage,
				abilityPowerPercentage = enemyPowerPercentage,
				movementSpeedPercentage = enemyMovementSpeedPercentage,
				attackSpeedPercentage = enemyAttackSpeedPercentage,
				abilityHasteFlat = enemyAbilityHasteFlat
			};
			if (monster.type == Monster.MonsterType.Boss)
			{
				statBonus.armorFlat = heroicBossArmor;
			}
			else if (monster.type == Monster.MonsterType.MiniBoss)
			{
				statBonus.armorFlat = miniBossArmor;
			}
			entity.Status.AddStatBonus(statBonus);
		}
		if (entity is Monster monster2)
		{
			float num;
			float num2;
			if (monster2 is BossMonster)
			{
				num = NetworkedManagerBase<GameManager>.instance.GetBossMonsterHealthMultiplierByScaling();
				num2 = NetworkedManagerBase<GameManager>.instance.GetBossMonsterDamageMultiplierByScaling();
			}
			else if (monster2.type == Monster.MonsterType.MiniBoss)
			{
				num = NetworkedManagerBase<GameManager>.instance.GetMiniBossMonsterHealthMultiplierByScaling();
				num2 = NetworkedManagerBase<GameManager>.instance.GetMiniBossMonsterDamageMultiplierByScaling();
			}
			else
			{
				num = NetworkedManagerBase<GameManager>.instance.GetRegularMonsterHealthMultiplierByScaling();
				num2 = NetworkedManagerBase<GameManager>.instance.GetRegularMonsterDamageMultiplierByScaling();
			}
			entity.Status.AddStatBonus(new StatBonus
			{
				maxHealthPercentage = (num - 1f) * 100f,
				attackDamagePercentage = (num2 - 1f) * 100f,
				abilityPowerPercentage = (num2 - 1f) * 100f
			});
		}
		else if (entity is Hero)
		{
			entity.takenHealProcessor.Add(delegate(ref HealData data, Actor actor, Entity target)
			{
				data.ApplyRawMultiplier(NetworkedManagerBase<GameManager>.instance.difficulty.healRawMultiplier);
			}, 100);
		}
	}

	public float GetScaledZoneIndexForDamage(float zi = float.NaN)
	{
		return GetScaledZoneIndex(isDamage: true, zi);
	}

	public float GetScaledZoneIndexForHealth(float zi = float.NaN)
	{
		return GetScaledZoneIndex(isDamage: false, zi);
	}

	private float GetScaledZoneIndex(bool isDamage, float zi)
	{
		if (float.IsNaN(zi))
		{
			zi = NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex;
		}
		List<int> zoneCountByTier = DewBuildProfile.current.content.zoneCountByTier;
		int num = 0;
		for (int i = 0; i < zoneCountByTier.Count; i++)
		{
			num += zoneCountByTier[i];
		}
		float num2 = (isDamage ? damageFactorBeforeLoop : healthFactorBeforeLoop);
		float num3 = (isDamage ? damageFactorAfterLoop : healthFactorAfterLoop);
		return Mathf.Min(zi, num) * num2 + Mathf.Max(zi - (float)num, 0f) * num3;
	}
}
