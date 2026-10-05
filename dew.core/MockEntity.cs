using System;
using System.Runtime.InteropServices;
using Mirror;

public class MockEntity : Entity
{
	[SyncVar]
	public string conversationNameUIKey;

	public string conversationRawName;

	public string NetworkconversationNameUIKey
	{
		get
		{
			return conversationNameUIKey;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<string>(value, ref conversationNameUIKey, 32uL, (Action<string, string>)null);
		}
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			CreateBasicEffect(this, new InvulnerableEffect(), float.PositiveInfinity);
			CreateBasicEffect(this, new InvisibleEffect
			{
				ignoreReveal = true
			}, float.PositiveInfinity);
			CreateBasicEffect(this, new UntargetableEffect(), float.PositiveInfinity);
			CreateBasicEffect(this, new UncollidableEffect(), float.PositiveInfinity);
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
			NetworkWriterExtensions.WriteString(writer, conversationNameUIKey);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x20L) != 0L)
		{
			NetworkWriterExtensions.WriteString(writer, conversationNameUIKey);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<string>(ref conversationNameUIKey, (Action<string, string>)null, NetworkReaderExtensions.ReadString(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x20L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<string>(ref conversationNameUIKey, (Action<string, string>)null, NetworkReaderExtensions.ReadString(reader));
		}
	}
}
