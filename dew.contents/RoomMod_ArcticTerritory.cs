using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class RoomMod_ArcticTerritory : RoomModifierBase
{
	public float spawnDensity;

	public Vector2 zoneRadius;

	private float _initialVolumeAmount;

	private int _maxCalCount = 10;

	private List<AbilityInstance> _spawnedObjects = new List<AbilityInstance>();

	public override void OnStart()
	{
		base.OnStart();
		_initialVolumeAmount = 0.1f;
	}

	public override void OnStartServer()
	{
		base.OnStartServer();
		GameManager.CallOnReady(() =>
		{
			SpawnArcticTerritory();
		});
	}

	private void SpawnArcticTerritory()
	{
		List<(Vector3, float)> list = new List<(Vector3, float)>();
		int num = Mathf.FloorToInt(SingletonDewNetworkBehaviour<Room>.instance.map.mapData.area / spawnDensity);
		Cells2D<MapCellType> cells = SingletonDewNetworkBehaviour<Room>.instance.map.mapData.cells;
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < 50; j++)
			{
				(int, int) indices = (Random.Range(0, cells.dataWidth), Random.Range(0, cells.dataHeight));
				if (cells.Get(indices) != MapCellType.Playable)
				{
					continue;
				}
				Vector3 positionOnGround = Dew.GetPositionOnGround(cells.GetWorldPos(indices).ToXZ());
				float num2 = Random.Range(zoneRadius.x, zoneRadius.y);
				bool flag = false;
				foreach (var item2 in list)
				{
					if (Vector2.Distance(positionOnGround.ToXY(), item2.Item1.ToXY()) < item2.Item2 + num2)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					list.Add((positionOnGround, num2));
					break;
				}
			}
		}
		foreach (var item3 in list)
		{
			(Vector3, float) p = item3;
			Ai_ArcticTerritory item = CreateAbilityInstance(p.Item1, null, default, (Ai_ArcticTerritory b) =>
			{
				b.radius = p.Item2;
			});
			_spawnedObjects.Add(item);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		foreach (AbilityInstance spawnedObject in _spawnedObjects)
		{
			spawnedObject.Destroy();
		}
		_spawnedObjects.Clear();
	}

	private void MirrorProcessed()
	{
	}
}
