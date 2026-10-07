using System;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

public class RoomMod_Ambush : RoomModifierBase, IPrewarmRoomContributor
{
	private Mon_Special_AmbushSpawner _prop;

	private RoomSection _final;

	public void ContributeMonsterPrewarm(Dictionary<Monster, int> counts)
	{
		Mon_Special_AmbushSpawner byType = DewResources.GetByType<Mon_Special_AmbushSpawner>(default(ResourceLoadSettings));
		if ((UnityEngine.Object)(object)byType != null)
		{
			counts.TryGetValue(byType, out var value);
			counts[byType] = value + 1;
		}
		MonsterSpawnRule monsterSpawnRule = (((UnityEngine.Object)(object)SingletonDewNetworkBehaviour<Room>.instance != null) ? SingletonDewNetworkBehaviour<Room>.instance.monsters.defaultRule : null);
		if (monsterSpawnRule == null || monsterSpawnRule.pool == null)
		{
			return;
		}
		List<MonsterPool.SpawnRuleEntry> filteredEntries = monsterSpawnRule.pool.GetFilteredEntries();
		if (filteredEntries == null || filteredEntries.Count == 0)
		{
			return;
		}
		List<MonsterPool.SpawnRuleEntry> list = filteredEntries.Where((MonsterPool.SpawnRuleEntry e) =>
		{
			Monster asset2 = e.monster.asset;
			return asset2 != null && asset2.type != Monster.MonsterType.Lesser;
		}).ToList();
		if (list.Count == 0)
		{
			return;
		}
		int num = Mathf.CeilToInt(30f / (float)list.Count);
		foreach (MonsterPool.SpawnRuleEntry item in list)
		{
			Monster asset = item.monster.asset;
			if (asset != null)
			{
				counts.TryGetValue(asset, out var value2);
				counts[asset] = value2 + num;
			}
		}
	}

	public override void OnLateStartServer()
	{
		base.OnLateStartServer();
		if (!isNewInstance)
		{
			return;
		}
		Room_Barrier[] array = UnityEngine.Object.FindObjectsOfType<Room_Barrier>();
		for (int i = 0; i < array.Length; i++)
		{
			NetworkServer.Destroy(((Component)(object)array[i]).gameObject);
		}
		Room instance = SingletonDewNetworkBehaviour<Room>.instance;
		instance.rewards.DisableRegularRewards();
		instance.openRoomExitOnClear = false;
		instance.monsters.RemoveAllCamps();
		_final = instance.GetFinalSection();
		if (!_final.TryGetGoodNodePosition(out var anyRandomNode))
		{
			anyRandomNode = _final.GetAnyRandomNode();
		}
		_prop = Dew.SpawnEntity<Mon_Special_AmbushSpawner>(anyRandomNode, null, null, DewPlayer.creep, NetworkedManagerBase<GameManager>.instance.ambientLevel);
		_prop.ClientActorEvent_OnDestroyed += new Action<Actor>(OnPropDestroyed);
		RoomMod_Hunted hunted = DewResources.GetByType<RoomMod_Hunted>(default(ResourceLoadSettings));
		ModifyEntities((Entity e) =>
		{
			if (e is Monster && !(e is Mon_Special_AmbushSpawner))
			{
				CreateStatusEffect<Se_HunterBuff>(e, new CastInfo(e));
				hunted.ApplyHunterStatBonusAndAIPrediction(e, NetworkedManagerBase<ZoneManager>.instance.currentHuntLevel);
			}
		}, (Entity e) =>
		{
			if (e.Status.TryGetStatusEffect<Se_HunterBuff>(out var effect))
			{
				effect.Destroy();
			}
		});
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)_prop != null)
		{
			_prop.ClientActorEvent_OnDestroyed -= new Action<Actor>(OnPropDestroyed);
		}
	}

	private void OnPropDestroyed(Actor obj)
	{
		Entity[] array = NetworkedManagerBase<ActorManager>.instance.allEntities.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] is Monster monster && !monster.IsNullInactiveDeadOrKnockedOut())
			{
				monster.Destroy();
			}
		}
		SingletonDewNetworkBehaviour<Room>.instance.monsters.FinishAllOngoingSpawns();
		SingletonDewNetworkBehaviour<Room>.instance.monsters.RemoveAllCamps();
		SingletonDewNetworkBehaviour<Room>.instance.ClearRoom();
		RemoveModifier();
		Rift_RoomExit.instance.Open();
	}

	private void MirrorProcessed()
	{
	}
}
