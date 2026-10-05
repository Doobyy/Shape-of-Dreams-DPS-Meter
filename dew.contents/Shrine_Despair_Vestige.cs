using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Shrine_Despair_Vestige : Shrine
{
	[NonSerialized]
	[SaveVar(SaveVarFlags.Default)]
	public Vector3 destination;

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
		SingletonDewNetworkBehaviour<Room>.instance.onRoomClear.AddListener(() =>
		{
			isLocked = false;
		});
		IList<RoomSection> sections = SingletonDewNetworkBehaviour<Room>.instance.sections;
		float num = float.PositiveInfinity;
		RoomSection roomSection = null;
		foreach (RoomSection item in sections)
		{
			float sqrMagnitude = (item.transform.position - position).sqrMagnitude;
			if (!(num < sqrMagnitude))
			{
				num = sqrMagnitude;
				roomSection = item;
			}
		}
		if (!(roomSection == null))
		{
			roomSection.monsters.onClearCombatArea.AddListener(() =>
			{
				isLocked = false;
			});
		}
	}

	protected override bool OnUse(Entity entity)
	{
		if (entity.Status.HasStatusEffect<Se_Shrine_Despair_Teleport>())
		{
			return false;
		}
		entity.CreateStatusEffect<Se_Shrine_Despair_Teleport>(entity, new CastInfo(entity, destination));
		return true;
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

	private void MirrorProcessed()
	{
	}
}
