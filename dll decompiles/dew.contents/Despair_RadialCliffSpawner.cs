using UnityEngine;

public class Despair_RadialCliffSpawner : MonoBehaviour
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

	[Range(0f, 90f)]
	public float outwardTiltAngle = 15f;

	[Range(0f, 10f)]
	public float rotationRandomness = 5f;
}
