using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

[DewResourceLink(ResourceLinkBy.None)]
public class Rift : Actor, IInteractable, IBanRoomNodesNearby
{
	public static Rift softInstance;

	private const float InternalCooldownTimePerHero = 0.5f;

	[SyncVar]
	private float _openTime;

	public GameObject fxCharge;

	public GameObject fxOpen;

	public GameObject fxLoop;

	public GameObject fxLocked;

	public GameObject fxUnlocked;

	public GameObject fxActivate;

	public float chargeDuration;

	public float afterOpenInteractableDelay;

	[CompilerGenerated]
	[SyncVar(hook = "OnIsOpenChanged")]
	private bool isOpen__BackingField;

	[SaveVar(SaveVarFlags.Default)]
	private bool _didStartCharging;

	[CompilerGenerated]
	[SyncVar(hook = "OnIsLockedChanged")]
	private bool isLocked__BackingField;

	private Dictionary<Hero, float> _lastUseTimes = new Dictionary<Hero, float>();

	public Action<bool, bool> _Mirror_SyncVarHookDelegate__003CisOpen_003Ek__BackingField;

	public Action<bool, bool> _Mirror_SyncVarHookDelegate__003CisLocked_003Ek__BackingField;

	public static Rift instance => Dew.Helper_GetInstanceOfActor(ref softInstance);

	public override bool isDestroyedOnRoomChange => true;

	int IInteractable.priority => 100;

