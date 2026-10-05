using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Primus_Pizza0 : Actor
{
	public static List<Primus_Pizza0> instances = new List<Primus_Pizza0>();

	public GameObject[] brokenObject;

	public GameObject[] nonBrokenObject;

	public GameObject fxBreak;

	public GameObject fxTelegraph;

	public Transform centerPoint;

	public DewCollider range;

	[CompilerGenerated]
	[SyncVar(hook = "OnIsBrokenChanged")]
	private bool isBroken__BackingField;

	public Action<bool, bool> _Mirror_SyncVarHookDelegate__003CisBroken_003Ek__BackingField;

	public bool isBroken
	{
		[CompilerGenerated]
		get
		{
			return isBroken__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CisBroken_003Ek__BackingField = value;
		}
	}

	public override bool isDestroyedOnRoomChange => true;

	public bool Network_003CisBroken_003Ek__BackingField
	{
		get
		{
			return isBroken__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref isBroken__BackingField, 8uL, _Mirror_SyncVarHookDelegate__003CisBroken_003Ek__BackingField);
		}
	}

	private void OnIsBrokenChanged(bool oldValue, bool newValue)
	{
		nonBrokenObject.SetActiveAll(!newValue);
		brokenObject.SetActiveAll(newValue);
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void OnInit()
	{
		instances = new List<Primus_Pizza0>();
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		for (int num = instances.Count - 1; num >= 0; num--)
		{
			if (instances[num].IsNullOrInactive())
			{
				instances.RemoveAt(num);
			}
		}
		instances.Add(this);
		OnIsBrokenChanged(oldValue: false, isBroken);
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		instances.Remove(this);
	}

	public void Break()
	{
		if (!Application.IsPlaying((UnityEngine.Object)(object)this) || isBroken)
		{
			return;
		}
		Network_003CisBroken_003Ek__BackingField = true;
		FxPlayNetworked(fxBreak);
		FxStopNetworked(fxTelegraph);
		ListReturnHandle<Entity> handle;
		foreach (Entity entity in range.GetEntities(out handle, new CollisionCheckSettings
		{
			includeUncollidable = true
		}))
		{
			Vector3 point = SingletonBehaviour<Room_BossArena>.instance.center + (entity.position - SingletonBehaviour<Room_BossArena>.instance.center).Flattened().normalized * 6f + UnityEngine.Random.insideUnitSphere.Flattened() * 1.5f;
			CreateStatusEffect<Se_Primus_Pizza_Fly>(entity, new CastInfo(entity, point));
		}
		handle.Return();
	}

	public void Unbreak()
	{
		if (Application.IsPlaying((UnityEngine.Object)(object)this) && isBroken)
		{
			Network_003CisBroken_003Ek__BackingField = false;
			FxStopNetworked(fxBreak);
		}
	}

	public void PlayTelegraph()
	{
		FxPlayNetworked(fxTelegraph);
	}

	public Primus_Pizza0()
	{
		_Mirror_SyncVarHookDelegate__003CisBroken_003Ek__BackingField = OnIsBrokenChanged;
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteBool(writer, isBroken__BackingField);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 8L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, isBroken__BackingField);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref isBroken__BackingField, _Mirror_SyncVarHookDelegate__003CisBroken_003Ek__BackingField, NetworkReaderExtensions.ReadBool(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 8L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref isBroken__BackingField, _Mirror_SyncVarHookDelegate__003CisBroken_003Ek__BackingField, NetworkReaderExtensions.ReadBool(reader));
		}
	}
}
