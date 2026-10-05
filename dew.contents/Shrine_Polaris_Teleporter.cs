using System;
using Mirror;
using UnityEngine;

public class Shrine_Polaris_Teleporter : Shrine, IPlayerPathablePoint, ICustomInteractable
{
	public enum ObjectType
	{
		Storage,
		Pray,
		Quaters
	}

	public Transform targetTransform;

	[NonSerialized]
	public bool isSpawnedByOthers;

	[NonSerialized]
	[SaveVar(SaveVarFlags.Default)]
	public Vector3 destination;

	public ObjectType myType;

	Vector3 IPlayerPathablePoint.pathablePosition => ((Component)(object)this).transform.position;

	public string nameRawText
	{
		get
		{
			if (isSpawnedByOthers)
			{
				return DewLocalization.GetUIValue("Shrine_Polaris_Teleporter_Name");
			}
			return myType switch
			{
				ObjectType.Storage => DewLocalization.GetUIValue("InGame_Interact_PlaceStorage"), 
				ObjectType.Pray => DewLocalization.GetUIValue("InGame_Interact_PlacePray"), 
				ObjectType.Quaters => DewLocalization.GetUIValue("InGame_Interact_PlaceQuaters"), 
				_ => null, 
			};
		}
	}

	public string interactActionRawText
	{
		get
		{
			if (isSpawnedByOthers)
			{
				return DewLocalization.GetUIValue("InGame_Interact_Rift_GoBack");
			}
			return DewLocalization.GetUIValue("InGame_Interact_Rift_Enter");
		}
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		NetworkedManagerBase<GameManager>.instance.LockMidRunSave();
		if (!isSpawnedByOthers)
		{
			Dew.CreateActor(destination = Dew.GetPositionOnGround(targetTransform.position), null, this, (Shrine_Polaris_Teleporter b) =>
			{
				b.destination = Dew.GetPositionOnGround(position);
				b.isSpawnedByOthers = true;
			});
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (bool)(UnityEngine.Object)(object)NetworkedManagerBase<GameManager>.instance)
		{
			NetworkedManagerBase<GameManager>.instance.UnlockMidRunSave();
		}
	}

	protected override bool OnUse(Entity entity)
	{
		if (entity.Status.HasStatusEffect<Se_Shrine_Polaris_Teleporter_Teleport>())
		{
			return false;
		}
		entity.CreateStatusEffect<Se_Shrine_Polaris_Teleporter_Teleport>(entity, new CastInfo(entity, destination));
		return true;
	}

	private void MirrorProcessed()
	{
	}
}
