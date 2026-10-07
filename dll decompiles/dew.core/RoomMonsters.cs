using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

[RoomComponentStartDependency(typeof(RoomModifiers))]
public class RoomMonsters : RoomComponent
{
	public class MonsterSpawnData
	{
		public Entity lastKiller;

		public Vector3 lastDeathPosition;

		public float remainingPopulation;
	}

	private class Camp
	{
		public Vector3 position;

		public bool wasInRange;

		public bool isSpawned;

		public List<SpawnEntry> entries = new List<SpawnEntry>();

		public float nextRegenerateChance = 1f;

		public float nextRegenerateTime = float.PositiveInfinity;

		public DewRandom regenRandom;

		public DewRandom spawnRandom;

		public uint debugSeed;

		public Camp(Vector3 position, uint seed)
		{
			this.position = position;
			debugSeed = seed;
			regenRandom = new DewRandom(seed);
			spawnRandom = new DewRandom(seed + 100);
		}

		public void DoCampTick()
		{
			bool flag = Dew.GetClosestHeroDistance(position) < 40f;
			if (!flag && isSpawned && AreAllEntitiesSleeping())
			{
				Despawn();
			}
			if (!wasInRange & flag)
			{
				if (entries.Count == 0)
				{
					RegenerateEntries();
				}
				else if (Time.time > nextRegenerateTime)
				{
					if (UnityEngine.Random.value < nextRegenerateChance)
					{
						RegenerateEntries();
					}
					else
					{
						nextRegenerateTime = Time.time + float.PositiveInfinity;
					}
				}
				if (!isSpawned && HasAnyAliveEntries())
				{
					Spawn();
				}
			}
			wasInRange = flag;
		}

		private bool AreAllEntitiesSleeping()
		{
			foreach (SpawnEntry entry in entries)
			{
				if (!entry.instance.IsNullOrInactive() && !entry.instance.isSleeping)
				{
					return false;
				}
			}
			return true;
		}

		private bool HasAnyAliveEntries()
		{
			foreach (SpawnEntry entry in entries)
			{
				if (!entry.isKilled)
				{
					return true;
				}
			}
			return false;
		}

		public void RegenerateEntries()
		{
			if (isSpawned)
			{
				throw new InvalidOperationException();
			}
			RoomMonsters monsters = SingletonDewNetworkBehaviour<Room>.instance.monsters;
			float original = regenRandom.Range(monsters.campPopulation.x, monsters.campPopulation.y);
			original = NetworkedManagerBase<GameManager>.instance.GetAdjustedMonsterSpawnPopulation(original) * SingletonDewNetworkBehaviour<Room>.instance.monsters.spawnedPopMultiplier;
			IEnumerator<Monster> monsters2 = ((monsters.envSpawnPoolOverride != null) ? monsters.envSpawnPoolOverride : monsters.defaultRule.pool).GetMonsters(int.MaxValue, regenRandom);
			monsters2.MoveNext();
			float num = 0f;
			while (true)
			{
				float populationCost = monsters2.Current.populationCost;
				entries.Add(new SpawnEntry
				{
					prefab = monsters2.Current
				});
				num += populationCost;
				if (num > original)
				{
					break;
				}
				monsters2.MoveNext();
			}
			nextRegenerateChance *= 0.65f;
			nextRegenerateTime = float.PositiveInfinity;
		}

		public void Spawn()
		{
			for (int i = 0; i < entries.Count; i++)
			{
				SpawnEntry value = entries[i];
				if (value.isKilled)
				{
					continue;
				}
				int index = i;
				Vector3 pos = Dew.GetPositionOnGround(position + spawnRandom.InsideUnitSphere() * 6f);
				pos = Dew.GetValidAgentDestination_LinearSweep(position, pos);
				value.instance = Dew.SpawnEntity(value.prefab.asset, pos, Quaternion.Euler(0f, spawnRandom.Range(0f, 360f), 0f), NetworkedManagerBase<ActorManager>.instance.serverActor, DewPlayer.creep, NetworkedManagerBase<GameManager>.instance.ambientLevel, (Entity entity) =>
				{
					entity._accumulatedSleepTime = float.PositiveInfinity;
					entity.Visual.NetworkskipSpawning = true;
					if (entity is Monster monster)
					{
						monster.campPosition = pos;
					}
					entity.EntityEvent_OnDeath += (Action<EventInfoKill>)((EventInfoKill _) =>
					{
						SpawnEntry value2 = entries[index];
						value2.isKilled = true;
						entries[index] = value2;
						nextRegenerateTime = Time.time + float.PositiveInfinity;
						if (isSpawned)
						{
							foreach (SpawnEntry entry in entries)
							{
								if (!entry.isKilled)
								{
									return;
								}
							}
							isSpawned = false;
						}
					});
					SingletonDewNetworkBehaviour<Room>.instance.monsters.onBeforeSpawn?.Invoke(entity);
				});
				SingletonDewNetworkBehaviour<Room>.instance.monsters.onAfterSpawn?.Invoke(value.instance);
				if (SingletonDewNetworkBehaviour<Room>.instance.monsters.prewarmEnvMonstersOnRoomStart && (UnityEngine.Object)(object)value.instance != null)
				{
					value.instance.InvalidatePoolReuseEverywhere();
				}
				entries[i] = value;
			}
			isSpawned = true;
		}

		public void Despawn()
		{
			for (int i = 0; i < entries.Count; i++)
			{
				SpawnEntry value = entries[i];
				if (!value.instance.IsNullOrInactive())
				{
					value.instance.Destroy();
					value.instance = null;
					entries[i] = value;
				}
			}
			isSpawned = false;
		}
	}

	private struct SpawnEntry
	{
		public AssetRef<Entity> prefab;

		public Entity instance;

		public bool isKilled;
	}

	public struct PrewarmEntry
	{
		public uint assetId;

		public int count;

		public uint ownerNetId;
	}

	private struct LootPrewarmContext
	{
		public Pickup_LargeGoldOrb largeGold;

		public Pickup_MediumGoldOrb mediumGold;

		public Pickup_SmallGoldOrb smallGold;

		public Pickup_LargeExpOrb largeExp;

		public Pickup_MediumExpOrb mediumExp;

		public Pickup_SmallExpOrb smallExp;

		public Pickup_RegenOrb regen;

		public static LootPrewarmContext Build()
		{
			LootPrewarmContext result = default;
			Actor parentActor = (((UnityEngine.Object)(object)NetworkedManagerBase<ActorManager>.instance != null) ? NetworkedManagerBase<ActorManager>.instance.serverActor : null);
			result.largeGold = DewResources.GetByType<Pickup_LargeGoldOrb>(DewResources.GetSuggestedResourceLoadSettings(parentActor, typeof(Pickup_LargeGoldOrb)));
			result.mediumGold = DewResources.GetByType<Pickup_MediumGoldOrb>(DewResources.GetSuggestedResourceLoadSettings(parentActor, typeof(Pickup_MediumGoldOrb)));
			result.smallGold = DewResources.GetByType<Pickup_SmallGoldOrb>(DewResources.GetSuggestedResourceLoadSettings(parentActor, typeof(Pickup_SmallGoldOrb)));
			result.largeExp = DewResources.GetByType<Pickup_LargeExpOrb>(DewResources.GetSuggestedResourceLoadSettings(parentActor, typeof(Pickup_LargeExpOrb)));
			result.mediumExp = DewResources.GetByType<Pickup_MediumExpOrb>(DewResources.GetSuggestedResourceLoadSettings(parentActor, typeof(Pickup_MediumExpOrb)));
			result.smallExp = DewResources.GetByType<Pickup_SmallExpOrb>(DewResources.GetSuggestedResourceLoadSettings(parentActor, typeof(Pickup_SmallExpOrb)));
			result.regen = DewResources.GetByType<Pickup_RegenOrb>(DewResources.GetSuggestedResourceLoadSettings(parentActor, typeof(Pickup_RegenOrb)));
			return result;
		}
	}

	public const float CombatAreaScoreRandomness = 0.3f;

	public SafeAction<Entity> onBeforeSpawn;

	public SafeAction<Entity> onAfterSpawn;

	public int insertedCombatAreas;

	public float spawnedPopMultiplier = 1f;

	public float maxPopulationMultiplier = 1f;

	public float addedMirageChance;

	public float addedHunterChance;

	public bool clearRoomOnClearAllCombatAreas = true;

	public bool prewarmCombatMonstersOnRoomStart = true;

	public bool prewarmEnvMonstersOnRoomStart = true;

	public bool prewarmLootPickupsOnRoomStart = true;

	private bool prewarmAbilitiesAndEffectsOnRoomStart = true;

	[NonSerialized]
	public bool disableMiniBossRewards;

	public Dictionary<SpawnMonsterSettings, Coroutine> ongoingSpawns = new Dictionary<SpawnMonsterSettings, Coroutine>();

	public SafeAction onWelcomingSpawnStart;

	public SafeAction onWelcomingSpawnEnd;

	[NonSerialized]
	[SaveVar(SaveVarFlags.Default)]
	public List<ulong> destroyedSceneIds = new List<ulong>();

	private bool _didPrewarm;

	[NonSerialized]
	public uint cachedWelcomingSpawnSeed;

	[NonSerialized]
	public bool hasCachedWelcomingSpawnSeed;

	public const float CampWanderRange = 6f;

	private const float RespawnChanceMultiplier = 0.65f;

	private const float CampRespawnTimeAfterBeingSeen = float.PositiveInfinity;

	private const float CampScoreFuzziness = 0.15f;

	private const float BanCampDistanceFromHeroSpawn = 25f;

	private const float BanCampDistanceFromBanPosition = 3f;

	private const float CampActivateDistanceFromHero = 40f;

	private const float EnvSpawnTickInterval = 1f;

	[Space(16f)]
	public bool disableEnvSpawn;

	public MonsterPool envSpawnPoolOverride;

	public float campDensity;

	public Vector2 campPopulation = new Vector2(0.5f, 3f);

	private List<Camp> _camps;

	private float _nextEnvSpawnTickTime;

	[NonSerialized]
	public bool prewarmServerComplete;

	[NonSerialized]
	private PrewarmEntry[] _lastPrewarmEntries;

	private const float kAdaptiveOverrideMargin = 1.3f;

	private const int kOrbPrewarmCount = 20;

	private const int kOtherPickupPrewarmCount = 5;

	public const float OverpopulationStallDelayMin = 0.5f;

	public const float OverpopulationStallDelayMax = 1.5f;

	public const float SpawnDelayMin = 0.1f;

	public const float SpawnDelayMax = 0.5f;

	public MonsterSpawnRule defaultRule;

	private static NavMeshPath _tempPath;

	public bool didSetupCombatAreas { get; private set; }

	public bool isDoingHunterWelcomingSpawn { get; private set; }

