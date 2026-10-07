using UnityEngine;

public class Despair_CliffSpawnerAdvanced : MonoBehaviour
{
	public Vector3 boundSize;

	public Terrain terrain;

	public GameObject[] templates;

	[Tooltip("Will use X component of scale configuration")]
	public bool uniformScale;

	public Vector3 scaleMin;

	public Vector3 scaleMax;

	public float stepSize;

	public float minDistance;

	public float randomMagnitude;

	public Vector2 yOffsetRange;

	public Vector3 rotMin;

	public Vector3 rotMax;

	[Header("Hole Border Customization")]
	[Tooltip("Controls where objects spawn along hole borders. -1: closer to solid terrain, 0: exactly on border, 1: closer to holes")]
	[Range(-1f, 1f)]
	public float borderPlacementBias;

	[Tooltip("Minimum number of adjacent holes to spawn (0-8)")]
	[Range(0f, 8f)]
	public int minAdjacentHoles = 2;

	[Tooltip("Maximum number of adjacent holes to spawn (0-8)")]
	[Range(0f, 8f)]
	public int maxAdjacentHoles = 7;

	[Tooltip("Controls how steep the terrain-to-hole transition should be to place objects")]
	[Range(0f, 10f)]
	public float heightDifferenceThreshold = 1f;

	[Header("Border Offset Settings")]
	[Tooltip("How many grid steps inward or outward from the border to place objects")]
	[Range(0f, 5f)]
	public int borderOffsetSteps;

	[Tooltip("Direction of offset. True = toward solid terrain, False = toward hole")]
	public bool offsetTowardTerrain = true;

	[Tooltip("Apply random variation to border offset")]
	public bool randomizeOffset;

	[Tooltip("Range of random offset variation")]
	public Vector2Int randomOffsetRange = new Vector2Int(0, 1);
}
