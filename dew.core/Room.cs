using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(RoomMap))]
[RequireComponent(typeof(RoomMonsters))]
[RequireComponent(typeof(RoomProps))]
[RequireComponent(typeof(RoomRifts))]
[RequireComponent(typeof(RoomRewards))]
[RequireComponent(typeof(RoomModifiers))]
[RequireComponent(typeof(RoomEditorMaintenance))]
public class Room : SingletonDewNetworkBehaviour<Room>
{
	public SafeAction<Entity> ClientEvent_OnFootstep;

	public SafeAction<Room_Waypoint> ClientEvent_WaypointUnlocked;

	public SafeAction<Room_Waypoint> ClientEvent_WaypointLocked;

	public SafeAction ClientEvent_OnRoomStart;

	public DewRoomMetadata metadata;

	public DewMusicItem music;

	public DewSurfaceData defaultSurface;

	public float[] availableCameraAngles = new float[1];

	public bool openRoomExitOnClear = true;

	[CompilerGenerated]
	[SyncVar]
	private int numOfActivatedCombatAreas__BackingField;

	private RoomSection[] _sections;

	private Room_HeroSpawnPos[] _heroSpawnPoses;

	private bool _didSetupHeroSpawnPos;

	[CompilerGenerated]
	[SyncVar]
	private bool isActive__BackingField;

	[CompilerGenerated]
	[SyncVar]
	private bool isRevisit__BackingField;

	public SafeAction ClientEvent_OnPrepareAsNonHostileRoom;

	[CompilerGenerated]
	[SyncVar(hook = "OnIsNonHostileRoomChanged")]
	private bool isNonHostileRoom__BackingField;

	public SafeAction ClientEvent_OnRoomClear;

	public SafeAction onRemoveObstacles;

	public UnityEvent onRoomClear;

	private readonly List<Room_Waypoint> _unlockedWaypoints = new List<Room_Waypoint>();

	public readonly List<Vector3> playerPathablePoints = new List<Vector3>();

	[SaveVar(SaveVarFlags.Default)]
	private Dictionary<uint, DewRandom> _randomInstances = new Dictionary<uint, DewRandom>();

	public Action<bool, bool> _Mirror_SyncVarHookDelegate__003CisNonHostileRoom_003Ek__BackingField;

	public RoomMap map { get; private set; }

	public RoomMonsters monsters { get; private set; }

	public RoomProps props { get; private set; }

	public RoomRifts rifts { get; private set; }

	public RoomRewards rewards { get; private set; }

	public RoomModifiers modifiers { get; private set; }

	public int numOfActivatedCombatAreas
	{
		[CompilerGenerated]
		get
		{
			return numOfActivatedCombatAreas__BackingField;
		}
		[CompilerGenerated]
		internal set
		{
			Network_003CnumOfActivatedCombatAreas_003Ek__BackingField = value;
		}
	}

	public IList<RoomSection> sections => _sections;

	public bool isActive
	{
		[CompilerGenerated]
		get
		{
			return isActive__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			Network_003CisActive_003Ek__BackingField = value;
		}
	}

	public bool isRevisit
	{
		[CompilerGenerated]
		get
		{
			return isRevisit__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CisRevisit_003Ek__BackingField = value;
		}
	}

	public Vector3 heroSpawnPos { get; private set; }

	public Quaternion heroSpawnRot { get; private set; }

	public bool isNonHostileRoom
	{
		[CompilerGenerated]
		get
		{
			return isNonHostileRoom__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CisNonHostileRoom_003Ek__BackingField = value;
		}
	}

	public bool didRemoveObstacles { get; set; }

	public bool didClearRoom { get; private set; }

	public IReadOnlyList<Room_Waypoint> unlockedWaypoints => _unlockedWaypoints;

	public int Network_003CnumOfActivatedCombatAreas_003Ek__BackingField
	{
		get
		{
			return numOfActivatedCombatAreas__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<int>(value, ref numOfActivatedCombatAreas__BackingField, 1uL, (Action<int, int>)null);
		}
	}

