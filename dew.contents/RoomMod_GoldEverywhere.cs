using System.Collections.Generic;
using UnityEngine;

public class RoomMod_GoldEverywhere : RoomModifierBase
{
	public Vector2Int goldProps;

	public override void OnStartServer()
	{
		base.OnStartServer();
		if (!isNewInstance)
		{
			return;
		}
		SingletonDewNetworkBehaviour<Room>.instance.rewards.DisableRegularRewards();
		int num = Random.Range(goldProps.x, goldProps.y + 1);
		IReadOnlyList<(int, int)> innerPropNodeIndices = SingletonDewNetworkBehaviour<Room>.instance.map.mapData.innerPropNodeIndices;
		for (int i = 0; i < num; i++)
		{
			Vector3 positionOnGround = Dew.GetPositionOnGround(SingletonDewNetworkBehaviour<Room>.instance.map.mapData.cells.GetWorldPos(innerPropNodeIndices[Random.Range(0, innerPropNodeIndices.Count)]).ToXZ() + Vector3.up * 100f, 200f);
			Vector3 positionOnGround2 = Dew.GetPositionOnGround(positionOnGround + Random.insideUnitSphere * 1f);
			positionOnGround2 = Dew.GetValidAgentDestination_LinearSweep(positionOnGround, positionOnGround2);
			SpawnEntity<PropEnt_Stone_Gold>(positionOnGround2, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f), DewPlayer.environment, 1);
		}
		ModifyEntities((Entity e) =>
		{
			if (e is Monster)
			{
				e.CreateStatusEffect<Se_GoldEverywhere>(e, new CastInfo(e));
			}
		}, (Entity e) =>
		{
			if (e.Status.TryGetStatusEffect<Se_GoldEverywhere>(out var effect))
			{
				effect.Destroy();
			}
		});
	}

	private void MirrorProcessed()
	{
	}
}
