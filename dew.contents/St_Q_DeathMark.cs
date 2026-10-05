using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class St_Q_DeathMark : SkillTrigger
{
	public bool resetAmpOnZoneLoad = true;

	[NonSerialized]
	[SyncVar]
	[SaveVar(SaveVarFlags.Default)]
	public float currentDamageAmp;

	public float NetworkcurrentDamageAmp
	{
		get
		{
			return currentDamageAmp;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref currentDamageAmp, 134217728uL, (Action<float, float>)null);
		}
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnZoneLoadStarted += new Action<EventInfoLoadZone>(ClientEventOnZoneLoaded);
		}
	}

	private void ClientEventOnZoneLoaded(EventInfoLoadZone obj)
	{
		if (obj.isTraveling && resetAmpOnZoneLoad)
		{
			NetworkcurrentDamageAmp = 0f;
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (bool)(UnityEngine.Object)(object)NetworkedManagerBase<ZoneManager>.instance)
		{
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnZoneLoadStarted -= new Action<EventInfoLoadZone>(ClientEventOnZoneLoaded);
		}
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteFloat(writer, currentDamageAmp);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x8000000L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, currentDamageAmp);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref currentDamageAmp, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x8000000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref currentDamageAmp, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
	}
}
