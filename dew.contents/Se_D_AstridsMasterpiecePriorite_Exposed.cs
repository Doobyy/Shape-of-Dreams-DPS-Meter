using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Se_D_AstridsMasterpiecePriorite_Exposed : StatusEffect
{
	public int maxCharge = 6;

	public float maxDistance = 15f;

	public float distCheckInterval = 1f;

	public GameObject[] fxEntity;

	[NonSerialized]
	[SyncVar(hook = "OnChangeChargeCount")]
	private int _chargeCount;

	private float _lastDistCheckTime;

	public Action<int, int> _Mirror_SyncVarHookDelegate__chargeCount;

	public override bool reuseInRoom => true;

	public int chargeCount
	{
		get
		{
			return _chargeCount;
		}
		set
		{
			Network_chargeCount = value;
		}
	}

	public int Network_chargeCount
	{
		get
		{
			return _chargeCount;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<int>(value, ref _chargeCount, 4096uL, _Mirror_SyncVarHookDelegate__chargeCount);
		}
	}

	private void OnChangeChargeCount(int oldVar, int newVar)
	{
		if (((NetworkBehaviour)this).isServer && newVar >= 0 && newVar < maxCharge)
		{
			FxStopNetworked(fxEntity[newVar]);
		}
	}

	protected override void OnPrepare()
	{
		base.OnPrepare();
		Network_chargeCount = -1;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DestroyOnDeath(info.caster);
			DestroyOnDestroy(parentActor);
			GameObject[] array = fxEntity;
			foreach (GameObject effect in array)
			{
				FxPlayNetworked(effect, victim);
			}
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer && Time.time - _lastDistCheckTime > distCheckInterval)
		{
			_lastDistCheckTime = Time.time;
			if (Vector2.Distance(info.caster.agentPosition.ToXY(), victim.agentPosition.ToXY()) > maxDistance)
			{
				Destroy();
			}
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			GameObject[] array = fxEntity;
			foreach (GameObject effect in array)
			{
				FxStopNetworked(effect);
			}
		}
	}

	public Se_D_AstridsMasterpiecePriorite_Exposed()
	{
		_Mirror_SyncVarHookDelegate__chargeCount = OnChangeChargeCount;
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteInt(writer, _chargeCount);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x1000L) != 0L)
		{
			NetworkWriterExtensions.WriteInt(writer, _chargeCount);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref _chargeCount, _Mirror_SyncVarHookDelegate__chargeCount, NetworkReaderExtensions.ReadInt(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x1000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref _chargeCount, _Mirror_SyncVarHookDelegate__chargeCount, NetworkReaderExtensions.ReadInt(reader));
		}
	}
}
