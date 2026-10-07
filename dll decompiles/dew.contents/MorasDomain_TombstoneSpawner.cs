using System.Collections.Generic;
using UnityEngine;

public class MorasDomain_TombstoneSpawner : MonoBehaviour
{
	public GameObject[] templates;

	public RoomMap map;

	public float sqrMinDistance;

	public int maxSpawnCount;

	public int maxTryCount;

	public float preventCircleSize;

	public Vector2 scaleRange;

	public Vector2 rotMin;

	public Vector2 rotMax;

	private List<Vector2> _added;
}
