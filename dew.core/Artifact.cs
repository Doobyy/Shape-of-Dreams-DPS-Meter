using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

[DewResourceLink(ResourceLinkBy.Type)]
public class Artifact : Actor, IInteractable, ICustomInteractable, IExcludeFromPool
{
	public Sprite icon;

	[ColorUsage(false)]
	public Color mainColor;

	public DewAudioClip touchSound;

	public bool excludeFromPool;

	private ArtifactWorldModel _worldModel;

	public Transform interactPivot => _worldModel.interactPivot;

	public bool canInteractWithMouse => true;

	public float focusDistance => 3.75f;

	public int priority => 10;

	public int grantedStardust => 60;

	public string nameRawText => DewLocalization.GetArtifactName(DewLocalization.GetArtifactKey(((object)this).GetType().Name));

	public string interactActionRawText => DewLocalization.GetUIValue("InGame_Tooltip_PickUp");

	public string interactAltActionRawText => null;

	public float? altInteractProgress => null;

	public Cost cost => default;

	public bool canAltInteract => false;

	bool IExcludeFromPool.excludeFromPool => excludeFromPool;

	protected override void OnCreate()
	{
		base.OnCreate();
		GameObject original = Resources.Load<GameObject>("WorldModels/Artifact World Model");
		_worldModel = Object.Instantiate(original, position, rotation, ((Component)(object)this).transform).GetComponent<ArtifactWorldModel>();
		_worldModel.Setup(this);
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		FxStop(_worldModel.fxAppear);
		FxStop(_worldModel.fxLoop);
	}

	public bool CanInteract(Entity entity)
	{
		return Time.time - creationTime > 1f;
	}

	public void OnInteract(Entity entity, bool alt)
	{
		if (alt)
		{
			return;
		}
		if (((NetworkBehaviour)entity).isOwned && NetworkedManagerBase<QuestManager>.instance.currentArtifact != null)
		{
			InGameUIManager.instance.ShowCenterMessage(CenterMessageType.Error, "InGame_Message_AlreadyHasArtifact");
		}
		if (((NetworkBehaviour)this).isServer && NetworkedManagerBase<QuestManager>.instance.currentArtifact == null)
		{
			FxPlayNetworked(_worldModel.fxPickUp, entity);
			NetworkedManagerBase<QuestManager>.instance.PickUpArtifact(((object)this).GetType().Name, entity.owner, _worldModel.icon.transform.position);
			NetworkedManagerBase<QuestManager>.instance.didCollectArtifactThisLoop = true;
			Destroy();
			if (!((IEnumerable<WorldNodeData>)NetworkedManagerBase<ZoneManager>.instance.nodes).Any((WorldNodeData n) => n.HasModifier("RoomMod_DreamTeller")) && NetworkedManagerBase<ZoneManager>.instance.TryGetNodeIndexForNextGoal(new GetNodeIndexSettings
			{
				avoidMainModifier = true,
				allowedTypes = new WorldNodeType[1] { WorldNodeType.Combat },
				preferCloserToExit = true,
				desiredDistance = new Vector2Int(2, 4)
			}, out var nodeIndex))
			{
				NetworkedManagerBase<ZoneManager>.instance.AddModifier<RoomMod_DreamTeller>(nodeIndex);
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
