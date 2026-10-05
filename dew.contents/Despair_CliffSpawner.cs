using UnityEngine;

public class Despair_CliffSpawner : MonoBehaviour
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
}
