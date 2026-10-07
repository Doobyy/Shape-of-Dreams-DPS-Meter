using UnityEngine;

public class Despair_RadialRockSpawner : MonoBehaviour
{
	public enum PatternType
	{
		Random,
		Spiral,
		Radial,
		CircularRings
	}

	public Vector3 boundSize;

	public GameObject[] templates;

	public int rocksCount;

	[Tooltip("Will use X component of scale configuration")]
	public bool uniformScale;

	public Vector3 scaleMin;

	public Vector3 scaleMax;

	public float collisionCheckRadius;

	[Header("Pattern Settings")]
	public PatternType patternType;

	public float spiralSpacing = 1f;

	public float spiralMinDistance = 3f;

	public bool useHeightVariation = true;

	public float spiralStartHeight;

	public float spiralEndHeight = 10f;

	[Range(0f, 1f)]
	public float heightVariationIntensity = 0.5f;

	public int radialLines = 8;

	public float radialSpacing = 2f;

	public int rings = 3;

	public float ringSpacing = 5f;

	[Range(0f, 1f)]
	public float randomOffset = 0.2f;

	[Header("Rotation Settings")]
	public bool randomRotation = true;

	public Vector3 fixedRotation = Vector3.zero;

	public Vector2 rotationRangeX = new Vector2(0f, 360f);

	public Vector2 rotationRangeY = new Vector2(0f, 360f);

	public Vector2 rotationRangeZ = new Vector2(0f, 360f);

	[Header("Orientation Settings")]
	public bool autoOrientToCenter;

	[Range(0f, 90f)]
	public float orientationAngle = 30f;

	[Range(0f, 45f)]
	public float orientationRandomness = 10f;

	public Vector3 primaryOrientationAxis = Vector3.up;

	public bool addRandomYRotation = true;
}
