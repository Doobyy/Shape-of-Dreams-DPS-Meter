using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

public abstract class Shrine : Actor, IInteractable, IProp, IBanRoomNodesNearby
{
	public SafeAction<Entity> ClientEvent_OnSuccessfulUse;

	private NavMeshObstacle _obstacle;

	public MapItemVisibility mapVisibility;

	public GameObject model;

	public GameObject availableEffect;

	public GameObject unavailableEffect;

	public GameObject lockedEffect;

	public GameObject unlockEffect;

	public GameObject useLockedEffect;

	public GameObject useEffect;

	public GameObject useEffectLocal;

	public float interactableDelay;

	[SyncVar]
	[SaveVar(SaveVarFlags.Default)]
	[FormerlySerializedAs("maxUseCount")]
	public int maxTotalUseCount = -1;

	public int maxUseCountPerPlayer = -1;

	public float useCooldown = 1f;

	public int baseGoldCost;

	public bool unavailableByDefault;

	[SerializeField]
	private bool _scaleSpawnRateWithPlayers = true;

	[CompilerGenerated]
	[SyncVar]
	private int totalUseCount__BackingField;

	[SyncVar]
	private double _lastUseTime = double.NegativeInfinity;

	[SaveVar(SaveVarFlags.Default)]
	private readonly SyncDictionary<string, int> _useCountByPlayer = new SyncDictionary<string, int>();

	[CompilerGenerated]
	[SyncVar(hook = "OnIsAvailableChanged")]
	private bool isAvailable__BackingField;

	[CompilerGenerated]
	[SyncVar(hook = "OnIsLockedChanged")]
	private bool isLocked__BackingField;

	public Action<bool, bool> _Mirror_SyncVarHookDelegate__003CisAvailable_003Ek__BackingField;

	public Action<bool, bool> _Mirror_SyncVarHookDelegate__003CisLocked_003Ek__BackingField;

	public virtual bool isRegularReward => false;

	int IInteractable.priority => 50;

	[SaveVar(SaveVarFlags.Default)]
	public int totalUseCount
	{
		[CompilerGenerated]
		get
		{
			return totalUseCount__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CtotalUseCount_003Ek__BackingField = value;
		}
	}

	public bool isOnCooldown
	{
		get
		{
			if (NetworkTime.time - _lastUseTime < (double)useCooldown)
			{
				return useCooldown > 0.01f;
			}
			return false;
		}
	}

	[SaveVar(SaveVarFlags.Default)]
	public bool isAvailable
	{
		[CompilerGenerated]
		get
		{
			return isAvailable__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CisAvailable_003Ek__BackingField = value;
		}
	}

	[SaveVar(SaveVarFlags.Default)]
	public bool isLocked
	{
		[CompilerGenerated]
		get
		{
			return isLocked__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CisLocked_003Ek__BackingField = value;
		}
	}

	public virtual Transform interactPivot => ((Component)(object)this).transform;

	public virtual bool canInteractWithMouse => false;

	public float focusDistance => 3f;

	public bool scaleSpawnRateWithPlayers => _scaleSpawnRateWithPlayers;

	public int NetworkmaxTotalUseCount
	{
		get
		{
			return maxTotalUseCount;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<int>(value, ref maxTotalUseCount, 8uL, (Action<int, int>)null);
		}
	}

	public int Network_003CtotalUseCount_003Ek__BackingField
	{
		get
		{
			return totalUseCount__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<int>(value, ref totalUseCount__BackingField, 16uL, (Action<int, int>)null);
		}
	}

	public double Network_lastUseTime
	{
		get
		{
			return _lastUseTime;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<double>(value, ref _lastUseTime, 32uL, (Action<double, double>)null);
		}
	}

