using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Mon_Polaris : Monster, IInteractable, IForceHeroicHealthbar, IDontKillOnBossMonsterDeath
{
	[CompilerGenerated]
	[SyncVar]
	private bool canInteract__BackingField;

	public SafeAction<Entity> onInteract;

	protected override DewPlayer defaultOwner => DewPlayer.environment;

	public bool canInteract
	{
		[CompilerGenerated]
		get
		{
			return canInteract__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CcanInteract_003Ek__BackingField = value;
		}
	}

	public Transform interactPivot => ((Component)(object)this).transform;

	public bool canInteractWithMouse => false;

	public float focusDistance => 4.5f;

	public int priority => 100;

	public bool Network_003CcanInteract_003Ek__BackingField
	{
		get
		{
			return canInteract__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref canInteract__BackingField, 256uL, (Action<bool, bool>)null);
		}
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			CreateStatusEffect<Se_OntologicalShield>(this, new CastInfo(this));
			CreateBasicEffect(this, new UntargetableEffect(), float.PositiveInfinity);
			CreateBasicEffect(this, new UncollidableEffect(), float.PositiveInfinity);
		}
	}

	public bool CanInteract(Entity entity)
	{
		if (canInteract)
		{
			return !Status.isInConversation;
		}
		return false;
	}

	public void OnInteract(Entity entity, bool alt)
	{
		if (((NetworkBehaviour)this).isServer && canInteract && entity is Hero hero && hero.owner.isHumanPlayer)
		{
			onInteract?.Invoke(entity);
		}
	}

	public override bool CanSleep()
	{
		return false;
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteBool(writer, canInteract__BackingField);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x100L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, canInteract__BackingField);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref canInteract__BackingField, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x100L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref canInteract__BackingField, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
		}
	}
}