	[SaveVar(SaveVarFlags.Default)]
	public bool isOpen
	{
		[CompilerGenerated]
		get
		{
			return isOpen__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CisOpen_003Ek__BackingField = value;
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

	public Transform interactPivot => ((Component)(object)this).transform;

	public bool canInteractWithMouse => false;

	public float focusDistance => 2.5f;

	public float Network_openTime
	{
		get
		{
			return _openTime;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref _openTime, 8uL, (Action<float, float>)null);
		}
	}

	public bool Network_003CisOpen_003Ek__BackingField
	{
		get
		{
			return isOpen__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref isOpen__BackingField, 16uL, _Mirror_SyncVarHookDelegate__003CisOpen_003Ek__BackingField);
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
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref isLocked__BackingField, 32uL, _Mirror_SyncVarHookDelegate__003CisLocked_003Ek__BackingField);
		}
	}

	public override bool ShouldBeSavedWithRoom()
	{
		return true;
	}

	private void OnIsOpenChanged(bool oldVal, bool newVal)
	{
		if (isOpen)
		{
			FxPlay(fxLoop);
			if (isLocked)
			{
				FxPlay(fxLocked);
			}
			else
			{
				FxStop(fxLocked);
			}
		}
		else
		{
			FxStop(fxLoop);
			FxStop(fxLocked);
		}
	}

	private void OnIsLockedChanged(bool oldVal, bool newVal)
	{
		if (!isOpen)
		{
			return;
		}
		if (newVal)
		{
			FxPlay(fxLocked);
			FxStop(fxUnlocked);
			return;
		}
		FxStop(fxLocked);
		FxPlay(fxUnlocked);
		if (!NetworkedManagerBase<ZoneManager>.instance.isInAnyTransition)
		{
			ManagerBase<ObjectiveArrowManager>.instance.objectivePosition = ((Component)(object)this).transform.position;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		softInstance = this;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		OnIsOpenChanged(oldVal: false, isOpen);
		OnIsLockedChanged(oldVal: false, isLocked);
		if (!isNewInstance && isOpen && fxLoop.TryGetComponent<ParticleSystem>(out var component))
		{
			component.Simulate(2f);
			component.Play();
		}
		if (((NetworkBehaviour)this).isServer && !isNewInstance && _didStartCharging && !isOpen)
		{
			Network_003CisOpen_003Ek__BackingField = true;
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if ((UnityEngine.Object)(object)softInstance == (UnityEngine.Object)(object)this)
		{
			softInstance = null;
		}
		FxStop(fxCharge);
		FxStop(fxOpen);
		FxStop(fxLoop);
		FxStop(fxLocked);
		FxStop(fxUnlocked);
		FxStop(fxActivate);
	}

	public virtual bool CanInteract(Entity entity)
	{
		if (isOpen && !NetworkedManagerBase<ZoneManager>.instance.isInRoomTransition)
		{
			return NetworkTime.time - (double)_openTime > (double)afterOpenInteractableDelay;
		}
		return false;
	}

	void IInteractable.OnInteract(Entity entity, bool alt)
	{
		if (alt || !((NetworkBehaviour)this).isServer || NetworkedManagerBase<ZoneManager>.instance.isInRoomTransition || !isOpen || isLocked || !(entity is Hero hero) || (_lastUseTimes.TryGetValue(hero, out var value) && Time.time - value < 0.5f))
		{
			return;
		}
		_lastUseTimes[hero] = Time.time;
		try
		{
			if (OnInteractRift(hero))
			{
				FxPlayNetworked(fxActivate);
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	protected virtual bool OnInteractRift(Hero hero)
	{
		return false;
	}

	[Server]
	public void Open()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Rift::Open()' called when server was not active");
			return;
		}
		((MonoBehaviour)(object)this).StopAllCoroutines();
		((MonoBehaviour)(object)this).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			if (!isOpen)
			{
				FxPlayNetworked(fxCharge);
				_didStartCharging = true;
				yield return new WaitForSeconds(chargeDuration);
				FxStopNetworked(fxCharge);
				FxPlayNetworked(fxOpen);
				Network_openTime = (float)NetworkTime.time;
				Network_003CisOpen_003Ek__BackingField = true;
				if (!isLocked)
				{
					RpcSetObjectivePosition();
				}
			}
		}
	}

	[ClientRpc]
	private void RpcSetObjectivePosition()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendRPCInternal("System.Void Rift::RpcSetObjectivePosition()", 211845117, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[Server]
	public void Close()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Rift::Close()' called when server was not active");
		}
		else
		{
			Network_003CisOpen_003Ek__BackingField = false;
		}
	}

	public Rift()
	{
		_Mirror_SyncVarHookDelegate__003CisOpen_003Ek__BackingField = OnIsOpenChanged;
		_Mirror_SyncVarHookDelegate__003CisLocked_003Ek__BackingField = OnIsLockedChanged;
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcSetObjectivePosition()
	{
		ManagerBase<ObjectiveArrowManager>.instance.objectivePosition = ((Component)(object)this).transform.position;
	}

	protected static void InvokeUserCode_RpcSetObjectivePosition(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetObjectivePosition called on server.");
		}
		else
		{
			((Rift)(object)obj).UserCode_RpcSetObjectivePosition();
		}
	}

	static Rift()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterRpc(typeof(Rift), "System.Void Rift::RpcSetObjectivePosition()", (RemoteCallDelegate)InvokeUserCode_RpcSetObjectivePosition);
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteFloat(writer, _openTime);
			NetworkWriterExtensions.WriteBool(writer, isOpen__BackingField);
			NetworkWriterExtensions.WriteBool(writer, isLocked__BackingField);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 8L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, _openTime);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x10L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, isOpen__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x20L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, isLocked__BackingField);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _openTime, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref isOpen__BackingField, _Mirror_SyncVarHookDelegate__003CisOpen_003Ek__BackingField, NetworkReaderExtensions.ReadBool(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref isLocked__BackingField, _Mirror_SyncVarHookDelegate__003CisLocked_003Ek__BackingField, NetworkReaderExtensions.ReadBool(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 8L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _openTime, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
		if ((num & 0x10L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref isOpen__BackingField, _Mirror_SyncVarHookDelegate__003CisOpen_003Ek__BackingField, NetworkReaderExtensions.ReadBool(reader));
		}
		if ((num & 0x20L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref isLocked__BackingField, _Mirror_SyncVarHookDelegate__003CisLocked_003Ek__BackingField, NetworkReaderExtensions.ReadBool(reader));
		}
	}
}
