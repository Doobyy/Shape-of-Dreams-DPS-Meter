using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using MagicaCloth2;
using Mirror;
using Steamworks;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public static class Dew
{
	public struct BakedMeshHandle
	{
		internal long _returnHandleId;

		private bool _isReturned;

		public void Return()
		{
			if (_isReturned)
			{
				UnityEngine.Debug.LogWarning("Tried to return a resource twice");
				return;
			}
			_isReturned = true;
			long num = default;
			if (!_returnHandleIdToMeshId.Remove(_returnHandleId, ref num) || !_meshIdToEntry.TryGetValue(num, out var value))
			{
				return;
			}
			value.users--;
			if (value.users == 0 && Time.unscaledTime - value.bakeUnscaledTime > BakeMeshInterval)
			{
				_meshIdToEntry.Remove(num);
				if (_skinInstanceIdToNewestMeshId.TryGetValue(value.skinInstanceId, out var value2) && value2 == num)
				{
					_skinInstanceIdToNewestMeshId.Remove(value.skinInstanceId);
				}
				if ((bool)value.mesh)
				{
					ReleaseBakedMesh(value.mesh);
				}
			}
			else
			{
				_meshIdToEntry[num] = value;
			}
		}
	}

	private struct BakedMesh
	{
		public Mesh mesh;

		public float bakeUnscaledTime;

		public int users;

		public int skinInstanceId;
	}

	private enum DestroyPhase
	{
		InitialPoll,
		RpcWait,
		PostRpcPoll,
		CanDestroyCheck,
		RetryWait,
		RetryPoll,
		Finalize
	}

	private class PendingDestroy
	{
		public GameObject go;

		public DestroyPhase phase;

		public float rpcWaitEnd;

		public int retryFrames;

		public bool hasCleanups;

		public bool handleObtained;

		public bool addedToDestroying;

		public List<ICleanup> cleanups;

		public ListReturnHandle<ICleanup> cleanupsHandle;
	}

	private class DewDestroyRunner : MonoBehaviour
	{
		private void Update()
		{
			ProcessPendingDestroys();
		}
	}

	public static readonly WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();

	public static readonly Color CommonColor = new Color(228f / 255f, 240f / 255f, 242f / 255f);

	public static readonly Color RareColor = new Color(52f / 255f, 252f / 255f, 1f);

	public static readonly Color EpicColor = new Color(199f / 255f, 93f / 255f, 247f / 255f);

	public static readonly Color LegendaryColor = new Color(1f, 62f / 255f, 52f / 255f);

	public static readonly Color CharacterColor = new Color(240f / 255f, 175f / 255f, 79f / 255f);

	public static readonly Color IdentityColor = new Color(240f / 255f, 175f / 255f, 79f / 255f);

	public static readonly Color UniqueColor = new Color(1f, 240f / 255f, 74f / 255f);

	public static readonly Color GoldColor = new Color(1f, 197f / 255f, 0f);

	public static readonly Color DreamDustColor = new Color(0.6f, 14f / 15f, 1f);

	public static readonly Color StardustColor = new Color(1f, 0.8f, 225f / 255f);

	public static readonly Color PlatinumCoinColor = new Color(252f / 255f, 252f / 255f, 252f / 255f);

	public static readonly Color HealthColor = new Color(13f / 15f, 59f / 255f, 59f / 255f);

	public static readonly string CommonColorHex = GetHex(CommonColor);

	public static readonly string RareColorHex = GetHex(RareColor);

	public static readonly string EpicColorHex = GetHex(EpicColor);

	public static readonly string LegendaryColorHex = GetHex(LegendaryColor);

	public static readonly string CharacterColorHex = GetHex(CharacterColor);

	public static readonly string IdentityColorHex = GetHex(IdentityColor);

	public static readonly string UniqueColorHex = GetHex(UniqueColor);

	public static readonly List<string> HeroOrder = new List<string> { "Hero_Lacerta", "Hero_Mist", "Hero_Yubar", "Hero_Vesper", "Hero_Aurena", "Hero_Bismuth", "Hero_Nachia", "Hero_Husk", "Hero_Cetus" };

	public static readonly Color DestructionColor = new Color(1f, 52f / 255f, 52f / 255f);

	public static readonly Color LifeColor = new Color(189f / 255f, 1f, 81f / 255f);

	public static readonly Color ImaginationColor = new Color(61f / 255f, 244f / 255f, 1f);

	public static readonly Color FlexibleColor = new Color(1f, 233f / 255f, 150f / 255f);

	private static int _banRoomNodePositionsRoomId;

	private static bool _banRoomNodePositionsValid;

	private static readonly List<Vector2> _banRoomNodePositions = new List<Vector2>(32);

	private const float GroundYCacheGridSize = 0.5f;

	private const float GroundYCacheDefaultYDiffRange = 50f;

	private static readonly Dictionary<long, float> _groundYCache = new Dictionary<long, float>(256);

	private static int _groundYCacheFrame = -1;

	private static NavMeshPath _path = new NavMeshPath();

	private static NavMeshPath _pathStatusPath;

	private static NavMeshQueryFilter _pathStatusFilter;

	private static bool _pathStatusInitialized;

	private static NavMeshPath _navPath;

	private static NavMeshQueryFilter _navFilter;

	private static bool _navPathInitialized;

	private const float PathStatusCacheLifetime = 0.25f;

	private static readonly Dictionary<(int, int), (float time, NavMeshPathStatus status)> _pathStatusCache = new Dictionary<(int, int), (float, NavMeshPathStatus)>();

	private static PointerEventData _uiRaycastPointer;

	private static List<Hero> _aliveHeroesBuffer = new List<Hero>();

	private static readonly string[] BigNumberSuffixes = new string[5] { "", "K", "M", "B", "T" };

	private static readonly long[] RequiredMasteryPointsToLevelUp = new long[31]
	{
		7280L, 7280L, 8190L, 8190L, 9100L, 10500L, 11550L, 11550L, 12600L, 12600L,
		15680L, 15680L, 16800L, 16800L, 17920L, 19040L, 20230L, 20230L, 21420L, 21420L,
		21420L, 21420L, 21420L, 21420L, 21420L, 21420L, 21420L, 21420L, 21420L, 21420L,
		30600L
	};

	private static readonly int[] DejavuCostUnique = new int[3] { 350, 250, 150 };

	private static readonly int[] DejavuCostLegendary = new int[3] { 350, 250, 150 };

	private static readonly int[] DejavuCostEpic = new int[4] { 180, 150, 120, 90 };

	private static readonly int[] DejavuCostRare = new int[5] { 120, 110, 100, 90, 80 };

	private static readonly int[] DejavuCostCommon = new int[6] { 75, 70, 65, 60, 55, 50 };

	private static Dictionary<object, Coroutine> _debounces = new Dictionary<object, Coroutine>();

	private static Coroutiner _coroutiner;

	public static readonly int[] ObliterationSlotPrice = new int[10] { 100, 100, 100, 150, 200, 250, 300, 350, 400, 400 };

	public static float BakeMeshInterval = 0.1f;

	public static float BakeMeshMaintenanceInterval = 1f;

	private static Dictionary<int, long> _skinInstanceIdToNewestMeshId = new Dictionary<int, long>();

	private static Dictionary<long, BakedMesh> _meshIdToEntry = new Dictionary<long, BakedMesh>();

	private static Dictionary<long, long> _returnHandleIdToMeshId = new Dictionary<long, long>();

	private static float _lastBakeMeshMaintenanceUnscaledTime;

	private static long _nextMeshId;

	private static long _nextReturnHandleId;

	private const int kBakedMeshPoolCap = 8;

	private static readonly Stack<Mesh> _bakedMeshPool = new Stack<Mesh>();

	private static ZoneManager _bakedMeshSubscribedZoneManager;

	public const float DestroyDelay = 0.3f;

	internal static HashSet<GameObject> _destroyingGameObject = new HashSet<GameObject>();

	private static readonly List<PendingDestroy> _pendingDestroys = new List<PendingDestroy>();

	private static readonly HashSet<GameObject> _pendingSet = new HashSet<GameObject>();

	private static readonly Stack<PendingDestroy> _pendingDestroyPool = new Stack<PendingDestroy>();

	private static DewDestroyRunner _destroyDriver;

	private static Camera _mainCamera;

	private static Canvas _gameCanvas;

	private static List<Type> _allHeroes;

	private static List<Type> _allMonsters;

	private static List<Type> _allSkills;

	private static List<Type> _allHeroSkills;

	private static List<Type> _allGems;

	private static List<Type> _allArtifacts;

	private static List<Type> _allGameModifiers;

	private static List<Type> _allRoomModifiers;

	private static List<Type> _allAchievements;

	private static List<Type> _allTutorialItems;

	private static List<Type> _allLucidDreams;

	private static List<DewReverieItem> _allReveries;

	private static Dictionary<string, DewReverieItem> _reveriesByName;

	private static Dictionary<string, Type> _achievementsByName;

	private static List<DewStarItemOld> _allOldStars;

	private static List<Type> _allStarTypes;

	private static Dictionary<DewStarItemOld, Type> _heroTypeOfOldStar;

	private static List<(Type, DewResourceLinkAttribute)> _resourceLinkTypes;

	private static Dictionary<string, List<string>> _storyNodeIdsByHero;

	public static Camera mainCamera
	{
		get
		{
			if (_mainCamera == null)
			{
				_mainCamera = Camera.main;
			}
			return _mainCamera;
		}
	}

	public static Canvas gameCanvas
	{
		get
		{
			if ((UnityEngine.Object)(object)_gameCanvas == null)
			{
				_gameCanvas = GameObject.FindGameObjectWithTag("GameCanvas").GetComponent<Canvas>();
			}
			return _gameCanvas;
		}
	}

	public static IReadOnlyList<Type> allHeroes
	{
		get
		{
			if (_allHeroes == null)
			{
				InitAllTypeReferences();
			}
			return _allHeroes;
		}
	}

	public static IReadOnlyList<Type> allMonsters
	{
		get
		{
			if (_allMonsters == null)
			{
				InitAllTypeReferences();
			}
			return _allMonsters;
		}
	}

	public static IReadOnlyList<Type> allSkills
	{
		get
		{
			if (_allSkills == null)
			{
				InitAllTypeReferences();
			}
			return _allSkills;
		}
	}

	public static IReadOnlyList<Type> allHeroSkills
	{
		get
		{
			if (_allHeroSkills == null)
			{
				InitAllTypeReferences();
			}
			return _allHeroSkills;
		}
	}

	public static IReadOnlyList<Type> allGems
	{
		get
		{
			if (_allGems == null)
			{
				InitAllTypeReferences();
			}
			return _allGems;
		}
	}

	public static IReadOnlyList<Type> allArtifacts
	{
		get
		{
			if (_allArtifacts == null)
			{
				InitAllTypeReferences();
			}
			return _allArtifacts;
		}
	}

	public static IReadOnlyList<Type> allGameModifiers
	{
		get
		{
			if (_allGameModifiers == null)
			{
				InitAllTypeReferences();
			}
			return _allGameModifiers;
		}
	}

	public static IReadOnlyList<Type> allRoomModifiers
	{
		get
		{
			if (_allRoomModifiers == null)
			{
				InitAllTypeReferences();
			}
			return _allRoomModifiers;
		}
	}

	public static IReadOnlyList<Type> allAchievements
	{
		get
		{
			if (_allAchievements == null)
			{
				InitAllTypeReferences();
			}
			return _allAchievements;
		}
	}

	public static IReadOnlyList<Type> allTutorialItems
	{
		get
		{
			if (_allTutorialItems == null)
			{
				InitAllTypeReferences();
			}
			return _allTutorialItems;
		}
	}

	public static IReadOnlyList<Type> allLucidDreams
	{
		get
		{
			if (_allLucidDreams == null)
			{
				InitAllTypeReferences();
			}
			return _allLucidDreams;
		}
	}

	public static IReadOnlyList<DewReverieItem> allReveries
	{
		get
		{
			if (_allReveries == null)
			{
				InitAllTypeReferences();
			}
			return _allReveries;
		}
	}

	public static IReadOnlyDictionary<string, DewReverieItem> reveriesByName
	{
		get
		{
			if (_reveriesByName == null)
			{
				InitAllTypeReferences();
			}
			return _reveriesByName;
		}
	}

	public static IReadOnlyDictionary<string, Type> achievementsByName
	{
		get
		{
			if (_achievementsByName == null)
			{
				InitAllTypeReferences();
			}
			return _achievementsByName;
		}
	}

	public static IReadOnlyList<DewStarItemOld> allOldStars
	{
		get
		{
			if (_allOldStars == null)
			{
				InitAllTypeReferences();
			}
			return _allOldStars;
		}
	}

	public static IReadOnlyList<Type> allStarTypes
	{
		get
		{
			if (_allStarTypes == null)
			{
				InitAllTypeReferences();
			}
			return _allStarTypes;
		}
	}

	public static IReadOnlyDictionary<DewStarItemOld, Type> heroTypeOfOldStar
	{
		get
		{
			if (_heroTypeOfOldStar == null)
			{
				InitAllTypeReferences();
			}
			return _heroTypeOfOldStar;
		}
	}

	public static IReadOnlyList<(Type, DewResourceLinkAttribute)> resourceLinkTypes
	{
		get
		{
			if (_resourceLinkTypes == null)
			{
				InitAllTypeReferences();
			}
			return _resourceLinkTypes;
		}
	}

	public static IReadOnlyDictionary<string, List<string>> storyNodeIdsByHero
	{
		get
		{
			if (_storyNodeIdsByHero == null)
			{
				InitAllTypeReferences();
			}
			return _storyNodeIdsByHero;
		}
	}

	public static string GetHex(Color color)
	{
		return $"#{(int)(color.r * 255f):X2}{(int)(color.g * 255f):X2}{(int)(color.b * 255f):X2}";
	}

	public static Color GetRarityColor(Rarity rarity)
	{
		return rarity switch
		{
			Rarity.Common => CommonColor, 
			Rarity.Rare => RareColor, 
			Rarity.Epic => EpicColor, 
			Rarity.Legendary => LegendaryColor, 
			Rarity.Character => CharacterColor, 
			Rarity.Identity => IdentityColor, 
			Rarity.Unique => UniqueColor, 
			_ => throw new ArgumentOutOfRangeException("rarity", rarity, null), 
		};
	}

	public static Color GetRarityColor(SkinRarity rarity)
	{
		return rarity switch
		{
			SkinRarity.Default => CommonColor, 
			SkinRarity.Rare => RareColor, 
			SkinRarity.Epic => EpicColor, 
			SkinRarity.Legendary => LegendaryColor, 
			_ => throw new ArgumentOutOfRangeException("rarity", rarity, null), 
		};
	}

	public static string GetRarityColorHex(SkinRarity rarity)
	{
		return rarity switch
		{
			SkinRarity.Default => CommonColorHex, 
			SkinRarity.Rare => RareColorHex, 
			SkinRarity.Epic => EpicColorHex, 
			SkinRarity.Legendary => LegendaryColorHex, 
			_ => throw new ArgumentOutOfRangeException("rarity", rarity, null), 
		};
	}

	public static Color GetCurrencyColor(CurrencyType type)
	{
		return type switch
		{
			CurrencyType.Gold => GoldColor, 
			CurrencyType.DreamDust => DreamDustColor, 
			CurrencyType.Stardust => StardustColor, 
			CurrencyType.Health => HealthColor, 
			CurrencyType.PlatinumCoin => PlatinumCoinColor, 
			_ => throw new ArgumentOutOfRangeException("type", type, null), 
		};
	}

	public static string GetCurrencyColorHex(CurrencyType type)
	{
		return GetHex(GetCurrencyColor(type));
	}

	public static string GetRarityColorHex(Rarity rarity)
	{
		return rarity switch
		{
			Rarity.Common => CommonColorHex, 
			Rarity.Rare => RareColorHex, 
			Rarity.Epic => EpicColorHex, 
			Rarity.Legendary => LegendaryColorHex, 
			Rarity.Character => CharacterColorHex, 
			Rarity.Identity => IdentityColorHex, 
			Rarity.Unique => UniqueColorHex, 
			_ => throw new ArgumentOutOfRangeException("rarity", rarity, null), 
		};
	}

	public static Color GetStarCategoryColor(StarType type)
	{
		return type switch
		{
			StarType.Life => LifeColor, 
			StarType.Destruction => DestructionColor, 
			StarType.Imagination => ImaginationColor, 
			StarType.Flexible => FlexibleColor, 
			_ => throw new ArgumentOutOfRangeException("type", type, null), 
		};
	}

	public static bool IsNotPlayableOnTop(Vector3 pos)
	{
		pos = GetPositionOnGround(pos, 500f);
		RaycastHit val = default;
		if (Physics.Raycast(pos + Vector3.up * 3f, Vector3.down, ref val, 6f, LayerMasks.Ground) && ((Component)(object)val.collider).GetComponent<INotPlayableOnTop>() != null)
		{
			return true;
		}
		return false;
	}

	public static Vector3 GetGoodRewardPosition(Vector3 pivot, float randomOffset = 2f)
	{
		if (!IsOkay(pivot))
		{
			Vector3 fallback = Vector3.zero;
			if (DewPlayer.gamePlayers.Count > 0)
			{
				Hero closestAliveHero = GetClosestAliveHero(Vector3.zero);
				fallback = ((!closestAliveHero.Control.isDisplacing || !(closestAliveHero.Control.ongoingDisplacement is DispByDestination { canGoOverTerrain: not false } dispByDestination)) ? closestAliveHero.agentPosition : dispByDestination.destination);
			}
			FilterNonOkayValues(ref pivot, fallback);
		}
		List<Vector2> list = new List<Vector2>(GetCachedBanRoomNodePositions());
		foreach (Actor allActor in NetworkedManagerBase<ActorManager>.instance.allActors)
		{
			if (allActor is SkillTrigger skillTrigger)
			{
				if ((UnityEngine.Object)(object)skillTrigger.owner == null)
				{
					list.Add(allActor.position);
				}
			}
			else if (allActor is Gem gem)
			{
				if ((UnityEngine.Object)(object)gem.owner == null)
				{
					list.Add(allActor.position);
				}
			}
			else if (allActor is IInteractable)
			{
				list.Add(allActor.position);
			}
		}
		pivot = GetPositionOnGround(pivot);
		float num = randomOffset;
		Vector3 vector = default;
		for (int i = 0; i < 60; i++)
		{
			vector = pivot + UnityEngine.Random.insideUnitCircle.ToXZ() * num;
			vector = ((i <= 40) ? GetValidAgentDestination_LinearSweep(pivot, vector) : GetValidAgentDestination_Closest(pivot, vector));
			bool flag = false;
			foreach (Vector2 item in list)
			{
				if (Vector2.SqrMagnitude(vector.ToXY() - item) < 5.0625f)
				{
					flag = true;
					break;
				}
			}
			if (!flag && IsNotPlayableOnTop(vector))
			{
				flag = true;
			}
			if (!flag)
			{
				break;
			}
			num += 0.25f;
		}
		Hero closestAliveHero2 = GetClosestAliveHero(vector);
		if (closestAliveHero2.Control.isDisplacing && closestAliveHero2.Control.ongoingDisplacement is DispByDestination { canGoOverTerrain: not false } dispByDestination2)
		{
			vector = dispByDestination2.destination + 2f * UnityEngine.Random.insideUnitCircle.ToXZ();
		}
		return GetValidAgentDestination_Closest(closestAliveHero2.agentPosition, vector);
	}

	private static List<Vector2> GetCachedBanRoomNodePositions()
	{
		ZoneManager instance = NetworkedManagerBase<ZoneManager>.instance;
		Room room = (((UnityEngine.Object)(object)instance != null) ? instance.currentRoom : null);
		int num = (((UnityEngine.Object)(object)room != null) ? ((UnityEngine.Object)(object)room).GetInstanceID() : 0);
		if (!_banRoomNodePositionsValid || num != _banRoomNodePositionsRoomId)
		{
			_banRoomNodePositionsRoomId = num;
			_banRoomNodePositionsValid = (UnityEngine.Object)(object)room != null;
			_banRoomNodePositions.Clear();
			IBanRoomNodesNearby[] array = FindInterfacesOfType<IBanRoomNodesNearby>(includeInactive: true);
			foreach (IBanRoomNodesNearby banRoomNodesNearby in array)
			{
				_banRoomNodePositions.Add(((Component)banRoomNodesNearby).transform.position.ToXY());
			}
		}
		return _banRoomNodePositions;
	}

	public static void PrewarmBanRoomNodeCache()
	{
		_banRoomNodePositionsValid = false;
		GetCachedBanRoomNodePositions();
	}

	public static Vector3 GetPositionOnGround(Vector3 position, float yDiffRange = 50f)
	{
		FilterNonOkayValues(ref position);
		if (Mathf.Approximately(yDiffRange, 50f))
		{
			if (_groundYCacheFrame != Time.frameCount)
			{
				_groundYCacheFrame = Time.frameCount;
				_groundYCache.Clear();
			}
			int num = Mathf.RoundToInt(position.x / 0.5f);
			int num2 = Mathf.RoundToInt(position.z / 0.5f);
			long key = ((long)num << 32) ^ (uint)num2;
			if (_groundYCache.TryGetValue(key, out var value))
			{
				position.y = value;
				FilterNonOkayValues(ref position);
				return position;
			}
			RaycastHit val = default;
			RaycastHit val2 = default;
			if (Physics.Raycast(position + Vector3.up * yDiffRange, Vector3.down, ref val, yDiffRange + 1f, LayerMasks.Ground))
			{
				position.y = val.point.y;
			}
			else if (Physics.SphereCast(position, 0.4f, Vector3.down, ref val2, yDiffRange, LayerMasks.Ground))
			{
				position.y = val2.point.y;
			}
			FilterNonOkayValues(ref position);
			_groundYCache[key] = position.y;
			return position;
		}
		RaycastHit val3 = default;
		RaycastHit val4 = default;
		if (Physics.Raycast(position + Vector3.up * yDiffRange, Vector3.down, ref val3, yDiffRange + 1f, LayerMasks.Ground))
		{
			position.y = val3.point.y;
		}
		else if (Physics.SphereCast(position, 0.4f, Vector3.down, ref val4, yDiffRange, LayerMasks.Ground))
		{
			position.y = val4.point.y;
		}
		FilterNonOkayValues(ref position);
		return position;
	}

	public static Vector3 GetValidAgentPosition(Vector3 pos, float threshold = 10f)
	{
		FilterNonOkayValues(ref pos);
		float num = 0.1f;
		if (num > threshold)
		{
			num = threshold;
		}
		NavMeshHit val = default;
		do
		{
			if (NavMesh.SamplePosition(pos, ref val, num, -1))
			{
				Vector3 v = val.position;
				FilterNonOkayValues(ref v);
				return v;
			}
			num = ((!(num < 0.5f)) ? (num + 0.5f) : (num + 0.1f));
		}
		while (!(num > threshold));
		if (Application.isPlaying)
		{
			UnityEngine.Debug.LogWarning("GetValidAgentPosition position is not a valid point on NavMesh!");
		}
		return pos;
	}

	public static Vector3 GetValidAgentDestination_LinearSweep(Vector3 start, Vector3 end)
	{
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		FilterNonOkayValues(ref start);
		FilterNonOkayValues(ref end, start);
		float num = 0.5f;
		NavMeshHit val = default;
		Vector3 vector;
		float num2;
		while (true)
		{
			if (NavMesh.SamplePosition(start, ref val, num, -1))
			{
				start = val.position;
				vector = end;
				num2 = 0.25f;
				if (Vector3.Distance(start, end) / num2 > 10f)
				{
					num2 = Vector3.Distance(start, end) / 10f;
				}
				break;
			}
			num += 0.5f;
			if (num > 10f)
			{
				UnityEngine.Debug.LogWarning("GetValidAgentDestination_LinearSweep start position is not a valid point on NavMesh!");
				return start;
			}
		}
		int num3 = 0;
		NavMeshHit val2 = default;
		do
		{
			num3++;
			if (num3 >= 50)
			{
				break;
			}
			if (NavMesh.SamplePosition(vector, ref val2, 1f, -1) && NavMesh.CalculatePath(start, vector, -1, _path) && (int)_path.status == 0)
			{
				Vector3 v = val2.position;
				FilterNonOkayValues(ref v, start);
				return v;
			}
			vector = GetPositionOnGround(Vector3.MoveTowards(vector, start, num2));
		}
		while (Vector2.SqrMagnitude(vector.ToXY() - start.ToXY()) > 0.2f);
		return start;
	}

	public static Vector3 GetValidAgentDestination_Closest(Vector3 start, Vector3 end)
	{
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Invalid comparison between Unknown and I4
		FilterNonOkayValues(ref start);
		FilterNonOkayValues(ref end, start);
		float num = 0.15f;
		NavMeshHit val = default;
		Vector3 vector;
		float num2;
		float num3;
		int num4;
		while (true)
		{
			if (NavMesh.SamplePosition(start, ref val, num, -1))
			{
				start = val.position;
				vector = end;
				num2 = 1f;
				num3 = Vector3.Distance(start, end);
				num4 = 0;
				break;
			}
			num += 0.5f;
			if (num > 10f)
			{
				UnityEngine.Debug.LogWarning("GetValidAgentDestination_LinearSweep start position is not a valid point on NavMesh!");
				return start;
			}
		}
		NavMeshHit val2 = default;
		while (true)
		{
			num4++;
			if (num4 >= 10 || num2 > 10f || num2 > num3)
			{
				break;
			}
			if (NavMesh.SamplePosition(vector, ref val2, num2, -1) && NavMesh.CalculatePath(start, val2.position, -1, _path))
			{
				if ((int)_path.status == 0)
				{
					Vector3 v = val2.position;
					FilterNonOkayValues(ref v, start);
					return val2.position;
				}
				if ((int)_path.status == 1)
				{
					Vector3 v2 = _path.corners[_path.corners.Length - 1];
					FilterNonOkayValues(ref v2, start);
					return v2;
				}
			}
			num2 *= 1.5f;
		}
		return GetValidAgentDestination_LinearSweep(start, end);
	}

	public static NavMeshPathStatus GetNavMeshPathStatusCached(Entity from, Entity to, Vector3 fromPos, Vector3 toPos)
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		(int, int) key = (((UnityEngine.Object)(object)from).GetInstanceID(), ((UnityEngine.Object)(object)to).GetInstanceID());
		if (_pathStatusCache.TryGetValue(key, out (float, NavMeshPathStatus) value) && Time.time - value.Item1 < 0.25f)
		{
			return value.Item2;
		}
		if (_pathStatusCache.Count > 2048)
		{
			_pathStatusCache.Clear();
		}
		NavMeshPathStatus navMeshPathStatus = GetNavMeshPathStatus(fromPos, toPos);
		_pathStatusCache[key] = (Time.time, navMeshPathStatus);
		return navMeshPathStatus;
	}

	public static NavMeshPathStatus GetNavMeshPathStatus(Vector3 start, Vector3 end)
	{
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected Obj, but got Unknown
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		FilterNonOkayValues(ref start);
		FilterNonOkayValues(ref end, start);
		start = GetValidAgentPosition(start);
		end = GetValidAgentPosition(end);
		if (!_pathStatusInitialized)
		{
			_pathStatusPath = new NavMeshPath();
			NavMeshQueryFilter pathStatusFilter = default;
			pathStatusFilter.agentTypeID = 0;
			pathStatusFilter.areaMask = -1;
			_pathStatusFilter = pathStatusFilter;
			_pathStatusInitialized = true;
		}
		NavMesh.CalculatePath(start, end, _pathStatusFilter, _pathStatusPath);
		return _pathStatusPath.status;
	}

	public static NavMeshPath GetNavMeshPath(Vector3 start, Vector3 end)
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Invalid comparison between Unknown and I4
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Expected Obj, but got Unknown
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		FilterNonOkayValues(ref start);
		FilterNonOkayValues(ref end, start);
		if (!_navPathInitialized)
		{
			_navPath = new NavMeshPath();
			NavMeshQueryFilter navFilter = default;
			navFilter.agentTypeID = 0;
			navFilter.areaMask = -1;
			_navFilter = navFilter;
			_navPathInitialized = true;
		}
		NavMesh.CalculatePath(start, end, _navFilter, _navPath);
		NavMeshHit val = default;
		if ((int)_navPath.status == 2 && NavMesh.Raycast(start, end, ref val, _navFilter))
		{
			NavMesh.CalculatePath(start, val.position, _navFilter, _navPath);
		}
		return _navPath;
	}

	public static T GetUIComponentBelowCursor<T>(bool includeParent = true) where T : Component
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		ListReturnHandle<RaycastResult> handle;
		foreach (RaycastResult item in RaycastAllUIElementsBelowCursor(out handle))
		{
			RaycastResult current = item;
			T val = (includeParent ? current.gameObject.GetComponentInParent<T>() : current.gameObject.GetComponent<T>());
			if (val != null)
			{
				handle.Return();
				return val;
			}
		}
		handle.Return();
		return null;
	}

	public static List<RaycastResult> RaycastAllUIElementsBelowCursor(out ListReturnHandle<RaycastResult> handle)
	{
		List<RaycastResult> list = DewPool.GetList(out handle);
		RaycastAllUIElementsBelowCursor(list);
		return list;
	}

	public static void RaycastAllUIElementsBelowCursor(List<RaycastResult> results)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Expected Obj, but got Unknown
		EventSystem current = EventSystem.current;
		if (_uiRaycastPointer == null)
		{
			_uiRaycastPointer = new PointerEventData(current)
			{
				pointerId = -1
			};
		}
		_uiRaycastPointer.position = Input.mousePosition;
		results.Clear();
		current.RaycastAll(_uiRaycastPointer, results);
	}

	public static List<RaycastResult> RaycastAllUIElementsBelowScreenPoint(Vector2 screenPoint, out ListReturnHandle<RaycastResult> handle)
	{
		List<RaycastResult> list = DewPool.GetList(out handle);
		RaycastAllUIElementsBelowScreenPoint(screenPoint, list);
		return list;
	}

	public static void RaycastAllUIElementsBelowScreenPoint(Vector2 screenPoint, List<RaycastResult> results)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Expected Obj, but got Unknown
		EventSystem current = EventSystem.current;
		if (_uiRaycastPointer == null)
		{
			_uiRaycastPointer = new PointerEventData(current)
			{
				pointerId = -1
			};
		}
		_uiRaycastPointer.position = screenPoint;
		results.Clear();
		current.RaycastAll(_uiRaycastPointer, results);
	}

	public static Type GetRequiredAchievementOfTarget(Type targetType)
	{
		foreach (Type allAchievement in allAchievements)
		{
			object[] customAttributes = allAchievement.GetCustomAttributes(typeof(AchUnlockOnComplete), inherit: false);
			for (int i = 0; i < customAttributes.Length; i++)
			{
				if (((AchUnlockOnComplete)customAttributes[i]).targetType == targetType)
				{
					return allAchievement;
				}
			}
		}
		return null;
	}

	public static Type GetRequiredAchievementOfTarget(string targetTypeName)
	{
		foreach (Type allAchievement in allAchievements)
		{
			object[] customAttributes = allAchievement.GetCustomAttributes(typeof(AchUnlockOnComplete), inherit: false);
			for (int i = 0; i < customAttributes.Length; i++)
			{
				if (((AchUnlockOnComplete)customAttributes[i]).targetType.Name == targetTypeName)
				{
					return allAchievement;
				}
			}
		}
		return null;
	}

	public static List<Type> GetUnlockedTargetsOfAchievement(Type achievementType)
	{
		List<Type> list = new List<Type>();
		object[] customAttributes = achievementType.GetCustomAttributes(typeof(AchUnlockOnComplete), inherit: false);
		for (int i = 0; i < customAttributes.Length; i++)
		{
			AchUnlockOnComplete achUnlockOnComplete = (AchUnlockOnComplete)customAttributes[i];
			list.Add(achUnlockOnComplete.targetType);
		}
		return list;
	}

	public static Color GetAchievementColor(Type achievementType)
	{
		List<Type> unlockedTargetsOfAchievement = GetUnlockedTargetsOfAchievement(achievementType);
		if (unlockedTargetsOfAchievement.Count > 0)
		{
			if (unlockedTargetsOfAchievement[0].IsSubclassOf(typeof(Gem)))
			{
				return GetRarityColor(((Gem)(object)DewResources.GetByType(unlockedTargetsOfAchievement[0])).rarity);
			}
			if (unlockedTargetsOfAchievement[0].IsSubclassOf(typeof(SkillTrigger)))
			{
				return GetRarityColor(((SkillTrigger)(object)DewResources.GetByType(unlockedTargetsOfAchievement[0])).rarity);
			}
			if (unlockedTargetsOfAchievement[0].IsSubclassOf(typeof(LucidDream)))
			{
				return ((LucidDream)(object)DewResources.GetByType(unlockedTargetsOfAchievement[0])).color;
			}
			if (unlockedTargetsOfAchievement[0].IsSubclassOf(typeof(Hero)))
			{
				return ((Hero)(object)DewResources.GetByType(unlockedTargetsOfAchievement[0])).mainColor.WithV(1f);
			}
		}
		return CommonColor;
	}

	public static string GetAchievementColorHex(Type achievementType)
	{
		return GetHex(GetAchievementColor(achievementType));
	}

	public static Hero SelectRandomAliveHero(bool fallbackToDead = true, bool skipStealthed = false)
	{
		_aliveHeroesBuffer.Clear();
		foreach (DewPlayer gamePlayer in DewPlayer.gamePlayers)
		{
			if (!gamePlayer.hero.IsNullInactiveDeadOrKnockedOut() && (!skipStealthed || !gamePlayer.hero.Status.isUndetectableByNonAllies))
			{
				_aliveHeroesBuffer.Add(gamePlayer.hero);
			}
		}
		if ((_aliveHeroesBuffer.Count <= 0) & skipStealthed)
		{
			foreach (DewPlayer gamePlayer2 in DewPlayer.gamePlayers)
			{
				if (!gamePlayer2.hero.IsNullInactiveDeadOrKnockedOut())
				{
					_aliveHeroesBuffer.Add(gamePlayer2.hero);
				}
			}
		}
		if (_aliveHeroesBuffer.Count <= 0)
		{
			if (fallbackToDead && NetworkedManagerBase<ActorManager>.instance.allHeroes.Count > 0)
			{
				return NetworkedManagerBase<ActorManager>.instance.allHeroes.ElementAt(UnityEngine.Random.Range(0, NetworkedManagerBase<ActorManager>.instance.allHeroes.Count));
			}
			return null;
		}
		return _aliveHeroesBuffer[UnityEngine.Random.Range(0, _aliveHeroesBuffer.Count)];
	}

	public static Hero GetClosestAliveHero(Vector3 pivot, bool fallbackToDead = true, Entity prober = null)
	{
		_aliveHeroesBuffer.Clear();
		foreach (Hero allHero in NetworkedManagerBase<ActorManager>.instance.allHeroes)
		{
			if (!allHero.IsNullInactiveDeadOrKnockedOut())
			{
				_aliveHeroesBuffer.Add(allHero);
			}
		}
		if (_aliveHeroesBuffer.Count <= 0 && fallbackToDead && NetworkedManagerBase<ActorManager>.instance.allHeroes.Count > 0)
		{
			_aliveHeroesBuffer.AddRange(NetworkedManagerBase<ActorManager>.instance.allHeroes);
		}
		Hero result = null;
		float num = float.PositiveInfinity;
		foreach (Hero item in _aliveHeroesBuffer)
		{
			float num2 = Vector3.SqrMagnitude((((UnityEngine.Object)(object)prober) ? item.GetAIAgentPosition(prober) : item.agentPosition) - pivot);
			if (num2 < num)
			{
				num = num2;
				result = item;
			}
		}
		return result;
	}

	public static IEnumerator WaitForClientsReadyRoutine(List<DewPlayer> players = null)
	{
		if (!NetworkServer.active)
		{
			yield break;
		}
		if (players == null)
		{
			players = DewPlayer.allHumanPlayers;
		}
		bool didSetStatus = false;
		float startTime = Time.unscaledTime;
		while (true)
		{
			yield return new WaitForSecondsRealtime(0.15f);
			if (players.All((DewPlayer c) => ((NetworkConnection)((NetworkBehaviour)c).connectionToClient).isReady))
			{
				ManagerBase<TransitionManager>.instance.UpdateLoadingStatus(LoadingStatus.Empty);
				yield break;
			}
			if (Time.unscaledTime - startTime > 35f)
			{
				break;
			}
			if (!didSetStatus)
			{
				ManagerBase<TransitionManager>.instance.UpdateLoadingStatus(LoadingStatus.WaitingForOtherPlayers);
				didSetStatus = true;
			}
		}
		UnityEngine.Debug.LogWarning("WARNING: Clients ready check timed out.");
		ManagerBase<TransitionManager>.instance.UpdateLoadingStatus(LoadingStatus.Empty);
		foreach (DewPlayer player in players)
		{
			UnityEngine.Debug.LogWarning(((UnityEngine.Object)(object)player).name + ": " + (((NetworkConnection)((NetworkBehaviour)player).connectionToClient).isReady ? "Ready" : "Not Ready"));
		}
	}

	public static IEnumerator WaitForAggroedEnemiesRoutine()
	{
		bool flag = false;
		foreach (DewPlayer h in DewPlayer.gamePlayers)
		{
			if (!h.hero.isKnockedOut)
			{
				int count = DewPhysics.OverlapCircleAllEntities(out var handle, h.hero.agentPosition, 8f, (Entity entity) => entity.GetRelation(h.hero) == EntityRelation.Enemy).Count;
				handle.Return();
				if (count > 0)
				{
					flag = true;
					break;
				}
			}
		}
		if (flag)
		{
			yield return new WaitForSeconds(1.5f);
		}
		bool isClear = false;
		while (!isClear)
		{
			isClear = true;
			if (NetworkedManagerBase<ZoneManager>.instance.isInRoomTransition)
			{
				break;
			}
			foreach (Entity allEntity in NetworkedManagerBase<ActorManager>.instance.allEntities)
			{
				if (allEntity is Monster monster && !monster.IsNullInactiveDeadOrKnockedOut() && (!monster.isSleeping || monster.IsAnyBoss()) && (UnityEngine.Object)(object)monster.AI._aiContext.targetEnemy != null)
				{
					isClear = false;
					break;
				}
				if (allEntity is BossMonster e && !e.IsNullInactiveDeadOrKnockedOut())
				{
					isClear = false;
					break;
				}
			}
			yield return new WaitForSeconds(0.5f);
		}
	}

	public static T SelectRandomWeightedInList<T>(IList<T> list, Func<T, float> weightGetter, DewRandom random = null)
	{
		if (list.Count <= 0)
		{
			return default;
		}
		if (random == null)
		{
			random = DewRandom.instance;
		}
		float num = 0f;
		float[] array = DewPool.GetArray(out ArrayReturnHandle<float> handle, list.Count);
		for (int i = 0; i < list.Count; i++)
		{
			float num2 = weightGetter(list[i]);
			num += num2;
			array[i] = num2;
		}
		float num3 = 0f;
		float num4 = random.Range(0f, num);
		int index = 0;
		for (int j = 0; j < list.Count; j++)
		{
			num3 += array[j];
			if (num4 < num3)
			{
				index = j;
				break;
			}
		}
		handle.Return();
		return list[index];
	}

	public unsafe static T SelectRandomWeightedInList<T>(ReadOnlySpan<T> list, Func<T, float> weightGetter)
	{
		if (list.Length <= 0)
		{
			return default;
		}
		float num = 0f;
		float[] array = DewPool.GetArray(out ArrayReturnHandle<float> handle, list.Length);
		for (int i = 0; i < list.Length; i++)
		{
			float num2 = weightGetter(System.Runtime.CompilerServices.Unsafe.Read<T>((void*)list[i]));
			num += num2;
			array[i] = num2;
		}
		float num3 = 0f;
		float num4 = UnityEngine.Random.Range(0f, num);
		int num5 = 0;
		for (int j = 0; j < list.Length; j++)
		{
			num3 += array[j];
			if (num4 < num3)
			{
				num5 = j;
				break;
			}
		}
		handle.Return();
		return System.Runtime.CompilerServices.Unsafe.Read<T>((void*)list[num5]);
	}

	public static T SelectRandomWeightedInReadOnlyList<T>(IReadOnlyList<T> list, Func<T, float> weightGetter)
	{
		if (list.Count <= 0)
		{
			return default;
		}
		float num = 0f;
		float[] array = DewPool.GetArray(out ArrayReturnHandle<float> handle, list.Count);
		for (int i = 0; i < list.Count; i++)
		{
			float num2 = weightGetter(list[i]);
			num += num2;
			array[i] = num2;
		}
		float num3 = 0f;
		float num4 = UnityEngine.Random.Range(0f, num);
		int index = 0;
		for (int j = 0; j < list.Count; j++)
		{
			num3 += array[j];
			if (num4 < num3)
			{
				index = j;
				break;
			}
		}
		handle.Return();
		return list[index];
	}

	public static void QuitApplication(bool dontSave = false)
	{
		if (!dontSave)
		{
			try
			{
				DewSave.SaveProfileAll(immediate: true);
				DewSave.SavePlatformSettings();
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}
		Application.Quit();
	}

	public static string GetCurrentMultiplayerCompatibilityVersion()
	{
		return GetMultiplayerCompatibilityVersion(Application.version);
	}

	public static string GetMultiplayerCompatibilityVersion(string bundleVersion)
	{
		if (bundleVersion.Contains("_"))
		{
			return bundleVersion.Split("_", StringSplitOptions.None)[0];
		}
		return bundleVersion;
	}

	public static IControlPresetWindow GetControlPresetWindow()
	{
		return FindInterfaceOfType<IControlPresetWindow>(includeInactive: true);
	}

	public static T FindInterfaceOfType<T>(bool includeInactive) where T : class
	{
		Transform[] array = UnityEngine.Object.FindObjectsOfType<Transform>(includeInactive);
		for (int i = 0; i < array.Length; i++)
		{
			T component = array[i].GetComponent<T>();
			if (component != null)
			{
				return component;
			}
		}
		return null;
	}

	public static T[] FindInterfacesOfType<T>(bool includeInactive) where T : class
	{
		List<T> list = new List<T>();
		Transform[] array = UnityEngine.Object.FindObjectsOfType<Transform>(includeInactive);
		for (int i = 0; i < array.Length; i++)
		{
			T[] components = array[i].GetComponents<T>();
			if (components != null)
			{
				list.AddRange(components);
			}
		}
		return list.ToArray();
	}

	public static int SelectBestIndexWithScore<T>(IList<T> list, Func<T, int, float> scoreFunc, float fuzziness = 0f, DewRandom random = null)
	{
		if (random == null)
		{
			random = DewRandom.instance;
		}
		int result = 0;
		float num = float.NegativeInfinity;
		for (int i = 0; i < list.Count; i++)
		{
			T arg = list[i];
			float num2 = scoreFunc(arg, i);
			num2 *= 1f + random.Range(0f - fuzziness, fuzziness);
			if (num2 > num)
			{
				num = num2;
				result = i;
			}
		}
		return result;
	}

	public static T SelectBestWithScore<T>(HashSet<T> list, Func<T, float> scoreFunc, float fuzziness = 0f, DewRandom random = null)
	{
		if (random == null)
		{
			random = DewRandom.instance;
		}
		T result = default;
		float num = float.NegativeInfinity;
		foreach (T item in list)
		{
			float num2 = scoreFunc(item);
			num2 *= 1f + random.Range(0f - fuzziness, fuzziness);
			if (num2 > num)
			{
				num = num2;
				result = item;
			}
		}
		return result;
	}

	public unsafe static int SelectBestIndexWithScore<T>(ReadOnlySpan<T> list, Func<T, int, float> scoreFunc, float fuzziness = 0f, DewRandom random = null)
	{
		if (random == null)
		{
			random = DewRandom.instance;
		}
		int result = -1;
		float num = float.NegativeInfinity;
		for (int i = 0; i < list.Length; i++)
		{
			T arg = System.Runtime.CompilerServices.Unsafe.Read<T>((void*)list[i]);
			float num2 = scoreFunc(arg, i);
			num2 *= 1f + random.Range(0f - fuzziness, fuzziness);
			if (num2 > num)
			{
				num = num2;
				result = i;
			}
		}
		return result;
	}

	public static T SelectBestWithScore<T>(IList<T> list, Func<T, int, float> scoreFunc, float fuzziness = 0f, DewRandom random = null)
	{
		if (list.Count == 0)
		{
			return default;
		}
		return list[SelectBestIndexWithScore(list, scoreFunc, fuzziness, random)];
	}

	public unsafe static T SelectBestWithScore<T>(ReadOnlySpan<T> list, Func<T, int, float> scoreFunc, float fuzziness = 0f, DewRandom random = null)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		if (list.Length == 0)
		{
			return default;
		}
		return System.Runtime.CompilerServices.Unsafe.Read<T>((void*)list[SelectBestIndexWithScore(list, scoreFunc, fuzziness, random)]);
	}

	public static T SelectRandomWeightedInParams<T>(params (float, T)[] elements)
	{
		if (elements.Length == 0)
		{
			return default;
		}
		float num = 0f;
		for (int i = 0; i < elements.Length; i++)
		{
			num += elements[i].Item1;
		}
		float num2 = 0f;
		float num3 = UnityEngine.Random.Range(0f, num);
		int num4 = 0;
		for (int j = 0; j < elements.Length; j++)
		{
			num2 += elements[j].Item1;
			if (num3 < num2)
			{
				num4 = j;
				break;
			}
		}
		return elements[num4].Item2;
	}

	public static int SelectRandomWeightedIndexInParams(params float[] weights)
	{
		if (weights.Length == 0)
		{
			return -1;
		}
		float num = 0f;
		for (int i = 0; i < weights.Length; i++)
		{
			num += weights[i];
		}
		float num2 = 0f;
		float num3 = UnityEngine.Random.Range(0f, num);
		int result = 0;
		for (int j = 0; j < weights.Length; j++)
		{
			num2 += weights[j];
			if (num3 < num2)
			{
				result = j;
				break;
			}
		}
		return result;
	}

	public static float GetClosestHeroDistance(Vector3 pivot, bool fallbackToDead = true)
	{
		float num = float.PositiveInfinity;
		float num2 = float.PositiveInfinity;
		foreach (DewPlayer gamePlayer in DewPlayer.gamePlayers)
		{
			if (!((UnityEngine.Object)(object)gamePlayer.hero == null))
			{
				float b = Vector2.Distance(gamePlayer.hero.agentPosition.ToXY(), pivot.ToXY());
				num2 = Mathf.Min(num2, b);
				if (!gamePlayer.hero.IsNullInactiveDeadOrKnockedOut())
				{
					num = Mathf.Min(num, b);
				}
			}
		}
		if (fallbackToDead && float.IsPositiveInfinity(num))
		{
			return num2;
		}
		return num;
	}

	public static string GetParentDirectory(string path)
	{
		int num = path.LastIndexOf("/");
		if (num <= -1)
		{
			return "";
		}
		return path.Substring(0, num);
	}

	public static T FindActorOfType<T>() where T : Actor
	{
		if ((UnityEngine.Object)(object)NetworkedManagerBase<ActorManager>.instance == null)
		{
			return null;
		}
		foreach (Actor allActor in NetworkedManagerBase<ActorManager>.instance.allActors)
		{
			if (allActor is T result)
			{
				return result;
			}
		}
		return null;
	}

	public static List<T> FindAllActorsOfType<T>(out ListReturnHandle<T> handle) where T : Actor
	{
		List<T> list = DewPool.GetList(out handle);
		if ((UnityEngine.Object)(object)NetworkedManagerBase<ActorManager>.instance == null)
		{
			return list;
		}
		foreach (Actor allActor in NetworkedManagerBase<ActorManager>.instance.allActors)
		{
			if (allActor is T item)
			{
				list.Add(item);
			}
		}
		return list;
	}

	public static string IntToRoman(int num)
	{
		string text = string.Empty;
		string[] array = new string[13]
		{
			"M", "CM", "D", "CD", "C", "XC", "L", "XL", "X", "IX",
			"V", "IV", "I"
		};
		int[] array2 = new int[13]
		{
			1000, 900, 500, 400, 100, 90, 50, 40, 10, 9,
			5, 4, 1
		};
		int num2 = 0;
		while (num != 0)
		{
			if (num >= array2[num2])
			{
				num -= array2[num2];
				text += array[num2];
			}
			else
			{
				num2++;
			}
		}
		return text;
	}

	public static Type GetTypeFromShortName(string typeName)
	{
		if (DewResources.database.typeNameToType.TryGetValue(typeName, out var value))
		{
			return value;
		}
		Type type = Type.GetType(typeName);
		if (type != null)
		{
			return type;
		}
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		for (int i = 0; i < assemblies.Length; i++)
		{
			type = assemblies[i].GetType(typeName);
			if (type != null)
			{
				return type;
			}
		}
		return null;
	}

	public static bool TryGetTypeFromShortName(string typeName, out Type type)
	{
		type = GetTypeFromShortName(typeName);
		return type != null;
	}

	public static int GetAliveHeroCount()
	{
		int num = 0;
		foreach (DewPlayer gamePlayer in DewPlayer.gamePlayers)
		{
			if (!gamePlayer.hero.IsNullInactiveDeadOrKnockedOut())
			{
				num++;
			}
		}
		return num;
	}

	public static void RefreshRenderer()
	{
		ManagerBase<ShaderManager>.instance.StartCoroutine(Routine());
		static IEnumerator Routine()
		{
			QualitySettings.SetQualityLevel(0, applyExpensiveChanges: true);
			yield return null;
			QualitySettings.SetQualityLevel(3, applyExpensiveChanges: true);
			yield return null;
		}
	}

	public static string ConvertToStringInvariantCulture(object value)
	{
		if (value is float num)
		{
			return num.ToString(CultureInfo.InvariantCulture);
		}
		if (value is double num2)
		{
			return num2.ToString(CultureInfo.InvariantCulture);
		}
		if (value is int num3)
		{
			return num3.ToString(CultureInfo.InvariantCulture);
		}
		return value.ToString();
	}

	public static void CallOnReady(MonoBehaviour caller, Func<bool> condition, Action func)
	{
		ManagerBase<GlobalLogicPackage>.instance.StartCoroutine(Routine());
		IEnumerator Routine()
		{
			while (true)
			{
				if (caller == null)
				{
					yield break;
				}
				if (condition())
				{
					break;
				}
				yield return null;
			}
			func();
		}
	}

	public static void CallDelayed(Action func, int frameCount = 1)
	{
		GetCoroutiner().StartCoroutine(Routine());
		IEnumerator Routine()
		{
			for (int i = 0; i < frameCount; i++)
			{
				yield return null;
			}
			func();
		}
	}

	public static void OpenURL(string url, OpenURLSettings settings = null)
	{
		if (settings == null)
		{
			settings = new OpenURLSettings();
		}
		if (!settings.showConfirmation)
		{
			Open();
			return;
		}
		ManagerBase<MessageManager>.instance.ShowMessage(new DewMessageSettings
		{
			rawContent = DewLocalization.GetUIValue("PromotionBanner_ConfirmOpenExternalLink"),
			buttons = (DewMessageSettings.ButtonType.Yes | DewMessageSettings.ButtonType.Cancel),
			defaultButton = DewMessageSettings.ButtonType.Cancel,
			onClose = (DewMessageSettings.ButtonType b) =>
			{
				if (b == DewMessageSettings.ButtonType.Yes)
				{
					Open();
				}
			}
		});
		void Open()
		{
			if (settings.allowSteamOverlay && DewSteam.isInitialized)
			{
				SteamFriends.ActivateGameOverlayToWebPage(url, (EActivateGameOverlayToWebPageMode)(settings.isSteamOverlayModal ? 1 : 0));
			}
			else
			{
				Application.OpenURL(url);
			}
		}
	}

	public static string GetProjectRootPath()
	{
		return Directory.GetParent(Application.dataPath).ToString();
	}

	public static bool IsMeleeHero(Hero.HeroClassType type)
	{
		if (type != Hero.HeroClassType.MeleeAttacker && type != Hero.HeroClassType.MeleeMage && type != Hero.HeroClassType.MeleeTank && type != Hero.HeroClassType.MeleeSupport)
		{
			return type == Hero.HeroClassType.MeleeSummoner;
		}
		return true;
	}

	public static bool IsRangedHero(Hero.HeroClassType type)
	{
		return !IsMeleeHero(type);
	}

	public static string FormatBigNumbers(float number, float threshold, string format)
	{
		if (number < 1000f)
		{
			return number.ToString(format);
		}
		int num = 0;
		while (number >= threshold && num < BigNumberSuffixes.Length - 1)
		{
			number /= 1000f;
			num++;
		}
		return number.ToString(format) + BigNumberSuffixes[num];
	}

	public static int FormatBigNumbers(char[] buffer, float number, float threshold, string format)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		while (number >= threshold && num < BigNumberSuffixes.Length - 1)
		{
			number /= 1000f;
			num++;
		}
		int num2 = default;
		number.TryFormat(Span<char>.op_Implicit(buffer), ref num2, string.op_Implicit(format), (IFormatProvider)CultureInfo.CurrentCulture);
		string text = BigNumberSuffixes[num];
		for (int i = 0; i < text.Length; i++)
		{
			if (num2 >= buffer.Length)
			{
				break;
			}
			buffer[num2++] = text[i];
		}
		return num2;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsOkay(float f)
	{
		if (float.IsNaN(f) || float.IsInfinity(f))
		{
			UnityEngine.Debug.LogException(new InvalidOperationException("Non-okay floating value found"));
			return false;
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsOkay(double d)
	{
		if (double.IsNaN(d) || double.IsInfinity(d))
		{
			UnityEngine.Debug.LogException(new InvalidOperationException("Non-okay floating value found"));
			return false;
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsOkay(Vector2 v)
	{
		if (IsOkay(v.x))
		{
			return IsOkay(v.y);
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsOkay(Vector3 v)
	{
		if (IsOkay(v.x) && IsOkay(v.y))
		{
			return IsOkay(v.z);
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsOkay(Vector4 v)
	{
		if (IsOkay(v.x) && IsOkay(v.y) && IsOkay(v.z))
		{
			return IsOkay(v.w);
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsOkay(Quaternion q)
	{
		if (IsOkay(q.x) && IsOkay(q.y) && IsOkay(q.z))
		{
			return IsOkay(q.w);
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsOkay(Displacement d)
	{
		if (d is DispByDestination dispByDestination)
		{
			if (IsOkay(dispByDestination.duration) && IsOkay(dispByDestination.destination))
			{
				return dispByDestination.duration >= 0f;
			}
			return false;
		}
		if (d is DispByTarget dispByTarget)
		{
			if (IsOkay(dispByTarget.goalDistance))
			{
				return IsOkay(dispByTarget.speed);
			}
			return false;
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void FilterNonOkayValues(ref Vector3 v, Vector3 fallback = default(Vector3))
	{
		FilterNonOkayValues(ref v.x, fallback.x);
		FilterNonOkayValues(ref v.y, fallback.y);
		FilterNonOkayValues(ref v.z, fallback.z);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void FilterNonOkayValues(ref Vector2 v, Vector2 fallback = default(Vector2))
	{
		FilterNonOkayValues(ref v.x, fallback.x);
		FilterNonOkayValues(ref v.y, fallback.y);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void FilterNonOkayValues(ref float v, float fallback = 0f)
	{
		if (!IsOkay(v))
		{
			v = (IsOkay(fallback) ? fallback : 0f);
		}
	}

	public static bool IsNearChristmas()
	{
		DateTime today = DateTime.Today;
		DateTime dateTime = new DateTime(today.Year, 12, 25);
		if (today.Month == 1 && today.Day <= 7)
		{
			dateTime = new DateTime(today.Year - 1, 12, 25);
		}
		else if (today.Month == 12 && today.Day > 25)
		{
			dateTime = new DateTime(today.Year + 1, 12, 25);
		}
		return Math.Abs((dateTime - today).Days) <= 7;
	}

	public static bool IsChristmas()
	{
		DateTime today = DateTime.Today;
		if (today.Month == 12)
		{
			return today.Day == 25;
		}
		return false;
	}

	public static string GetReadableTimespanConcise(TimeSpan t)
	{
		if (t.Days > 1)
		{
			return string.Format(DewLocalization.GetUIValue("Timespan_Days_Plural"), t.Days);
		}
		if (t.Days == 1)
		{
			return string.Format(DewLocalization.GetUIValue("Timespan_Day_Singular"), t.Days);
		}
		if (t.Hours > 1)
		{
			return string.Format(DewLocalization.GetUIValue("Timespan_Hours_Plural"), t.Hours);
		}
		if (t.Hours == 1)
		{
			return string.Format(DewLocalization.GetUIValue("Timespan_Hour_Singular"), t.Hours);
		}
		if (t.Minutes > 1)
		{
			return string.Format(DewLocalization.GetUIValue("Timespan_Minutes_Plural"), t.Minutes);
		}
		return string.Format(DewLocalization.GetUIValue("Timespan_Minute_Singular"), 1);
	}

	public static string GetReadableTimespanDetailed(TimeSpan t)
	{
		string str = "";
		if (t.Days > 1)
		{
			Append("Timespan_Days_Plural", t.Days);
		}
		if (t.Days == 1)
		{
			Append("Timespan_Day_Singular", t.Days);
		}
		if (t.Hours > 1)
		{
			Append("Timespan_Hours_Plural", t.Hours);
		}
		if (t.Hours == 1)
		{
			Append("Timespan_Hour_Singular", t.Hours);
		}
		if (t.Minutes > 1)
		{
			Append("Timespan_Minutes_Plural", t.Minutes);
		}
		if (t.Minutes == 1)
		{
			Append("Timespan_Minute_Singular", t.Minutes);
		}
		if (str.Length == 0)
		{
			return "-";
		}
		return str;
		void Append(string template, int val)
		{
			if (str.Length > 0)
			{
				str += " ";
			}
			str += string.Format(DewLocalization.GetUIValue(template), val);
		}
	}

	public static long GetRequiredMasteryPointsToLevelUp(int currentLevel)
	{
		return RequiredMasteryPointsToLevelUp.GetClamped(currentLevel);
	}

	public static int GetRequiredMasteryLevelForStarSlotUnlock(int index)
	{
		return Mathf.Clamp(5 + index * 10, 5, 35);
	}

	public static int GetRequiredStardustForStarSlotUnlock(int index)
	{
		return 40 + index * 20;
	}

	public static int GetDejavuMaxWins(UnityEngine.Object target)
	{
		Rarity rarity = Rarity.Rare;
		if (target is SkillTrigger skillTrigger)
		{
			rarity = skillTrigger.rarity;
		}
		else if (target is Gem gem)
		{
			rarity = gem.rarity;
		}
		return rarity switch
		{
			Rarity.Common => 6, 
			Rarity.Rare => 5, 
			Rarity.Epic => 4, 
			Rarity.Legendary => 3, 
			Rarity.Unique => 3, 
			_ => 3, 
		};
	}

	public static bool IsDejavuFree(UnityEngine.Object target)
	{
		return IsDejavuFree(target.GetType().Name);
	}

	public static bool IsDejavuFree(string typeName)
	{
		long num = DateTime.UtcNow.ToTimestamp();
		DewSave.profileMain.dejavuCostReductionPeriodTimestamp.TryGetValue(typeName, out var value);
		return value >= num;
	}

	public static int GetDejavuCost(UnityEngine.Object target, int wins = -1)
	{
		Rarity rarity = Rarity.Rare;
		if (target is SkillTrigger skillTrigger)
		{
			rarity = skillTrigger.rarity;
		}
		else if (target is Gem gem)
		{
			rarity = gem.rarity;
		}
		if (!DewSave.profileStats.TryGetItemData(target.GetType(), out var data))
		{
			return int.MaxValue;
		}
		if (wins == -1)
		{
			wins = (int)data.wins;
		}
		return rarity switch
		{
			Rarity.Common => DejavuCostCommon.GetClamped(wins - 1), 
			Rarity.Rare => DejavuCostRare.GetClamped(wins - 1), 
			Rarity.Epic => DejavuCostEpic.GetClamped(wins - 1), 
			Rarity.Legendary => DejavuCostLegendary.GetClamped(wins - 1), 
			Rarity.Unique => DejavuCostUnique.GetClamped(wins - 1), 
			_ => int.MaxValue, 
		};
	}

	public static void ShowJsonWithEditor(string json)
	{
		string arg = DateTime.Now.ToString("yyyyMMdd_HHmmss");
		string text = Path.Combine(Path.GetTempPath(), $"{arg}_{UnityEngine.Random.Range(int.MinValue, int.MaxValue)}.json");
		File.WriteAllText(text, json);
		try
		{
			if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
			{
				Process.Start("explorer.exe", "\"" + text + "\"");
			}
			else if (Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXEditor)
			{
				Process.Start("open", "\"" + text + "\"");
			}
			else if (Application.platform == RuntimePlatform.LinuxPlayer || Application.platform == RuntimePlatform.LinuxEditor)
			{
				Process.Start("xdg-open", "\"" + text + "\"");
			}
			else
			{
				UnityEngine.Debug.LogWarning("Opening files not supported on this platform.");
			}
		}
		catch (Exception ex)
		{
			UnityEngine.Debug.LogError("Failed to open file: " + ex.Message);
		}
	}

	public static void ResolveLocalizedFormatArgs(string[] localizedFormatArgs)
	{
		for (int i = 0; i < localizedFormatArgs.Length; i++)
		{
			if (!string.IsNullOrEmpty(localizedFormatArgs[i]) && localizedFormatArgs[i].StartsWith("ui."))
			{
				localizedFormatArgs[i] = DewLocalization.GetUIValue(localizedFormatArgs[i].Substring(3));
			}
		}
	}

	public static void Debounce(UnityEngine.Object owner, float unscaledTime, Action callback, object id = null)
	{
		if ((object)owner == null)
		{
			owner = GetCoroutiner();
		}
		if (id == null)
		{
			id = owner;
		}
		if (_debounces.ContainsKey(id))
		{
			GetCoroutiner().StopCoroutine(_debounces[id]);
			_debounces.Remove(id);
		}
		if (!(owner == null) && (!(owner is Actor a) || !a.IsNullOrInactive()))
		{
			_debounces[id] = GetCoroutiner().StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			if (unscaledTime <= 0f)
			{
				yield return null;
			}
			else
			{
				yield return new WaitForSecondsRealtime(unscaledTime);
			}
			_debounces.Remove(id);
			try
			{
				if (!(owner == null) && (!(owner is Actor a2) || !a2.IsNullOrInactive()))
				{
					callback?.Invoke();
				}
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}
	}

	public static long GetRewardedMasteryPoints(float minutes)
	{
		float effectiveMinutes = 0f;
		AddToEffectiveMinutes(20f, 1.55f);
		AddToEffectiveMinutes(20f, 1.35f);
		AddToEffectiveMinutes(20f, 1.2f);
		AddToEffectiveMinutes(float.PositiveInfinity, 1f);
		return (long)(effectiveMinutes * 1000f * 1.6f);
		void AddToEffectiveMinutes(float amount, float efficiency)
		{
			if (minutes >= amount)
			{
				effectiveMinutes += amount * efficiency;
				minutes -= amount;
			}
			else
			{
				effectiveMinutes += minutes * efficiency;
				minutes = 0f;
			}
		}
	}

	public static string GetOriginalName(string name)
	{
		int num = name.IndexOf('(');
		name = ((num >= 0) ? name.Substring(0, num).Trim() : name.Trim());
		return name;
	}

	public static Coroutiner GetCoroutiner()
	{
		if (_coroutiner == null)
		{
			_coroutiner = new GameObject("Coroutiner").AddComponent<Coroutiner>();
			UnityEngine.Object.DontDestroyOnLoad(_coroutiner);
		}
		return _coroutiner;
	}

	public static int GetStardustRewardOnLevelUpAmount(int nextLevel)
	{
		return 250;
	}

	public static string NicifyVariableName(this string input)
	{
		if (string.IsNullOrEmpty(input))
		{
			return "";
		}
		string text = input;
		if (text.StartsWith("m_"))
		{
			text = text.Substring(2);
		}
		else if (text.StartsWith("_"))
		{
			text = text.Substring(1);
		}
		text = Regex.Replace(text, "(\\B[A-Z])", " $1");
		text = text.Replace('_', ' ');
		if (string.IsNullOrEmpty(text))
		{
			return "";
		}
		return char.ToUpper(text[0]) + text.Substring(1);
	}

	public static T Helper_GetInstanceOfActor<T>(ref T softInstance) where T : Actor
	{
		if ((UnityEngine.Object)(object)softInstance != null && (!softInstance.isActive || (UnityEngine.Object)(object)((NetworkBehaviour)softInstance).netIdentity == null || !((NetworkBehaviour)softInstance).isClient))
		{
			softInstance = null;
		}
		if ((UnityEngine.Object)(object)softInstance == null)
		{
			softInstance = FindActorOfType<T>();
		}
		if ((UnityEngine.Object)(object)softInstance == null)
		{
			softInstance = UnityEngine.Object.FindAnyObjectByType<T>(FindObjectsInactive.Include);
		}
		return softInstance;
	}

	public static int GetObliterationSlotPrice(int slotIndex)
	{
		return ObliterationSlotPrice.GetClamped(slotIndex);
	}

	public static int GetDefaultObliterationSlots()
	{
		return 2;
	}

	public static int GetMaxObliterationSlots()
	{
		return 10;
	}

	public static int GetObliterationMinAllowedItemsPerRarity()
	{
		return 6;
	}

	public static int GetObliterationMinAllowedHeroes()
	{
		return 1;
	}

	public static bool TryGetData<T>(this IDictionary<string, string> dict, string key0, out T value)
	{
		try
		{
			if (dict.TryGetValue(key0, out var value2))
			{
				value = DewPersistence.FromJson<T>(value2);
				return true;
			}
		}
		catch (Exception exception)
		{
			UnityEngine.Debug.LogException(exception);
		}
		value = default;
		return false;
	}

	public static bool TryGetData<T>(this IDictionary<string, string> dict, string key0, string key1, out T value)
	{
		return dict.TryGetData<T>(key0 + "::" + key1, out value);
	}

	public static bool TryGetData<T>(this IDictionary<string, string> dict, string key0, string key1, string key2, out T value)
	{
		return dict.TryGetData<T>(key0 + "::" + key1 + "::" + key2, out value);
	}

	public static void SetData<T>(this IDictionary<string, string> dict, string key0, T value)
	{
		dict[key0] = DewPersistence.ToJson(value);
	}

	public static void SetData<T>(this IDictionary<string, string> dict, string key0, string key1, T value)
	{
		dict.SetData(key0 + "::" + key1, value);
	}

	public static void SetData<T>(this IDictionary<string, string> dict, string key0, string key1, string key2, T value)
	{
		dict.SetData(key0 + "::" + key1 + "::" + key2, value);
	}

	public static T GetData<T>(this IDictionary<string, string> dict, string key0)
	{
		return DewPersistence.FromJson<T>(dict[key0]);
	}

	public static T GetData<T>(this IDictionary<string, string> dict, string key0, string key1)
	{
		return dict.GetData<T>(key0 + "::" + key1);
	}

	public static T GetData<T>(this IDictionary<string, string> dict, string key0, string key1, string key2)
	{
		return dict.GetData<T>(key0 + "::" + key1 + "::" + key2);
	}

	public static T GetDataOrDefault<T>(this IDictionary<string, string> dict, string key0, T defaultValue = default(T))
	{
		if (!dict.TryGetData<T>(key0, out var value))
		{
			return defaultValue;
		}
		return value;
	}

	public static T GetDataOrDefault<T>(this IDictionary<string, string> dict, string key0, string key1, T defaultValue = default(T))
	{
		return dict.GetDataOrDefault(key0 + "::" + key1, defaultValue);
	}

	public static T GetDataOrDefault<T>(this IDictionary<string, string> dict, string key0, string key1, string key2, T defaultValue = default(T))
	{
		return dict.GetDataOrDefault(key0 + "::" + key1 + "::" + key2, defaultValue);
	}

	public static bool RemoveData(this IDictionary<string, string> dict, string key0)
	{
		return dict.Remove(key0);
	}

	public static bool RemoveData(this IDictionary<string, string> dict, string key0, string key1)
	{
		return dict.RemoveData(key0 + "::" + key1);
	}

	public static bool RemoveData(this IDictionary<string, string> dict, string key0, string key1, string key2)
	{
		return dict.RemoveData(key0 + "::" + key1 + "::" + key2);
	}

	public static bool ContainsData(this IDictionary<string, string> dict, string key0)
	{
		return dict.ContainsKey(key0);
	}

	public static bool ContainsData(this IDictionary<string, string> dict, string key0, string key1)
	{
		return dict.ContainsData(key0 + "::" + key1);
	}

	public static bool ContainsData(this IDictionary<string, string> dict, string key0, string key1, string key2)
	{
		return dict.ContainsData(key0 + "::" + key1 + "::" + key2);
	}

	public static string GetDataKey(string key0)
	{
		return key0;
	}

	public static string GetDataKey(string key0, string key1)
	{
		return key0 + "::" + key1;
	}

	public static string GetDataKey(string key0, string key1, string key2)
	{
		return key0 + "::" + key1 + "::" + key2;
	}

	public static void SetUnscaledTimeUpdate(GameObject gobj)
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		Animator[] componentsInChildren = gobj.GetComponentsInChildren<Animator>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].updateMode = (AnimatorUpdateMode)2;
		}
		PhysicBonesCore[] componentsInChildren2 = gobj.GetComponentsInChildren<PhysicBonesCore>();
		for (int i = 0; i < componentsInChildren2.Length; i++)
		{
			((Behaviour)(object)componentsInChildren2[i]).enabled = false;
		}
		ParticleSystem[] componentsInChildren3 = gobj.GetComponentsInChildren<ParticleSystem>();
		for (int i = 0; i < componentsInChildren3.Length; i++)
		{
			MainModule main = componentsInChildren3[i].main;
			main.useUnscaledTime = true;
		}
		DewAudioSource[] componentsInChildren4 = gobj.GetComponentsInChildren<DewAudioSource>();
		for (int i = 0; i < componentsInChildren4.Length; i++)
		{
			componentsInChildren4[i].ignoreTimescale = true;
		}
		MagicaCloth[] componentsInChildren5 = gobj.GetComponentsInChildren<MagicaCloth>();
		for (int i = 0; i < componentsInChildren5.Length; i++)
		{
			componentsInChildren5[i].SerializeData.updateMode = (ClothUpdateMode)2;
		}
	}

	public static void CheckCosmeticPrerequisites(string name, string category, string heroType, CosmeticPurchasePrerequisite condition, string conditionType, int conditionValue, out bool canPurchase, out int currentProgress, out int maxProgress, out string conditionText)
	{
		currentProgress = 0;
		maxProgress = 1;
		conditionText = null;
		if (condition == CosmeticPurchasePrerequisite.None)
		{
			canPurchase = true;
			return;
		}
		string uIValue = DewLocalization.GetUIValue("CosmeticPurchasePrerequisite_" + condition);
		switch (condition)
		{
		case CosmeticPurchasePrerequisite.NightmareKillCount:
		{
			string[] array = conditionType.Split("|", StringSplitOptions.None);
			List<string> list = new List<string>();
			maxProgress = conditionValue;
			currentProgress = 0;
			string[] array2 = array;
			foreach (string text2 in array2)
			{
				if (DewSave.profileStats.monsters.TryGetValue(text2, out var value3))
				{
					currentProgress += (int)value3.nightmareKills;
				}
				if (!DewBuildProfile.current.HasFeature(BuildFeatureTag.Booth) && (value3 == null || (value3.nightmareKills == 0L && value3.kills == 0L && value3.deaths == 0L)))
				{
					list.Add("???");
				}
				else
				{
					list.Add(DewLocalization.GetUIValue(text2 + "_Name"));
				}
			}
			string arg;
			if (list.Count == 1)
			{
				arg = list[0];
			}
			else
			{
				string uIValue4 = DewLocalization.GetUIValue("CosmeticPurchasePrerequisite_MultipleOrTemplate" + list.Count);
				object[] args = list.ToArray();
				arg = string.Format(uIValue4, args);
			}
			conditionText = string.Format(uIValue, arg, maxProgress.ToString("#,##0"));
			canPurchase = currentProgress >= maxProgress;
			break;
		}
		case CosmeticPurchasePrerequisite.ReachZone:
		{
			string uIValue5 = DewLocalization.GetUIValue(conditionType + "_Name");
			maxProgress = conditionValue;
			if (DewSave.profileStats.zones.TryGetValue(conditionType, out var value4))
			{
				currentProgress = (int)value4.visited;
			}
			else
			{
				currentProgress = 0;
			}
			conditionText = string.Format(uIValue, uIValue5, maxProgress.ToString("#,##0"));
			canPurchase = currentProgress >= maxProgress;
			break;
		}
		case CosmeticPurchasePrerequisite.HeroMastery:
		{
			bool flag = name.StartsWith("Nametag_");
			string uIValue3;
			string key;
			if (flag)
			{
				uIValue3 = DewLocalization.GetUIValue(category + "_Name");
				key = category;
			}
			else
			{
				string text = name.Split("_", StringSplitOptions.None)[1];
				uIValue3 = DewLocalization.GetUIValue("Hero_" + text + "_Name");
				key = "Hero_" + text;
			}
			if (DewSave.profileStats.heroes.TryGetValue(key, out var value))
			{
				currentProgress = value.masteryLevel;
			}
			else
			{
				currentProgress = 0;
			}
			int num = -1;
			if (flag)
			{
				num = int.Parse(name.Substring(name.Length - 1, 1));
				if (num > 0)
				{
					uIValue = DewLocalization.GetUIValue("CosmeticPurchasePrerequisite_HeroMastery_WithPrevious");
				}
				maxProgress = 10 + num * 10;
			}
			else
			{
				maxProgress = conditionValue;
			}
			conditionText = string.Format(uIValue, uIValue3, maxProgress.ToString("#,##0"));
			if (num > 0)
			{
				string key2 = name.Substring(0, name.Length - 1) + (num - 1);
				canPurchase = currentProgress >= maxProgress && DewSave.profileMain.nametags.TryGetValue(key2, out var value2) && value2.isUnlocked;
			}
			else
			{
				canPurchase = currentProgress >= maxProgress;
			}
			break;
		}
		case CosmeticPurchasePrerequisite.NightmareWin:
			maxProgress = conditionValue;
			currentProgress = (int)DewSave.profileStats.total.winsNightmare;
			conditionText = string.Format(uIValue, maxProgress.ToString("#,##0"));
			canPurchase = currentProgress >= maxProgress;
			break;
		case CosmeticPurchasePrerequisite.LimboMaxDepth:
			maxProgress = conditionValue;
			currentProgress = DewSave.profileStats.total.completedLimboDepth;
			conditionText = string.Format(uIValue, maxProgress.ToString("#,##0"));
			canPurchase = currentProgress >= maxProgress;
			break;
		case CosmeticPurchasePrerequisite.WayOfStars:
		{
			string uIValue2 = DewLocalization.GetUIValue(heroType + "_Name");
			maxProgress = 0;
			currentProgress = 0;
			conditionText = string.Format(uIValue, uIValue2);
			canPurchase = false;
			break;
		}
		default:
			throw new ArgumentOutOfRangeException();
		}
		if (canPurchase)
		{
			conditionText = conditionText + " (" + DewLocalization.GetUIValue("CosmeticPurchasePrerequisite_Completed") + ")";
		}
		if (currentProgress > maxProgress)
		{
			currentProgress = maxProgress;
		}
		if (DewBuildProfile.current.HasFeature(BuildFeatureTag.Booth))
		{
			currentProgress = maxProgress;
			canPurchase = true;
		}
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void OnInitBakeMesh()
	{
		_nextMeshId = 0L;
		_nextReturnHandleId = 0L;
		_lastBakeMeshMaintenanceUnscaledTime = 0f;
		foreach (KeyValuePair<long, BakedMesh> item in _meshIdToEntry)
		{
			if ((bool)item.Value.mesh)
			{
				UnityEngine.Object.DestroyImmediate(item.Value.mesh);
			}
		}
		while (_bakedMeshPool.Count > 0)
		{
			Mesh mesh = _bakedMeshPool.Pop();
			if ((bool)mesh)
			{
				UnityEngine.Object.DestroyImmediate(mesh);
			}
		}
		_bakedMeshSubscribedZoneManager = null;
		_skinInstanceIdToNewestMeshId = new Dictionary<int, long>();
		_meshIdToEntry = new Dictionary<long, BakedMesh>();
		_returnHandleIdToMeshId = new Dictionary<long, long>();
	}

	private static void ReleaseBakedMesh(Mesh mesh)
	{
		if (_bakedMeshPool.Count < 8)
		{
			_bakedMeshPool.Push(mesh);
		}
		else
		{
			UnityEngine.Object.DestroyImmediate(mesh);
		}
	}

	private static void OnRoomLoadedBakeMesh(EventInfoLoadRoom _)
	{
		while (_bakedMeshPool.Count > 0)
		{
			UnityEngine.Object.DestroyImmediate(_bakedMeshPool.Pop());
		}
	}

	private static void EnsureBakeMeshZoneSubscription()
	{
		ZoneManager softInstance = NetworkedManagerBase<ZoneManager>.softInstance;
		if (!((UnityEngine.Object)(object)softInstance == (UnityEngine.Object)(object)_bakedMeshSubscribedZoneManager))
		{
			if ((UnityEngine.Object)(object)_bakedMeshSubscribedZoneManager != null)
			{
				_bakedMeshSubscribedZoneManager.ClientEvent_OnRoomLoaded -= new Action<EventInfoLoadRoom>(OnRoomLoadedBakeMesh);
			}
			if ((UnityEngine.Object)(object)softInstance != null)
			{
				softInstance.ClientEvent_OnRoomLoaded += new Action<EventInfoLoadRoom>(OnRoomLoadedBakeMesh);
			}
			_bakedMeshSubscribedZoneManager = softInstance;
		}
	}

	public static void GetBakedMeshOptimized(SkinnedMeshRenderer skin, out Mesh mesh, out BakedMeshHandle handle, out bool isNew)
	{
		if (!Application.IsPlaying(skin))
		{
			throw new InvalidOperationException("Can only bake when playing");
		}
		if (Time.unscaledTime - _lastBakeMeshMaintenanceUnscaledTime > BakeMeshMaintenanceInterval)
		{
			_lastBakeMeshMaintenanceUnscaledTime = Time.unscaledTime;
			EnsureBakeMeshZoneSubscription();
			ListReturnHandle<long> handle2;
			foreach (long item in _meshIdToEntry.Keys.ToListNonAlloc(out handle2))
			{
				BakedMesh bakedMesh = _meshIdToEntry[item];
				if (bakedMesh.users <= 0 && !(Time.unscaledTime - bakedMesh.bakeUnscaledTime <= BakeMeshInterval))
				{
					if (_skinInstanceIdToNewestMeshId.TryGetValue(bakedMesh.skinInstanceId, out var value) && value == item)
					{
						_skinInstanceIdToNewestMeshId.Remove(bakedMesh.skinInstanceId);
					}
					if ((bool)bakedMesh.mesh)
					{
						ReleaseBakedMesh(bakedMesh.mesh);
					}
					_meshIdToEntry.Remove(item);
				}
			}
			handle2.Return();
		}
		BakedMesh value2 = default;
		int instanceID = skin.GetInstanceID();
		if (!_skinInstanceIdToNewestMeshId.TryGetValue(instanceID, out var value3) || !_meshIdToEntry.TryGetValue(value3, out value2) || Time.unscaledTime - value2.bakeUnscaledTime > BakeMeshInterval)
		{
			if ((bool)value2.mesh && value2.users <= 0)
			{
				_meshIdToEntry.Remove(value3);
				ReleaseBakedMesh(value2.mesh);
			}
			value3 = _nextMeshId++;
			value2 = new BakedMesh
			{
				mesh = ((_bakedMeshPool.Count > 0) ? _bakedMeshPool.Pop() : new Mesh()),
				users = 0,
				bakeUnscaledTime = Time.unscaledTime,
				skinInstanceId = instanceID
			};
			skin.BakeMesh(value2.mesh, useScale: false);
		}
		value2.users++;
		_skinInstanceIdToNewestMeshId[instanceID] = value3;
		_meshIdToEntry[value3] = value2;
		handle = new BakedMeshHandle
		{
			_returnHandleId = _nextReturnHandleId++
		};
		_returnHandleIdToMeshId[handle._returnHandleId] = value3;
		isNew = value2.bakeUnscaledTime == Time.unscaledTime;
		mesh = value2.mesh;
	}

	public static bool IsExcludedFromPool(string type)
	{
		return DewResources.database.excludedFromPoolObjects.Contains(type);
	}

	public static bool IsHeroIncludedInGame(string type, DewGameContentSettings content = null)
	{
		if (content == null)
		{
			content = DewBuildProfile.current.content;
		}
		if (content.includeAllContents)
		{
			return true;
		}
		if (content.availableHeroes != null && content.availableHeroes.Count > 0 && !content.availableHeroes.Contains(type))
		{
			return false;
		}
		return true;
	}

	public static bool IsSkillIncludedInGame(string type, DewGameContentSettings content = null)
	{
		if (content == null)
		{
			content = DewBuildProfile.current.content;
		}
		if (content.includeAllContents)
		{
			return true;
		}
		if (content.availableSkills != null && content.availableSkills.Count > 0 && !content.availableSkills.Contains(type))
		{
			return false;
		}
		return true;
	}

	public static bool IsGemIncludedInGame(string type, DewGameContentSettings content = null)
	{
		if (content == null)
		{
			content = DewBuildProfile.current.content;
		}
		if (content.includeAllContents)
		{
			return true;
		}
		if (content.availableGems != null && content.availableGems.Count > 0 && !content.availableGems.Contains(type))
		{
			return false;
		}
		return true;
	}

	public static bool IsArtifactIncludedInGame(string type, DewGameContentSettings content = null)
	{
		if (content == null)
		{
			content = DewBuildProfile.current.content;
		}
		if (content.includeAllContents)
		{
			return true;
		}
		if (content.availableArtifacts != null && content.availableArtifacts.Count > 0 && !content.availableArtifacts.Contains(type))
		{
			return false;
		}
		return true;
	}

	public static bool IsAchievementIncludedInGame(string typeStr, DewGameContentSettings content = null)
	{
		if (content == null)
		{
			content = DewBuildProfile.current.content;
		}
		if (content.includeAllContents)
		{
			return true;
		}
		if (!achievementsByName.TryGetValue(typeStr, out var value))
		{
			return false;
		}
		List<Type> unlockedTargetsOfAchievement = GetUnlockedTargetsOfAchievement(value);
		if (unlockedTargetsOfAchievement.Count == 0)
		{
			return true;
		}
		foreach (Type item in unlockedTargetsOfAchievement)
		{
			if (typeof(SkillTrigger).IsAssignableFrom(item) && IsSkillIncludedInGame(item.Name, content))
			{
				return true;
			}
			if (typeof(Gem).IsAssignableFrom(item) && IsGemIncludedInGame(item.Name, content))
			{
				return true;
			}
			if (typeof(Hero).IsAssignableFrom(item) && IsHeroIncludedInGame(item.Name, content))
			{
				return true;
			}
			if (typeof(LucidDream).IsAssignableFrom(item) && IsLucidDreamIncludedInGame(item.Name, content))
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsStarIncludedInGame(string type, DewGameContentSettings content = null)
	{
		if (content == null)
		{
			content = DewBuildProfile.current.content;
		}
		if (content.includeAllContents)
		{
			return true;
		}
		if (content.availableStars != null && content.availableStars.Count > 0 && !content.availableStars.Contains(type))
		{
			return false;
		}
		return true;
	}

	public static bool IsGameModifierIncludedInGame(string type, DewGameContentSettings content = null)
	{
		if (content == null)
		{
			content = DewBuildProfile.current.content;
		}
		if (content.includeAllContents)
		{
			return true;
		}
		if (content.availableGameModifiers != null && content.availableGameModifiers.Count > 0 && !content.availableGameModifiers.Contains(type))
		{
			return false;
		}
		return true;
	}

	public static bool IsCurseIncludedInGame(string type, DewGameContentSettings content = null)
	{
		if (content == null)
		{
			content = DewBuildProfile.current.content;
		}
		if (content.includeAllContents)
		{
			return true;
		}
		if (content.availableCurses != null && content.availableCurses.Count > 0 && !content.availableCurses.Contains(type))
		{
			return false;
		}
		return true;
	}

	public static bool IsRoomModifierIncludedInGame(string type, DewGameContentSettings content = null)
	{
		if (content == null)
		{
			content = DewBuildProfile.current.content;
		}
		if (content.includeAllContents)
		{
			return true;
		}
		if (content.availableRoomModifiers != null && content.availableRoomModifiers.Count > 0 && !content.availableRoomModifiers.Contains(type))
		{
			return false;
		}
		return true;
	}

	public static bool IsLucidDreamIncludedInGame(string type, DewGameContentSettings content = null)
	{
		if (content == null)
		{
			content = DewBuildProfile.current.content;
		}
		if (content.includeAllContents)
		{
			return true;
		}
		if (content.availableLucidDreams != null && content.availableLucidDreams.Count > 0 && !content.availableLucidDreams.Contains(type))
		{
			return false;
		}
		return true;
	}

	public static bool IsMonsterIncludedInGame(string type, DewGameContentSettings content = null)
	{
		if (content == null)
		{
			content = DewBuildProfile.current.content;
		}
		if (content.includeAllContents)
		{
			return true;
		}
		if (content.excludedMonsters != null && content.excludedMonsters.Count != 0)
		{
			return !content.excludedMonsters.Contains(type);
		}
		return true;
	}

	public static bool IsTreasureIncludedInGame(string type, DewGameContentSettings content = null)
	{
		if (content == null)
		{
			content = DewBuildProfile.current.content;
		}
		if (content.includeAllContents)
		{
			return true;
		}
		if (content.availableTreasures != null && content.availableTreasures.Count > 0 && !content.availableTreasures.Contains(type))
		{
			return false;
		}
		return true;
	}

	public static bool IsAccessoryIncludedInGame(string type, DewGameContentSettings content = null)
	{
		if (content == null)
		{
			content = DewBuildProfile.current.content;
		}
		if (content.includeAllContents)
		{
			return true;
		}
		if (content.availableAccessories != null && content.availableAccessories.Count > 0 && !content.availableAccessories.Contains(type))
		{
			return false;
		}
		return true;
	}

	public static bool IsNametagIncludedInGame(string type, DewGameContentSettings content = null)
	{
		if (content == null)
		{
			content = DewBuildProfile.current.content;
		}
		if (content.includeAllContents)
		{
			return true;
		}
		if (content.availableNametags != null && content.availableNametags.Count > 0 && !content.availableNametags.Contains(type))
		{
			return false;
		}
		return true;
	}

	public static bool IsEmoteIncludedInGame(string type, DewGameContentSettings content = null)
	{
		if (content == null)
		{
			content = DewBuildProfile.current.content;
		}
		if (content.includeAllContents)
		{
			return true;
		}
		if (content.availableEmotes != null && content.availableEmotes.Count > 0 && !content.availableEmotes.Contains(type))
		{
			return false;
		}
		return true;
	}

	public static bool IsSkinIncludedInGame(string type, DewGameContentSettings content = null)
	{
		if (content == null)
		{
			content = DewBuildProfile.current.content;
		}
		if (content.includeAllContents)
		{
			return true;
		}
		if (content.availableSkins != null && content.availableSkins.Count > 0 && !content.availableSkins.Contains(type))
		{
			return false;
		}
		return true;
	}

	public static bool IsZoneIncludedInGame(string type, DewGameContentSettings content = null)
	{
		if (content == null)
		{
			content = DewBuildProfile.current.content;
		}
		if (content.includeAllContents)
		{
			return true;
		}
		if (content.includedZones != null && content.includedZones.Count > 0 && !content.includedZones.Contains(type))
		{
			return false;
		}
		return true;
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void Init()
	{
		_destroyingGameObject = new HashSet<GameObject>();
		_pendingDestroys.Clear();
		_pendingSet.Clear();
		_pendingDestroyPool.Clear();
	}

	public static void Destroy(GameObject gameObject)
	{
		if ((UnityEngine.Object)(object)NetworkedManagerBase<GameManager>.instance == null || !((Behaviour)(object)NetworkedManagerBase<GameManager>.instance).isActiveAndEnabled)
		{
			UnityEngine.Object.Destroy(gameObject);
		}
		else if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogError($"Cannot call destroy on clients: {gameObject}");
		}
		else if (SpawnManager.IsParkedInPool(gameObject))
		{
			UnityEngine.Debug.LogError($"Tried to destroy '{gameObject.name}' while it sits in the object pool (no-op)\n{new StackTrace(1, fNeedFileInfo: false)}");
		}
		else
		{
			EnqueueDestroy(gameObject);
		}
	}

	public static bool IsAnyActorBeingDestroyed()
	{
		return _destroyingGameObject.Count > 0;
	}

	public static bool IsBeingDestroyed(GameObject gameObject)
	{
		return _destroyingGameObject.Contains(gameObject);
	}

	private static void EnqueueDestroy(GameObject gameObject)
	{
		if (!_pendingSet.Add(gameObject))
		{
			return;
		}
		PendingDestroy pendingDestroy = ((_pendingDestroyPool.Count > 0) ? _pendingDestroyPool.Pop() : new PendingDestroy());
		pendingDestroy.go = gameObject;
		pendingDestroy.phase = DestroyPhase.InitialPoll;
		pendingDestroy.hasCleanups = false;
		pendingDestroy.handleObtained = false;
		pendingDestroy.addedToDestroying = false;
		pendingDestroy.retryFrames = 0;
		_pendingDestroys.Add(pendingDestroy);
		EnsureDestroyDriver();
		int count = _pendingDestroys.Count;
		bool flag;
		try
		{
			flag = StepDestroy(pendingDestroy, CanContinueDestroy());
		}
		catch (Exception exception)
		{
			UnityEngine.Debug.LogException(exception, pendingDestroy.go);
			flag = true;
		}
		if (flag)
		{
			int num = ((count - 1 < _pendingDestroys.Count && _pendingDestroys[count - 1] == pendingDestroy) ? (count - 1) : _pendingDestroys.IndexOf(pendingDestroy));
			if (num >= 0)
			{
				_pendingDestroys.RemoveAt(num);
			}
			ReturnPending(pendingDestroy);
		}
	}

	private static void EnsureDestroyDriver()
	{
		if (!(_destroyDriver != null))
		{
			GameObject gameObject = new GameObject("[DewDestroyDriver]")
			{
				hideFlags = HideFlags.HideAndDontSave
			};
			UnityEngine.Object.DontDestroyOnLoad(gameObject);
			_destroyDriver = gameObject.AddComponent<DewDestroyRunner>();
		}
	}

	private static void ProcessPendingDestroys()
	{
		int count = _pendingDestroys.Count;
		if (count == 0)
		{
			return;
		}
		bool canContinue = CanContinueDestroy();
		for (int num = count - 1; num >= 0; num--)
		{
			if (num < _pendingDestroys.Count)
			{
				PendingDestroy pendingDestroy = _pendingDestroys[num];
				bool flag;
				try
				{
					flag = StepDestroy(pendingDestroy, canContinue);
				}
				catch (Exception exception)
				{
					UnityEngine.Debug.LogException(exception, pendingDestroy.go);
					flag = true;
				}
				if (flag)
				{
					_pendingDestroys.RemoveAt(num);
					ReturnPending(pendingDestroy);
				}
			}
		}
	}

	private static bool StepDestroy(PendingDestroy pd, bool canContinue)
	{
		while (true)
		{
			switch (pd.phase)
			{
			case DestroyPhase.InitialPoll:
				if (!canContinue)
				{
					return false;
				}
				if (pd.go == null)
				{
					throw new ArgumentNullException("gameObject");
				}
				if (_destroyingGameObject.Contains(pd.go))
				{
					UnityEngine.Debug.LogWarning($"Tried to destroy a gameobject twice: {pd.go}");
					return true;
				}
				pd.go.transform.parent = null;
				UnityEngine.Object.DontDestroyOnLoad(pd.go);
				_destroyingGameObject.Add(pd.go);
				pd.addedToDestroying = true;
				pd.cleanups = pd.go.GetComponentsNonAlloc(out pd.cleanupsHandle);
				pd.handleObtained = true;
				pd.hasCleanups = pd.cleanups.Count > 0;
				if (pd.hasCleanups)
				{
					foreach (ICleanup cleanup in pd.cleanups)
					{
						try
						{
							cleanup.OnCleanup();
						}
						catch (Exception exception2)
						{
							UnityEngine.Debug.LogException(exception2, cleanup as UnityEngine.Object);
						}
					}
				}
				pd.rpcWaitEnd = Time.realtimeSinceStartup + 0.35f;
				pd.phase = DestroyPhase.RpcWait;
				break;
			case DestroyPhase.RpcWait:
				if (Time.realtimeSinceStartup < pd.rpcWaitEnd)
				{
					return false;
				}
				pd.phase = DestroyPhase.PostRpcPoll;
				break;
			case DestroyPhase.PostRpcPoll:
				if (!canContinue)
				{
					return false;
				}
				pd.phase = (pd.hasCleanups ? DestroyPhase.CanDestroyCheck : DestroyPhase.Finalize);
				break;
			case DestroyPhase.CanDestroyCheck:
			{
				bool flag = true;
				foreach (ICleanup cleanup2 in pd.cleanups)
				{
					try
					{
						if (!cleanup2.canDestroy)
						{
							flag = false;
							break;
						}
					}
					catch (Exception exception3)
					{
						flag = false;
						UnityEngine.Debug.LogException(exception3, cleanup2 as UnityEngine.Object);
						break;
					}
				}
				if (flag)
				{
					pd.phase = DestroyPhase.Finalize;
					break;
				}
				pd.retryFrames = 3;
				pd.phase = DestroyPhase.RetryWait;
				break;
			}
			case DestroyPhase.RetryWait:
				if (pd.retryFrames-- > 0)
				{
					return false;
				}
				pd.phase = DestroyPhase.RetryPoll;
				break;
			case DestroyPhase.RetryPoll:
				if (!canContinue)
				{
					return false;
				}
				pd.phase = DestroyPhase.CanDestroyCheck;
				break;
			case DestroyPhase.Finalize:
			{
				if (pd.handleObtained)
				{
					pd.cleanupsHandle.Return();
					pd.handleObtained = false;
				}
				List<ICustomDestroyRoutine> componentsNonAlloc = pd.go.GetComponentsNonAlloc(out ListReturnHandle<ICustomDestroyRoutine> handle);
				try
				{
					if (componentsNonAlloc.Count > 0)
					{
						NetworkServer.UnSpawn(pd.go);
						foreach (ICustomDestroyRoutine item in componentsNonAlloc)
						{
							try
							{
								item.CustomDestroyRoutine();
							}
							catch (Exception exception)
							{
								UnityEngine.Debug.LogException(exception, pd.go);
							}
						}
					}
					else
					{
						SpawnManager.Destroy(pd.go);
					}
				}
				finally
				{
					handle.Return();
					if (pd.addedToDestroying)
					{
						_destroyingGameObject.Remove(pd.go);
						pd.addedToDestroying = false;
					}
				}
				return true;
			}
			default:
				return true;
			}
		}
	}

	private static void ReturnPending(PendingDestroy pd)
	{
		if (pd.handleObtained)
		{
			pd.cleanupsHandle.Return();
			pd.handleObtained = false;
		}
		if (pd.addedToDestroying)
		{
			_destroyingGameObject.Remove(pd.go);
			pd.addedToDestroying = false;
		}
		_pendingSet.Remove(pd.go);
		pd.go = null;
		pd.cleanups = null;
		_pendingDestroyPool.Push(pd);
	}

	internal static void FlushPendingDestroys()
	{
		for (int i = 0; i < _pendingDestroys.Count; i++)
		{
			PendingDestroy pendingDestroy = _pendingDestroys[i];
			if (pendingDestroy.handleObtained)
			{
				pendingDestroy.cleanupsHandle.Return();
				pendingDestroy.handleObtained = false;
			}
			if (pendingDestroy.addedToDestroying)
			{
				_destroyingGameObject.Remove(pendingDestroy.go);
				pendingDestroy.addedToDestroying = false;
			}
			pendingDestroy.go = null;
			pendingDestroy.cleanups = null;
			_pendingDestroyPool.Push(pendingDestroy);
		}
		_pendingDestroys.Clear();
		_pendingSet.Clear();
	}

	private static bool CanContinueDestroy()
	{
		if (!NetworkClient.ready || NetworkServer.isLoadingScene)
		{
			return false;
		}
		if (!NetworkedManagerBase<ZoneManager>.instance.isInRoomTransition)
		{
			return true;
		}
		foreach (KeyValuePair<int, NetworkConnectionToClient> connection in NetworkServer.connections)
		{
			if (!((NetworkConnection)connection.Value).isReady)
			{
				return false;
			}
		}
		return true;
	}

	public static void ClearTypeReferences()
	{
		_allHeroes = null;
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
	public static void InitAllTypeReferences()
	{
		try
		{
			if (_allHeroes != null)
			{
				return;
			}
			_allHeroes = new List<Type>();
			_allSkills = new List<Type>();
			_allHeroSkills = new List<Type>();
			_allGems = new List<Type>();
			_allArtifacts = new List<Type>();
			_allGameModifiers = new List<Type>();
			_allAchievements = new List<Type>();
			_achievementsByName = new Dictionary<string, Type>();
			_allTutorialItems = new List<Type>();
			_allOldStars = new List<DewStarItemOld>();
			_allStarTypes = new List<Type>();
			_heroTypeOfOldStar = new Dictionary<DewStarItemOld, Type>();
			_allRoomModifiers = new List<Type>();
			_allLucidDreams = new List<Type>();
			_allReveries = new List<DewReverieItem>();
			_allMonsters = new List<Type>();
			_reveriesByName = new Dictionary<string, DewReverieItem>();
			_resourceLinkTypes = new List<(Type, DewResourceLinkAttribute)>();
			_storyNodeIdsByHero = new Dictionary<string, List<string>>();
			_resourceLinkTypes.Add((typeof(AnimationClip), new DewResourceLinkAttribute(ResourceLinkBy.Guid)));
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (Assembly assembly in assemblies)
			{
				Type[] array;
				try
				{
					array = assembly.GetTypes();
				}
				catch (ReflectionTypeLoadException ex)
				{
					array = ex.Types.Where((Type t) => t != null).ToArray();
					UnityEngine.Debug.LogWarning($"[Dew] Assembly '{assembly.FullName}' did not load all types ({ex.Message}); continuing with {array.Length} resolvable types.");
				}
				Type[] array2 = array;
				foreach (Type type in array2)
				{
					object[] customAttributes = type.GetCustomAttributes(typeof(DewResourceLinkAttribute), inherit: false);
					if (customAttributes.Length != 0)
					{
						_resourceLinkTypes.Add((type, (DewResourceLinkAttribute)customAttributes[0]));
					}
					if (!type.IsAbstract)
					{
						if (type.IsSubclassOf(typeof(StarEffect)) && !type.IsAbstract)
						{
							_allStarTypes.Add(type);
						}
						if (type.IsSubclassOf(typeof(Hero)))
						{
							_allHeroes.Add(type);
						}
						if (type.IsSubclassOf(typeof(SkillTrigger)))
						{
							_allSkills.Add(type);
						}
						if (type.IsSubclassOf(typeof(Gem)))
						{
							_allGems.Add(type);
						}
						if (type.IsSubclassOf(typeof(Artifact)))
						{
							_allArtifacts.Add(type);
						}
						if (type.IsSubclassOf(typeof(GameModifierBase)))
						{
							_allGameModifiers.Add(type);
						}
						if (type.IsSubclassOf(typeof(RoomModifierBase)))
						{
							_allRoomModifiers.Add(type);
						}
						if (type.IsSubclassOf(typeof(LucidDream)))
						{
							_allLucidDreams.Add(type);
						}
						if (type.IsSubclassOf(typeof(Monster)))
						{
							_allMonsters.Add(type);
						}
						if (type.IsSubclassOf(typeof(DewReverieItem)) && !type.IsAbstract)
						{
							DewReverieItem dewReverieItem = (DewReverieItem)Activator.CreateInstance(type);
							_allReveries.Add(dewReverieItem);
							_reveriesByName.Add(type.Name, dewReverieItem);
						}
						if (type.IsSubclassOf(typeof(DewAchievementItem)))
						{
							_allAchievements.Add(type);
							_achievementsByName.Add(type.Name, type);
						}
						if (type.IsSubclassOf(typeof(DewInGameTutorialItem)))
						{
							_allTutorialItems.Add(type);
						}
						if (type.IsSubclassOf(typeof(DewStarItemOld)) && !type.IsAbstract)
						{
							DewStarItemOld item = (DewStarItemOld)Activator.CreateInstance(type);
							_allOldStars.Add(item);
						}
					}
				}
			}
			foreach (Type allHero in _allHeroes)
			{
				if (IsHeroIncludedInGame(allHero.Name))
				{
					HeroSkill component = ((Component)(object)DewResources.GetByType<Hero>(allHero, default(ResourceLoadSettings))).GetComponent<HeroSkill>();
					SkillTrigger[] loadoutSkills = component.GetLoadoutSkills(HeroSkillLocation.Q);
					foreach (SkillTrigger skillTrigger in loadoutSkills)
					{
						_allHeroSkills.Add(((object)skillTrigger).GetType());
					}
					loadoutSkills = component.GetLoadoutSkills(HeroSkillLocation.R);
					foreach (SkillTrigger skillTrigger2 in loadoutSkills)
					{
						_allHeroSkills.Add(((object)skillTrigger2).GetType());
					}
					loadoutSkills = component.GetLoadoutSkills(HeroSkillLocation.Identity);
					foreach (SkillTrigger skillTrigger3 in loadoutSkills)
					{
						_allHeroSkills.Add(((object)skillTrigger3).GetType());
					}
				}
			}
			_resourceLinkTypes.Sort(((Type, DewResourceLinkAttribute) x, (Type, DewResourceLinkAttribute) y) =>
			{
				int num2 = GetInheritanceDepth(x.Item1);
				int num3 = GetInheritanceDepth(y.Item1);
				return (num2 == num3) ? string.Compare(x.Item1.FullName, y.Item1.FullName, StringComparison.Ordinal) : num3.CompareTo(num2);
			});
			foreach (TravelerStoryNodeDef item2 in DewResources.FindAllByNameSubstring<TravelerStoryNodeDef>("TravelerStory_", default(ResourceLoadSettings)))
			{
				if (!string.IsNullOrEmpty(item2.heroType))
				{
					if (!_storyNodeIdsByHero.ContainsKey(item2.heroType))
					{
						_storyNodeIdsByHero[item2.heroType] = new List<string>();
					}
					_storyNodeIdsByHero[item2.heroType].Add(((UnityEngine.Object)(object)item2).name);
				}
			}
		}
		catch (Exception exception)
		{
			UnityEngine.Debug.LogException(exception);
			GlobalLogicPackage.CallOnReady(() =>
			{
				DewSessionError.ShowError(new DewException(DewExceptionType.CorruptGameFiles), isFatal: true);
			});
		}
		static int GetInheritanceDepth(Type baseType)
		{
			int num2 = 0;
			while (baseType.BaseType != null)
			{
				num2++;
				baseType = baseType.BaseType;
			}
			return num2;
		}
	}

	public static T SpawnHero<T>(Vector3 position, Quaternion? rotation, DewPlayer owner, int level, HeroLoadoutData loadout, string skin, Action<T> beforeSpawn = null) where T : Hero
	{
		return SpawnHero(DewResources.GetByType<T>(default(ResourceLoadSettings)), position, rotation, owner, level, loadout, skin, (T h) =>
		{
			beforeSpawn?.Invoke(h);
		});
	}

	public static T SpawnHero<T>(T hero, Vector3 position, Quaternion? rotation, DewPlayer owner, int level, HeroLoadoutData loadout, string skin, Action<T> beforeSpawn = null) where T : Hero
	{
		if ((UnityEngine.Object)(object)owner == null)
		{
			throw new Exception("Provided owner player is null!");
		}
		return PrepareAndSpawnHero_Imp(InstantiateActor_Imp(hero, position, rotation, null), owner, level, loadout, skin, beforeSpawn);
	}

	public static T SpawnEntity<T>(Vector3 position, Quaternion? rotation, Actor parent, DewPlayer owner, int level, Action<T> beforeSpawn = null) where T : Entity
	{
		return SpawnEntity(DewResources.GetByType<T>(default(ResourceLoadSettings)), position, rotation, parent, owner, level, beforeSpawn);
	}

	public static T SpawnEntity<T>(T entity, Vector3 position, Quaternion? rotation, Actor parent, DewPlayer owner, int level, Action<T> beforeSpawn = null) where T : Entity
	{
		if ((UnityEngine.Object)(object)owner == null)
		{
			throw new Exception("Provided owner player is null!");
		}
		T val = InstantiateActor_Imp(entity, position, rotation, null);
		val.Status.level = level;
		val.parentActor = parent;
		val.owner = owner;
		return PrepareAndSpawnActor_Imp(val, beforeSpawn);
	}

	public static T CreateSkillTrigger<T>(Vector3 position, int level, DewPlayer tempOwner = null, Action<T> beforePrepare = null) where T : SkillTrigger
	{
		return CreateSkillTrigger(DewResources.GetByType<T>(default(ResourceLoadSettings)), position, level, tempOwner, beforePrepare);
	}

	public static T CreateSkillTrigger<T>(T trigger, Vector3 position, int level, DewPlayer tempOwner = null, Action<T> beforePrepare = null) where T : SkillTrigger
	{
		T val = InstantiateActor_Imp(trigger, position, null, null);
		val.level = level;
		val.tempOwner = tempOwner;
		return PrepareAndSpawnActor_Imp(val, beforePrepare);
	}

	public static T CreateAbilityTrigger<T>(Action<T> beforeSpawn = null) where T : AbilityTrigger
	{
		return CreateAbilityTrigger(DewResources.GetByType<T>(default(ResourceLoadSettings)), beforeSpawn);
	}

	public static T CreateAbilityTrigger<T>(T trigger, Action<T> beforePrepare = null) where T : AbilityTrigger
	{
		return CreateActor(trigger, Vector3.zero, null, null, beforePrepare);
	}

	public static T CreateGem<T>(Vector3 position, int quality, DewPlayer tempOwner = null, Action<T> beforePrepare = null) where T : Gem
	{
		return CreateGem(DewResources.GetByType<T>(default(ResourceLoadSettings)), position, quality, tempOwner, beforePrepare);
	}

	public static T CreateGem<T>(T gem, Vector3 position, int quality, DewPlayer tempOwner = null, Action<T> beforePrepare = null) where T : Gem
	{
		T val = InstantiateActor_Imp(gem, position, null, null);
		val.quality = quality;
		val.tempOwner = tempOwner;
		return PrepareAndSpawnActor_Imp(val, beforePrepare);
	}

	public static T CreateStatusEffect<T>(Entity victim, Actor parent, CastInfo info, Action<T> beforePrepare = null) where T : StatusEffect
	{
		return CreateStatusEffect(DewResources.GetByType<T>(DewResources.GetSuggestedResourceLoadSettings(parent, typeof(T))), victim, parent, info, beforePrepare);
	}

	public static T CreateStatusEffect<T>(T prefab, Entity victim, Actor parent, CastInfo info, Action<T> beforePrepare = null) where T : StatusEffect
	{
		if ((UnityEngine.Object)(object)victim == null)
		{
			UnityEngine.Debug.LogWarning("Tried to create " + typeof(T).Name + " with null victim");
			return null;
		}
		if (prefab.isKilledByCrowdControlImmunity && (victim.Status.hasUnstoppable || victim.Status.hasInvulnerable))
		{
			NetworkedManagerBase<ClientEventManager>.instance.InvokeOnIgnoreCC(victim);
			return null;
		}
		return PrepareAndSpawnStatusEffect_Imp(InstantiateActor_Imp(prefab, victim.position, null, parent), victim, parent, info, beforePrepare);
	}

	public static T CreateAbilityInstance<T>(Vector3 position, Quaternion? rotation, Actor parent, CastInfo info, Action<T> beforePrepare = null) where T : AbilityInstance
	{
		return CreateAbilityInstance(DewResources.GetByType<T>(DewResources.GetSuggestedResourceLoadSettings(parent, typeof(T))), position, rotation, parent, info, beforePrepare);
	}

	public static T CreateAbilityInstance<T>(T prefab, Vector3 position, Quaternion? rotation, Actor parent, CastInfo info, Action<T> beforePrepare = null) where T : AbilityInstance
	{
		if (prefab is StatusEffect)
		{
			throw new Exception("Use CreateStatusEffect for creating status effects");
		}
		T val = InstantiateActor_Imp(prefab, position, rotation, parent);
		val.parentActor = parent;
		val.info = info;
		if ((UnityEngine.Object)(object)parent != null)
		{
			try
			{
				parent.InvokeOnAbilityInstanceBeforePrepare(new EventInfoAbilityInstance
				{
					actor = parent,
					instance = val
				});
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}
		PrepareAndSpawnActor_Imp(val, beforePrepare);
		if ((UnityEngine.Object)(object)parent != null)
		{
			try
			{
				parent.InvokeOnAbilityInstanceCreated(new EventInfoAbilityInstance
				{
					actor = parent,
					instance = val
				});
			}
			catch (Exception exception2)
			{
				UnityEngine.Debug.LogException(exception2);
			}
		}
		return val;
	}

	public static T CreateActor<T>(Action<T> beforePrepare = null) where T : Actor
	{
		return CreateActor(Vector3.zero, null, null, beforePrepare);
	}

	public static T CreateActor<T>() where T : Actor
	{
		return CreateActor<T>(Vector3.zero, null);
	}

	public static T CreateActor<T>(Vector3 position, Quaternion? rotation, Actor parent = null, Action<T> beforePrepare = null) where T : Actor
	{
		return CreateActor(DewResources.GetByType<T>(default(ResourceLoadSettings)), position, rotation, parent, beforePrepare);
	}

	public static T CreateActor<T>(T prefab, Vector3 position, Quaternion? rotation, Actor parent = null, Action<T> beforePrepare = null) where T : Actor
	{
		return PrepareAndSpawnActor_Imp(InstantiateActor_Imp(prefab, position, rotation, parent), beforePrepare);
	}

	public static T InstantiateActor_Imp<T>(T prefab, Vector3 position, Quaternion? rotation, Actor parent) where T : Actor
	{
		if ((UnityEngine.Object)(object)prefab == null && prefab != null)
		{
			prefab = DewResources.GetByType<T>(((object)prefab).GetType(), default(ResourceLoadSettings));
			if ((UnityEngine.Object)(object)prefab != null)
			{
				UnityEngine.Debug.LogWarning("Tried to instantiate destroyed actor (" + ((object)prefab).GetType().Name + "). You must not store actors for later use; instead fetch them from the database whenever they're needed");
			}
		}
		if ((UnityEngine.Object)(object)prefab == null)
		{
			throw new Exception("Actor you're trying to instantiate is null");
		}
		if (!NetworkServer.active)
		{
			throw new Exception("You can only create actors on server");
		}
		if (!rotation.HasValue)
		{
			rotation = ManagerBase<CameraManager>.instance.entityCamAngleRotation;
		}
		T val = SpawnManager.Create<T>(prefab, position, rotation.Value, (Transform)null);
		val.parentActor = parent;
		if (val is AbilityInstance && !(val is PickupInstance))
		{
			uint ownerNetId = 0u;
			Entity entity = (parent as Entity) ?? (((UnityEngine.Object)(object)parent != null) ? parent.firstEntity : null);
			if ((UnityEngine.Object)(object)entity != null && (UnityEngine.Object)(object)entity.owner != null && entity.owner.isHumanPlayer && (UnityEngine.Object)(object)entity.owner.hero != null)
			{
				ownerNetId = ((NetworkBehaviour)entity.owner).netId;
			}
			if (SpawnManager.IsMonsterAbilityPrefab(((Component)(object)prefab).gameObject))
			{
				if (SpawnManager.TryGetReuseInRoomActiveCount(((Component)(object)prefab).gameObject, out var activeCount))
				{
					AdaptiveAbilityBaseline.RecordPeakActiveUsage(((NetworkBehaviour)val).netIdentity.assetId, ownerNetId, activeCount);
				}
				else
				{
					AdaptiveAbilityBaseline.IncrementCurrentRoomUsage(((NetworkBehaviour)val).netIdentity.assetId, ownerNetId);
				}
			}
		}
		return val;
	}

	public static T PrepareAndSpawnActor_Imp<T>(T newActor, Action<T> beforePrepare) where T : Actor
	{
		try
		{
			beforePrepare?.Invoke(newActor);
		}
		catch (Exception exception)
		{
			UnityEngine.Debug.LogException(exception);
		}
		newActor.PrepareAndSpawn();
		return newActor;
	}

	public static T PrepareAndSpawnHero_Imp<T>(T newEntity, DewPlayer owner, int level, HeroLoadoutData loadout, string skin, Action<T> beforePrepare) where T : Hero
	{
		newEntity.Status.level = level;
		newEntity.owner = owner;
		newEntity.loadout = loadout;
		if (!string.IsNullOrEmpty(skin))
		{
			newEntity.skin = skin;
		}
		return PrepareAndSpawnActor_Imp(newEntity, beforePrepare);
	}

	public static T PrepareAndSpawnStatusEffect_Imp<T>(T newSe, Entity victim, Actor parent, CastInfo info, Action<T> beforePrepare) where T : StatusEffect
	{
		newSe.parentActor = parent;
		newSe.info = info;
		newSe.victim = victim;
		if ((UnityEngine.Object)(object)parent != null)
		{
			try
			{
				parent.InvokeOnAbilityInstanceBeforePrepare(new EventInfoAbilityInstance
				{
					actor = parent,
					instance = newSe
				});
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}
		PrepareAndSpawnActor_Imp(newSe, beforePrepare);
		if ((UnityEngine.Object)(object)parent != null)
		{
			try
			{
				parent.InvokeOnAbilityInstanceCreated(new EventInfoAbilityInstance
				{
					actor = parent,
					instance = newSe
				});
			}
			catch (Exception exception2)
			{
				UnityEngine.Debug.LogException(exception2);
			}
		}
		return newSe;
	}

	public static T InstantiateAndSpawn<T>(T prefab, Action<T> beforeSpawn = null) where T : Component
	{
		return InstantiateAndSpawn(prefab, Vector3.zero, null, beforeSpawn);
	}

	public static T InstantiateAndSpawn<T>(Vector3 position, Quaternion? rotation, Action<T> beforeSpawn = null) where T : Component
	{
		return InstantiateAndSpawn(DewResources.GetByType<T>(), position, rotation, beforeSpawn);
	}

	public static T InstantiateAndSpawn<T>(Action<T> beforeSpawn = null) where T : Component
	{
		return InstantiateAndSpawn(DewResources.GetByType<T>(), beforeSpawn);
	}

	public static T InstantiateAndSpawn<T>(T prefab, Vector3 position, Quaternion? rotation, Action<T> beforeSpawn = null) where T : Component
	{
		if (!NetworkServer.active)
		{
			throw new Exception("You can only instantiate and spawn on server.");
		}
		Type type = prefab.GetType();
		if (typeof(Hero).IsAssignableFrom(type))
		{
			throw new Exception($"Use SpawnHero for spawning heroes: {type}");
		}
		if (typeof(Entity).IsAssignableFrom(type))
		{
			throw new Exception($"Use SpawnEntity for spawning entities: {type}");
		}
		if (typeof(SkillTrigger).IsAssignableFrom(type))
		{
			throw new Exception($"Use CreateSkillTrigger for creating skill triggers: {type}");
		}
		if (typeof(AbilityTrigger).IsAssignableFrom(type))
		{
			throw new Exception($"Use CreateAbilityTrigger for creating ability triggers: {type}");
		}
		if (typeof(StatusEffect).IsAssignableFrom(type))
		{
			throw new Exception($"Use CreateStatusEffect for creating status effects: {type}");
		}
		if (typeof(AbilityInstance).IsAssignableFrom(type))
		{
			throw new Exception($"Use CreateAbilityInstance for creating ability instances: {type}");
		}
		if (typeof(Gem).IsAssignableFrom(type))
		{
			throw new Exception($"Use CreateGem for creating gems: {type}");
		}
		if (!rotation.HasValue)
		{
			rotation = ManagerBase<CameraManager>.instance.entityCamAngleRotation;
		}
		T val = SpawnManager.Create(prefab, position, rotation.Value, null);
		val.name = prefab.name;
		beforeSpawn?.Invoke(val);
		NetworkServer.Spawn(val.gameObject, (NetworkConnection)null);
		return val;
	}

	static Dew()
	{
		//IL_0291: Unknown result type (might be due to invalid IL or missing references)
		//IL_029b: Expected Obj, but got Unknown
	}
}