	public override void OnRoomStart()
	{
		base.OnRoomStart();
		if (defaultRule == null && (UnityEngine.Object)(object)NetworkedManagerBase<ZoneManager>.instance != null && NetworkedManagerBase<ZoneManager>.instance.currentZone != null)
		{
			defaultRule = NetworkedManagerBase<ZoneManager>.instance.currentZone.defaultMonsters;
		}
		if (!isRevisit && !didSetupCombatAreas)
		{
			SetupCombatAreas();
		}
		DewResources.AddPreloadRule((MonoBehaviour)(object)this, (PreloadInterface preload) =>
		{
			if (defaultRule != null && defaultRule.pool != null)
			{
				preload.AddFromMonsterPool(defaultRule.pool);
			}
			foreach (RoomSection section in room.sections)
			{
				if (!(section == null) && !(section.monsters.ruleOverride == null) && !(section.monsters.ruleOverride.pool == null))
				{
					preload.AddFromMonsterPool(section.monsters.ruleOverride.pool);
				}
			}
			MonsterAbilityPrewarmOverrides.GlobalEntry[] globalPrewarms = MonsterAbilityPrewarmOverrides.globalPrewarms;
			if (globalPrewarms != null)
			{
				for (int i = 0; i < globalPrewarms.Length; i++)
				{
					if (!string.IsNullOrEmpty(globalPrewarms[i].abilityName))
					{
						preload.AddType(globalPrewarms[i].abilityName);
					}
				}
			}
			MonsterAbilityPrewarmOverrides.ConditionalEntry[] conditionalPrewarms = MonsterAbilityPrewarmOverrides.conditionalPrewarms;
			if (conditionalPrewarms != null)
			{
				HashSet<string> hashSet = new HashSet<string>();
				if ((UnityEngine.Object)(object)NetworkedManagerBase<ZoneManager>.softInstance != null)
				{
					List<ModifierData> modifiers = NetworkedManagerBase<ZoneManager>.instance.currentNode.modifiers;
					if (modifiers != null)
					{
						for (int j = 0; j < modifiers.Count; j++)
						{
							if (!string.IsNullOrEmpty(modifiers[j].type))
							{
								hashSet.Add(modifiers[j].type);
							}
						}
					}
				}
				List<RoomModifierBase> list = Dew.FindAllActorsOfType(out ListReturnHandle<RoomModifierBase> handle);
				for (int k = 0; k < list.Count; k++)
				{
					if ((UnityEngine.Object)(object)list[k] != null)
					{
						hashSet.Add(((object)list[k]).GetType().Name);
					}
				}
				handle.Return();
				if ((UnityEngine.Object)(object)NetworkedManagerBase<ZoneManager>.softInstance != null && NetworkedManagerBase<ZoneManager>.instance.isCurrentNodeHunted)
				{
					hashSet.Add("RoomMod_Hunted");
				}
				for (int l = 0; l < conditionalPrewarms.Length; l++)
				{
					MonsterAbilityPrewarmOverrides.ConditionalEntry conditionalEntry = conditionalPrewarms[l];
					if (!string.IsNullOrEmpty(conditionalEntry.abilityName) && !string.IsNullOrEmpty(conditionalEntry.modifierType) && hashSet.Contains(conditionalEntry.modifierType))
					{
						preload.AddType(conditionalEntry.abilityName);
					}
				}
			}
		});
	}

	public override void OnRoomStartServer()
	{
		base.OnRoomStartServer();
		if (defaultRule == null && NetworkedManagerBase<ZoneManager>.instance.currentZone != null)
		{
			defaultRule = NetworkedManagerBase<ZoneManager>.instance.currentZone.defaultMonsters;
		}
		if (!isRevisit && !didSetupCombatAreas)
		{
			SetupCombatAreas();
		}
		((MonoBehaviour)(object)this).StartCoroutine(OnRoomStartServerRoutine());
	}

	private IEnumerator OnRoomStartServerRoutine()
	{
		if (!_didPrewarm && (prewarmCombatMonstersOnRoomStart || prewarmEnvMonstersOnRoomStart))
		{
			RecordAdaptiveBaseline();
			yield return PrewarmAllRoutine();
			_didPrewarm = true;
		}
		else
		{
			prewarmServerComplete = true;
		}
		if (!isRevisit)
		{
			DoEnvSpawnRoomStart();
			if (NetworkedManagerBase<ZoneManager>.instance.currentNode.HasMainModifier())
			{
				RoomSection sectionFromWorldPos = room.GetSectionFromWorldPos(room.heroSpawnPos);
				if (sectionFromWorldPos != null)
				{
					sectionFromWorldPos.monsters.addedInitDelay += Vector2.one * 1.5f;
				}
			}
			if (clearRoomOnClearAllCombatAreas)
			{
				RefValue<int> remaining = new RefValue<int>(0);
				foreach (RoomSection s in room.sections)
				{
					if (!s.monsters.isMarkedAsCombatArea)
					{
						continue;
					}
					remaining.value++;
					s.monsters.onClearCombatArea.AddListener(() =>
					{
						remaining.value--;
						if (remaining.value <= 0)
						{
							s.monsters.StartCoroutine(Routine());
						}
					});
				}
			}
		}
		if (NetworkedManagerBase<ZoneManager>.instance.isCurrentNodeHunted && NetworkedManagerBase<ZoneManager>.instance.lastLoadNodeSettings.isTravelingRoom)
		{
			isDoingHunterWelcomingSpawn = true;
			onWelcomingSpawnStart?.Invoke();
			ListReturnHandle<Rift> handle;
			foreach (Rift item in Dew.FindAllActorsOfType(out handle))
			{
				item.isLocked = true;
			}
			handle.Return();
			((MonoBehaviour)(object)this).StartCoroutine(WelcomingSpawnRoutine());
		}
		Actor[] array = NetworkedManagerBase<ActorManager>.instance.allActors.ToArray();
		foreach (Actor actor in array)
		{
			ulong sceneId = ((NetworkBehaviour)actor).netIdentity.sceneId;
			if (sceneId != 0L && destroyedSceneIds.Contains(sceneId))
			{
				actor.Destroy();
			}
		}
		NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnActorRemove += new Action<Actor>(OnActorRemove);
		static IEnumerator Routine()
		{
			yield return Dew.WaitForAggroedEnemiesRoutine();
			if (!SingletonDewNetworkBehaviour<Room>.instance.didClearRoom)
			{
				SingletonDewNetworkBehaviour<Room>.instance.ClearRoom();
			}
		}
	}

	private IEnumerator WelcomingSpawnRoutine()
	{
		yield return new WaitWhile(() => NetworkedManagerBase<ZoneManager>.instance.isInRoomTransition);
		yield return new WaitForSeconds(UnityEngine.Random.Range(0.5f, 1f));
		if (NetworkedManagerBase<ZoneManager>.instance.currentNode.HasMainModifier() && !isRevisit)
		{
			yield return new WaitForSeconds(1.25f);
		}
		SpawnMonsters(new SpawnMonsterSettings
		{
			rule = defaultRule,
			initDelayMultiplier = 0.1f,
			invalidatePooledInstanceOnUse = prewarmCombatMonstersOnRoomStart,
			spawnPopulationMultiplier = NetworkedManagerBase<GameManager>.instance.ges.welcomingSpawnPopMultiplierByArea.Evaluate(room.map.mapData.area),
			random = (hasCachedWelcomingSpawnSeed ? new DewRandom(cachedWelcomingSpawnSeed) : null),
			onFinish = () =>
			{
				ListReturnHandle<Rift> handle;
				foreach (Rift item in Dew.FindAllActorsOfType(out handle))
				{
					item.isLocked = false;
				}
				handle.Return();
				isDoingHunterWelcomingSpawn = false;
				onWelcomingSpawnEnd?.Invoke();
			}
		});
	}

	private void OnActorRemove(Actor obj)
	{
		if (!(ManagerBase<TransitionManager>.softInstance == null) && !((UnityEngine.Object)(object)NetworkedManagerBase<ZoneManager>.softInstance == null) && ManagerBase<TransitionManager>.instance.state != TransitionManager.StateType.Loading && !NetworkedManagerBase<ZoneManager>.instance.isInAnyTransition && ((NetworkBehaviour)obj).netIdentity.sceneId != 0L)
		{
			destroyedSceneIds.Add(((NetworkBehaviour)obj).netIdentity.sceneId);
		}
	}

	public override void OnRoomStopServer()
	{
		base.OnRoomStopServer();
		DoEnvSpawnRoomStop();
		foreach (KeyValuePair<SpawnMonsterSettings, Coroutine> ongoingSpawn in ongoingSpawns)
		{
			((MonoBehaviour)(object)this).StopCoroutine(ongoingSpawn.Value);
			ongoingSpawn.Key.onFinish?.Invoke();
		}
		ongoingSpawns.Clear();
		((MonoBehaviour)(object)this).StopAllCoroutines();
		if ((UnityEngine.Object)(object)NetworkedManagerBase<ActorManager>.instance != null)
		{
			NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnActorRemove -= new Action<Actor>(OnActorRemove);
		}
	}

	public override void OnRoomStop()
	{
		base.OnRoomStop();
		foreach (Actor allActor in NetworkedManagerBase<ActorManager>.instance.allActors)
		{
			((NetworkBehaviour)allActor).netIdentity.sceneId = 0uL;
		}
		_didPrewarm = false;
		prewarmServerComplete = false;
		_lastPrewarmEntries = null;
		hasCachedWelcomingSpawnSeed = false;
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		DoEnvSpawnLogicUpdate(dt);
	}

	private void SetupCombatAreas()
	{
		didSetupCombatAreas = true;
		int requiredRiftAreas = 0;
		int numOfCurrentRiftAreas = 0;
		List<RoomSection> combatAreas = new List<RoomSection>();
		int inserted = 0;
		foreach (RoomSection section in room.sections)
		{
			if (section.monsters.combatAreaSettings == SectionCombatAreaType.Yes)
			{
				inserted--;
				InsertCombatArea(section);
			}
		}
		if (insertedCombatAreas <= 0)
		{
			return;
		}
		if (combatAreas.Count == 0)
		{
			List<RoomSection> list = new List<RoomSection>();
			foreach (RoomSection section2 in room.sections)
			{
				if (section2.monsters.combatAreaSettings == SectionCombatAreaType.Random)
				{
					list.Add(section2);
				}
			}
			if (list.Count == 0)
			{
				LogNotEnoughCandidates();
				return;
			}
			RoomSection s = list[room.GetRoomRandom(-2912).Range(0, list.Count)];
			InsertCombatArea(s);
		}
		Dictionary<RoomSection, float> dictionary = new Dictionary<RoomSection, float>();
		while (true)
		{
			dictionary.Clear();
			foreach (RoomSection section3 in room.sections)
			{
				if (combatAreas.Contains(section3) || section3.monsters.combatAreaSettings != SectionCombatAreaType.Random)
				{
					continue;
				}
				float num = float.PositiveInfinity;
				foreach (RoomSection item in combatAreas)
				{
					float navDistanceTo = section3.GetNavDistanceTo(item);
					if (navDistanceTo < num)
					{
						num = navDistanceTo;
					}
				}
				float num2 = num * (1f + room.GetRoomRandom(-2911).Range(-0.3f, 0.3f));
				if (numOfCurrentRiftAreas < requiredRiftAreas && section3.GetComponentInChildren<Room_RiftPos>() != null)
				{
					num2 += 100f;
				}
				dictionary.Add(section3, num2);
			}
			if (dictionary.Count == 0)
			{
				LogNotEnoughCandidates();
				break;
			}
			float num3 = float.NegativeInfinity;
			RoomSection s2 = null;
			foreach (KeyValuePair<RoomSection, float> item2 in dictionary)
			{
				if (!(item2.Value <= num3))
				{
					num3 = item2.Value;
					s2 = item2.Key;
				}
			}
			InsertCombatArea(s2);
			inserted++;
			if (inserted >= insertedCombatAreas)
			{
				if (numOfCurrentRiftAreas >= requiredRiftAreas)
				{
					break;
				}
				Debug.Log($"Inserted {inserted} combat areas but did not meet minimum rift area requirement ({numOfCurrentRiftAreas}/{requiredRiftAreas}), continuing...");
			}
		}
		void InsertCombatArea(RoomSection roomSection)
		{
			combatAreas.Add(roomSection);
			roomSection.monsters.isMarkedAsCombatArea = true;
			roomSection.monsters.ComputeSpawnSeed();
			if (roomSection.GetComponentInChildren<Room_RiftPos>() != null)
			{
				numOfCurrentRiftAreas++;
			}
		}
		void LogNotEnoughCandidates()
		{
			Debug.LogWarning("Room " + SceneManager.GetActiveScene().name + " does not have enough combat area candidates");
			Debug.LogWarning($"Inserted: {inserted}/{insertedCombatAreas}, Rift: {numOfCurrentRiftAreas}/{requiredRiftAreas}");
		}
	}