	public bool Network_003CisAvailable_003Ek__BackingField
	{
		get
		{
			return isAvailable__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref isAvailable__BackingField, 64uL, _Mirror_SyncVarHookDelegate__003CisAvailable_003Ek__BackingField);
		}
	}

	public bool Network_003CisLocked_003Ek__BackingField
	{
		get
		{
			return isLocked__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref isLocked__BackingField, 128uL, _Mirror_SyncVarHookDelegate__003CisLocked_003Ek__BackingField);
		}
	}

	public void ResetUseCounts()
	{
		Network_003CtotalUseCount_003Ek__BackingField = 0;
		((SyncIDictionary<string, int>)(object)_useCountByPlayer).Clear();
		UpdateIsAvailable();
	}

	public int GetUseCountByPlayer(DewPlayer player)
	{
		return CollectionExtensions.GetValueOrDefault<string, int>((IReadOnlyDictionary<string, int>)_useCountByPlayer, player.guid, 0);
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			if (isNewInstance)
			{
				Network_003CisAvailable_003Ek__BackingField = !unavailableByDefault;
			}
			else
			{
				UpdateIsAvailable();
			}
			NetworkedManagerBase<ClientEventManager>.instance.OnHeroRevive += new Action<Hero>(UpdateIsAvailable);
			NetworkedManagerBase<ClientEventManager>.instance.OnHeroKnockedOut += new Action<Hero>(UpdateIsAvailable);
			NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnHeroAdd += new Action<Hero>(UpdateIsAvailable);
			NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnHeroRemove += new Action<Hero>(UpdateIsAvailable);
		}
		if (!isAvailable)
		{
			OnIsAvailableChanged(oldVal: true, newVal: false);
		}
	}

	[Server]
	public void MakeAvailable()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Shrine::MakeAvailable()' called when server was not active");
		}
		else
		{
			Network_003CisAvailable_003Ek__BackingField = true;
		}
	}

	[Server]
	public void MakeUnavailable()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Shrine::MakeUnavailable()' called when server was not active");
		}
		else
		{
			Network_003CisAvailable_003Ek__BackingField = false;
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (availableEffect != null)
		{
			FxStop(availableEffect);
		}
		if (unavailableEffect != null)
		{
			FxStop(unavailableEffect);
		}
		if (lockedEffect != null)
		{
			FxStop(lockedEffect);
		}
		if (model != null)
		{
			ListReturnHandle<Renderer> handle;
			foreach (Renderer item in model.GetComponentsInChildrenNonAlloc(out handle))
			{
				item.enabled = false;
			}
			handle.Return();
		}
		if (((NetworkBehaviour)this).isServer)
		{
			if ((UnityEngine.Object)(object)NetworkedManagerBase<ClientEventManager>.instance != null)
			{
				NetworkedManagerBase<ClientEventManager>.instance.OnHeroRevive -= new Action<Hero>(UpdateIsAvailable);
				NetworkedManagerBase<ClientEventManager>.instance.OnHeroKnockedOut -= new Action<Hero>(UpdateIsAvailable);
			}
			if ((UnityEngine.Object)(object)NetworkedManagerBase<ActorManager>.instance != null)
			{
				NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnHeroAdd -= new Action<Hero>(UpdateIsAvailable);
				NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnHeroRemove -= new Action<Hero>(UpdateIsAvailable);
			}
		}
	}

	protected abstract bool OnUse(Entity entity);

	public virtual Cost? GetCost(Entity activator)
	{
		if (baseGoldCost > 0)
		{
			return Cost.Gold(Mathf.RoundToInt(NetworkedManagerBase<GameManager>.instance.GetAdjustedGoldAmount_Cost_Service(baseGoldCost)));
		}
		return null;
	}

	public AffordType CanAfford(Entity entity)
	{
		return GetCost(entity).CanAfford(entity);
	}

	private void OnIsAvailableChanged(bool oldVal, bool newVal)
	{
		if (newVal)
		{
			if (availableEffect != null)
			{
				FxPlay(availableEffect);
			}
			if (unavailableEffect != null)
			{
				FxStop(unavailableEffect);
			}
		}
		else
		{
			if (availableEffect != null)
			{
				FxStop(availableEffect);
			}
			if (unavailableEffect != null)
			{
				FxPlay(unavailableEffect);
			}
		}
	}

	private void OnIsLockedChanged(bool oldVal, bool newVal)
	{
		if (newVal)
		{
			FxPlay(lockedEffect);
			FxStop(unlockEffect);
		}
		else
		{
			FxPlay(unlockEffect);
			FxStop(lockedEffect);
		}
	}

	public virtual bool CanInteract(Entity entity)
	{
		if (isAvailable && Time.time - creationTime > interactableDelay && !isOnCooldown)
		{
			if (maxUseCountPerPlayer >= 0)
			{
				return GetUseCountByPlayer(entity.owner) < maxUseCountPerPlayer;
			}
			return true;
		}
		return false;
	}

	public virtual void OnInteract(Entity entity, bool alt)
	{
		if (!alt && ((NetworkBehaviour)this).isServer)
		{
			if (isLocked)
			{
				FxPlayNew(useLockedEffect, entity);
			}
			else if (CanAfford(entity) == AffordType.Yes && (maxTotalUseCount < 0 || totalUseCount < maxTotalUseCount) && (maxUseCountPerPlayer < 0 || GetUseCountByPlayer(entity.owner) < maxUseCountPerPlayer) && isAvailable && OnUse(entity))
			{
				DoPostUseRoutines(entity);
			}
		}
	}

	public void DoPostUseRoutines(Entity entity)
	{
		Cost? cost = GetCost(entity);
		totalUseCount++;
		((SyncIDictionary<string, int>)(object)_useCountByPlayer)[entity.owner.guid] = GetUseCountByPlayer(entity.owner) + 1;
		FxPlayNewNetworked(useEffect, entity);
		TpcShowLocalUseEffect((NetworkConnection)(object)(NetworkConnectionToClient)entity.owner, entity);
		if (cost.HasValue)
		{
			entity.owner.Spend(cost.Value);
		}
		Network_lastUseTime = NetworkTime.time;
		UpdateIsAvailable();
		RpcInvokeOnSuccessfulUse(entity);
	}

	[TargetRpc]
	private void TpcShowLocalUseEffect(NetworkConnection conn, Entity entity)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteNetworkBehaviour((NetworkWriter)(object)val, (NetworkBehaviour)(object)entity);
		((NetworkBehaviour)this).SendTargetRPCInternal(conn, "System.Void Shrine::TpcShowLocalUseEffect(Mirror.NetworkConnection,Entity)", -1934875115, (NetworkWriter)(object)val, 0);
		NetworkWriterPool.Return(val);
	}

	private void UpdateIsAvailable(Hero _)
	{
		UpdateIsAvailable();
	}

	private void UpdateIsAvailable()
	{
		if (maxTotalUseCount >= 0 && totalUseCount >= maxTotalUseCount)
		{
			Network_003CisAvailable_003Ek__BackingField = false;
			return;
		}
		if (maxUseCountPerPlayer < 0)
		{
			Network_003CisAvailable_003Ek__BackingField = true;
			return;
		}
		foreach (DewPlayer gamePlayer in DewPlayer.gamePlayers)
		{
			if (!gamePlayer.hero.IsNullInactiveDeadOrKnockedOut() && GetUseCountByPlayer(gamePlayer) < maxUseCountPerPlayer)
			{
				Network_003CisAvailable_003Ek__BackingField = true;
				return;
			}
		}
		Network_003CisAvailable_003Ek__BackingField = false;
	}

	[ClientRpc]
	private void RpcInvokeOnSuccessfulUse(Entity entity)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteNetworkBehaviour((NetworkWriter)(object)val, (NetworkBehaviour)(object)entity);
		((NetworkBehaviour)this).SendRPCInternal("System.Void Shrine::RpcInvokeOnSuccessfulUse(Entity)", 823538102, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	public Vector3 GetClosestPointOnShrine(Vector3 pos)
	{
		NavMeshHit val = default;
		if (NavMesh.Raycast(pos, ((Component)(object)this).transform.position, ref val, -1))
		{
			return val.position;
		}
		return ((Component)(object)this).transform.position;
	}

	public Vector3 GetRandomSpawnPosition(Vector3 activatorPos)
	{
		Vector3 vector = (GetClosestPointOnShrine(activatorPos) + ((Component)(object)this).transform.position) / 2f;
		return Dew.GetValidAgentDestination_LinearSweep(vector, vector + (UnityEngine.Random.insideUnitSphere * 3f).Flattened());
	}

	public override bool ShouldBeSavedWithRoom()
	{
		return true;
	}

	protected Shrine()
	{
		((NetworkBehaviour)this).InitSyncObject((SyncObject)(object)_useCountByPlayer);
		_Mirror_SyncVarHookDelegate__003CisAvailable_003Ek__BackingField = OnIsAvailableChanged;
		_Mirror_SyncVarHookDelegate__003CisLocked_003Ek__BackingField = OnIsLockedChanged;
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_TpcShowLocalUseEffect__NetworkConnection__Entity(NetworkConnection conn, Entity entity)
	{
		FxPlay(useEffectLocal, entity);
	}

	protected static void InvokeUserCode_TpcShowLocalUseEffect__NetworkConnection__Entity(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TpcShowLocalUseEffect called on server.");
		}
		else
		{
			((Shrine)(object)obj).UserCode_TpcShowLocalUseEffect__NetworkConnection__Entity(NetworkClient.connection, NetworkReaderExtensions.ReadNetworkBehaviour<Entity>(reader));
		}
	}

	protected void UserCode_RpcInvokeOnSuccessfulUse__Entity(Entity entity)
	{
		try
		{
			ClientEvent_OnSuccessfulUse?.Invoke(entity);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	protected static void InvokeUserCode_RpcInvokeOnSuccessfulUse__Entity(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcInvokeOnSuccessfulUse called on server.");
		}
		else
		{
			((Shrine)(object)obj).UserCode_RpcInvokeOnSuccessfulUse__Entity(NetworkReaderExtensions.ReadNetworkBehaviour<Entity>(reader));
		}
	}

	static Shrine()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterRpc(typeof(Shrine), "System.Void Shrine::RpcInvokeOnSuccessfulUse(Entity)", (RemoteCallDelegate)InvokeUserCode_RpcInvokeOnSuccessfulUse__Entity);
		RemoteProcedureCalls.RegisterRpc(typeof(Shrine), "System.Void Shrine::TpcShowLocalUseEffect(Mirror.NetworkConnection,Entity)", (RemoteCallDelegate)InvokeUserCode_TpcShowLocalUseEffect__NetworkConnection__Entity);
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteInt(writer, maxTotalUseCount);
			NetworkWriterExtensions.WriteInt(writer, totalUseCount__BackingField);
			NetworkWriterExtensions.WriteDouble(writer, _lastUseTime);
			NetworkWriterExtensions.WriteBool(writer, isAvailable__BackingField);
			NetworkWriterExtensions.WriteBool(writer, isLocked__BackingField);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 8L) != 0L)
		{
			NetworkWriterExtensions.WriteInt(writer, maxTotalUseCount);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x10L) != 0L)
		{
			NetworkWriterExtensions.WriteInt(writer, totalUseCount__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x20L) != 0L)
		{
			NetworkWriterExtensions.WriteDouble(writer, _lastUseTime);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x40L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, isAvailable__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x80L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, isLocked__BackingField);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref maxTotalUseCount, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref totalUseCount__BackingField, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<double>(ref _lastUseTime, (Action<double, double>)null, NetworkReaderExtensions.ReadDouble(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref isAvailable__BackingField, _Mirror_SyncVarHookDelegate__003CisAvailable_003Ek__BackingField, NetworkReaderExtensions.ReadBool(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref isLocked__BackingField, _Mirror_SyncVarHookDelegate__003CisLocked_003Ek__BackingField, NetworkReaderExtensions.ReadBool(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 8L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref maxTotalUseCount, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
		}
		if ((num & 0x10L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref totalUseCount__BackingField, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
		}
		if ((num & 0x20L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<double>(ref _lastUseTime, (Action<double, double>)null, NetworkReaderExtensions.ReadDouble(reader));
		}
		if ((num & 0x40L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref isAvailable__BackingField, _Mirror_SyncVarHookDelegate__003CisAvailable_003Ek__BackingField, NetworkReaderExtensions.ReadBool(reader));
		}
		if ((num & 0x80L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref isLocked__BackingField, _Mirror_SyncVarHookDelegate__003CisLocked_003Ek__BackingField, NetworkReaderExtensions.ReadBool(reader));
		}
	}
}
