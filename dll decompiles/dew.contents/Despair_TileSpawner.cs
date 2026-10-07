using UnityEngine;

public class Despair_TileSpawner : MonoBehaviour
{
	public Vector3 boundSize;

	public Terrain terrain;

	public GameObject[] brickTemplates;

	[Tooltip("Will use X component of scale configuration")]
	public bool uniformScale;

	public Vector3 scaleMin = new Vector3(0.8f, 0.8f, 0.8f);

	public Vector3 scaleMax = new Vector3(1.2f, 1.2f, 1.2f);

	public float stepSize = 1f;

	public float minDistance = 0.5f;

	public float randomMagnitude = 0.3f;

	public Vector2 yOffsetRange = new Vector2(0.05f, 0.2f);

	public Vector3 rotMin = new Vector3(0f, 0f, 0f);

	public Vector3 rotMax = new Vector3(5f, 360f, 5f);

	public bool createPath;

	public Transform pathStart;

	public Transform pathEnd;

	public float pathWidth = 2f;

	[Range(0f, 8f)]
	public int requiredSolidNeighbors = 6;
}
