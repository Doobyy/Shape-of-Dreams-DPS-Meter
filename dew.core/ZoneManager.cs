using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ZoneManager : NetworkedManagerBase<ZoneManager>
{
	public SafeAction<EventInfoLoadRoom> ClientEvent_OnRoomLoaded;

	public SafeAction ClientEvent_OnLoopStarted;

	public SafeAction<EventInfoLoadZone> ClientEvent_OnZoneLoaded;

	public SafeAction<EventInfoLoadZone> ClientEvent_OnZoneLoadStarted;

	public SafeAction<EventInfoLoadRoom> ClientEvent_OnRoomLoadStarted;

	public SafeAction<bool> ClientEvent_OnIsInTransitionChanged;

	[SyncVar]
	private SyncableAssetRef _currentZone;

	[CompilerGenerated]
	[SyncVar]
	private Room currentRoom__BackingField;

	[CompilerGenerated]
	[SyncVar]
	private int currentZoneIndex__BackingField;

	[CompilerGenerated]
	[SyncVar(hook = "OnCurrentRoomIndexChanged")]
	private int currentRoomIndex__BackingField;

	public SafeAction ClientEvent_OnCurrentRoomIndexChanged;

	[CompilerGenerated]
	[SyncVar(hook = "OnClearedCombatRoomsChanged")]
	private int clearedCombatRooms__BackingField;

	public SafeAction ClientEvent_OnClearedCombatRoomsChanged;

	[CompilerGenerated]
	[SyncVar]
	private int currentZoneClearedNodes__BackingField;

	[CompilerGenerated]
	[SyncVar(hook = "OnIsInTransitionChanged")]
	private bool isInRoomTransition__BackingField;

	[CompilerGenerated]
	[SyncVar(hook = "OnCurrentHuntLevelChanged")]
	private int currentHuntLevel__BackingField;

	public SafeAction ClientEvent_OnCurrentHuntLevelChanged;

	[NonSerialized]
	public bool didLocallyShowHunterWarning;

	[NonSerialized]
	[SaveVar(SaveVarFlags.Default)]
	public List<string> bannedSidetracksForCurrentLoop = new List<string>();

	[NonSerialized]
	[SaveVar(SaveVarFlags.Default)]
	public List<string> bannedRoomModifiersForCurrentLoop = new List<string>();

	private List<Func<EventInfoTravelToNodeInterrupt, bool>> _travelToNodeInterrupts = new List<Func<EventInfoTravelToNodeInterrupt, bool>>();

	[NonSerialized]
	public bool? forceVoteForDebug;

	public SafeAction<DewPlayer> ClientEvent_OnVoteStarted;

	public SafeAction<DewPlayer> ClientEvent_OnVoteCanceled;

	public SafeAction ClientEvent_OnVoteCompleted;

	[CompilerGenerated]
	[SyncVar]
	private bool isVoting__BackingField;

	[CompilerGenerated]
	[SyncVar]
	private int voteRemainingSeconds__BackingField = -1;

	[SyncVar]
	public VoteType voteType;

	[CompilerGenerated]
	[SyncVar]
	private int voteData__BackingField = -1;

	private Coroutine _voteRoutine;

	private Coroutine _voteCheckRoutine;

	private Coroutine _voteCompleteRoutine;

	public static float WorldHeight;

	public static float WorldWidth;

	public static float ScorePerIsolatedNode;

	public static float ScorePerIsolatedImportantNode;

	public static float ScoreFuzziness;

	public static int WorldGenerationIterations;

	public static int NodePlacementTries;

	public static int HunterStartSkippedTurns;

	private const int NodeDistInfinity = 10000;

	public SafeAction onWorldGenerated;

	[CompilerGenerated]
	[SyncVar]
	private int currentNodeIndex__BackingField = -1;

	[CompilerGenerated]
	[SyncVar]
	private int sidetrackReturnNodeIndex__BackingField = -1;

	[SaveVar(SaveVarFlags.Default)]
	public readonly SyncList<WorldNodeData> nodes = new SyncList<WorldNodeData>();

	[SaveVar(SaveVarFlags.Default)]
	public readonly SyncList<HunterStatus> hunterStatuses = new SyncList<HunterStatus>();

	public SafeAction ClientEvent_OnNodesChanged;

	[SaveVar(SaveVarFlags.Default)]
	public readonly SyncList<int> nodeDistanceMatrix = new SyncList<int>();

	[NonSerialized]
	[SaveVar(SaveVarFlags.Default)]
	public List<DewPersistence.RoomData> visitedNodesSaveData;

	[SaveVar(SaveVarFlags.Default)]
	[SyncVar]
	private uint _worldSeed;

	[SaveVar(SaveVarFlags.Default)]
	private Dictionary<uint, DewRandom> _currentWorldRandomInstances = new Dictionary<uint, DewRandom>();

	[SaveVar(SaveVarFlags.Default)]
	private List<string> _combatRoomPool = new List<string>();

	[SaveVar(SaveVarFlags.Default)]
	private List<string> _shopRoomPool = new List<string>();

	[SaveVar(SaveVarFlags.Default)]
	internal Dictionary<string, List<int>> _usedAngleIndexes = new Dictionary<string, List<int>>();

	[CompilerGenerated]
	[SyncVar]
	private int hunterSkippedTurns__BackingField;

	[CompilerGenerated]
	[SyncVar]
	private bool isHuntAdvanceDisabled__BackingField;

	[CompilerGenerated]
	[SyncVar]
	private int hunterStartNodeIndex__BackingField;

	private bool _needToCallNodesChanged;

	[SaveVar(SaveVarFlags.Default)]
	private int _visitedCombatNodesWithoutMiniBoss;

	[SaveVar(SaveVarFlags.Default)]
	internal List<RoomRewardFlowItemType> _nextRewards;

	[SaveVar(SaveVarFlags.Default)]
	private float _hunterSpreadCredit;

	[SaveVar(SaveVarFlags.Default)]
	internal int _nextModifierId = 1;

	[SaveVar(SaveVarFlags.Default)]
	public Dictionary<int, ModifierServerData> modifierServerData = new Dictionary<int, ModifierServerData>();

	[NonSerialized]
	public float hunterSpreadMultiplier = 1f;

	[CompilerGenerated]
	[SyncVar]
	private int loopIndex__BackingField;

	[CompilerGenerated]
	[SyncVar]
	private int currentTier__BackingField = -1;

	[NonSerialized]
	[SaveVar(SaveVarFlags.Default)]
	public List<AssetRef<Zone>> remainingZonesOfCurrentTier = new List<AssetRef<Zone>>();

	[NonSerialized]
	[SaveVar(SaveVarFlags.Default)]
	public AssetRef<Zone> nextZoneOverride = null;

	[NonSerialized]
	[SaveVar(SaveVarFlags.Default)]
	public List<int> hardVariantBossZoneIndices = new List<int>();

	protected NetworkBehaviourSyncVar ____003CcurrentRoom_003Ek__BackingFieldNetId;

	public Action<int, int> _Mirror_SyncVarHookDelegate__003CcurrentRoomIndex_003Ek__BackingField;

	public Action<int, int> _Mirror_SyncVarHookDelegate__003CclearedCombatRooms_003Ek__BackingField;

	public Action<bool, bool> _Mirror_SyncVarHookDelegate__003CisInRoomTransition_003Ek__BackingField;

	public Action<int, int> _Mirror_SyncVarHookDelegate__003CcurrentHuntLevel_003Ek__BackingField;

	[SaveVar(SaveVarFlags.Default)]
	public Zone currentZone
	{
		get
		{
			return _currentZone.asset as Zone;
		}
		set
		{
			Network_currentZone = value;
		}
	}

	public Room currentRoom
	{
		[CompilerGenerated]
		get
		{
			return Network_003CcurrentRoom_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CcurrentRoom_003Ek__BackingField = value;
		}
	}

	[SaveVar(SaveVarFlags.Default)]
	public int currentZoneIndex
	{
		[CompilerGenerated]
		get
		{
			return currentZoneIndex__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CcurrentZoneIndex_003Ek__BackingField = value;
		}
	}

	[SaveVar(SaveVarFlags.Default)]
	public int currentRoomIndex
	{
		[CompilerGenerated]
		get
		{
			return currentRoomIndex__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CcurrentRoomIndex_003Ek__BackingField = value;
		}
	}

	[SaveVar(SaveVarFlags.Default)]
	public int clearedCombatRooms
	{
		[CompilerGenerated]
		get
		{
			return clearedCombatRooms__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CclearedCombatRooms_003Ek__BackingField = value;
		}
	}

	[SaveVar(SaveVarFlags.Default)]
	public int currentZoneClearedNodes
	{
		[CompilerGenerated]
		get
		{
			return currentZoneClearedNodes__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CcurrentZoneClearedNodes_003Ek__BackingField = value;
		}
	}

	public bool isInAnyTransition
	{
		get
		{
			if (!isInLocalTransition)
			{
				return isInRoomTransition;
			}
			return true;
		}
	}

	public bool isInLocalTransition { get; internal set; }

	public bool isInRoomTransition
	{
		[CompilerGenerated]
		get
		{
			return isInRoomTransition__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			Network_003CisInRoomTransition_003Ek__BackingField = value;
		}
	}

	[SaveVar(SaveVarFlags.Default)]
	public int currentHuntLevel
	{
		[CompilerGenerated]
		get
		{
			return currentHuntLevel__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CcurrentHuntLevel_003Ek__BackingField = value;
		}
	}

	public bool isVoting
	{
		[CompilerGenerated]
		get
		{
			return isVoting__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			Network_003CisVoting_003Ek__BackingField = value;
		}
	}

	public int voteRemainingSeconds
	{
		[CompilerGenerated]
		get
		{
			return voteRemainingSeconds__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			Network_003CvoteRemainingSeconds_003Ek__BackingField = value;
		}
	}

	public int voteData
	{
		[CompilerGenerated]
		get
		{
			return voteData__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			Network_003CvoteData_003Ek__BackingField = value;
		}
	}

	[SaveVar(SaveVarFlags.Default)]
	public int currentNodeIndex
	{
		[CompilerGenerated]
		get
		{
			return currentNodeIndex__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CcurrentNodeIndex_003Ek__BackingField = value;
		}
	}

	public bool isSidetracking => sidetrackReturnNodeIndex >= 0;

	[SaveVar(SaveVarFlags.Default)]
	public int sidetrackReturnNodeIndex
	{
		[CompilerGenerated]
		get
		{
			return sidetrackReturnNodeIndex__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CsidetrackReturnNodeIndex_003Ek__BackingField = value;
		}
	}

	public WorldNodeData currentNode
	{
		get
		{
			return nodes[currentNodeIndex];
		}
		set
		{
			nodes[currentNodeIndex] = value;
		}
	}

	public HunterStatus currentNodeHunterStatus => ((IList<HunterStatus>)hunterStatuses).GetOrDefault(currentNodeIndex, HunterStatus.None);

	public bool isCurrentNodeHunted => currentNodeHunterStatus >= HunterStatus.Level1;

	public LoadNodeSettings lastLoadNodeSettings { get; private set; } = new LoadNodeSettings();

	public uint worldSeed
	{
		get
		{
			return _worldSeed;
		}
		set
		{
			_currentWorldRandomInstances.Clear();
			Network_worldSeed = value;
		}
	}

	[SaveVar(SaveVarFlags.Default)]
	public int hunterSkippedTurns
	{
		[CompilerGenerated]
		get
		{
			return hunterSkippedTurns__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003ChunterSkippedTurns_003Ek__BackingField = value;
		}
	}

	[SaveVar(SaveVarFlags.Default)]
	public int currentTurnIndex { get; private set; }

	public bool isHuntAdvanceDisabled
	{
		[CompilerGenerated]
		get
		{
			return isHuntAdvanceDisabled__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CisHuntAdvanceDisabled_003Ek__BackingField = value;
		}
	}

	[SaveVar(SaveVarFlags.Default)]
	public int hunterStartNodeIndex
	{
		[CompilerGenerated]
		get
		{
			return hunterStartNodeIndex__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003ChunterStartNodeIndex_003Ek__BackingField = value;
		}
	}

	[SaveVar(SaveVarFlags.Default)]
	public int loopIndex
	{
		[CompilerGenerated]
		get
		{
			return loopIndex__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CloopIndex_003Ek__BackingField = value;
		}
	}

	[SaveVar(SaveVarFlags.Default)]
	public int currentTier
	{
		[CompilerGenerated]
		get
		{
			return currentTier__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CcurrentTier_003Ek__BackingField = value;
		}
	}

	public bool isCurrentZoneHardVariant => hardVariantBossZoneIndices.Contains(currentZoneIndex);

	public SyncableAssetRef Network_currentZone
	{
		get
		{
			return _currentZone;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<SyncableAssetRef>(value, ref _currentZone, 1uL, (Action<SyncableAssetRef, SyncableAssetRef>)null);
		}
	}

	public Room Network_003CcurrentRoom_003Ek__BackingField
	{
		get
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			return ((NetworkBehaviour)this).GetSyncVarNetworkBehaviour<Room>(____003CcurrentRoom_003Ek__BackingFieldNetId, ref currentRoom__BackingField);
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter_NetworkBehaviour<Room>(value, ref currentRoom__BackingField, 2uL, (Action<Room, Room>)null, ref ____003CcurrentRoom_003Ek__BackingFieldNetId);
		}
	}

	public int Network_003CcurrentZoneIndex_003Ek__BackingField
	{
		get
		{
			return currentZoneIndex__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<int>(value, ref currentZoneIndex__BackingField, 4uL, (Action<int, int>)null);
		}
	}

	public int Network_003CcurrentRoomIndex_003Ek__BackingField
	{
		get
		{
			return currentRoomIndex__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<int>(value, ref currentRoomIndex__BackingField, 8uL, _Mirror_SyncVarHookDelegate__003CcurrentRoomIndex_003Ek__BackingField);
		}
	}

	public int Network_003CclearedCombatRooms_003Ek__BackingField
	{
		get
		{
			return clearedCombatRooms__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<int>(value, ref clearedCombatRooms__BackingField, 16uL, _Mirror_SyncVarHookDelegate__003CclearedCombatRooms_003Ek__BackingField);
		}
	}

	public int Network_003CcurrentZoneClearedNodes_003Ek__BackingField
	{
		get
		{
			return currentZoneClearedNodes__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<int>(value, ref currentZoneClearedNodes__BackingField, 32uL, (Action<int, int>)null);
		}
	}

	public bool Network_003CisInRoomTransition_003Ek__BackingField
	{
		get
		{
			return isInRoomTransition__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref isInRoomTransition__BackingField, 64uL, _Mirror_SyncVarHookDelegate__003CisInRoomTransition_003Ek__BackingField);
		}
	}

	public int Network_003CcurrentHuntLevel_003Ek__BackingField
	{
		get
		{
			return currentHuntLevel__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<int>(value, ref currentHuntLevel__BackingField, 128uL, _Mirror_SyncVarHookDelegate__003CcurrentHuntLevel_003Ek__BackingField);
		}
	}

	public bool Network_003CisVoting_003Ek__BackingField
	{
		get
		{
			return isVoting__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref isVoting__BackingField, 256uL, (Action<bool, bool>)null);
		}
	}

	public int Network_003CvoteRemainingSeconds_003Ek__BackingField
	{
		get
		{
			return voteRemainingSeconds__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<int>(value, ref voteRemainingSeconds__BackingField, 512uL, (Action<int, int>)null);
		}
	}

	public VoteType NetworkvoteType
	{
		get
		{
			return voteType;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<VoteType>(value, ref voteType, 1024uL, (Action<VoteType, VoteType>)null);
		}
	}

	public int Network_003CvoteData_003Ek__BackingField
	{
		get
		{
			return voteData__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<int>(value, ref voteData__BackingField, 2048uL, (Action<int, int>)null);
		}
	}

	public int Network_003CcurrentNodeIndex_003Ek__BackingField
	{
		get
		{
			return currentNodeIndex__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<int>(value, ref currentNodeIndex__BackingField, 4096uL, (Action<int, int>)null);
		}
	}

	public int Network_003CsidetrackReturnNodeIndex_003Ek__BackingField
	{
		get
		{
			return sidetrackReturnNodeIndex__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<int>(value, ref sidetrackReturnNodeIndex__BackingField, 8192uL, (Action<int, int>)null);
		}
	}

	public uint Network_worldSeed
	{
		get
		{
			return _worldSeed;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<uint>(value, ref _worldSeed, 16384uL, (Action<uint, uint>)null);
		}
	}

	public int Network_003ChunterSkippedTurns_003Ek__BackingField
	{
		get
		{
			return hunterSkippedTurns__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<int>(value, ref hunterSkippedTurns__BackingField, 32768uL, (Action<int, int>)null);
		}
	}

	public bool Network_003CisHuntAdvanceDisabled_003Ek__BackingField
	{
		get
		{
			return isHuntAdvanceDisabled__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref isHuntAdvanceDisabled__BackingField, 65536uL, (Action<bool, bool>)null);
		}
	}

	public int Network_003ChunterStartNodeIndex_003Ek__BackingField
	{
		get
		{
			return hunterStartNodeIndex__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<int>(value, ref hunterStartNodeIndex__BackingField, 131072uL, (Action<int, int>)null);
		}
	}

	public int Network_003CloopIndex_003Ek__BackingField
	{
		get
		{
			return loopIndex__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<int>(value, ref loopIndex__BackingField, 262144uL, (Action<int, int>)null);
		}
	}

	public int Network_003CcurrentTier_003Ek__BackingField
	{
		get
		{
			return currentTier__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<int>(value, ref currentTier__BackingField, 524288uL, (Action<int, int>)null);
		}
	}

	private void OnCurrentRoomIndexChanged(int _, int __)
	{
		ClientEvent_OnCurrentRoomIndexChanged?.Invoke();
	}

	private void OnClearedCombatRoomsChanged(int _, int __)
	{
		ClientEvent_OnClearedCombatRoomsChanged?.Invoke();
	}

	private void OnIsInTransitionChanged(bool oldVal, bool newVal)
	{
		try
		{
			ClientEvent_OnIsInTransitionChanged?.Invoke(newVal);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception, (UnityEngine.Object)(object)this);
		}
	}

	private void OnCurrentHuntLevelChanged(int _, int __)
	{
		ClientEvent_OnCurrentHuntLevelChanged?.Invoke();
	}

	public override void OnStart()
	{
		base.OnStart();
		DewResources.AddPreloadRule((MonoBehaviour)(object)this, (PreloadInterface preload) =>
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			Enumerator<WorldNodeData> enumerator = nodes.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					foreach (ModifierData modifier in enumerator.Current.modifiers)
					{
						preload.AddType(modifier.type);
					}
				}
			}
			finally
			{
				((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
			}
			if (currentZone != null && currentZone.defaultMonsters != null)
			{
				preload.AddFromMonsterPool(currentZone.defaultMonsters.pool);
			}
		});
	}

	private void OnLoopStart()
	{
		Debug.Log("New loop has started");
		bannedSidetracksForCurrentLoop.Clear();
		bannedRoomModifiersForCurrentLoop.Clear();
		SelectHardVariantBossZones();
		InvokeOnLoopStarted();
	}

	public List<RoomModifierBase> LoadModifierLightPrefabsOfCurrentZone()
	{
		List<RoomModifierBase> list = new List<RoomModifierBase>();
		if (currentZone.disableRoomModifiers)
		{
			return list;
		}
		foreach (RoomModifierBase item in DewResources.FindAllByTypeSubstring<RoomModifierBase>("RoomMod_", ResourceLoadSettings.Light))
		{
			if (Dew.IsRoomModifierIncludedInGame(((object)item).GetType().Name) && !item.excludeFromPool && (item.allowedZones == null || item.allowedZones.Length == 0 || item.allowedZones.Contains(currentZone.name)) && item.IsAvailableInGame() && currentZoneIndex >= item.zoneIndexRange.x && currentZoneIndex <= item.zoneIndexRange.y && !bannedRoomModifiersForCurrentLoop.Contains(((object)item).GetType().Name))
			{
				list.Add(item);
			}
		}
		return list;
	}

	[ClientRpc]
	private void InvokeOnRoomLoadStarted(EventInfoLoadRoom info)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_EventInfoLoadRoom((NetworkWriter)(object)val, info);
		((NetworkBehaviour)this).SendRPCInternal("System.Void ZoneManager::InvokeOnRoomLoadStarted(EventInfoLoadRoom)", 514859420, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[ClientRpc]
	private void InvokeOnZoneLoadStarted(EventInfoLoadZone info)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_EventInfoLoadZone((NetworkWriter)(object)val, info);
		((NetworkBehaviour)this).SendRPCInternal("System.Void ZoneManager::InvokeOnZoneLoadStarted(EventInfoLoadZone)", -660047876, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[ClientRpc]
	private void InvokeOnRoomLoaded(EventInfoLoadRoom info)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_EventInfoLoadRoom((NetworkWriter)(object)val, info);
		((NetworkBehaviour)this).SendRPCInternal("System.Void ZoneManager::InvokeOnRoomLoaded(EventInfoLoadRoom)", 2041766036, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[ClientRpc]
	private void InvokeOnZoneLoaded(EventInfoLoadZone info)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_EventInfoLoadZone((NetworkWriter)(object)val, info);
		((NetworkBehaviour)this).SendRPCInternal("System.Void ZoneManager::InvokeOnZoneLoaded(EventInfoLoadZone)", 2089226802, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[ClientRpc]
	private void InvokeOnLoopStarted()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendRPCInternal("System.Void ZoneManager::InvokeOnLoopStarted()", 786369066, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	public void AddTravelToNodeInterrupt(Func<EventInfoTravelToNodeInterrupt, bool> func)
	{
		_travelToNodeInterrupts.Add(func);
	}

	public void RemoveTravelToNodeInterrupt(Func<EventInfoTravelToNodeInterrupt, bool> func)
	{
		_travelToNodeInterrupts.Remove(func);
	}

	private bool CheckTravelToNodeInterrupts(EventInfoTravelToNodeInterrupt info)
	{
		Func<EventInfoTravelToNodeInterrupt, bool>[] array = _travelToNodeInterrupts.ToArray();
		foreach (Func<EventInfoTravelToNodeInterrupt, bool> func in array)
		{
			try
			{
				if (func(info))
				{
					Debug.Log("Travel has been interrupt");
					return true;
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
		return false;
	}

	public void CallOnReadyAfterTransition(Action action)
	{
		bool wasCalledInTransition = isInAnyTransition;
		GameManager.CallOnReady(() =>
		{
			((MonoBehaviour)(object)this).StartCoroutine(Routine());
		});
		IEnumerator Routine()
		{
			if (wasCalledInTransition)
			{
				yield return new WaitForSeconds(UnityEngine.Random.Range(0.85f, 1.3f));
			}
			try
			{
				action?.Invoke();
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
	}

	[Server]
	public void TravelToZone(Zone prefab, bool noAdvance = false)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void ZoneManager::TravelToZone(Zone,System.Boolean)' called when server was not active");
			return;
		}
		LoadNode(new LoadNodeSettings
		{
			from = -1,
			to = 0,
			advanceTurn = true,
			newZoneNoAdvance = noAdvance,
			newZone = prefab,
			isTravelingZone = true,
			isTravelingRoom = true
		});
	}

	[Server]
	public void ReturnFromSidetracking()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void ZoneManager::ReturnFromSidetracking()' called when server was not active");
			return;
		}
		if (sidetrackReturnNodeIndex < 0)
		{
			throw new InvalidOperationException();
		}
		TravelToNode(sidetrackReturnNodeIndex, advanceTurn: false, isSidetrackTransition: true, ignoreInterrupts: true);
		Network_003CsidetrackReturnNodeIndex_003Ek__BackingField = -1;
	}

	[Server]
	public void LoadSidetrackRoom(string roomName)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void ZoneManager::LoadSidetrackRoom(System.String)' called when server was not active");
			return;
		}
		if (sidetrackReturnNodeIndex >= 0)
		{
			throw new InvalidOperationException();
		}
		int num = currentNodeIndex;
		int count = nodes.Count;
		AddSidetrackNode(roomName);
		Network_003CsidetrackReturnNodeIndex_003Ek__BackingField = num;
		TravelToNode(count, advanceTurn: false, isSidetrackTransition: true, ignoreInterrupts: true);
	}

	[Server]
	public void TravelToNode(int to, bool advanceTurn = true, bool isSidetrackTransition = false, bool ignoreInterrupts = false)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void ZoneManager::TravelToNode(System.Int32,System.Boolean,System.Boolean,System.Boolean)' called when server was not active");
		}
		else if (ignoreInterrupts || !CheckTravelToNodeInterrupts(new EventInfoTravelToNodeInterrupt
		{
			from = currentNodeIndex,
			to = to,
			newZone = null
		}))
		{
			LoadNode(new LoadNodeSettings
			{
				from = currentNodeIndex,
				to = to,
				advanceTurn = advanceTurn,
				isSidetrackTransition = isSidetrackTransition,
				isTravelingRoom = true
			});
		}
	}

	[Server]
	public void DoDeadEndTravel()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void ZoneManager::DoDeadEndTravel()' called when server was not active");
		}
		else
		{
			((MonoBehaviour)(object)this).StartCoroutine(Routine());
		}
		static IEnumerator Routine()
		{
			NetworkedManagerBase<GameManager>.instance.isGameTimePausedByGame = true;
			for (int i = 0; i < NetworkedManagerBase<ActorManager>.instance.allHeroes.Count; i++)
			{
				Hero h = NetworkedManagerBase<ActorManager>.instance.allHeroes[i];
				if (!h.IsNullInactiveDeadOrKnockedOut())
				{
					yield return new WaitForSeconds((i == 0) ? 0.25f : 0.15f);
					if (!h.IsNullInactiveDeadOrKnockedOut())
					{
						h.CreateStatusEffect<Se_PortalTransition>(h, default);
					}
				}
			}
			yield return new WaitForSeconds(0.45f);
			ManagerBase<FeedbackManager>.instance.PlayFeedbackEffect("UI_Game_RiftTransition");
		}
	}

	[Server]
	public void LoadNode(LoadNodeSettings s)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void ZoneManager::LoadNode(LoadNodeSettings)' called when server was not active");
		}
		else
		{
			((MonoBehaviour)(object)this).StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			if (isInRoomTransition)
			{
				throw new InvalidOperationException("Already in transition");
			}
			lastLoadNodeSettings = s;
			if (s.newZone == null && nodes[s.to].type == WorldNodeType.ExitBoss && NetworkedManagerBase<GameSettingsManager>.instance.midJoinWaitType == MidJoinWaitType.None)
			{
				NetworkedManagerBase<GameSettingsManager>.instance.midJoinWaitType = MidJoinWaitType.BeforeBossFight;
			}
			if (s.newZone == null && nodes[s.to].type == WorldNodeType.ExitBoss && NetworkedManagerBase<GameSettingsManager>.instance.midJoinBanType == MidJoinBanType.None && currentZone.name == "Zone_Primus")
			{
				NetworkedManagerBase<GameSettingsManager>.instance.midJoinBanType = MidJoinBanType.TooLateToJoin;
			}
			if (s.isLoadingFromSave && NetworkedManagerBase<GameSettingsManager>.instance.midJoinWaitType == MidJoinWaitType.None)
			{
				NetworkedManagerBase<GameSettingsManager>.instance.midJoinWaitType = MidJoinWaitType.HostLoadingGame;
			}
			bool isFirstTime = (UnityEngine.Object)(object)Network_003CcurrentRoom_003Ek__BackingField == null && !s.isLoadingFromSave;
			bool isSidetrackTransition = s.isSidetrackTransition;
			NetworkedManagerBase<GameManager>.instance.isGameTimePausedByGame = true;
			Network_003CisInRoomTransition_003Ek__BackingField = true;
			if (!s.isLoadingFromSave && (s.isTravelingZone || s.isTravelingRoom))
			{
				if (s.advanceTurn)
				{
					int num = currentRoomIndex;
					Network_003CcurrentRoomIndex_003Ek__BackingField = num + 1;
				}
				if (s.from >= 0 && nodes[s.from].type == WorldNodeType.Combat)
				{
					int num = clearedCombatRooms;
					Network_003CclearedCombatRooms_003Ek__BackingField = num + 1;
				}
			}
			if (!s.isSidetrackTransition && (s.isTravelingRoom || s.isTravelingZone))
			{
				Network_003CsidetrackReturnNodeIndex_003Ek__BackingField = -1;
			}
			Hero[] heroes = NetworkedManagerBase<ActorManager>.instance.allHeroes.ToArray();
			if (isFirstTime || s.isLoadingFromSave)
			{
				if (isFirstTime)
				{
					yield return new WaitForSecondsRealtime(0.5f);
				}
				if (s.isTravelingZone || s.isTravelingRoom)
				{
					for (int i = 0; i < heroes.Length; i++)
					{
						Hero h = heroes[i];
						yield return new WaitForSeconds((i == 0) ? 0.75f : 0.3f);
						if (!h.IsNullInactiveDeadOrKnockedOut())
						{
							Summon[] array = h.summons.ToArray();
							foreach (Summon summon in array)
							{
								if (!summon.IsNullInactiveDeadOrKnockedOut())
								{
									summon.CreateStatusEffect(summon, default, (Se_PortalTransition p) =>
									{
										p.playDisappearEffect = false;
									});
									yield return new WaitForSeconds(Mathf.Min(0.5f / (float)h.summons.Count, 0.3f));
								}
							}
							if (!h.IsNullInactiveDeadOrKnockedOut())
							{
								h.CreateStatusEffect(h, default, (Se_PortalTransition p) =>
								{
									p.playDisappearEffect = false;
								});
							}
						}
					}
				}
				yield return new WaitForSecondsRealtime(1f);
			}
			else
			{
				if (!isSidetrackTransition && !s.isWhiteTransition && !s.dontDoRiftTransition)
				{
					for (int i = 0; i < heroes.Length; i++)
					{
						Hero h = heroes[i];
						if (!h.IsNullInactiveDeadOrKnockedOut())
						{
							yield return new WaitForSeconds((i == 0) ? 0.25f : 0.15f);
							if (!h.IsNullInactiveDeadOrKnockedOut())
							{
								Summon[] array = h.summons.ToArray();
								foreach (Summon summon2 in array)
								{
									if (!summon2.IsNullInactiveDeadOrKnockedOut())
									{
										summon2.CreateStatusEffect<Se_PortalTransition>(summon2, default);
										yield return new WaitForSeconds(Mathf.Min(0.5f / (float)h.summons.Count, 0.3f));
									}
								}
								if (!h.IsNullInactiveDeadOrKnockedOut())
								{
									h.CreateStatusEffect<Se_PortalTransition>(h, default);
								}
							}
						}
					}
					yield return new WaitForSeconds(0.45f);
				}
				if (s.isWhiteTransition)
				{
					DewNetworkManager.instance.SetWhiteLoadingStatus(isLoading: true, DewPlayer.gamePlayers);
				}
				else
				{
					DewNetworkManager.instance.SetLoadingStatus(isLoading: true, DewPlayer.gamePlayers);
				}
				yield return new WaitForSecondsRealtime(ManagerBase<TransitionManager>.instance.fadeTime);
			}
			EventInfoLoadZone loadZoneInfo = default;
			EventInfoLoadRoom loadRoomInfo = new EventInfoLoadRoom
			{
				fromIndex = s.from,
				toIndex = s.to,
				isSidetrackTransition = s.isSidetrackTransition,
				isLoadingFromSave = s.isLoadingFromSave,
				isTraveling = s.isTravelingRoom
			};
			try
			{
				if ((UnityEngine.Object)(object)SingletonDewNetworkBehaviour<Room>.instance != null)
				{
					if (s.from >= 0 && visitedNodesSaveData[s.from] == null)
					{
						int num = currentZoneClearedNodes;
						Network_003CcurrentZoneClearedNodes_003Ek__BackingField = num + 1;
					}
					DewPersistence.RoomData value = DewPersistence.SerializeRoomData();
					SingletonDewNetworkBehaviour<Room>.instance.StopRoom();
					if (s.from >= 0)
					{
						visitedNodesSaveData[s.from] = value;
					}
				}
				if (s.newZone != null)
				{
					loadZoneInfo = new EventInfoLoadZone
					{
						from = ((currentZone != null) ? currentZone.name : ""),
						to = s.newZone.name,
						isLoadingFromSave = s.isLoadingFromSave,
						isTraveling = s.isTravelingZone
					};
					InvokeOnZoneLoadStarted(loadZoneInfo);
				}
				InvokeOnRoomLoadStarted(loadRoomInfo);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
			yield return new WaitForSecondsRealtime(0.1f);
			if (s.newZone != null)
			{
				yield return new WaitForSecondsRealtime(0.2f);
			}
			try
			{
				Hero[] array2 = heroes;
				foreach (Hero hero in array2)
				{
					if (!hero.IsNullOrInactive())
					{
						hero.Control.Teleport(new Vector3(-5000f, -5000f, 0f));
					}
				}
			}
			catch (Exception exception2)
			{
				Debug.LogException(exception2);
			}
			try
			{
				foreach (Actor item in new List<Actor>(NetworkedManagerBase<ActorManager>.instance.allActors))
				{
					if (item.isDestroyedOnRoomChange && item.isActive)
					{
						item.Destroy();
					}
				}
			}
			catch (Exception exception3)
			{
				Debug.LogException(exception3);
			}
			try
			{
				if (s.newZone != null)
				{
					if (!s.isLoadingFromSave && currentZone == null)
					{
						Network_003CcurrentRoomIndex_003Ek__BackingField = 0;
						Network_003CcurrentZoneIndex_003Ek__BackingField = -1;
						OnLoopStart();
					}
					if (!s.isLoadingFromSave && s.isTravelingZone)
					{
						Network_003CcurrentZoneClearedNodes_003Ek__BackingField = 0;
						_hunterSpreadCredit = 0f;
					}
					currentZone = s.newZone;
					if (s.isTravelingZone && !s.newZoneNoAdvance && !s.isLoadingFromSave)
					{
						int num = currentZoneIndex;
						Network_003CcurrentZoneIndex_003Ek__BackingField = num + 1;
						NetworkedManagerBase<GameManager>.instance.SetAmbientLevel(currentZoneIndex + 1);
					}
					if (s.isTravelingZone)
					{
						foreach (DewPlayer gamePlayer in DewPlayer.gamePlayers)
						{
							if (!gamePlayer.hero.IsNullOrInactive() && gamePlayer.hero.isKnockedOut && gamePlayer.hero.Status.TryGetStatusEffect<Se_HeroKnockedOut>(out var effect))
							{
								effect.Destroy();
								gamePlayer.hero.Status.SetHealth(gamePlayer.hero.maxHealth);
							}
						}
					}
					if (s.isTravelingZone && !s.isLoadingFromSave)
					{
						GenerateWorldAuto();
					}
				}
			}
			catch (Exception exception4)
			{
				Debug.LogException(exception4);
			}
			try
			{
				DewRandom worldRandom = GetWorldRandom(-84737);
				DewRandom worldRandom2 = GetWorldRandom(-84725);
				DewRandom worldRandom3 = GetWorldRandom(-84711);
				if (nodes[s.to].room == null)
				{
					if (nodes[s.to].type == WorldNodeType.Merchant)
					{
						if (_shopRoomPool.Count == 0)
						{
							_shopRoomPool.AddRange(currentZone.shopRooms);
						}
						int index = worldRandom.Range(0, _shopRoomPool.Count);
						SetRoom(s.to, _shopRoomPool[index]);
						_shopRoomPool.RemoveAt(index);
					}
					else
					{
						if (_combatRoomPool.Count == 0)
						{
							_combatRoomPool.AddRange(currentZone.combatRooms);
						}
						int index2 = worldRandom2.Range(0, _combatRoomPool.Count);
						SetRoom(s.to, _combatRoomPool[index2]);
						_combatRoomPool.RemoveAt(index2);
					}
				}
				if (s.isTravelingRoom && !currentZone.disableRoomModifiers && nodes[s.to].type == WorldNodeType.Combat && !nodes[s.to].HasModifier<RoomMod_SpawnMiniBoss>() && visitedNodesSaveData[s.to] == null)
				{
					float num2 = NetworkedManagerBase<GameManager>.instance.ges.miniBossSpawnChanceByNonMiniBossCombatNodesVisited.Evaluate(_visitedCombatNodesWithoutMiniBoss);
					if (!nodes[s.to].HasMainModifier() && worldRandom3.Value() < num2)
					{
						_visitedCombatNodesWithoutMiniBoss = 0;
						AddModifier<RoomMod_SpawnMiniBoss>(s.to);
					}
					else
					{
						_visitedCombatNodesWithoutMiniBoss++;
					}
				}
			}
			catch (Exception exception5)
			{
				Debug.LogException(exception5);
			}
			yield return null;
			try
			{
				if (!s.isLoadingFromSave)
				{
					if (s.advanceTurn)
					{
						int num = currentTurnIndex;
						currentTurnIndex = num + 1;
						if (s.newZone == null)
						{
							AdvanceHunterTurn();
						}
					}
					if (!isSidetrackTransition)
					{
						UpdateModifiersByHunterStatus(s.to);
					}
				}
			}
			catch (Exception exception6)
			{
				Debug.LogException(exception6);
			}
			if (!s.isLoadingFromSave)
			{
				if (isFirstTime)
				{
					DewSave.profileContinue.continueData = null;
					DewSave.SaveProfileContinue();
				}
				else
				{
					NetworkedManagerBase<GameManager>.instance.SaveContinueData(s);
				}
			}
			try
			{
				SetCurrentNodeIndexAndRevealAdjacent(s.to);
			}
			catch (Exception exception7)
			{
				Debug.LogException(exception7);
			}
			DewPersistence.RoomData saveData = ((s.to >= 0) ? visitedNodesSaveData[s.to] : null);
			JsonSerializerSettings applyRoomDataSettings = DewPersistence.GetNewSettings();
			string sceneName = (string.IsNullOrEmpty(nodes[s.to].roomOverride) ? nodes[s.to].room : nodes[s.to].roomOverride);
			yield return DewNetworkManager.instance.LoadSceneAsync(sceneName, DewPlayer.gamePlayers, () =>
			{
				if ((UnityEngine.Object)(object)SingletonDewNetworkBehaviour<Room>.instance != null)
				{
					SingletonDewNetworkBehaviour<Room>.instance.isRevisit = saveData != null;
				}
				if (saveData != null)
				{
					DewPersistence.ApplyRoomDataBeforeSpawnObjects(saveData, applyRoomDataSettings);
				}
			});
			Room newRoom = UnityEngine.Object.FindObjectOfType<Room>(true);
			if ((UnityEngine.Object)(object)newRoom == null)
			{
				throw new Exception("Could not find room in loaded scene: " + SceneManager.GetActiveScene().name);
			}
			Network_003CcurrentRoom_003Ek__BackingField = newRoom;
			if (isCurrentNodeHunted && s.isTravelingRoom)
			{
				int num = currentHuntLevel;
				Network_003CcurrentHuntLevel_003Ek__BackingField = num + 1;
			}
			yield return new WaitUntil(() => NetworkClient.ready);
			Debug.Log("TravelToNode: Start waiting for clients");
			yield return Dew.WaitForClientsReadyRoutine(DewPlayer.gamePlayers);
			Debug.Log("TravelToNode: All clients are ready");
			if (saveData != null)
			{
				try
				{
					DewPersistence.ApplyRoomDataAfterSpawnObjects(saveData, applyRoomDataSettings);
				}
				catch (Exception exception8)
				{
					Debug.LogException(exception8);
				}
				yield return new WaitForSecondsRealtime(0.25f);
			}
			try
			{
				SingletonDewNetworkBehaviour<Room>.instance.StartRoom();
				InvokeOnRoomLoaded(loadRoomInfo);
				Vector3 vector;
				if (Rift_RoomExit.instance.IsNullOrInactive() && Rift.instance.IsNullOrInactive())
				{
					vector = Vector3.zero;
				}
				else
				{
					Transform transform = ((!Rift_RoomExit.instance.IsNullOrInactive()) ? ((Component)(object)Rift_RoomExit.instance).transform : ((Component)(object)Rift.instance).transform);
					vector = Dew.GetValidAgentDestination_Closest(Dew.GetValidAgentPosition(transform.position), transform.position + transform.forward * 3.5f);
				}
				Hero[] array2 = heroes;
				foreach (Hero hero2 in array2)
				{
					if (!hero2.IsNullOrInactive() && !hero2.isKnockedOut)
					{
						if (saveData != null)
						{
							Vector3 validAgentDestination_Closest = Dew.GetValidAgentDestination_Closest(vector, Dew.GetPositionOnGround(vector + UnityEngine.Random.onUnitSphere.Flattened() * 3f));
							hero2.Control.Teleport(validAgentDestination_Closest);
						}
						else
						{
							hero2.Control.Teleport(newRoom.GetHeroSpawnPosition());
						}
						foreach (Summon summon4 in hero2.summons)
						{
							summon4.Control.Teleport(Dew.GetValidAgentDestination_LinearSweep(hero2.agentPosition, Dew.GetPositionOnGround(hero2.agentPosition + UnityEngine.Random.onUnitSphere.Flattened() * 3f)));
						}
					}
				}
				SingletonDewNetworkBehaviour<Room>.instance.SyncCameraAngle();
			}
			catch (Exception exception9)
			{
				Debug.LogException(exception9);
			}
			yield return new WaitForSeconds(0.15f);
			if (isFirstTime)
			{
				NetworkedManagerBase<GameManager>.instance.elapsedGameTime = 0f;
			}
			if (s.newZone != null)
			{
				foreach (DewPlayer gamePlayer2 in DewPlayer.gamePlayers)
				{
					if (!gamePlayer2.hero.IsNullOrInactive())
					{
						gamePlayer2.hero.Control.lastMoveTime = float.NegativeInfinity;
					}
				}
				NetworkedManagerBase<GameManager>.instance.isGameTimePausedByAfk = true;
			}
			if (!s.isLoadingFromSave && !s.dontStopLoading)
			{
				if (s.isWhiteTransition)
				{
					DewNetworkManager.instance.SetWhiteLoadingStatus(isLoading: false, DewPlayer.gamePlayers);
				}
				else
				{
					DewNetworkManager.instance.SetLoadingStatus(isLoading: false, DewPlayer.gamePlayers);
				}
				yield return new WaitForSeconds(ManagerBase<TransitionManager>.instance.fadeTime - 0.15f + (s.isLoadingFromSave ? 0.45f : 0f));
			}
			for (int i = 0; i < heroes.Length; i++)
			{
				Hero h = heroes[i];
				if (!h.IsNullOrInactive())
				{
					Summon[] array = h.summons.ToArray();
					foreach (Summon summon3 in array)
					{
						if (!summon3.IsNullInactiveDeadOrKnockedOut() && summon3.Status.TryGetStatusEffect<Se_PortalTransition>(out var effect2))
						{
							effect2.Destroy();
							h.CreateBasicEffect(h, new UntargetableEffect(), 2f);
							h.CreateBasicEffect(h, new InvisibleEffect
							{
								ignoreReveal = true
							}, 2f);
							yield return new WaitForSeconds(Mathf.Min(0.4f / (float)h.summons.Count, 0.25f));
						}
					}
					if (h.Status.TryGetStatusEffect<Se_PortalTransition>(out var effect3))
					{
						effect3.Destroy();
						h.CreateBasicEffect(h, new UntargetableEffect(), 2f);
						h.CreateBasicEffect(h, new InvisibleEffect
						{
							ignoreReveal = true
						}, 2f);
						if (i != heroes.Length - 1)
						{
							yield return new WaitForSeconds(0.15f);
						}
					}
				}
			}
			Network_003CisInRoomTransition_003Ek__BackingField = false;
			NetworkedManagerBase<GameManager>.instance.isGameTimePausedByGame = false;
			NetworkedManagerBase<GameManager>.instance.spawnedPopulation = 0f;
			if (s.isLoadingFromSave && NetworkedManagerBase<GameSettingsManager>.instance.midJoinWaitType == MidJoinWaitType.HostLoadingGame)
			{
				NetworkedManagerBase<GameSettingsManager>.instance.midJoinWaitType = MidJoinWaitType.None;
			}
			if (nodes[s.to].type != WorldNodeType.ExitBoss && nodes[s.to].type != WorldNodeType.Special && NetworkedManagerBase<GameSettingsManager>.instance.midJoinWaitType >= MidJoinWaitType.BeforeBossFight && NetworkedManagerBase<GameSettingsManager>.instance.midJoinWaitType <= MidJoinWaitType.AfterBossFight)
			{
				NetworkedManagerBase<GameSettingsManager>.instance.midJoinWaitType = MidJoinWaitType.None;
			}
			if (s.newZone != null)
			{
				InvokeOnZoneLoaded(loadZoneInfo);
				if (isCurrentZoneHardVariant)
				{
					CallOnReadyAfterTransition(() =>
					{
						NetworkedManagerBase<ChatManager>.instance.BroadcastMessage(new ChatManager.Message
						{
							type = ChatManager.MessageType.Notice,
							content = "Chat_Notice_HardBossZone"
						});
					});
				}
			}
		}
	}

	public bool ShouldVoteOnTravel()
	{
		if (forceVoteForDebug.HasValue)
		{
			return forceVoteForDebug.Value;
		}
		if (NetworkedManagerBase<GameSettingsManager>.instance.enableVotes)
		{
			return DewPlayer.gamePlayers.Count((DewPlayer p) => !p.hero.IsNullInactiveDeadOrKnockedOut()) > 1;
		}
		return false;
	}

	[Server]
	public void StartVoteNextNode(DewPlayer player, int nextNodeIndex)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void ZoneManager::StartVoteNextNode(DewPlayer,System.Int32)' called when server was not active");
		}
		else
		{
			StartVote_Imp(player, VoteType.NextNode, nextNodeIndex);
		}
	}

	[Server]
	public void StartVoteNextZone(DewPlayer player)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void ZoneManager::StartVoteNextZone(DewPlayer)' called when server was not active");
		}
		else
		{
			StartVote_Imp(player, VoteType.NextZone, -1);
		}
	}

	[Server]
	public void StartVoteSidetrack(DewPlayer player, Rift_Sidetrack rift)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void ZoneManager::StartVoteSidetrack(DewPlayer,Rift_Sidetrack)' called when server was not active");
		}
		else
		{
			StartVote_Imp(player, VoteType.Sidetrack, (int)((NetworkBehaviour)rift).netId);
		}
	}

	private void StartVote_Imp(DewPlayer player, VoteType type, int data)
	{
		if (isVoting)
		{
			return;
		}
		foreach (DewPlayer gamePlayer in DewPlayer.gamePlayers)
		{
			gamePlayer.isReady = (UnityEngine.Object)(object)player == (UnityEngine.Object)(object)gamePlayer;
		}
		Network_003CisVoting_003Ek__BackingField = true;
		RpcInvokeVoteStarted(player);
		RpcShowVoteChatMessage(isStart: true, player);
		Network_003CvoteRemainingSeconds_003Ek__BackingField = 6;
		Network_003CvoteData_003Ek__BackingField = data;
		NetworkvoteType = type;
		_voteRoutine = ((MonoBehaviour)(object)this).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			while (voteRemainingSeconds > 0)
			{
				yield return new WaitForSeconds(1f);
				voteRemainingSeconds--;
				if (voteRemainingSeconds <= 0)
				{
					break;
				}
			}
			WaitAndCompleteVote();
			_voteRoutine = null;
		}
	}

	[Server]
	internal void UpdateVoteStatus()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void ZoneManager::UpdateVoteStatus()' called when server was not active");
		}
		else if (isVoting)
		{
			if (_voteCheckRoutine != null)
			{
				((MonoBehaviour)(object)this).StopCoroutine(_voteCheckRoutine);
				_voteCheckRoutine = null;
			}
			_voteCheckRoutine = ((MonoBehaviour)(object)this).StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			yield return new WaitForSeconds(0.4f);
			if (isVoting && DewPlayer.gamePlayers.All((DewPlayer p) => p.isReady || p.hero.IsNullInactiveDeadOrKnockedOut()))
			{
				WaitAndCompleteVote();
			}
			_voteCheckRoutine = null;
		}
	}

	[Command(requiresAuthority = false)]
	public void CmdCancelVote(NetworkConnectionToClient sender = null)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendCommandInternal("System.Void ZoneManager::CmdCancelVote(Mirror.NetworkConnectionToClient)", 539255647, (NetworkWriter)(object)val, 0, false);
		NetworkWriterPool.Return(val);
	}

	private void WaitAndCompleteVote()
	{
		if (isVoting)
		{
			if (_voteCompleteRoutine != null)
			{
				((MonoBehaviour)(object)this).StopCoroutine(_voteCompleteRoutine);
			}
			if (_voteCheckRoutine != null)
			{
				((MonoBehaviour)(object)this).StopCoroutine(_voteCheckRoutine);
				_voteCheckRoutine = null;
			}
			_voteCompleteRoutine = ((MonoBehaviour)(object)this).StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			float lastCannotTravelMessageShowTime = float.NegativeInfinity;
			if (GetCannotTravelReason().shouldCancel)
			{
				RpcShowCannotTravelMessage();
				RpcInvokeVoteCanceled(DewPlayer.local);
				ClearVoteState();
			}
			else
			{
				while (GetCannotTravelReason().reasonText != null)
				{
					if (Time.time - lastCannotTravelMessageShowTime > 4f)
					{
						RpcShowCannotTravelMessage();
						lastCannotTravelMessageShowTime = Time.time;
					}
					yield return new WaitForSeconds(0.25f);
				}
				try
				{
					switch (voteType)
					{
					case VoteType.NextNode:
						TravelToNode(voteData);
						break;
					case VoteType.NextZone:
						NetworkedManagerBase<GameManager>.instance.LoadNextZone();
						break;
					case VoteType.Sidetrack:
					{
						Rift_Sidetrack voteSidetrackRift = GetVoteSidetrackRift();
						if ((UnityEngine.Object)(object)voteSidetrackRift != null)
						{
							voteSidetrackRift.TravelImmediately();
						}
						break;
					}
					default:
						throw new ArgumentOutOfRangeException();
					case VoteType.None:
						break;
					}
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
				}
				ClearVoteState();
				RpcInvokeVoteCompleted();
			}
		}
	}

	public Rift_Sidetrack GetVoteSidetrackRift()
	{
		if (voteData < 0)
		{
			return null;
		}
		if (!NetworkClient.spawned.TryGetValue((uint)voteData, out var value))
		{
			return null;
		}
		return ((Component)(object)value).GetComponent<Rift_Sidetrack>();
	}

	private void ClearVoteState()
	{
		Network_003CvoteData_003Ek__BackingField = -1;
		Network_003CisVoting_003Ek__BackingField = false;
		if (_voteRoutine != null)
		{
			((MonoBehaviour)(object)this).StopCoroutine(_voteRoutine);
			_voteRoutine = null;
		}
		if (_voteCheckRoutine != null)
		{
			((MonoBehaviour)(object)this).StopCoroutine(_voteCheckRoutine);
			_voteCheckRoutine = null;
		}
		if (_voteCompleteRoutine != null)
		{
			((MonoBehaviour)(object)this).StopCoroutine(_voteCompleteRoutine);
			_voteCompleteRoutine = null;
		}
	}

	[ClientRpc]
	private void RpcShowVoteChatMessage(bool isStart, DewPlayer player)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteBool((NetworkWriter)(object)val, isStart);
		NetworkWriterExtensions.WriteNetworkBehaviour((NetworkWriter)(object)val, (NetworkBehaviour)(object)player);
		((NetworkBehaviour)this).SendRPCInternal("System.Void ZoneManager::RpcShowVoteChatMessage(System.Boolean,DewPlayer)", 160450285, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[ClientRpc]
	private void RpcShowCannotTravelMessage()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendRPCInternal("System.Void ZoneManager::RpcShowCannotTravelMessage()", 1307992230, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[ClientRpc]
	private void RpcInvokeVoteStarted(DewPlayer player)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteNetworkBehaviour((NetworkWriter)(object)val, (NetworkBehaviour)(object)player);
		((NetworkBehaviour)this).SendRPCInternal("System.Void ZoneManager::RpcInvokeVoteStarted(DewPlayer)", -2004448797, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[ClientRpc]
	private void RpcInvokeVoteCanceled(DewPlayer player)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteNetworkBehaviour((NetworkWriter)(object)val, (NetworkBehaviour)(object)player);
		((NetworkBehaviour)this).SendRPCInternal("System.Void ZoneManager::RpcInvokeVoteCanceled(DewPlayer)", -2054120585, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[ClientRpc]
	private void RpcInvokeVoteCompleted()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendRPCInternal("System.Void ZoneManager::RpcInvokeVoteCompleted()", 1605286274, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[Server]
	public DewRandom GetWorldRandom(int offset)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'DewRandom ZoneManager::GetWorldRandom(System.Int32)' called when server was not active");
			return null;
		}
		if (!_currentWorldRandomInstances.TryGetValue((uint)(worldSeed + offset), out var value))
		{
			value = new DewRandom((uint)(worldSeed + offset));
			_currentWorldRandomInstances.Add((uint)(worldSeed + offset), value);
		}
		return value;
	}

	private void Start()
	{
		nodes.Callback += (Operation<WorldNodeData> op, int index, WorldNodeData item, WorldNodeData newItem) =>
		{
			_needToCallNodesChanged = true;
		};
	}

	private void LateUpdate()
	{
		if (_needToCallNodesChanged)
		{
			_needToCallNodesChanged = false;
			ClientEvent_OnNodesChanged?.Invoke();
		}
	}

	public float GetHunterProgress()
	{
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < nodes.Count; i++)
		{
			if (!nodes[i].IsSidetrackNode() && nodes[i].type != WorldNodeType.ExitBoss)
			{
				num++;
				if (hunterStatuses[i] != HunterStatus.None)
				{
					num2++;
				}
			}
		}
		return (float)num2 / (float)num;
	}

	public void TravelWithValidationAndConfirmation(Action action, bool ignoreSidetrack = false)
	{
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c1: Unknown result type (might be due to invalid IL or missing references)
		if ((UnityEngine.Object)(object)DewPlayer.local == null || DewPlayer.local.hero.IsNullInactiveDeadOrKnockedOut())
		{
			return;
		}
		if (isVoting)
		{
			InGameUIManager.instance.ShowCenterMessage(CenterMessageType.Error, "InGame_Message_AlreadyVotingForTravel");
			return;
		}
		if (!ShouldVoteOnTravel())
		{
			(string, bool) cannotTravelReason = GetCannotTravelReason();
			if (cannotTravelReason.Item1 != null)
			{
				InGameUIManager.instance.ShowCenterMessageRaw(CenterMessageType.Error, cannotTravelReason.Item1);
				return;
			}
		}
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		try
		{
			foreach (DewPlayer gamePlayer in DewPlayer.gamePlayers)
			{
				if (gamePlayer.isReadingArtifactStory && !gamePlayer.hero.IsNullInactiveDeadOrKnockedOut())
				{
					string uIValue = DewLocalization.GetUIValue("InGame_Message_ReadingArtifactStory");
					InGameUIManager.instance.ShowCenterMessageRaw(CenterMessageType.Error, string.Format(uIValue, ChatManager.GetColoredDescribedPlayerName(gamePlayer)));
					return;
				}
			}
			foreach (Actor allActor in NetworkedManagerBase<ActorManager>.instance.allActors)
			{
				if (allActor.IsNullOrInactive())
				{
					continue;
				}
				if (allActor is IRewardActor)
				{
					if (allActor is Shrine { isAvailable: false })
					{
						continue;
					}
					flag = true;
				}
				if (allActor is Shrine_Stardust shrine_Stardust)
				{
					Vector3 vector = Vector3.zero;
					if (!Rift_RoomExit.instance.IsNullOrInactive())
					{
						vector = ((Component)(object)Rift_RoomExit.instance).transform.position;
					}
					else if (!Rift_Sidetrack.instance.IsNullOrInactive())
					{
						vector = ((Component)(object)Rift_Sidetrack.instance).transform.position;
					}
					if (vector != Vector3.zero && Vector3.Distance(vector, shrine_Stardust.position) < 12f)
					{
						flag = true;
					}
				}
				if (allActor is SkillTrigger skillTrigger && (UnityEngine.Object)(object)skillTrigger.owner == null && (UnityEngine.Object)(object)skillTrigger.handOwner == null && !Rift.instance.IsNullOrInactive() && (int)Dew.GetNavMeshPathStatus(Dew.GetValidAgentPosition(Dew.GetPositionOnGround(((Component)(object)Rift.instance).transform.position)), Dew.GetValidAgentPosition(Dew.GetPositionOnGround(skillTrigger.position))) == 0)
				{
					flag3 = true;
					InGameUIManager.instance.ShowCenterMessageRaw(CenterMessageType.Error, GetCannotTravelReasonByDroppedSkill(skillTrigger));
					return;
				}
				if (allActor is Gem gem && (UnityEngine.Object)(object)gem.owner == null && (UnityEngine.Object)(object)gem.handOwner == null && !Rift.instance.IsNullOrInactive() && (int)Dew.GetNavMeshPathStatus(Dew.GetValidAgentPosition(Dew.GetPositionOnGround(((Component)(object)Rift.instance).transform.position)), Dew.GetValidAgentPosition(Dew.GetPositionOnGround(gem.position))) == 0)
				{
					flag4 = true;
					InGameUIManager.instance.ShowCenterMessageRaw(CenterMessageType.Error, GetCannotTravelReasonByDroppedGem(gem));
					return;
				}
				if (allActor is Shrine_HeroSoul { isAvailable: not false })
				{
					flag2 = true;
				}
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		string text = "";
		if (flag2)
		{
			text = text + DewLocalization.GetUIValue("InGame_Message_DidntSaveLostSoul") + "\n";
		}
		if (flag)
		{
			text = text + DewLocalization.GetUIValue("InGame_Message_UnclaimedReward") + "\n";
		}
		if (flag3)
		{
			text = text + DewLocalization.GetUIValue("InGame_Message_HasMemoryOnGround") + "\n";
		}
		if (flag4)
		{
			text = text + DewLocalization.GetUIValue("InGame_Message_HasEssenceOnGround") + "\n";
		}
		if (text == "")
		{
			action();
			return;
		}
		DewMessageSettings msg = new DewMessageSettings
		{
			buttons = (DewMessageSettings.ButtonType.Yes | DewMessageSettings.ButtonType.No),
			defaultButton = DewMessageSettings.ButtonType.No,
			destructiveConfirm = true,
			onClose = (DewMessageSettings.ButtonType b) =>
			{
				if (b == DewMessageSettings.ButtonType.Yes)
				{
					action();
				}
			},
			validator = () => InGameUIManager.ValidateInGameActionMessage(),
			rawContent = text + DewLocalization.GetUIValue("InGame_Message_DoYouWishToContinue")
		};
		ManagerBase<MessageManager>.instance.ShowMessage(msg);
	}

	public void GenerateWorldAuto()
	{
		if (worldSeed == 0)
		{
			worldSeed = DewRandom.GetRandomSeed();
		}
		else
		{
			worldSeed = GetWorldRandom(-999).NextUInt32();
		}
		if (DewBuildProfile.current.HasFeature(BuildFeatureTag.Booth))
		{
			worldSeed = 16u;
		}
		GenerateWorld_Imp();
	}

	public void GenerateWorldWithSeed(uint seed)
	{
		worldSeed = seed;
		GenerateWorld_Imp();
	}

	private void GenerateWorld_Imp()
	{
		List<RoomModifierBase> list = LoadModifierLightPrefabsOfCurrentZone();
		uint num = worldSeed;
		int num2 = new DewRandom(num + 123).Range(currentZone.numOfNodes.x, currentZone.numOfNodes.y + 1) + DewBuildProfile.current.worldNodeCountOffset;
		if (currentZone.useSpecialGeneration)
		{
			num2 = currentZone.specialNodes;
		}
		float minDistanceBetweenNode = 375f / (float)num2;
		float maxDistanceToNearestNode = 750f / (float)num2;
		float adjacentThreshold = 900f / (float)num2;
		List<Vector2> nodesTemp = new List<Vector2>();
		List<int> nodeDistanceMatrixTemp = new List<int>();
		List<bool> adjacentMatrix = new List<bool>();
		float num3 = float.NegativeInfinity;
		List<Vector2> list2 = new List<Vector2>();
		int bestExitNode = -1;
		List<int> list3 = new List<int>();
		float[] array = new float[8] { -3200f, -3200f, -800f, 1200f, 1200f, 0f, -3200f, -3200f };
		bool isRandomAlgorithm = false;
		int num4 = 0;
		int num5 = 0;
		float num6 = 0f;
		float num7 = 0f;
		int startNodeMarginDistance = 1;
		if (!isRandomAlgorithm && !currentZone.useSpecialGeneration)
		{
			float num8 = Mathf.Sqrt((float)num2 / 2f);
			num4 = Mathf.RoundToInt(num8 * 2f);
			num5 = Mathf.RoundToInt(num8);
			num2 = num5 * num4;
			num6 = WorldWidth / (float)num4;
			num7 = WorldHeight / (float)num5;
			adjacentThreshold = (num6 + num7) * 0.65f;
			minDistanceBetweenNode = Mathf.Min(num6, num7) * 0.55f;
		}
		bool[] isIsolated;
		bool[] isExplored;
		if (currentZone.useSpecialGeneration)
		{
			for (int i = 0; i < num2; i++)
			{
				list2.Add(new Vector2(WorldWidth / 2f, WorldHeight / 2f));
			}
			nodesTemp.AddRange(list2);
			bestExitNode = num2 - 1;
			UpdateAdjacent();
			CalculateDistance(bestExitNode);
			list3.AddRange(nodeDistanceMatrixTemp);
		}
		else
		{
			isIsolated = new bool[num2];
			isExplored = new bool[num2];
			for (int j = 0; j < WorldGenerationIterations; j++)
			{
				DewRandom dewRandom = new DewRandom((uint)(num + j * 1000));
				float num9 = 0f;
				nodesTemp.Clear();
				if (isRandomAlgorithm)
				{
					Vector2 vector = new Vector2(20f, 50f);
					if (dewRandom.Value() > 0.5f)
					{
						nodesTemp.Add(new Vector2(dewRandom.Range(vector.x, vector.y), dewRandom.Range(vector.x, vector.y)));
						nodesTemp.Add(new Vector2(WorldWidth - dewRandom.Range(vector.x, vector.y), WorldHeight - dewRandom.Range(vector.x, vector.y)));
					}
					else
					{
						nodesTemp.Add(new Vector2(WorldWidth - dewRandom.Range(vector.x, vector.y), dewRandom.Range(vector.x, vector.y)));
						nodesTemp.Add(new Vector2(dewRandom.Range(vector.x, vector.y), WorldHeight - dewRandom.Range(vector.x, vector.y)));
					}
					if (dewRandom.Value() < 0.5f)
					{
						List<Vector2> list4 = nodesTemp;
						List<Vector2> list5 = nodesTemp;
						Vector2 vector2 = nodesTemp[1];
						Vector2 vector3 = nodesTemp[0];
						Vector2 vector4 = (list4[0] = vector2);
						vector4 = (list5[1] = vector3);
					}
					int count = nodesTemp.Count;
					for (int k = 0; k < num2 - count; k++)
					{
						GetRandomNodePosWithMinDistance(out var candidate, dewRandom);
						nodesTemp.Add(candidate);
					}
				}
				else
				{
					for (int l = 0; l < num4; l++)
					{
						for (int m = 0; m < num5; m++)
						{
							float num10 = 0.05f;
							nodesTemp.Add(new Vector2(num6 * (float)l + dewRandom.Range(num6 * num10, num6 * (1f - num10)), num7 * (float)m + dewRandom.Range(num7 * num10, num7 * (1f - num10))));
						}
					}
					for (int num11 = nodesTemp.Count - 1; num11 >= 0; num11--)
					{
						for (int n = 0; n < nodesTemp.Count; n++)
						{
							if (num11 != n && Vector2.SqrMagnitude(nodesTemp[num11] - nodesTemp[n]) < minDistanceBetweenNode * minDistanceBetweenNode)
							{
								nodesTemp.RemoveAt(num11);
								num9 += ScorePerIsolatedNode;
								break;
							}
						}
					}
					if (dewRandom.Value() < 0.5f)
					{
						List<Vector2> list6 = nodesTemp;
						List<Vector2> list5;
						int index = (list5 = nodesTemp).Count - 1;
						List<Vector2> list7 = nodesTemp;
						Vector2 vector3 = list7[list7.Count - 1];
						Vector2 vector2 = nodesTemp[0];
						Vector2 vector4 = (list6[0] = vector3);
						vector4 = (list5[index] = vector2);
					}
				}
				UpdateAdjacent();
				for (int num12 = 0; num12 < nodesTemp.Count; num12++)
				{
					int num13 = 0;
					for (int num14 = 0; num14 < nodesTemp.Count; num14++)
					{
						if (adjacentMatrix[nodesTemp.Count * num12 + num14])
						{
							num13++;
						}
					}
					num9 += array[Mathf.Clamp(num13, 0, array.Length - 1)];
				}
				for (int num15 = 0; num15 < nodesTemp.Count; num15++)
				{
					isIsolated[num15] = true;
					isExplored[num15] = false;
				}
				isIsolated[0] = false;
				ExploreIndex(0);
				for (int num16 = isIsolated.Length - 1; num16 >= 0; num16--)
				{
					if (isIsolated[num16])
					{
						num9 = ((num16 > 2) ? (num9 + ScorePerIsolatedNode) : (num9 + ScorePerIsolatedImportantNode));
					}
				}
				CalculateDistance(-1);
				List<int> needToTraverse = new List<int>();
				Queue<int> floodFillQueue = new Queue<int>();
				List<int> list8 = new List<int>();
				for (int num17 = 1; num17 < nodesTemp.Count; num17++)
				{
					list8.Add(num17);
				}
				int exitNode = Dew.SelectBestWithScore((IList<int>)list8, (Func<int, int, float>)((int num43, int _) =>
				{
					float score = 0f;
					int num42 = GetDistanceTemp(0, num43);
					int num44 = 5 + ((!isRandomAlgorithm) ? startNodeMarginDistance : 0);
					if (num42 == num44)
					{
						score += 1000f;
					}
					else
					{
						score -= 500 * Mathf.Abs(num42 - num44);
					}
					CheckIsolatedNodes(num43);
					CheckIsolatedNodes(0);
					floodFillQueue.Clear();
					needToTraverse.Clear();
					int num45 = 0;
					for (int num46 = 0; num46 < nodesTemp.Count; num46++)
					{
						if (GetDistanceTemp(num43, num46) == 1)
						{
							num45++;
						}
					}
					if (num45 == 2 || num45 == 3)
					{
						score += 100f;
					}
					else
					{
						score -= 50f * Mathf.Abs((float)num45 - 2.5f);
					}
					return score;
					void CheckIsolatedNodes(int specialIndex)
					{
						needToTraverse.Clear();
						for (int num47 = 1; num47 < nodesTemp.Count; num47++)
						{
							if (num47 != specialIndex)
							{
								needToTraverse.Add(num47);
							}
						}
						floodFillQueue.Enqueue((specialIndex == 0) ? 1 : 0);
						int num48 = default;
						while (floodFillQueue.TryDequeue(ref num48))
						{
							for (int num49 = needToTraverse.Count - 1; num49 >= 0; num49--)
							{
								if (GetDistanceTemp(num48, needToTraverse[num49]) == 1)
								{
									floodFillQueue.Enqueue(needToTraverse[num49]);
									needToTraverse.RemoveAt(num49);
								}
							}
						}
						score -= needToTraverse.Count * 5000;
					}
				}), 0f, dewRandom);
				if (!isRandomAlgorithm)
				{
					int toExitOriginal = GetDistanceTemp(0, exitNode);
					int num18 = Dew.SelectBestIndexWithScore((IList<Vector2>)nodesTemp, (Func<Vector2, int, float>)((Vector2 _, int num42) =>
					{
						if (num42 == 0)
						{
							return 0f;
						}
						if (num42 == exitNode)
						{
							return float.NegativeInfinity;
						}
						return (GetDistanceTemp(0, num42) == startNodeMarginDistance && GetDistanceTemp(num42, exitNode) < toExitOriginal) ? 100f : float.NegativeInfinity;
					}), 0.1f, dewRandom);
					List<Vector2> list9 = nodesTemp;
					List<Vector2> list5 = nodesTemp;
					int index = num18;
					Vector2 vector2 = nodesTemp[num18];
					Vector2 vector3 = nodesTemp[0];
					Vector2 vector4 = (list9[0] = vector2);
					vector4 = (list5[index] = vector3);
					UpdateAdjacent();
				}
				CalculateDistance(exitNode);
				int num19 = GetDistanceTemp(0, exitNode);
				for (int num20 = 1; num20 < nodesTemp.Count; num20++)
				{
					if (GetDistanceTemp(0, num20) > num19 + 1)
					{
						num9 -= 1000f;
					}
				}
				int num21 = GetDistanceTemp(0, exitNode);
				for (int num22 = 1; num22 < nodesTemp.Count; num22++)
				{
					if (num22 == exitNode)
					{
						continue;
					}
					int num23 = GetDistanceTemp(0, num22);
					int num24 = GetDistanceTemp(num22, exitNode);
					if (num23 >= 2)
					{
						int num25 = num23 + num24;
						if (num25 >= num21)
						{
							num9 += (float)(-2000 * (num25 - num21));
						}
					}
				}
				if (num9 > num3)
				{
					num3 = num9;
					bestExitNode = exitNode;
					list3.Clear();
					list3.AddRange(nodeDistanceMatrixTemp);
					list2.Clear();
					list2.AddRange(nodesTemp);
				}
			}
		}
		Debug.Log($"Generated a map with {list2.Count} nodes (Target: {num2})");
		Bounds bounds = default;
		foreach (Vector2 item2 in list2)
		{
			bounds.Encapsulate(new Vector2(item2.x - WorldWidth * 0.5f, item2.y - WorldHeight * 0.5f));
		}
		for (int num26 = 0; num26 < list2.Count; num26++)
		{
			list2[num26] -= (Vector2)bounds.center;
		}
		List<int> list10 = new List<int>();
		WorldNodeData[] newNodes = new WorldNodeData[list2.Count];
		DewRandom dewRandom2 = new DewRandom(num + 16161);
		for (int num27 = 0; num27 < list2.Count; num27++)
		{
			newNodes[num27].position = list2[num27];
			newNodes[num27].status = WorldNodeStatus.Unexplored;
			newNodes[num27].modifiers = new List<ModifierData>();
			newNodes[num27].roomRotValue = dewRandom2.Value();
			list10.Add(num27);
		}
		HunterStatus[] array2 = new HunterStatus[newNodes.Length];
		hunterStatuses.Clear();
		hunterStatuses.AddRange((IEnumerable<HunterStatus>)array2);
		currentTurnIndex = 0;
		Network_003ChunterSkippedTurns_003Ek__BackingField = HunterStartSkippedTurns;
		list10.Remove(0);
		newNodes[0].type = WorldNodeType.Start;
		if (currentZone.startRooms == null || currentZone.startRooms.Count == 0)
		{
			Debug.LogWarning(currentZone.name + " does not have start rooms");
		}
		else
		{
			newNodes[0].room = currentZone.startRooms[new DewRandom(num + 315).Range(0, currentZone.startRooms.Count)];
		}
		newNodes[0].status = WorldNodeStatus.Unexplored;
		if (DewConsoleCommands.bossRushMode)
		{
			int num28 = -1;
			for (int num29 = 1; num29 < newNodes.Length; num29++)
			{
				if (list3[num29] == 1)
				{
					num28 = num29;
					break;
				}
			}
			if (num28 >= 0 && num28 != bestExitNode)
			{
				newNodes[bestExitNode].type = WorldNodeType.Combat;
				list10.Add(bestExitNode);
				bestExitNode = num28;
				Debug.Log($"[BossRush] boss node moved next to start (node {num28})");
			}
			else if (num28 < 0)
			{
				Debug.LogWarning("[BossRush] no node adjacent to start - keeping the generated exit");
			}
		}
		list10.Remove(bestExitNode);
		newNodes[bestExitNode].type = WorldNodeType.ExitBoss;
		if (currentZone.bossRooms == null || currentZone.bossRooms.Count == 0)
		{
			Debug.LogWarning(currentZone.name + " does not have exit boss rooms");
		}
		else
		{
			newNodes[bestExitNode].room = currentZone.bossRooms[new DewRandom(num + 912).Range(0, currentZone.bossRooms.Count)];
		}
		newNodes[bestExitNode].status = WorldNodeStatus.Revealed;
		nodeDistanceMatrix.Clear();
		nodeDistanceMatrix.AddRange((IEnumerable<int>)list3);
		int num30 = Dew.SelectBestIndexWithScore((IList<WorldNodeData>)newNodes, (Func<WorldNodeData, int, float>)((WorldNodeData _, int num42) => nodeDistanceMatrix[newNodes.Length * num42 + bestExitNode]), 0.01f, new DewRandom(num + 513));
		Network_003ChunterStartNodeIndex_003Ek__BackingField = num30;
		if (hunterStartNodeIndex != 0 && hunterStartNodeIndex != bestExitNode)
		{
			list10.Remove(hunterStartNodeIndex);
			newNodes[hunterStartNodeIndex].type = WorldNodeType.Combat;
		}
		if (!currentZone.useSpecialGeneration)
		{
			DewRandom dewRandom3 = new DewRandom(num + 234);
			int num31 = (DewBuildProfile.current.HasFeature(BuildFeatureTag.Booth) ? 1 : dewRandom3.Range(currentZone.numOfMerchants.x, currentZone.numOfMerchants.y + 1));
			for (int num32 = 0; num32 < num31; num32++)
			{
				int num33;
				if (currentZoneIndex <= 0)
				{
					int index2 = Dew.SelectBestIndexWithScore((IList<int>)list10, (Func<int, int, float>)((int node, int _) => Mathf.Min(nodeDistanceMatrix[node], 3)), 0.4f, dewRandom3);
					num33 = list10[index2];
					list10.RemoveAt(index2);
				}
				else
				{
					int index3 = dewRandom3.Range(0, list10.Count);
					num33 = list10[index3];
					list10.RemoveAt(index3);
				}
				newNodes[num33].type = WorldNodeType.Merchant;
			}
			int num34 = dewRandom3.Range(currentZone.numOfEvents.x, currentZone.numOfEvents.y + 1);
			for (int num35 = 0; num35 < num34; num35++)
			{
				int index4 = dewRandom3.Range(0, list10.Count);
				int num36 = list10[index4];
				list10.RemoveAt(index4);
				newNodes[num36].type = WorldNodeType.Event;
				if (currentZone.eventRooms == null || currentZone.eventRooms.Count == 0)
				{
					Debug.LogWarning(currentZone.name + " does not have challenge rooms");
				}
				else
				{
					newNodes[num36].room = currentZone.eventRooms[dewRandom3.Range(0, currentZone.eventRooms.Count)];
				}
			}
		}
		foreach (int item3 in list10)
		{
			newNodes[item3].type = WorldNodeType.Combat;
		}
		list10.Clear();
		nodes.Clear();
		nodes.AddRange((IEnumerable<WorldNodeData>)newNodes);
		List<int> candidateIndices = new List<int>();
		foreach (RoomModifierBase item4 in list)
		{
			RoomModifierBase modifierToAdd = item4;
			candidateIndices.Clear();
			for (int num37 = 0; num37 < newNodes.Length; num37++)
			{
				if (num37 != hunterStartNodeIndex && modifierToAdd.CanSpawnAtNode(num37) && newNodes[num37].type == WorldNodeType.Combat)
				{
					candidateIndices.Add(num37);
				}
			}
			candidateIndices.Sort((int x, int y) =>
			{
				int num42 = newNodes[x].modifiers.Count((ModifierData modifierData) => DewResources.GetByShortTypeName<RoomModifierBase>(modifierData.type, ResourceLoadSettings.Light).visibilityOnWorld == NodeModifierVisibility.OnRevealed);
				int num43 = newNodes[y].modifiers.Count((ModifierData modifierData) => DewResources.GetByShortTypeName<RoomModifierBase>(modifierData.type, ResourceLoadSettings.Light).visibilityOnWorld == NodeModifierVisibility.OnRevealed);
				return (num42 / 2).CompareTo(num43 / 2);
			});
			DewRandom dewRandom4 = new DewRandom(num + ((UnityEngine.Object)(object)modifierToAdd).name.GetStableHashCode());
			float num38 = 1f;
			if (DewBuildProfile.current.HasFeature(BuildFeatureTag.Booth))
			{
				string name = ((object)modifierToAdd).GetType().Name;
				num38 = ((!(name == "RoomMod_SpawnMiniBoss") && !(name == "RoomMod_HarderFightBetterReward")) ? 2f : 1.5f);
			}
			if (modifierToAdd.spawnType == ModifierSpawnType.Chance)
			{
				int num39 = 0;
				for (int num40 = 0; num40 < candidateIndices.Count; num40++)
				{
					if (dewRandom4.Value() < modifierToAdd.GetScaledChance() * num38)
					{
						num39++;
					}
				}
				SpawnWithCount(num39);
			}
			else if (modifierToAdd.spawnType == ModifierSpawnType.Count)
			{
				SpawnWithCount(DewMath.RandomRoundToInt(modifierToAdd.GetScaledChance() * num38, dewRandom4));
			}
			else if (modifierToAdd.spawnType == ModifierSpawnType.Ratio)
			{
				SpawnWithCount(DewMath.RandomRoundToInt(modifierToAdd.GetScaledChance() * (float)candidateIndices.Count * num38, dewRandom4));
			}
			void SpawnWithCount(int num42)
			{
				if (num42 > 0)
				{
					for (int num43 = 0; num43 < num42; num43++)
					{
						bool flag = false;
						for (int num44 = 0; num44 < candidateIndices.Count; num44++)
						{
							bool flag2 = true;
							foreach (ModifierData modifier in newNodes[candidateIndices[num44]].modifiers)
							{
								RoomModifierBase byShortTypeName2 = DewResources.GetByShortTypeName<RoomModifierBase>(modifier.type, ResourceLoadSettings.Light);
								if (byShortTypeName2.disallowOtherModifiers || modifierToAdd.disallowOtherModifiers)
								{
									flag2 = false;
									break;
								}
								if (byShortTypeName2.modifiesRewards && modifierToAdd.modifiesRewards)
								{
									flag2 = false;
									break;
								}
								if (byShortTypeName2.isMain && modifierToAdd.isMain)
								{
									flag2 = false;
									break;
								}
							}
							if (flag2)
							{
								List<ModifierData> modifiers = newNodes[candidateIndices[num44]].modifiers;
								ModifierData item = default;
								ZoneManager zoneManager = this;
								int nextModifierId = _nextModifierId;
								zoneManager._nextModifierId = nextModifierId + 1;
								item.id = nextModifierId;
								item.type = ((UnityEngine.Object)(object)modifierToAdd).name;
								item.clientData = "";
								modifiers.Add(item);
								candidateIndices.RemoveAt(num44);
								flag = true;
								break;
							}
						}
						if (!flag)
						{
							Debug.LogWarning("No valid node found to put " + ((UnityEngine.Object)(object)modifierToAdd).name);
							break;
						}
					}
				}
			}
		}
		WorldNodeData[] array3 = newNodes;
		for (int index = 0; index < array3.Length; index++)
		{
			WorldNodeData worldNodeData = array3[index];
			if (worldNodeData.modifiers == null)
			{
				continue;
			}
			foreach (ModifierData modifier2 in worldNodeData.modifiers)
			{
				if (!modifierServerData.ContainsKey(modifier2.id))
				{
					RoomModifierBase byShortTypeName = DewResources.GetByShortTypeName<RoomModifierBase>(modifier2.type, default(ResourceLoadSettings));
					if (!((UnityEngine.Object)(object)byShortTypeName == null))
					{
						RoomModifierBase roomModifierBase = UnityEngine.Object.Instantiate<RoomModifierBase>(byShortTypeName);
						roomModifierBase.id = modifier2.id;
						modifierServerData[modifier2.id] = new ModifierServerData
						{
							didCreateInstance = false,
							persistentData = DewPersistence.SerializeGameObject(((Component)(object)roomModifierBase).gameObject)
						};
						UnityEngine.Object.Destroy(((Component)(object)roomModifierBase).gameObject);
					}
				}
			}
		}
		nodes.Clear();
		nodes.AddRange((IEnumerable<WorldNodeData>)newNodes);
		Network_003CcurrentNodeIndex_003Ek__BackingField = -1;
		visitedNodesSaveData = new List<DewPersistence.RoomData>();
		for (int num41 = 0; num41 < nodes.Count; num41++)
		{
			visitedNodesSaveData.Add(null);
		}
		_combatRoomPool.Clear();
		_shopRoomPool.Clear();
		_usedAngleIndexes.Clear();
		onWorldGenerated?.Invoke();
		void CalculateDistance(int num45)
		{
			int count2 = nodesTemp.Count;
			List<int> list11 = nodeDistanceMatrixTemp;
			list11.Clear();
			for (int num42 = 0; num42 < count2 * count2; num42++)
			{
				list11.Add(0);
			}
			for (int num43 = 0; num43 < count2; num43++)
			{
				for (int num44 = 0; num44 < count2; num44++)
				{
					int index5 = num43 * count2 + num44;
					if (num43 == num44)
					{
						list11[index5] = 0;
					}
					else if (num43 == num45)
					{
						list11[index5] = 10000;
					}
					else if (adjacentMatrix[index5])
					{
						list11[index5] = 1;
					}
					else
					{
						list11[index5] = 10000;
					}
				}
			}
			for (int num46 = 0; num46 < count2; num46++)
			{
				for (int num47 = 0; num47 < count2; num47++)
				{
					for (int num48 = 0; num48 < count2; num48++)
					{
						if (list11[num47 * count2 + num48] > list11[num47 * count2 + num46] + list11[num46 * count2 + num48])
						{
							list11[num47 * count2 + num48] = list11[num47 * count2 + num46] + list11[num46 * count2 + num48];
						}
					}
				}
			}
		}
		void ExploreIndex(int num42)
		{
			isExplored[num42] = true;
			for (int num43 = 0; num43 < nodesTemp.Count; num43++)
			{
				if (adjacentMatrix[nodesTemp.Count * num42 + num43])
				{
					isIsolated[num43] = false;
					if (!isExplored[num43])
					{
						ExploreIndex(num43);
					}
				}
			}
		}
		int GetDistanceTemp(int from, int to)
		{
			return nodeDistanceMatrixTemp[from * nodesTemp.Count + to];
		}
		bool GetRandomNodePosWithMinDistance(out Vector2 reference, DewRandom random)
		{
			reference = default;
			for (int num42 = 0; num42 < NodePlacementTries; num42++)
			{
				if (nodesTemp.Count > 0)
				{
					Vector2 vector11 = nodesTemp[random.Range(0, nodesTemp.Count)];
					bool flag = true;
					for (int num43 = 0; num43 < 10; num43++)
					{
						reference = vector11 + random.InsideUnitCircle().normalized * random.Range(minDistanceBetweenNode, maxDistanceToNearestNode);
						if (reference.x >= 0f && reference.x <= WorldWidth && reference.y >= 0f && reference.y <= WorldHeight)
						{
							flag = false;
							break;
						}
					}
					if (flag)
					{
						continue;
					}
				}
				else
				{
					reference = new Vector2(random.Range(0f, WorldWidth), random.Range(0f, WorldHeight));
				}
				bool flag2 = false;
				float num44 = 0f;
				foreach (Vector2 item5 in nodesTemp)
				{
					float num45 = Vector2.SqrMagnitude(reference - item5);
					num44 = Mathf.Min(num44, num45);
					if (num45 < minDistanceBetweenNode * minDistanceBetweenNode)
					{
						flag2 = true;
					}
				}
				if (!flag2 && !(num44 > maxDistanceToNearestNode * maxDistanceToNearestNode))
				{
					return true;
				}
			}
			return false;
		}
		void UpdateAdjacent()
		{
			adjacentMatrix.Clear();
			for (int num42 = 0; num42 < nodesTemp.Count; num42++)
			{
				Vector2 vector11 = nodesTemp[num42];
				for (int num43 = 0; num43 < nodesTemp.Count; num43++)
				{
					Vector2 vector12 = nodesTemp[num43];
					if (num42 == num43)
					{
						adjacentMatrix.Add(item: false);
					}
					else
					{
						adjacentMatrix.Add(Vector2.SqrMagnitude(vector11 - vector12) < adjacentThreshold * adjacentThreshold);
					}
				}
			}
			for (int num44 = 0; num44 < nodesTemp.Count; num44++)
			{
				for (int num45 = 0; num45 < nodesTemp.Count; num45++)
				{
					Vector2 vector13 = nodesTemp[num45] - nodesTemp[num44];
					if (adjacentMatrix[num44 * nodesTemp.Count + num45])
					{
						for (int num46 = 0; num46 < nodesTemp.Count; num46++)
						{
							if (num45 != num46 && adjacentMatrix[num44 * nodesTemp.Count + num46])
							{
								Vector2 to = nodesTemp[num46] - nodesTemp[num44];
								if (Vector2.Angle(vector13, to) < 20f && vector13.sqrMagnitude > to.sqrMagnitude)
								{
									adjacentMatrix[num44 * nodesTemp.Count + num45] = false;
									adjacentMatrix[num45 * nodesTemp.Count + num44] = false;
									break;
								}
							}
						}
					}
				}
			}
		}
	}

	[Server]
	public void SetCurrentNodeIndexAndRevealAdjacent(int index)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void ZoneManager::SetCurrentNodeIndexAndRevealAdjacent(System.Int32)' called when server was not active");
			return;
		}
		if (index < 0 || index >= nodes.Count)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		Network_003CcurrentNodeIndex_003Ek__BackingField = index;
		if (currentNodeIndex < 0)
		{
			return;
		}
		WorldNodeData worldNodeData = nodes[currentNodeIndex];
		worldNodeData.status = WorldNodeStatus.HasVisited;
		nodes[currentNodeIndex] = worldNodeData;
		for (int i = 0; i < nodes.Count; i++)
		{
			if (IsNodeConnected(index, i))
			{
				WorldNodeData worldNodeData2 = nodes[i];
				if (worldNodeData2.status == WorldNodeStatus.Unexplored)
				{
					worldNodeData2.status = WorldNodeStatus.Revealed;
					nodes[i] = worldNodeData2;
				}
			}
		}
	}

	[Server]
	public void AddSidetrackNode(string roomName)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void ZoneManager::AddSidetrackNode(System.String)' called when server was not active");
			return;
		}
		nodes.Add(new WorldNodeData
		{
			room = roomName,
			type = WorldNodeType.Special,
			position = Vector2.one * -100000f,
			modifiers = new List<ModifierData>()
		});
		hunterStatuses.Add(HunterStatus.None);
		visitedNodesSaveData.Add(null);
		int num = nodes.Count - 1;
		for (int i = 0; i < num + 1; i++)
		{
			nodeDistanceMatrix.Add((i != num) ? 10000 : 0);
		}
		for (int num2 = num * num; num2 > 0; num2 -= num)
		{
			nodeDistanceMatrix.Insert(num2, 10000);
		}
	}

	public bool IsNodeConnected(int a, int b)
	{
		if (GetNodeDistance(a, b) != 1)
		{
			return GetNodeDistance(b, a) == 1;
		}
		return true;
	}

	public int GetNodeDistance(int a, int b)
	{
		return nodeDistanceMatrix[nodes.Count * a + b];
	}

	[Server]
	public void RevealWorld(bool fully = false)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void ZoneManager::RevealWorld(System.Boolean)' called when server was not active");
			return;
		}
		for (int i = 0; i < nodes.Count; i++)
		{
			WorldNodeData worldNodeData = nodes[i];
			WorldNodeStatus worldNodeStatus = ((!fully) ? WorldNodeStatus.Revealed : WorldNodeStatus.RevealedFull);
			if (worldNodeData.status < worldNodeStatus)
			{
				worldNodeData.status = worldNodeStatus;
				nodes[i] = worldNodeData;
			}
		}
	}

	[Server]
	public void AdvanceHunterTurn(bool forceMove = false)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void ZoneManager::AdvanceHunterTurn(System.Boolean)' called when server was not active");
		}
		else
		{
			if (isHuntAdvanceDisabled)
			{
				return;
			}
			if (!forceMove && hunterSkippedTurns > 0)
			{
				hunterSkippedTurns--;
				return;
			}
			if (hunterStatuses[hunterStartNodeIndex] == HunterStatus.None)
			{
				hunterStatuses[hunterStartNodeIndex] = HunterStatus.AboutToBeTaken;
				return;
			}
			for (int i = 0; i < nodes.Count; i++)
			{
				if (hunterStatuses[i] == HunterStatus.AboutToBeTaken)
				{
					hunterStatuses[i] = HunterStatus.Level1;
				}
				else if (hunterStatuses[i] == HunterStatus.Level1)
				{
					hunterStatuses[i] = HunterStatus.Level2;
				}
				else if (hunterStatuses[i] == HunterStatus.Level2)
				{
					hunterStatuses[i] = HunterStatus.Level3;
				}
			}
			List<int> list = new List<int>();
			for (int j = 0; j < nodes.Count; j++)
			{
				if (hunterStatuses[j] < HunterStatus.Level1)
				{
					continue;
				}
				for (int k = 0; k < nodes.Count; k++)
				{
					if (IsNodeConnected(j, k) && hunterStatuses[k] == HunterStatus.None && nodes[k].type != WorldNodeType.ExitBoss && !list.Contains(k))
					{
						list.Add(k);
					}
				}
			}
			float num = (isCurrentNodeHunted ? Mathf.Lerp(0.5f, 0.9f, (float)currentHuntLevel / 5f) : 1f);
			float num2 = NetworkedManagerBase<GameManager>.instance.difficulty.hunterSpreadChance * num;
			num2 *= hunterSpreadMultiplier;
			num2 = Mathf.Clamp01(num2);
			_hunterSpreadCredit += num2 * (float)list.Count;
			nodes.FindIndex((Predicate<WorldNodeData>)((WorldNodeData w) => w.type == WorldNodeType.ExitBoss));
			while (_hunterSpreadCredit >= 1f && list.Count > 0)
			{
				_hunterSpreadCredit--;
				int index = Dew.SelectBestIndexWithScore((IList<int>)list, (Func<int, int, float>)((int num3, int _) => 0f - Vector2.Distance(nodes[hunterStartNodeIndex].position, nodes[num3].position)), 0f, (DewRandom)null);
				hunterStatuses[list[index]] = HunterStatus.AboutToBeTaken;
				list.RemoveAt(index);
			}
		}
	}

	[Server]
	public void UpdateModifiersByHunterStatus(int currentTravelDestinationNode = -1)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void ZoneManager::UpdateModifiersByHunterStatus(System.Int32)' called when server was not active");
			return;
		}
		for (int i = 0; i < nodes.Count; i++)
		{
			WorldNodeData worldNodeData = nodes[i];
			if (worldNodeData.HasModifier<RoomMod_Hunted>())
			{
				HunterStatus hunterStatus = hunterStatuses[i];
				if (hunterStatus == HunterStatus.None || hunterStatus == HunterStatus.AboutToBeTaken)
				{
					RemoveModifier<RoomMod_Hunted>(i);
				}
			}
			if (hunterStatuses[i] == HunterStatus.None || (hunterStatuses[i] == HunterStatus.AboutToBeTaken && i == currentTravelDestinationNode))
			{
				continue;
			}
			if (worldNodeData.type == WorldNodeType.Merchant && !worldNodeData.HasModifier<RoomMod_MerchantBackpack>() && !worldNodeData.HasModifier<RoomMod_NoMerchant>())
			{
				AddModifier<RoomMod_MerchantBackpack>(i);
				AddModifier<RoomMod_NoMerchant>(i);
			}
			for (int num = worldNodeData.modifiers.Count - 1; num >= 0; num--)
			{
				RoomModifierBase byShortTypeName = DewResources.GetByShortTypeName<RoomModifierBase>(worldNodeData.modifiers[num].type, default(ResourceLoadSettings));
				if ((UnityEngine.Object)(object)byShortTypeName != null && byShortTypeName.isMain && !(byShortTypeName is RoomMod_Hunted))
				{
					RemoveModifier(i, worldNodeData.modifiers[num].id);
					break;
				}
			}
			if (!worldNodeData.HasModifier<RoomMod_Hunted>())
			{
				AddModifier<RoomMod_Hunted>(i);
			}
		}
	}

	public (string reasonText, bool shouldCancel) GetCannotTravelReason()
	{
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_025e: Unknown result type (might be due to invalid IL or missing references)
		foreach (DewPlayer gamePlayer in DewPlayer.gamePlayers)
		{
			if (!gamePlayer.hero.isKnockedOut)
			{
				if (!gamePlayer.hero.Skill.holdingObject.IsHoldableObjectNullOrInactive())
				{
					return (reasonText: string.Format(DewLocalization.GetUIValue("InGame_Message_RiftCantHoldOntoItem"), ChatManager.GetColoredDescribedPlayerName(gamePlayer)), shouldCancel: false);
				}
				if (gamePlayer.hero.Status.isInConversation)
				{
					return (reasonText: string.Format(DewLocalization.GetUIValue("InGame_Message_RiftInConversation"), ChatManager.GetColoredDescribedPlayerName(gamePlayer)), shouldCancel: false);
				}
				if (gamePlayer.isReadingArtifactStory && !gamePlayer.hero.IsNullInactiveDeadOrKnockedOut())
				{
					return (reasonText: string.Format(DewLocalization.GetUIValue("InGame_Message_ReadingArtifactStory"), ChatManager.GetColoredDescribedPlayerName(gamePlayer)), shouldCancel: false);
				}
			}
		}
		foreach (Entity allEntity in NetworkedManagerBase<ActorManager>.instance.allEntities)
		{
			if (allEntity.IsAnyBoss())
			{
				return (reasonText: DewLocalization.GetUIValue("InGame_Message_RiftBossAlive"), shouldCancel: true);
			}
		}
		foreach (Actor allActor in NetworkedManagerBase<ActorManager>.instance.allActors)
		{
			if (allActor is SkillTrigger skillTrigger && (UnityEngine.Object)(object)skillTrigger.owner == null && (UnityEngine.Object)(object)skillTrigger.handOwner == null && !Rift.instance.IsNullOrInactive() && (int)Dew.GetNavMeshPathStatus(Dew.GetValidAgentPosition(Dew.GetPositionOnGround(((Component)(object)Rift.instance).transform.position)), Dew.GetValidAgentPosition(Dew.GetPositionOnGround(skillTrigger.position))) == 0)
			{
				return (reasonText: GetCannotTravelReasonByDroppedSkill(skillTrigger), shouldCancel: false);
			}
			if (allActor is Gem gem && (UnityEngine.Object)(object)gem.owner == null && (UnityEngine.Object)(object)gem.handOwner == null && !Rift.instance.IsNullOrInactive() && (int)Dew.GetNavMeshPathStatus(Dew.GetValidAgentPosition(Dew.GetPositionOnGround(((Component)(object)Rift.instance).transform.position)), Dew.GetValidAgentPosition(Dew.GetPositionOnGround(gem.position))) == 0)
			{
				return (reasonText: GetCannotTravelReasonByDroppedGem(gem), shouldCancel: false);
			}
		}
		return (reasonText: null, shouldCancel: false);
	}

	public string GetCannotTravelReasonByDroppedSkill(SkillTrigger st)
	{
		string uIValue = DewLocalization.GetUIValue("InGame_Message_RiftMemoryIsOnGround");
		if ((UnityEngine.Object)(object)st.tempOwner != null && !((NetworkBehaviour)st.tempOwner).isOwned)
		{
			return string.Format(uIValue, ChatManager.GetColoredDescribedPlayerName(st.tempOwner) + " - " + ChatManager.GetColoredSkillName(((object)st).GetType().Name, st.level));
		}
		return string.Format(uIValue, ChatManager.GetColoredSkillName(((object)st).GetType().Name, st.level));
	}

	public string GetCannotTravelReasonByDroppedGem(Gem g)
	{
		string uIValue = DewLocalization.GetUIValue("InGame_Message_RiftEssenceIsOnGround");
		if ((UnityEngine.Object)(object)g.tempOwner != null && !((NetworkBehaviour)g.tempOwner).isOwned)
		{
			return string.Format(uIValue, ChatManager.GetColoredDescribedPlayerName(g.tempOwner) + " - " + ChatManager.GetColoredGemName(((object)g).GetType().Name, g.quality));
		}
		return string.Format(uIValue, ChatManager.GetColoredGemName(((object)g).GetType().Name, g.quality));
	}

	[Command(requiresAuthority = false)]
	public void CmdTravelToNode(int index, NetworkConnectionToClient sender = null)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteInt((NetworkWriter)(object)val, index);
		((NetworkBehaviour)this).SendCommandInternal("System.Void ZoneManager::CmdTravelToNode(System.Int32,Mirror.NetworkConnectionToClient)", -2012969603, (NetworkWriter)(object)val, 0, false);
		NetworkWriterPool.Return(val);
	}

	[Command(requiresAuthority = false)]
	public void CmdTravelToNextZone(NetworkConnectionToClient sender = null)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendCommandInternal("System.Void ZoneManager::CmdTravelToNextZone(Mirror.NetworkConnectionToClient)", -277905233, (NetworkWriter)(object)val, 0, false);
		NetworkWriterPool.Return(val);
	}

	public bool TryGetNodeIndexForNextGoal(GetNodeIndexSettings s, out int nodeIndex)
	{
		int exitNodeIndex = 0;
		for (int i = 0; i < nodes.Count; i++)
		{
			if (nodes[i].type == WorldNodeType.ExitBoss)
			{
				exitNodeIndex = i;
				break;
			}
		}
		int currentDistToExit = GetNodeDistance(currentNodeIndex, exitNodeIndex);
		int num = (nodeIndex = Dew.SelectBestIndexWithScore((IList<WorldNodeData>)nodes, GetScore));
		return GetScore(nodes[num], num) > -5000f;
		float GetScore(WorldNodeData data, int num3)
		{
			float num2 = 0f;
			if (data.IsSidetrackNode())
			{
				num2 -= 10000f;
			}
			if (!s.allowedTypes.Contains(data.type))
			{
				num2 -= 10000f;
			}
			if (num3 == currentNodeIndex)
			{
				num2 -= 10000f;
			}
			switch (data.status)
			{
			case WorldNodeStatus.Revealed:
			case WorldNodeStatus.RevealedFull:
				num2 -= 2.5f;
				break;
			case WorldNodeStatus.HasVisited:
				num2 -= 10000f;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			case WorldNodeStatus.Unexplored:
				break;
			}
			int nodeDistance = GetNodeDistance(currentNodeIndex, num3);
			if (nodeDistance < s.desiredDistance.x)
			{
				num2 -= (float)(s.desiredDistance.x - nodeDistance) * 1f;
			}
			num2 = ((nodeDistance <= s.desiredDistance.y) ? (num2 + 5f) : (num2 - (float)(nodeDistance - s.desiredDistance.y) * 1f));
			if (s.preferCloserToExit)
			{
				int num4 = currentDistToExit - GetNodeDistance(num3, exitNodeIndex);
				num2 = ((num4 <= 0) ? (num2 + (float)num4 * 3f) : (num2 + (float)num4 * 0.75f));
			}
			if (s.avoidMainModifier && nodes[num3].HasMainModifier())
			{
				num2 -= 10000f;
			}
			return num2 + GetWorldRandom(-511).Range(-1.5f, 1.5f);
		}
	}

	public int AddModifier<T>(int nodeIndex, Action<T> beforePrepare = null) where T : RoomModifierBase
	{
		Action<RoomModifierBase> beforePrepare2 = null;
		if (beforePrepare != null)
		{
			beforePrepare2 = (RoomModifierBase mod) =>
			{
				beforePrepare((T)mod);
			};
		}
		return AddModifier(nodeIndex, new ModifierData
		{
			id = _nextModifierId++,
			type = typeof(T).Name
		}, beforePrepare2);
	}

	public int AddModifier<T>(int nodeIndex, string clientData, Action<T> beforePrepare = null) where T : RoomModifierBase
	{
		Action<RoomModifierBase> beforePrepare2 = null;
		if (beforePrepare != null)
		{
			beforePrepare2 = (RoomModifierBase mod) =>
			{
				beforePrepare((T)mod);
			};
		}
		int id = _nextModifierId++;
		return AddModifier(nodeIndex, new ModifierData
		{
			id = id,
			type = typeof(T).Name,
			clientData = clientData
		}, beforePrepare2);
	}

	public int AddModifier(int nodeIndex, ModifierData mod, Action<RoomModifierBase> beforePrepare = null)
	{
		if (mod.id == 0)
		{
			mod.id = _nextModifierId++;
		}
		if (mod.clientData == null)
		{
			mod.clientData = "";
		}
		if (DewResources.GetByShortTypeName<RoomModifierBase>(mod.type, default(ResourceLoadSettings)).isMain)
		{
			List<ModifierData> modifiers = nodes[nodeIndex].modifiers;
			for (int num = modifiers.Count - 1; num >= 0; num--)
			{
				if (DewResources.GetByShortTypeName<RoomModifierBase>(modifiers[num].type, default(ResourceLoadSettings)).isMain)
				{
					RemoveModifier(nodeIndex, modifiers[num].id);
					break;
				}
			}
		}
		WorldNodeData worldNodeData = nodes[nodeIndex];
		worldNodeData.modifiers = new List<ModifierData>(worldNodeData.modifiers);
		worldNodeData.modifiers.Add(mod);
		nodes[nodeIndex] = worldNodeData;
		RoomModifierBase byShortTypeName = DewResources.GetByShortTypeName<RoomModifierBase>(mod.type, default(ResourceLoadSettings));
		if (nodeIndex == currentNodeIndex && (UnityEngine.Object)(object)SingletonDewNetworkBehaviour<Room>.instance != null && (UnityEngine.Object)(object)SingletonDewNetworkBehaviour<Room>.instance.modifiers != null && SingletonDewNetworkBehaviour<Room>.instance.modifiers.isRoomActive)
		{
			modifierServerData[mod.id] = new ModifierServerData
			{
				didCreateInstance = false
			};
			SingletonDewNetworkBehaviour<Room>.instance.modifiers.HandleRuntimeAddition(mod.id, beforePrepare);
			if (!string.IsNullOrEmpty(byShortTypeName.roomOverride))
			{
				Debug.LogWarning("RoomModifier " + mod.type + " with room override " + byShortTypeName.roomOverride + " is added on runtime. This is not allowed.");
			}
		}
		else
		{
			RoomModifierBase roomModifierBase = UnityEngine.Object.Instantiate<RoomModifierBase>(byShortTypeName);
			roomModifierBase.id = mod.id;
			try
			{
				beforePrepare?.Invoke(roomModifierBase);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
			modifierServerData[mod.id] = new ModifierServerData
			{
				didCreateInstance = false,
				persistentData = DewPersistence.SerializeGameObject(((Component)(object)roomModifierBase).gameObject)
			};
			UnityEngine.Object.Destroy(((Component)(object)roomModifierBase).gameObject);
			if (!string.IsNullOrEmpty(byShortTypeName.roomOverride))
			{
				SetRoomOverride(nodeIndex, byShortTypeName.roomOverride);
			}
		}
		return mod.id;
	}

	public void SetRoomRotIndex(int nodeIndex, int roomRotIndex)
	{
		if (nodeIndex >= 0 && nodeIndex < nodes.Count)
		{
			WorldNodeData worldNodeData = nodes[nodeIndex];
			worldNodeData.roomRotIndex = roomRotIndex;
			nodes[nodeIndex] = worldNodeData;
		}
	}

	public bool RemoveModifier<T>(int nodeIndex)
	{
		if (nodeIndex < 0 || nodeIndex >= nodes.Count)
		{
			return false;
		}
		ModifierData modifierData = nodes[nodeIndex].modifiers.Find((ModifierData m) => m.type == typeof(T).Name);
		if (modifierData.id == 0)
		{
			return false;
		}
		RemoveModifier(nodeIndex, modifierData.id);
		return true;
	}

	public void RemoveModifier(int nodeIndex, int modifierId)
	{
		if (nodeIndex == currentNodeIndex && (UnityEngine.Object)(object)SingletonDewNetworkBehaviour<Room>.instance != null && (UnityEngine.Object)(object)SingletonDewNetworkBehaviour<Room>.instance.modifiers != null && SingletonDewNetworkBehaviour<Room>.instance.modifiers.isRoomActive)
		{
			SingletonDewNetworkBehaviour<Room>.instance.modifiers.HandleRuntimeRemoval(modifierId);
		}
		WorldNodeData worldNodeData = nodes[nodeIndex];
		worldNodeData.modifiers = new List<ModifierData>(worldNodeData.modifiers);
		int num = worldNodeData.modifiers.FindIndex((ModifierData m) => m.id == modifierId);
		if (num >= 0)
		{
			worldNodeData.modifiers.RemoveAt(num);
		}
		nodes[nodeIndex] = worldNodeData;
	}

	public void RemoveModifier(int modifierId)
	{
		for (int i = 0; i < nodes.Count; i++)
		{
			if (nodes[i].modifiers.FindIndex((ModifierData m) => m.id == modifierId) >= 0)
			{
				RemoveModifier(i, modifierId);
				break;
			}
		}
	}

	[Server]
	public void SyncNodeChanges(int nodeIndex)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void ZoneManager::SyncNodeChanges(System.Int32)' called when server was not active");
			return;
		}
		WorldNodeData worldNodeData = nodes[nodeIndex];
		worldNodeData.modifiers = new List<ModifierData>(worldNodeData.modifiers);
		nodes[nodeIndex] = worldNodeData;
	}

	public void SetRoomOverride(int nodeIndex, string roomOverride)
	{
		if (nodeIndex == currentNodeIndex)
		{
			Debug.LogWarning("Tried to set room override on current node. Doing this is prone to bugs.");
		}
		WorldNodeData worldNodeData = nodes[nodeIndex];
		worldNodeData.roomOverride = roomOverride;
		nodes[nodeIndex] = worldNodeData;
		visitedNodesSaveData[nodeIndex] = null;
		ModifierData[] array = worldNodeData.modifiers.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			ModifierData modifierData = array[i];
			RoomModifierBase byShortTypeName = DewResources.GetByShortTypeName<RoomModifierBase>(modifierData.type, ResourceLoadSettings.Light);
			if (!((UnityEngine.Object)(object)byShortTypeName == null) && !string.IsNullOrEmpty(byShortTypeName.roomOverride) && byShortTypeName.removeModifierWhenOverrideOverwritten && !(byShortTypeName.roomOverride == roomOverride))
			{
				RemoveModifier(nodeIndex, modifierData.id);
			}
		}
	}

	public void SetRoom(int nodeIndex, string room)
	{
		WorldNodeData worldNodeData = nodes[nodeIndex];
		worldNodeData.room = room;
		nodes[nodeIndex] = worldNodeData;
	}

	public void RevealNodesAndAnnounce(DewPlayer revealer, int nodeCount)
	{
		List<int> list = new List<int>();
		for (int i = 0; i < nodes.Count; i++)
		{
			if (nodes[i].type != WorldNodeType.ExitBoss && nodes[i].status == WorldNodeStatus.Unexplored)
			{
				list.Add(i);
			}
		}
		if (list.Count < nodeCount)
		{
			nodeCount = list.Count;
		}
		for (int j = 0; j < nodeCount; j++)
		{
			if (list.Count == 0)
			{
				break;
			}
			int index = GetWorldRandom(-1612).Range(0, list.Count);
			int num = list[index];
			WorldNodeData worldNodeData = nodes[num];
			worldNodeData.status = WorldNodeStatus.RevealedFull;
			nodes[num] = worldNodeData;
			list.RemoveAt(index);
		}
		RpcAnnounceRevealedNodes(revealer, nodeCount);
	}

	[ClientRpc]
	private void RpcAnnounceRevealedNodes(DewPlayer revealer, int nodeCount)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteNetworkBehaviour((NetworkWriter)(object)val, (NetworkBehaviour)(object)revealer);
		NetworkWriterExtensions.WriteInt((NetworkWriter)(object)val, nodeCount);
		((NetworkBehaviour)this).SendRPCInternal("System.Void ZoneManager::RpcAnnounceRevealedNodes(DewPlayer,System.Int32)", 1760406833, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[Server]
	private void SelectHardVariantBossZones()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void ZoneManager::SelectHardVariantBossZones()' called when server was not active");
			return;
		}
		hardVariantBossZoneIndices.Clear();
		int num = DewBuildProfile.current.content.zoneCountByTier.Sum();
		if (DewConsoleCommands.bossRushMode)
		{
			for (int i = 0; i <= num; i++)
			{
				hardVariantBossZoneIndices.Add(currentZoneIndex + i);
			}
			return;
		}
		int num2 = Mathf.Clamp(NetworkedManagerBase<GameManager>.instance.difficulty.hardVariantBossZones, 0, num);
		List<int> list = new List<int>();
		for (int j = 0; j < num; j++)
		{
			list.Add(currentZoneIndex + 1 + j);
		}
		list.Shuffle();
		for (int k = 0; k < num2; k++)
		{
			hardVariantBossZoneIndices.Add(list[k]);
		}
	}

	[Server]
	public void LoadNextZoneByContentSettings()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void ZoneManager::LoadNextZoneByContentSettings()' called when server was not active");
			return;
		}
		if (remainingZonesOfCurrentTier.Count == 0)
		{
			DewGameContentSettings content = DewBuildProfile.current.content;
			currentTier++;
			if (currentTier >= content.zoneCountByTier.Count)
			{
				Network_003CcurrentTier_003Ek__BackingField = 0;
				loopIndex++;
				OnLoopStart();
			}
			Zone[] zones = DewResources.FindAllByNameSubstring<Zone>("Zone_").ToArray();
			zones = DewBuildProfile.current.content.FilterZones(zones);
			Zone[] array = zones;
			foreach (Zone zone in array)
			{
				if (zone.zoneTier == currentTier)
				{
					remainingZonesOfCurrentTier.Add(zone);
				}
			}
			remainingZonesOfCurrentTier.Shuffle();
			if (remainingZonesOfCurrentTier.Count < content.zoneCountByTier[currentTier])
			{
				Debug.LogWarning($"Not enough zones to match current zone settings requirement: {remainingZonesOfCurrentTier.Count}/{content.zoneCountByTier[currentTier]}");
			}
			while (remainingZonesOfCurrentTier.Count > content.zoneCountByTier[currentTier])
			{
				remainingZonesOfCurrentTier.RemoveAt(remainingZonesOfCurrentTier.Count - 1);
			}
		}
		int index = UnityEngine.Random.Range(0, remainingZonesOfCurrentTier.Count);
		AssetRef<Zone> assetRef = remainingZonesOfCurrentTier[index];
		remainingZonesOfCurrentTier.RemoveAt(index);
		if (nextZoneOverride.asset != null)
		{
			assetRef = nextZoneOverride;
			nextZoneOverride = null;
		}
		TravelToZone(assetRef);
	}

	public ZoneManager()
	{
		((NetworkBehaviour)this).InitSyncObject((SyncObject)(object)nodes);
		((NetworkBehaviour)this).InitSyncObject((SyncObject)(object)hunterStatuses);
		((NetworkBehaviour)this).InitSyncObject((SyncObject)(object)nodeDistanceMatrix);
		_Mirror_SyncVarHookDelegate__003CcurrentRoomIndex_003Ek__BackingField = OnCurrentRoomIndexChanged;
		_Mirror_SyncVarHookDelegate__003CclearedCombatRooms_003Ek__BackingField = OnClearedCombatRoomsChanged;
		_Mirror_SyncVarHookDelegate__003CisInRoomTransition_003Ek__BackingField = OnIsInTransitionChanged;
		_Mirror_SyncVarHookDelegate__003CcurrentHuntLevel_003Ek__BackingField = OnCurrentHuntLevelChanged;
	}

	static ZoneManager()
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Expected Obj, but got Unknown
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Expected Obj, but got Unknown
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected Obj, but got Unknown
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Expected Obj, but got Unknown
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Expected Obj, but got Unknown
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Expected Obj, but got Unknown
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Expected Obj, but got Unknown
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Expected Obj, but got Unknown
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Expected Obj, but got Unknown
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Expected Obj, but got Unknown
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Expected Obj, but got Unknown
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Expected Obj, but got Unknown
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ef: Expected Obj, but got Unknown
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Expected Obj, but got Unknown
		WorldHeight = 120f;
		WorldWidth = 240f;
		ScorePerIsolatedNode = -500f;
		ScorePerIsolatedImportantNode = -50000f;
		ScoreFuzziness = 0.1f;
		WorldGenerationIterations = 300;
		NodePlacementTries = 200;
		HunterStartSkippedTurns = 2;
		RemoteProcedureCalls.RegisterCommand(typeof(ZoneManager), "System.Void ZoneManager::CmdCancelVote(Mirror.NetworkConnectionToClient)", (RemoteCallDelegate)InvokeUserCode_CmdCancelVote__NetworkConnectionToClient, false);
		RemoteProcedureCalls.RegisterCommand(typeof(ZoneManager), "System.Void ZoneManager::CmdTravelToNode(System.Int32,Mirror.NetworkConnectionToClient)", (RemoteCallDelegate)InvokeUserCode_CmdTravelToNode__Int32__NetworkConnectionToClient, false);
		RemoteProcedureCalls.RegisterCommand(typeof(ZoneManager), "System.Void ZoneManager::CmdTravelToNextZone(Mirror.NetworkConnectionToClient)", (RemoteCallDelegate)InvokeUserCode_CmdTravelToNextZone__NetworkConnectionToClient, false);
		RemoteProcedureCalls.RegisterRpc(typeof(ZoneManager), "System.Void ZoneManager::InvokeOnRoomLoadStarted(EventInfoLoadRoom)", (RemoteCallDelegate)InvokeUserCode_InvokeOnRoomLoadStarted__EventInfoLoadRoom);
		RemoteProcedureCalls.RegisterRpc(typeof(ZoneManager), "System.Void ZoneManager::InvokeOnZoneLoadStarted(EventInfoLoadZone)", (RemoteCallDelegate)InvokeUserCode_InvokeOnZoneLoadStarted__EventInfoLoadZone);
		RemoteProcedureCalls.RegisterRpc(typeof(ZoneManager), "System.Void ZoneManager::InvokeOnRoomLoaded(EventInfoLoadRoom)", (RemoteCallDelegate)InvokeUserCode_InvokeOnRoomLoaded__EventInfoLoadRoom);
		RemoteProcedureCalls.RegisterRpc(typeof(ZoneManager), "System.Void ZoneManager::InvokeOnZoneLoaded(EventInfoLoadZone)", (RemoteCallDelegate)InvokeUserCode_InvokeOnZoneLoaded__EventInfoLoadZone);
		RemoteProcedureCalls.RegisterRpc(typeof(ZoneManager), "System.Void ZoneManager::InvokeOnLoopStarted()", (RemoteCallDelegate)InvokeUserCode_InvokeOnLoopStarted);
		RemoteProcedureCalls.RegisterRpc(typeof(ZoneManager), "System.Void ZoneManager::RpcShowVoteChatMessage(System.Boolean,DewPlayer)", (RemoteCallDelegate)InvokeUserCode_RpcShowVoteChatMessage__Boolean__DewPlayer);
		RemoteProcedureCalls.RegisterRpc(typeof(ZoneManager), "System.Void ZoneManager::RpcShowCannotTravelMessage()", (RemoteCallDelegate)InvokeUserCode_RpcShowCannotTravelMessage);
		RemoteProcedureCalls.RegisterRpc(typeof(ZoneManager), "System.Void ZoneManager::RpcInvokeVoteStarted(DewPlayer)", (RemoteCallDelegate)InvokeUserCode_RpcInvokeVoteStarted__DewPlayer);
		RemoteProcedureCalls.RegisterRpc(typeof(ZoneManager), "System.Void ZoneManager::RpcInvokeVoteCanceled(DewPlayer)", (RemoteCallDelegate)InvokeUserCode_RpcInvokeVoteCanceled__DewPlayer);
		RemoteProcedureCalls.RegisterRpc(typeof(ZoneManager), "System.Void ZoneManager::RpcInvokeVoteCompleted()", (RemoteCallDelegate)InvokeUserCode_RpcInvokeVoteCompleted);
		RemoteProcedureCalls.RegisterRpc(typeof(ZoneManager), "System.Void ZoneManager::RpcAnnounceRevealedNodes(DewPlayer,System.Int32)", (RemoteCallDelegate)InvokeUserCode_RpcAnnounceRevealedNodes__DewPlayer__Int32);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_InvokeOnRoomLoadStarted__EventInfoLoadRoom(EventInfoLoadRoom info)
	{
		ClientEvent_OnRoomLoadStarted?.Invoke(info);
	}

	protected static void InvokeUserCode_InvokeOnRoomLoadStarted__EventInfoLoadRoom(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC InvokeOnRoomLoadStarted called on server.");
		}
		else
		{
			((ZoneManager)(object)obj).UserCode_InvokeOnRoomLoadStarted__EventInfoLoadRoom(GeneratedNetworkCode._Read_EventInfoLoadRoom(reader));
		}
	}

	protected void UserCode_InvokeOnZoneLoadStarted__EventInfoLoadZone(EventInfoLoadZone info)
	{
		ClientEvent_OnZoneLoadStarted?.Invoke(info);
	}

	protected static void InvokeUserCode_InvokeOnZoneLoadStarted__EventInfoLoadZone(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC InvokeOnZoneLoadStarted called on server.");
		}
		else
		{
			((ZoneManager)(object)obj).UserCode_InvokeOnZoneLoadStarted__EventInfoLoadZone(GeneratedNetworkCode._Read_EventInfoLoadZone(reader));
		}
	}

	protected void UserCode_InvokeOnRoomLoaded__EventInfoLoadRoom(EventInfoLoadRoom info)
	{
		DewResources.UnloadUnused();
		ClientEvent_OnRoomLoaded?.Invoke(info);
	}

	protected static void InvokeUserCode_InvokeOnRoomLoaded__EventInfoLoadRoom(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC InvokeOnRoomLoaded called on server.");
		}
		else
		{
			((ZoneManager)(object)obj).UserCode_InvokeOnRoomLoaded__EventInfoLoadRoom(GeneratedNetworkCode._Read_EventInfoLoadRoom(reader));
		}
	}

	protected void UserCode_InvokeOnZoneLoaded__EventInfoLoadZone(EventInfoLoadZone info)
	{
		ClientEvent_OnZoneLoaded?.Invoke(info);
	}

	protected static void InvokeUserCode_InvokeOnZoneLoaded__EventInfoLoadZone(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC InvokeOnZoneLoaded called on server.");
		}
		else
		{
			((ZoneManager)(object)obj).UserCode_InvokeOnZoneLoaded__EventInfoLoadZone(GeneratedNetworkCode._Read_EventInfoLoadZone(reader));
		}
	}

	protected void UserCode_InvokeOnLoopStarted()
	{
		ClientEvent_OnLoopStarted?.Invoke();
	}

	protected static void InvokeUserCode_InvokeOnLoopStarted(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC InvokeOnLoopStarted called on server.");
		}
		else
		{
			((ZoneManager)(object)obj).UserCode_InvokeOnLoopStarted();
		}
	}

	protected void UserCode_CmdCancelVote__NetworkConnectionToClient(NetworkConnectionToClient sender)
	{
		if (isVoting)
		{
			DewPlayer player = sender.GetPlayer();
			if (!((UnityEngine.Object)(object)player == null))
			{
				RpcShowVoteChatMessage(isStart: false, player);
				RpcInvokeVoteCanceled(player);
				ClearVoteState();
			}
		}
	}

	protected static void InvokeUserCode_CmdCancelVote__NetworkConnectionToClient(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdCancelVote called on client.");
		}
		else
		{
			((ZoneManager)(object)obj).UserCode_CmdCancelVote__NetworkConnectionToClient(senderConnection);
		}
	}

	protected void UserCode_RpcShowVoteChatMessage__Boolean__DewPlayer(bool isStart, DewPlayer player)
	{
		NetworkedManagerBase<ChatManager>.instance.ShowMessageLocally(new ChatManager.Message
		{
			type = ChatManager.MessageType.Notice,
			content = (isStart ? "InGame_Vote_StartedTravelVote" : "InGame_Vote_CanceledTravelVote"),
			args = new string[1] { ChatManager.GetDescribedPlayerName(player) }
		});
	}

	protected static void InvokeUserCode_RpcShowVoteChatMessage__Boolean__DewPlayer(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcShowVoteChatMessage called on server.");
		}
		else
		{
			((ZoneManager)(object)obj).UserCode_RpcShowVoteChatMessage__Boolean__DewPlayer(NetworkReaderExtensions.ReadBool(reader), NetworkReaderExtensions.ReadNetworkBehaviour<DewPlayer>(reader));
		}
	}

	protected void UserCode_RpcShowCannotTravelMessage()
	{
		InGameUIManager.instance.ShowCenterMessageRaw(CenterMessageType.Error, GetCannotTravelReason().reasonText);
	}

	protected static void InvokeUserCode_RpcShowCannotTravelMessage(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcShowCannotTravelMessage called on server.");
		}
		else
		{
			((ZoneManager)(object)obj).UserCode_RpcShowCannotTravelMessage();
		}
	}

	protected void UserCode_RpcInvokeVoteStarted__DewPlayer(DewPlayer player)
	{
		try
		{
			ClientEvent_OnVoteStarted?.Invoke(player);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	protected static void InvokeUserCode_RpcInvokeVoteStarted__DewPlayer(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcInvokeVoteStarted called on server.");
		}
		else
		{
			((ZoneManager)(object)obj).UserCode_RpcInvokeVoteStarted__DewPlayer(NetworkReaderExtensions.ReadNetworkBehaviour<DewPlayer>(reader));
		}
	}

	protected void UserCode_RpcInvokeVoteCanceled__DewPlayer(DewPlayer player)
	{
		try
		{
			ClientEvent_OnVoteCanceled?.Invoke(player);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	protected static void InvokeUserCode_RpcInvokeVoteCanceled__DewPlayer(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcInvokeVoteCanceled called on server.");
		}
		else
		{
			((ZoneManager)(object)obj).UserCode_RpcInvokeVoteCanceled__DewPlayer(NetworkReaderExtensions.ReadNetworkBehaviour<DewPlayer>(reader));
		}
	}

	protected void UserCode_RpcInvokeVoteCompleted()
	{
		ClientEvent_OnVoteCompleted?.Invoke();
	}

	protected static void InvokeUserCode_RpcInvokeVoteCompleted(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcInvokeVoteCompleted called on server.");
		}
		else
		{
			((ZoneManager)(object)obj).UserCode_RpcInvokeVoteCompleted();
		}
	}

	protected void UserCode_CmdTravelToNode__Int32__NetworkConnectionToClient(int index, NetworkConnectionToClient sender)
	{
		if (isInRoomTransition || index < 0 || index >= nodes.Count || !IsNodeConnected(currentNodeIndex, index) || isVoting)
		{
			return;
		}
		DewPlayer player = sender.GetPlayer();
		if (!((UnityEngine.Object)(object)player == null))
		{
			if (ShouldVoteOnTravel())
			{
				StartVoteNextNode(player, index);
			}
			else
			{
				TravelToNode(index);
			}
		}
	}

	protected static void InvokeUserCode_CmdTravelToNode__Int32__NetworkConnectionToClient(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdTravelToNode called on client.");
		}
		else
		{
			((ZoneManager)(object)obj).UserCode_CmdTravelToNode__Int32__NetworkConnectionToClient(NetworkReaderExtensions.ReadInt(reader), senderConnection);
		}
	}

	protected void UserCode_CmdTravelToNextZone__NetworkConnectionToClient(NetworkConnectionToClient sender)
	{
		if (isInRoomTransition || (currentNode.type != WorldNodeType.ExitBoss && (Rift_RoomExit.instance.IsNullOrInactive() || !Rift_RoomExit.instance.forceTravelToNextZone)))
		{
			return;
		}
		DewPlayer player = sender.GetPlayer();
		if (!((UnityEngine.Object)(object)player == null))
		{
			if (ShouldVoteOnTravel())
			{
				StartVoteNextZone(player);
			}
			else
			{
				NetworkedManagerBase<GameManager>.instance.LoadNextZone();
			}
		}
	}

	protected static void InvokeUserCode_CmdTravelToNextZone__NetworkConnectionToClient(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdTravelToNextZone called on client.");
		}
		else
		{
			((ZoneManager)(object)obj).UserCode_CmdTravelToNextZone__NetworkConnectionToClient(senderConnection);
		}
	}

	protected void UserCode_RpcAnnounceRevealedNodes__DewPlayer__Int32(DewPlayer revealer, int nodeCount)
	{
		NetworkedManagerBase<ChatManager>.instance.ShowMessageLocally(new ChatManager.Message
		{
			type = ChatManager.MessageType.Notice,
			content = "InGame_LocationsHaveBeenRevealedBy",
			args = new string[2]
			{
				ChatManager.GetColoredDescribedPlayerName(revealer),
				nodeCount.ToString("#,##0")
			}
		});
	}

	protected static void InvokeUserCode_RpcAnnounceRevealedNodes__DewPlayer__Int32(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcAnnounceRevealedNodes called on server.");
		}
		else
		{
			((ZoneManager)(object)obj).UserCode_RpcAnnounceRevealedNodes__DewPlayer__Int32(NetworkReaderExtensions.ReadNetworkBehaviour<DewPlayer>(reader), NetworkReaderExtensions.ReadInt(reader));
		}
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		((NetworkBehaviour)this).SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			GeneratedNetworkCode._Write_SyncableAssetRef(writer, _currentZone);
			NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)Network_003CcurrentRoom_003Ek__BackingField);
			NetworkWriterExtensions.WriteInt(writer, currentZoneIndex__BackingField);
			NetworkWriterExtensions.WriteInt(writer, currentRoomIndex__BackingField);
			NetworkWriterExtensions.WriteInt(writer, clearedCombatRooms__BackingField);
			NetworkWriterExtensions.WriteInt(writer, currentZoneClearedNodes__BackingField);
			NetworkWriterExtensions.WriteBool(writer, isInRoomTransition__BackingField);
			NetworkWriterExtensions.WriteInt(writer, currentHuntLevel__BackingField);
			NetworkWriterExtensions.WriteBool(writer, isVoting__BackingField);
			NetworkWriterExtensions.WriteInt(writer, voteRemainingSeconds__BackingField);
			GeneratedNetworkCode._Write_VoteType(writer, voteType);
			NetworkWriterExtensions.WriteInt(writer, voteData__BackingField);
			NetworkWriterExtensions.WriteInt(writer, currentNodeIndex__BackingField);
			NetworkWriterExtensions.WriteInt(writer, sidetrackReturnNodeIndex__BackingField);
			NetworkWriterExtensions.WriteUInt(writer, _worldSeed);
			NetworkWriterExtensions.WriteInt(writer, hunterSkippedTurns__BackingField);
			NetworkWriterExtensions.WriteBool(writer, isHuntAdvanceDisabled__BackingField);
			NetworkWriterExtensions.WriteInt(writer, hunterStartNodeIndex__BackingField);
			NetworkWriterExtensions.WriteInt(writer, loopIndex__BackingField);
			NetworkWriterExtensions.WriteInt(writer, currentTier__BackingField);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 1L) != 0L)
		{
			GeneratedNetworkCode._Write_SyncableAssetRef(writer, _currentZone);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 2L) != 0L)
		{
			NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)Network_003CcurrentRoom_003Ek__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 4L) != 0L)
		{
			NetworkWriterExtensions.WriteInt(writer, currentZoneIndex__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 8L) != 0L)
		{
			NetworkWriterExtensions.WriteInt(writer, currentRoomIndex__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x10L) != 0L)
		{
			NetworkWriterExtensions.WriteInt(writer, clearedCombatRooms__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x20L) != 0L)
		{
			NetworkWriterExtensions.WriteInt(writer, currentZoneClearedNodes__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x40L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, isInRoomTransition__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x80L) != 0L)
		{
			NetworkWriterExtensions.WriteInt(writer, currentHuntLevel__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x100L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, isVoting__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x200L) != 0L)
		{
			NetworkWriterExtensions.WriteInt(writer, voteRemainingSeconds__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x400L) != 0L)
		{
			GeneratedNetworkCode._Write_VoteType(writer, voteType);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x800L) != 0L)
		{
			NetworkWriterExtensions.WriteInt(writer, voteData__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x1000L) != 0L)
		{
			NetworkWriterExtensions.WriteInt(writer, currentNodeIndex__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x2000L) != 0L)
		{
			NetworkWriterExtensions.WriteInt(writer, sidetrackReturnNodeIndex__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x4000L) != 0L)
		{
			NetworkWriterExtensions.WriteUInt(writer, _worldSeed);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x8000L) != 0L)
		{
			NetworkWriterExtensions.WriteInt(writer, hunterSkippedTurns__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x10000L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, isHuntAdvanceDisabled__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x20000L) != 0L)
		{
			NetworkWriterExtensions.WriteInt(writer, hunterStartNodeIndex__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x40000L) != 0L)
		{
			NetworkWriterExtensions.WriteInt(writer, loopIndex__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x80000L) != 0L)
		{
			NetworkWriterExtensions.WriteInt(writer, currentTier__BackingField);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		((NetworkBehaviour)this).DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<SyncableAssetRef>(ref _currentZone, (Action<SyncableAssetRef, SyncableAssetRef>)null, GeneratedNetworkCode._Read_SyncableAssetRef(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize_NetworkBehaviour<Room>(ref currentRoom__BackingField, (Action<Room, Room>)null, reader, ref ____003CcurrentRoom_003Ek__BackingFieldNetId);
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref currentZoneIndex__BackingField, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref currentRoomIndex__BackingField, _Mirror_SyncVarHookDelegate__003CcurrentRoomIndex_003Ek__BackingField, NetworkReaderExtensions.ReadInt(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref clearedCombatRooms__BackingField, _Mirror_SyncVarHookDelegate__003CclearedCombatRooms_003Ek__BackingField, NetworkReaderExtensions.ReadInt(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref currentZoneClearedNodes__BackingField, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref isInRoomTransition__BackingField, _Mirror_SyncVarHookDelegate__003CisInRoomTransition_003Ek__BackingField, NetworkReaderExtensions.ReadBool(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref currentHuntLevel__BackingField, _Mirror_SyncVarHookDelegate__003CcurrentHuntLevel_003Ek__BackingField, NetworkReaderExtensions.ReadInt(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref isVoting__BackingField, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref voteRemainingSeconds__BackingField, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<VoteType>(ref voteType, (Action<VoteType, VoteType>)null, GeneratedNetworkCode._Read_VoteType(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref voteData__BackingField, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref currentNodeIndex__BackingField, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref sidetrackReturnNodeIndex__BackingField, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<uint>(ref _worldSeed, (Action<uint, uint>)null, NetworkReaderExtensions.ReadUInt(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref hunterSkippedTurns__BackingField, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref isHuntAdvanceDisabled__BackingField, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref hunterStartNodeIndex__BackingField, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref loopIndex__BackingField, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref currentTier__BackingField, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 1L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<SyncableAssetRef>(ref _currentZone, (Action<SyncableAssetRef, SyncableAssetRef>)null, GeneratedNetworkCode._Read_SyncableAssetRef(reader));
		}
		if ((num & 2L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize_NetworkBehaviour<Room>(ref currentRoom__BackingField, (Action<Room, Room>)null, reader, ref ____003CcurrentRoom_003Ek__BackingFieldNetId);
		}
		if ((num & 4L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref currentZoneIndex__BackingField, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
		}
		if ((num & 8L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref currentRoomIndex__BackingField, _Mirror_SyncVarHookDelegate__003CcurrentRoomIndex_003Ek__BackingField, NetworkReaderExtensions.ReadInt(reader));
		}
		if ((num & 0x10L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref clearedCombatRooms__BackingField, _Mirror_SyncVarHookDelegate__003CclearedCombatRooms_003Ek__BackingField, NetworkReaderExtensions.ReadInt(reader));
		}
		if ((num & 0x20L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref currentZoneClearedNodes__BackingField, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
		}
		if ((num & 0x40L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref isInRoomTransition__BackingField, _Mirror_SyncVarHookDelegate__003CisInRoomTransition_003Ek__BackingField, NetworkReaderExtensions.ReadBool(reader));
		}
		if ((num & 0x80L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref currentHuntLevel__BackingField, _Mirror_SyncVarHookDelegate__003CcurrentHuntLevel_003Ek__BackingField, NetworkReaderExtensions.ReadInt(reader));
		}
		if ((num & 0x100L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref isVoting__BackingField, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
		}
		if ((num & 0x200L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref voteRemainingSeconds__BackingField, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
		}
		if ((num & 0x400L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<VoteType>(ref voteType, (Action<VoteType, VoteType>)null, GeneratedNetworkCode._Read_VoteType(reader));
		}
		if ((num & 0x800L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref voteData__BackingField, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
		}
		if ((num & 0x1000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref currentNodeIndex__BackingField, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
		}
		if ((num & 0x2000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref sidetrackReturnNodeIndex__BackingField, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
		}
		if ((num & 0x4000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<uint>(ref _worldSeed, (Action<uint, uint>)null, NetworkReaderExtensions.ReadUInt(reader));
		}
		if ((num & 0x8000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref hunterSkippedTurns__BackingField, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
		}
		if ((num & 0x10000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref isHuntAdvanceDisabled__BackingField, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
		}
		if ((num & 0x20000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref hunterStartNodeIndex__BackingField, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
		}
		if ((num & 0x40000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref loopIndex__BackingField, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
		}
		if ((num & 0x80000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref currentTier__BackingField, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
		}
	}
}
