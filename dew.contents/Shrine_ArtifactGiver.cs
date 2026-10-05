using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Shrine_ArtifactGiver : Shrine, ICustomInteractable
{
	public Transform conversationPivot;

	public AssetRef<Artifact> artifact;

	public bool enableChat;

	public string conversationKey;

	[SyncVar]
	private MockEntity _mockEntity;

	private string _artifactTypeName;

	protected NetworkBehaviourSyncVar ____mockEntityNetId;

	private bool _isMockEntityBusy
	{
		get
		{
			if (!Network_mockEntity.IsNullOrInactive())
			{
				return Network_mockEntity.Status.isInConversation;
			}
			return false;
		}
	}

	public string interactActionRawText => DewLocalization.GetUIValue("InGame_Interact_TakeALook");

	public string nameRawText => DewLocalization.GetArtifactName(DewLocalization.GetArtifactKey(_artifactTypeName));

	public MockEntity Network_mockEntity
	{
		get
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			return ((NetworkBehaviour)this).GetSyncVarNetworkBehaviour<MockEntity>(____mockEntityNetId, ref _mockEntity);
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter_NetworkBehaviour<MockEntity>(value, ref _mockEntity, 256uL, (Action<MockEntity, MockEntity>)null, ref ____mockEntityNetId);
		}
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		_artifactTypeName = ((object)artifact.lightAsset).GetType().Name;
		if (enableChat && string.IsNullOrEmpty(conversationKey))
		{
			conversationKey = "ArtifactGiver.Artifact";
		}
		if (((NetworkBehaviour)this).isServer && enableChat)
		{
			Network_mockEntity = Dew.CreateActor<MockEntity>(position, null);
		}
		if (enableChat)
		{
			((MonoBehaviour)(object)this).StartCoroutine(SetupMockEntityRoutine());
		}
	}

	private IEnumerator SetupMockEntityRoutine()
	{
		while (Network_mockEntity.IsNullOrInactive() || !Network_mockEntity.Visual.model)
		{
			yield return null;
		}
		Network_mockEntity.Visual.model.conversationPivotPosition = conversationPivot;
		Network_mockEntity.conversationRawName = DewLocalization.GetArtifactName(DewLocalization.GetArtifactKey(_artifactTypeName));
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && !Network_mockEntity.IsNullOrInactive())
		{
			Network_mockEntity.Destroy();
		}
	}

	protected override bool OnUse(Entity entity)
	{
		((MonoBehaviour)(object)this).StartCoroutine(Routine());
		return true;
		IEnumerator Routine()
		{
			yield return new WaitForSeconds(1f);
			if (enableChat)
			{
				if (!Network_mockEntity.IsNullOrInactive() && !_isMockEntityBusy)
				{
					TalkWithShrine(entity);
				}
			}
			else
			{
				GiveArtifact(entity.owner);
			}
		}
	}

	private void TalkWithShrine(Entity entity)
	{
		NetworkedManagerBase<ConversationManager>.instance.StartConversation(new DewConversationSettings
		{
			player = entity.owner,
			rotateTowardsCenter = true,
			speakers = new Entity[2] { Network_mockEntity, entity },
			startConversationKey = conversationKey,
			visibility = ConversationVisibility.OnlyToPlayer,
			onStop = () =>
			{
				GiveArtifact(entity.owner);
			},
			variables = new Dictionary<string, string> { { "artifact", _artifactTypeName } }
		});
	}

	private void GiveArtifact(DewPlayer target = null)
	{
		NetworkedManagerBase<QuestManager>.instance.DiscoverArtifactAndShowStory(_artifactTypeName, target);
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)Network_mockEntity);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x100L) != 0L)
		{
			NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)Network_mockEntity);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize_NetworkBehaviour<MockEntity>(ref _mockEntity, (Action<MockEntity, MockEntity>)null, reader, ref ____mockEntityNetId);
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x100L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize_NetworkBehaviour<MockEntity>(ref _mockEntity, (Action<MockEntity, MockEntity>)null, reader, ref ____mockEntityNetId);
		}
	}
}
