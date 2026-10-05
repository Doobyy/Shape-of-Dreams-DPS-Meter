using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class Mon_DreamTeller : Monster, IInteractable, IForceHeroicHealthbar
{
	public GameObject fxExplode;

	public float showStoryDelay;

	private bool _isEligibleForMoreTalks;

	protected override DewPlayer defaultOwner => DewPlayer.environment;

	public Transform interactPivot => ((Component)(object)this).transform;

	public bool canInteractWithMouse => false;

	public float focusDistance => 4.5f;

	public int priority
	{
		get
		{
			if (NetworkedManagerBase<QuestManager>.instance.currentArtifact == null)
			{
				return 100;
			}
			return 10;
		}
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			CreateStatusEffect<Se_OntologicalShield>(this, new CastInfo(this));
			_isEligibleForMoreTalks = UnityEngine.Random.value < 0.25f;
		}
	}

	public bool CanInteract(Entity entity)
	{
		return !Status.isInConversation;
	}

	public void OnInteract(Entity entity, bool alt)
	{
		if (!alt && ((NetworkBehaviour)entity).isOwned && !Status.isInConversation)
		{
			int num = DewSave.profileMain.artifacts.Count((KeyValuePair<string, DewProfile.UnlockData> p) => p.Value.status == UnlockStatus.Complete);
			CmdTalkWithDreamTeller(entity, DewSave.profileMain.didMeetDreamTeller, num >= 3);
			DewSave.profileMain.didMeetDreamTeller = true;
		}
	}

	[Command(requiresAuthority = false)]
	private void CmdTalkWithDreamTeller(Entity entity, bool didMeetDreamTeller, bool canDoMoreTalks, NetworkConnectionToClient sender = null)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteNetworkBehaviour((NetworkWriter)(object)val, (NetworkBehaviour)(object)entity);
		NetworkWriterExtensions.WriteBool((NetworkWriter)(object)val, didMeetDreamTeller);
		NetworkWriterExtensions.WriteBool((NetworkWriter)(object)val, canDoMoreTalks);
		((NetworkBehaviour)this).SendCommandInternal("System.Void Mon_DreamTeller::CmdTalkWithDreamTeller(Entity,System.Boolean,System.Boolean,Mirror.NetworkConnectionToClient)", 964231878, (NetworkWriter)(object)val, 0, false);
		NetworkWriterPool.Return(val);
	}

	private void GiveArtifact(Entity from)
	{
		if (NetworkedManagerBase<QuestManager>.instance.currentArtifact != null)
		{
			Artifact artifact = DewResources.GetByShortTypeName<Artifact>(NetworkedManagerBase<QuestManager>.instance.currentArtifact, default(ResourceLoadSettings));
			NetworkedManagerBase<QuestManager>.instance.RemoveArtifact();
			NetworkedManagerBase<ConversationManager>.instance.StartConversation(new DewConversationSettings
			{
				player = from.owner,
				speakers = new Entity[2] { this, from },
				visibility = ConversationVisibility.Everyone,
				rotateTowardsCenter = true,
				startConversationKey = "DreamTeller.Thanks*",
				onStop = () =>
				{
					OpenArtifact(from, artifact);
				},
				variables = new Dictionary<string, string> { 
				{
					"artifact",
					((object)artifact).GetType().Name
				} }
			});
		}
	}

	[Server]
	private void OpenArtifact(Entity from, Artifact artifact)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Mon_DreamTeller::OpenArtifact(Entity,Artifact)' called when server was not active");
			return;
		}
		Vector3 pos = position;
		Control.StartDaze(4f);
		((MonoBehaviour)(object)this).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			FxPlayNetworked(fxExplode, this);
			if ((UnityEngine.Object)(object)from != null && (UnityEngine.Object)(object)from.owner != null && from.owner.isHumanPlayer)
			{
				TpcDisableCharacterControls(from.owner);
			}
			yield return new WaitForSeconds(showStoryDelay);
			if ((UnityEngine.Object)(object)from != null && (UnityEngine.Object)(object)from.owner != null && from.owner.isHumanPlayer)
			{
				TpcEnableCharacterControls(from.owner);
			}
			NetworkedManagerBase<QuestManager>.instance.DiscoverArtifactAndShowStory(((object)artifact).GetType().Name);
			yield return new WaitForSeconds(1f);
			NetworkedManagerBase<PickupManager>.instance.DropStarDust(artifact.grantedStardust, pos);
		}
	}

	[TargetRpc]
	private void TpcDisableCharacterControls(NetworkConnectionToClient target)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendTargetRPCInternal((NetworkConnection)(object)target, "System.Void Mon_DreamTeller::TpcDisableCharacterControls(Mirror.NetworkConnectionToClient)", 1498787282, (NetworkWriter)(object)val, 0);
		NetworkWriterPool.Return(val);
	}

	[TargetRpc]
	private void TpcEnableCharacterControls(NetworkConnectionToClient target)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendTargetRPCInternal((NetworkConnection)(object)target, "System.Void Mon_DreamTeller::TpcEnableCharacterControls(Mirror.NetworkConnectionToClient)", 39458877, (NetworkWriter)(object)val, 0);
		NetworkWriterPool.Return(val);
	}

	public override bool ShouldBeSavedWithRoom()
	{
		return false;
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_CmdTalkWithDreamTeller__Entity__Boolean__Boolean__NetworkConnectionToClient(Entity entity, bool didMeetDreamTeller, bool canDoMoreTalks, NetworkConnectionToClient sender)
	{
		if (sender == null || (UnityEngine.Object)(object)sender.GetPlayer() == null || (UnityEngine.Object)(object)entity.owner != (UnityEngine.Object)(object)sender.GetPlayer() || Status.isInConversation)
		{
			return;
		}
		if (NetworkedManagerBase<QuestManager>.instance.currentArtifact != null)
		{
			NetworkedManagerBase<ConversationManager>.instance.StartConversation(new DewConversationSettings
			{
				player = entity.owner,
				speakers = new Entity[2] { this, entity },
				visibility = ConversationVisibility.Everyone,
				rotateTowardsCenter = true,
				startConversationKey = (didMeetDreamTeller ? "DreamTeller.Intro*" : "DreamTeller.FirstTimeIntro"),
				callFunctions = new Dictionary<string, Action> { 
				{
					"GiveArtifact",
					() =>
					{
						GiveArtifact(entity);
					}
				} }
			});
		}
		else if (canDoMoreTalks && _isEligibleForMoreTalks)
		{
			NetworkedManagerBase<ConversationManager>.instance.StartConversation(new DewConversationSettings
			{
				player = entity.owner,
				speakers = new Entity[2] { this, entity },
				visibility = ConversationVisibility.Everyone,
				rotateTowardsCenter = true,
				startConversationKey = "DreamTeller.TalkAboutSelfChoice"
			});
			_isEligibleForMoreTalks = false;
		}
		else
		{
			NetworkedManagerBase<ConversationManager>.instance.StartConversation(new DewConversationSettings
			{
				player = entity.owner,
				speakers = new Entity[2] { this, entity },
				visibility = ConversationVisibility.Everyone,
				rotateTowardsCenter = true,
				startConversationKey = "DreamTeller.ChitChat*"
			});
		}
	}

	protected static void InvokeUserCode_CmdTalkWithDreamTeller__Entity__Boolean__Boolean__NetworkConnectionToClient(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdTalkWithDreamTeller called on client.");
		}
		else
		{
			((Mon_DreamTeller)(object)obj).UserCode_CmdTalkWithDreamTeller__Entity__Boolean__Boolean__NetworkConnectionToClient(NetworkReaderExtensions.ReadNetworkBehaviour<Entity>(reader), NetworkReaderExtensions.ReadBool(reader), NetworkReaderExtensions.ReadBool(reader), senderConnection);
		}
	}

	protected void UserCode_TpcDisableCharacterControls__NetworkConnectionToClient(NetworkConnectionToClient target)
	{
		ManagerBase<ControlManager>.instance.DisableCharacterControls();
	}

	protected static void InvokeUserCode_TpcDisableCharacterControls__NetworkConnectionToClient(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TpcDisableCharacterControls called on server.");
		}
		else
		{
			((Mon_DreamTeller)(object)obj).UserCode_TpcDisableCharacterControls__NetworkConnectionToClient((NetworkConnectionToClient)(object)NetworkClient.connection);
		}
	}

	protected void UserCode_TpcEnableCharacterControls__NetworkConnectionToClient(NetworkConnectionToClient target)
	{
		ManagerBase<ControlManager>.instance.EnableCharacterControls();
	}

	protected static void InvokeUserCode_TpcEnableCharacterControls__NetworkConnectionToClient(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TpcEnableCharacterControls called on server.");
		}
		else
		{
			((Mon_DreamTeller)(object)obj).UserCode_TpcEnableCharacterControls__NetworkConnectionToClient((NetworkConnectionToClient)(object)NetworkClient.connection);
		}
	}

	static Mon_DreamTeller()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected Obj, but got Unknown
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Expected Obj, but got Unknown
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterCommand(typeof(Mon_DreamTeller), "System.Void Mon_DreamTeller::CmdTalkWithDreamTeller(Entity,System.Boolean,System.Boolean,Mirror.NetworkConnectionToClient)", (RemoteCallDelegate)InvokeUserCode_CmdTalkWithDreamTeller__Entity__Boolean__Boolean__NetworkConnectionToClient, false);
		RemoteProcedureCalls.RegisterRpc(typeof(Mon_DreamTeller), "System.Void Mon_DreamTeller::TpcDisableCharacterControls(Mirror.NetworkConnectionToClient)", (RemoteCallDelegate)InvokeUserCode_TpcDisableCharacterControls__NetworkConnectionToClient);
		RemoteProcedureCalls.RegisterRpc(typeof(Mon_DreamTeller), "System.Void Mon_DreamTeller::TpcEnableCharacterControls(Mirror.NetworkConnectionToClient)", (RemoteCallDelegate)InvokeUserCode_TpcEnableCharacterControls__NetworkConnectionToClient);
	}
}
