using System;
using System.Linq;
using Mirror;
using UnityEngine;

public abstract class SpecialEntityRoomModifier : RoomModifierBase
{
	public Vector2Int spawnedDreamDust;

	public Vector2Int spawnedGold;

	public Vector2Int spawnedStardust;

	public bool dontSpawnIfHunted = true;

	private ActorRef<Entity> _entity;

	protected abstract Type GetEntityType();

	protected virtual Entity SpawnNpc(Vector3 pos)
	{
		Entity byType = DewResources.GetByType<Entity>(GetEntityType(), default(ResourceLoadSettings));
		return SpawnEntity(byType, pos, null, DewPlayer.environment, 1);
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		SingletonDewNetworkBehaviour<Room>.instance.GetFinalSection().TryGetGoodNodePosition(out var pos);
		Type entityType = GetEntityType();
		if (dontSpawnIfHunted && NetworkedManagerBase<ZoneManager>.instance.isCurrentNodeHunted)
		{
			Entity[] array = NetworkedManagerBase<ActorManager>.instance.allEntities.ToArray();
			foreach (Entity entity in array)
			{
				if (entityType.IsInstanceOfType(entity))
				{
					entity.Destroy();
				}
			}
		}
		else
		{
			Entity byType = DewResources.GetByType<Entity>(GetEntityType(), default(ResourceLoadSettings));
			if (isNewInstance || !byType.ShouldBeSavedWithRoom())
			{
				_entity = SpawnNpc(pos);
			}
			else
			{
				Entity[] array = NetworkedManagerBase<ActorManager>.instance.allEntities.ToArray();
				foreach (Entity entity2 in array)
				{
					if (entityType.IsInstanceOfType(entity2))
					{
						_entity = entity2;
					}
				}
				if ((UnityEngine.Object)(object)_entity.Get() == null)
				{
					_entity = SpawnNpc(pos);
				}
			}
		}
		Dew.CallDelayed(() =>
		{
			SingletonDewNetworkBehaviour<Room>.instance.RemoveObstacles();
			SingletonDewNetworkBehaviour<Room>.instance.RemoveCombat(clearRoomOnEnteringLastSection: true);
		});
		if (!isNewInstance)
		{
			return;
		}
		int num = UnityEngine.Random.Range(spawnedDreamDust.x, spawnedDreamDust.y + 1);
		int num2 = UnityEngine.Random.Range(spawnedGold.x, spawnedGold.y + 1);
		int num3 = UnityEngine.Random.Range(spawnedStardust.x, spawnedStardust.y + 1);
		for (int num4 = 0; num4 < num; num4++)
		{
			SingletonDewNetworkBehaviour<Room>.instance.props.TryGetGoodNodePosition(out var pos2);
			SpawnEntity<PropEnt_Stone_DreamDust>(pos2, Quaternion.Euler(0f, UnityEngine.Random.Range(0, 360), 0f), DewPlayer.environment, 1);
		}
		for (int num5 = 0; num5 < num2; num5++)
		{
			SingletonDewNetworkBehaviour<Room>.instance.props.TryGetGoodNodePosition(out var pos3);
			SpawnEntity<PropEnt_Stone_Gold>(pos3, Quaternion.Euler(0f, UnityEngine.Random.Range(0, 360), 0f), DewPlayer.environment, 1);
		}
		for (int num6 = 0; num6 < num3; num6++)
		{
			SingletonDewNetworkBehaviour<Room>.instance.props.TryGetGoodNodePosition(out var pos4);
			CreateActor(pos4, Quaternion.Euler(0f, UnityEngine.Random.Range(0, 360), 0f), (Shrine_Stardust s) =>
			{
				s.amount = 1;
			});
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && !_entity.IsNullInactiveDeadOrKnockedOut())
		{
			_entity.Get().Destroy();
		}
	}

	private void MirrorProcessed()
	{
	}
}
