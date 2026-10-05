using System;
using Mirror;
using UnityEngine;
using UnityEngine.Serialization;

public class Shrine_Despair : Shrine, IPlayerPathablePoint
{
	[FormerlySerializedAs("isOpen")]
	public bool isOpenByDefault;

	public bool enableVestige = true;

	public Transform targetPos;

	Vector3 IPlayerPathablePoint.pathablePosition => ((Component)(object)this).transform.position;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		SingletonDewNetworkBehaviour<Room>.instance.onRemoveObstacles += new Action(OnRemoveObstacles);
		if (!isNewInstance)
		{
			if (NetworkedManagerBase<ZoneManager>.instance.isCurrentNodeHunted)
			{
				SingletonDewNetworkBehaviour<Room>.instance.monsters.onWelcomingSpawnStart += (Action)(() =>
				{
					isLocked = true;
				});
				SingletonDewNetworkBehaviour<Room>.instance.monsters.onWelcomingSpawnEnd += (Action)(() =>
				{
					isLocked = false;
				});
			}
			else
			{
				isLocked = false;
			}
			return;
		}
		isLocked = !isOpenByDefault;
		SingletonDewNetworkBehaviour<Room>.instance.onRoomClear.AddListener(() =>
		{
			isLocked = false;
		});
		RoomSection sectionFromWorldPos = SingletonDewNetworkBehaviour<Room>.instance.GetSectionFromWorldPos(position);
		if (sectionFromWorldPos != null)
		{
			sectionFromWorldPos.monsters.onClearCombatArea.AddListener(() =>
			{
				isLocked = false;
			});
		}
		if (enableVestige)
		{
			Dew.CreateActor(Dew.GetPositionOnGround(targetPos.position), null, this, (Shrine_Despair_Vestige b) =>
			{
				b.destination = Dew.GetPositionOnGround(position);
				b.isLocked = true;
			});
		}
	}

	private void OnRemoveObstacles()
	{
		isLocked = false;
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)SingletonDewNetworkBehaviour<Room>.instance != null)
		{
			SingletonDewNetworkBehaviour<Room>.instance.onRemoveObstacles -= new Action(OnRemoveObstacles);
		}
	}

	protected override bool OnUse(Entity entity)
	{
		if (entity.Status.HasStatusEffect<Se_Shrine_Despair_Teleport>())
		{
			return false;
		}
		entity.CreateStatusEffect<Se_Shrine_Despair_Teleport>(entity, new CastInfo(entity, targetPos.position));
		return true;
	}

	private void MirrorProcessed()
	{
	}
}
