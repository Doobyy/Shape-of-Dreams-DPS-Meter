using System;
using Mirror;
using UnityEngine;

[RoomComponentStartDependency(typeof(RoomModifiers))]
public class RoomProps : RoomComponent
{
	private const int RandomNodeAnySectionMaxTries = 5;

	public PropSpawnRule globalRule;

	public PropSpawnRule defaultPerSectionRule;

	public override void OnRoomStartServer()
	{
		base.OnRoomStartServer();
		if (globalRule == null && NetworkedManagerBase<ZoneManager>.instance.currentNode.type == WorldNodeType.Combat)
		{
			globalRule = NetworkedManagerBase<ZoneManager>.instance.currentZone.defaultProps;
		}
		if (isRevisit)
		{
			return;
		}
		if (globalRule != null)
		{
			SpawnProps(globalRule, null);
		}
		for (int i = 0; i < room.sections.Count; i++)
		{
			RoomSection roomSection = room.sections[i];
			Section_Props component = roomSection.GetComponent<Section_Props>();
			if (component != null && component.sectionRule != null)
			{
				SpawnProps(component.sectionRule, roomSection);
			}
			else if (defaultPerSectionRule != null)
			{
				SpawnProps(defaultPerSectionRule, roomSection);
			}
		}
	}

	private void SpawnProps(PropSpawnRule rule, RoomSection section)
	{
		if (rule == null)
		{
			return;
		}
		DewRandom roomRandom = room.GetRoomRandom(-16116);
		foreach (PropSpawnRule.PropEntry entry in rule.entries)
		{
			int currentRoomIndex = NetworkedManagerBase<ZoneManager>.instance.currentRoomIndex;
			if (currentRoomIndex < entry.roomIndexRange.x || currentRoomIndex > entry.roomIndexRange.y)
			{
				continue;
			}
			int num = 0;
			float num2 = entry.chance;
			IProp component = entry.prop.asset.GetComponent<IProp>();
			if (component.isRegularReward && room.rewards.isRegularRewardDisabled)
			{
				continue;
			}
			if (component.scaleSpawnRateWithPlayers)
			{
				num2 *= 1f + NetworkedManagerBase<GameManager>.instance.GetMultiplayerDifficultyFactor(reduceWhenDead: false);
			}
			while (num2 > 1f)
			{
				num2--;
				num += roomRandom.Range(entry.count.x, entry.count.y + 1);
			}
			if (roomRandom.Value() < num2)
			{
				num += roomRandom.Range(entry.count.x, entry.count.y + 1);
			}
			for (int i = 0; i < num; i++)
			{
				if (component.isSingleton)
				{
					UnityEngine.Object obj = UnityEngine.Object.FindObjectOfType(component.GetType());
					if (obj != null && !(obj is MonoBehaviour { isActiveAndEnabled: false }) && !(obj is Actor { isActive: false }))
					{
						break;
					}
				}
				Vector3 position = default;
				if (component.customSpawnPosition.HasValue)
				{
					position = component.customSpawnPosition.Value;
				}
				else if ((bool)section)
				{
					section.TryGetGoodNodePosition(out position, roomRandom);
				}
				else
				{
					TryGetGoodNodePosition(out position, roomRandom);
				}
				Quaternion value = ManagerBase<CameraManager>.instance.entityCamAngleRotation;
				if (component.customSpawnRotation.HasValue)
				{
					value = component.customSpawnRotation.Value;
				}
				Actor component3;
				if (entry.prop.asset.TryGetComponent<Entity>(out var component2))
				{
					Dew.SpawnEntity(component2, position, value, null, DewPlayer.environment, NetworkedManagerBase<GameManager>.instance.ambientLevel);
				}
				else if (entry.prop.asset.TryGetComponent<Actor>(out component3))
				{
					Dew.CreateActor(component3, position, value);
				}
				else
				{
					Dew.InstantiateAndSpawn<NetworkIdentity>(entry.prop.asset.GetComponent<NetworkIdentity>(), position, (Quaternion?)value, (Action<NetworkIdentity>)null);
				}
			}
		}
	}

	public bool TryGetGoodNodePosition(out Vector3 position, DewRandom random = null)
	{
		if (random == null)
		{
			random = room.GetRoomRandom(-9191);
		}
		position = default;
		int num = 0;
		while (true)
		{
			num++;
			if (num >= 5)
			{
				break;
			}
			if (Dew.SelectRandomWeightedInList(room.sections, (RoomSection s) => (s.TryGetComponent<Section_Props>(out var component) && component.excludeFromGlobalRule) ? 0f : s.area, random).TryGetGoodNodePosition(out position, random))
			{
				return true;
			}
		}
		return false;
	}

	private void MirrorProcessed()
	{
	}
}
