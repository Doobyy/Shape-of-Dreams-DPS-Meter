using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Se_GenericEffectContainer : StatusEffect
{
	public BasicEffect effect;

	public float duration;

	[SyncVar]
	internal string _id;

	public override bool reuseInRoom => true;

	public string id => _id;

	public string Network_id
	{
		get
		{
			return _id;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<string>(value, ref _id, 4096uL, (Action<string, string>)null);
		}
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			SetTimer(duration);
			DoBasicEffect(effect);
		}
	}

	protected override void OnDestroyActor()
	{
		if (((NetworkBehaviour)this).isServer && effect is UnstoppableEffect && float.IsPositiveInfinity(duration) && parentActor is BossMonster && parentActor == victim && victim.isActive && !victim.Status.isDead)
		{
			UnityEngine.Debug.LogWarning($"Permanent boss unstoppable destroyed mid-fight on {victim.GetActorReadableName()}\n{new StackTrace()}");
		}
		base.OnDestroyActor();
	}

	public override string GetActorReadableName()
	{
		return base.GetActorReadableName() + " '" + id + "' (" + ((effect != null) ? effect.GetType().Name : "Unknown") + ")";
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteString(writer, _id);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x1000L) != 0L)
		{
			NetworkWriterExtensions.WriteString(writer, _id);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<string>(ref _id, (Action<string, string>)null, NetworkReaderExtensions.ReadString(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x1000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<string>(ref _id, (Action<string, string>)null, NetworkReaderExtensions.ReadString(reader));
		}
	}
}
