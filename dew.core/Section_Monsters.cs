using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class Section_Monsters : RoomSectionComponent
{
	public SectionCombatAreaType combatAreaSettings;

	public CombatAreaActivateCondition activateCondition;

	public MonsterSpawnRule ruleOverride;

	public Transform spawnPosOverride;

	public Vector2 addedInitDelay;

	public bool spawnMiniBossInstead;

	public int miniBossCount = 1;

	public UnityEvent onStartCombatArea = new UnityEvent();

	public UnityEvent onClearCombatArea = new UnityEvent();

	[NonSerialized]
	public uint cachedSpawnSeed;

	[NonSerialized]
	public bool hasCachedSpawnSeed;

	private Room_Barrier[] _barriers;

	private static NavMeshPath _tempPath;

	public bool didClearCombatArea { get; set; }

	public bool isCombatActive { get; private set; }

	public bool isMarkedAsCombatArea { get; set; }

	public float maxPopulation
	{
		get
		{
			DewGameplayExperienceSettings ges = NetworkedManagerBase<GameManager>.instance.ges;
			float maxSectionPopulation = ges.maxSectionPopulation;
			float num = maxSectionPopulation;
			num += maxSectionPopulation * (ges.maxSectionPopulationMultiplierByAmbientLevel.Evaluate(NetworkedManagerBase<GameManager>.instance.ambientLevel - 1) - 1f);
			num += maxSectionPopulation * (ges.maxSectionPopulationMultiplierPerPlayerInSection - 1f) * (float)Mathf.Max(section.numOfHeroes - 1, 0);
			if ((UnityEngine.Object)(object)SingletonDewNetworkBehaviour<Room>.softInstance != null)
			{
				num *= SingletonDewNetworkBehaviour<Room>.softInstance.monsters.maxPopulationMultiplier;
			}
			return num * NetworkedManagerBase<GameManager>.instance.maxAndSpawnedPopulationMultiplier;
		}
	}

	public float population { get; private set; }

	public bool isOverPopulation => population >= maxPopulation;

	public void ComputeSpawnSeed()
	{
		uint num = (((UnityEngine.Object)(object)NetworkedManagerBase<ZoneManager>.instance != null) ? NetworkedManagerBase<ZoneManager>.instance.worldSeed : 0u);
		int num2 = (((UnityEngine.Object)(object)NetworkedManagerBase<ZoneManager>.instance != null) ? NetworkedManagerBase<ZoneManager>.instance.currentNodeIndex : 0);
		uint stableHashCode = transform.GetScenePath().GetStableHashCode();
		cachedSpawnSeed = (uint)(int)(num + num2) ^ stableHashCode;
		hasCachedSpawnSeed = true;
	}

	protected override void Awake()
	{
		base.Awake();
		section.onEntitiesChanged.AddListener(() =>
		{
			population = 0f;
			IReadOnlyList<ActorRef<Entity>> entities = section.entities;
			for (int i = 0; i < entities.Count; i++)
			{
				if (!entities[i].IsNullOrInactive() && entities[i].Get() is Monster monster)
				{
					population += monster.populationCost;
				}
			}
		});
	}

	private void Start()
	{
		_barriers = GetComponentsInChildren<Room_Barrier>(includeInactive: true);
		(activateCondition switch
		{
			CombatAreaActivateCondition.OnEnterFirstTime => section.onEnterFirstTime, 
			CombatAreaActivateCondition.OnEveryonePresent => section.onEveryonePresent, 
			_ => throw new ArgumentOutOfRangeException(), 
		}).AddListener(() =>
		{
			StartCoroutine(ActivateCombatRoutine());
		});
	}

	private IEnumerator ActivateCombatRoutine()
	{
		if (!isMarkedAsCombatArea)
		{
			yield break;
		}
		if ((UnityEngine.Object)(object)SingletonDewNetworkBehaviour<Room>.instance != null && (UnityEngine.Object)(object)SingletonDewNetworkBehaviour<Room>.instance.monsters != null && ((NetworkBehaviour)SingletonDewNetworkBehaviour<Room>.instance.monsters).isServer)
		{
			while ((UnityEngine.Object)(object)SingletonDewNetworkBehaviour<Room>.instance != null && (UnityEngine.Object)(object)SingletonDewNetworkBehaviour<Room>.instance.monsters != null && !SingletonDewNetworkBehaviour<Room>.instance.monsters.prewarmServerComplete)
			{
				yield return null;
			}
			if ((UnityEngine.Object)(object)SingletonDewNetworkBehaviour<Room>.instance == null || (UnityEngine.Object)(object)SingletonDewNetworkBehaviour<Room>.instance.monsters == null)
			{
				yield break;
			}
		}
		isCombatActive = true;
		NetworkedManagerBase<ZoneManager>.instance.currentRoom.numOfActivatedCombatAreas++;
		foreach (ActorRef<Entity> entity in section.entities)
		{
			if (!entity.IsNullOrInactive() && entity.Get() is Hero hero)
			{
				hero.MarkAsInCombat();
			}
		}
		Room_Barrier[] barriers = _barriers;
		foreach (Room_Barrier room_Barrier in barriers)
		{
			if (!((UnityEngine.Object)(object)room_Barrier == null))
			{
				room_Barrier.Close();
			}
		}
		try
		{
			onStartCombatArea.Invoke();
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		MonsterSpawnRule rule = ((ruleOverride != null) ? ruleOverride : SingletonDewNetworkBehaviour<Room>.instance.monsters.defaultRule);
		RoomMonsters.MonsterSpawnData monsterSpawnData = new RoomMonsters.MonsterSpawnData();
		SpawnMonsterSettings settings = new SpawnMonsterSettings
		{
			rule = rule,
			section = section,
			spawnPosGetter = ((spawnPosOverride == null) ? null : ((Func<Vector3>)(() => spawnPosOverride.position))),
			spawnRotGetter = ((spawnPosOverride == null) ? null : ((Func<Quaternion>)(() => spawnPosOverride.rotation))),
			initDelayFlat = UnityEngine.Random.Range(addedInitDelay.x, addedInitDelay.y),
			monsterSpawnData = monsterSpawnData,
			onFinish = () =>
			{
				isCombatActive = false;
				didClearCombatArea = true;
				Room_Barrier[] barriers2 = _barriers;
				foreach (Room_Barrier room_Barrier2 in barriers2)
				{
					if (!((UnityEngine.Object)(object)room_Barrier2 == null))
					{
						room_Barrier2.Open();
					}
				}
				try
				{
					onClearCombatArea.Invoke();
				}
				catch (Exception exception2)
				{
					Debug.LogException(exception2);
				}
			},
			random = new DewRandom(hasCachedSpawnSeed ? cachedSpawnSeed : transform.GetScenePath().GetStableHashCode()),
			positionRandom = new DewRandom((hasCachedSpawnSeed ? cachedSpawnSeed : transform.GetScenePath().GetStableHashCode()) ^ 0xDEADBEEFu),
			invalidatePooledInstanceOnUse = SingletonDewNetworkBehaviour<Room>.instance.monsters.prewarmCombatMonstersOnRoomStart
		};
		if (spawnMiniBossInstead)
		{
			yield return Dew.WaitForAggroedEnemiesRoutine();
			for (int i2 = 0; i2 < miniBossCount; i2++)
			{
				if (SingletonDewNetworkBehaviour<Room>.instance.didClearRoom)
				{
					break;
				}
				SingletonDewNetworkBehaviour<Room>.instance.monsters.SpawnMiniBoss(settings.Clone());
				yield return new WaitForSeconds(0.5f);
			}
		}
		else
		{
			SingletonDewNetworkBehaviour<Room>.instance.monsters.SpawnMonsters(settings);
		}
	}

	public Vector3 GetSpawnPositionInSection(float minDist, float maxDist, DewRandom random = null)
	{
		return GetSpawnPositionInSection((Vector3 pos) =>
		{
			bool flag = false;
			bool flag2 = true;
			foreach (DewPlayer gamePlayer in DewPlayer.gamePlayers)
			{
				float num = Vector2.Distance(gamePlayer.hero.agentPosition.ToXY(), pos.ToXY());
				if (num < minDist)
				{
					flag = true;
					break;
				}
				if (num < maxDist)
				{
					flag2 = false;
				}
			}
			return !flag && !flag2;
		}, random);
	}

	public Vector3 GetSpawnPositionInSection(Func<Vector3, bool> evaluator = null, DewRandom random = null)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected Obj, but got Unknown
		if (random == null)
		{
			random = DewRandom.instance;
		}
		if (_tempPath == null)
		{
			_tempPath = new NavMeshPath();
		}
		for (int i = 0; i < 30; i++)
		{
			Vector3 randomWorldPosition = section.GetRandomWorldPosition(random);
			if (i < 29)
			{
				if ((i >= 10 || evaluator == null || evaluator(randomWorldPosition)) && RoomMonsters.FilterSpawnPosition(randomWorldPosition, out var filteredPos, section.pathablePivot))
				{
					Debug.DrawLine(filteredPos, filteredPos + Vector3.up, Color.red, 2f);
					return filteredPos;
				}
				continue;
			}
			Debug.LogWarning("Using fallback spawn position for '" + SceneManager.GetActiveScene().name + "::" + name + "'");
			RoomMonsters.FilterSpawnPosition(randomWorldPosition, out var filteredPos2, section.pathablePivot);
			return filteredPos2;
		}
		throw new InvalidOperationException("");
	}

	private void OnDrawGizmos()
	{
		if (combatAreaSettings == SectionCombatAreaType.Yes || isMarkedAsCombatArea)
		{
			DewGizmos.DrawText("Combat", transform.position + Vector3.down * 4f, Color.red, 12);
		}
	}
}
