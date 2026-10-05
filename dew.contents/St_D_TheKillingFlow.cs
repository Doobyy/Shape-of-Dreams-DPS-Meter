using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Mirror;

public class St_D_TheKillingFlow : SkillTrigger
{
	[CompilerGenerated]
	[SyncVar]
	private int gainedAd__BackingField;

	public int gainedAd
	{
		[CompilerGenerated]
		get
		{
			return gainedAd__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CgainedAd_003Ek__BackingField = value;
		}
	}

	public int Network_003CgainedAd_003Ek__BackingField
	{
		get
		{
			return gainedAd__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<int>(value, ref gainedAd__BackingField, 134217728uL, (Action<int, int>)null);
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
			NetworkWriterExtensions.WriteInt(writer, gainedAd__BackingField);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x8000000L) != 0L)
		{
			NetworkWriterExtensions.WriteInt(writer, gainedAd__BackingField);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref gainedAd__BackingField, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x8000000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref gainedAd__BackingField, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
		}
	}
}
