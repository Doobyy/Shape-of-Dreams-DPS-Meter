using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public abstract class BaseJonasInteractableObject : Shrine, ICustomInteractable
{
	public Transform conversationPivot;

	public string conversationNameUIKey;

	[SyncVar]
	private MockEntity _spawnedMockEntity;

	protected NetworkBehaviourSyncVar ____spawnedMockEntityNetId;

	protected bool isMockEntityBusy
	{
		get
		{
			if (!Network_spawnedMockEntity.IsNullOrInactive())
			{
				return Network_spawnedMockEntity.Status.isInConversation;
			}
			return false;
		}
	}

	protected abstract string ConversationKey { get; }

	protected abstract string InteractTextKey { get; }

	public string interactActionRawText => DewLocalization.GetUIValue(InteractTextKey);

	public string nameRawText => DewLocalization.GetUIValue(((object)this).GetType().Name + "_Name");

	public MockEntity Network_spawnedMockEntity
	{
		get
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			return ((NetworkBehaviour)this).GetSyncVarNetworkBehaviour<MockEntity>(____spawnedMockEntityNetId, ref _spawnedMockEntity);
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter_NetworkBehaviour<MockEntity>(value, ref _spawnedMockEntity, 256uL, (Action<MockEntity, MockEntity>)null, ref ____spawnedMockEntityNetId);
		}
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			Transform transform = ((conversationPivot != null) ? conversationPivot : ((Component)(object)this).transform);
			string nameUIKey = ((!string.IsNullOrEmpty(conversationNameUIKey)) ? conversationNameUIKey : (((object)this).GetType().Name + "_Name"));
			Network_spawnedMockEntity = Dew.CreateActor(transform.position, transform.rotation, null, (MockEntity e) =>
			{
				e.conversationNameUIKey = nameUIKey;
			});
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && !Network_spawnedMockEntity.IsNullOrInactive())
		{
			Network_spawnedMockEntity.Destroy();
		}
	}

	protected override bool OnUse(Entity entity)
	{
		if (Network_spawnedMockEntity.IsNullOrInactive())
		{
			return false;
		}
		if (isMockEntityBusy)
		{
			return false;
		}
		TalkWithShrine(entity);
		return true;
	}

	protected virtual void TalkWithShrine(Entity entity)
	{
		NetworkedManagerBase<ConversationManager>.instance.StartConversation(new DewConversationSettings
		{
			player = entity.owner,
			rotateTowardsCenter = true,
			speakers = new Entity[2] { Network_spawnedMockEntity, entity },
			startConversationKey = ConversationKey,
			visibility = ConversationVisibility.Everyone
		});
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)Network_spawnedMockEntity);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x100L) != 0L)
		{
			NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)Network_spawnedMockEntity);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize_NetworkBehaviour<MockEntity>(ref _spawnedMockEntity, (Action<MockEntity, MockEntity>)null, reader, ref ____spawnedMockEntityNetId);
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x100L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize_NetworkBehaviour<MockEntity>(ref _spawnedMockEntity, (Action<MockEntity, MockEntity>)null, reader, ref ____spawnedMockEntityNetId);
		}
	}
}