	[Server]
	public void OverrideMonsterType(Monster m)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void RoomMonsters::OverrideMonsterType(Monster)' called when server was not active");
			return;
		}
		if (defaultRule == null)
		{
			defaultRule = NetworkedManagerBase<ZoneManager>.instance.currentZone.defaultMonsters;
		}
		defaultRule = UnityEngine.Object.Instantiate(defaultRule);
		defaultRule.pool = UnityEngine.Object.Instantiate(defaultRule.pool);
		defaultRule.pool.entries.Clear();
		defaultRule.pool.entries.Add(new MonsterPool.SpawnRuleEntry
		{
			chance = 1f,
			monster = m,
			minCount = 1,
			maxCount = 1
		});
		foreach (RoomSection section in room.sections)
		{
			section.monsters.ruleOverride = defaultRule;
		}
	}

	[Server]
	public void FinishAllOngoingSpawns()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void RoomMonsters::FinishAllOngoingSpawns()' called when server was not active");
			return;
		}
		foreach (KeyValuePair<SpawnMonsterSettings, Coroutine> ongoingSpawn in ongoingSpawns)
		{
			ongoingSpawn.Key.onFinish?.Invoke();
			((MonoBehaviour)(object)this).StopCoroutine(ongoingSpawn.Value);
		}
		SingletonDewNetworkBehaviour<Room>.instance.monsters.ongoingSpawns.Clear();
	}

	public void SpawnMiniBoss(SpawnMonsterSettings settings, Entity ent = null, StatusEffect se = null)
	{
		SelectMiniBoss(out var entity, out var effect, settings.random);
		if ((UnityEngine.Object)(object)ent == null)
		{
			ent = entity;
		}
		if ((UnityEngine.Object)(object)se == null)
		{
			se = effect;
		}
		if (!(ent is ISpawnableAsMiniBoss))
		{
			Debug.LogWarning("Invalid entity for mini boss spawn provided: " + ((UnityEngine.Object)(object)ent).name);
			return;
		}
		settings.rule = ScriptableObject.CreateInstance<MonsterSpawnRule>();
		settings.rule.isBossSpawn = true;
		settings.rule.pool = ScriptableObject.CreateInstance<MonsterPool>();
		settings.rule.pool.entries = new List<MonsterPool.SpawnRuleEntry>();
		settings.rule.pool.entries.Add(new MonsterPool.SpawnRuleEntry
		{
			monster = (Monster)ent,
			chance = 1f,
			minCount = 1,
			maxCount = 1
		});
		settings.rule.initialDelay = Vector2.one * 0.5f;
		settings.rule.wavesMax = 1;
		settings.rule.wavesMin = 1;
		settings.rule.spawnMinDistance = 6f;
		settings.rule.spawnMaxDistance = 9f;
		settings.rule.onOverPopulation = OverpopulationBehavior.Ignore;
		settings.rule.populationPerWave = ((Monster)ent).populationCost * 0.55f * Vector2.one;
		settings.rule.waveTimeoutMin = float.PositiveInfinity;
		settings.rule.waveTimeoutMax = float.PositiveInfinity;
		if (settings.spawnPosGetter == null && settings.section != null)
		{
			settings.spawnPosGetter = () => Dew.GetValidAgentDestination_Closest(settings.section.pathablePivot, Dew.GetPositionOnGround(settings.section.pathablePivot + UnityEngine.Random.onUnitSphere * 4f));
		}
		SpawnMonsterSettings spawnMonsterSettings = settings;
		spawnMonsterSettings.beforeSpawn = (Action<Entity>)Delegate.Combine(spawnMonsterSettings.beforeSpawn, (Action<Entity>)((Entity e) =>
		{
			((Monster)e).Networktype = Monster.MonsterType.MiniBoss;
			((ISpawnableAsMiniBoss)e).OnBeforeSpawnAsMiniBoss();
		}));
		SpawnMonsterSettings spawnMonsterSettings2 = settings;
		spawnMonsterSettings2.afterSpawn = (Action<Entity>)Delegate.Combine(spawnMonsterSettings2.afterSpawn, (Action<Entity>)((Entity e) =>
		{
			CallOnCreateAsMiniBoss(e);
			e.InvalidatePoolReuseEverywhere();
			foreach (AbilityTrigger value in e.Ability.abilities.Values)
			{
				if ((UnityEngine.Object)(object)value != null)
				{
					value.InvalidatePoolReuseEverywhere();
				}
			}
			e.CreateStatusEffect(((object)se).GetType(), e, new CastInfo(e));
			e.CreateStatusEffect<Se_AutoDetectPresence>(e, new CastInfo(e));
			e.EntityEvent_OnDeath += (Action<EventInfoKill>)((EventInfoKill _) =>
			{
				((MonoBehaviour)(object)this).StartCoroutine(Routine());
			});
			IEnumerator Routine()
			{
				if (!SingletonDewNetworkBehaviour<Room>.instance.monsters.disableMiniBossRewards)
				{
					Vector3 pos = e.agentPosition;
					yield return new WaitForSeconds(1.5f);
					SingletonDewNetworkBehaviour<Room>.instance.rewards.DropChaosReward(pos, isHighQuality: false);
				}
			}
		}));
		SpawnMonsters(settings);
	}

	[ClientRpc]
	private void CallOnCreateAsMiniBoss(Entity e)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteNetworkBehaviour((NetworkWriter)(object)val, (NetworkBehaviour)(object)e);
		((NetworkBehaviour)this).SendRPCInternal("System.Void RoomMonsters::CallOnCreateAsMiniBoss(Entity)", 1965624741, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	public void SelectMiniBoss(out Entity entity, out MiniBossEffect effect, DewRandom random)
	{
		List<MiniBossEffect> list = DewResources.FindAllByType<MiniBossEffect>(default(ResourceLoadSettings)).ToList();
		List<Entity> list2 = new List<Entity>();
		foreach (MonsterPool.SpawnRuleEntry filteredEntry in defaultRule.pool.GetFilteredEntries())
		{
			if (filteredEntry.monster.asset is ISpawnableAsMiniBoss && !list2.Contains(filteredEntry.monster.asset))
			{
				list2.Add(filteredEntry.monster.asset);
			}
		}
		if (random == null)
		{
			random = DewRandom.instance;
		}
		entity = list2[random.Range(0, list2.Count)];
		effect = list[random.Range(0, list.Count)];
	}

	private void DoEnvSpawnRoomStart()
	{
		if (disableEnvSpawn)
		{
			return;
		}
		_camps = new List<Camp>();
		int num = Mathf.RoundToInt(campDensity * room.map.mapData.area);
		IBanCampsNearby[] array = Dew.FindInterfacesOfType<IBanCampsNearby>(includeInactive: false);
		Vector2[] array2 = new Vector2[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			IBanCampsNearby banCampsNearby = array[i];
			array2[i] = ((Component)banCampsNearby).transform.position.ToXY();
		}
		for (int j = 0; j < num; j++)
		{
			AddCampPos(array2);
		}
		foreach (Hero allHero in NetworkedManagerBase<ActorManager>.instance.allHeroes)
		{
			allHero.Control.ClientEvent_OnTeleport += new Action<Vector3, Vector3>(ClientEventOnTeleport);
		}
	}

	private void DoEnvSpawnRoomStop()
	{
		if ((UnityEngine.Object)(object)NetworkedManagerBase<ActorManager>.instance == null)
		{
			return;
		}
		foreach (Hero allHero in NetworkedManagerBase<ActorManager>.instance.allHeroes)
		{
			allHero.Control.ClientEvent_OnTeleport -= new Action<Vector3, Vector3>(ClientEventOnTeleport);
		}
	}

	private void DoEnvSpawnLogicUpdate(float dt)
	{
		if (((NetworkBehaviour)this).isServer && _camps != null && !(Time.time < _nextEnvSpawnTickTime))
		{
			_nextEnvSpawnTickTime = Time.time + 1f;
			DoCampTickImmediately();
		}
	}

	private void DoCampTickImmediately()
	{
		if (_camps != null)
		{
			for (int i = 0; i < _camps.Count; i++)
			{
				_camps[i].DoCampTick();
			}
		}
	}

	private void ClientEventOnTeleport(Vector3 arg1, Vector3 arg2)
	{
		if (Vector2.Distance(arg1.ToXY(), arg2.ToXY()) > 5f)
		{
			DoCampTickImmediately();
		}
	}

	private void AddCampPos(Vector2[] bannedPositions)
	{
		DewRandom roomRandom = room.GetRoomRandom(-91917);
		bool flag = NetworkedManagerBase<ZoneManager>.instance.currentNode.HasMainModifier();
		List<Vector2> list = new List<Vector2>();
		IReadOnlyList<(int, int)> innerPropNodeIndices = room.map.mapData.innerPropNodeIndices;
		for (int i = 0; i < 30; i++)
		{
			Vector2 worldPos = room.map.mapData.cells.GetWorldPos(innerPropNodeIndices[roomRandom.Range(0, innerPropNodeIndices.Count)]);
			list.Add(worldPos);
		}
		Vector3 heroSpawnPos = room.heroSpawnPos;
		Vector3 position = Dew.SelectBestWithScore((IList<Vector2>)list, (Func<Vector2, int, float>)((Vector2 v, int _) =>
		{
			if (Vector2.Distance(heroSpawnPos.ToXY(), v) < 25f)
			{
				return float.NegativeInfinity;
			}
			Vector2[] array = bannedPositions;
			for (int j = 0; j < array.Length; j++)
			{
				if (Vector2.Distance(array[j], v) < 3f)
				{
					return float.NegativeInfinity;
				}
			}
			float num = float.PositiveInfinity;
			foreach (Camp camp in _camps)
			{
				num = Mathf.Min(num, Vector2.Distance(camp.position.ToXY(), v));
			}
			return num;
		}), 0.15f, roomRandom).ToXZ();
		position = Dew.GetPositionOnGround(position);
		position = Dew.GetValidAgentPosition(position);
		if (!flag || !(Vector2.Distance(position.ToXY(), heroSpawnPos.ToXY()) < 25f))
		{
			DewRandom roomRandom2 = SingletonDewNetworkBehaviour<Room>.instance.GetRoomRandom(-4090);
			_camps.Add(new Camp(position, roomRandom2.NextUInt32()));
		}
	}

	[Server]
	public void RemoveAllCamps()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void RoomMonsters::RemoveAllCamps()' called when server was not active");
		}
		else
		{
			if (_camps == null)
			{
				return;
			}
			foreach (Camp camp in _camps)
			{
				foreach (SpawnEntry entry in camp.entries)
				{
					if (!entry.instance.IsNullInactiveDeadOrKnockedOut())
					{
						entry.instance.Destroy();
					}
				}
			}
			_camps.Clear();
		}
	}

	private void OnDrawGizmosSelected()
	{
		if (_camps == null)
		{
			return;
		}
		foreach (Camp camp in _camps)
		{
			Gizmos.color = Color.red;
			Gizmos.DrawSphere(camp.position, 1.5f);
		}
	}

	private void RecordAdaptiveBaseline()
	{
		if (!(ManagerBase<SpawnManager>.instance == null))
		{
			AdaptiveAbilityBaseline.CommitCurrentRoomToLastUsage(ManagerBase<SpawnManager>.instance.previousRoomWasBoss);
		}
	}

	private MonsterSpawnRule GetResolvedDefaultRule()
	{
		if (defaultRule != null)
		{
			return defaultRule;
		}
		if ((UnityEngine.Object)(object)NetworkedManagerBase<ZoneManager>.instance == null || NetworkedManagerBase<ZoneManager>.instance.currentZone == null)
		{
			return null;
		}
		return NetworkedManagerBase<ZoneManager>.instance.currentZone.defaultMonsters;
	}

	public IEnumerator PrewarmAllRoutine()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		SpawnManager instance = ManagerBase<SpawnManager>.instance;
		int num = (((UnityEngine.Object)(object)NetworkedManagerBase<ZoneManager>.softInstance != null) ? NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex : (-1));
		bool flag = instance != null && num != instance.lastClearedZoneIndex;
		if (flag)
		{
			SpawnManager.ClearMonsterAbilityPools();
			AdaptiveAbilityBaseline.Clear();
			instance.lastClearedZoneIndex = num;
		}
		float num2 = CalculatePreemptivePopMultiplier();
		float num3 = spawnedPopMultiplier;
		if (num2 != 1f)
		{
			spawnedPopMultiplier *= num2;
		}
		Dictionary<Monster, int> counts = new Dictionary<Monster, int>();
		Dictionary<Monster, int> miniBossCounts = new Dictionary<Monster, int>();
		Dictionary<(uint assetId, uint owner), int> abilityCounts = new Dictionary<(uint, uint), int>();
		Dictionary<GameObject, int> pickupCounts = new Dictionary<GameObject, int>();
		List<PrewarmEntry> list = new List<PrewarmEntry>();
		List<GameObject> variantBuffer = new List<GameObject>();
		try
		{
			BuildPrewarmEntries(counts, miniBossCounts, abilityCounts, pickupCounts, list);
		}
		finally
		{
			if (num2 != 1f)
			{
				spawnedPopMultiplier = num3;
			}
		}
		_lastPrewarmEntries = list.ToArray();
		RpcPrewarm(_lastPrewarmEntries, flag);
		yield return null;
		HashSet<GameObject> hashSet = new HashSet<GameObject>();
		foreach (KeyValuePair<Monster, int> item in counts)
		{
			if ((UnityEngine.Object)(object)item.Key != null)
			{
				hashSet.Add(((Component)(object)item.Key).gameObject);
			}
		}
		foreach (KeyValuePair<(uint, uint), int> item2 in abilityCounts)
		{
			if (item2.Key.Item1 != 0)
			{
				GameObject gameObject = ResolveClientPrewarmPrefab(item2.Key.Item1, item2.Key.Item2);
				if (gameObject != null)
				{
					hashSet.Add(gameObject);
				}
			}
		}
		foreach (KeyValuePair<GameObject, int> item3 in pickupCounts)
		{
			if (item3.Key == null)
			{
				continue;
			}
			ResolveAbilityVariantsForPrewarm(item3.Key, variantBuffer);
			foreach (GameObject item4 in variantBuffer)
			{
				hashSet.Add(item4);
			}
		}
		SpawnManager.ReleaseInactivePoolsExcept(hashSet);
		DewEffect.AgeAndDropInactivePools();
		foreach (KeyValuePair<Monster, int> item5 in counts)
		{
			if (!((UnityEngine.Object)(object)item5.Key == null) && item5.Value > 0)
			{
				SpawnManager.Prewarm<Monster>(item5.Key, item5.Value);
			}
		}
		Dictionary<GameObject, int> dictionary = new Dictionary<GameObject, int>();
		foreach (KeyValuePair<(uint, uint), int> item6 in abilityCounts)
		{
			if (item6.Key.Item1 != 0 && item6.Value > 0)
			{
				GameObject gameObject2 = ResolveClientPrewarmPrefab(item6.Key.Item1, item6.Key.Item2);
				if (!(gameObject2 == null))
				{
					dictionary.TryGetValue(gameObject2, out var value);
					dictionary[gameObject2] = value + item6.Value;
				}
			}
		}
		foreach (KeyValuePair<GameObject, int> item7 in dictionary)
		{
			SpawnManager.Prewarm(item7.Key, item7.Value);
		}
		foreach (KeyValuePair<GameObject, int> item8 in pickupCounts)
		{
			if (item8.Key == null || item8.Value <= 0)
			{
				continue;
			}
			ResolveAbilityVariantsForPrewarm(item8.Key, variantBuffer);
			foreach (GameObject item9 in variantBuffer)
			{
				SpawnManager.Prewarm(item9, item8.Value);
			}
		}
		if (ManagerBase<SpawnManager>.instance != null)
		{
			ManagerBase<SpawnManager>.instance.previousRoomWasBoss = IsBossRoom();
		}
		prewarmServerComplete = true;
	}

	private void BuildPrewarmEntries(Dictionary<Monster, int> counts, Dictionary<Monster, int> miniBossCounts, Dictionary<(uint assetId, uint owner), int> abilityCounts, Dictionary<GameObject, int> pickupCounts, List<PrewarmEntry> entries)
	{
		if (!isRevisit && prewarmCombatMonstersOnRoomStart)
		{
			AccumulateSectionPrewarmCounts(counts, miniBossCounts);
		}
		if (prewarmCombatMonstersOnRoomStart)
		{
			AccumulateHunterPrewarmCounts(counts);
		}
		if (!isRevisit && prewarmEnvMonstersOnRoomStart)
		{
			AccumulateEnvPrewarmCounts(counts);
		}
		AccumulateRoomContributors(counts);
		AccumulateAlwaysContributors(counts);
		ApplyForceMonsterPrewarms(counts);
		int num = 0;
		foreach (KeyValuePair<Monster, int> count in counts)
		{
			if ((UnityEngine.Object)(object)count.Key != null && count.Value > 0)
			{
				num += count.Value;
			}
		}
		AdaptiveAbilityBaseline.SetCurrentRoomMonsterCount(num);
		if (prewarmAbilitiesAndEffectsOnRoomStart)
		{
			foreach (KeyValuePair<Monster, int> count2 in counts)
			{
				if (!((UnityEngine.Object)(object)count2.Key == null) && count2.Value > 0)
				{
					CollectAbilityTriggers(((Component)(object)count2.Key).gameObject, count2.Value, count2.Value, abilityCounts, 0u);
				}
			}
			MonsterAbilityPrewarmConfig instance = MonsterAbilityPrewarmConfig.instance;
			if (instance != null)
			{
				foreach (KeyValuePair<Monster, int> count3 in counts)
				{
					if (!((UnityEngine.Object)(object)count3.Key == null) && count3.Value > 0)
					{
						instance.AppendPrewarmCounts(count3.Key, count3.Value, count3.Value, abilityCounts);
					}
				}
			}
			ApplyGlobalPrewarms(abilityCounts, num);
			ApplyConditionalPrewarms(abilityCounts, num);
			AccumulateMiragePrewarms(abilityCounts, counts);
			AdaptiveAbilityBaseline.PopulateFromPeakUsage(abilityCounts, 1.3f, num);
		}
		if (prewarmLootPickupsOnRoomStart)
		{
			LootPrewarmContext lootPrewarmContext = LootPrewarmContext.Build();
			AddPickupPrefab(pickupCounts, (Component)(object)lootPrewarmContext.largeGold, 20);
			AddPickupPrefab(pickupCounts, (Component)(object)lootPrewarmContext.mediumGold, 20);
			AddPickupPrefab(pickupCounts, (Component)(object)lootPrewarmContext.smallGold, 20);
			AddPickupPrefab(pickupCounts, (Component)(object)lootPrewarmContext.largeExp, 20);
			AddPickupPrefab(pickupCounts, (Component)(object)lootPrewarmContext.mediumExp, 20);
			AddPickupPrefab(pickupCounts, (Component)(object)lootPrewarmContext.smallExp, 20);
			AddPickupPrefab(pickupCounts, (Component)(object)lootPrewarmContext.regen, 5);
		}
		foreach (KeyValuePair<Monster, int> count4 in counts)
		{
			if (!((UnityEngine.Object)(object)count4.Key == null) && count4.Value > 0 && ((Component)(object)count4.Key).TryGetComponent(out NetworkIdentity component) && component.assetId != 0)
			{
				entries.Add(new PrewarmEntry
				{
					assetId = component.assetId,
					count = count4.Value
				});
			}
		}
		foreach (KeyValuePair<(uint, uint), int> abilityCount in abilityCounts)
		{
			if (abilityCount.Key.Item1 != 0 && abilityCount.Value > 0)
			{
				entries.Add(new PrewarmEntry
				{
					assetId = abilityCount.Key.Item1,
					count = abilityCount.Value,
					ownerNetId = abilityCount.Key.Item2
				});
			}
		}
		foreach (KeyValuePair<GameObject, int> pickupCount in pickupCounts)
		{
			if (!(pickupCount.Key == null) && pickupCount.Value > 0 && pickupCount.Key.TryGetComponent<NetworkIdentity>(out var component2) && component2.assetId != 0)
			{
				entries.Add(new PrewarmEntry
				{
					assetId = component2.assetId,
					count = pickupCount.Value
				});
			}
		}
	}

	private void ApplyForceMonsterPrewarms(Dictionary<Monster, int> counts)
	{
		MonsterAbilityPrewarmOverrides.ForcePrewarmEntry[] forceMonsterPrewarms = MonsterAbilityPrewarmOverrides.forceMonsterPrewarms;
		if (forceMonsterPrewarms == null || forceMonsterPrewarms.Length == 0)
		{
			return;
		}
		string name = SceneManager.GetActiveScene().name;
		if (string.IsNullOrEmpty(name))
		{
			return;
		}
		for (int i = 0; i < forceMonsterPrewarms.Length; i++)
		{
			MonsterAbilityPrewarmOverrides.ForcePrewarmEntry forcePrewarmEntry = forceMonsterPrewarms[i];
			if (forcePrewarmEntry.count <= 0 || string.IsNullOrEmpty(forcePrewarmEntry.roomName) || string.IsNullOrEmpty(forcePrewarmEntry.monsterName) || !string.Equals(forcePrewarmEntry.roomName, name, StringComparison.OrdinalIgnoreCase))
			{
				continue;
			}
			Monster byShortTypeName = DewResources.GetByShortTypeName<Monster>(forcePrewarmEntry.monsterName, default(ResourceLoadSettings));
			if (!((UnityEngine.Object)(object)byShortTypeName == null))
			{
				counts.TryGetValue(byShortTypeName, out var value);
				if (forcePrewarmEntry.count > value)
				{
					counts[byShortTypeName] = forcePrewarmEntry.count;
				}
			}
		}
	}

	private static void ApplyGlobalPrewarms(Dictionary<(uint assetId, uint owner), int> abilityCounts, int totalMonsterCount)
	{
		MonsterAbilityPrewarmOverrides.GlobalEntry[] globalPrewarms = MonsterAbilityPrewarmOverrides.globalPrewarms;
		if (globalPrewarms == null)
		{
			return;
		}
		for (int i = 0; i < globalPrewarms.Length; i++)
		{
			MonsterAbilityPrewarmOverrides.GlobalEntry globalEntry = globalPrewarms[i];
			if (string.IsNullOrEmpty(globalEntry.abilityName) || globalEntry.count <= 0f)
			{
				continue;
			}
			int num = (globalEntry.perMonster ? Mathf.CeilToInt(globalEntry.count * (float)totalMonsterCount) : Mathf.CeilToInt(globalEntry.count));
			if (num <= 0)
			{
				continue;
			}
			UnityEngine.Object byShortTypeName = DewResources.GetByShortTypeName(globalEntry.abilityName);
			if (byShortTypeName == null)
			{
				continue;
			}
			GameObject gameObject2;
			if (byShortTypeName is GameObject gameObject)
			{
				gameObject2 = gameObject;
			}
			else
			{
				if (!(byShortTypeName is Component component))
				{
					continue;
				}
				gameObject2 = component.gameObject;
			}
			if (gameObject2.TryGetComponent<NetworkIdentity>(out var component2) && component2.assetId != 0)
			{
				(uint, uint) key = (component2.assetId, 0u);
				abilityCounts.TryGetValue(key, out var value);
				if (num > value)
				{
					abilityCounts[key] = num;
				}
			}
		}
	}

	private static void ApplyConditionalPrewarms(Dictionary<(uint assetId, uint owner), int> abilityCounts, int totalMonsterCount)
	{
		MonsterAbilityPrewarmOverrides.ConditionalEntry[] conditionalPrewarms = MonsterAbilityPrewarmOverrides.conditionalPrewarms;
		if (conditionalPrewarms == null || conditionalPrewarms.Length == 0)
		{
			return;
		}
		HashSet<string> hashSet = new HashSet<string>();
		if ((UnityEngine.Object)(object)NetworkedManagerBase<ZoneManager>.softInstance != null)
		{
			List<ModifierData> modifiers = NetworkedManagerBase<ZoneManager>.instance.currentNode.modifiers;
			if (modifiers != null)
			{
				for (int i = 0; i < modifiers.Count; i++)
				{
					if (!string.IsNullOrEmpty(modifiers[i].type))
					{
						hashSet.Add(modifiers[i].type);
					}
				}
			}
		}
		List<RoomModifierBase> list = Dew.FindAllActorsOfType(out ListReturnHandle<RoomModifierBase> handle);
		for (int j = 0; j < list.Count; j++)
		{
			RoomModifierBase roomModifierBase = list[j];
			if (!((UnityEngine.Object)(object)roomModifierBase == null))
			{
				hashSet.Add(((object)roomModifierBase).GetType().Name);
			}
		}
		handle.Return();
		if ((UnityEngine.Object)(object)NetworkedManagerBase<ZoneManager>.softInstance != null && NetworkedManagerBase<ZoneManager>.instance.isCurrentNodeHunted)
		{
			hashSet.Add("RoomMod_Hunted");
		}
		if (hashSet.Count == 0)
		{
			return;
		}
		for (int k = 0; k < conditionalPrewarms.Length; k++)
		{
			MonsterAbilityPrewarmOverrides.ConditionalEntry conditionalEntry = conditionalPrewarms[k];
			if (string.IsNullOrEmpty(conditionalEntry.modifierType) || conditionalEntry.count <= 0f || !hashSet.Contains(conditionalEntry.modifierType))
			{
				continue;
			}
			int num = (conditionalEntry.perMonster ? Mathf.CeilToInt(conditionalEntry.count * (float)totalMonsterCount) : Mathf.CeilToInt(conditionalEntry.count));
			if (num <= 0)
			{
				continue;
			}
			UnityEngine.Object byShortTypeName = DewResources.GetByShortTypeName(conditionalEntry.abilityName);
			if (byShortTypeName == null)
			{
				continue;
			}
			GameObject gameObject2;
			if (byShortTypeName is GameObject gameObject)
			{
				gameObject2 = gameObject;
			}
			else
			{
				if (!(byShortTypeName is Component component))
				{
					continue;
				}
				gameObject2 = component.gameObject;
			}
			if (gameObject2.TryGetComponent<NetworkIdentity>(out var component2) && component2.assetId != 0)
			{
				(uint, uint) key = (component2.assetId, 0u);
				abilityCounts.TryGetValue(key, out var value);
				if (num > value)
				{
					abilityCounts[key] = num;
				}
			}
		}
	}

	private void AccumulateMiragePrewarms(Dictionary<(uint assetId, uint owner), int> abilityCounts, Dictionary<Monster, int> counts)
	{
		GameMod_MirageSkin gameMod_MirageSkin = (((UnityEngine.Object)(object)GameMod_MirageSkin.softInstance != null) ? GameMod_MirageSkin.softInstance : GameMod_MirageSkin.instance);
		if ((UnityEngine.Object)(object)gameMod_MirageSkin == null || gameMod_MirageSkin.currentZonePool == null || gameMod_MirageSkin.currentZonePool.Count == 0)
		{
			return;
		}
		int num = 0;
		foreach (KeyValuePair<Monster, int> count in counts)
		{
			if (!((UnityEngine.Object)(object)count.Key == null) && count.Value > 0 && count.Key.type != Monster.MonsterType.Lesser && count.Key.type != Monster.MonsterType.Boss)
			{
				num += count.Value;
			}
		}
		float num2 = gameMod_MirageSkin.GetCurrentBaseMirageChance() + addedMirageChance;
		int num3 = Mathf.CeilToInt((float)num * num2);
		if (num <= 0 || num3 <= 0)
		{
			return;
		}
		foreach (AssetRef<MirageSkinEffect> item in gameMod_MirageSkin.currentZonePool)
		{
			MirageSkinEffect asset = item.asset;
			if ((UnityEngine.Object)(object)asset == null)
			{
				continue;
			}
			string name = ((object)asset).GetType().Name;
			AddPrefabAbilityPrewarm(abilityCounts, ((Component)(object)asset).gameObject, num3);
			if (MonsterAbilityPrewarmOverrides.mirageSubEffects.TryGetValue(name, out var value))
			{
				MonsterAbilityPrewarmOverrides.MirageSub[] array = value;
				for (int i = 0; i < array.Length; i++)
				{
					MonsterAbilityPrewarmOverrides.MirageSub mirageSub = array[i];
					AddNamedAbilityPrewarm(abilityCounts, mirageSub.abilityName, num3 * mirageSub.countPerMirage);
				}
			}
		}
	}

	private static void AddPrefabAbilityPrewarm(Dictionary<(uint assetId, uint owner), int> abilityCounts, GameObject prefabGo, int count)
	{
		if (count > 0 && !(prefabGo == null) && prefabGo.TryGetComponent<NetworkIdentity>(out var component) && component.assetId != 0)
		{
			(uint, uint) key = (component.assetId, 0u);
			abilityCounts.TryGetValue(key, out var value);
			if (count > value)
			{
				abilityCounts[key] = count;
			}
		}
	}

	private static void AddNamedAbilityPrewarm(Dictionary<(uint assetId, uint owner), int> abilityCounts, string typeName, int count)
	{
		if (count <= 0 || string.IsNullOrEmpty(typeName))
		{
			return;
		}
		UnityEngine.Object byShortTypeName = DewResources.GetByShortTypeName(typeName);
		GameObject gameObject2;
		if (byShortTypeName is GameObject gameObject)
		{
			gameObject2 = gameObject;
		}
		else
		{
			if (!(byShortTypeName is Component component))
			{
				return;
			}
			gameObject2 = component.gameObject;
		}
		if (gameObject2.TryGetComponent<NetworkIdentity>(out var component2) && component2.assetId != 0)
		{
			(uint, uint) key = (component2.assetId, 0u);
			abilityCounts.TryGetValue(key, out var value);
			if (count > value)
			{
				abilityCounts[key] = count;
			}
		}
	}

	private float CalculatePreemptivePopMultiplier()
	{
		IPrewarmPopulationMultiplier[] array = Dew.FindInterfacesOfType<IPrewarmPopulationMultiplier>(includeInactive: false);
		if (array == null || array.Length == 0)
		{
			return 1f;
		}
		float num = 1f;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] != null)
			{
				num *= array[i].GetSpawnedPopMultiplierContribution();
			}
		}
		return num;
	}

	private bool IsBossRoom()
	{
		MonsterSpawnRule resolvedDefaultRule = GetResolvedDefaultRule();
		if (resolvedDefaultRule != null && resolvedDefaultRule.isBossSpawn)
		{
			return true;
		}
		if ((UnityEngine.Object)(object)room == null || room.sections == null)
		{
			return false;
		}
		foreach (RoomSection section in room.sections)
		{
			if (!(section == null) && !(section.monsters == null) && section.monsters.ruleOverride != null && section.monsters.ruleOverride.isBossSpawn)
			{
				return true;
			}
		}
		return false;
	}

	private static void AddPickupPrefab(Dictionary<GameObject, int> pickupCounts, Component prefab, int count)
	{
		if (!(prefab == null) && count > 0)
		{
			GameObject gameObject = prefab.gameObject;
			pickupCounts.TryGetValue(gameObject, out var value);
			pickupCounts[gameObject] = value + count;
		}
	}

	private static void CollectAbilityTriggers(GameObject entityPrefabOrInstance, int basicAttackCount, int abilityCount, Dictionary<(uint assetId, uint owner), int> abilityCounts, uint ownerNetId)
	{
		if (entityPrefabOrInstance == null || (basicAttackCount <= 0 && abilityCount <= 0))
		{
			return;
		}
		EntityAbility componentInChildren = entityPrefabOrInstance.GetComponentInChildren<EntityAbility>(includeInactive: true);
		if ((UnityEngine.Object)(object)componentInChildren == null)
		{
			return;
		}
		if (basicAttackCount > 0)
		{
			uint networkAssetId = DewResources.GetNetworkAssetId(componentInChildren.attackAbilityPreset.guid);
			if (networkAssetId != 0)
			{
				(uint, uint) key = (networkAssetId, ownerNetId);
				abilityCounts.TryGetValue(key, out var value);
				abilityCounts[key] = value + basicAttackCount;
			}
		}
		if (abilityCount <= 0 || componentInChildren.abilityPreset == null)
		{
			return;
		}
		for (int i = 0; i < componentInChildren.abilityPreset.Length; i++)
		{
			uint networkAssetId2 = DewResources.GetNetworkAssetId(componentInChildren.abilityPreset[i].guid);
			if (networkAssetId2 != 0)
			{
				(uint, uint) key2 = (networkAssetId2, ownerNetId);
				abilityCounts.TryGetValue(key2, out var value2);
				abilityCounts[key2] = value2 + abilityCount;
			}
		}
	}

	[ClientRpc]
	private void RpcPrewarm(PrewarmEntry[] entries, bool clearMonsterPools)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_RoomMonsters_002FPrewarmEntry_005B_005D((NetworkWriter)(object)val, entries);
		NetworkWriterExtensions.WriteBool((NetworkWriter)(object)val, clearMonsterPools);
		((NetworkBehaviour)this).SendRPCInternal("System.Void RoomMonsters::RpcPrewarm(RoomMonsters/PrewarmEntry[],System.Boolean)", -452919250, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[TargetRpc]
	private void TpcReplayPrewarm(NetworkConnectionToClient conn, PrewarmEntry[] entries)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_RoomMonsters_002FPrewarmEntry_005B_005D((NetworkWriter)(object)val, entries);
		((NetworkBehaviour)this).SendTargetRPCInternal((NetworkConnection)(object)conn, "System.Void RoomMonsters::TpcReplayPrewarm(Mirror.NetworkConnectionToClient,RoomMonsters/PrewarmEntry[])", -1653298731, (NetworkWriter)(object)val, 0);
		NetworkWriterPool.Return(val);
	}

	[Server]
	public void ReplayPrewarmTo(NetworkConnectionToClient conn)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void RoomMonsters::ReplayPrewarmTo(Mirror.NetworkConnectionToClient)' called when server was not active");
		}
		else if (conn != null && prewarmServerComplete && _lastPrewarmEntries != null)
		{
			TpcReplayPrewarm(conn, _lastPrewarmEntries);
		}
	}

	private void ApplyPrewarm(PrewarmEntry[] entries)
	{
		HashSet<GameObject> hashSet = new HashSet<GameObject>();
		for (int i = 0; i < entries.Length; i++)
		{
			GameObject gameObject = ResolveClientPrewarmPrefab(entries[i].assetId, entries[i].ownerNetId);
			if (gameObject != null)
			{
				hashSet.Add(gameObject);
			}
		}
		SpawnManager.ReleaseInactivePoolsExcept(hashSet);
		DewEffect.AgeAndDropInactivePools();
		Dictionary<GameObject, int> dictionary = new Dictionary<GameObject, int>();
		for (int j = 0; j < entries.Length; j++)
		{
			PrewarmEntry prewarmEntry = entries[j];
			GameObject gameObject2 = ResolveClientPrewarmPrefab(prewarmEntry.assetId, prewarmEntry.ownerNetId);
			if (!(gameObject2 == null))
			{
				dictionary.TryGetValue(gameObject2, out var value);
				dictionary[gameObject2] = value + prewarmEntry.count;
			}
		}
		foreach (KeyValuePair<GameObject, int> item in dictionary)
		{
			SpawnManager.Prewarm(item.Key, item.Value);
		}
	}

	private static GameObject ResolveClientPrewarmPrefab(uint assetId, uint ownerNetId)
	{
		if (!DewResources.database.netObjectAssetIdToGuid.TryGetValue(assetId, out var value) || !DewResources.database.guidToType.TryGetValue(value, out var value2) || !typeof(Actor).IsAssignableFrom(value2))
		{
			return DewResources.GetNetworkedPrefab(assetId);
		}
		DewPlayer component = null;
		if (ownerNetId != 0)
		{
			NetworkIdentity spawnedInServerOrClient = Utils.GetSpawnedInServerOrClient(ownerNetId);
			if ((bool)(UnityEngine.Object)(object)spawnedInServerOrClient)
			{
				((Component)(object)spawnedInServerOrClient).TryGetComponent(out component);
			}
		}
		VariantDef varDef;
		if ((UnityEngine.Object)(object)component != null && (UnityEngine.Object)(object)component.hero != null)
		{
			varDef = DewResources.GetSuggestedVarDef(component.hero, value2);
		}
		else if (ownerNetId == 0)
		{
			varDef = DewResources.GetSuggestedVarDef(((UnityEngine.Object)(object)NetworkedManagerBase<ActorManager>.softInstance != null) ? NetworkedManagerBase<ActorManager>.instance.serverActor : null, value2);
		}
		else
		{
			varDef = default;
			if (value2.IsSubclassOf(typeof(AbilityInstance)))
			{
				varDef = varDef.Add(DewResources.vQualityAdjusted);
				DewPlayer dewPlayer = DewPlayer.local;
				if (ManagerBase<CameraManager>.instance != null && (UnityEngine.Object)(object)ManagerBase<CameraManager>.instance.focusedEntity != null && (UnityEngine.Object)(object)ManagerBase<CameraManager>.instance.focusedEntity.owner != null && ManagerBase<CameraManager>.instance.focusedEntity.owner.isHumanPlayer)
				{
					dewPlayer = ManagerBase<CameraManager>.instance.focusedEntity.owner;
				}
				if ((UnityEngine.Object)(object)component != null && component.isHumanPlayer && (UnityEngine.Object)(object)dewPlayer != null && (UnityEngine.Object)(object)dewPlayer != (UnityEngine.Object)(object)component)
				{
					varDef = varDef.Add(DewResources.vOtherPlayersTonedDown);
				}
			}
		}
		GameObject networkedPrefab = DewResources.GetNetworkedPrefab(assetId, new ResourceLoadSettings
		{
			varDef = varDef
		});
		if (!(networkedPrefab != null))
		{
			return DewResources.GetNetworkedPrefab(assetId);
		}
		return networkedPrefab;
	}

	private static void ResolveAbilityVariantsForPrewarm(uint assetId, Dictionary<uint, HashSet<uint>> abilityOwners, List<GameObject> output)
	{
		output.Clear();
		if (assetId == 0)
		{
			return;
		}
		if (abilityOwners != null && abilityOwners.TryGetValue(assetId, out var value) && value.Count > 0)
		{
			foreach (uint item in value)
			{
				GameObject gameObject = ResolveClientPrewarmPrefab(assetId, item);
				if (gameObject != null)
				{
					output.Add(gameObject);
				}
			}
			return;
		}
		GameObject gameObject2 = ResolveClientPrewarmPrefab(assetId, 0u);
		if (gameObject2 != null)
		{
			output.Add(gameObject2);
		}
	}

	private static void ResolveAbilityVariantsForPrewarm(GameObject prefab, List<GameObject> output)
	{
		output.Clear();
		if (!(prefab == null))
		{
			if (!prefab.TryGetComponent<NetworkIdentity>(out var component) || component.assetId == 0)
			{
				output.Add(prefab);
			}
			else
			{
				ResolveAbilityVariantsForPrewarm(component.assetId, null, output);
			}
		}
	}

	private void AccumulateSectionPrewarmCounts(Dictionary<Monster, int> counts, Dictionary<Monster, int> miniBossCounts)
	{
		MonsterSpawnRule resolvedDefaultRule = GetResolvedDefaultRule();
		IPreemptiveMiniBossSection[] preemptiveMods = Dew.FindInterfacesOfType<IPreemptiveMiniBossSection>(includeInactive: false);
		foreach (RoomSection section in room.sections)
		{
			if (section == null || !section.monsters.isMarkedAsCombatArea)
			{
				continue;
			}
			MonsterSpawnRule monsterSpawnRule = ((section.monsters.ruleOverride != null) ? section.monsters.ruleOverride : resolvedDefaultRule);
			if (monsterSpawnRule == null)
			{
				continue;
			}
			if (section.monsters.spawnMiniBossInstead)
			{
				AddMiniBossPrewarmEstimate(counts, miniBossCounts, monsterSpawnRule, section.monsters.miniBossCount);
				continue;
			}
			int preemptiveMiniBossCount = GetPreemptiveMiniBossCount(preemptiveMods, section);
			if (preemptiveMiniBossCount > 0)
			{
				AddMiniBossPrewarmEstimate(counts, miniBossCounts, monsterSpawnRule, preemptiveMiniBossCount);
				continue;
			}
			SimulateRuleSpawns(counts, monsterSpawnRule, 1f, section.monsters.cachedSpawnSeed, "section='" + section.name + "' rule='" + monsterSpawnRule.name + "'");
		}
	}

	private int GetPreemptiveMiniBossCount(IPreemptiveMiniBossSection[] preemptiveMods, RoomSection section)
	{
		if (preemptiveMods == null)
		{
			return 0;
		}
		int num = 0;
		foreach (IPreemptiveMiniBossSection preemptiveMiniBossSection in preemptiveMods)
		{
			if (preemptiveMiniBossSection != null && !(preemptiveMiniBossSection.GetTargetSection(room) != section))
			{
				num += preemptiveMiniBossSection.GetMiniBossCount();
			}
		}
		return num;
	}

	private void AccumulateHunterPrewarmCounts(Dictionary<Monster, int> counts)
	{
		if (((NetworkBehaviour)this).isServer)
		{
			MonsterSpawnRule resolvedDefaultRule = GetResolvedDefaultRule();
			if (!(resolvedDefaultRule == null) && !((UnityEngine.Object)(object)NetworkedManagerBase<ZoneManager>.instance == null) && NetworkedManagerBase<ZoneManager>.instance.isCurrentNodeHunted && NetworkedManagerBase<ZoneManager>.instance.lastLoadNodeSettings.isTravelingRoom)
			{
				DewRandom roomRandom = room.GetRoomRandom(-8162);
				uint s = roomRandom.s0;
				uint s2 = roomRandom.s1;
				uint s3 = roomRandom.s2;
				uint s4 = roomRandom.s3;
				cachedWelcomingSpawnSeed = roomRandom.NextUInt32();
				hasCachedWelcomingSpawnSeed = true;
				roomRandom.s0 = s;
				roomRandom.s1 = s2;
				roomRandom.s2 = s3;
				roomRandom.s3 = s4;
				float spawnPopulationMultiplier = NetworkedManagerBase<GameManager>.instance.ges.welcomingSpawnPopMultiplierByArea.Evaluate(room.map.mapData.area);
				SimulateRuleSpawns(counts, resolvedDefaultRule, spawnPopulationMultiplier, cachedWelcomingSpawnSeed, "hunter rule='" + resolvedDefaultRule.name + "'");
			}
		}
	}

	private void AccumulateEnvPrewarmCounts(Dictionary<Monster, int> counts)
	{
		if (!((NetworkBehaviour)this).isServer || disableEnvSpawn)
		{
			return;
		}
		MonsterPool monsterPool = ((envSpawnPoolOverride != null) ? envSpawnPoolOverride : GetResolvedDefaultRule()?.pool);
		if (monsterPool == null)
		{
			return;
		}
		int num = Mathf.RoundToInt(campDensity * room.map.mapData.area);
		if (num <= 0)
		{
			return;
		}
		Dictionary<Monster, int> dictionary = new Dictionary<Monster, int>();
		DewRandom roomRandom = room.GetRoomRandom(-4090);
		uint s = roomRandom.s0;
		uint s2 = roomRandom.s1;
		uint s3 = roomRandom.s2;
		uint s4 = roomRandom.s3;
		for (int i = 0; i < num; i++)
		{
			uint campSeed = roomRandom.NextUInt32();
			Dictionary<Monster, int> dictionary2 = new Dictionary<Monster, int>();
			SimulateCamp(dictionary2, monsterPool, campSeed);
			foreach (KeyValuePair<Monster, int> item in dictionary2)
			{
				dictionary.TryGetValue(item.Key, out var value);
				dictionary[item.Key] = value + item.Value;
			}
		}
		roomRandom.s0 = s;
		roomRandom.s1 = s2;
		roomRandom.s2 = s3;
		roomRandom.s3 = s4;
		HashSet<Monster> hashSet = new HashSet<Monster>();
		foreach (MonsterPool.SpawnRuleEntry filteredEntry in monsterPool.GetFilteredEntries())
		{
			Monster asset = filteredEntry.monster.asset;
			if (asset != null && filteredEntry.EvaluateCondition())
			{
				hashSet.Add(asset);
			}
		}
		foreach (KeyValuePair<Monster, int> item2 in dictionary)
		{
			if (hashSet.Contains(item2.Key))
			{
				counts.TryGetValue(item2.Key, out var value2);
				counts[item2.Key] = value2 + item2.Value;
			}
		}
	}

	private void SimulateCamp(Dictionary<Monster, int> counts, MonsterPool pool, uint campSeed)
	{
		DewRandom dewRandom = new DewRandom(campSeed);
		float original = dewRandom.Range(campPopulation.x, campPopulation.y);
		original = NetworkedManagerBase<GameManager>.instance.GetAdjustedMonsterSpawnPopulation(original) * spawnedPopMultiplier;
		IEnumerator<Monster> monsters = pool.GetMonsters(int.MaxValue, dewRandom);
		if (!monsters.MoveNext())
		{
			return;
		}
		float num = 0f;
		int num2 = 0;
		while (++num2 <= 4096)
		{
			Monster current = monsters.Current;
			if (current != null)
			{
				Monster monster = current;
				counts.TryGetValue(monster, out var value);
				counts[monster] = value + 1;
				num += monster.populationCost;
			}
			else
			{
				num++;
			}
			if (num > original || !monsters.MoveNext())
			{
				break;
			}
		}
	}

	private void AddMiniBossPrewarmEstimate(Dictionary<Monster, int> counts, Dictionary<Monster, int> miniBossCounts, MonsterSpawnRule resolvedDefaultRule, int miniBossCount)
	{
		if (resolvedDefaultRule == null || resolvedDefaultRule.pool == null || miniBossCount <= 0)
		{
			return;
		}
		foreach (MonsterPool.SpawnRuleEntry filteredEntry in resolvedDefaultRule.pool.GetFilteredEntries())
		{
			Monster asset = filteredEntry.monster.asset;
			if (asset != null && asset is ISpawnableAsMiniBoss)
			{
				counts.TryGetValue(asset, out var value);
				counts[asset] = value + miniBossCount;
				miniBossCounts.TryGetValue(asset, out var value2);
				miniBossCounts[asset] = value2 + miniBossCount;
				if (asset is IPrewarmMiniBossContributor prewarmMiniBossContributor && IsContributorsEnabled())
				{
					prewarmMiniBossContributor.ContributeMiniBossPrewarm(counts, miniBossCount);
				}
			}
		}
	}

	private static bool IsContributorsEnabled()
	{
		if (!(ManagerBase<SpawnManager>.instance == null))
		{
			return ManagerBase<SpawnManager>.instance.prewarmContributorsEnabled;
		}
		return true;
	}

	private void AccumulateRoomContributors(Dictionary<Monster, int> counts)
	{
		if (!IsContributorsEnabled())
		{
			return;
		}
		IPrewarmRoomContributor[] array = Dew.FindInterfacesOfType<IPrewarmRoomContributor>(includeInactive: false);
		if (array != null && array.Length != 0)
		{
			IPrewarmRoomContributor[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i]?.ContributeMonsterPrewarm(counts);
			}
		}
	}

	private void AccumulateAlwaysContributors(Dictionary<Monster, int> counts)
	{
		if (!IsContributorsEnabled())
		{
			return;
		}
		Queue<Monster> queue = new Queue<Monster>(counts.Keys);
		HashSet<Monster> hashSet = new HashSet<Monster>();
		while (queue.Count > 0)
		{
			Monster monster = queue.Dequeue();
			if ((UnityEngine.Object)(object)monster == null || !hashSet.Add(monster) || !(monster is IPrewarmMonsterContributor prewarmMonsterContributor) || !counts.TryGetValue(monster, out var value) || value <= 0)
			{
				continue;
			}
			HashSet<Monster> hashSet2 = new HashSet<Monster>(counts.Keys);
			prewarmMonsterContributor.ContributeMonsterPrewarm(counts, value);
			foreach (Monster key in counts.Keys)
			{
				if (!((UnityEngine.Object)(object)key == null) && !hashSet2.Contains(key) && !hashSet.Contains(key))
				{
					queue.Enqueue(key);
				}
			}
		}
	}

	private void SimulateRuleSpawns(Dictionary<Monster, int> counts, MonsterSpawnRule rule, float spawnPopulationMultiplier, uint seed, string source)
	{
		if (rule == null || rule.pool == null)
		{
			return;
		}
		if (rule.isBossSpawn)
		{
			foreach (MonsterPool.SpawnRuleEntry filteredEntry in rule.pool.GetFilteredEntries())
			{
				Monster asset = filteredEntry.monster.asset;
				if (asset != null)
				{
					counts.TryGetValue(asset, out var value);
					counts[asset] = value + 1;
				}
			}
			return;
		}
		DewRandom dewRandom = new DewRandom(seed);
		dewRandom.Range(rule.initialDelay.x, rule.initialDelay.y);
		int original = dewRandom.Range(rule.wavesMin, rule.wavesMax + 1);
		original = NetworkedManagerBase<GameManager>.instance.GetAdjustedMonsterWaves(original, dewRandom);
		Dictionary<Monster, int> dictionary = new Dictionary<Monster, int>();
		for (int i = 0; i < original; i++)
		{
			dewRandom.Range(rule.waveTimeoutMin, rule.waveTimeoutMax);
			float original2 = dewRandom.Range(rule.populationPerWave.x, rule.populationPerWave.y);
			original2 = NetworkedManagerBase<GameManager>.instance.GetAdjustedMonsterSpawnPopulation(original2) * spawnPopulationMultiplier * spawnedPopMultiplier;
			dewRandom.Range(rule.nextWavePopulationThreshold.x, rule.nextWavePopulationThreshold.y);
			IEnumerator<Monster> monsters = rule.pool.GetMonsters(int.MaxValue, dewRandom);
			if (!monsters.MoveNext())
			{
				break;
			}
			float num = 0f;
			bool flag = true;
			int num2 = 0;
			int num3 = 0;
			while (++num2 <= 4096)
			{
				if (flag)
				{
					flag = false;
				}
				else
				{
					dewRandom.Range(0.1f, 0.5f);
				}
				Monster current = monsters.Current;
				if (current != null)
				{
					Monster monster = current;
					dictionary.TryGetValue(monster, out var value2);
					dictionary[monster] = value2 + 1;
					num += monster.populationCost;
				}
				else
				{
					num++;
				}
				dewRandom.Value();
				num3++;
				if (num > original2 || !monsters.MoveNext())
				{
					break;
				}
			}
		}
		HashSet<Monster> hashSet = new HashSet<Monster>();
		foreach (MonsterPool.SpawnRuleEntry filteredEntry2 in rule.pool.GetFilteredEntries())
		{
			Monster asset2 = filteredEntry2.monster.asset;
			if (asset2 != null && filteredEntry2.EvaluateCondition())
			{
				hashSet.Add(asset2);
			}
		}
		foreach (KeyValuePair<Monster, int> item in dictionary)
		{
			if (hashSet.Contains(item.Key))
			{
				counts.TryGetValue(item.Key, out var value3);
				counts[item.Key] = value3 + item.Value;
			}
		}
	}

	private int EstimateEnvRulePrewarmCount(Monster monster, MonsterPool.SpawnRuleEntry entry, int campCount)
	{
		float num = Mathf.Max(monster.populationCost, 0.05f);
		float num2 = NetworkedManagerBase<GameManager>.instance.GetAdjustedMonsterSpawnPopulation(campPopulation.y) * spawnedPopMultiplier;
		return Mathf.Max(1, Mathf.CeilToInt(num2 / num)) * campCount;
	}

	private IEnumerator WaitForPopulationRoutine(MonsterSpawnRule rule, RoomSection section, float requiredPopulation, RefValue<bool> didFail)
	{
		if (requiredPopulation <= 0f || !IsOverPop())
		{
			didFail.value = false;
			yield break;
		}
		switch (rule.onOverPopulation)
		{
		case OverpopulationBehavior.Stall:
		{
			float stallStart = Time.time;
			do
			{
				yield return new WaitForSeconds(UnityEngine.Random.Range(0.5f, 1.5f));
				if (Time.time - stallStart > rule.stallCancelTimeout)
				{
					didFail.value = true;
					yield break;
				}
			}
			while (IsOverPop());
			break;
		}
		case OverpopulationBehavior.Cancel:
			didFail.value = true;
			yield break;
		default:
			throw new ArgumentOutOfRangeException();
		case OverpopulationBehavior.Ignore:
			break;
		}
		didFail.value = false;
		bool IsOverPop()
		{
			if (!NetworkedManagerBase<GameManager>.instance.isSpawnOverPopulation)
			{
				if (section != null)
				{
					return section.monsters.isOverPopulation;
				}
				return false;
			}
			return true;
		}
	}

	public void SpawnMonsters(SpawnMonsterSettings settings)
	{
		if (settings.random == null)
		{
			settings.random = new DewRandom(room.GetRoomRandom(-8162).NextUInt32());
		}
		ongoingSpawns.Add(settings, ((MonoBehaviour)(object)this).StartCoroutine(SpawnMonstersRoutine(settings)));
	}

	public UniTask SpawnMonstersAsync(SpawnMonsterSettings settings)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected Obj, but got Unknown
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		UniTaskCompletionSource completionSource = new UniTaskCompletionSource();
		settings.onFinish = (Action)Delegate.Combine(settings.onFinish, (Action)(() =>
		{
			completionSource.TrySetResult();
		}));
		SpawnMonsters(settings);
		return completionSource.Task;
	}

	private IEnumerator SpawnMonstersRoutine(SpawnMonsterSettings s)
	{
		if (ongoingSpawns.ContainsKey(s))
		{
			throw new InvalidOperationException();
		}
		yield return null;
		while (NetworkedManagerBase<ZoneManager>.instance.isInRoomTransition)
		{
			yield return null;
		}
		MonsterSpawnData monsterSpawnData = s.monsterSpawnData;
		MonsterSpawnRule rule = s.rule;
		float waitStartTime = Time.time;
		float timeToWait = s.random.Range(rule.initialDelay.x, rule.initialDelay.y) * s.initDelayMultiplier + s.initDelayFlat;
		yield return new WaitWhile(() => Time.time - waitStartTime < timeToWait && !s.isCutsceneSkipped);
		int waves = s.random.Range(rule.wavesMin, rule.wavesMax + 1);
		waves = NetworkedManagerBase<GameManager>.instance.GetAdjustedMonsterWaves(waves, s.random);
		float hunterChance = addedHunterChance;
		if (rule.isBossSpawn)
		{
			waves = 1;
			hunterChance = 0f;
		}
		int waveIndex = 0;
		while (true)
		{
			float waveStartTime;
			float waveTimeout;
			float nextWaveThreshold;
			if (waveIndex < waves)
			{
				waveStartTime = Time.time;
				waveTimeout = s.random.Range(rule.waveTimeoutMin, rule.waveTimeoutMax);
				float population = s.random.Range(rule.populationPerWave.x, rule.populationPerWave.y);
				population = NetworkedManagerBase<GameManager>.instance.GetAdjustedMonsterSpawnPopulation(population, s.ignoreTurnPopMultiplier, s.ignoreCoopPopMultiplier) * s.spawnPopulationMultiplier * spawnedPopMultiplier;
				nextWaveThreshold = s.random.Range(rule.nextWavePopulationThreshold.x, rule.nextWavePopulationThreshold.y);
				nextWaveThreshold = NetworkedManagerBase<GameManager>.instance.GetAdjustedMonsterSpawnPopulation(nextWaveThreshold, s.ignoreTurnPopMultiplier, s.ignoreCoopPopMultiplier);
				nextWaveThreshold = Mathf.Clamp(nextWaveThreshold, 0.0001f, population - 0.1f);
				IEnumerator<Monster> enumerator = rule.pool.GetMonsters(int.MaxValue, s.random);
				enumerator.MoveNext();
				float spawnedPop = 0f;
				bool isFirstSpawn = true;
				int iter = 0;
				while (true)
				{
					if (isFirstSpawn)
					{
						isFirstSpawn = false;
					}
					else
					{
						float num = s.random.Range(0.1f, 0.5f);
						if (!rule.isBossSpawn)
						{
							num = NetworkedManagerBase<GameManager>.instance.GetAdjustedMonsterSpawnDelay(num);
						}
						yield return new WaitForSeconds(num);
					}
					float popCost = enumerator.Current.populationCost;
					if (!rule.isBossSpawn)
					{
						RefValue<bool> didFail = new RefValue<bool>(v: false);
						yield return WaitForPopulationRoutine(rule, s.section, popCost, didFail);
						if ((bool)didFail)
						{
							break;
						}
					}
					if (s.earlyFinishCondition != null && s.earlyFinishCondition())
					{
						goto end_IL_0638;
					}
					Entity entity = SpawnMonsterImp(s, monsterSpawnData, enumerator.Current, popCost);
					float num2 = s.random.Value();
					spawnedPop += popCost;
					if ((UnityEngine.Object)(object)entity != null && num2 < hunterChance && !entity.Status.HasStatusEffect<Se_HunterBuff>())
					{
						entity.CreateStatusEffect<Se_HunterBuff>(entity, new CastInfo(entity));
					}
					iter++;
					if (!rule.isBossSpawn && !(spawnedPop > population))
					{
						enumerator.MoveNext();
						continue;
					}
					goto IL_05f8;
				}
				if (rule.onOverPopulation == OverpopulationBehavior.Stall)
				{
					Debug.Log(rule.name + " timed out due to overpopulation");
				}
				else
				{
					Debug.Log(rule.name + " canceled due to overpopulation");
				}
			}
			while (monsterSpawnData.remainingPopulation > 0.05f)
			{
				yield return new WaitForSeconds(0.25f);
			}
			break;
			IL_05f8:
			while (monsterSpawnData.remainingPopulation > nextWaveThreshold && Time.time - waveStartTime < waveTimeout)
			{
				yield return new WaitForSeconds(0.25f);
			}
			waveIndex++;
			continue;
			end_IL_0638:
			break;
		}
		s.onFinish?.Invoke();
		ongoingSpawns.Remove(s);
	}

	internal (Vector3, Quaternion) GetSpawnMonsterPosRot(SpawnMonsterSettings s, Entity monster)
	{
		DewRandom random = s.positionRandom ?? s.random;
		Vector3 vector;
		if (monster is Monster { spawnPosOverride: not null, spawnPosOverride: var spawnPosOverride })
		{
			vector = spawnPosOverride.Value;
		}
		else if (s.spawnPosGetter != null)
		{
			vector = s.spawnPosGetter();
		}
		else
		{
			vector = ((!(s.section != null)) ? GetSpawnPositionNearPlayer(s.rule.spawnMinDistance, s.rule.spawnMaxDistance, s.hero) : s.section.monsters.GetSpawnPositionInSection(s.rule.spawnMinDistance, s.rule.spawnMaxDistance, random));
		}
		Quaternion item;
		if (monster is Monster { spawnRotOverride: not null, spawnRotOverride: var spawnRotOverride })
		{
			item = spawnRotOverride.Value;
		}
		else if (s.spawnRotGetter != null)
		{
			item = s.spawnRotGetter();
		}
		else
		{
			Vector3 vector2 = NetworkedManagerBase<ActorManager>.instance.allHeroes[0].position;
			float num = Vector3.Distance(vector, NetworkedManagerBase<ActorManager>.instance.allHeroes[0].position);
			for (int i = 1; i < NetworkedManagerBase<ActorManager>.instance.allHeroes.Count; i++)
			{
				Vector3 position = NetworkedManagerBase<ActorManager>.instance.allHeroes[i].position;
				float num2 = Vector3.Distance(vector, position);
				if (!(num2 >= num))
				{
					num = num2;
					vector2 = position;
				}
			}
			item = Quaternion.LookRotation(vector2 - vector).Flattened();
		}
		return (vector, item);
	}

	private Entity SpawnMonsterImp(SpawnMonsterSettings s, MonsterSpawnData monsterSpawnData, Entity monster, float popCost)
	{
		try
		{
			(Vector3, Quaternion) spawnMonsterPosRot = GetSpawnMonsterPosRot(s, monster);
			Entity entity = Dew.SpawnEntity(monster, spawnMonsterPosRot.Item1, spawnMonsterPosRot.Item2, NetworkedManagerBase<ActorManager>.instance.serverActor, DewPlayer.creep, NetworkedManagerBase<GameManager>.instance.ambientLevel, (Entity e) =>
			{
				if (s.isCutsceneSkipped)
				{
					e.Visual.NetworkskipSpawning = true;
				}
				onBeforeSpawn?.Invoke(e);
				s.beforeSpawn?.Invoke(e);
				monsterSpawnData.remainingPopulation += popCost;
				e.EntityEvent_OnDeath += (Action<EventInfoKill>)((EventInfoKill kill) =>
				{
					monsterSpawnData.lastKiller = kill.actor.firstEntity;
					monsterSpawnData.lastDeathPosition = kill.victim.agentPosition;
				});
				e.ClientActorEvent_OnDestroyed += (Action<Actor>)((Actor _) =>
				{
					monsterSpawnData.remainingPopulation -= popCost;
				});
			});
			onAfterSpawn?.Invoke(entity);
			s.afterSpawn?.Invoke(entity);
			if (s.invalidatePooledInstanceOnUse && (UnityEngine.Object)(object)entity != null)
			{
				entity.InvalidatePoolReuseEverywhere();
			}
			Hero hero = null;
			float num = float.PositiveInfinity;
			foreach (DewPlayer gamePlayer in DewPlayer.gamePlayers)
			{
				if (!((UnityEngine.Object)(object)gamePlayer.hero == null) && !gamePlayer.hero.isKnockedOut)
				{
					float num2 = Vector2.SqrMagnitude(gamePlayer.hero.GetAIPosition(entity).ToXY() - spawnMonsterPosRot.Item1.ToXY());
					if (num2 < num)
					{
						num = num2;
						hero = gamePlayer.hero;
					}
				}
			}
			if ((UnityEngine.Object)(object)hero != null && !EntityAI.DisableAI)
			{
				entity.Control.MoveToDestination(hero.GetAIPosition(entity), immediately: false);
			}
			return entity;
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			return null;
		}
	}

	private Vector3 GetSpawnPositionNearPlayer(float minDist, float maxDist, Hero target = null)
	{
		if ((UnityEngine.Object)(object)target == null)
		{
			target = Dew.SelectRandomAliveHero(fallbackToDead: true, skipStealthed: true);
		}
		for (int i = 0; i < 30; i++)
		{
			Vector3 vector = target.agentPosition + UnityEngine.Random.insideUnitSphere.Flattened().normalized * UnityEngine.Random.Range(minDist, maxDist);
			if (i < 29)
			{
				if (i < 10)
				{
					bool flag = false;
					bool flag2 = true;
					foreach (DewPlayer gamePlayer in DewPlayer.gamePlayers)
					{
						float num = Vector2.Distance(gamePlayer.hero.agentPosition.ToXY(), vector.ToXY());
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
					if (flag | flag2)
					{
						continue;
					}
				}
				if (FilterSpawnPosition(vector, out var filteredPos, target.agentPosition))
				{
					return filteredPos;
				}
				continue;
			}
			Debug.LogWarning("Using fallback spawn position for room '" + SceneManager.GetActiveScene().name + "'");
			FilterSpawnPosition(vector, out var filteredPos2, target.agentPosition);
			return filteredPos2;
		}
		throw new InvalidOperationException("");
	}

	internal static bool FilterSpawnPosition(Vector3 pos, out Vector3 filteredPos, Vector3? pathPivot)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected Obj, but got Unknown
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		if (_tempPath == null)
		{
			_tempPath = new NavMeshPath();
		}
		RaycastHit val = default;
		if (!Physics.Raycast(pos + Vector3.up * 50f, Vector3.down, ref val, 100f, LayerMasks.Ground))
		{
			filteredPos = pos;
			return false;
		}
		NavMeshHit val2 = default;
		if (!NavMesh.SamplePosition(val.point, ref val2, 5f, -1))
		{
			filteredPos = val.point;
			return false;
		}
		if ((pathPivot.HasValue && !NavMesh.CalculatePath(pathPivot.Value, val2.position, -1, _tempPath)) || (int)_tempPath.status != 0)
		{
			filteredPos = val2.position;
			return false;
		}
		filteredPos = val2.position;
		return true;
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_CallOnCreateAsMiniBoss__Entity(Entity e)
	{
		((ISpawnableAsMiniBoss)e).OnCreateAsMiniBoss();
	}

	protected static void InvokeUserCode_CallOnCreateAsMiniBoss__Entity(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC CallOnCreateAsMiniBoss called on server.");
		}
		else
		{
			((RoomMonsters)(object)obj).UserCode_CallOnCreateAsMiniBoss__Entity(NetworkReaderExtensions.ReadNetworkBehaviour<Entity>(reader));
		}
	}

	protected void UserCode_RpcPrewarm__PrewarmEntry_005B_005D__Boolean(PrewarmEntry[] entries, bool clearMonsterPools)
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			if (clearMonsterPools)
			{
				SpawnManager.ClearMonsterAbilityPools();
			}
			ApplyPrewarm(entries);
		}
	}

	protected static void InvokeUserCode_RpcPrewarm__PrewarmEntry_005B_005D__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPrewarm called on server.");
		}
		else
		{
			((RoomMonsters)(object)obj).UserCode_RpcPrewarm__PrewarmEntry_005B_005D__Boolean(GeneratedNetworkCode._Read_RoomMonsters_002FPrewarmEntry_005B_005D(reader), NetworkReaderExtensions.ReadBool(reader));
		}
	}

	protected void UserCode_TpcReplayPrewarm__NetworkConnectionToClient__PrewarmEntry_005B_005D(NetworkConnectionToClient conn, PrewarmEntry[] entries)
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			((MonoBehaviour)(object)this).StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			yield return new WaitWhile(() => (UnityEngine.Object)(object)DewPlayer.local == null || (UnityEngine.Object)(object)DewPlayer.local.hero == null);
			ApplyPrewarm(entries);
		}
	}

	protected static void InvokeUserCode_TpcReplayPrewarm__NetworkConnectionToClient__PrewarmEntry_005B_005D(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TpcReplayPrewarm called on server.");
		}
		else
		{
			((RoomMonsters)(object)obj).UserCode_TpcReplayPrewarm__NetworkConnectionToClient__PrewarmEntry_005B_005D((NetworkConnectionToClient)(object)NetworkClient.connection, GeneratedNetworkCode._Read_RoomMonsters_002FPrewarmEntry_005B_005D(reader));
		}
	}

	static RoomMonsters()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Expected Obj, but got Unknown
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterRpc(typeof(RoomMonsters), "System.Void RoomMonsters::CallOnCreateAsMiniBoss(Entity)", (RemoteCallDelegate)InvokeUserCode_CallOnCreateAsMiniBoss__Entity);
		RemoteProcedureCalls.RegisterRpc(typeof(RoomMonsters), "System.Void RoomMonsters::RpcPrewarm(RoomMonsters/PrewarmEntry[],System.Boolean)", (RemoteCallDelegate)InvokeUserCode_RpcPrewarm__PrewarmEntry_005B_005D__Boolean);
		RemoteProcedureCalls.RegisterRpc(typeof(RoomMonsters), "System.Void RoomMonsters::TpcReplayPrewarm(Mirror.NetworkConnectionToClient,RoomMonsters/PrewarmEntry[])", (RemoteCallDelegate)InvokeUserCode_TpcReplayPrewarm__NetworkConnectionToClient__PrewarmEntry_005B_005D);
	}
}
