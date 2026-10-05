using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Mon_Despair_BossAzurak : BossMonster, IPrewarmMonsterContributor
{
	[NonSerialized]
	public GameObject[] headParts;

	public void ContributeMonsterPrewarm(Dictionary<Monster, int> counts, int instanceCount)
	{
		Ai_Mon_Despair_BossAzurak_Roll byType = DewResources.GetByType<Ai_Mon_Despair_BossAzurak_Roll>(default(ResourceLoadSettings));
		Mon_Despair_AzurakRollPillar byType2 = DewResources.GetByType<Mon_Despair_AzurakRollPillar>(default(ResourceLoadSettings));
		if ((UnityEngine.Object)(object)byType != null && (UnityEngine.Object)(object)byType2 != null)
		{
			counts.TryGetValue(byType2, out var value);
			counts[byType2] = value + byType.pillarCount * 4 * instanceCount;
		}
		Mon_Despair_Displacer byType3 = DewResources.GetByType<Mon_Despair_Displacer>(default(ResourceLoadSettings));
		if ((UnityEngine.Object)(object)byType3 != null)
		{
			float num = (((UnityEngine.Object)(object)NetworkedManagerBase<GameManager>.instance != null) ? NetworkedManagerBase<GameManager>.instance.GetMultiplayerDifficultyFactor(reduceWhenDead: true) : 0f);
			int num2 = Mathf.RoundToInt(3f + num) * 2;
			counts.TryGetValue(byType3, out var value2);
			counts[byType3] = value2 + num2 * 6 * instanceCount;
		}
		Ai_Mon_Despair_BossAzurak_SpawnMonsters byType4 = DewResources.GetByType<Ai_Mon_Despair_BossAzurak_SpawnMonsters>(default(ResourceLoadSettings));
		Mon_Despair_AzurakMonsterSpawner byType5 = DewResources.GetByType<Mon_Despair_AzurakMonsterSpawner>(default(ResourceLoadSettings));
		if ((UnityEngine.Object)(object)byType4 != null && (UnityEngine.Object)(object)byType5 != null)
		{
			counts.TryGetValue(byType5, out var value3);
			counts[byType5] = value3 + byType4.spawnCount * 10 * instanceCount;
			int count = byType5.maxSpawnCount * byType4.spawnCount * instanceCount;
			AddMinionPrewarm<Mon_Despair_ParalyticFly>(counts, count);
			AddMinionPrewarm<Mon_Despair_Displacer>(counts, count);
			AddMinionPrewarm<Mon_Despair_DreadBug>(counts, count);
			AddMinionPrewarm<Mon_Despair_WretchedArtillery>(counts, count);
		}
	}

	private static void AddMinionPrewarm<T>(Dictionary<Monster, int> counts, int count) where T : Monster
	{
		T byType = DewResources.GetByType<T>(default(ResourceLoadSettings));
		if (!((UnityEngine.Object)(object)byType == null))
		{
			counts.TryGetValue(byType, out var value);
			if (count > value)
			{
				counts[byType] = count;
			}
		}
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			CreateBasicEffect(this, new UnstoppableEffect(), float.PositiveInfinity);
		}
	}

	public override void OnModelLoaded()
	{
		base.OnModelLoaded();
		headParts = Visual.model.GetCustomMappingArray<GameObject>("headParts");
	}

	protected override bool PoolAIUpdate(ref EntityAIContext context, PoolAbilityInfo abilityInfo)
	{
		if ((UnityEngine.Object)(object)context.targetEnemy == null)
		{
			return false;
		}
		return base.PoolAIUpdate(ref context, abilityInfo);
	}

	protected override void AIUpdate(ref EntityAIContext context)
	{
		base.AIUpdate(ref context);
		if (!((UnityEngine.Object)(object)context.targetEnemy == null))
		{
			AI.Helper_ChaseTarget();
		}
	}

	public override Type GetUniqueReward()
	{
		return typeof(St_U_Burrow);
	}

	private void MirrorProcessed()
	{
	}
}