	public bool Network_003CisActive_003Ek__BackingField
	{
		get
		{
			return isActive__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref isActive__BackingField, 2uL, (Action<bool, bool>)null);
		}
	}

	public bool Network_003CisRevisit_003Ek__BackingField
	{
		get
		{
			return isRevisit__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref isRevisit__BackingField, 4uL, (Action<bool, bool>)null);
		}
	}

	public bool Network_003CisNonHostileRoom_003Ek__BackingField
	{
		get
		{
			return isNonHostileRoom__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref isNonHostileRoom__BackingField, 8uL, _Mirror_SyncVarHookDelegate__003CisNonHostileRoom_003Ek__BackingField);
		}
	}

	private void OnIsNonHostileRoomChanged(bool prev, bool newVal)
	{
		if (isNonHostileRoom)
		{
			ClientEvent_OnPrepareAsNonHostileRoom?.Invoke();
		}
	}

	protected override void Awake()
	{
		base.Awake();
		map = ((Component)(object)this).GetComponent<RoomMap>();
		monsters = ((Component)(object)this).GetComponent<RoomMonsters>();
		props = ((Component)(object)this).GetComponent<RoomProps>();
		rifts = ((Component)(object)this).GetComponent<RoomRifts>();
		rewards = ((Component)(object)this).GetComponent<RoomRewards>();
		modifiers = ((Component)(object)this).GetComponent<RoomModifiers>();
		if (defaultSurface == null)
		{
			defaultSurface = Resources.Load<GameObject>("Footsteps/Surface_Default").GetComponent<DewSurfaceData>();
		}
	}

	private void Start()
	{
		_sections = UnityEngine.Object.FindObjectsOfType<RoomSection>();
	}

	public override void OnStartServer()
	{
		base.OnStartServer();
		onRoomClear.AddListener(() =>
		{
			if (NetworkServer.active)
			{
				RpcInvokeOnRoomClear();
			}
		});
	}

	public override void OnStartClient()
	{
		base.OnStartClient();
		IPlayerPathablePoint[] array = Dew.FindInterfacesOfType<IPlayerPathablePoint>(includeInactive: true);
		foreach (IPlayerPathablePoint playerPathablePoint in array)
		{
			if (playerPathablePoint is Component component)
			{
				Actor componentInParent = component.GetComponentInParent<Actor>();
				if ((UnityEngine.Object)(object)componentInParent != null && componentInParent.IsNullOrInactive())
				{
					continue;
				}
			}
			playerPathablePoints.Add(Dew.GetValidAgentPosition(playerPathablePoint.pathablePosition));
		}
	}

	[ClientRpc]
	private void RpcInvokeOnRoomClear()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendRPCInternal("System.Void Room::RpcInvokeOnRoomClear()", 109277974, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	public override void OnLateStartServer()
	{
		base.OnLateStartServer();
		if (NetworkedManagerBase<ZoneManager>.instance.currentNodeIndex < 0)
		{
			return;
		}
		if (NetworkedManagerBase<ZoneManager>.instance.currentNode.roomRotIndex < 0)
		{
			if (!NetworkedManagerBase<ZoneManager>.instance._usedAngleIndexes.TryGetValue(((UnityEngine.Object)(object)this).name, out var value))
			{
				value = new List<int>();
				NetworkedManagerBase<ZoneManager>.instance._usedAngleIndexes.Add(((UnityEngine.Object)(object)this).name, value);
			}
			List<int> list = DewPool.GetList(out ListReturnHandle<int> handle);
			for (int i = 0; i < availableCameraAngles.Length; i++)
			{
				if (!value.Contains(i))
				{
					list.Add(i);
				}
			}
			float roomRotValue = NetworkedManagerBase<ZoneManager>.instance.currentNode.roomRotValue;
			int num;
			if (list.Count == 0)
			{
				num = Mathf.Clamp((int)(roomRotValue * (float)availableCameraAngles.Length), 0, availableCameraAngles.Length - 1);
				value.Clear();
				value.Add(num);
			}
			else
			{
				num = list[Mathf.Clamp((int)(roomRotValue * (float)list.Count), 0, list.Count - 1)];
				value.Add(num);
			}
			handle.Return();
			NetworkedManagerBase<ZoneManager>.instance.SetRoomRotIndex(NetworkedManagerBase<ZoneManager>.instance.currentNodeIndex, num);
		}
		SyncCameraAngle();
	}

	[Server]
	public void SyncCameraAngle()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Room::SyncCameraAngle()' called when server was not active");
			return;
		}
		int roomRotIndex = NetworkedManagerBase<ZoneManager>.instance.currentNode.roomRotIndex;
		SetCameraAngleIndex_Local(roomRotIndex);
		RpcSetCameraAngleIndex_Imp(roomRotIndex);
	}

	[ClientRpc]
	internal void RpcSetCameraAngleIndex_Imp(int index)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteInt((NetworkWriter)(object)val, index);
		((NetworkBehaviour)this).SendRPCInternal("System.Void Room::RpcSetCameraAngleIndex_Imp(System.Int32)", 45229983, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	public void SetCameraAngleIndex_Local(int index)
	{
		if (availableCameraAngles == null || index < 0 || index >= availableCameraAngles.Length)
		{
			ManagerBase<CameraManager>.instance.entityCamAngle = 0f;
			Debug.LogWarning("Received invalid camera angle for room " + ((UnityEngine.Object)(object)this).name);
		}
		else
		{
			ManagerBase<CameraManager>.instance.entityCamAngle = availableCameraAngles[index];
		}
		ManagerBase<CameraManager>.instance.SnapCameraToFocusedEntity();
	}

	public override void OnLateStart()
	{
		base.OnLateStart();
		if (music == null && NetworkedManagerBase<ZoneManager>.instance.currentNode.type != WorldNodeType.ExitBoss && !NetworkedManagerBase<ZoneManager>.instance.isSidetracking)
		{
			music = NetworkedManagerBase<ZoneManager>.instance.currentZone.defaultMusicRef.asset;
		}
		ManagerBase<MusicManager>.instance.Play(music);
	}

	public Vector3 GetHeroSpawnPosition()
	{
		Vector3 vector = heroSpawnPos;
		Vector3 position = vector + GetRoomRandom(-7617).OnUnitSphere() * 2.5f;
		position = Dew.GetPositionOnGround(position);
		return Dew.GetValidAgentDestination_LinearSweep(vector, position);
	}

	public Quaternion GetHeroSpawnRotation()
	{
		return heroSpawnRot;
	}

	public RoomSection GetSectionFromWorldPos(Vector3 pos)
	{
		foreach (RoomSection section in sections)
		{
			if (section.OverlapPoint(pos.ToXY()))
			{
				return section;
			}
		}
		return null;
	}

	[Server]
	public void ClearRoom()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Room::ClearRoom()' called when server was not active");
		}
		else
		{
			((MonoBehaviour)(object)this).StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			if (!didClearRoom)
			{
				didClearRoom = true;
				try
				{
					onRoomClear?.Invoke();
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
				}
				foreach (Entity item in new List<Entity>(NetworkedManagerBase<ActorManager>.instance.allEntities))
				{
					if (!item.IsNullInactiveDeadOrKnockedOut() && item is Monster monster && !((UnityEngine.Object)(object)monster.owner != (UnityEngine.Object)(object)DewPlayer.creep) && !monster.AI.disableAI && !NetworkedManagerBase<ZoneManager>.instance.isCurrentNodeHunted && !(Time.time - monster.creationTime < 3f) && (monster.isSleeping || (UnityEngine.Object)(object)monster.AI.context.targetEnemy == null))
					{
						Debug.Log("Destroying leftover monster: " + item.GetActorReadableName());
						item.Destroy();
					}
				}
				if (openRoomExitOnClear)
				{
					rifts.OpenRifts();
				}
			}
			yield break;
		}
	}

	[Server]
	public void AddUnlockedWaypoint(Room_Waypoint waypoint)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Room::AddUnlockedWaypoint(Room_Waypoint)' called when server was not active");
		}
		else if (!_unlockedWaypoints.Contains(waypoint))
		{
			_unlockedWaypoints.Add(waypoint);
			RpcInvokeWaypointEvent(waypoint, isUnlocked: true);
		}
	}

	[Server]
	public void RemoveUnlockedWaypoint(Room_Waypoint waypoint)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Room::RemoveUnlockedWaypoint(Room_Waypoint)' called when server was not active");
		}
		else if (_unlockedWaypoints.Contains(waypoint))
		{
			_unlockedWaypoints.Remove(waypoint);
			RpcInvokeWaypointEvent(waypoint, isUnlocked: false);
		}
	}

	[ClientRpc]
	private void RpcInvokeWaypointEvent(Room_Waypoint waypoint, bool isUnlocked)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteNetworkBehaviour((NetworkWriter)(object)val, (NetworkBehaviour)(object)waypoint);
		NetworkWriterExtensions.WriteBool((NetworkWriter)(object)val, isUnlocked);
		((NetworkBehaviour)this).SendRPCInternal("System.Void Room::RpcInvokeWaypointEvent(Room_Waypoint,System.Boolean)", -1139962996, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[Server]
	public void StartRoom()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Room::StartRoom()' called when server was not active");
			return;
		}
		_sections = UnityEngine.Object.FindObjectsOfType<RoomSection>();
		Network_003CisActive_003Ek__BackingField = true;
		didClearRoom = isRevisit;
		if (!_didSetupHeroSpawnPos)
		{
			Room_HeroSpawnPos[] array = UnityEngine.Object.FindObjectsOfType<Room_HeroSpawnPos>();
			Transform transform = array[GetRoomRandom(-991).Range(0, array.Length)].transform;
			_didSetupHeroSpawnPos = true;
			heroSpawnPos = Dew.GetValidAgentPosition(transform.position);
			heroSpawnRot = transform.rotation;
		}
		RoomComponent[] componentsInChildren = ((Component)(object)this).GetComponentsInChildren<RoomComponent>();
		componentsInChildren = GetSortedByStartDependency(componentsInChildren);
		RoomComponent[] array2 = componentsInChildren;
		foreach (RoomComponent roomComponent in array2)
		{
			try
			{
				roomComponent.isRoomActive = true;
				roomComponent.OnRoomStartServer();
				roomComponent.OnRoomStart();
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
		Dew.PrewarmBanRoomNodeCache();
		ClientEvent_OnRoomStart?.Invoke();
		RpcInvokeRoomStart();
	}

	[ClientRpc]
	private void RpcInvokeRoomStart()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendRPCInternal("System.Void Room::RpcInvokeRoomStart()", -1029386294, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[Server]
	public void StopRoom()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Room::StopRoom()' called when server was not active");
			return;
		}
		Network_003CisActive_003Ek__BackingField = false;
		RoomComponent[] componentsInChildren = ((Component)(object)this).GetComponentsInChildren<RoomComponent>();
		foreach (RoomComponent roomComponent in componentsInChildren)
		{
			try
			{
				roomComponent.isRoomActive = false;
				roomComponent.OnRoomStopServer();
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
		RpcInvokeRoomStop();
	}

	[ClientRpc]
	private void RpcInvokeRoomStop()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendRPCInternal("System.Void Room::RpcInvokeRoomStop()", 1906869436, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[Server]
	public void OverrideHeroSpawn(Vector3 pos, Quaternion rot)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Room::OverrideHeroSpawn(UnityEngine.Vector3,UnityEngine.Quaternion)' called when server was not active");
			return;
		}
		_didSetupHeroSpawnPos = true;
		heroSpawnPos = pos;
		heroSpawnRot = rot;
	}

	public RoomSection GetFinalSection()
	{
		if (Rift_RoomExit.instance.IsNullOrInactive() && Rift.instance.IsNullOrInactive())
		{
			return null;
		}
		Vector3 finalRiftPos = ((!Rift_RoomExit.instance.IsNullOrInactive()) ? ((Component)(object)Rift_RoomExit.instance).transform.position : ((Component)(object)Rift.instance).transform.position);
		return Dew.SelectBestWithScore(sections, (RoomSection section, int i) => 0f - Vector2.Distance(finalRiftPos.ToXY(), section.transform.position.ToXY()));
	}

	private RoomComponent[] GetSortedByStartDependency(RoomComponent[] arr)
	{
		RoomComponentStartDependencyAttribute[][] array = new RoomComponentStartDependencyAttribute[arr.Length][];
		for (int i = 0; i < arr.Length; i++)
		{
			object[] customAttributes = ((object)arr[i]).GetType().GetCustomAttributes(typeof(RoomComponentStartDependencyAttribute), inherit: true);
			array[i] = new RoomComponentStartDependencyAttribute[customAttributes.Length];
			for (int j = 0; j < customAttributes.Length; j++)
			{
				array[i][j] = (RoomComponentStartDependencyAttribute)customAttributes[j];
			}
		}
		Dictionary<Type, int> dictionary = new Dictionary<Type, int>();
		for (int k = 0; k < arr.Length; k++)
		{
			dictionary[((object)arr[k]).GetType()] = k;
		}
		List<RoomComponent> list = new List<RoomComponent>();
		Queue<RoomComponent> queue = new Queue<RoomComponent>();
		foreach (RoomComponent roomComponent in arr)
		{
			if (array[dictionary[((object)roomComponent).GetType()]].Length == 0)
			{
				queue.Enqueue(roomComponent);
			}
		}
		while (queue.Count > 0)
		{
			RoomComponent rc = queue.Dequeue();
			list.Add(rc);
			for (int m = 0; m < arr.Length; m++)
			{
				if (array[m].Any((RoomComponentStartDependencyAttribute dep) => dep.targetRoomComponent == ((object)rc).GetType()))
				{
					array[m] = array[m].Where((RoomComponentStartDependencyAttribute dep) => dep.targetRoomComponent != ((object)rc).GetType()).ToArray();
					if (array[m].Length == 0)
					{
						queue.Enqueue(arr[m]);
					}
				}
			}
		}
		if (list.Count < arr.Length)
		{
			throw new Exception("Circular dependency detected.");
		}
		return list.ToArray();
	}

	[Server]
	public void RemoveCombat(bool clearRoomOnEnteringLastSection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Room::RemoveCombat(System.Boolean)' called when server was not active");
			return;
		}
		if (clearRoomOnEnteringLastSection)
		{
			GetFinalSection().clearRoomOnEnterFirstTime = true;
		}
		foreach (RoomSection section in sections)
		{
			section.monsters.combatAreaSettings = SectionCombatAreaType.No;
			section.monsters.isMarkedAsCombatArea = false;
		}
		monsters.RemoveAllCamps();
		Entity[] array = NetworkedManagerBase<ActorManager>.instance.allEntities.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] is Monster monster && monster.GetTeamRelation(DewPlayer.local) == TeamRelation.Enemy)
			{
				monster.Destroy();
			}
		}
	}

	[Server]
	public void RemoveObstacles()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Room::RemoveObstacles()' called when server was not active");
		}
		else if (!didRemoveObstacles)
		{
			didRemoveObstacles = true;
			onRemoveObstacles?.Invoke();
		}
	}

	[Server]
	public DewRandom GetRoomRandom(int offset)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'DewRandom Room::GetRoomRandom(System.Int32)' called when server was not active");
			return null;
		}
		long num = NetworkedManagerBase<ZoneManager>.instance.worldSeed + NetworkedManagerBase<ZoneManager>.instance.currentNodeIndex;
		if (!_randomInstances.TryGetValue((uint)(num + offset), out var value))
		{
			value = new DewRandom((uint)(num + offset));
			_randomInstances.Add((uint)(num + offset), value);
		}
		return value;
	}

	public Room()
	{
		_Mirror_SyncVarHookDelegate__003CisNonHostileRoom_003Ek__BackingField = OnIsNonHostileRoomChanged;
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcInvokeOnRoomClear()
	{
		ClientEvent_OnRoomClear?.Invoke();
	}

	protected static void InvokeUserCode_RpcInvokeOnRoomClear(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcInvokeOnRoomClear called on server.");
		}
		else
		{
			((Room)(object)obj).UserCode_RpcInvokeOnRoomClear();
		}
	}

	protected void UserCode_RpcSetCameraAngleIndex_Imp__Int32(int index)
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			SetCameraAngleIndex_Local(index);
		}
	}

	protected static void InvokeUserCode_RpcSetCameraAngleIndex_Imp__Int32(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetCameraAngleIndex_Imp called on server.");
		}
		else
		{
			((Room)(object)obj).UserCode_RpcSetCameraAngleIndex_Imp__Int32(NetworkReaderExtensions.ReadInt(reader));
		}
	}

	protected void UserCode_RpcInvokeWaypointEvent__Room_Waypoint__Boolean(Room_Waypoint waypoint, bool isUnlocked)
	{
		try
		{
			if (isUnlocked)
			{
				ClientEvent_WaypointUnlocked?.Invoke(waypoint);
			}
			else
			{
				ClientEvent_WaypointLocked?.Invoke(waypoint);
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	protected static void InvokeUserCode_RpcInvokeWaypointEvent__Room_Waypoint__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcInvokeWaypointEvent called on server.");
		}
		else
		{
			((Room)(object)obj).UserCode_RpcInvokeWaypointEvent__Room_Waypoint__Boolean(NetworkReaderExtensions.ReadNetworkBehaviour<Room_Waypoint>(reader), NetworkReaderExtensions.ReadBool(reader));
		}
	}

	protected void UserCode_RpcInvokeRoomStart()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			return;
		}
		RoomComponent[] componentsInChildren = ((Component)(object)this).GetComponentsInChildren<RoomComponent>();
		componentsInChildren = GetSortedByStartDependency(componentsInChildren);
		_sections = UnityEngine.Object.FindObjectsOfType<RoomSection>();
		RoomComponent[] array = componentsInChildren;
		foreach (RoomComponent roomComponent in array)
		{
			try
			{
				roomComponent.isRoomActive = true;
				roomComponent.OnRoomStart();
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
		ClientEvent_OnRoomStart?.Invoke();
	}

	protected static void InvokeUserCode_RpcInvokeRoomStart(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcInvokeRoomStart called on server.");
		}
		else
		{
			((Room)(object)obj).UserCode_RpcInvokeRoomStart();
		}
	}

	protected void UserCode_RpcInvokeRoomStop()
	{
		RoomComponent[] componentsInChildren = ((Component)(object)this).GetComponentsInChildren<RoomComponent>();
		foreach (RoomComponent roomComponent in componentsInChildren)
		{
			try
			{
				roomComponent.isRoomActive = false;
				roomComponent.OnRoomStop();
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
	}

	protected static void InvokeUserCode_RpcInvokeRoomStop(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcInvokeRoomStop called on server.");
		}
		else
		{
			((Room)(object)obj).UserCode_RpcInvokeRoomStop();
		}
	}

	static Room()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Expected Obj, but got Unknown
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Expected Obj, but got Unknown
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Expected Obj, but got Unknown
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterRpc(typeof(Room), "System.Void Room::RpcInvokeOnRoomClear()", (RemoteCallDelegate)InvokeUserCode_RpcInvokeOnRoomClear);
		RemoteProcedureCalls.RegisterRpc(typeof(Room), "System.Void Room::RpcSetCameraAngleIndex_Imp(System.Int32)", (RemoteCallDelegate)InvokeUserCode_RpcSetCameraAngleIndex_Imp__Int32);
		RemoteProcedureCalls.RegisterRpc(typeof(Room), "System.Void Room::RpcInvokeWaypointEvent(Room_Waypoint,System.Boolean)", (RemoteCallDelegate)InvokeUserCode_RpcInvokeWaypointEvent__Room_Waypoint__Boolean);
		RemoteProcedureCalls.RegisterRpc(typeof(Room), "System.Void Room::RpcInvokeRoomStart()", (RemoteCallDelegate)InvokeUserCode_RpcInvokeRoomStart);
		RemoteProcedureCalls.RegisterRpc(typeof(Room), "System.Void Room::RpcInvokeRoomStop()", (RemoteCallDelegate)InvokeUserCode_RpcInvokeRoomStop);
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		((NetworkBehaviour)this).SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteInt(writer, numOfActivatedCombatAreas__BackingField);
			NetworkWriterExtensions.WriteBool(writer, isActive__BackingField);
			NetworkWriterExtensions.WriteBool(writer, isRevisit__BackingField);
			NetworkWriterExtensions.WriteBool(writer, isNonHostileRoom__BackingField);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 1L) != 0L)
		{
			NetworkWriterExtensions.WriteInt(writer, numOfActivatedCombatAreas__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 2L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, isActive__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 4L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, isRevisit__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 8L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, isNonHostileRoom__BackingField);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		((NetworkBehaviour)this).DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref numOfActivatedCombatAreas__BackingField, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref isActive__BackingField, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref isRevisit__BackingField, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref isNonHostileRoom__BackingField, _Mirror_SyncVarHookDelegate__003CisNonHostileRoom_003Ek__BackingField, NetworkReaderExtensions.ReadBool(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 1L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref numOfActivatedCombatAreas__BackingField, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
		}
		if ((num & 2L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref isActive__BackingField, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
		}
		if ((num & 4L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref isRevisit__BackingField, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
		}
		if ((num & 8L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref isNonHostileRoom__BackingField, _Mirror_SyncVarHookDelegate__003CisNonHostileRoom_003Ek__BackingField, NetworkReaderExtensions.ReadBool(reader));
		}
	}
}
